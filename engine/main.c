/* command line interpretation, image loading etc. for Gforth


  Copyright (C) 1995,1996,1997,1998,2000 Free Software Foundation, Inc.

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
  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.
*/

#include "config.h"
#include <errno.h>
#include <ctype.h>
#include <stdio.h>
#include <unistd.h>
#include <string.h>
#include <math.h>
#include <sys/types.h>
#ifndef STANDALONE
#include <sys/stat.h>
#endif
#include <fcntl.h>
#include <assert.h>
#include <stdlib.h>
#ifndef STANDALONE
#if HAVE_SYS_MMAN_H
#include <sys/mman.h>
#endif
#endif
#include "forth.h"
#include "io.h"
#include "getopt.h"
#ifdef STANDALONE
#include <systypes.h>
#endif

#define PRIM_VERSION 1
/* increment this whenever the primitives change in an incompatible way */

#ifndef DEFAULTPATH
#  define DEFAULTPATH "."
#endif

#ifdef MSDOS
jmp_buf throw_jmp_buf;
#endif

#if defined(DOUBLY_INDIRECT)
#  define CFA(n)	({Cell _n = (n); ((Cell)(((_n & 0x4000) ? symbols : xts)+(_n&~0x4000UL)));})
#else
#  define CFA(n)	((Cell)(symbols+((n)&~0x4000UL)))
#endif

#define maxaligned(n)	(typeof(n))((((Cell)n)+sizeof(Float)-1)&-sizeof(Float))

static UCell dictsize=0;
static UCell dsize=0;
static UCell rsize=0;
static UCell fsize=0;
static UCell lsize=0;
int offset_image=0;
int die_on_signal=0;
#ifndef INCLUDE_IMAGE
static int clear_dictionary=0;
UCell pagesize=1;
char *progname;
#else
char *progname = "gforth";
int optind = 1;
#endif

Address code_area=0;
Address code_here=0; /* does for code-area what HERE does for the dictionary */

static int no_super=0;   /* true if compile_prim should not fuse prims */
static int no_dynamic=0; /* true if compile_prim should not generate code */

#ifdef HAS_DEBUG
static int debug=0;
#else
# define debug 0
# define perror(x...)
# define fprintf(x...)
#endif

ImageHeader *gforth_header;
Label *vm_prims;
#ifdef DOUBLY_INDIRECT
Label *xts; /* same content as vm_prims, but should only be used for xts */
#endif

#ifdef MEMCMP_AS_SUBROUTINE
int gforth_memcmp(const char * s1, const char * s2, size_t n)
{
  return memcmp(s1, s2, n);
}
#endif

/* image file format:
 *  "#! binary-path -i\n" (e.g., "#! /usr/local/bin/gforth-0.4.0 -i\n")
 *   padding to a multiple of 8
 *   magic: "Gforth2x" means format 0.4,
 *              where x is a byte with
 *              bit 7:   reserved = 0
 *              bit 6:5: address unit size 2^n octets
 *              bit 4:3: character size 2^n octets
 *              bit 2:1: cell size 2^n octets
 *              bit 0:   endian, big=0, little=1.
 *  The magic are always 8 octets, no matter what the native AU/character size is
 *  padding to max alignment (no padding necessary on current machines)
 *  ImageHeader structure (see forth.h)
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
 * If the word is <CF(DOESJUMP) and bit 14 is set, it's the xt of a primitive
 * If the word is <CF(DOESJUMP) and bit 14 is clear, 
 *                                        it's the threaded code of a primitive
 */

