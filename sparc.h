/*
  $Id: sparc.h,v 1.4 1994-12-14 16:15:33 anton Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for a SPARC
*/

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

/* direct threading is probably faster on the SPARC, but has it been
   tested? Therefore, DIRECT_THREADED is not defined */

#ifdef DIRECT_THREADED
/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
/* the improvement here is that we may destroy cfa before using PFA1 */
#define PFA1(cfa)	/* PFA(cfa) */ \
			({register Cell *pfa asm("%15"); \
			  pfa+2; })
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	((Label)((*(unsigned *)(cfa))<<2))
/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
/* we use call, since 'branch always' only has 22 bits displacement */
#define MAKE_CF(cfa,ca)	({long *_cfa        = (long *)(cfa); \
			  unsigned _ca = (unsigned)(ca); \
			  _cfa[0] = 0x8000000|((_ca+4-(unsigned)_cfa)>>2) /* CALL ca */ \
			  _cfa[1] = *(long *)_ca; /* delay slot */})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_CODE(label)	(((Xt *)(label))+2)

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	({register Xt *_does_code asm("%15"); \
			  	_does_code+2; })

/* this stores a call dodoes at addr */
#define MAKE_DOESJUMP(addr)	({long *_addr = (long *)(addr); \
				  _addr[0] = 0x8000000|((((unsigned)&&dodoes)+4-((unsigned)_addr))>>2) /* CALL dodoes */ \
			  _addr[1] = *(long *)&&dodoes; /* delay slot */})
#endif
