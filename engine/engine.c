/* Gforth virtual machine (aka inner interpreter)

  Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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
#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <assert.h>
#include <stdlib.h>
#include <errno.h>
#include "forth.h"
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
#else
#include "systypes.h"
#endif

#if defined(HAVE_LIBDL) || defined(HAVE_DLOPEN) /* what else? */
#include <dlfcn.h>
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

/* are macros for setting necessary? */
#define F83NAME_COUNT(np)	((np)->countetc & 0x1f)
#define F83NAME_SMUDGE(np)	(((np)->countetc & 0x40) != 0)
#define F83NAME_IMMEDIATE(np)	(((np)->countetc & 0x20) != 0)

Cell *SP;
Float *FP;
Address UP=NULL;

#if 0
/* not used currently */
int emitcounter;
#endif
#define NULLC '\0'

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

#ifdef STANDALONE
char *tilde_cstr(Char *from, UCell size, int clear)
{
  return cstr(from, size, clear);
}
#else
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

#define NEWLINE	'\n'

#ifndef HAVE_RINT
#define rint(x)	floor((x)+0.5)
#endif

static char* fileattr[6]={"r","rb","r+","r+b","w","wb"};

#ifndef O_BINARY
#define O_BINARY 0
#endif
#ifndef O_TEXT
#define O_TEXT 0
#endif

static int ufileattr[6]= {
  O_RDONLY|O_TEXT, O_RDONLY|O_BINARY,
  O_RDWR  |O_TEXT, O_RDWR  |O_BINARY,
  O_WRONLY|O_TEXT, O_WRONLY|O_BINARY };

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

/* declare and compute cfa for certain threading variants */
/* warning: this is nonsyntactical; it will not work in place of a statement */
#ifdef CFA_NEXT
#define DOCFA
#else
#define DOCFA	Xt cfa; GETCFA(cfa)
#endif

