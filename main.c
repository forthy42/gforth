/* command line interpretation, image loading etc. for Gforth


  Copyright (C) 1995 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/

#include "config.h"
#include <errno.h>
#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <assert.h>
#include <stdlib.h>
#if HAVE_SYS_MMAN_H
#include <sys/mman.h>
#endif
#include "forth.h"
#include "io.h"
#include "getopt.h"
#include "version.h"

#define PRIM_VERSION 1
/* increment this whenever the primitives change in an incompatible way */

#ifdef MSDOS
jmp_buf throw_jmp_buf;
#  ifndef DEFAULTPATH
#    define DEFAULTPATH "."
#  endif
#endif

#if defined(DIRECT_THREADED) 
#  define CA(n)	(symbols[(n)])
#else
#  define CA(n)	((Cell)(symbols+(n)))
#endif

#define maxaligned(n)	(typeof(n))((((Cell)n)+sizeof(Float)-1)&-sizeof(Float))

static UCell dictsize=0;
static UCell dsize=0;
static UCell rsize=0;
static UCell fsize=0;
static UCell lsize=0;
int offset_image=0;
static int clear_dictionary=0;
static int debug=0;
static size_t pagesize=0;
char *progname;

/* image file format:
 *  "#! binary-path -i\n" (e.g., "#! /usr/local/bin/gforth-0.3.0 -i\n")
 *   padding to a multiple of 8
 *   magic: "Gforth1x" means format 0.2,
 *              where x is even for big endian and odd for little endian
 *              and x & ~1 is the size of the cell in bytes.
 *  padding to max alignment (no padding necessary on current machines)
 *  ImageHeader structure (see below)
 *  data (size in ImageHeader.image_size)
 *  tags ((if relocatable, 1 bit/data cell)
 *
 * tag==1 means that the corresponding word is an address;
 * If the word is >=0, the address is within the image;
 * addresses within the image are given relative to the start of the image.
 * If the word =-1 (CF_NIL), the address is NIL,
 * If the word is <CF_NIL and >CF(DODOES), it's a CFA (:, Create, ...)
 * If the word =CF(DODOES), it's a DOES> CFA
 * If the word =CF(DOESJUMP), it's a DOES JUMP (2 Cells after DOES>,
 *					possibly containing a jump to dodoes)
 * If the word is <CF(DOESJUMP), it's a primitive
 */

typedef struct {
  Address base;		/* base address of image (0 if relocatable) */
  UCell checksum;	/* checksum of ca's to protect against some
			   incompatible	binary/executable combinations
			   (0 if relocatable) */
  UCell image_size;	/* all sizes in bytes */
  UCell dict_size;
  UCell data_stack_size;
  UCell fp_stack_size;
  UCell return_stack_size;
  UCell locals_stack_size;
  Xt *boot_entry;	/* initial ip for booting (in BOOT) */
  Xt *throw_entry;	/* ip after signal (in THROW) */
  Cell unused1;		/* possibly tib stack size */
  Cell unused2;
  Address data_stack_base; /* this and the following fields are initialized by the loader */
  Address fp_stack_base;
  Address return_stack_base;
  Address locals_stack_base;
} ImageHeader;
/* the image-header is created in main.fs */

void relocate(Cell *image, char *bitstring, int size, Label symbols[])
{
  int i=0, j, k, steps=(size/sizeof(Cell))/8;
  char bits;
/*   static char bits[8]={0x80,0x40,0x20,0x10,0x08,0x04,0x02,0x01};*/

/*  printf("relocating %x[%x]\n", image, size); */
   
  for(k=0; k<=steps; k++)
    for(j=0, bits=bitstring[k]; j<8; j++, i++, bits<<=1) {
      /*      fprintf(stderr,"relocate: image[%d]\n", i);*/
      if(bits & 0x80) {
	/* fprintf(stderr,"relocate: image[%d]=%d\n", i, image[i]);*/
	if(image[i]<0)
	  switch(image[i])
	    {
	    case CF_NIL      : image[i]=0; break;
#if !defined(DOUBLY_INDIRECT)
	    case CF(DOCOL)   :
	    case CF(DOVAR)   :
	    case CF(DOCON)   :
	    case CF(DOUSER)  : 
	    case CF(DODEFER) : 
	    case CF(DOFIELD) : MAKE_CF(image+i,symbols[CF(image[i])]); break;
	    case CF(DOESJUMP): MAKE_DOES_HANDLER(image+i); break;
#endif /* !defined(DOUBLY_INDIRECT) */
	    case CF(DODOES)  :
	      MAKE_DOES_CF(image+i,image[i+1]+((Cell)image));
	      break;
	    default          :
/*	      printf("Code field generation image[%x]:=CA(%x)\n",
		     i, CF(image[i])); */
	      image[i]=(Cell)CA(CF(image[i]));
	    }
	else
	  image[i]+=(Cell)image;
      }
    }
}

