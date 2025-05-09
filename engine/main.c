/* command line interpretation, image loading etc. for Gforth


  Authors: Anton Ertl, Bernd Paysan, Jens Wilke, David Kühling
  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.
*/

#include "config.h"
#include "forth.h"
#include "symver.h"
#include <errno.h>
#include <ctype.h>
#include <stdio.h>
#include <unistd.h>
#include <string.h>
#include <math.h>
#include <sys/types.h>
#ifdef HAVE_ALLOCA_H
#include <alloca.h>
#endif
#ifdef HAVE_MCHECK
#include <mcheck.h>
#endif
#ifndef STANDALONE
#include <sys/stat.h>
#endif
#include <fcntl.h>
#include <assert.h>
#include <stdlib.h>
#include <signal.h>

#ifndef STANDALONE
#if HAVE_SYS_MMAN_H
#include <sys/mman.h>
#endif
#endif
#include "io.h"
#include "getopt.h"
#ifndef STANDALONE
#include <locale.h>
#endif


#if defined(MAP_JIT)
# include <pthread.h>
// if JIT is enabled, set DEFAULT_TRIGGER to 0
# define DEFAULT_TRIGGER 0
# define jit_map_normal() ({ trigger_no_dynamic = DEFAULT_TRIGGER; map_extras = pthread_jit_write_protect_supported_np() ? 0 : MAP_JIT; })
# define jit_map_code()   ({ trigger_no_dynamic = 1; map_extras = MAP_JIT; })
# define jit_write_enable()  pthread_jit_write_protect_np(0)
# define jit_write_disable() pthread_jit_write_protect_np(1)
/* #elif defined(__OpenBSD__)
# define MAP_JIT 0
# define DEFAULT_TRIGGER 0
# define jit_map_normal() trigger_no_dynamic = DEFAULT_TRIGGER
# define jit_map_code()   trigger_no_dynamic = 0
# define jit_write_enable() ({ debugp(stderr, "code -> RW %p:%lx\n", code_area, code_area_size); mprotect(code_area, code_area_size, PROT_READ | PROT_WRITE); })
# define jit_write_disable() ({ debugp(stderr, "code -> RX %p:%lx\n", code_area, code_area_size); mprotect(code_area, code_area_size, PROT_READ | PROT_EXEC); })*/
#else
# define MAP_JIT 0
# define DEFAULT_TRIGGER 0
# define jit_map_normal() trigger_no_dynamic = DEFAULT_TRIGGER
# define jit_map_code()   trigger_no_dynamic = 1
# define jit_write_enable()
# define jit_write_disable()
#endif

static int trigger_no_dynamic = DEFAULT_TRIGGER;

/* output rules etc. for burg with --debug and --print-sequences */
/* #define BURG_FORMAT*/

typedef enum prim_num {
/* definitions of N_execute etc. */
#include PRIM_NUM_I
  N_START_SUPER
} PrimNum;

/* global variables for engine.c 
   We put them here because engine.c is compiled several times in
   different ways for the same engine. */
PER_THREAD stackpointers gforth_SPs;

user_area* gforth_main_UP=NULL;

#ifdef HAS_FFCALL

#include <callback.h>

PER_THREAD va_alist gforth_clist;

void gforth_callback(Xt* fcall, void * alist)
{
  /* save global valiables */
  Cell *rp = gforth_RP;
  Cell *sp = gforth_SP;
  Float *fp = gforth_FP;
  Address lp = gforth_LP;
  va_alist clist = gforth_clist;

  gforth_clist = (va_alist)alist;

  gforth_engine(fcall sr_call);

  /* restore global variables */
  gforth_RP = rp;
  gforth_SP = sp;
  gforth_FP = fp;
  gforth_LP = lp;
  gforth_clist = clist;
}
#endif

#ifdef HAS_FILE
char* fileattr[6]={"rb","rb","r+b","r+b","wb","wb"};
char* pfileattr[6]={"r","r","r+","r+","w","w"};

#ifndef O_BINARY
#define O_BINARY 0
#endif
#ifndef O_TEXT
#define O_TEXT 0
#endif

int ufileattr[6]= {
  O_RDONLY|O_BINARY, O_RDONLY|O_BINARY,
  O_RDWR  |O_BINARY, O_RDWR  |O_BINARY,
  O_WRONLY|O_BINARY, O_WRONLY|O_BINARY };
#endif
/* end global vars for engine.c */

#define PRIM_VERSION 1
/* increment this whenever the primitives change in an incompatible way */

#ifndef DEFAULTPATH
#  define DEFAULTPATH "."
#endif

#ifdef MSDOS
jmp_buf throw_jmp_handler;
#endif

#ifdef NEW_CFA
#define CFA_BYTE_OFFSET         (2*sizeof(Cell))
#else
#define CFA_BYTE_OFFSET         0
#endif
#if defined(DOUBLY_INDIRECT)
#  define CFA(n)	({Cell _n = (n); (((Cell)(((_n & 0x4000) ? vm_prims : xts)+(_n&~0x4000UL)))+((_n & 0x4000) ? CFA_BYTE_OFFSET : 0));})
#else
#  define CFA(n)	(((Cell)(vm_prims+((n)&~0x4000UL)))+CFA_BYTE_OFFSET)
#endif

#define maxaligned(n)	(typeof(n))((((Cell)n)+sizeof(Float)-1)&-sizeof(Float))

#ifdef GFORTH_DEBUGGING
char gforth_debugging=1;
#else
char gforth_debugging=0;
#endif

static UCell dictsize=0;
static UCell dsize=0;
static UCell rsize=0;
static UCell fsize=0;
static UCell lsize=0;
int offset_image=0;
Cell die_on_signal=0;
int ignore_async_signals=0;
#ifndef INCLUDE_IMAGE
static int clear_dictionary=0;
#ifdef PAGESIZE
UCell pagesize=PAGESIZE;
#else
UCell pagesize=1;
#endif
char *progname;
#else
char *progname = "gforth";
int optind = 1;
#endif
#ifndef MAP_NORESERVE
#define MAP_NORESERVE 0
#endif
int map_32bit=0; /* mmap option, can be set to MAP_32BIT with --map_32bit */
#ifndef MAP_32BIT
#define MAP_32BIT 0
#endif
#if defined(__CYGWIN__) && defined(__x86_64)
#define MAP_NORESERVE 0
#endif

#ifndef PROT_EXEC
#define PROT_EXEC 0
#endif

static int map_noreserve=MAP_NORESERVE;
PER_THREAD int map_extras=0;
PER_THREAD int prot_exec=PROT_EXEC;

#define CODE_BLOCK_SIZE (2*1024*1024) /* !! overflow handling for -native */
Address code_area=0;
Cell code_area_size = CODE_BLOCK_SIZE;
Address code_here; /* does for code-area what HERE does for the dictionary */
Address start_flush=NULL; /* start of unflushed code */
UCell generated_bytes=0;
UCell generated_nop_bytes=0;
PrimNum last_jump=0; /* if the last prim was compiled without jump, this
                        is it's PrimNum, otherwise this contains 0 */
Label *ip_at=0; /* during execution of the currently compiled code ip
                    points to ip_at, which may be somewhere behind
                    the position where it would be without ip_update
                    optimization */
PrimNum ip_update0=0; /* base primitive for ip updates */
int min_ip_update=0;
int max_ip_update=0;
Cell inst_index; /* current instruction */
Label **ginstps; /* array of threaded code locations for
                           primitives being optimize_rewrite()d */

static int no_super=0;   /* true if compile_prim should not fuse prims */
static int no_dynamic=NO_DYNAMIC_DEFAULT; /* if true, no code is generated
					     dynamically */
static int no_dynamic_image=0; /* disable dynamic while loading the image */
static int no_rc0=0;  /* true if don't load ~/.config/gforthrc0 */
static int print_metrics=0; /* if true, print metrics on exit */
static int print_prims=0; /* if true, print primitives on exit */
static int print_nonreloc=0; /* if true, print non-relocatable prims */
static int static_super_number = 10000; /* number of ss used if available */
#define MAX_STATE 9 /* maximum number of states */
#define CANONICAL_STATE 0
static int maxstates = MAX_STATE; /* number of states for stack caching */
static int opt_ip_updates =  /* 0=disable, 1=simple, >=2=also optimize ;s */
#ifdef GFORTH_DEBUGGING
  0
#else
  7
#endif
  ;
static int opt_ip_updates_branch =  /* 0=disable, n>0: branch can use n */
#ifdef GFORTH_DEBUGGING
  0
#else
  3
#endif
  ;
static int ss_greedy = 0; /* if true: use greedy, not optimal ss selection */
static int tpa_noequiv = 0;     /* if true: no state equivalence checking */
static int tpa_noautomaton = 0; /* if true: no tree parsing automaton */
static int tpa_trace = 0; /* if true: data for line graph of new states etc. */
static int print_sequences = 0; /* print primitive sequences for optimization */
static int relocs = 0;
static int nonrelocs = 0;

#ifdef HAS_DEBUG
int debug=0;
int debug_mcheck=0;
int debug_prim=1;
# define debugp(x...) do { if (debug) fprintf(x); } while (0)
#else
# define perror(x...)
# define fprintf(x...)
# define debugp(x...)
#endif

ImageHeader *gforth_header;
Label *vm_prims;
#ifdef DOUBLY_INDIRECT
Label *xts; /* same content as vm_prims, but should only be used for xts */
Label *labels; /* labels, as pointed to by vm_prims */
#endif

#ifndef CODE_ALIGNMENT
#define CODE_ALIGNMENT 0
#endif

#define MAX_IMMARGS 2

typedef struct {
  Label start; /* NULL if not relocatable */
  Cell len1;   /* the length of the ip update */
  Cell length; /* includes len1; only includes the jump iff superend is true*/
  Cell restlength; /* length of the rest (i.e., the jump or (on superend) 0) */
  unsigned uses; /* number of uses */
  char superend; /* true if primitive ends superinstruction, i.e.,
                     unconditional branch, execute, etc. */
  signed char max_ip_offset; /* this primitive has ip_offset=0, but
                                the following max_ip_offset primitives
                                are variants of the same
                                primitive+stack-caching with
                                consecutive ip_offsets, up to
                                max_ip_offset, and they are all
                                relocatable */
  signed char min_ip_offset; /* as above */
  Cell nimmargs;
  struct immarg {
    Cell offset; /* offset of immarg within prim */
    char rel;    /* true if immarg is relative */
  } immargs[MAX_IMMARGS];
} PrimInfo;

PrimInfo *priminfos;

#ifndef NO_DYNAMIC
void init_ss_cost(void);

static int is_relocatable(int p)
{
  return !no_dynamic && priminfos[p].start != NULL;
}
#else /* defined(NO_DYNAMIC) */
static int is_relocatable(int p)
{
  return 0;
}
#endif /* defined(NO_DYNAMIC) */

const char * const prim_names[]={
#if !defined(NO_DYNAMIC)
#include PRIM_NAMES_I
#endif
  ""
};

#ifdef MEMCMP_AS_SUBROUTINE
int gforth_memcmp(const char * s1, const char * s2, size_t n)
{
  return memcmp(s1, s2, n);
}

Char *gforth_memmove(Char * dest, const Char* src, Cell n)
{
  return memmove(dest, src, n);
}

Char *gforth_memset(Char * s, Cell c, UCell n)
{
  return memset(s, c, n);
}
#endif

static Cell max(Cell a, Cell b)
{
  return a>b?a:b;
}

static Cell min(Cell a, Cell b)
{
  return a<b?a:b;
}

#ifndef STANDALONE
/* image file format:
 *  "#! binary-path -i\n" (e.g., "#! /usr/local/bin/gforth-0.4.0 -i\n")
 *   padding to a multiple of 8
 *   magic: "Gforth4x" means format 0.8,
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
 * If the image has sections, they follow after the main image with each
 * section starting with the magic "Section.". A section starts with the
 * section header (see section.fs and forth.h), has data and tags.
 *
 * tag==1 means that the corresponding word is an address;
 * If the word is >=0, the address is within the image;
 * addresses within the image are given relative to the start of the section.
 * bits MSB..MSB-7 (8 bits) index the section
 * If the word =-1 (CF_NIL), the address is NIL,
 * If the word is <CF_NIL and >=CF(DOER_MAX), it's a CFA (:, Create, ...)
 * If the word is <CF(DOER_MAX) and bit 14 is set, it's the xt of a primitive
 * If the word is <CF(DOER_MAX) and bit 14 is clear, 
 *                                        it's the threaded code of a primitive
 * bits 13..9 of a primitive token state which group the primitive belongs to,
 * bits 8..0 of a primitive token index into the group
 */

Cell groups[32] = {
  0,
  0
#undef GROUP
#undef GROUPADD
#define GROUPADD(n) +n
#define GROUP(x, n) , 0
#include PRIM_GRP_I
#undef GROUP
#undef GROUPADD
#define GROUP(x, n)
#define GROUPADD(n)
};

static void flush_to_here(void)
{
#ifndef NO_DYNAMIC
  if (start_flush)
    FLUSH_ICACHE((caddr_t)start_flush, code_here-start_flush);
  start_flush=code_here;
#endif
}

void gforth_compile_range(Cell *image, Cell size,
			  Char *bitstring, Char *targets)
{
  int i, k;
  int steps=(((size-1)/sizeof(Cell))/RELINFOBITS)+1;

  if(size<=0)
    return;

  debugp(stderr, "compile code range %p:%lx\n", image, size);
  jit_write_enable();
  for(i=k=0; k<steps; k++) {
    Char bitmask;
    for(bitmask=(1U<<(RELINFOBITS-1)); bitmask; i++, bitmask>>=1) {
      /*      fprintf(stderr,"relocate: image[%d]\n", i);*/
      if(targets[k] & bitmask) {
	// debugp(stderr,"target: image[%d]\n", i);
	compile_prim1(0);
      }
      if(bitstring[k] & bitmask) {
	// debugp(stderr,"relocate: image[%d]=%d of %d\n", i, image[i], size/sizeof(Cell));
	compile_prim1(&image[i]);
      }
    }
  }

  compile_prim1(NULL);
  flush_to_here();
  jit_write_disable();
}

