/*
  $Id: main.c,v 1.26 1995-08-27 19:56:33 pazsan Exp $
  Copyright 1993 by the ANSI figForth Development Group
*/

#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <assert.h>
#include <stdlib.h>
#include "forth.h"
#include "io.h"
#include "getopt.h"

#ifdef MSDOS
jmp_buf throw_jmp_buf;
#endif

#ifndef DEFAULTPATH
#  define DEFAULTPATH "/usr/local/lib/gforth:."
#endif

#ifdef DIRECT_THREADED
#  define CA(n)	(symbols[(n)])
#else
#  define CA(n)	((Cell)(symbols+(n)))
#endif

#define maxaligned(n)	(typeof(n))((((Cell)n)+sizeof(Float)-1)&-sizeof(Float))

static Cell dictsize=0;
static Cell dsize=0;
static Cell rsize=0;
static Cell fsize=0;
static Cell lsize=0;
char *progname;


/* image file format:
 *   preamble (is skipped off), size multiple of 8
 *   magig: "gforth00" (means format version 0.0)
 *   "gforth0x" means format 0.1,
 *              whereas x in 2 4 8 for big endian and 3 5 9 for little endian
 *              and x & -2 is the size of the cell in byte.
 *   size of image with stacks without tags (in bytes)
 *   size of image without stacks and tags (in bytes)
 *   size of data and FP stack (in bytes)
 *   pointer to start of code
 *   pointer into throw (for signal handling)
 *   pointer to dictionary
 *   data (size in image[1])
 *   tags (1 bit/data cell)
 *
 * tag==1 mean that the corresponding word is an address;
 * If the word is >=0, the address is within the image;
 * addresses within the image are given relative to the start of the image.
 * If the word is =-1, the address is NIL,
 * If the word is between -2 and -5, it's a CFA (:, Create, Constant, User)
 * If the word is -7, it's a DOES> CFA
 * If the word is -8, it's a DOES JUMP
 * If the word is <-9, it's a primitive
 */

void relocate(Cell *image, char *bitstring, int size, Label symbols[])
{
  int i=0, j, k, steps=(size/sizeof(Cell))/8;
  char bits;
/*   static char bits[8]={0x80,0x40,0x20,0x10,0x08,0x04,0x02,0x01};*/
   
  for(k=0; k<=steps; k++)
    for(j=0, bits=bitstring[k]; j<8; j++, i++, bits<<=1)
      if(bits & 0x80)
	if(image[i]<0)
	  switch(image[i])
	    {
	    case CF_NIL      : image[i]=0; break;
	    case CF(DOCOL)   :
	    case CF(DOVAR)   :
	    case CF(DOCON)   :
	    case CF(DOUSER)  : 
	    case CF(DODEFER) : 
	    case CF(DOSTRUC) : MAKE_CF(image+i,symbols[CF(image[i])]); break;
	    case CF(DODOES)  : MAKE_DOES_CF(image+i,image[i+1]+((Cell)image));
	      break;
	    case CF(DOESJUMP): MAKE_DOES_HANDLER(image+i); break;
	    default          : image[i]=(Cell)CA(CF(image[i]));
	    }
	else
	  image[i]+=(Cell)image;
}

