/*
  $Id: sparc.h,v 1.2 1994-11-29 16:22:49 pazsan Exp $
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for a SPARC running SunOS
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
#endif

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

/* OS dependences */

#define SEEK_SET 0

#define memmove(a,b,c) ({ if((long)(a)<(long)(b)) memcpy(a,b,c); \
			  else \
			    {  int i; \
			       for(i=(c)-1; i>=0; i--) \
				  (char*)a[i]=(char*)b[i]; \
			    } \
			})
#define strtoul(a,b,c) strtol(a,b,c)