void relocate(Cell *image, const char *bitstring, 
              int size, int base, Label symbols[])
{
  int i=0, j, k, steps=(size/sizeof(Cell))/RELINFOBITS;
  Cell token;
  char bits;
  Cell max_symbols;
  /* 
   * A virtial start address that's the real start address minus 
   * the one in the image 
   */
  Cell *start = (Cell * ) (((void *) image) - ((void *) base));

  
/* printf("relocating to %x[%x] start=%x base=%x\n", image, size, start, base); */
  
  for (max_symbols=DOESJUMP+1; symbols[max_symbols]!=0; max_symbols++)
    ;
  max_symbols--;
  size/=sizeof(Cell);

  for(k=0; k<=steps; k++) {
    for(j=0, bits=bitstring[k]; j<RELINFOBITS; j++, i++, bits<<=1) {
      /*      fprintf(stderr,"relocate: image[%d]\n", i);*/
      if((i < size) && (bits & (1U << (RELINFOBITS-1)))) {
	/* fprintf(stderr,"relocate: image[%d]=%d of %d\n", i, image[i], size/sizeof(Cell)); */
        token=image[i];
	if(token<0)
	  switch(token|0x4000)
	    {
	    case CF_NIL      : image[i]=0; break;
#if !defined(DOUBLY_INDIRECT)
	    case CF(DOCOL)   :
	    case CF(DOVAR)   :
	    case CF(DOCON)   :
	    case CF(DOUSER)  : 
	    case CF(DODEFER) : 
	    case CF(DOFIELD) : MAKE_CF(image+i,symbols[CF(token)]); break;
	    case CF(DOESJUMP): MAKE_DOES_HANDLER(image+i); break;
#endif /* !defined(DOUBLY_INDIRECT) */
	    case CF(DODOES)  :
	      MAKE_DOES_CF(image+i,(Xt *)(image[i+1]+((Cell)start)));
	      break;
	    default          :
/*	      printf("Code field generation image[%x]:=CFA(%x)\n",
		     i, CF(image[i])); */
	      if (CF((token | 0x4000))<max_symbols) {
		image[i]=(Cell)CFA(CF(token));
#ifdef DIRECT_THREADED
		if ((token & 0x4000) == 0) /* threade code, no CFA */
		  image[i] = (Cell)compile_prim((Label)image[i]);
#endif
	      } else
		fprintf(stderr,"Primitive %d used in this image at $%lx is not implemented by this\n engine (%s); executing this code will crash.\n",CF(token),(long)&image[i],VERSION);
	    }
	else {
          // if base is > 0: 0 is a null reference so don't adjust
          if (token>=base) {
            image[i]+=(Cell)start;
          }
        }
      }
    }
  }
  ((ImageHeader*)(image))->base = (Address) image;
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

Address verbose_malloc(Cell size)
{
  Address r;
  /* leave a little room (64B) for stack underflows */
  if ((r = malloc(size+64))==NULL) {
    perror(progname);
    exit(1);
  }
  r = (Address)((((Cell)r)+(sizeof(Float)-1))&(-sizeof(Float)));
  if (debug)
    fprintf(stderr, "malloc succeeds, address=$%lx\n", (long)r);
  return r;
}

static Address next_address=0;
void after_alloc(Address r, Cell size)
{
  if (r != (Address)-1) {
    if (debug)
      fprintf(stderr, "success, address=$%lx\n", (long) r);
    if (pagesize != 1)
      next_address = (Address)(((((Cell)r)+size-1)&-pagesize)+2*pagesize); /* leave one page unmapped */
  } else {
    if (debug)
      fprintf(stderr, "failed: %s\n", strerror(errno));
  }
}

#ifndef MAP_FAILED
#define MAP_FAILED ((Address) -1)
#endif
#ifndef MAP_FILE
# define MAP_FILE 0
#endif
#ifndef MAP_PRIVATE
# define MAP_PRIVATE 0
#endif

#if defined(HAVE_MMAP)
static Address alloc_mmap(Cell size)
{
  Address r;

#if defined(MAP_ANON)
  if (debug)
    fprintf(stderr,"try mmap($%lx, $%lx, ..., MAP_ANON, ...); ", (long)next_address, (long)size);
  r = mmap(next_address, size, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE, -1, 0);
#else /* !defined(MAP_ANON) */
  /* Ultrix (at least) does not define MAP_FILE and MAP_PRIVATE (both are
     apparently defaults) */
  static int dev_zero=-1;

  if (dev_zero == -1)
    dev_zero = open("/dev/zero", O_RDONLY);
  if (dev_zero == -1) {
    r = MAP_FAILED;
    if (debug)
      fprintf(stderr, "open(\"/dev/zero\"...) failed (%s), no mmap; ", 
	      strerror(errno));
  } else {
    if (debug)
      fprintf(stderr,"try mmap($%lx, $%lx, ..., MAP_FILE, dev_zero, ...); ", (long)next_address, (long)size);
    r=mmap(next_address, size, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_FILE|MAP_PRIVATE, dev_zero, 0);
  }
#endif /* !defined(MAP_ANON) */
  after_alloc(r, size);
  return r;  
}
#endif

Address my_alloc(Cell size)
{
#if HAVE_MMAP
  Address r;

  r=alloc_mmap(size);
  if (r!=MAP_FAILED)
    return r;
#endif /* HAVE_MMAP */
  /* use malloc as fallback */
  return verbose_malloc(size);
}

Address dict_alloc_read(FILE *file, Cell imagesize, Cell dictsize, Cell offset)
{
  Address image = MAP_FAILED;

#if defined(HAVE_MMAP)
  if (offset==0) {
    image=alloc_mmap(dictsize);
    if (debug)
      fprintf(stderr,"try mmap($%lx, $%lx, ..., MAP_FIXED|MAP_FILE, imagefile, 0); ", (long)image, (long)imagesize);
    image = mmap(image, imagesize, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_FIXED|MAP_FILE|MAP_PRIVATE, fileno(file), 0);
    after_alloc(image,dictsize);
  }
#endif /* defined(HAVE_MMAP) */
  if (image == MAP_FAILED) {
    image = my_alloc(dictsize+offset)+offset;
    rewind(file);  /* fseek(imagefile,0L,SEEK_SET); */
    fread(image, 1, imagesize, file);
  }
  return image;
}

void set_stack_sizes(ImageHeader * header)
{
  if (dictsize==0)
    dictsize = header->dict_size;
  if (dsize==0)
    dsize = header->data_stack_size;
  if (rsize==0)
    rsize = header->return_stack_size;
  if (fsize==0)
    fsize = header->fp_stack_size;
  if (lsize==0)
    lsize = header->locals_stack_size;
  dictsize=maxaligned(dictsize);
  dsize=maxaligned(dsize);
  rsize=maxaligned(rsize);
  lsize=maxaligned(lsize);
  fsize=maxaligned(fsize);
}

void alloc_stacks(ImageHeader * header)
{
  header->dict_size=dictsize;
  header->data_stack_size=dsize;
  header->fp_stack_size=fsize;
  header->return_stack_size=rsize;
  header->locals_stack_size=lsize;

  header->data_stack_base=my_alloc(dsize);
  header->fp_stack_base=my_alloc(fsize);
  header->return_stack_base=my_alloc(rsize);
  header->locals_stack_base=my_alloc(lsize);
  code_here = code_area = my_alloc(dictsize);
}

#warning You can ignore the warnings about clobbered variables in go_forth
int go_forth(Address image, int stack, Cell *entries)
{
  volatile ImageHeader *image_header = (ImageHeader *)image;
  Cell *sp0=(Cell*)(image_header->data_stack_base + dsize);
  Cell *rp0=(Cell *)(image_header->return_stack_base + rsize);
  Float *fp0=(Float *)(image_header->fp_stack_base + fsize);
#ifdef GFORTH_DEBUGGING
  volatile Cell *orig_rp0=rp0;
#endif
  Address lp0=image_header->locals_stack_base + lsize;
  Xt *ip0=(Xt *)(image_header->boot_entry);
#ifdef SYSSIGNALS
  int throw_code;
#endif

  /* ensure that the cached elements (if any) are accessible */
  IF_spTOS(sp0--);
  IF_fpTOS(fp0--);
  
  for(;stack>0;stack--)
    *--sp0=entries[stack-1];

#ifdef SYSSIGNALS
  get_winsize();
   
  install_signal_handlers(); /* right place? */
  
  if ((throw_code=setjmp(throw_jmp_buf))) {
    static Cell signal_data_stack[8];
    static Cell signal_return_stack[8];
    static Float signal_fp_stack[1];

    signal_data_stack[7]=throw_code;

#ifdef GFORTH_DEBUGGING
    /* fprintf(stderr,"\nrp=%ld\n",(long)rp); */
    if (rp <= orig_rp0 && rp > (Cell *)(image_header->return_stack_base+5)) {
      /* no rstack overflow or underflow */
      rp0 = rp;
      *--rp0 = (Cell)ip;
    }
    else /* I love non-syntactic ifdefs :-) */
#endif
    rp0 = signal_return_stack+8;
    /* fprintf(stderr, "rp=$%x\n",rp0);*/
    
    return((int)(Cell)engine(image_header->throw_entry, signal_data_stack+7,
		       rp0, signal_fp_stack, 0));
  }
#endif

  return((int)(Cell)engine(ip0,sp0,rp0,fp0,lp0));
}


#ifndef INCLUDE_IMAGE
void print_sizes(Cell sizebyte)
     /* print size information */
{
  static char* endianstring[]= { "   big","little" };
  
  fprintf(stderr,"%s endian, cell=%d bytes, char=%d bytes, au=%d bytes\n",
	  endianstring[sizebyte & 1],
	  1 << ((sizebyte >> 1) & 3),
	  1 << ((sizebyte >> 3) & 3),
	  1 << ((sizebyte >> 5) & 3));
}

typedef struct {
  Label start;
  Cell length; /* excluding the jump */
  char super_end; /* true if primitive ends superinstruction, i.e.,
                     unconditional branch, execute, etc. */
} PrimInfo;

PrimInfo *priminfos;
Cell npriminfos=0;

void check_prims(Label symbols1[])
{
  int i;
  Label *symbols2;
  static char superend[]={
#include "prim_superend.i"
  };

  for (i=DOESJUMP+1; symbols1[i+1]!=0; i++)
    ;
  npriminfos = i;

#if defined(IS_NEXT_JUMP) && !defined(DOUBLY_INDIRECT)
  symbols2=engine2(0,0,0,0,0);
  priminfos = calloc(i,sizeof(PrimInfo));
  for (i=DOESJUMP+1; symbols1[i+1]!=0; i++) {
    int prim_len=symbols1[i+1]-symbols1[i];
    PrimInfo *pi=&priminfos[i];
    int j;
    pi->super_end = superend[i-DOESJUMP-1]|no_super;
    for (j=prim_len-IND_JUMP_LENGTH; ; j--) {
      if (IS_NEXT_JUMP(symbols1[i]+j)) {
	prim_len = j;
	if (pi->super_end)
	  prim_len += IND_JUMP_LENGTH; /* include the jump */
	break;
      }
      if (j==0) { /* NEXT jump not found, e.g., execute */
	if (!pi->super_end && debug)
	  fprintf(stderr, "NEXT jump not found for primitive %d, making it super_end\n", i);
        pi->super_end = 1;
	break;
      }
    }
    pi->length = prim_len;
    /* fprintf(stderr,"checking primitive %d: memcmp(%p, %p, %d)\n",
       i, symbols1[i], symbols2[i], prim_len);*/
    if (no_dynamic||memcmp(symbols1[i],symbols2[i],prim_len)!=0) {
      if (debug)
	fprintf(stderr,"Primitive %d not relocatable: memcmp(%p, %p, %d)\n",
		i, symbols1[i], symbols2[i], prim_len);
    } else {
      pi->start = symbols1[i];
      if (debug)
	fprintf(stderr,"Primitive %d relocatable: start %p, length %ld, super_end %d\n",
		i, pi->start, pi->length, pi->super_end);
    }      
  }  
#endif
}

Label compile_prim(Label prim)
{
#ifdef DOUBLY_INDIRECT
  if (prim<((Label)(xts+DOESJUMP)) || prim>((Label)(xts+npriminfos))) {
    fprintf(stderr,"compile_prim encountered xt %p\n", prim);
    return prim;
  } else
    return prim-((Label)xts)+((Label)vm_prims);
#else /* !defined(DOUBLY_INDIRECT) */
#if defined(IND_JUMP_LENGTH) && !defined(VM_PROFILING)
  unsigned i;
  Address old_code_here=code_here;
  static Address last_jump=0;

  i = ((Xt)prim)-vm_prims;
  prim = *(Xt)prim;
  if (i>=npriminfos || priminfos[i].start == 0) { /* not a relocatable prim */
    if (last_jump) { /* make sure the last sequence is complete */
      memcpy(code_here, last_jump, IND_JUMP_LENGTH);
      code_here += IND_JUMP_LENGTH;
      last_jump = 0;
    }
    return prim;
  }
  assert(priminfos[i].start = prim); 
#ifdef ALIGN_CODE
  ALIGN_CODE;
#endif
  memcpy(code_here, (Address)prim, priminfos[i].length);
  code_here += priminfos[i].length;
  last_jump = (priminfos[i].super_end) ? 0 : (prim+priminfos[i].length);
  return (Label)old_code_here;
#else
  prim = *(Xt)prim;
  return prim;
#endif
#endif /* !defined(DOUBLY_INDIRECT) */
}

#ifdef PRINT_SUPER_LENGTHS
Cell prim_length(Cell prim)
{
  return priminfos[prim+DOESJUMP+1].length;
}
#endif

Address loader(FILE *imagefile, char* filename)
/* returns the address of the image proper (after the preamble) */
{
  ImageHeader header;
  Address image;
  Address imp; /* image+preamble */
  Char magic[8];
  char magic7; /* size byte of magic number */
  Cell preamblesize=0;
  Cell data_offset = offset_image ? 56*sizeof(Cell) : 0;
  UCell check_sum;
  Cell ausize = ((RELINFOBITS ==  8) ? 0 :
		 (RELINFOBITS == 16) ? 1 :
		 (RELINFOBITS == 32) ? 2 : 3);
  Cell charsize = ((sizeof(Char) == 1) ? 0 :
		   (sizeof(Char) == 2) ? 1 :
		   (sizeof(Char) == 4) ? 2 : 3) + ausize;
  Cell cellsize = ((sizeof(Cell) == 1) ? 0 :
		   (sizeof(Cell) == 2) ? 1 :
		   (sizeof(Cell) == 4) ? 2 : 3) + ausize;
  Cell sizebyte = (ausize << 5) + (charsize << 3) + (cellsize << 1) +
#ifdef WORDS_BIGENDIAN
       0
#else
       1
#endif
    ;

  vm_prims = engine(0,0,0,0,0);
  check_prims(vm_prims);
#ifndef DOUBLY_INDIRECT
#ifdef PRINT_SUPER_LENGTHS
  print_super_lengths();
#endif
  check_sum = checksum(vm_prims);
#else /* defined(DOUBLY_INDIRECT) */
  check_sum = (UCell)vm_prims;
#endif /* defined(DOUBLY_INDIRECT) */
  
  do {
    if(fread(magic,sizeof(Char),8,imagefile) < 8) {
      fprintf(stderr,"%s: image %s doesn't seem to be a Gforth (>=0.4) image.\n",
	      progname, filename);
      exit(1);
    }
    preamblesize+=8;
  } while(memcmp(magic,"Gforth2",7));
  magic7 = magic[7];
  if (debug) {
    magic[7]='\0';
    fprintf(stderr,"Magic found: %s ", magic);
    print_sizes(magic7);
  }

  if (magic7 != sizebyte)
    {
      fprintf(stderr,"This image is:         ");
      print_sizes(magic7);
      fprintf(stderr,"whereas the machine is ");
      print_sizes(sizebyte);
      exit(-2);
    };

  fread((void *)&header,sizeof(ImageHeader),1,imagefile);

  set_stack_sizes(&header);
  
#if HAVE_GETPAGESIZE
  pagesize=getpagesize(); /* Linux/GNU libc offers this */
#elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
  pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
#elif PAGESIZE
  pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
#endif
  if (debug)
    fprintf(stderr,"pagesize=%ld\n",(unsigned long) pagesize);

  image = dict_alloc_read(imagefile, preamblesize+header.image_size,
			  preamblesize+dictsize, data_offset);
  imp=image+preamblesize;
  alloc_stacks((ImageHeader *)imp);
  if (clear_dictionary)
    memset(imp+header.image_size, 0, dictsize-header.image_size);
  if(header.base==0 || header.base  == 0x100) {
    Cell reloc_size=((header.image_size-1)/sizeof(Cell))/8+1;
    char reloc_bits[reloc_size];
    fseek(imagefile, preamblesize+header.image_size, SEEK_SET);
    fread(reloc_bits, 1, reloc_size, imagefile);
    relocate((Cell *)imp, reloc_bits, header.image_size, header.base, vm_prims);
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
#ifdef DOUBLY_INDIRECT
  ((ImageHeader *)imp)->xt_base = xts;
#endif
  fclose(imagefile);

  /* unnecessary, except maybe for CODE words */
  /* FLUSH_ICACHE(imp, header.image_size);*/

  return imp;
}

/* index of last '/' or '\' in file, 0 if there is none. !! Hmm, could
   be implemented with strrchr and the separator should be
   OS-dependent */
int onlypath(char *file)
{
  int i;
  i=strlen(file);
  while (i) {
    if (file[i]=='\\' || file[i]=='/') break;
    i--;
  }
  return i;
}

FILE *openimage(char *fullfilename)
{
  FILE *image_file;
  char * expfilename = tilde_cstr(fullfilename, strlen(fullfilename), 1);

  image_file=fopen(expfilename,"rb");
  if (image_file!=NULL && debug)
    fprintf(stderr, "Opened image file: %s\n", expfilename);
  return image_file;
}

/* try to open image file concat(path[0:len],imagename) */
FILE *checkimage(char *path, int len, char *imagename)
{
  int dirlen=len;
  char fullfilename[dirlen+strlen(imagename)+2];

  memcpy(fullfilename, path, dirlen);
  if (fullfilename[dirlen-1]!='/')
    fullfilename[dirlen++]='/';
  strcpy(fullfilename+dirlen,imagename);
  return openimage(fullfilename);
}

FILE * open_image_file(char * imagename, char * path)
{
  FILE * image_file=NULL;
  char *origpath=path;
  
  if(strchr(imagename, '/')==NULL) {
    /* first check the directory where the exe file is in !! 01may97jaw */
    if (onlypath(progname))
      image_file=checkimage(progname, onlypath(progname), imagename);
    if (!image_file)
      do {
	char *pend=strchr(path, PATHSEP);
	if (pend==NULL)
	  pend=path+strlen(path);
	if (strlen(path)==0) break;
	image_file=checkimage(path, pend-path, imagename);
	path=pend+(*pend==PATHSEP);
      } while (image_file==NULL);
  } else {
    image_file=openimage(imagename);
  }

  if (!image_file) {
    fprintf(stderr,"%s: cannot open image file %s in path %s for reading\n",
	    progname, imagename, origpath);
    exit(1);
  }

  return image_file;
}
#endif

#ifdef HAS_OS
UCell convsize(char *s, UCell elemsize)
/* converts s of the format [0-9]+[bekMGT]? (e.g. 25k) into the number
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
    else if (strcmp(endp,"G")==0)
      m=1024*1024*1024;
    else if (strcmp(endp,"T")==0) {
#if (SIZEOF_CHAR_P > 4)
      m=1024L*1024*1024*1024;
#else
      fprintf(stderr,"%s: size specification \"%s\" too large for this machine\n", progname, endp);
      exit(1);
#endif
    } else if (strcmp(endp,"e")!=0 && strcmp(endp,"")!=0) {
      fprintf(stderr,"%s: cannot grok size specification %s: invalid unit \"%s\"\n", progname, s, endp);
      exit(1);
    }
  }
  return n*m;
}

void gforth_args(int argc, char ** argv, char ** path, char ** imagename)
{
  int c;

  opterr=0;
  while (1) {
    int option_index=0;
    static struct option opts[] = {
      {"appl-image", required_argument, NULL, 'a'},
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
      {"die-on-signal", no_argument, &die_on_signal, 1},
      {"debug", no_argument, &debug, 1},
      {"no-super", no_argument, &no_super, 1},
      {"no-dynamic", no_argument, &no_dynamic, 1},
      {0,0,0,0}
      /* no-init-file, no-rc? */
    };
    
    c = getopt_long(argc, argv, "+i:m:d:r:f:l:p:vhoncsx", opts, &option_index);
    
    switch (c) {
    case EOF: return;
    case '?': optind--; return;
    case 'a': *imagename = optarg; return;
    case 'i': *imagename = optarg; break;
    case 'm': dictsize = convsize(optarg,sizeof(Cell)); break;
    case 'd': dsize = convsize(optarg,sizeof(Cell)); break;
    case 'r': rsize = convsize(optarg,sizeof(Cell)); break;
    case 'f': fsize = convsize(optarg,sizeof(Float)); break;
    case 'l': lsize = convsize(optarg,sizeof(Cell)); break;
    case 'p': *path = optarg; break;
    case 'o': offset_image = 1; break;
    case 'n': offset_image = 0; break;
    case 'c': clear_dictionary = 1; break;
    case 's': die_on_signal = 1; break;
    case 'x': debug = 1; break;
    case 'v': fprintf(stderr, "gforth %s\n", VERSION); exit(0);
    case 'h': 
      fprintf(stderr, "Usage: %s [engine options] ['--'] [image arguments]\n\
Engine Options:\n\
  --appl-image FILE		    equivalent to '--image-file=FILE --'\n\
  --clear-dictionary		    Initialize the dictionary with 0 bytes\n\
  -d SIZE, --data-stack-size=SIZE   Specify data stack size\n\
  --debug			    Print debugging information during startup\n\
  --die-on-signal		    exit instead of CATCHing some signals\n\
  -f SIZE, --fp-stack-size=SIZE	    Specify floating point stack size\n\
  -h, --help			    Print this message and exit\n\
  -i FILE, --image-file=FILE	    Use image FILE instead of `gforth.fi'\n\
  -l SIZE, --locals-stack-size=SIZE Specify locals stack size\n\
  -m SIZE, --dictionary-size=SIZE   Specify Forth dictionary size\n\
  --no-dynamic			    Use only statically compiled primitives\n\
  --no-offset-im		    Load image at normal position\n\
  --no-super                        No dynamically formed superinstructions\n\
  --offset-image		    Load image at a different position\n\
  -p PATH, --path=PATH		    Search path for finding image and sources\n\
  -r SIZE, --return-stack-size=SIZE Specify return stack size\n\
  -v, --version			    Print version and exit\n\
SIZE arguments consist of an integer followed by a unit. The unit can be\n\
  `b' (byte), `e' (element; default), `k' (KB), `M' (MB), `G' (GB) or `T' (TB).\n",
	      argv[0]);
      optind--;
      return;
    }
  }
}
#endif