UCell checksum(Label symbols[])
{
  UCell r=PRIM_VERSION;
  Cell i;

  for (i=DOCOL; i<=DOESJUMP; i++) {
    r ^= (UCell)(symbols[i]);
    r = (r << 5) | (r >> (8*sizeof(Cell)-5));
  }
#ifdef DIRECT_THREADED
  /* we have to consider all the primitives */
  for (; symbols[i]!=(Label)0; i++) {
    r ^= (UCell)(symbols[i]);
    r = (r << 5) | (r >> (8*sizeof(Cell)-5));
  }
#else
  /* in indirect threaded code all primitives are accessed through the
     symbols table, so we just have to put the base address of symbols
     in the checksum */
  r ^= (UCell)symbols;
#endif
  return r;
}

Address my_alloc(Cell size)
{
  static Address next_address=0;
  Address r;

/* the 256MB jump restriction on the MIPS architecture makes the
   combination of direct threading and mmap unsafe. */
#if HAVE_MMAP && (!defined(mips) || defined(INDIRECT_THREADED))
#if defined(MAP_ANON)
  if (debug)
    fprintf(stderr,"try mmap($%lx, $%lx, ..., MAP_ANON, ...); ", (long)next_address, (long)size);
  r=mmap(next_address, size, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE, -1, 0);
#else /* !defined(MAP_ANON) */
  /* Ultrix (at least does not define MAP_FILE and MAP_PRIVATE (both are
     apparently defaults*/
#ifndef MAP_FILE
# define MAP_FILE 0
#endif
#ifndef MAP_PRIVATE
# define MAP_PRIVATE 0
#endif
  static int dev_zero=-1;

  if (dev_zero == -1)
    dev_zero = open("/dev/zero", O_RDONLY);
  if (dev_zero == -1) {
    r = (Address)-1;
    if (debug)
      fprintf(stderr, "open(\"/dev/zero\"...) failed (%s), no mmap; ", 
	      strerror(errno));
  } else {
    if (debug)
      fprintf(stderr,"try mmap($%lx, $%lx, ..., MAP_FILE, dev_zero, ...); ", (long)next_address, (long)size);
    r=mmap(next_address, size, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_FILE|MAP_PRIVATE, dev_zero, 0);
  }
#endif /* !defined(MAP_ANON) */

  if (r != (Address)-1) {
    if (debug)
      fprintf(stderr, "success, address=$%lx\n", (long) r);
    if (pagesize != 0)
      next_address = (Address)(((((Cell)r)+size-1)&-pagesize)+2*pagesize); /* leave one page unmapped */
    return r;
  }
  if (debug)
    fprintf(stderr, "failed: %s\n", strerror(errno));
#endif /* HAVE_MMAP */
  /* use malloc as fallback, leave a little room (64B) for stack underflows */
  if ((r = malloc(size+64))==NULL) {
    perror(progname);
    exit(1);
  }
  r = (Address)((((Cell)r)+(sizeof(Float)-1))&(-sizeof(Float)));
  if (debug)
    fprintf(stderr, "malloc succeeds, address=$%lx\n", (long)r);
  return r;
}

