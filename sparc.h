/*
  $Id: sparc.h,v 1.8 1995-01-10 18:34:29 anton Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for a SPARC
*/

#include "32bit.h"

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

/* direct threading is probably faster on the SPARC, but has it been
   tested? Therefore, DIRECT_THREADED is not defined */

#ifdef DIRECT_THREADED
#ifndef WORDS_BIGENDIAN
#error Direct threading only supported for big-endian machines.
/* little endian SPARCs still store instructions in big-endian format,
   so you would have to reverse the instructions stores in the following
*/
#endif

/* according to the SPARC V9 architecture manual, we have to use flush,
   but as V2.20 does not recognize the opcode */
/* assuming size = 8 */
#define CACHE_FLUSH(addr,size) \
  asm("iflush %0; iflush %0+4"::"r"(addr))

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
/* the improvement here is that we may destroy cfa before using PFA1 */
#define PFA1(cfa)	PFA(cfa)
#ifdef undefined
#define PFA1(cfa)	/* PFA(cfa) */ \
			({register Cell *pfa asm("%o7"); \
			  pfa+2; })
#endif
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	({unsigned _cfa = (unsigned)(cfa); \
				    (Label)(_cfa+((*(unsigned *)_cfa)<<2));})
/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
/* we use call, since 'branch always' only has 22 bits displacement */
#define MAKE_CF(cfa,ca)	({long *_cfa        = (long *)(cfa); \
			  unsigned _ca = (unsigned)(ca); \
			  _cfa[0] = 0x40000000|((_ca+4-(unsigned)_cfa)>>2); /* CALL ca */ \
			  _cfa[1] = *(long *)_ca; /* delay slot */})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_CODE(label)	((Xt *)(CODE_ADDRESS(label)+DOES_HANDLER_SIZE))

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	DOES_CODE(label)
#ifdef undefined
#define DOES_CODE1(label)	({register Xt *_does_code asm("%o7"); \
			  	_does_code+2; })
#endif

/* this stores a call dodoes at addr */
#define MAKE_DOES_HANDLER(addr) MAKE_CF(addr,symbols[DODOES])

#define DOES_HANDLER_SIZE       8

#define MAKE_DOES_CF(addr,doesp) ({long *_addr        = (long *)(addr); \
			  unsigned _doesp = (unsigned)(doesp); \
			  _addr[0] = 0x40000000|((_doesp-8-(unsigned)_addr)>>2); /* CALL doesp-8 */ \
			  _addr[1] = 0x01000000; /* nop */})
#endif

