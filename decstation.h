/*
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for a Decstation running Ultrix
*/
/* cache flush stuff */

#ifdef DIRECT_THREADED

#include <mips/cachectl.h>

#define CACHE_FLUSH(addr,size) \
				cacheflush((char *)(addr), (int)(size), BCACHE)

#endif

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

/* some definitions for composing opcodes */
#define JUMP_MASK	0x03ffffff
#define J_PATTERN	0x08000000
#define JAL_PATTERN	0x0c000000
/* this provides the first 4 bits of a jump address, i.e. it must be <16 */
#define SEGMENT_NUM	1

#ifdef DIRECT_THREADED
	/* PFA gives the parameter field address corresponding to a cfa */
#	define PFA(cfa)	(((Cell *)cfa)+2)
	/* PFA1 is a special version for use just after a NEXT1 */
#	define PFA1(cfa)	PFA(cfa)
	/* CODE_ADDRESS is the address of the code jumped to through the code field */
#	define CODE_ADDRESS(cfa)	((Label)(((*(unsigned *)(cfa))^J_PATTERN^(SEGMENT_NUM<<26))<<2))
	/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
#	define MAKE_CF(cfa,ca)	({long * _cfa = (long *)(cfa); \
					  _cfa[0] = J_PATTERN|((((long)(ca))&JUMP_MASK)>>2); /* J ca */ \
					  _cfa[1] = 0; /* nop */})
#	ifdef undefined
		/* the following version uses JAL to make PFA1 faster */
#		define PFA1(label)	({register Cell *pfa asm("$31"); \
						pfa; })
		/* CODE_ADDRESS is the address of the code jumped to through the code field */
#		define CODE_ADDRESS(cfa)	((Label)(((*(unsigned *)(cfa))^JAL_PATTERN^(SEGMENT_NUM<<26))<<2))
#		define MAKE_CF(cfa,ca)	({long *_cfa = (long *)(cfa); \
					  long _ca = (long)(ca); \
						  _cfa[0] = JAL_PATTERN|(((((long)_ca)>>2))&JUMP_MASK); /* JAL ca+4 */ \
						  _cfa[1] = 0; /* *(long *)_ca; delay slot */})
#	endif /* undefined */

	/* this is the point where the does code starts if label points to the
	 * jump dodoes */
#	define DOES_CODE(cfa)	((Xt *)(((char *)CODE_ADDRESS(cfa))+8))

	/* this is a special version of DOES_CODE for use in dodoes */
#	define DOES_CODE1(cfa)	DOES_CODE(cfa)

#	define DOES_HANDLER_SIZE	8
#	define MAKE_DOES_CF(cfa,does_code) \
			({long does_handlerp=((long)(does_code))-DOES_HANDLER_SIZE; \
			  long *_cfa = (long*)(cfa); \
			  _cfa[0] = J_PATTERN|((does_handlerp&JUMP_MASK)>>2); /* J ca */ \
			  _cfa[1] = 0; /* nop */})
/*
#	define MAKE_DOES_CF(cfa, does_code)	({char *does_handlerp=((char *)does_code)-DOES_HANDLER_SIZE;	\
						  MAKE_CF(cfa,does_handlerp);	\
						  MAKE_DOES_HANDLER(does_handlerp) ;})
*/
	/* this stores a jump dodoes at addr */
#	define MAKE_DOES_HANDLER(addr)	MAKE_CF(addr,symbols[DODOES])

#endif
#ifdef undefined
/* and here are some more efficient versions that can be tried later */

/* the first version saves one cycle by doing something useful in the
   delay slot. !! check that the instruction in the delay slot is legal
*/

#define MAKE_DOESJUMP(addr)	({long * _addr = (long *)addr; \
				  _addr[0] = J_PATTERN|(((((long)symbols[DODOES])>>2)+4)&JUMP_MASK), /* J dodoes+4 */ \
				  _addr[1] = *(long *)symbols[DODOES]; /* delay */})

/* the following version uses JAL to make DOES_CODE1 faster */
/* !! does the declaration clear the register ? */
/* it's ok to use the same reg as in PFA1:
   dodoes is the only potential problem and I have taken care of it */

#define DOES_CODE1(cfa)	({register Code *_does_code asm("$31"); \
				    _does_code; })
#define MAKE_DOESJUMP(addr)	({long * _addr = (long *)addr; \
				  _addr[0] = JAL_PATTERN|(((((long)symbols[DODOES])>>2)+4)&JUMP_MASK), /* JAL dodoes+4 */ \
				  _addr[1] = *(long *)symbols[DODOES]; /* delay */})

#endif

#ifdef FORCE_REG
#define IPREG asm("$16")
#define SPREG asm("$17")
#define RPREG asm("$18")
#define LPREG asm("$19")
#define CFAREG asm("$20")
#define TOSREG asm("$21")
#endif /* FORCE_REG */
