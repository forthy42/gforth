/*
  $Id: engine.c,v 1.17 1994-09-28 17:02:46 anton Exp $
  Copyright 1992 by the ANSI figForth Development Group
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
#include <time.h>
#include <sys/time.h>
#include "forth.h"
#include "io.h"

typedef union {
  struct {
#ifdef BIG_ENDIAN
    Cell high;
    Cell low;
#else
    Cell low;
    Cell high;
#endif;
  } cells;
  DCell dcell;
} Double_Store;

typedef struct F83Name {
  struct F83Name	*next;  /* the link field for old hands */
  char			countetc;
  Char			name[0];
} F83Name;

/* are macros for setting necessary? */
#define F83NAME_COUNT(np)	((np)->countetc & 0x1f)
#define F83NAME_SMUDGE(np)	(((np)->countetc & 0x40) != 0)
#define F83NAME_IMMEDIATE(np)	(((np)->countetc & 0x20) != 0)

/* NEXT and NEXT1 are split into several parts to help scheduling,
   unless CISC_NEXT is defined */
#ifdef CISC_NEXT
#define NEXT1_P1
#define NEXT_P1
#define DEF_CA
#ifdef DIRECT_THREADED
#define NEXT1_P2 ({goto *cfa;})
#else
#define NEXT1_P2 ({goto **cfa;})
#endif /* DIRECT_THREADED */
#define NEXT_P2 ({cfa = *ip++; NEXT1_P2;})
#else /* CISC_NEXT */
#ifdef DIRECT_THREADED
#define NEXT1_P1
#define NEXT1_P2 ({goto *cfa;})
#define DEF_CA
#else /* DIRECT_THREADED */
#define NEXT1_P1 ({ca = *cfa;})
#define NEXT1_P2 ({goto *ca;})
#define DEF_CA	Label ca;
#endif /* DIRECT_THREADED */
#define NEXT_P1 ({cfa=*ip++; NEXT1_P1;})
#define NEXT_P2 NEXT1_P2
#endif /* CISC_NEXT */

#define NEXT1 ({DEF_CA NEXT1_P1; NEXT1_P2;})
#define NEXT ({DEF_CA NEXT_P1; NEXT_P2;})

#ifdef USE_TOS
#define IF_TOS(x) x
#else
#define IF_TOS(x)
#define TOS (sp[0])
#endif

#ifdef USE_FTOS
#define IF_FTOS(x) x
#else
#define IF_FTOS(x)
#define FTOS (fp[0])
#endif

int emitcounter;
#define NULLC '\0'

char *cstr(Char *from, UCell size, int clear)
/* if clear is true, scratch can be reused, otherwise we want more of
   the same */
{
  static char *scratch=NULL;
  static unsigned scratchsize=0;
  static char *nextscratch;
  char *oldnextscratch;

  if (clear)
    nextscratch=scratch;
  if (scratch==NULL) {
    scratch=malloc(size+1);
    nextscratch=scratch;
    scratchsize=size;
  }
  else if (nextscratch+size>scratch+scratchsize) {
    char *oldscratch=scratch;
    scratch = realloc(scratch, (nextscratch-scratch)+size+1);
    nextscratch=scratch+(nextscratch-oldscratch);
    scratchsize=size;
  }
  memcpy(nextscratch,from,size);
  nextscratch[size]='\0';
  oldnextscratch = nextscratch;
  nextscratch += size+1;
  return oldnextscratch;
}

#define NEWLINE	'\n'


static char* fileattr[6]={"r","rb","r+","r+b","w+","w+b"};

static Address up0=NULL;

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
  register Xt cfa CFAREG;
  register Address up UPREG = up0;
  IF_TOS(register Cell TOS TOSREG;)
  IF_FTOS(register Float FTOS FTOSREG;)
  static Label symbols[]= {
    &&docol,
    &&docon,
    &&dovar,
    &&douser,
    &&dodefer,
    &&dodoes,
    &&dodoes,  /* dummy for does handler address */
#include "prim_labels.i"
  };
#ifdef CPU_DEP
  CPU_DEP;
#endif

#ifdef DEBUG
  fprintf(stderr,"ip=%x, sp=%x, rp=%x, fp=%x, lp=%x, up=%x\n",
          ip,sp,rp,fp,lp,up);
#endif

  if (ip == NULL)
    return symbols;

  IF_TOS(TOS = sp[0]);
  IF_FTOS(FTOS = fp[0]);
  prep_terminal();
  NEXT;
  
 docol:
#ifdef DEBUG
  fprintf(stderr,"%08x: col: %08x\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef CISC_NEXT
  /* this is the simple version */
  *--rp = (Cell)ip;
  ip = (Xt *)PFA1(cfa);
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
  
 docon:
#ifdef DEBUG
  fprintf(stderr,"%08x: con: %08x\n",(Cell)ip,*(Cell*)PFA1(cfa));
#endif
#ifdef USE_TOS
  *sp-- = TOS;
  TOS = *(Cell *)PFA1(cfa);
#else
  *--sp = *(Cell *)PFA1(cfa);
#endif
  NEXT;
  
 dovar:
#ifdef DEBUG
  fprintf(stderr,"%08x: var: %08x\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef USE_TOS
  *sp-- = TOS;
  TOS = (Cell)PFA1(cfa);
#else
  *--sp = (Cell)PFA1(cfa);
#endif
  NEXT;
  
  /* !! user? */
  
 douser:
#ifdef DEBUG
  fprintf(stderr,"%08x: user: %08x\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
#ifdef USE_TOS
  *sp-- = TOS;
  TOS = (Cell)(up+*(Cell*)PFA1(cfa));
#else
  *--sp = (Cell)(up+*(Cell*)PFA1(cfa));
#endif
  NEXT;
  
 dodefer:
#ifdef DEBUG
  fprintf(stderr,"%08x: defer: %08x\n",(Cell)ip,(Cell)PFA1(cfa));
#endif
  cfa = *(Xt *)PFA1(cfa);
  NEXT1;

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
#ifdef DEBUG
  fprintf(stderr,"%08x/%08x: does: %08x\n",(Cell)ip,(Cell)PFA(cfa),(Cell)DOES_CODE1(cfa));
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
  NEXT;

#include "primitives.i"
}