static unsigned char *gforth_relocate_range(Address sections[], Cell bases[],
					    Cell *image, Cell size, Cell base,
					    Char *bitstring, int sect)
{
  int i, k;
  int steps=(((size-1)/sizeof(Cell))/RELINFOBITS)+1;
  unsigned char *targets=malloc_l(steps);
  memset(targets, 0, steps);

  for(i=k=0; k<steps; k++) {
    Char bitmask;
    for(bitmask=(1U<<(RELINFOBITS-1)); bitmask; i++, bitmask>>=1) {
      Cell token;
      /*      fprintf(stderr,"relocate: image[%d]\n", i);*/
      if(bitstring[k] & bitmask) {
	// debugp(stderr,"relocate: image[%d]=%d of %d\n", i, image[i], size/sizeof(Cell));
	assert(i < steps*RELINFOBITS);
	token=image[i];
	bitstring[k] &= ~bitmask;
	if(SECTION(token)==PRIMSECTION) {
	  int group = (-token & 0x3E00) >> 9;
	  if(group == 0) {
	    switch(token|0x4000) {
	    case CF_NIL      : image[i]=0; break;
#if !defined(DOUBLY_INDIRECT)
	    case CF(DOER_MAX) ... CF(DOCOL):
	      compile_prim1(0); /* flush primitive state whatever it is in */
	      MAKE_CF(image+i,vm_prims[CF(token)]);
	      break;
#endif /* !defined(DOUBLY_INDIRECT) */
	    default          : /* backward compatibility */
	      /*	      printf("Code field generation image[%x]:=CFA(%x)\n",
			      i, CF(image[i])); */
	      if (CF((token | 0x4000))<npriminfos) {
		image[i]=(Cell)CFA(CF(token));
#ifdef DIRECT_THREADED
		if ((token & 0x4000) == 0) { /* threaded code, no CFA */
		  bitstring[k] |= bitmask;
		}
#endif
	      } else if(debug_prim) {
		Char * dumpa = (Char*)&image[i];
		for(; dumpa < (Char*)&image[i+8]; dumpa++) {
		  fprintf(stderr, "%02x ", *dumpa);
		}
		fprintf(stderr, "\n");
		fprintf(stderr,"Primitive %ld used in this image at %p (offset $%x) is not implemented by this\n engine (%s); executing this code will crash.\n",(long)CF(token), &image[i], i, PACKAGE_VERSION);
	      }
	    }
	  } else {
	    int tok = -token & 0x1FF;
	    if (tok < (groups[group+1]-groups[group])) {
#if defined(DOUBLY_INDIRECT)
	      image[i]=(Cell)CFA(((groups[group]+tok) | (CF(token) & 0x4000)));
#else
	      image[i]=(Cell)CFA((groups[group]+tok));
#endif
#ifdef DIRECT_THREADED
	      if ((token & 0x4000) == 0) { /* threaded code, no CFA */
		bitstring[k] |= bitmask;
	      } else if((token & 0x8000) == 0) { /* special CFA */
		/* debugp(stderr, "image[%x] = symbols[%x]\n", i, groups[group]+tok); */
		MAKE_CF(image+i,vm_prims[groups[group]+tok]);
	      }
#endif
#if defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED)
	      if((token & 0x8000) == 0) { /* special CFA */
		/* debugp(stderr, "image[%x] = symbols[%x] = %p\n", i, groups[group]+tok, symbols[groups[group]+tok]); */
		MAKE_CF(image+i,vm_prims[groups[group]+tok]);
	      }
#endif
	    } else if(debug_prim) {
	      Char * dumpa = (Char*)&image[i];
	      for(; dumpa < (Char*)&image[i+8]; dumpa++) {
		fprintf(stderr, "%02x ", *dumpa);
	      }
	      fprintf(stderr, "\n");
	      fprintf(stderr,"Primitive %lx, %d of group %d used in this image at %p (offset $%x) is not implemented by this\n engine (%s); executing this code will crash.\n", (long)-token, tok, group, &image[i],i,PACKAGE_VERSION);
	    }
	  }
	} else {
	  /* if base is > 0: 0 is a null reference so don't adjust*/
	  if (token>=base) {
	    UCell sec = SECTION(token);
	    UCell start = (Cell) (((void *) sections[sec]) - ((void *) bases[sec]));
	    image[i]=start+INSECTION(token);
	    /* mark branch targets */
	    if(sec == sect) { /* address within the same section */
	      UCell bitnum=(INSECTION(token)-INSECTION(base))/sizeof(Cell);
	      if (bitnum/RELINFOBITS < (UCell)steps)
		targets[bitnum/RELINFOBITS] |= 1U << ((~bitnum)&(RELINFOBITS-1));
	    }
	  } else if(token!=0) {
	    fprintf(stderr, "tagged item image[%x]=%llx unrelocated\n", i, (long long)image[i]);
	  }
	}
      }
    }
  }
  return targets;
}

void gforth_relocate(Address sections[], Char *bitstrings[], 
		     UCell sizes[], Cell bases[])
{
  /* 
   * A virtual start address that's the real start address minus 
   * the one in the image 
   */
  int i;

  /* group index into table */
  if(groups[31]==0) {
    int groupsum=0;
    for(i=0; i<32; i++) {
      groupsum += groups[i];
      groups[i] = groupsum;
      /* printf("group[%d]=%d\n",i,groupsum); */
    }
    i=0;
  }
  
  for (i=0; i<=PRIMSECTION; i++) {
    Char * bitstring=bitstrings[i];
    Cell * image=(Cell*)sections[i];
    UCell size=sizes[i];
    Cell base=bases[i];

    debugp(stderr, "relocate section %i, %p:%lx\n", i, (void *)base, size);
    
    if(!bitstring) break;
    
    unsigned char *targets = gforth_relocate_range(sections, bases,
						   image, size, base,
						   bitstring, i);

    gforth_compile_range(image, size, bitstring, targets);
    
    free(targets);
    if(i==0)
      image[0] = (Cell)image;
  }
}

