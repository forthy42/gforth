/* Gforth virtual machine (aka inner interpreter)

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

undefine(`symbols')

#include "config.h"
#include "forth.h"
#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <assert.h>
#include <stdlib.h>
#include <errno.h>
#include "io.h"
#include "threaded.h"
#ifndef STANDALONE
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#include <sys/time.h>
#include <unistd.h>
#include <pwd.h>
#include <dirent.h>
#include <sys/resource.h>
#ifdef HAVE_FNMATCH_H
#include <fnmatch.h>
#else
#include "fnmatch.h"
#endif
#else
#include "systypes.h"
#endif

#if defined(HAVE_LIBDL) || defined(HAVE_DLOPEN) /* what else? */
#include <dlfcn.h>
#endif
#if defined(_WIN32)
#include <windows.h>
#endif
#ifdef hpux
#include <dl.h>
#endif

#ifndef SEEK_SET
/* should be defined in stdio.h, but some systems don't have it */
#define SEEK_SET 0
#endif

#define IOR(flag)	((flag)? -512-errno : 0)

struct F83Name {
  struct F83Name *next;  /* the link field for old hands */
  char		countetc;
  char		name[0];
};

#define F83NAME_COUNT(np)	((np)->countetc & 0x1f)

struct Longname {
  struct Longname *next;  /* the link field for old hands */
  Cell		countetc;
  char		name[0];
};

#define LONGNAME_COUNT(np)	((np)->countetc & (((~((UCell)0))<<3)>>3))

Cell *SP;
Float *FP;
Address UP=NULL;

#if 0
/* not used currently */
int emitcounter;
#endif
#define NULLC '\0'

#ifdef MEMCMP_AS_SUBROUTINE
extern int gforth_memcmp(const char * s1, const char * s2, size_t n);
#define memcmp(s1,s2,n) gforth_memcmp(s1,s2,n)
#endif

#ifdef HAS_FILE
char *cstr(Char *from, UCell size, int clear)
/* return a C-string corresponding to the Forth string ( FROM SIZE ).
   the C-string lives until the next call of cstr with CLEAR being true */
{
  static struct cstr_buffer {
    char *buffer;
    size_t size;
  } *buffers=NULL;
  static int nbuffers=0;
  static int used=0;
  struct cstr_buffer *b;

  if (buffers==NULL)
    buffers=malloc(0);
  if (clear)
    used=0;
  if (used>=nbuffers) {
    buffers=realloc(buffers,sizeof(struct cstr_buffer)*(used+1));
    buffers[used]=(struct cstr_buffer){malloc(0),0};
    nbuffers=used+1;
  }
  b=&buffers[used];
  if (size+1 > b->size) {
    b->buffer = realloc(b->buffer,size+1);
    b->size = size+1;
  }
  memcpy(b->buffer,from,size);
  b->buffer[size]='\0';
  used++;
  return b->buffer;
}

char *tilde_cstr(Char *from, UCell size, int clear)
/* like cstr(), but perform tilde expansion on the string */
{
  char *s1,*s2;
  int s1_len, s2_len;
  struct passwd *getpwnam (), *user_entry;

  if (size<1 || from[0]!='~')
    return cstr(from, size, clear);
  if (size<2 || from[1]=='/') {
    s1 = (char *)getenv ("HOME");
    if(s1 == NULL)
      s1 = "";
    s2 = from+1;
    s2_len = size-1;
  } else {
    UCell i;
    for (i=1; i<size && from[i]!='/'; i++)
      ;
    if (i==2 && from[1]=='+') /* deal with "~+", i.e., the wd */
      return cstr(from+3, size<3?0:size-3,clear);
    {
      char user[i];
      memcpy(user,from+1,i-1);
      user[i-1]='\0';
      user_entry=getpwnam(user);
    }
    if (user_entry==NULL)
      return cstr(from, size, clear);
    s1 = user_entry->pw_dir;
    s2 = from+i;
    s2_len = size-i;
  }
  s1_len = strlen(s1);
  if (s1_len>1 && s1[s1_len-1]=='/')
    s1_len--;
  {
    char path[s1_len+s2_len];
    memcpy(path,s1,s1_len);
    memcpy(path+s1_len,s2,s2_len);
    return cstr(path,s1_len+s2_len,clear);
  }
}
#endif

