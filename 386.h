/*
  Copyright 1992 by the ANSI figForth Development Group

  This is the machine-specific part for Intel 386 compatible processors
*/

#include "32bit.h"

/* indirect threading is faster on the 486, on the 386 direct
   threading is probably faster. Therefore we leave defining
   DIRECT_THREADED to configure */

/* define this if the processor cannot exploit instruction-level
   parallelism (no pipelining or too few registers) */
#define CISC_NEXT

/* 386 and below have no cache, 486 has a shared cache, and the
   Pentium probably employs hardware cache consistency, so
   flush-icache is a noop */
#define FLUSH_ICACHE(addr,size)

#ifdef DIRECT_THREADED

#define CALL 0xe8 /* call */
#define JMP  0xe9 /* jmp  */
#define GETCFA(reg)  ({ asm("popl %0" : "=r" (reg)); (int)reg -= 5;});

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
			  *(char *)_cfa = CALL; \
			  *(long *)(_cfa+1) = _ca-(_cfa+5);})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_HANDLER_SIZE       8
#define DOES_CODE(label)	((Xt *)(CODE_ADDRESS(label)+DOES_HANDLER_SIZE))

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	DOES_CODE(label)

/* this stores a jump dodoes at addr */
#define MAKE_DOES_CF(addr,doesp)	({long _addr = (long)(addr); \
                          long _doesp  = (long)(doesp)-8; \
			  *(char *)_addr = CALL; \
			  *(long *)(_addr+1) = _doesp-(_addr+5);})

#define MAKE_DOES_HANDLER(addr)	({long _addr = (long)(addr); \
                          long _dodo  = (long)symbols[DODOES]; \
			  *(char *)_addr = JMP; \
			  *(long *)(_addr+1) = _dodo-(_addr+5);})
#endif

#ifdef FORCE_REG
#if (__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__==5)
/* i.e. gcc-2.5.x */
/* this works with 2.5.7; nothing works with 2.5.8 */
#define IPREG asm("%esi")
#define SPREG asm("%edi")
#ifdef USE_TOS
#define CFAREG asm("%ecx")
#else
#define CFAREG asm("%edx")
#endif
#else /* gcc-version */
/* this works with 2.6.3 (and quite well, too) */
/* since this is not very demanding, it's the default for other gcc versions */
#if defined(USE_TOS) && !defined(CFA_NEXT)
#define IPREG asm("%ebx")
#else
#define SPREG asm("%ebx")
#endif /* USE_TOS && !CFA_NEXT */
#endif /* gcc-version */
#endif /* FORCE_REG */
