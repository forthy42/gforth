/*
  $Id: 386.h,v 1.5 1994-09-28 17:02:44 anton Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for Intel 386 compatible processors
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
/* #define BIG_ENDIAN */

/* define this if the processor cannot exploit instruction-level
   parallelism (no pipelining or too few registers) */
#define CISC_NEXT

#ifdef DIRECT_THREADED
/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa) \
    ({long _cfa = (long)(cfa); (Label)(_cfa+*((long *)(_cfa+1))+5);})
/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
#define MAKE_CF(cfa,ca)	({long _cfa = (long)(cfa); \
                          long _ca  = (long)(ca); \
			  *(char *)_cfa = 0xe9; /* jmp */ \
			  *(long *)(_cfa+1) = _ca-(_cfa+5);})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_HANDLER_SIZE       8
#define DOES_CODE(label)	((Xt *)(CODE_ADDRESS(label)+DOES_HANDLER_SIZE))

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	DOES_CODE(label)

/* this stores a jump dodoes at addr */
#define MAKE_DOES_HANDLER(addr) MAKE_CF(addr,symbols[DODOES])

#define MAKE_DOES_CF(addr,doesp) MAKE_CF(addr,((int)(doesp)-8))
#endif

#ifdef FORCE_REG
#define IPREG asm("%esi")
#define SPREG asm("%edi")
#ifdef USE_TOS
#define CFAREG asm("%ecx")
#else
#define CFAREG asm("%edx")
#endif
#endif /* FORCE_REG */

#define rint(x)	floor((x)+0.5)