Label *engine(Xt *ip0, Cell *sp0, Cell *rp0, Float *fp0, Address lp0)
/* executes code at ip, if ip!=NULL
   returns array of machine code labels (for use in a loader), if ip==NULL
*/
{
  register Xt *ip IPREG = ip0;
  register Cell *sp SPREG = sp0;
  register Cell *rp RPREG = rp0;
  register Float *fp FPREG = fp0;
  register Address lp LPREG = lp0;
#ifdef CFA_NEXT
  register Xt cfa CFAREG;
#endif
  register Address up UPREG = UP;
  IF_TOS(register Cell TOS TOSREG;)
  IF_FTOS(register Float FTOS FTOSREG;)
#if defined(DOUBLY_INDIRECT)
  static Label *symbols;
  static void *routines[]= {
#else /* !defined(DOUBLY_INDIRECT) */
  static Label symbols[]= {
#endif /* !defined(DOUBLY_INDIRECT) */
    (Label)&&docol,
    (Label)&&docon,
    (Label)&&dovar,
    (Label)&&douser,
    (Label)&&dodefer,
    (Label)&&dofield,
    (Label)&&dodoes,
    /* the following entry is normally unused;
       it's there because its index indicates a does-handler */
    CPU_DEP1,
#include "prim_lab.i"
    (Label)0
  };
#ifdef CPU_DEP2
  CPU_DEP2
#endif

#ifdef DEBUG
  fprintf(stderr,"ip=%x, sp=%x, rp=%x, fp=%x, lp=%x, up=%x\n",
          (unsigned)ip,(unsigned)sp,(unsigned)rp,
	  (unsigned)fp,(unsigned)lp,(unsigned)up);
#endif

  if (ip == NULL) {
#if defined(DOUBLY_INDIRECT)
#define MAX_SYMBOLS (sizeof(routines)/sizeof(routines[0]))
#define CODE_OFFSET (22*sizeof(Cell))
    int i;
    Cell code_offset = offset_image? CODE_OFFSET : 0;

    symbols = (Label *)(malloc(MAX_SYMBOLS*sizeof(Cell)+CODE_OFFSET)+code_offset);
    for (i=0; i<DOESJUMP+1; i++)
      symbols[i] = (Label)routines[i];
    for (; routines[i]!=0; i++) {
      if (i>=MAX_SYMBOLS) {
	fprintf(stderr,"gforth-ditc: more than %d primitives\n",MAX_SYMBOLS);
	exit(1);
    }
    symbols[i] = &routines[i];
  }
#endif /* defined(DOUBLY_INDIRECT) */
  return symbols;
}

  IF_TOS(TOS = sp[0]);
  IF_FTOS(FTOS = fp[0]);
/*  prep_terminal(); */
  NEXT_P0;
  NEXT;

#ifdef CPU_DEP3
  CPU_DEP3
#endif
  
 docol:
  {
    DOCFA;
#ifdef DEBUG
    fprintf(stderr,"%08lx: col: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef CISC_NEXT
    /* this is the simple version */
    *--rp = (Cell)ip;
    ip = (Xt *)PFA1(cfa);
    NEXT_P0;
    NEXT;
#else
    /* this one is important, so we help the compiler optimizing
       The following version may be better (for scheduling), but probably has
       problems with code fields employing calls and delay slots
       */
    {
      DEF_CA
      Xt *current_ip = (Xt *)PFA1(cfa);
      cfa = *current_ip;
      NEXT1_P1;
      *--rp = (Cell)ip;
      ip = current_ip+1;
      NEXT1_P2;
    }
#endif
  }

 docon:
  {
    DOCFA;
#ifdef DEBUG
    fprintf(stderr,"%08lx: con: %08lx\n",(Cell)ip,*(Cell*)PFA1(cfa));
#endif
#ifdef USE_TOS
    *sp-- = TOS;
    TOS = *(Cell *)PFA1(cfa);
#else
    *--sp = *(Cell *)PFA1(cfa);
#endif
  }
  NEXT_P0;
  NEXT;
  
 dovar:
  {
    DOCFA;
#ifdef DEBUG
    fprintf(stderr,"%08lx: var: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef USE_TOS
    *sp-- = TOS;
    TOS = (Cell)PFA1(cfa);
#else
    *--sp = (Cell)PFA1(cfa);
#endif
  }
  NEXT_P0;
  NEXT;
  
 douser:
  {
    DOCFA;
#ifdef DEBUG
    fprintf(stderr,"%08lx: user: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef USE_TOS
    *sp-- = TOS;
    TOS = (Cell)(up+*(Cell*)PFA1(cfa));
#else
    *--sp = (Cell)(up+*(Cell*)PFA1(cfa));
#endif
  }
  NEXT_P0;
  NEXT;
  
 dodefer:
  {
    DOCFA;
#ifdef DEBUG
    fprintf(stderr,"%08lx: defer: %08lx\n",(Cell)ip,*(Cell*)PFA1(cfa));
#endif
    EXEC(*(Xt *)PFA1(cfa));
  }

 dofield:
  {
    DOCFA;
#ifdef DEBUG
    fprintf(stderr,"%08lx: field: %08lx\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
    TOS += *(Cell*)PFA1(cfa); 
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
    DOCFA;

    /*    fprintf(stderr, "Got CFA %08lx at doescode %08lx/%08lx: does: %08lx\n",cfa,(Cell)ip,(Cell)PFA(cfa),(Cell)DOES_CODE1(cfa));*/
#ifdef DEBUG
    fprintf(stderr,"%08lx/%08lx: does: %08lx\n",(Cell)ip,(Cell)PFA(cfa),(Cell)DOES_CODE1(cfa));
    fflush(stderr);
#endif
    *--rp = (Cell)ip;
    /* PFA1 might collide with DOES_CODE1 here, so we use PFA */
    ip = DOES_CODE1(cfa);
#ifdef USE_TOS
    *sp-- = TOS;
    TOS = (Cell)PFA(cfa);
#else
    *--sp = (Cell)PFA(cfa);
#endif
    /*    fprintf(stderr,"TOS = %08lx, IP=%08lx\n", TOS, IP);*/
  }
  NEXT_P0;
  NEXT;

#include "prim.i"
}
