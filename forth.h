/*
  Copyright 1992 by the ANSI figForth Development Group
*/

/* common header file */

typedef void *Label;

/* symbol indexed constants */

#define DOCOL	0
#define DOCON	1
#define DOVAR	2
#define DOUSER	3
#define DODEFER	4
#define DOFIELD	5
#define DODOES	6
#define DOESJUMP	7

#include "machine.h"

/* Forth data types */
typedef int Bool;
#define FLAG(b) (-(b))
#define FILEIO(error)	(FLAG(error) & -37)
#define FILEEXIST(error)	(FLAG(error) & -38)

#define F_TRUE (FLAG(0==0))
#define F_FALSE (FLAG(0!=0))

typedef unsigned char Char;
typedef double Float;
typedef char *Address;

#ifdef DIRECT_THREADED
typedef Label Xt;
#else
typedef Label *Xt;
#endif

Label *engine(Xt *ip, Cell *sp, Cell *rp, Float *fp, Address lp);

#ifndef DIRECT_THREADED
/* i.e. indirect threaded */
/* the direct threaded version is machine dependent and resides in machine.h */

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	(*(Label *)(cfa))
      /* DOES_CODE is the Forth code does jumps to */
#define DOES_CODE(cfa)           (cfa[1])
#define DOES_CODE1(cfa)          DOES_CODE(cfa)
/* MAKE_CF creates an appropriate code field at the cfa;
   ca is the code address */
#define MAKE_CF(cfa,ca) ((*(Label *)(cfa)) = ((Label)ca))
/* make a code field for a defining-word-defined word */
#define MAKE_DOES_CF(cfa,does_code)	({MAKE_CF(cfa,symbols[DODOES]);	\
					  ((Cell *)cfa)[1] = (Cell)does_code;})
/* the does handler resides between DOES> and the following Forth code */
#define DOES_HANDLER_SIZE	(2*sizeof(Cell))
#define MAKE_DOES_HANDLER(addr)	0 /* do nothing */
#endif

#ifdef DEBUG
#	define	NAME(string)	fprintf(stderr,"%08x: "string"\n",(Cell)ip);
#else
#	define	NAME(string)
#endif

#define CF(const)	(-const-2)

#define CF_NIL	-1

#ifndef FLUSH_ICACHE
#warning flush-icache probably will not work (see manual)
#	define FLUSH_ICACHE(addr,size) 0
#endif

#ifdef DIRECT_THREADED
#define CACHE_FLUSH(addr,size) FLUSH_ICACHE(addr,size)
#else
#define CACHE_FLUSH(addr,size) 0
#endif