Address loader(FILE *imagefile, char* filename)
/* returns the address of the image proper (after the preamble) */
{
  ImageHeader header;
  Address image;
  Address imp; /* image+preamble */
  Char magic[9];
  Cell preamblesize=0;
  Label *symbols = engine(0,0,0,0,0);
  Cell data_offset = offset_image ? 28*sizeof(Cell) : 0;
  UCell check_sum;
  static char* endianstring[]= { "big","little" };

#ifndef DOUBLY_INDIRECT
  check_sum = checksum(symbols);
#else /* defined(DOUBLY_INDIRECT) */
  check_sum = (UCell)symbols;
#endif /* defined(DOUBLY_INDIRECT) */

  do
    {
      if(fread(magic,sizeof(Char),8,imagefile) < 8) {
	fprintf(stderr,"%s: image %s doesn't seem to be a Gforth (>=0.2) image.\n",
		progname, filename);
	exit(1);
      }
      preamblesize+=8;
    }
  while(memcmp(magic,"Gforth1",7));
  if (debug) {
    magic[8]='\0';
    fprintf(stderr,"Magic found: %s\n", magic);
  }

  if(magic[7] != sizeof(Cell) +
#ifdef WORDS_BIGENDIAN
       '0'
#else
       '1'
#endif
       )
    { fprintf(stderr,"This image is %d bit %s-endian, whereas the machine is %d bit %s-endian.\n", 
	      ((magic[7]-'0')&~1)*8, endianstring[magic[7]&1],
	      sizeof(Cell)*8, endianstring[
#ifdef WORDS_BIGENDIAN
		      0
#else
		      1
#endif
		      ]);
      exit(-2);
    };

  fread((void *)&header,sizeof(ImageHeader),1,imagefile);
  if (dictsize==0)
    dictsize = header.dict_size;
  if (dsize==0)
    dsize=header.data_stack_size;
  if (rsize==0)
    rsize=header.return_stack_size;
  if (fsize==0)
    fsize=header.fp_stack_size;
  if (lsize==0)
    lsize=header.locals_stack_size;
  dictsize=maxaligned(dictsize);
  dsize=maxaligned(dsize);
  rsize=maxaligned(rsize);
  lsize=maxaligned(lsize);
  fsize=maxaligned(fsize);
  
#if HAVE_GETPAGESIZE
  pagesize=getpagesize(); /* Linux/GNU libc offers this */
#elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
  pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
#elif PAGESIZE
  pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
#endif
  if (debug)
    fprintf(stderr,"pagesize=%d\n",pagesize);

  image = my_alloc(preamblesize+dictsize+data_offset)+data_offset;
  rewind(imagefile);  /* fseek(imagefile,0L,SEEK_SET); */
  if (clear_dictionary)
    memset(image,0,dictsize);
  fread(image,1,preamblesize+header.image_size,imagefile);
  imp=image+preamblesize;
  if(header.base==0) {
    Cell reloc_size=((header.image_size-1)/sizeof(Cell))/8+1;
    char reloc_bits[reloc_size];
    fread(reloc_bits,1,reloc_size,imagefile);
    relocate((Cell *)imp,reloc_bits,header.image_size,symbols);
#if 0
    { /* let's see what the relocator did */
      FILE *snapshot=fopen("snapshot.fi","wb");
      fwrite(image,1,imagesize,snapshot);
      fclose(snapshot);
    }
#endif
  }
  else if(header.base!=imp) {
    fprintf(stderr,"%s: Cannot load nonrelocatable image (compiled for address $%lx) at address $%lx\n",
	    progname, (unsigned long)header.base, (unsigned long)imp);
    exit(1);
  }
  if (header.checksum==0)
    ((ImageHeader *)imp)->checksum=check_sum;
  else if (header.checksum != check_sum) {
    fprintf(stderr,"%s: Checksum of image ($%lx) does not match the executable ($%lx)\n",
	    progname, (unsigned long)(header.checksum),(unsigned long)check_sum);
    exit(1);
  }
  fclose(imagefile);

  ((ImageHeader *)imp)->dict_size=dictsize;
  ((ImageHeader *)imp)->data_stack_size=dsize;
  ((ImageHeader *)imp)->fp_stack_size=fsize;
  ((ImageHeader *)imp)->return_stack_size=rsize;
  ((ImageHeader *)imp)->locals_stack_size=lsize;

  ((ImageHeader *)imp)->data_stack_base=my_alloc(dsize);
  ((ImageHeader *)imp)->fp_stack_base=my_alloc(fsize);
  ((ImageHeader *)imp)->return_stack_base=my_alloc(rsize);
  ((ImageHeader *)imp)->locals_stack_base=my_alloc(lsize);

  CACHE_FLUSH(imp, header.image_size);

  return imp;
}