Cell *loader(FILE *imagefile)
{
  Cell header[3];
  Cell *image;
  Char magic[8];
  int wholesize;
  int imagesize; /* everything needed by the image */

  static char* endsize[10]=
    {
      "no size information", "",
      "16 bit big endian", "16 bit little endian",
      "32 bit big endian", "32 bit little endian",
      "n/n", "n/n",
      "64 bit big endian", "64 bit little endian",
    };

  do
    {
      if(fread(magic,sizeof(Char),8,imagefile) < 8) {
	fprintf(stderr,"This image doesn't seem to be a gforth image.\n");
	exit(1);
      }
#ifdef DEBUG
      printf("Magic found: %s\n",magic);
#endif
    }
  while(memcmp(magic,"gforth0",7));
  
  if(!(magic[7]=='0' || magic[7] == sizeof(Cell) +
#ifdef WORDS_BIGENDIAN
       '0'
#else
       '1'
#endif
       ))
    { fprintf(stderr,"This image is %s, whereas the machine is %s.\n",
	      endsize[magic[7]-'0'],
	      endsize[sizeof(Cell) +
#ifdef WORDS_BIGENDIAN
		      0
#else
		      1
#endif
		      ]);
      exit(-2);
    };

  fread(header,sizeof(Cell),3,imagefile);
  if (dictsize==0)
    dictsize = header[0];
  if (dsize==0)
    dsize=header[2];
  if (rsize==0)
    rsize=header[2];
  if (fsize==0)
    fsize=header[2];
  if (lsize==0)
    lsize=header[2];
  dictsize=maxaligned(dictsize);
  dsize=maxaligned(dsize);
  rsize=maxaligned(rsize);
  lsize=maxaligned(lsize);
  fsize=maxaligned(fsize);
  
  wholesize = dictsize+dsize+rsize+fsize+lsize;
  imagesize = header[1]+((header[1]-1)/sizeof(Cell))/8+1;
  image=malloc((wholesize>imagesize?wholesize:imagesize)+sizeof(Float));
  image = maxaligned(image);
  memset(image,0,wholesize); /* why? - anton */
  image[0]=header[0];
  image[1]=header[1];
  image[2]=header[2];
  
  fread(image+3,1,header[1]-3*sizeof(Cell),imagefile);
  fread(((void *)image)+header[1],1,((header[1]-1)/sizeof(Cell))/8+1,
	imagefile);
  fclose(imagefile);
  
  if(image[5]==0) {
    relocate(image,(char *)image+header[1],header[1],engine(0,0,0,0,0));
  }
  else if(image[5]!=(Cell)image) {
    fprintf(stderr,"Corrupted image address, please recompile image\n");
    exit(1);
  }

  CACHE_FLUSH(image,image[1]);
  
  return(image);
}

int go_forth(Cell *image, int stack, Cell *entries)
{
  Cell *sp=(Cell*)((void *)image+dictsize+dsize);
  Address lp=(Address)((void *)sp+lsize);
  Float *fp=(Float *)((void *)lp+fsize);
  Cell *rp=(Cell*)((void *)fp+rsize);
  Xt *ip=(Xt *)((Cell)image[3]);
  int throw_code;
  
  for(;stack>0;stack--)
    *--sp=entries[stack-1];
  
  install_signal_handlers(); /* right place? */
  
  if ((throw_code=setjmp(throw_jmp_buf))) {
    static Cell signal_data_stack[8];
    static Cell signal_return_stack[8];
    static Float signal_fp_stack[1];
    
    signal_data_stack[7]=throw_code;
    
    return((int)engine((Xt *)image[4],signal_data_stack+7,
		       signal_return_stack+8,signal_fp_stack,0));
  }
  
  return((int)engine(ip,sp,rp,fp,lp));
}

int convsize(char *s, int elemsize)
/* converts s of the format #+u (e.g. 25k) into the number of bytes.
   the unit u can be one of bekM, where e stands for the element
   size. default is e */
{
  char *endp;
  int n,m;

  m = elemsize;
  n = strtoul(s,&endp,0);
  if (endp!=NULL) {
    if (strcmp(endp,"b")==0)
      m=1;
    else if (strcmp(endp,"k")==0)
      m=1024;
    else if (strcmp(endp,"M")==0)
      m=1024*1024;
    else if (strcmp(endp,"e")!=0) {
      fprintf(stderr,"%s: cannot grok size specification %s\n", progname, s);
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
#endif

  progname = argv[0];
  if ((path=getenv("GFORTHPATH"))==NULL)
    path = strcpy(malloc(strlen(DEFAULTPATH)+1),DEFAULTPATH);
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
      {0,0,0,0}
      /* no-init-file, no-rc? */
    };

    c = getopt_long(argc, argv, "+i:m:d:r:f:l:p:", opts, &option_index);

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
    case 'p': path = optarg; break;
    }
  }
  path1=path;
  do {
    char *pend=strchr(path, ':');
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
    }
    path=pend+(*pend==':');
  } while (image_file==NULL);
  
  {
    Cell environ[]= {
      (Cell)argc-(optind-1),
      (Cell)(argv+(optind-1)),
      (Cell)path1};
    argv[optind-1] = progname;
    /*
       for (i=0; i<environ[0]; i++)
       printf("%s\n", ((char **)(environ[1]))[i]);
       */
    retvalue=go_forth(loader(image_file),3,environ);
    deprep_terminal();
    exit(retvalue);
  }
}