#ifndef DOUBLY_INDIRECT
static UCell checksum(Label symbols[])
{
  UCell r=PRIM_VERSION;
  Cell i;

  for (i=DOCOL; i<=DOER_MAX; i++) {
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
#endif

static Address verbose_malloc(Cell size)
{
  Address r;
  /* leave a little room (64B) for stack underflows */
  if ((r = malloc_l(size+64))==NULL) {
    perror(progname);
    return r;
  }
  r = (Address)((((Cell)r)+(sizeof(Float)-1))&(-sizeof(Float)));
  debugp(stderr, "verbose malloc($%lx) succeeds, address=%p\n", (long)size, r);
  return r;
}

static void after_alloc(char * description, Address r, Cell size)
{
  if (r != (Address)-1) {
    debugp(stderr, "%s success, address=%p\n", description, r);
  } else {
    debugp(stderr, "%s failed: %s\n", description, strerror(errno));
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
#ifndef PROT_NONE
# define PROT_NONE 0
#endif
#if !defined(MAP_ANON) && defined(MAP_ANONYMOUS)
# define MAP_ANON MAP_ANONYMOUS
#endif

#if defined(HAVE_MMAP)
Address alloc_mmap(Cell size)
{
  void *r=MAP_FAILED;
  static int dev_zero=-1;

#if !defined(MAP_ANON)
  /* Ultrix (at least) does not define MAP_FILE and MAP_PRIVATE (both are
     apparently defaults) */
  int MAP_ANON=MAP_FILE;

  if (dev_zero == -1)
    dev_zero = open("/dev/zero", O_RDONLY);
  if (dev_zero == -1) {
    r = MAP_FAILED;
    debugp(stderr, "open(\"/dev/zero\"...) failed (%s), no mmap; ", 
	      strerror(errno));
    after_alloc("/dev/zero", r, size);
    return r;
  }
#endif /* !defined(MAP_ANON) */
  if (MAP_32BIT && map_32bit) {
    debugp(stderr,"try mmap(%p, $%lx, %x, %x, %i, %i); ", (void*)0, size, prot_exec|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras|MAP_32BIT, dev_zero, 0);
    r=
#ifdef PROT_RWX_FAIL
      prot_exec ? ({ errno=EPERM; MAP_FAILED; }) :
#endif
      mmap(0, size, prot_exec|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras|MAP_32BIT, dev_zero, 0);
    after_alloc("RWX+32", r, size);
  }
  if (r==MAP_FAILED) {
    debugp(stderr,"try mmap(%p, $%lx, %x, %x, %i, %i); ", (void*)0, size, prot_exec|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras, dev_zero, 0);
    r=
#ifdef PROT_RWX_FAIL
      prot_exec ? ({ errno=EPERM; MAP_FAILED; }) :
#endif
      mmap(0, size, prot_exec|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras, dev_zero, 0);
    after_alloc("RWX", r, size);
  }
  if (r==MAP_FAILED) {
    if(trigger_no_dynamic) {
      debugp(stderr,"disabling dynamic native code generation ");
      no_dynamic = 1;
#ifndef NO_DYNAMIC
      init_ss_cost();
#endif
      prot_exec = 0;
      debugp(stderr,"try mmap(%p, $%lx, %x, %x, %i, %i); ", (void*)0, size, prot_exec|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras, dev_zero, 0);
      r=
#ifdef PROT_RWX_FAIL
	prot_exec ? ({ errno=EPERM; MAP_FAILED; }) :
#endif
	mmap(0, size, prot_exec|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras, dev_zero, 0);
    } else {
      debugp(stderr,"try mmap(%p, $%lx, %x, %x, %i, %i); ", (void*)0, size, PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras, dev_zero, 0);
      r=mmap(0, size, PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE|map_noreserve|map_extras, dev_zero, 0);
    }
    after_alloc("RW", r, size);
  }
  return r;  
}

void page_noaccess(void *a)
{
  /* try mprotect first; with munmap the page might be allocated later */
  debugp(stderr, "try mprotect(%p,$%lx,PROT_NONE); ", a, (long)pagesize);
  if (mprotect(a, pagesize, PROT_NONE)==0) {
    debugp(stderr, "ok\n");
    return;
  }
  debugp(stderr, "failed: %s\n", strerror(errno));
  debugp(stderr, "try munmap(%p,$%lx); ", a, (long)pagesize);
  if (munmap(a,pagesize)==0) {
    debugp(stderr, "ok\n");
    return;
  }
  debugp(stderr, "failed: %s\n", strerror(errno));
}  
#endif

static inline size_t wholepage(size_t n)
{
  return (n+pagesize-1)& -pagesize;
}

static Address alloc_mmap_guard(Cell size)
{
  Address start, dictguard;
  size = wholepage(size+pagesize);
  start=alloc_mmap(size);
  if(start==MAP_FAILED) {
    start=malloc(size-pagesize);
  } else {
    dictguard=start+size-pagesize;
    page_noaccess(dictguard);
  }
  return start;
}

Address gforth_alloc(Cell size)
{
#if defined(HAVE_MMAP)
  Address r;

  r=alloc_mmap(size);
  if (r!=MAP_FAILED) {
    debugp(stderr, "mmap($%lx) succeeds, address=%p\n", (long)size, r);
    return r;
  }
#endif /* HAVE_MMAP */
  /* use malloc as fallback */
  return verbose_malloc(size);
}

Cell preamblesize=0;

static void *dict_alloc_read(FILE *file, Cell imagesize, Cell dictsize, Cell offset)
{
  void *image = MAP_FAILED;

#if defined(HAVE_MMAP)
  if (offset==0) {
    image=alloc_mmap_guard(dictsize);
    if (image != (void *)MAP_FAILED) {
      void *image1;
      debugp(stderr, "mmap($%lx) succeeds, address=%p\n", (long)dictsize, image);
      debugp(stderr,"try mmap(%p, $%lx, RWX, MAP_FIXED|MAP_FILE, imagefile, 0); ", image, imagesize);
      image1=
#ifdef PROT_RWX_FAIL
	PROT_EXEC ? ({ errno=EPERM; MAP_FAILED; }) :
#endif
	mmap(image, imagesize, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_FIXED|MAP_FILE|MAP_PRIVATE|map_noreserve, fileno(file), 0);
      after_alloc("image1 RWX", image1,dictsize);
#if defined(__ANDROID__) || defined(_AIX)
      if (image1 == (void *)MAP_FAILED)
	goto read_image;
#endif
      if (image1 == (void *)MAP_FAILED) {
        debugp(stderr,"try mmap(%p, $%lx, RW, MAP_FIXED|MAP_FILE, imagefile, 0); ", image, imagesize);
        image1 = mmap(image, imagesize, PROT_READ|PROT_WRITE, MAP_FIXED|MAP_FILE|MAP_PRIVATE|map_noreserve, fileno(file), 0);
        after_alloc("image1 RW", image1,dictsize);
      }
    }
  }
#endif /* defined(HAVE_MMAP) */
  if (image == (void *)MAP_FAILED) {
    if((image = gforth_alloc(dictsize+offset)+offset) == NULL)
      return NULL;
#if defined(__ANDROID__) || defined(_AIX)
  read_image: /* on Android, mmap will fail despite RWX allocs are possible */
#endif
    rewind(file);  /* fseek(imagefile,0L,SEEK_SET); */
    debugp(stderr,"try fread(%p, 1, %lx, file); ", image, imagesize);
    if(imagesize!=fread(image, 1, imagesize, file) || ferror(file)) {
      debugp(stderr, "failed\n");
      return NULL;
    } else {
      debugp(stderr, "succeeded\n");
    }
  }
  return image;
}
#endif

void gforth_free_dict()
{
  Cell image = (-pagesize) & (Cell)gforth_header;
#ifdef HAVE_MMAP
  debugp(stderr,"try unmmap(%p, $%lx); ", (void*)image, dictsize);
  if(!munmap((void*)image, wholepage(dictsize))) {
    debugp(stderr,"ok\n");
  }
#else
  free((void*)image);
#endif
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

  header->dict_size=dictsize;
  header->data_stack_size=dsize;
  header->fp_stack_size=fsize;
  header->return_stack_size=rsize;
  header->locals_stack_size=lsize;
}

#if (__GNUC__<4)
#warning You can ignore the warnings about clobbered variables in gforth_go
#endif

#define NEXTPAGE(addr) ((typeof(addr))((((UCell)(addr)-1)&-pagesize)+pagesize))
#define NEXTPAGE2(addr) ((typeof(addr))((((UCell)(addr)-1)&-pagesize)+2*pagesize))

Cell gforth_go(Xt* ip0)
{
#ifdef SYSSIGNALS
  int throw_code;
  jmp_buf throw_jmp_buf;
  jmp_buf* old_handler;
#endif
  Cell signal_data_stack[24];
  Cell signal_return_stack[16];
  Float signal_fp_stack[1];
  Cell result;

#if defined(SYSSIGNALS) && !defined(STANDALONE)
  old_handler = throw_jmp_handler;
  throw_jmp_handler = &throw_jmp_buf;

  debugp(stderr, "setjmp(%p)\n", *throw_jmp_handler);
  while((throw_code=setjmp(throw_jmp_buf))) {
    signal_data_stack[15]=throw_code;

#ifdef GFORTH_DEBUGGING
    debugp(stderr,"\ncaught signal, throwing exception %d, ip=%p rp=%p\n",
	   throw_code, saved_ip, saved_rp);
    if ((saved_rp-2 > NEXTPAGE2(gforth_UP->sp0)) &&
	(saved_rp < NEXTPAGE(gforth_UP->rp0))) {
      /* no rstack overflow or underflow */
      gforth_RP = saved_rp;
      /* return addresses point behind the call, so also do it here: */
      *--gforth_RP = (Cell)(saved_ip+1);
    } else {
      gforth_RP = signal_return_stack+16;
    }
#else  /* !defined(GFORTH_DEBUGGING) */
    debugp(stderr,"\ncaught signal, throwing exception %d\n", throw_code);
    gforth_RP = signal_return_stack+16;
#endif /* !defined(GFORTH_DEBUGGING) */
    /* fprintf(stderr, "rp=$%x\n",rp0);*/
    
    debugp(stderr,"header=%p, UP=%p\n", gforth_header, gforth_UP);
    ip0=gforth_UP->throw_entry;
    gforth_SP=signal_data_stack+15;
    gforth_FP=signal_fp_stack;
  }
#endif

  debugp(stderr,"run Gforth engine with ip=%p\n", ip0);
  result=((Cell)gforth_engine(ip0 sr_call));
  throw_jmp_handler = old_handler;
  return result;
}

#if !defined(INCLUDE_IMAGE) && !defined(STANDALONE)
static void print_sizes(Cell sizebyte)
     /* print size information */
{
  static char* endianstring[]= { "   big","little" };
  
  fprintf(stderr,"%s endian, cell=%d bytes, char=%d bytes, au=%d bytes\n",
	  endianstring[sizebyte & 1],
	  1 << ((sizebyte >> 1) & 3),
	  1 << ((sizebyte >> 3) & 3),
	  1 << ((sizebyte >> 5) & 3));
}

/* static superinstruction stuff */

struct cost { /* super_info might be a more accurate name */
  char loads;       /* number of stack loads */
  char stores;      /* number of stack stores */
  char updates;     /* number of stack pointer updates */
  char branch;	    /* is it a branch (SET_IP) */
  unsigned char state_in;    /* state on entry */
  unsigned char state_out;   /* state on exit */
  unsigned char imm_ops;     /* number of additional threaded-code slots
                                (immediate arguments+number of components-1) */
  signed char ip_offset;   /* 0 if ip points to the end of the instruction,
                                1 if it points 1 cell earlier ... */
  short offset;     /* offset into super2 table */
  unsigned char length;      /* number of components */
  char branch_to_ip; /* 1 if the prim is a conditional branch to ip, 0 otherwise */
};

PrimNum super2[] = {
#include SUPER2_I
};

struct cost super_costs[] = {
#include COSTS_I
};

struct super_state {
  struct super_state *next;
  PrimNum super;
};

#define HASH_SIZE 256

struct super_table_entry {
  struct super_table_entry *next;
  PrimNum *start;
  short length;
  struct super_state *ss_list; /* list of supers */
} *super_table[HASH_SIZE];
int max_super=2;

struct super_state *state_transitions=NULL;
PrimNum *branches_to_ip = NULL;

static int hash_super(PrimNum *start, int length)
{
  int i, r;
  
  for (i=0, r=0; i<length; i++) {
    r <<= 1;
    r += start[i];
  }
  return r & (HASH_SIZE-1);
}

static struct super_state **lookup_super(PrimNum *start, int length)
{
  int hash=hash_super(start,length);
  struct super_table_entry *p = super_table[hash];

  for (; p!=NULL; p = p->next) {
    if (length == p->length &&
	memcmp((char *)p->start, (char *)start, length*sizeof(PrimNum))==0)
      return &(p->ss_list);
  }
  return NULL;
}

static PrimNum lookup_ss(PrimNum *start, int length,
                              unsigned state_in, unsigned state_out)
{
  struct super_state *ss;
  struct super_state **ss_listp = lookup_super(start, length);
  if (ss_listp == NULL)
    return -1;
  ss = *ss_listp;
  for (; ss!=NULL; ss = ss->next) {
    struct cost *c = &super_costs[ss->super];
    if (c->state_in == state_in && c->state_out == state_out)
      return ss->super;
  }
  return -1;
}

/* primitives, where ip is dead at the start, so no ip update is needed */
static PrimNum ip_dead[] = {N_semis,
                            N_execute_semis,
                            N_execute_semis,
                            N_fast_throw};
/* the first N_execute_semis is overwritten by the 2->1 version */

static int MAYBE_UNUSED prim_ip_dead(PrimNum p)
{
  long i;
  for (i=0; i<sizeof(ip_dead)/sizeof(ip_dead[0]); i++)
    if (p==ip_dead[i])
      return 1;
  return 0;
}

static void prepare_super_table()
{
  long i;
  long nsupers = 0;
  long nprims = sizeof(super_costs)/sizeof(super_costs[0]);

  branches_to_ip = calloc(nprims,sizeof(PrimNum));
  for (i=0; i<nprims; i++) {
    struct cost *c = &super_costs[i];
    if (c->branch_to_ip) {
      PrimNum ss =
        lookup_ss(super2+c->offset, c->length, c->state_in, c->state_out);
      if (ss != -1)
        branches_to_ip[ss]=i;
      continue;
    }
    if ((c->length < 2 || nsupers < static_super_number) &&
	c->state_in < maxstates && c->state_out < maxstates) {
      struct super_state **ss_listp= lookup_super(super2+c->offset, c->length);
      if (c->ip_offset == 0) { /* don't enter ip_offset variants */
        struct super_state *ss = malloc_l(sizeof(struct super_state));
        ss->super= i;
        if (c->offset==N_noop && i != N_noop) { /* stack caching transition */
          if (is_relocatable(i)) {
            if (c->state_in==CANONICAL_STATE && c->state_out==CANONICAL_STATE) {
              /* this is actually from the ip-update series */
              assert(ip_update0 == 0); /* no second occurence */
              ip_update0 = i;
              min_ip_update = priminfos[i].min_ip_offset;
              max_ip_update = priminfos[i].max_ip_offset;
            } else { /* it's a state transition */
              ss->next = state_transitions;
              state_transitions = ss;
            }
          }
        } else if (ss_listp != NULL) { /* already registered */
          if (c->state_in==CANONICAL_STATE && c->state_out==CANONICAL_STATE &&
              priminfos != NULL && priminfos[i].max_ip_offset>0) {
            /* ip-update variation header for prim that has its
               original already in the table: replace the original
               with this one so that ip-update variants can be used */
            free(ss);
            ss = *ss_listp;
            assert(c->length == 1);
            for (; ss != NULL; ss = ss->next)
              if (ss->super == c->offset) {
                ss->super = i;
                break;
              }
            assert(ss != NULL);
          } else { /* just add another state combination for the primitive */
            ss->next = *ss_listp;
            *ss_listp = ss;
          }
        } else { /* new entry */
          int hash = hash_super(super2+c->offset, c->length);
          struct super_table_entry **p = &super_table[hash];
          struct super_table_entry *e = malloc_l(sizeof(struct super_table_entry));
          ss->next = NULL;
          e->next = *p;
          e->start = super2 + c->offset;
          e->length = c->length;
          e->ss_list = ss;
          *p = e;
        }
        if (c->length > max_super)
          max_super = c->length;
        if (c->length >= 2)
          nsupers++;
        if (c->offset == N_execute_semis && i != N_execute_semis) {
          /* this particular hack works only if this assertion holds */
          assert(ip_dead[1] == N_execute_semis);
          ip_dead[1]=i;
          debugp(stderr, "Another execute-;s implementation: %ld\n",i);
        }
      }
    }
  }
  debugp(stderr, "Using %ld static superinsts\n", nsupers);
  debugp(stderr, "ip-update0 = %d in %d..%d\n", ip_update0, min_ip_update, max_ip_update);
  if (nsupers>0 && !tpa_noautomaton && !tpa_noequiv) {
    /* Currently these two things don't work together; see Section 3.2
       of <http://www.complang.tuwien.ac.at/papers/ertl+06pldi.ps.gz>,
       in particular Footnote 6 for the reason; hmm, we should be able
       to use an automaton without state equivalence, but that costs
       significant space so we only do it if the user explicitly
       disables state equivalence. */
    debugp(stderr, "Disabling tpa-automaton, because nsupers>0 and state equivalence is enabled.\n");
    tpa_noautomaton = 1;
  }
}

/* dynamic replication/superinstruction stuff */
static char MAYBE_UNUSED superend[]={
#include PRIM_SUPEREND_I
};

Cell npriminfos=0;

Label goto_start;
Cell goto_len;

#ifndef NO_DYNAMIC
static int compare_labels(const void *pa, const void *pb)
{
  Label a = *(Label *)pa;
  Label b = *(Label *)pb;
  return a-b;
}
#endif

MAYBE_UNUSED static Label bsearch_next(Label key, Label *a, UCell n)
     /* a is sorted; return the label >=key that is the closest in a;
        return NULL if there is no label in a >=key */
{
  int mid = (n-1)/2;
  if (n<1)
    return NULL;
  if (n == 1) {
    if (a[0] < key)
      return NULL;
    else
      return a[0];
  }
  if (a[mid] < key)
    return bsearch_next(key, a+mid+1, n-mid-1);
  else
    return bsearch_next(key, a, mid+1);
}

int state_map(int state)
{
  if (state==0) return STACK_CACHE_DEFAULT;
  if (state<=STACK_CACHE_DEFAULT) return state-1;
  return state;
}

#ifndef NO_DYNAMIC
static void gforth_printprims()
{
  unsigned i;
  for (i=0; i<npriminfos; i++) {
    PrimInfo *pi=&priminfos[i];
    struct cost *sc=&super_costs[i];
    fprintf(stderr,"%-17s %d-%d %2d %4d %4d %4d %14p len=%2ld+%3ld+%2ld send=%1d\n",
	    prim_names[i], state_map(sc->state_in), state_map(sc->state_out),
	    sc->ip_offset, i, branches_to_ip[i], pi->uses, pi->start, (long)(pi->len1),
            (long)(pi->length - pi->len1), (long)(pi->restlength), pi->superend);
  }
}
#endif

static void check_prims(Label symbols1[])
{
  int i;
#ifndef NO_DYNAMIC
  Label *symbols2, *goto_p;
  Label *startb;                /* L labels (LABEL1 right behind ip update)*/
  Label *ends1;                 /* K labels (LABEL2 right before NEXT_P1_5) */
  Label *ends1j, *ends1jsorted; /* J labels (LABEL3 right before DO_GOTO) */
  int nends1j;
#endif

  if (debug)
#ifdef __VERSION__
    fprintf(stderr, "Compiled with gcc-" __VERSION__ "\n");
#else
#define xstr(s) str(s)
#define str(s) #s
  fprintf(stderr, "Compiled with gcc-" xstr(__GNUC__) "." xstr(__GNUC_MINOR__) "\n"); 
#endif
  for (i=0; symbols1[i]!=0; i++)
    ;
  npriminfos = i;
  
#ifndef NO_DYNAMIC
  if (no_dynamic)
    return;
  symbols2=gforth_engine2(0 sr_call);
  startb = symbols1+i+1;
  ends1  = startb+i;
  ends1j =   ends1+i;
  goto_p = ends1j+i+1; /* goto_p[0]==before; ...[1]==after;*/
  nends1j = i+1;
  ends1jsorted = (Label *)alloca(nends1j*sizeof(Label));
  memmove(ends1jsorted,ends1j,nends1j*sizeof(Label));
  qsort(ends1jsorted, nends1j, sizeof(Label), compare_labels);

  /* check whether the "goto *" is relocatable */
  goto_len = goto_p[1]-goto_p[0];
  debugp(stderr, "goto * %p %p len=%ld\n",
	 goto_p[0],symbols2[goto_p-symbols1],(long)goto_len);
  if ((goto_len < 0) ||
      memcmp(goto_p[0],symbols2[goto_p-symbols1],goto_len)!=0) { /* unequal */
    no_dynamic = 1;
    debugp(stderr,"  not relocatable, disabling dynamic code generation\n");
    init_ss_cost();
    return;
  }
  goto_start = goto_p[0];

  priminfos = calloc(i,sizeof(PrimInfo));
  
  for (i=0; symbols1[i]!=0; i++) {
    int prim_len = ends1[i]-symbols1[i];
    PrimInfo *pi=&priminfos[i];
    struct cost *sc=&super_costs[i];
    signed char o=sc->ip_offset;
    int j=0;
    char *s1 = (char *)symbols1[i];
    char *s2 = (char *)symbols2[i];
    Label endlabel = bsearch_next(symbols1[i]+1,ends1jsorted,nends1j);

    pi->start = s1;
    pi->len1 = ((char *)startb[i])-s1;
    pi->superend = superend[i]|no_super;
    pi->length = prim_len;
    pi->restlength = endlabel - symbols1[i] - pi->length;
    pi->uses = 0;
    pi->max_ip_offset = 0; /* initial value */
    pi->min_ip_offset = 0; /* initial value */
    pi->nimmargs = 0;
    relocs++;
#if defined(BURG_FORMAT)
    { /* output as burg-style rules */
      int p=super_costs[i].offset;
      if (p==N_noop)
	debugp(stderr, "S%d: S%d = %d (%d);", sc->state_in, sc->state_out, i+1, pi->length);
      else
	debugp(stderr, "S%d: op%d(S%d) = %d (%d);", sc->state_in, p, sc->state_out, i+1, pi->length);
    }
#else
    debugp(stderr, "%-15s %d-%d %4d %p %p len=%2ld+%3ld+%2ld send=%1d",
	   prim_names[i], state_map(sc->state_in), state_map(sc->state_out),
	   i, s1, s2, (long)(pi->len1),
           (long)(pi->length - pi->len1), (long)(pi->restlength), pi->superend);
#endif
    if (endlabel == NULL) {
      pi->start = NULL; /* not relocatable */
      if (pi->length<0) pi->length=100;
#ifndef BURG_FORMAT
      debugp(stderr,"\n   non_reloc: no J label > start found\n");
#endif
      if (print_nonreloc)
        printf("create %s\n",prim_names[i]);
      relocs--;
      nonrelocs++;
      continue;
    }
    if (ends1[i] > endlabel && !pi->superend) {
      pi->start = NULL; /* not relocatable */
      pi->length = endlabel-symbols1[i];
#ifndef BURG_FORMAT
      debugp(stderr,"\n   non_reloc: there is a J label before the K label (restlength<0)\n");
#endif
      if (print_nonreloc)
        printf("create %s\n",prim_names[i]);
      relocs--;
      nonrelocs++;
      continue;
    }
    if (ends1[i] < pi->start && !pi->superend) {
      pi->start = NULL; /* not relocatable */
      pi->length = endlabel-symbols1[i];
#ifndef BURG_FORMAT
      debugp(stderr,"\n   non_reloc: K label before I label (length<0)\n");
#endif
      if (print_nonreloc)
        printf("create %s\n",prim_names[i]);
      relocs--;
      nonrelocs++;
      continue;
    }
    if (CHECK_PRIM(s1, prim_len)) {
#ifndef BURG_FORMAT
      debugp(stderr,"\n   non_reloc: architecture specific check failed\n");
#endif
      pi->start = NULL; /* not relocatable */
      if (print_nonreloc)
        printf("create %s\n",prim_names[i]);
      relocs--;
      nonrelocs++;
      continue;
    }
    if((pi->length<0) || (pi->restlength<0)) {
      pi->length = endlabel-symbols1[i];
      pi->restlength = 0;
#ifndef BURG_FORMAT
      debugp(stderr,"\n   adjust restlen: len/restlen < 0, %ld/%ld",
	     (long)pi->length, (long)pi->restlength);
#endif
    };
    int overall_length=pi->length+pi->restlength;
    while (j<overall_length) {
      if (s1[j] != s2[j]) {
	pi->start = NULL; /* not relocatable */
#ifndef BURG_FORMAT
	debugp(stderr,"\n   non_reloc: engine1!=engine2 offset %3d",j);
#endif
	/* assert(j<prim_len); */
        if (print_nonreloc)
          printf("create %s\n",prim_names[i]);
	relocs--;
	nonrelocs++;
	break;
      }
      j++;
    }
    debugp(stderr,"\n");
    if (opt_ip_updates>2 && o>0) { /* ip-updates info */
      assert(strcmp(prim_names[i],prim_names[i-o])==0);
      /* add this primitive only if all the ip_update variants up to
         here are relocatable: */
      if (pi->start != NULL && priminfos[i-o].max_ip_offset == o-1)
        priminfos[i-o].max_ip_offset = o;
    }
    /* check for ip_update variants with negative ip_offset */
    if (o==0) {
      long k;
      for (k=-1; i+k>=0; k--) {
        // debugp(stderr,"  k=%d, priminfos[i+k].start=%p, super_costs[i+k].ip_offset=%d\n",k,priminfos[i+k].start,super_costs[i+k].ip_offset);
        if (priminfos[i+k].start == NULL || super_costs[i+k].ip_offset != k)
          break;
        assert(strcmp(prim_names[i],prim_names[i+k])==0);
        pi->min_ip_offset = k;
      }
    }
  }
#endif
}

/* Dynamic info for decompilation */
DynamicInfo *dynamicinfos = NULL; /* 2^n-sized growable array */
long ndynamicinfos=0; /* index of next dynamicinfos entry */

DynamicInfo *dynamic_info3(Label *tcp)
{
  DynamicInfo *di;
  if (dynamicinfos != NULL) {
    for (di=dynamicinfos; di<&dynamicinfos[ndynamicinfos]; di++)
      if (di->tcp == tcp)
        return di;
  }
  return NULL;
}

#if !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED))
static DynamicInfo *add_dynamic_info()
/* reserves space for a new Dynamicinfo, returning a pointer to it (for
   filling out) */
{
  long old=ndynamicinfos;
  long new=old+1;
  /* if (dynamicinfos!=NULL && dynamicinfos[old-1].length == 0) {
    return &dynamicinfos[old-1];
    } */
  ndynamicinfos=new;
  if ((old&new)==0) { /* one too early for old>=1, but we can live with that */
    dynamicinfos =
      (DynamicInfo *)realloc(dynamicinfos,2*new*sizeof(DynamicInfo));
    if (dynamicinfos==NULL)
      perror(progname);
  }
  return &dynamicinfos[old];
}
#endif

static void MAYBE_UNUSED  append_code(Address code, size_t length)
{
  memmove(code_here,code,length);
  code_here += length;
  generated_bytes += length;
}

static void MAYBE_UNUSED align_code(void)
     /* align code_here on some platforms */
{
#ifndef NO_DYNAMIC
#if defined(CODE_PADDING)
  Cell alignment = CODE_ALIGNMENT;
  static unsigned char nops[] = CODE_PADDING;
  UCell maxpadding=MAX_PADDING;
  UCell offset = ((UCell)code_here)&(alignment-1);
  UCell length = alignment-offset;
  if (length <= maxpadding) {
    append_code(nops+offset,length);
    generated_nop_bytes += length;
  }
#endif /* defined(CODE_PADDING) */
#endif /* defined(NO_DYNAMIC */
}  

static Cell *ip_update_metrics = NULL;
static Cell nip_update_metrics=0;
static Cell ip_update_metrics_base=-1; /* lower bound for ip_update_metrics */

#ifndef NO_DYNAMIC
static void record_ip_update(Cell n)
{
  n -= ip_update_metrics_base;
  assert(n>=0);
  if (n>=nip_update_metrics) {
    ip_update_metrics = realloc(ip_update_metrics, (n+1)*sizeof(Cell));
    memset(ip_update_metrics+nip_update_metrics,0,
           (n+1-nip_update_metrics)*sizeof(Cell));
    nip_update_metrics=n+1;
  }
  ip_update_metrics[n]++;
}

static Cell append_ip_update1(Label *target, Cell l, Cell u)
/* compile an ip update for updating the ip from ip_at to [l,u] cells
   before target; returns the remaining difference (in the range
   [l,u]) */
{
  Cell cellsdiff = target-ip_at;
  Label *old_ip_at=ip_at;
  assert(opt_ip_updates > 0 || cellsdiff == 0);
  if (cellsdiff < l || cellsdiff > u) {
    do {
      Cell cellsdiff1 = cellsdiff;
      if (cellsdiff1 > max_ip_update)
        cellsdiff1 = max_ip_update;
      else if (cellsdiff < min_ip_update)
        cellsdiff1 = min_ip_update;
      {
        PrimNum p = ip_update0+cellsdiff1;
        PrimInfo *pi = &priminfos[p];
        append_code(pi->start, pi->len1);
      }
      cellsdiff -= cellsdiff1;
      ip_at += cellsdiff1;
    } while (cellsdiff < l || cellsdiff > u);
    if (print_metrics)
      record_ip_update(ip_at-old_ip_at);
  }
  return cellsdiff;
}

static Cell append_ip_update(Cell n)
/* compile an ip update for updating the ip from ip_at to [0,n] cells
   before ginstps[inst_index+1] (where it points without ip-update
   optimization; returns the remaining difference (in the range [0,n]) */
{
  if (opt_ip_updates==0)
    return 0;
  return append_ip_update1(ginstps[inst_index+1],0,n);
}

static void append_jump(void)
{
  if (last_jump) {
    PrimInfo *pi = &priminfos[last_jump];
    /* debugp(stderr, "Copy code %p<=%p+%x,%d\n", code_here, pi->start, pi->length, pi->restlength); */
    assert(ip_at > ginstps[inst_index] || !priminfos[last_jump].superend);
    append_ip_update(0);
    append_code(pi->start+pi->length, pi->restlength);
    /* debugp(stderr, "Copy goto %p<=%p,%d\n", code_here, goto_start, goto_len); */
    append_code(goto_start, goto_len);
    align_code();
    last_jump=0;
  }
}

static void append_jump_previous(void)
/* append a dispatch to the previous primitive */
{
  inst_index--;
  append_jump();
  inst_index++;
}

/* Gforth remembers all code blocks in this list.  On forgetting (by
executing a marker) the code blocks are not freed (because Gforth does
not remember how they were allocated; hmm, remembering that might be
easier and cleaner).  Instead, code_here etc. are reset to the old
value, and the "forgotten" code blocks are reused when they are
needed. */

struct code_block_list {
  struct code_block_list *next;
  Address block;
  Cell size;
} *code_block_list=NULL, **next_code_blockp=&code_block_list;

static int reserve_code_space(UCell size)
{
  if(((Cell)size)<0) size=100;
  if (code_area+code_area_size < code_here+size) {
    struct code_block_list *p;
    int old_prot_exec = prot_exec;
    prot_exec = PROT_EXEC;
    append_jump_previous();
    debugp(stderr,"Did not use %ld bytes in code block\n",
           (long)(code_area+code_area_size-code_here));
    flush_to_here();
    jit_write_disable();
    if (*next_code_blockp == NULL) {
      jit_map_code();
      if((code_here = start_flush = code_area = gforth_alloc(code_area_size)) == NULL) {
	jit_map_normal();
	prot_exec = old_prot_exec;
	return 1;
      }
      jit_write_enable();
      jit_map_normal();
      prot_exec = old_prot_exec;
      p = (struct code_block_list *)malloc_l(sizeof(struct code_block_list));
      *next_code_blockp = p;
      p->next = NULL;
      p->block = code_here;
      p->size = code_area_size;
    } else {
      p = *next_code_blockp;
      code_here = start_flush = code_area = p->block;
    }
    next_code_blockp = &(p->next);
  } else {
    debugp(stderr, "Don't reserve code size %ld, code_area=%p, code_ares_size=%ld, code_here=%p\n", size, code_area, code_area_size, code_here);
  }
  return 0;
}

static Address append_prim(PrimNum p)
{
  PrimInfo *pi = &priminfos[p];
  struct cost *ci = &super_costs[p];
  Address old_code_here;
  if(reserve_code_space(pi->length+pi->restlength+goto_len+CODE_ALIGNMENT-1))
    return NULL;
  /* debugp(stderr, "Copy code %p<=%p,%d\n", code_here, pi->start, pi->length);*/
  old_code_here = code_here;
  if (opt_ip_updates>0) {
    int has_imm = ci->imm_ops+1>ci->length;
    int superend = pi->superend;
    int dead = 0;
    if (opt_ip_updates_branch>0 && ci->length==1 && super2[ci->offset]==N_branch) {
      Label *target = ((Label **)(ginstps[inst_index]))[1];
      // fprintf(stderr, "target = %p\n", target);
      if (ip_at+opt_ip_updates_branch*min_ip_update <= target &&
          target <= ip_at+opt_ip_updates_branch*max_ip_update) {
        append_ip_update1(target,0,0);
        /* append_jump();*/
        ip_at = ginstps[inst_index+1];
        goto append_prim_done;
      }
    }
    if (opt_ip_updates_branch>0 && branches_to_ip[p]!=0) {
      Label *target = ((Label **)(ginstps[inst_index+ci->length-1]))[1];
      if (ip_at+opt_ip_updates_branch*min_ip_update <= target &&
          target <= ip_at+(opt_ip_updates_branch/2)*max_ip_update) {
        append_ip_update1(target,0,0);
        // fprintf(stderr, "%d -> %d\n",p, branches_to_ip[p]);
        p = branches_to_ip[p];
        pi = &priminfos[p];
        ci = &super_costs[p];
        has_imm = 0; /* necessary to suppress append_ip_update() below */
        assert(superend == pi->superend);
      } 
    }
    if (opt_ip_updates>1 && superend && !has_imm)
      dead = prim_ip_dead(p);
    if (has_imm || (superend && !dead)) {
      inst_index += ci->length-1; /* -1 to correct for the +1 in other places */
      p += append_ip_update(pi->max_ip_offset);
      pi = &priminfos[p];
      ci = &super_costs[p];
      dead = superend;
    }
    if (dead)
      ip_at = ginstps[inst_index+1]; /* no (additional) ip update needed
                                        before the primitive, make
                                        sure that it isn't generated
                                        behind it */
    append_code(pi->start+pi->len1, pi->length-pi->len1);
    pi->uses++;
    assert((!superend) || ip_at == ginstps[inst_index+1]);
  } else {
    if (print_metrics)
      record_ip_update(ci->imm_ops+1);
    append_code(pi->start, pi->length);
    ip_at = ginstps[inst_index + ci->length];
  }
 append_prim_done:
  last_jump = (pi->restlength == 0) ? 0 : p;
  return old_code_here;
}

static void reserve_code_super(PrimNum origs[], int ninsts)
{
  int i;
  UCell size = CODE_ALIGNMENT-1; /* alignment may happen first */
  if (no_dynamic)
    return;
  /* use size of the original primitives as an upper bound for the
     size of the superinstruction.  !! This is only safe if we
     optimize for code size (the default) */
  for (i=0; i<ninsts; i++) {
    PrimNum p = origs[i];
    PrimInfo *pi = &priminfos[p];
    if (is_relocatable(p))
      size += pi->length;
    else
      if (i>0)
        size += priminfos[origs[i-1]].restlength+goto_len+CODE_ALIGNMENT-1;
  }
  if (i>0)
    size += priminfos[origs[i-1]].restlength+goto_len;
  reserve_code_space(size);
}
#endif

int forget_dyncode3(Label *tc)
{
#ifdef NO_DYNAMIC
  return -1;
#else
  struct code_block_list *p, **pp;
  Address code = *tc;

  DynamicInfo *di = dynamic_info3(tc);
  if (di != NULL)
    ndynamicinfos = di-dynamicinfos;
  for (pp=&code_block_list, p=*pp; p!=NULL; pp=&(p->next), p=*pp) {
    if (code >= p->block && code < p->block+p->size) {
      next_code_blockp = &(p->next);
      code_here = start_flush = code;
      code_area = p->block;
      last_jump = 0;
      return -1;
    }
  }
  return -no_dynamic;
#endif /* !defined(NO_DYNAMIC) */
}

static long dyncodesize(void)
{
#ifndef NO_DYNAMIC
  struct code_block_list *p;
  long size=0;
  for (p=code_block_list; p!=NULL; p=p->next) {
    if (code_here >= p->block && code_here < p->block+p->size)
      return size + (code_here - p->block);
    else
      size += p->size;
  }
#endif /* !defined(NO_DYNAMIC) */
  return 0;
}

static Cell prim_index(Label code)
/* !! use binary search or hashing instead */
{
  long i;
  for (i=0; vm_prims[i]!=NULL; i++)
    if (vm_prims[i]==code)
      return i;
  return -1;
}

DynamicInfo *decompile_prim3(Label *tcp)
{
  DynamicInfo *di = dynamic_info3(tcp);
  /* fprintf(stderr,"\n%p n=%ld\n",di,ndynamicinfos);*/
  if (di==NULL) {
    static DynamicInfo dyninfo;
    Label _code = *tcp;
    Cell p = prim_index(_code);
    if (p<0)
      dyninfo = (DynamicInfo){tcp,-1,0,0,0,0};
    else {
      struct cost *c = &super_costs[p];
      dyninfo = (DynamicInfo){tcp,0,p,c->length,c->state_in,c->state_out};
    }
    di = &dyninfo;
  }
  return di;
}

Cell fetch_decompile_prim(Cell *a_addr)
/* see documentation for @decompile-prim */
{
  DynamicInfo *di = dynamic_info3((Label *)a_addr);
  int p;
  struct cost *c;
  if (di==NULL) {
    Cell x = *a_addr;
    p = prim_index((Label)x);
    if (p<0) /* not a primitive */
      return x;
  } else
    p = di->prim;
  c = &super_costs[p];
  p = c->offset;
  if (c->length > 1)
    p = super2[p];
  return (Cell)(vm_prims[p]);
}

#if !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED))
static Cell compile_prim_dyn(PrimNum p)
     /* compile prim #p dynamically (mod flags etc.) and return start
        address of generated code for putting it into the threaded code */
{
  Cell static_prim = (Cell)vm_prims[p];

#if defined(NO_DYNAMIC)
  return static_prim;
#else /* !defined(NO_DYNAMIC) */
  Address old_code_here;

  if (no_dynamic)
    return static_prim;
  if (p>=npriminfos || !is_relocatable(p)) {
    append_jump_previous();
    ip_at = ginstps[inst_index+1]; /* advance to behind the non-relocatable inst */
    if (!is_relocatable(p))
      priminfos[p].uses++;
    return static_prim;
  }
  old_code_here = append_prim(p);
  last_jump = p;
  if (priminfos[p].superend)
    append_jump();
  return (Cell)old_code_here;
#endif  /* !defined(NO_DYNAMIC) */
}
#endif

#ifndef NO_DYNAMIC
static int cost_codesize(int prim)
{
  PrimInfo *pi = &priminfos[prim];
  int cost =  pi->length;
  if (opt_ip_updates>0)
    cost -= pi->len1;
  return cost;
}
#endif

static int cost_ls(int prim)
{
  struct cost *c = super_costs+prim;

  return c->loads + c->stores;
}

static int cost_lsu(int prim)
{
  struct cost *c = super_costs+prim;

  return c->loads + c->stores + c->updates;
}

static int cost_nexts(int prim)
{
  return 1;
}

typedef int Costfunc(int);
Costfunc *ss_cost =  /* cost function for optimize_bb */
#ifdef NO_DYNAMIC
cost_lsu;
#else
cost_codesize;
#endif

struct {
  Costfunc *costfunc;
  char *metricname;
  long sum;
} cost_sums[] = {
#ifndef NO_DYNAMIC
  { cost_codesize, "codesize", 0 },
#endif
  { cost_ls,       "ls",       0 },
  { cost_lsu,      "lsu",      0 },
  { cost_nexts,    "nexts",    0 }
};

#ifndef NO_DYNAMIC
void init_ss_cost(void) {
  if (no_dynamic && ss_cost == cost_codesize) {
    ss_cost = cost_nexts;
    cost_sums[0] = cost_sums[1]; /* don't use cost_codesize for print-metrics */
    debugp(stderr, "--no-dynamic conflicts with --ss-min-codesize, reverting to --ss-min-nexts\n");
  }
}
#endif

#define MAX_BB 128 /* maximum number of instructions in BB */
#define INF_COST 1000000 /* infinite cost */

struct waypoint {
  int cost;     /* the cost from here to the end */
  PrimNum inst; /* the inst used from here to the next waypoint */
  char relocatable; /* the last non-transition was relocatable */
  char no_transition; /* don't use the next transition (relocatability)
		       * or this transition (does not change state) */
};

struct tpa_state { /* tree parsing automaton (like) state */
  /* labeling is back-to-front */
  struct waypoint *inst;  /* in front of instruction */
  struct waypoint *trans; /* in front of instruction and transition */
}; 

struct tpa_state *termstate = NULL; /* initialized in loader() */

/* statistics about tree parsing (lazyburg) stuff */
long lb_basic_blocks = 0;
long lb_labeler_steps = 0;
long lb_labeler_automaton = 0;
long lb_labeler_dynprog = 0;
long lb_newstate_equiv = 0;
long lb_newstate_new = 0;
long lb_applicable_base_rules = 0;
long lb_applicable_chain_rules = 0;

#if !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED))
static void init_waypoints(struct waypoint ws[])
{
  int k;

  for (k=0; k<maxstates; k++)
    ws[k].cost=INF_COST;
}

static struct tpa_state *empty_tpa_state()
{
  struct tpa_state *s = malloc(sizeof(struct tpa_state));

  s->inst  = calloc(maxstates,sizeof(struct waypoint));
  init_waypoints(s->inst);
  s->trans = calloc(maxstates,sizeof(struct waypoint));
  /* init_waypoints(s->trans);*/
  return s;
}

static void transitions(struct tpa_state *t)
{
  int k;
  struct super_state *l;
  
  for (k=0; k<maxstates; k++) {
    t->trans[k] = t->inst[k];
    t->trans[k].no_transition = 1;
  }
  for (l = state_transitions; l != NULL; l = l->next) {
    PrimNum s = l->super;
    int jcost;
    struct cost *c=super_costs+s;
    struct waypoint *wi=&(t->trans[c->state_in]);
    struct waypoint *wo=&(t->inst[c->state_out]);
    lb_applicable_chain_rules++;
    if (wo->cost == INF_COST)
      continue;
    jcost = wo->cost + ss_cost(s);
    if (jcost <= wi->cost) {
      wi->cost = jcost;
      wi->inst = s;
      wi->relocatable = wo->relocatable;
      wi->no_transition = 0;
      /* if (ss_greedy) wi->cost = wo->cost ? */
    }
  }
}

static struct tpa_state *make_termstate()
{
  struct tpa_state *s = empty_tpa_state();

  s->inst[CANONICAL_STATE].cost = 0;
  transitions(s);
  return s;
}
#endif

#define TPA_SIZE 16384

struct tpa_entry {
  struct tpa_entry *next;
  PrimNum inst;
  struct tpa_state *state_behind;  /* note: back-to-front labeling */
  struct tpa_state *state_infront; /* note: back-to-front labeling */
} *tpa_table[TPA_SIZE];

#if !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED))
static Cell hash_tpa(PrimNum p, struct tpa_state *t)
{
  UCell it = (UCell )t;
  return (p+it+(it>>14))&(TPA_SIZE-1);
}

static struct tpa_state **lookup_tpa(PrimNum p, struct tpa_state *t2)
{
  int hash=hash_tpa(p, t2);
  struct tpa_entry *te = tpa_table[hash];

  if (tpa_noautomaton) {
    static struct tpa_state *t;
    t = NULL;
    return &t;
  }
  for (; te!=NULL; te = te->next) {
    if (p == te->inst && t2 == te->state_behind)
      return &(te->state_infront);
  }
  te = (struct tpa_entry *)malloc_l(sizeof(struct tpa_entry));
  te->next = tpa_table[hash];
  te->inst = p;
  te->state_behind = t2;
  te->state_infront = NULL;
  tpa_table[hash] = te;
  return &(te->state_infront);
}

static void tpa_state_normalize(struct tpa_state *t)
{
  /* normalize so cost of canonical state=0; this may result in
     negative costs for some states */
  int d = t->inst[CANONICAL_STATE].cost;
  int i;

  for (i=0; i<maxstates; i++) {
    if (t->inst[i].cost != INF_COST)
      t->inst[i].cost -= d;
    if (t->trans[i].cost != INF_COST)
      t->trans[i].cost -= d;
  }
}

static int tpa_state_equivalent(struct tpa_state *t1, struct tpa_state *t2)
{
  return (memcmp(t1->inst, t2->inst, maxstates*sizeof(struct waypoint)) == 0 &&
	  memcmp(t1->trans,t2->trans,maxstates*sizeof(struct waypoint)) == 0);
}
#endif

struct tpa_state_entry {
  struct tpa_state_entry *next;
  struct tpa_state *state;
} *tpa_state_table[TPA_SIZE];

#if !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED))
static Cell hash_tpa_state(struct tpa_state *t)
{
  int *ti = (int *)(t->inst);
  int *tt = (int *)(t->trans);
  int r=0;
  int i;

  for (i=0; ti+i < (int *)(t->inst+maxstates); i++)
    r += ti[i]+tt[i];
  return (r+(r>>14)+(r>>22)) & (TPA_SIZE-1);
}

static struct tpa_state *lookup_tpa_state(struct tpa_state *t)
{
  Cell hash = hash_tpa_state(t);
  struct tpa_state_entry *te = tpa_state_table[hash];
  struct tpa_state_entry *tn;

  if (!tpa_noequiv) {
    for (; te!=NULL; te = te->next) {
      if (tpa_state_equivalent(t, te->state)) {
	lb_newstate_equiv++;
	free(t->inst);
	free(t->trans);
	free(t);
	return te->state;
      }
    }
    tn = (struct tpa_state_entry *)malloc_l(sizeof(struct tpa_state_entry));
    tn->next = te;
    tn->state = t;
    tpa_state_table[hash] = tn;
  }
  lb_newstate_new++;
  if (tpa_trace)
    fprintf(stderr, "%ld %ld lb_states\n", lb_labeler_steps, lb_newstate_new);
  return t;
}

/* use dynamic programming to find the shortest paths within the basic
   block origs[0..ninsts-1] and rewrite the instructions pointed to by
   instps to use it */
static void optimize_rewrite(Cell *instps[], PrimNum origs[], int ninsts)
{
  int i,j;
  struct tpa_state *ts[ninsts+1];
  int nextdyn, nextstate, startstate, no_transition, no_relocatable=0;
  Address old_code_area;
  ginstps = (Label **)instps;
  DynamicInfo *di = NULL;
  Cell last=origs[ninsts-1];
  struct cost *cl=&super_costs[last];
  if (ninsts == 0)
    return;
  instps[ninsts] = instps[ninsts-1]+cl->imm_ops+1;
  /* !! better provide this info through the interface */
  lb_basic_blocks++;
  ts[ninsts] = termstate;
#ifndef NO_DYNAMIC
  if (print_sequences) {
    for (i=0; i<ninsts; i++)
#if defined(BURG_FORMAT)
      fprintf(stderr, "op%d ", super_costs[origs[i]].offset);
#else
      fprintf(stderr, "%s ", prim_names[origs[i]]);
#endif
    fprintf(stderr, "\n");
  }
#endif
  for (i=ninsts-1; i>=0; i--) {
    struct tpa_state **tp = lookup_tpa(origs[i],ts[i+1]);
    struct tpa_state *t = *tp;
    lb_labeler_steps++;
    if (t) {
      ts[i] = t;
      lb_labeler_automaton++;
    }
    else {
      lb_labeler_dynprog++;
      ts[i] = empty_tpa_state();
      for (j=1; j<=max_super && i+j<=ninsts; j++) {
	struct super_state **superp = lookup_super(origs+i, j);
	if (superp!=NULL) {
	  struct super_state *supers = *superp;
	  for (; supers!=NULL; supers = supers->next) {
	    PrimNum s = supers->super;
	    int jcost;
	    struct cost *c=super_costs+s;
	    struct waypoint *wi=&(ts[i]->inst[c->state_in]);
	    struct waypoint *wo=&(ts[i+j]->trans[c->state_out]);
	    int no_transition = wo->no_transition;
	    lb_applicable_base_rules++;
	    if (!(is_relocatable(s)) && !wo->relocatable) {
	      wo=&(ts[i+j]->inst[c->state_out]);
	      no_transition=1;
	    }
	    if (wo->cost == INF_COST) 
	      continue;
	    jcost = wo->cost + ss_cost(s);
	    if (jcost <= wi->cost) {
	      wi->cost = jcost;
	      wi->inst = s;
	      wi->relocatable = is_relocatable(s);
	      wi->no_transition = no_transition;
	      /* if (ss_greedy) wi->cost = wo->cost ? */
	    }
	  }
	}
      }
      transitions(ts[i]);
      if (!tpa_noautomaton)
        tpa_state_normalize(ts[i]);
      *tp = ts[i] = lookup_tpa_state(ts[i]);
      if (tpa_trace)
	fprintf(stderr, "%ld %ld lb_table_entries\n", lb_labeler_steps, lb_labeler_dynprog);
    }
  }
  /* now rewrite the instructions */
  inst_index=0;
  reserve_code_super(origs,ninsts);
  old_code_area = code_area;
  nextdyn=0;
  nextstate=CANONICAL_STATE;
  ip_at = ginstps[0];
  no_transition = ((!ts[0]->trans[nextstate].relocatable) 
		   ||ts[0]->trans[nextstate].no_transition);
  for (i=0; i<ninsts; i++) {
    Cell tc=0, tc2;
    inst_index=i;
    startstate = nextstate;
    if (i==nextdyn) {
      PrimNum p;
      if (!no_transition) {
	/* process trans */
	PrimNum pt = ts[i]->trans[nextstate].inst;
	struct cost *c = super_costs+pt;
	assert(ts[i]->trans[nextstate].cost != INF_COST);
	assert(c->state_in==nextstate);
	tc = compile_prim_dyn(pt);
	nextstate = c->state_out;
      }
      p = ts[i]->inst[nextstate].inst;
      {
	/* process inst */
	struct cost *c=super_costs+p;
	assert(c->state_in==nextstate);
	assert(ts[i]->inst[nextstate].cost != INF_COST);
#if defined(GFORTH_DEBUGGING)
	assert(p == origs[i]);
#endif
	tc2 = compile_prim_dyn(p);
	no_relocatable = !is_relocatable(p);
	if (no_transition || no_relocatable) {
	  /* !! actually what we care about is if and where
	   * compile_prim_dyn() puts NEXTs */
	  tc=tc2;
          startstate=nextstate;
        }
	no_transition = ts[i]->inst[nextstate].no_transition;
	nextstate = c->state_out;
	nextdyn += c->length;
	/* if the starter of a i<nextdyn sequence was non-relocatable,
	 * the rest of the sequence needs to keep up ip_at */
	if(no_relocatable)
	  ip_at = ginstps[nextdyn];
      }
      if (is_relocatable(p)) {
        di = add_dynamic_info();
        if (ndynamicinfos>1 &&
            ((UCell)(((Address)tc)-code_area)) < (UCell)code_area_size) {
          di[-1].length = ((Address)tc) - (Address)*(di[-1].tcp);
        }
        di->prim = p;
        di->seqlen = super_costs[p].length;
        di->tcp = (Label *)(instps[i]);
        di->length = code_here - (Address)tc;
        di->start_state = startstate;
        di->end_state = nextstate;
      }
    } else {
#if defined(GFORTH_DEBUGGING)
      assert(0);
#endif
      tc= (Cell)vm_prims[super2[super_costs[ts[i]->inst[CANONICAL_STATE].inst].offset]];
    }
    *(instps[i]) = tc;
  }
  assert(inst_index == i-1);
  if (!no_dynamic)
    append_ip_update(0);
  if (!no_transition) {
    PrimNum p = ts[i]->trans[nextstate].inst;
    struct cost *c = super_costs+p;
    assert(c->state_in==nextstate);
    assert(ts[i]->trans[nextstate].cost != INF_COST);
    assert(i==nextdyn);
    (void)compile_prim_dyn(p);
    nextstate = c->state_out;
    di->length = code_here - (Address)*(di->tcp);
    di->end_state = nextstate;
  }
  assert(nextstate==CANONICAL_STATE);
  assert(code_area==old_code_area); /* does reserve_code_super() work? */
}
#endif

/* compile *start, possibly rewriting it into a static and/or dynamic
   superinstruction */
void compile_prim1(Cell *start)
{
#if defined(DOUBLY_INDIRECT)
  Label prim;

  if (start==NULL)
    return;
  prim = (Label)*start;
  if (prim<((Label)(xts+DOER_MAX)) || prim>((Label)(xts+npriminfos))) {
    debugp(stderr,"compile_prim encountered xt %p [%lx]\n", prim, (((Cell)CODE_ADDRESS(prim)-(Cell)labels)));
    *start = (Cell)(((Cell)CODE_ADDRESS(prim)-(Cell)labels)+(Cell)vm_prims)+CFA_BYTE_OFFSET;
    return;
  } else {
    *start = (Cell)(prim-((Cell)xts)+((Cell)vm_prims));
    return;
  }
#elif defined(INDIRECT_THREADED)
  return;
#else /* !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED)) */
  static Cell *instps[MAX_BB+1];
  static PrimNum origs[MAX_BB+1];
  static int ninsts=0;
  PrimNum prim_num;

  if (start==NULL || ninsts >= MAX_BB ||
      (ninsts>0 && superend[origs[ninsts-1]])) {
    /* after bb, or at the start of the next bb */
    optimize_rewrite(instps,origs,ninsts);
    /* fprintf(stderr,"optimize_rewrite(...,%d)\n",ninsts); */
    ninsts=0;
    if (start==NULL) {
      align_code();
      return;
    }
  }
  prim_num = (((Xt)*start)-vm_prims)-CFA_OFFSET;
  if(prim_num >= npriminfos) {
    /* try search prim number in vm_prims */
    int step, i;
    UCell inst = (UCell)CODE_ADDRESS(*(UCell**)start);
    for(i=1; i<npriminfos; i*=2);
    i/=2;
    for(step=i/2; step>0; step/=2) {
      // debugp(stderr, "Search at label[%x] for %p=%p\n", i, inst, vm_prims[i]);
      if((i < npriminfos) && (inst == (UCell)(vm_prims[i]))) {
	prim_num = i;
	break;
      }
      i += ((i < npriminfos) && (inst > (UCell)(vm_prims[i]))) ? step : -step;
    }
    if(inst == (UCell)(vm_prims[i]))
      prim_num = i;
  }
  // debugp(stderr, "Prim %d %p[%p] compiled\n", prim_num, *start, **(Cell**)start);
  if(prim_num >= npriminfos) {
    /* code word */
    optimize_rewrite(instps,origs,ninsts);
    // debugp(stderr,"optimize_rewrite(...,%d)\n",ninsts);
    ninsts=0;
    append_jump();
    *start = (Cell)CODE_ADDRESS(*start);
    return;
  }    
  assert(ninsts<MAX_BB);
  instps[ninsts] = start;
  origs[ninsts] = prim_num;
  ninsts++;
#endif /* !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED)) */
}

#ifdef HAVE_LIBLTDL
static int gforth_ltdlinited=0;
#endif

int gforth_init()
{
#if 0 && defined(__i386)
  /* disabled because the drawbacks may be worse than the benefits */
  /* set 387 precision control to use 53-bit mantissae to avoid most
     cases of double rounding */
  short fpu_control = 0x027f ;
  asm("fldcw %0" : : "m"(fpu_control));
#endif /* defined(__i386) */

  jit_map_normal();
#ifdef MACOSX_DEPLOYMENT_TARGET
  setenv("MACOSX_DEPLOYMENT_TARGET", MACOSX_DEPLOYMENT_TARGET, 0);
#endif
#ifdef LTDL_LIBRARY_PATH
  setenv("LTDL_LIBRARY_PATH", LTDL_LIBRARY_PATH, 0);
#endif
#ifndef STANDALONE
  /* buffering of the user output device */
#ifdef _IONBF
  if (isatty(fileno(stdout))) {
    fflush(stdout);
    setvbuf(stdout,NULL,_IONBF,0);
  }
#endif
  setlocale(LC_ALL, "");
  setlocale(LC_NUMERIC, "C");
#else
  prep_terminal();
#endif

#ifndef STANDALONE
#ifdef HAVE_LIBLTDL
  if (lt_dlinit()!=0) {
    fprintf(stderr,"%s: lt_dlinit failed", progname);
    return 1;
  }
  gforth_ltdlinited=1;
#endif
#ifdef HAS_OS
#ifndef NO_DYNAMIC
  init_ss_cost();
#endif /* !defined(NO_DYNAMIC) */
#endif /* defined(HAS_OS) */
#endif
  code_here = ((void *)1)+code_area_size;

  get_winsize();
   
  install_signal_handlers(); /* right place? */

  return 0;
}

/* pointer to last '/' or '\' in file, 0 if there is none. */
static char *onlypath(char *filename)
{
  return strrchr(filename, DIRSEP);
}

static FILE *openimage(char *fullfilename)
{
  FILE *image_file;
  char * expfilename = tilde_cstr((Char *)fullfilename, strlen(fullfilename));

  image_file=fopen(expfilename,"rb");
  if (image_file!=NULL && debug)
    fprintf(stderr, "Opened image file: %s\n", expfilename);
  free(expfilename);
  return image_file;
}

/* try to open image file concat(path[0:len],imagename) */

/* global variables from checkimage */

Char magic[8];

static FILE *checkimage(char *path, int len, char *imagename)
{
  int dirlen=len;
  char fullfilename[dirlen+strlen((char *)imagename)+2];
  FILE* imagefile;
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

  memmove(fullfilename, path, dirlen);
  if (dirlen && fullfilename[dirlen-1]!=DIRSEP)
    fullfilename[dirlen++]=DIRSEP;
  strcpy(fullfilename+dirlen,imagename);
  imagefile=openimage(fullfilename);

  if(!imagefile) return 0;

  preamblesize=0;
  do {
    if(fread(magic,sizeof(Char),8,imagefile) < 8) {
      fprintf(stderr,"%s: image %s doesn't seem to be a Gforth (>=0.8) image.\n",
	      progname, imagename);
      return NULL;
    }
    preamblesize+=8;
  } while(memcmp(magic,"Gforth6",7));
  if (debug) {
    fprintf(stderr,"Magic found: %*s ", 6, magic);
    print_sizes(magic[7]);
  }
  if (magic[7] != sizebyte) {
    if (debug) {
      fprintf(stderr,"This image is:         ");
      print_sizes(magic[7]);
      fprintf(stderr,"whereas the machine is ");
      print_sizes(sizebyte);
    };
    fclose(imagefile);
    imagefile = 0;
  }

  return imagefile;
}

static FILE * open_image_file(char * imagename, char * path)
{
  FILE * image_file=NULL;
  char *origpath=path;
  
  if(strchr(imagename, DIRSEP)==NULL) {
    /* first check the directory where the exe file is in !! 01may97jaw */
    if (onlypath(progname))
      image_file=checkimage(progname, onlypath(progname)-progname, imagename);
    if (!image_file)
      do {
	char *pend=strchr(path, PATHSEP);
	if (pend==NULL)
	  pend=path+strlen(path);
	if (strlen(path)==0) break;
	image_file=checkimage(path, pend-path, imagename);
	path=pend+(*pend==PATHSEP);
      } while (image_file==NULL);
    path=origpath;
  } else {
    path="";
    image_file=checkimage(path, 0, imagename);
  }

  if (!image_file) {
    fprintf(stderr,"%s: cannot open image file %s in path %s for reading\n",
	    progname, imagename, path);
    return NULL;
  }

  return image_file;
}

#ifdef STANDALONE
ImageHeader* gforth_loader(char* imagename, char* path)
{
  if(gforth_init()) return NULL;
  return gforth_engine(0 sr_call);
}
#else
ImageHeader* gforth_loader(char* imagename, char* path)
/* returns the address of the image proper (after the preamble) */
{
  ImageHeader header;
  SectionHeader section;
  Address sections[0x100];  /* base address of all sections */
  Char* reloc_bits[0x100];  /* reloc bits of all images     */
  UCell sizes[0x100];       /* section sizes */
  Cell bases[0x100];        /* section bases */
  Address image;
  Address imp; /* image+preamble */
  Cell data_offset = offset_image ? 56*sizeof(Cell) : 0;
  UCell check_sum;
  FILE* imagefile=open_image_file(imagename, path);

  if(imagefile == NULL) return NULL;
  if(gforth_init()) return NULL;

  if(sizeof(ImageHeader)!=fread((void *)&header, 1, sizeof(ImageHeader), imagefile)) {
    fprintf(stderr, "ImageHeader read failed\n");
  }

  set_stack_sizes(&header);
  memset(sections, 0, sizeof(sections));
  memset(reloc_bits, 0, sizeof(reloc_bits));
  
#if HAVE_GETPAGESIZE
  pagesize=getpagesize(); /* Linux/GNU libc offers this */
#elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
  pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
#elif PAGESIZE
  pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
#endif
  debugp(stderr,"pagesize=%ld\n",(unsigned long) pagesize);

  sizes[0]=header.image_dp-header.base;
  bases[0]=(Cell)header.base;

  image = dict_alloc_read(imagefile, preamblesize+sizes[0],
			  dictsize+preamblesize, data_offset);
  if(image==NULL) return NULL;

  vm_prims = gforth_engine(0 sr_call);
  check_prims(vm_prims);
  prepare_super_table();
  ip_update_metrics_base = opt_ip_updates_branch*min_ip_update;
#ifndef DOUBLY_INDIRECT
#ifdef PRINT_SUPER_LENGTHS
  print_super_lengths();
#endif
  check_sum = checksum(vm_prims);
#else /* defined(DOUBLY_INDIRECT) */
  check_sum = (UCell)vm_prims;
#endif /* defined(DOUBLY_INDIRECT) */
#if !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED))
  termstate = make_termstate();
#endif /* !(defined(DOUBLY_INDIRECT) || defined(INDIRECT_THREADED)) */
  
  sections[0]=imp=image+preamblesize;

  set_stack_sizes((ImageHeader*)imp);

  if (clear_dictionary)
    memset(imp+sizes[0], 0, dictsize-sizes[0]-preamblesize);
  
  fseek(imagefile, preamblesize+sizes[0], SEEK_SET);
    
  int i;
  for(i=0; i<0xFE; ) {
    Cell reloc_size=((sizes[i]-1)/sizeof(Cell))/8+1;
    if(bases[i]==0 || bases[i] == 0x100) {
      reloc_bits[i]=malloc(reloc_size);
      
      if(reloc_size != fread(reloc_bits[i], 1, reloc_size, imagefile)) {
	fprintf(stderr, "Image reloc bits read terminated early\n");
	break;
      }
    } else if(bases[i]!=(Cell)sections[i]) {
      fprintf(stderr,"%s: Cannot load nonrelocatable image (compiled for address %p) at address %p\n",
	      progname, (Address)bases[i], sections[i]);
      return NULL;
    }
    
    if(8 != fread(magic, 1, 8, imagefile)) break;
    
    if(memcmp(magic, "Section.", 8)) break;
    
    if(sizeof(SectionHeader) !=
       fread(&section, 1, sizeof(SectionHeader), imagefile)) break;
    
    i++;
    
    bases[i] = INSECTION(section.base);
    sizes[i] = section.dp-section.base;
    sections[i] = alloc_mmap_guard(section.size);
    fseek(imagefile, -sizeof(SectionHeader), SEEK_CUR);
    debugp(stderr, "section base=%p, dp=%p, size=%lx\n", section.base, section.dp, section.size);
    if(fread(sections[i], 1, sizes[i], imagefile) != sizes[i]) break;
  }
#ifndef NO_DYNAMIC
  reserve_code_space(0);
#endif
  int no_dynamic_orig = no_dynamic;
  no_dynamic |= no_dynamic_image;
  gforth_relocate(sections, reloc_bits, sizes, bases);
  no_dynamic = no_dynamic_orig;
#if 0
  { /* let's see what the relocator did */
    FILE *snapshot=fopen("snapshot.fi","wb");
    fwrite(image,1,imagesize,snapshot);
    fclose(snapshot);
  }
#endif
  if (header.checksum==0)
    ((ImageHeader *)imp)->checksum=check_sum;
  else if (header.checksum != check_sum) {
    fprintf(stderr,"%s: Checksum of image ($%lx) does not match the executable ($%lx)\n",
	    progname, header.checksum, check_sum);
    return NULL;
  }
#ifdef DOUBLY_INDIRECT
  ((ImageHeader *)imp)->xt_base = xts;
  ((ImageHeader *)imp)->label_base = labels;
#endif
  fclose(imagefile);

  for(i=0; i<0x100; i++) {
    if(reloc_bits[i]!=NULL)
      free(reloc_bits[i]);
  }
  /* unnecessary, except maybe for CODE words */
  /* FLUSH_ICACHE(imp, header.image_size);*/

  return (ImageHeader*)imp;
}
#endif
#endif

#ifdef STANDALONE_ALLOC
Address gforth_alloc(Cell size)
{
  Address r;
  /* leave a little room (64B) for stack underflows */
  if ((r = malloc_l(size+64))==NULL) {
    perror(progname);
    return NULL;
  }
  r = (Address)((((Cell)r)+(sizeof(Float)-1))&(-sizeof(Float)));
  debugp(stderr, "malloc($%lx) succeeds, address=%p\n", (long)size, r);
  return r;
}
#endif

#ifdef HAS_OS
static UCell convsize(char *s, UCell elemsize)
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
      return -1;
#endif
    } else if (strcmp(endp,"e")!=0 && strcmp(endp,"")!=0) {
      fprintf(stderr,"%s: cannot grok size specification %s: invalid unit \"%s\"\n", progname, s, endp);
      return -1;
    }
  }
  return n*m;
}

enum {
  ss_number = 256,
  ss_states,
  ss_min_codesize,
  ss_min_ls,
  ss_min_lsu,
  ss_min_nexts,
  opt_code_block_size,
  opt_opt_ip_updates,
  die_on_signal_param
};

static void print_diag()
{

#if !defined(HAVE_GETRUSAGE) || (!defined(HAS_ATOMIC) && !defined(HAS_SYNC))
  printf("*** missing functionality ***\n"
#ifndef HAVE_GETRUSAGE
	 "    no getrusage -> CPUTIME broken\n"
#endif
#if !defined(HAS_ATOMIC) && !defined(HAS_SYNC)
	 "    no atomic operations -> !@ and co. broken\n"
#endif
	  );
#endif
  if((relocs < nonrelocs) ||
#if defined(BUGGY_LL_CMP) || defined(BUGGY_LL_MUL) || defined(BUGGY_LL_DIV) || defined(BUGGY_LL_ADD) || defined(BUGGY_LL_SHIFT) || defined(BUGGY_LL_D2F) || defined(BUGGY_LL_F2D)
     1
#else
     0
#endif
     )
    debugp(stderr, "relocs: %d:%d\n", relocs, nonrelocs);
  printf("*** %sperformance problems ***\n%s%s",
#if defined(BUGGY_LL_CMP) || defined(BUGGY_LL_MUL) || defined(BUGGY_LL_DIV) || defined(BUGGY_LL_ADD) || defined(BUGGY_LL_SHIFT) || defined(BUGGY_LL_D2F) || defined(BUGGY_LL_F2D) || !(defined(FORCE_REG) || defined(FORCE_REG_UNNECESSARY)) || defined(BUGGY_LONG_LONG) || (NO_DYNAMIC_DEFAULT)
	    "",
#else
	    "no ",
#endif
#if (NO_DYNAMIC_DEFAULT)
	    "    no dynamic code generation by default\n"
#endif
#if defined(BUGGY_LL_CMP) || defined(BUGGY_LL_MUL) || defined(BUGGY_LL_DIV) || defined(BUGGY_LL_ADD) || defined(BUGGY_LL_SHIFT) || defined(BUGGY_LL_D2F) || defined(BUGGY_LL_F2D)
	    "    double-cell integer type buggy ->\n        "
#ifdef BUGGY_LL_CMP
	    "double comparisons, "
#endif
#ifdef BUGGY_LL_MUL
	    "*/MOD */ M* UM* "
#endif
#ifdef BUGGY_LL_DIV
	    /* currently nothing is affected */
#endif
#ifdef BUGGY_LL_ADD
	    "M+ D+ D- DNEGATE "
#endif
#ifdef BUGGY_LL_SHIFT
	    "D2/ "
#endif
#ifdef BUGGY_LL_D2F
	    "D>F "
#endif
#ifdef BUGGY_LL_F2D
	    "F>D "
#endif
	    "\b\b slow\n"
#endif
#if !(defined(FORCE_REG) || defined(FORCE_REG_UNNECESSARY))
	    "    automatic register allocation: performance degradation possible\n"
#endif
	    "",
	    (relocs < nonrelocs) ? "no dynamic code generation (--debug for details) -> factor 2 slowdown\n" : "");
}

#ifdef STANDALONE
int gforth_args(int argc, char ** argv, char ** path, char ** imagename)
{
#ifdef HAS_OS
  *path = getenv("GFORTHPATH") ? : DEFAULTPATH;
#else
  *path = DEFAULTPATH;
#endif
  *imagename="gforth.fi";
  return 0;
}
#else
int gforth_args(int argc, char ** argv, char ** path, char ** imagename)
{
  int c;
#ifdef HAS_OS
  *path = getenv("GFORTHPATH") ? : DEFAULTPATH;
#else
  *path = DEFAULTPATH;
#endif
  *imagename="gforth.fi";
  progname = argv[0];

  opterr=0;
  while (1) {
    int option_index=0, oldoptind=optind;
    static struct option opts[] = {
      {"appl-image", required_argument, NULL, 'a'},
      {"image-file", required_argument, NULL, 'i'},
      {"dictionary-size", required_argument, NULL, 'm'},
      {"data-stack-size", required_argument, NULL, 'd'},
      {"return-stack-size", required_argument, NULL, 'r'},
      {"fp-stack-size", required_argument, NULL, 'f'},
      {"locals-stack-size", required_argument, NULL, 'l'},
      {"vm-commit", no_argument, &map_noreserve, 0},
      {"map-32bit", no_argument, &map_32bit, 1},
      {"path", required_argument, NULL, 'p'},
      {"version", no_argument, NULL, 'v'},
      {"help", no_argument, NULL, 'h'},
      /* put something != 0 into offset_image */
      {"offset-image", no_argument, &offset_image, 1},
      {"no-offset-im", no_argument, &offset_image, 0},
      {"clear-dictionary", no_argument, &clear_dictionary, 1},
      {"debug", no_argument, &debug, 1},
      {"debug-mcheck", no_argument, &debug_mcheck, 1},
      {"no-debug-prim", no_argument, &debug_prim, 0},
      {"diag", no_argument, NULL, 'D'},
      {"die-on-signal", optional_argument, NULL, die_on_signal_param},
      {"ignore-async-signals", no_argument, &ignore_async_signals, 1},
      {"no-super", no_argument, &no_super, 1},
      {"no-dynamic", no_argument, &no_dynamic, 1},
      {"no-dynamic-image", no_argument, &no_dynamic_image, 1},
      {"no-0rc", no_argument, &no_rc0, 1},
      {"dynamic", no_argument, &no_dynamic, 0},
      {"code-block-size", required_argument, NULL, opt_code_block_size},
      {"opt-ip-updates", required_argument, NULL, opt_opt_ip_updates},
      {"print-metrics", no_argument, &print_metrics, 1},
      {"print-nonreloc", no_argument, &print_nonreloc, 1},
      {"print-prims", no_argument, &print_prims, 1},
      {"print-sequences", no_argument, &print_sequences, 1},
      {"ss-number", required_argument, NULL, ss_number},
      {"ss-states", required_argument, NULL, ss_states},
#ifndef NO_DYNAMIC
      {"ss-min-codesize", no_argument, NULL, ss_min_codesize},
#endif
      {"ss-min-ls",       no_argument, NULL, ss_min_ls},
      {"ss-min-lsu",      no_argument, NULL, ss_min_lsu},
      {"ss-min-nexts",    no_argument, NULL, ss_min_nexts},
      {"ss-greedy",       no_argument, &ss_greedy, 1},
      {"tpa-noequiv",     no_argument, &tpa_noequiv, 1},
      {"tpa-noautomaton", no_argument, &tpa_noautomaton, 1},
      {"tpa-trace",	  no_argument, &tpa_trace, 1},
      {0,0,0,0}
      /* no-init-file, no-rc? */
    };
    
    c = getopt_long(argc, argv, "+i:m:d:r:f:l:p:vhoncs::xD", opts, &option_index);
    
    switch (c) {
    case EOF: return 0;
    case '?': optind=oldoptind; return 0;
    case 'a': *imagename = optarg; return 0;
    case 'i': *imagename = optarg; break;
    case 'm': if((dictsize = convsize(optarg,sizeof(Cell)))==-1L) return 1; break;
    case 'd': if((dsize = convsize(optarg,sizeof(Cell)))==-1L) return 1; break;
    case 'r': if((rsize = convsize(optarg,sizeof(Cell)))==-1L) return 1; break;
    case 'f': if((fsize = convsize(optarg,sizeof(Float)))==-1L) return 1; break;
    case 'l': if((lsize = convsize(optarg,sizeof(Cell)))==-1L) return 1; break;
    case 'p': *path = optarg; break;
    case 'o': offset_image = 1; break;
    case 'n': offset_image = 0; break;
    case 'c': clear_dictionary = 1; break;
    case 's':
    case die_on_signal_param:
      if(!optarg && NULL != argv[optind] && '-' != argv[optind][0]) {
	const char *tmp_optarg = argv[optind];
	char * endptr;
	Cell number = strtol(tmp_optarg, &endptr, 10);
	if(*endptr == '\0') {
	  optind++;
	  die_on_signal = number;
	} else {
	  die_on_signal = 1;
	}
      } else {
	die_on_signal = (optarg == NULL) ? 1 : atoi(optarg+(optarg[0]=='='));
      }
      break;
    case 'x': debug = 1; break;
    case 'D': print_diag(); break;
    case 'v': fputs(PACKAGE_STRING" "ARCH"\n", stderr); exit(0);
    case opt_code_block_size: if((code_area_size = convsize(optarg,sizeof(Char)))==-1L) return 1; break;
    case opt_opt_ip_updates:
      opt_ip_updates = atoi(optarg);
      opt_ip_updates_branch = opt_ip_updates>>3;
      opt_ip_updates &= 7;
      break;
    case ss_number: static_super_number = atoi(optarg); break;
    case ss_states: maxstates = max(min(atoi(optarg),MAX_STATE),1); break;
#ifndef NO_DYNAMIC
    case ss_min_codesize: ss_cost = cost_codesize; break;
#endif
    case ss_min_ls:       ss_cost = cost_ls;       break;
    case ss_min_lsu:      ss_cost = cost_lsu;      break;
    case ss_min_nexts:    ss_cost = cost_nexts;    break;
    case 'h': 
      fprintf(stderr, "Usage: %s [engine options] ['--'] [image arguments]\n\
Engine Options:\n\
  --appl-image FILE		    Equivalent to '--image-file=FILE --'\n\
  --clear-dictionary		    Initialize the dictionary with 0 bytes\n\
  --code-block-size=SIZE            size of native code blocks [512KB]\n\
  -d SIZE, --data-stack-size=SIZE   Specify data stack size\n\
  --debug			    Print debugging information during startup\n"
#ifdef HAVE_MCHECK
"  --debug-mcheck		    Diagnostics for malloc/free (thread unsafe)\n"
#endif
"  -D, --diag			    Print diagnostic information during startup\n\
  --die-on-signal		    Exit instead of THROWing some signals\n\
  --dynamic			    Use dynamic native code\n\
  -f SIZE, --fp-stack-size=SIZE	    Specify floating point stack size\n\
  -h, --help			    Print this message and exit\n\
  --ignore-async-signals	    Ignore instead of THROWing async. signals\n\
  -i FILE, --image-file=FILE	    Use image FILE instead of `gforth.fi'\n\
  -l SIZE, --locals-stack-size=SIZE Specify locals stack size\n\
  -m SIZE, --dictionary-size=SIZE   Specify Forth dictionary size\n\
  --map-32bit			    Try to put the dictionary in the first 2GB\n\
  --no-dynamic			    Use only statically compiled primitives\n\
  --no-dynamic-image		    Use statically compiled primitives in image\n\
  --no-offset-im		    Load image at normal position\n\
  --no-super			    No dynamically formed superinstructions\n\
  --no-0rc			    do not load ~/.config/gforthrc0\n\
  --offset-image		    Load image at a different position\n\
  --opt-ip-updates=n                ip-update optimization (0=disabled)\n\
  -p PATH, --path=PATH		    Search path for finding image and sources\n\
  --print-metrics		    Print some code generation metrics on exit\n\
  --print-nonreloc		    Print non-relocatable primitives at start\n\
  --print-prims			    Print primitives with usage counts on exit\n\
  --print-sequences		    Print primitive sequences for optimization\n\
  -r SIZE, --return-stack-size=SIZE Specify return stack size\n\
  --ss-greedy			    Greedy, not optimal superinst selection\n\
  --ss-min-codesize		    Select superinsts for smallest native code\n\
  --ss-min-ls			    Minimize loads and stores\n\
  --ss-min-lsu			    Minimize loads, stores, and pointer updates\n\
  --ss-min-nexts		    Minimize the number of static superinsts\n\
  --ss-number=N			    Use N static superinsts (default max)\n\
  --ss-states=N			    N states for stack caching (default max)\n\
  --tpa-noequiv			    Automaton without state equivalence\n\
  --tpa-noautomaton		    Dynamic programming only\n\
  --tpa-trace			    Report new states etc.\n\
  -v, --version			    Print engine version and exit\n\
  --vm-commit			    Use OS default for memory overcommit\n\
SIZE arguments consist of an integer followed by a unit. The unit can be\n\
  `b' (byte), `e' (element; default), `k' (KB), `M' (MB), `G' (GB) or `T' (TB).\n",
	      argv[0]);
      optind=oldoptind;
      return 0;
    }
  }
  return 0;
}
#endif
#endif

#ifdef STANDALONE
Cell data_abort_pc;

void data_abort_C(void)
{
  while(1) {
  }
}
#endif

Cell const * gforth_pointers(Cell n)
{
  switch(n) {
  case 0: return (Cell *)&gforth_SPs; // per thread pointer structure
  case 1: return (Cell *)&gforth_engine;
#ifdef HAS_FILE
  case 2: return (Cell *)&cstr;
  case 3: return (Cell *)&tilde_cstr;
#endif
  case 4: return (Cell *)&gforth_stacks;
  case 5: return (Cell *)&gforth_free_stacks;
  case 6: return (Cell *)&gforth_main_UP;
  case 7: return (Cell *)&gforth_go;
  case 8: return (Cell *)&gforth_sigset;
  case 9: return (Cell *)&gforth_setstacks;
  default: return NULL;
  }
}

void gforth_printmetrics()
{
  if (print_metrics) {
    long i;
    fprintf(stderr, "%8ld native code bytes generated (including deleted code) without padding\n", generated_bytes - generated_nop_bytes);
    fprintf(stderr, "%8ld native code bytes generated (including deleted code)\n", generated_bytes);
    fprintf(stderr, "code size = %8ld\n", dyncodesize());
#ifndef STANDALONE
    for (i=0; i<sizeof(cost_sums)/sizeof(cost_sums[0]); i++)
      fprintf(stderr, "metric %8s: %8ld\n",
	      cost_sums[i].metricname, cost_sums[i].sum);
#endif
    fprintf(stderr,"lb_basic_blocks = %ld\n", lb_basic_blocks);
    fprintf(stderr,"lb_labeler_steps = %ld\n", lb_labeler_steps);
    fprintf(stderr,"lb_labeler_automaton = %ld\n", lb_labeler_automaton);
    fprintf(stderr,"lb_labeler_dynprog = %ld\n", lb_labeler_dynprog);
    fprintf(stderr,"lb_newstate_equiv = %ld\n", lb_newstate_equiv);
    fprintf(stderr,"lb_newstate_new = %ld\n", lb_newstate_new);
    fprintf(stderr,"lb_applicable_base_rules = %ld\n", lb_applicable_base_rules);
    fprintf(stderr,"lb_applicable_chain_rules = %ld\n", lb_applicable_chain_rules);
    if (ip_update_metrics==NULL)
      fprintf(stderr, "no ip update metrics recorded\n");
    else
      for (i=0; i<nip_update_metrics; i++)
        if (ip_update_metrics[i] > 0)
          fprintf(stderr,"%5ld ip update by %2ld cells\n",
                  ip_update_metrics[i],i+ip_update_metrics_base);
  }
  if (tpa_trace) {
    fprintf(stderr, "%ld %ld lb_states\n", lb_labeler_steps, lb_newstate_new);
    fprintf(stderr, "%ld %ld lb_table_entries\n", lb_labeler_steps, lb_labeler_dynprog);
  }
}

void gforth_cleanup()
{
#if defined(SIGPIPE) && !defined(STANDALONE)
  bsd_signal(SIGPIPE, SIG_IGN);
#endif
#ifdef VM_PROFILING
  vm_print_profile(stderr);
#endif
  deprep_terminal();
#ifndef STANDALONE
#ifdef HAVE_LIBLTDL
  if (gforth_ltdlinited)
    if (lt_dlexit()!=0)
      fprintf(stderr,"%s: lt_dlexit failed", progname);
#endif
#endif
}

user_area* gforth_stacks(Cell dsize, Cell fsize, Cell rsize, Cell lsize)
{
  size_t totalsize;
  Cell a;
  user_area * up0;
  Cell dsizep = wholepage(dsize);
  Cell rsizep = wholepage(rsize);
  Cell fsizep = wholepage(fsize);
  Cell lsizep = wholepage(lsize);
  totalsize = dsizep+fsizep+rsizep+lsizep+6*pagesize;
#ifdef SIGSTKSZ
  totalsize += 2*SIGSTKSZ;
#endif
#ifdef HAVE_MMAP
#ifdef GFORTH_DEBUGGING
  /* make sure the stack bottom is page-aligned for stack underflow detection*/
  dsize = dsizep;
  rsize = rsizep;
  fsize = fsizep;
  lsize = lsizep;
#endif
  a = (Cell)alloc_mmap(totalsize);
  if (a != (Cell)MAP_FAILED) {
    up0=(user_area*)a; a+=pagesize;
    page_noaccess((void*)a); a+=pagesize; up0->sp0=(Cell*)(a+dsize); a+=dsizep;
    page_noaccess((void*)a); a+=pagesize; up0->rp0=(Cell*)(a+rsize); a+=rsizep;
    page_noaccess((void*)a); a+=pagesize; up0->fp0=(Float*)(a+fsize); a+=fsizep;
    page_noaccess((void*)a); a+=pagesize; up0->lp0=(Address)(a+lsize); a+=lsizep;
    page_noaccess((void*)a); a+=pagesize;
  /* ensure that the cached elements (if any) are accessible */
#if !(defined(GFORTH_DEBUGGING) || defined(INDIRECT_THREADED) || defined(DOUBLY_INDIRECT) || defined(VM_PROFILING))
    up0->sp0 -= 8; /* make stuff below bottom accessible for stack caching */
    up0->fp0--;
#else
# ifdef DEBUG
    up0->sp0--; // debug will print TOS even if the stack is empty
# endif
#endif
    return up0;
  }
#endif
  a = (Cell)verbose_malloc(totalsize);
  if (a) {
    up0=(user_area*)a; a+=pagesize;
    a+=pagesize; up0->sp0=(Cell *)(a+dsize); a+=dsizep;
    a+=pagesize; up0->rp0=(Cell *)(a+rsize); a+=rsizep;
    a+=pagesize; up0->fp0=(Float *)(a+fsize); a+=fsizep;
    a+=pagesize; up0->lp0=(Address)(a+lsize); a+=lsizep;
    return up0;
  }
  return 0;
}

static inline void gforth_set_sigaltstack(user_area * t)
{
#ifdef SIGSTKSZ
  stack_t sigstack;
  int sas_retval=-1;
#endif
  Cell a=wholepage((size_t)(t->lp0));
  a+=pagesize;
#ifdef SIGSTKSZ
  sigstack.ss_sp=(void*)a+SIGSTKSZ;
  sigstack.ss_size=SIGSTKSZ;
  sigstack.ss_flags=0;
  sas_retval=sigaltstack(&sigstack,(stack_t *)0);
#if defined(HAS_FILE) || !defined(STANDALONE)
  if(sas_retval)
    debugp(stderr,"sigaltstack: %s\n",strerror(errno));
#endif
#endif
}

void gforth_free_stacks(user_area * t)
{
  int r;
#if HAVE_GETPAGESIZE
  Cell pagesize=getpagesize(); /* Linux/GNU libc offers this */
#elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
  Cell pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
#elif PAGESIZE
  Cell pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
#endif
  Cell size = wholepage((Cell)((t)->lp0)+pagesize-(Cell)t);
#ifdef SIGSTKSZ
  size += 2*SIGSTKSZ;
#endif
  debugp(stderr,"try munmap(%p, %lx); ", t, size);
  r=munmap(t, size);
  if(r)
    fprintf(stderr,"munmap(%p, %lx) failed: %s\n", t, size, strerror(errno));
  else
    debugp(stderr,"sucess\n");
}

void gforth_setstacks(user_area * t)
{
  gforth_magic = GFORTH_MAGIC; /* mark task as maintained */
  t->next_task = 0; /* mark user area as need-to-be-set */

  gforth_SP = t->sp0;
  gforth_RP = t->rp0;
  gforth_FP = t->fp0;
  gforth_LP = t->lp0;

  gforth_SPs.handler = 0;
  gforth_SPs.first_throw = ~0;
  gforth_SPs.wraphandler = 0;

  gforth_set_sigaltstack(t);
}

Cell gforth_boot(int argc, char** argv, char* path)
{
  char *path2=malloc_l(strlen(path)+1);
  char *p1, *p2;
  
  argv[optind-1] = progname;
  
  /* make path OS-independent by replacing path separators with NUL */
  for (p1=path, p2=path2; *p1!='\0'; p1++, p2++)
    if (*p1==PATHSEP)
      *p2 = '\0';
    else
      *p2 = *p1;
  *p2='\0';
  
  *--gforth_SP=(Cell)path2;
  *--gforth_SP=(Cell)strlen(path);
  *--gforth_SP=(Cell)(argv+(optind-1));
  *--gforth_SP=(Cell)(argc-(optind-1));
  
  debugp(stderr, "Booting Gforth: %p\n", gforth_header->boot_entry);
  return gforth_go(gforth_header->boot_entry);
}

Cell gforth_quit()
{
  debugp(stderr, "Quit into Gforth: %p\n", gforth_header->quit_entry);
  return gforth_go(gforth_header->quit_entry);
}

Cell gforth_execute(Xt xt)
{
  debugp(stderr, "Execute Gforth xt %p: %p\n", xt, gforth_header->execute_entry);

  *--gforth_SP = (Cell)xt;

  return gforth_go(gforth_header->execute_entry);
}

Xt gforth_find(Char * name)
{
  Xt xt;
  debugp(stderr, "Find '%s' in Gforth: %p\n", name, gforth_header->find_entry);

  *--gforth_SP = (Cell)name;
  *--gforth_SP = strlen((char*)name);

  xt = (Xt)gforth_go(gforth_header->find_entry);
  debugp(stderr, "Found %p\n", xt);
  return xt;
}

Cell winch_addr=0;

Cell gforth_bootmessage()
{
  Cell retval = -13;
  Xt bootmessage=gforth_find((Char*)"bootmessage");
  if(bootmessage != 0) {
    retval = gforth_execute(bootmessage);
  } else {
    debugp(stderr, "Can't print bootmessage\n");
  }
  return retval;
}

Cell gforth_start(int argc, char ** argv)
{
  char *path, *imagename;

#ifdef HAVE_MCHECK
  mcheck_init(debug_mcheck);
#endif
  if(gforth_args(argc, argv, &path, &imagename))
    return -24; /* Invalid numeric argument */
  if(no_rc0)
    setenv("GFORTH_ENV", "off", 1);
  gforth_header = gforth_loader(imagename, path);
  if(gforth_header==NULL)
    return -59; /* allocate error */
  gforth_main_UP = gforth_UP = gforth_stacks(dsize, fsize, rsize, lsize);
  gforth_setstacks(gforth_UP);
  return gforth_boot(argc, argv, path);
}

Cell gforth_main(int argc, char **argv, char **env)
{
  Cell retvalue=gforth_start(argc, argv);
  debugp(stderr, "Start returned %ld\n", retvalue);

  if(retvalue == -56) { /* throw-code for quit */
    retvalue = gforth_bootmessage();
    if(retvalue == -56)
      retvalue = gforth_quit();
  }
  gforth_cleanup();
  gforth_printmetrics();
#ifndef NO_DYNAMIC
  if (print_prims)
    gforth_printprims();
#endif
  // gforth_free_dict();

  return retvalue;
}

Cell gforth_make_image(int debugflag)
{
  char *argv0[] = { "gforth", "--clear-dictionary", "--no-offset-im", "--die-on-signal", "-i", GKERNEL, "exboot.fs", "startup.fs", "arch/" ARCH "/asm.fs", "arch/" ARCH "/disasm.fs", "-e", "savesystem temp-file.fi1 bye" };
  char *argv1[] = { "gforth", "--clear-dictionary", "--offset-image", "--die-on-signal", "-i", GKERNEL, "exboot.fs", "startup.fs", "arch/" ARCH "/asm.fs", "arch/" ARCH "/disasm.fs", "-e", "savesystem temp-file.fi2 bye" };
  char *argv2[] = { "gforth", "--die-on-signal", "-i", GKERNEL, "exboot.fs", "startup.fs", "comp-i.fs", "-e", "comp-image temp-file.fi1 temp-file.fi2 gforth.fi bye" };
  const int argc0 = sizeof(argv0)/sizeof(char*);
  const int argc1 = sizeof(argv1)/sizeof(char*);
  const int argc2 = sizeof(argv2)/sizeof(char*);

  Cell retvalue;

  debug=debugflag;

  retvalue=gforth_start(argc0, argv0);
  gforth_free_stacks(gforth_UP);
  gforth_free_dict();

  optind=1;

  retvalue=gforth_start(argc1, argv1);
  gforth_free_stacks(gforth_UP);
  gforth_free_dict();

  optind=1;

  retvalue=gforth_start(argc2, argv2);
  gforth_free_stacks(gforth_UP);
  gforth_free_dict();
  
  unlink("temp-file.fi1");
  unlink("temp-file.fi2");

  return retvalue;
}