DCell timeval2us(struct timeval *tvp)
{
#ifndef BUGGY_LONG_LONG
  return (tvp->tv_sec*(DCell)1000000)+tvp->tv_usec;
#else
  DCell d2;
  DCell d1=mmul(tvp->tv_sec,1000000);
  d2.lo = d1.lo+tvp->tv_usec;
  d2.hi = d1.hi + (d2.lo<d1.lo);
  return d2;
#endif
}

#define NEWLINE	'\n'

#ifndef HAVE_RINT
#define rint(x)	floor((x)+0.5)
#endif

#ifdef HAS_FILE
static char* fileattr[6]={"rb","rb","r+b","r+b","wb","wb"};
static char* pfileattr[6]={"r","r","r+","r+","w","w"};

#ifndef O_BINARY
#define O_BINARY 0
#endif
#ifndef O_TEXT
#define O_TEXT 0
#endif

static int ufileattr[6]= {
  O_RDONLY|O_BINARY, O_RDONLY|O_BINARY,
  O_RDWR  |O_BINARY, O_RDWR  |O_BINARY,
  O_WRONLY|O_BINARY, O_WRONLY|O_BINARY };
#endif

/* conversion on fetch */

#define vm_Cell2f(x)		((Bool)(x))
#define vm_Cell2c(x)		((Char)(x))
#define vm_Cell2n(x)		((Cell)x)
#define vm_Cell2w(x)		((Cell)x)
#define vm_Cell2u(x)		((UCell)(x))
#define vm_Cell2a_(x)		((Cell *)(x))
#define vm_Cell2c_(x)		((Char *)(x))
#define vm_Cell2f_(x)		((Float *)(x))
#define vm_Cell2df_(x)		((DFloat *)(x))
#define vm_Cell2sf_(x)		((SFloat *)(x))
#define vm_Cell2xt(x)		((Xt)(x))
#define vm_Cell2f83name(x)	((struct F83Name *)(x))
#define vm_Cell2longname(x)	((struct Longname *)(x))
#define vm_Float2r(x)	(x)

/* conversion on store */

#define vm_f2Cell(x)		((Cell)(x))
#define vm_c2Cell(x)		((Cell)(x))
#define vm_n2Cell(x)		((Cell)(x))
#define vm_w2Cell(x)		((Cell)(x))
#define vm_u2Cell(x)		((Cell)(x))
#define vm_a_2Cell(x)		((Cell)(x))
#define vm_c_2Cell(x)		((Cell)(x))
#define vm_f_2Cell(x)		((Cell)(x))
#define vm_df_2Cell(x)		((Cell)(x))
#define vm_sf_2Cell(x)		((Cell)(x))
#define vm_xt2Cell(x)		((Cell)(x))
#define vm_f83name2Cell(x)	((Cell)(x))
#define vm_longname2Cell(x)	((Cell)(x))
#define vm_r2Float(x)	(x)

#define vm_Cell2Cell(x)		(x)

/* if machine.h has not defined explicit registers, define them as implicit */
#ifndef IPREG
#define IPREG
#endif
#ifndef SPREG
#define SPREG
#endif
#ifndef RPREG
#define RPREG
#endif
#ifndef FPREG
#define FPREG
#endif
#ifndef LPREG
#define LPREG
#endif
#ifndef CFAREG
#define CFAREG
#endif
#ifndef UPREG
#define UPREG
#endif
#ifndef TOSREG
#define TOSREG
#endif
#ifndef FTOSREG
#define FTOSREG
#endif

#ifndef CPU_DEP1
# define CPU_DEP1 0
#endif

/* instructions containing these must be the last instruction of a
   super-instruction (e.g., branches, EXECUTE, and other instructions
   ending the basic block). Instructions containing SET_IP get this
   automatically, so you usually don't have to write it.  If you have
   to write it, write it after IP points to the next instruction.
   Used for profiling.  Don't write it in a word containing SET_IP, or
   the following block will be counted twice. */
#ifdef VM_PROFILING
#define SUPER_END  vm_count_block(IP)
#else
#define SUPER_END
#endif
#define SUPER_CONTINUE

#ifdef GFORTH_DEBUGGING
/* define some VM registers as global variables, so they survive exceptions;
   global register variables are not up to the task (according to the 
   GNU C manual) */