int go_forth(Address image, int stack, Cell *entries)
{
  Cell *sp=(Cell*)(((ImageHeader *)image)->data_stack_base + dsize);
  Float *fp=(Float *)(((ImageHeader *)image)->fp_stack_base + fsize);
  Cell *rp=(Cell *)(((ImageHeader *)image)->return_stack_base + rsize);
  Address lp=((ImageHeader *)image)->locals_stack_base + lsize;
  Xt *ip=(Xt *)(((ImageHeader *)image)->boot_entry);
  int throw_code;

  /* ensure that the cached elements (if any) are accessible */
  IF_TOS(sp--);
  IF_FTOS(fp--);
  
  for(;stack>0;stack--)
    *--sp=entries[stack-1];

#if !defined(MSDOS) && !defined(_WIN32) && !defined(__EMX__)
  get_winsize();
#endif
   
  install_signal_handlers(); /* right place? */
  
  if ((throw_code=setjmp(throw_jmp_buf))) {
    static Cell signal_data_stack[8];
    static Cell signal_return_stack[8];
    static Float signal_fp_stack[1];
    
    signal_data_stack[7]=throw_code;
    
    return((int)engine(((ImageHeader *)image)->throw_entry,signal_data_stack+7,
		       signal_return_stack+8,signal_fp_stack,0));
  }

  return((int)engine(ip,sp,rp,fp,lp));
}

UCell convsize(char *s, UCell elemsize)
/* converts s of the format [0-9]+[bekM]? (e.g. 25k) into the number
   of bytes.  the letter at the end indicates the unit, where e stands
   for the element size. default is e */
{
  char *endp;
  UCell n,m;

  m = elemsize;
  n = strtoul(s,&endp,0);
  if (endp!=NULL) {
    if (strcmp(endp,"b")==0)
      m=1;
    else if (strcmp(endp,"k")==0)
      m=1024;
    else if (strcmp(endp,"M")==0)
      m=1024*1024;
    else if (strcmp(endp,"e")!=0 && strcmp(endp,"")!=0) {
      fprintf(stderr,"%s: cannot grok size specification %s: invalid unit \"%s\"\n", progname, s, endp);
      exit(1);
    }
  }
  return n*m;
}

