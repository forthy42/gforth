/*
  $Id: apollo68k.h,v 1.2 1994-05-31 07:08:10 benschop Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for a HP/Apollo with a 680x0
  processor running Domain/OS
*/


/* Cell and UCell must be the same size as a pointer */
typedef long Cell;
typedef unsigned long UCell;

/* DCell and UDCell must be twice as large as Cell */
typedef long long DCell;
typedef unsigned long long UDCell;

/* define this if IEEE singles and doubles are available as C data types */
#define IEEE_FP

/* the IEEE types are used only for loading and storing */
/* the IEEE double precision type */
typedef double DFloat;
/* the IEEE single precision type */
typedef float SFloat;

/* define this if the least-significant byte is at the largets address */
#define BIG_ENDIAN

#ifdef DIRECT_THREADED
/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	(*(Label *)(((char *)(cfa))+2))
/* MAKE_CF creates an appropriate code field at the cfa;
   ca is the code address */
#define MAKE_CF(cfa,ca)		({short * _cfa = (short *)cfa; \
				  _cfa[0] = 0x4ef9; /* jmp.l */ \
				  *(long *)(_cfa+1) = (long)(ca);})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_CODE(label)	((Xt *)(((char *)label)+8))

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	DOES_CODE(label)

/* this stores a jump dodoes at ca */
#define MAKE_DOESJUMP(ca)	({short * _ca = (short *)ca; \
				  _ca[0] = 0x4ef9; /* jmp.l */ \
				  *(long *)(_ca+1) = (long)&&dodoes;})
#endif