#ifdef INCLUDE_IMAGE
extern Cell image[];
extern const char reloc_bits[];
#endif

int main(int argc, char **argv, char **env)
{
#ifdef HAS_OS
  char *path = getenv("GFORTHPATH") ? : DEFAULTPATH;
#else
  char *path = DEFAULTPATH;
#endif
#ifndef INCLUDE_IMAGE
  char *imagename="gforth.fi";
  FILE *image_file;
  Address image;
#endif
  int retvalue;
	  
#if defined(i386) && defined(ALIGNMENT_CHECK)
  /* turn on alignment checks on the 486.
   * on the 386 this should have no effect. */
  __asm__("pushfl; popl %eax; orl $0x40000, %eax; pushl %eax; popfl;");
  /* this is unusable with Linux' libc.4.6.27, because this library is
     not alignment-clean; we would have to replace some library
     functions (e.g., memcpy) to make it work. Also GCC doesn't try to keep
     the stack FP-aligned. */
#endif

  /* buffering of the user output device */
#ifdef _IONBF
  if (isatty(fileno(stdout))) {
    fflush(stdout);
    setvbuf(stdout,NULL,_IONBF,0);
  }
#endif

  progname = argv[0];

#ifdef HAS_OS
  gforth_args(argc, argv, &path, &imagename);
#endif

#ifdef INCLUDE_IMAGE
  set_stack_sizes((ImageHeader *)image);
  if(((ImageHeader *)image)->base != image)
    relocate(image, reloc_bits, ((ImageHeader *)image)->image_size,
	     (Label*)engine(0, 0, 0, 0, 0));
  alloc_stacks((ImageHeader *)image);
#else
  image_file = open_image_file(imagename, path);
  image = loader(image_file, imagename);
#endif
  gforth_header=(ImageHeader *)image; /* used in SIGSEGV handler */

  {
    char path2[strlen(path)+1];
    char *p1, *p2;
    Cell environ[]= {
      (Cell)argc-(optind-1),
      (Cell)(argv+(optind-1)),
      (Cell)strlen(path),
      (Cell)path2};
    argv[optind-1] = progname;
    /*
       for (i=0; i<environ[0]; i++)
       printf("%s\n", ((char **)(environ[1]))[i]);
       */
    /* make path OS-independent by replacing path separators with NUL */
    for (p1=path, p2=path2; *p1!='\0'; p1++, p2++)
      if (*p1==PATHSEP)
	*p2 = '\0';
      else
	*p2 = *p1;
    *p2='\0';
    retvalue = go_forth(image, 4, environ);
#ifdef VM_PROFILING
    vm_print_profile(stderr);
#endif
    deprep_terminal();
  }
  return retvalue;
}
