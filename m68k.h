/*
  $Id: m68k.h,v 1.2 1995-01-18 18:41:41 anton Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for the 68000 and family
*/

#include "32bit.h"

#ifdef DIRECT_THREADED

#define CACHE_FLUSH(addr,size)    cache_$clear()
/* Clearing the whole cache is a bit drastic, but this is the only
   cache control available on the apollo.
*/

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
#define DOES_CODE(label)	((Xt *)(((char *)CODE_ADDRESS(label))+DOES_HANDLER_SIZE))

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	DOES_CODE(label)

/* this stores a call dodoes at addr */
#define MAKE_DOES_HANDLER(addr) MAKE_CF(addr,symbols[DODOES])

#define DOES_HANDLER_SIZE       8

#define MAKE_DOES_CF(addr,doesp)   MAKE_CF(addr,((int)(doesp)-8))
#endif