Xt *ip;
Cell *rp;
#endif

#ifdef DEBUG
#define CFA_TO_NAME(__cfa) \
      Cell len, i; \
      char * name = __cfa; \
      for(i=0; i<32; i+=sizeof(Cell)) { \
        len = ((Cell*)name)[-1]; \
        if(len < 0) { \
	  len &= 0x1F; \
          if((len+sizeof(Cell)) > i) break; \
	} len = 0; \
	name -= sizeof(Cell); \
      }
#endif

Xt *primtable(Label symbols[], Cell size)
     /* used in primitive primtable for peephole optimization */
{
  Xt *xts = (Xt *)malloc(size*sizeof(Xt));
  Cell i;

  for (i=0; i<size; i++)
    xts[i] = &symbols[i];
  return xts;
}


define(enginerest,
`(Xt *ip0, Cell *sp0, Cell *rp0, Float *fp0, Address lp0)
/* executes code at ip, if ip!=NULL
   returns array of machine code labels (for use in a loader), if ip==NULL
*/
{
#ifndef GFORTH_DEBUGGING
  register Xt *ip IPREG;
  register Cell *rp RPREG;
#endif
  register Cell *sp SPREG = sp0;
  register Float *fp FPREG = fp0;
  register Address lp LPREG = lp0;
  register Xt cfa CFAREG;
#ifdef MORE_VARS
  MORE_VARS
#endif
  register Address up UPREG = UP;
  IF_spTOS(register Cell spTOS TOSREG;)
  IF_fpTOS(register Float fpTOS FTOSREG;)
#if defined(DOUBLY_INDIRECT)
  static Label *symbols;
  static void *routines[]= {
#define MAX_SYMBOLS (sizeof(routines)/sizeof(routines[0]))
#else /* !defined(DOUBLY_INDIRECT) */
  static Label symbols[]= {
#define MAX_SYMBOLS (sizeof(symbols)/sizeof(symbols[0]))
#endif /* !defined(DOUBLY_INDIRECT) */
    (Label)&&docol,
    (Label)&&docon,
    (Label)&&dovar,
    (Label)&&douser,
    (Label)&&dodefer,
    (Label)&&dofield,
    (Label)&&dodoes,
    /* the following entry is normally unused;
       it is there because its index indicates a does-handler */
    CPU_DEP1,
#define INST_ADDR(name) (Label)&&I_##name
#include "prim_lab.i"
#undef INST_ADDR
    (Label)&&after_last,
    (Label)0,
#ifdef IN_ENGINE2
#define INST_ADDR(name) (Label)&&J_##name
#include "prim_lab.i"
#undef INST_ADDR
#endif
  };
#ifdef CPU_DEP2
  CPU_DEP2
#endif

  ip = ip0;
  rp = rp0;
#ifdef DEBUG
  fprintf(stderr,"ip=%x, sp=%x, rp=%x, fp=%x, lp=%x, up=%x\n",
          (unsigned)ip,(unsigned)sp,(unsigned)rp,
	  (unsigned)fp,(unsigned)lp,(unsigned)up);
#endif

  if (ip == NULL) {
#if defined(DOUBLY_INDIRECT)
#define CODE_OFFSET (26*sizeof(Cell))
#define XT_OFFSET (22*sizeof(Cell))
    int i;
    Cell code_offset = offset_image? CODE_OFFSET : 0;
    Cell xt_offset = offset_image? XT_OFFSET : 0;

    symbols = (Label *)(malloc(MAX_SYMBOLS*sizeof(Cell)+CODE_OFFSET)+code_offset);
    xts = (Label *)(malloc(MAX_SYMBOLS*sizeof(Cell)+XT_OFFSET)+xt_offset);
    for (i=0; i<DOESJUMP+1; i++)
      xts[i] = symbols[i] = (Label)routines[i];
    for (; routines[i]!=0; i++) {
      if (i>=MAX_SYMBOLS) {
	fprintf(stderr,"gforth-ditc: more than %d primitives\n",MAX_SYMBOLS);
	exit(1);
      }
      xts[i] = symbols[i] = &routines[i];
    }
#endif /* defined(DOUBLY_INDIRECT) */
    return symbols;
  }

  IF_spTOS(spTOS = sp[0]);
  IF_fpTOS(fpTOS = fp[0]);
/*  prep_terminal(); */
  SET_IP(ip);
  SUPER_END; /* count the first block, too */
  NEXT;


#ifdef CPU_DEP3
  CPU_DEP3
#endif
  
 docol:
  {
#ifdef DEBUG
    {
      CFA_TO_NAME(cfa);
      fprintf(stderr,"%08lx: col: %08lx %.*s\n",(Cell)ip,(Cell)PFA1(cfa),
	      len,name);
    }
#endif
#ifdef CISC_NEXT
    /* this is the simple version */
    *--rp = (Cell)ip;
    SET_IP((Xt *)PFA1(cfa));
    SUPER_END;
    NEXT;
#else
    /* this one is important, so we help the compiler optimizing */
    {
      DEF_CA
      rp[-1] = (Cell)ip;
      SET_IP((Xt *)PFA1(cfa));
      SUPER_END;
      NEXT_P1;
      rp--;
      NEXT_P2;
    }
#endif
  }

 docon:
  {
#ifdef DEBUG
    fprintf(stderr,"%08lx: con: %08lx\n",(Cell)ip,*(Cell*)PFA1(cfa));
#endif
#ifdef USE_TOS
    *sp-- = spTOS;
    spTOS = *(Cell *)PFA1(cfa);
#else
    *--sp = *(Cell *)PFA1(cfa);
#endif
  }
  NEXT_P0;
  NEXT;
  
 dovar:
  {
#ifdef DEBUG
    fprintf(stderr,"%08lx: var: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef USE_TOS
    *sp-- = spTOS;
    spTOS = (Cell)PFA1(cfa);
#else
    *--sp = (Cell)PFA1(cfa);
#endif
  }
  NEXT_P0;
  NEXT;
  
 douser:
  {
#ifdef DEBUG
    fprintf(stderr,"%08lx: user: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef USE_TOS
    *sp-- = spTOS;
    spTOS = (Cell)(up+*(Cell*)PFA1(cfa));
#else
    *--sp = (Cell)(up+*(Cell*)PFA1(cfa));
#endif
  }
  NEXT_P0;
  NEXT;
  
 dodefer:
  {
#ifdef DEBUG
    fprintf(stderr,"%08lx: defer: %08lx\n",(Cell)ip,*(Cell*)PFA1(cfa));
#endif
    SUPER_END;
    EXEC(*(Xt *)PFA1(cfa));
  }

 dofield:
  {
#ifdef DEBUG
    fprintf(stderr,"%08lx: field: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
    spTOS += *(Cell*)PFA1(cfa);
  }
  NEXT_P0;
  NEXT;

 dodoes:
  /* this assumes the following structure:
     defining-word:
     
     ...
     DOES>
     (possible padding)
     possibly handler: jmp dodoes
     (possible branch delay slot(s))
     Forth code after DOES>
     
     defined word:
     
     cfa: address of or jump to handler OR
          address of or jump to dodoes, address of DOES-code
     pfa:
     
     */
  {
    /*    fprintf(stderr, "Got CFA %08lx at doescode %08lx/%08lx: does: %08lx\n",cfa,(Cell)ip,(Cell)PFA(cfa),(Cell)DOES_CODE1(cfa));*/
#ifdef DEBUG
    fprintf(stderr,"%08lx/%08lx: does: %08lx\n",(Cell)ip,(Cell)PFA(cfa),(Cell)DOES_CODE1(cfa));
    fflush(stderr);
#endif
    *--rp = (Cell)ip;
    /* PFA1 might collide with DOES_CODE1 here, so we use PFA */
#ifdef USE_TOS
    *sp-- = spTOS;
    spTOS = (Cell)PFA(cfa);
#else
    *--sp = (Cell)PFA(cfa);
#endif
    SET_IP(DOES_CODE1(cfa));
    SUPER_END;
    /*    fprintf(stderr,"TOS = %08lx, IP=%08lx\n", spTOS, IP);*/
  }
  NEXT;

#ifndef IN_ENGINE2
#define LABEL(name) I_##name
#else
#define LABEL(name) J_##name: asm(".skip 16"); I_##name
#endif
#include "prim.i"
#undef LABEL
  after_last: return (Label *)0;
  /*needed only to get the length of the last primitive */
}'
)

Label *engine enginerest

#define IN_ENGINE2
Label *engine2 enginerest