int main(int argc, char **argv, char **env)
{
  char *path, *path1;
  char *imagename="gforth.fi";
  FILE *image_file;
  int c, retvalue;
	  
#if defined(i386) && defined(ALIGNMENT_CHECK) && !defined(DIRECT_THREADED)
  /* turn on alignment checks on the 486.
   * on the 386 this should have no effect. */
  __asm__("pushfl; popl %eax; orl $0x40000, %eax; pushl %eax; popfl;");
  /* this is unusable with Linux' libc.4.6.27, because this library is
     not alignment-clean; we would have to replace some library
     functions (e.g., memcpy) to make it work */
#endif

  progname = argv[0];
  if ((path1=getenv("GFORTHPATH"))==NULL)
    path1 = DEFAULTPATH;
  
  opterr=0;
  while (1) {
    int option_index=0;
    static struct option opts[] = {
      {"image-file", required_argument, NULL, 'i'},
      {"dictionary-size", required_argument, NULL, 'm'},
      {"data-stack-size", required_argument, NULL, 'd'},
      {"return-stack-size", required_argument, NULL, 'r'},
      {"fp-stack-size", required_argument, NULL, 'f'},
      {"locals-stack-size", required_argument, NULL, 'l'},
      {"path", required_argument, NULL, 'p'},
      {"version", no_argument, NULL, 'v'},
      {"help", no_argument, NULL, 'h'},
      /* put something != 0 into offset_image */
      {"offset-image", no_argument, &offset_image, 1},
      {"no-offset-im", no_argument, &offset_image, 0},
      {"clear-dictionary", no_argument, &clear_dictionary, 1},
      {"debug", no_argument, &debug, 1},
      {0,0,0,0}
      /* no-init-file, no-rc? */
    };
    
    c = getopt_long(argc, argv, "+i:m:d:r:f:l:p:vh", opts, &option_index);
    
    if (c==EOF)
      break;
    if (c=='?') {
      optind--;
      break;
    }
    switch (c) {
    case 'i': imagename = optarg; break;
    case 'm': dictsize = convsize(optarg,sizeof(Cell)); break;
    case 'd': dsize = convsize(optarg,sizeof(Cell)); break;
    case 'r': rsize = convsize(optarg,sizeof(Cell)); break;
    case 'f': fsize = convsize(optarg,sizeof(Float)); break;
    case 'l': lsize = convsize(optarg,sizeof(Cell)); break;
    case 'p': path1 = optarg; break;
    case 'v': fprintf(stderr, "gforth %s\n", gforth_version); exit(0);
    case 'h': 
      fprintf(stderr, "Usage: %s [engine options] [image arguments]\n\
Engine Options:\n\
 -c, --clear-dictionary		    Initialize the dictionary with 0 bytes\n\
 -d SIZE, --data-stack-size=SIZE    Specify data stack size\n\
 --debug			    Print debugging information during startup\n\
 -f SIZE, --fp-stack-size=SIZE	    Specify floating point stack size\n\
 -h, --help			    Print this message and exit\n\
 -i FILE, --image-file=FILE	    Use image FILE instead of `gforth.fi'\n\
 -l SIZE, --locals-stack-size=SIZE  Specify locals stack size\n\
 -m SIZE, --dictionary-size=SIZE    Specify Forth dictionary size\n\
 --offset-image			    Load image at a different position\n\
 -p PATH, --path=PATH		    Search path for finding image and sources\n\
 -r SIZE, --return-stack-size=SIZE  Specify return stack size\n\
 -v, --version			    Print version and exit\n\
SIZE arguments consist of an integer followed by a unit. The unit can be\n\
  `b' (bytes), `e' (elements), `k' (kilobytes), or `M' (Megabytes).\n\
\n\
Arguments of default image `gforth.fi':\n\
 FILE				    load FILE (with `require')\n\
 -e STRING, --evaluate STRING       interpret STRING (with `EVALUATE')\n",
	      argv[0]); exit(0);
    }
  }
  path=path1;
  
  if(strchr(imagename, '/')==NULL)
    {
      do {
	char *pend=strchr(path, PATHSEP);
	if (pend==NULL)
	  pend=path+strlen(path);
	if (strlen(path)==0) {
	  fprintf(stderr,"%s: cannot open image file %s in path %s for reading\n",
		  progname, imagename, path1);
	  exit(1);
	}
	{
	  int dirlen=pend-path;
	  char fullfilename[dirlen+strlen(imagename)+2];
	  memcpy(fullfilename, path, dirlen);
	  if (fullfilename[dirlen-1]!='/')
	    fullfilename[dirlen++]='/';
	  strcpy(fullfilename+dirlen,imagename);
	  image_file=fopen(fullfilename,"rb");
	  if (image_file!=NULL && debug)
	    fprintf(stderr, "Opened image file: %s\n", fullfilename);
	}
	path=pend+(*pend==PATHSEP);
      } while (image_file==NULL);
    }
  else
    {
      image_file=fopen(imagename,"rb");
    }

  {
    char path2[strlen(path1)+1];
    char *p1, *p2;
    Cell environ[]= {
      (Cell)argc-(optind-1),
      (Cell)(argv+(optind-1)),
      (Cell)strlen(path1),
      (Cell)path2};
    argv[optind-1] = progname;
    /*
       for (i=0; i<environ[0]; i++)
       printf("%s\n", ((char **)(environ[1]))[i]);
       */
    /* make path OS-independent by replacing path separators with NUL */
    for (p1=path1, p2=path2; *p1!='\0'; p1++, p2++)
      if (*p1==PATHSEP)
	*p2 = '\0';
      else
	*p2 = *p1;
    *p2='\0';
    retvalue=go_forth(loader(image_file, imagename),4,environ);
    deprep_terminal();
    exit(retvalue);
  }
}
