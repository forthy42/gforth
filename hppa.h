/*
  $Id: hppa.h,v 1.1.1.1 1994-02-11 16:30:47 anton Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for a HPPA running HP-UX
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
#	define PFA(cfa)	(((Cell *)cfa)+2)
	/* PFA1 is a special version for use just after a NEXT1 */
	/* the improvement here is that we may destroy cfa before using PFA1 */
#	define PFA1(cfa)       /* PFA(cfa) */ \
	                        ({register volatile Cell *pfa asm("%31"); \
	                          ((int)pfa & (-4)); })
	/* HPPA uses register 2 for branch and link */

	/* CODE_ADDRESS is the address of the code jumped to through the code field */
#	define CODE_ADDRESS(cfa)	((Label)((*(unsigned *)(cfa))/* <<2 */))
	/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
	/* we use ble and a register, since 'bl' only has 21 bits displacement */
#endif
#define MAKE_CFA(cfa,ca)	({long *_cfa        = (long *)(cfa); \
			  unsigned _ca = (unsigned)(ca); \
				  _cfa[0] = 0xE4A02000 | ((_ca+4-symbols[0]) & 0x7FC)<<1 ; \
				  _cfa[1] = *(long *)(_ca); \
				  /* printf("%08x:%08x,%08x\n",_cfa,_cfa[0],_cfa[1]); */ \
			  })

#ifdef DIRECT_THREADED
#	define MAKE_CF(cfa,ca)		MAKE_CFA(cfa,ca)
	/*
	#define MAKE_CF(cfa,ca)	({long *_cfa        = (long *)(cfa); \
			  unsigned _ca = (unsigned)(ca); \
			  if((_ca-(int)(_cfa+1)>=-0x40000)||(_ca-(int)(_cfa+1)<0x40000)) \
			  { \
				  _cfa[1] = *(long *)(_ca); \
				  _ca = _ca-(int)(_cfa+1); \
				  _cfa[0] = 0xEBE00000 | \
				            ((int)_ca<0) | \
				            (_ca & 0x00FFC)<<1 | \
				            (_ca & 0x01000)>>10 | \
				            (_ca & 0x3E000)<<3; \
			  } \
			  else \
			  { \
				  _cfa[0] = 0x20200000 | \
				            ((int)_ca<0) | \
				            (_ca & 0x00001800)<<1 | \
				            (_ca & 0x0003E000)<<3 | \
				            (_ca & 0x000C0000)>>4 | \
				            (_ca & 0x7FF00000)>>19  \
				  _cfa[1] = 0xE4202002 | (_ca & 0x7FC)<<1 ; \
			  }})
	*/
	/* HP wins the price for the most obfuscated binary opcode */

	/* this is the point where the does code starts if label points to the
	 * jump dodoes */

#	define DOES_CODE(label)	(((Xt *)(label))+2)

	/* this is a special version of DOES_CODE for use in dodoes */
#	define DOES_CODE1(label)	({register volatile Xt *_does_code asm("%31"); \
					  (Xt *)((int)_does_code & (-4)); })
	/* HPPA uses register 2 for branch and link */

	/* this stores a call dodoes at addr */
#	define MAKE_DOESJUMP(addr)	MAKE_CFA((addr),symbols[3])
#endif

/* OS dependences */

#define SEEK_SET 0
#define rint(x)	floor((x)+0.5)

#ifdef DIRECT_THREADED
#	define CPU_DEP  register Label branchto asm("%5")=symbols[0];
#	define CPU_DEP2 &&deadcode
#	define CPU_DEP3 deadcode: return((Label *)branchto);
#endif
