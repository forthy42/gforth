/*
  This is the machine-specific part for Intel 386 compatible processors

  Copyright (C) 1995,1996,1997,1998,2000 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.
*/

#ifdef HAVE_LIBKERNEL32
#ifdef i386
#define SYSCALL     __attribute__ ((stdcall))
#endif
#endif

#ifndef THREADING_SCHEME
#define THREADING_SCHEME 8
#endif

#if ((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=95) || (__GNUC__>2))
#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif
#endif
#define MEMCMP_AS_SUBROUTINE 1

#ifndef USE_FTOS
#ifndef USE_NO_FTOS
/* gcc can't keep TOS in a register on the 386, so don't try */
#define USE_NO_FTOS
#endif
#endif

#include "../generic/machine.h"

/* indirect threading is faster on the Pentium, on the 486 direct
   threading is faster. Therefore we leave defining
   DIRECT_THREADED to configure */

/* define this if the processor cannot exploit instruction-level
   parallelism (no pipelining or too few registers) */
#define CISC_NEXT

/* 386 and below have no cache, 486 has a shared cache, and the
   Pentium probably employs hardware cache consistency, so
   flush-icache is a noop */
#define FLUSH_ICACHE(addr,size)

#if 0

#define CALL 0xe8 /* call */
#define JMP  0xe9 /* jmp  */
#define GETCFA(reg)  ({ asm("popl %0" : "=r" (reg)); (int)reg -= 5;});

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* a special version of CODE_ADDRESS for DOES_CODE1 and DOES_CODE */
#define CODE_ADDRESS1(cfa) \
    ({long _cfa1 = (long)(cfa); (Label)(_cfa1+*((long *)(_cfa1+1))+5);})

/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa) \
    ({long _cfa = (long)(cfa); \
       (((*(unsigned char *)_cfa)==CALL) ? \
       CODE_ADDRESS1(_cfa) : \
       (Label)(_cfa));})

/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
#define MAKE_CF(cfa,ca)	({long _cfa = (long)(cfa); \
                          long _ca  = (long)(ca); \
			  *(char *)_cfa = CALL; \
			  *(long *)(_cfa+1) = _ca-(_cfa+5);})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_CODE(xt) \
({ long _xt = (long)(xt); \
   long _ca = (long)(CODE_ADDRESS(_xt)); \
   ((((*(unsigned char *)_xt) == CALL) \
     && ((*(unsigned char *)_ca) == JMP) \
     && ((long)(CODE_ADDRESS1(_ca)) == (long)&&dodoes)) \
    ? _ca+DOES_HANDLER_SIZE : 0L); })

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)      (CODE_ADDRESS1(label)+DOES_HANDLER_SIZE)

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

/* dynamic superinstruction stuff */
#define INST_GRANULARITY 1
#define IND_JUMP_LENGTH 3
#define IS_NEXT_JUMP(_addr) (((*(Cell *)(symbols1[i]+j))&0xfff8ff) == 0xfc60ff)
	/* jmp -4(reg), i.e., the NEXT jump */

#ifdef FORCE_REG
#if (__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__==5)
/* i.e. gcc-2.5.x */
/* this works with 2.5.7; nothing works with 2.5.8 */
#define IPREG asm("%esi")
#define SPREG asm("%edi")
#if 0
#ifdef USE_TOS
#define CFAREG asm("%ecx")
#else
#define CFAREG asm("%edx")
#endif
#endif
#else /* gcc-version */
/* this works with 2.6.3 (and quite well, too) */
/* since this is not very demanding, it's the default for other gcc versions */
#if defined(USE_TOS) && !defined(CFA_NEXT)
#if ((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=95) || (__GNUC__>2))
     /* gcc 2.95 has a better register allocater */
#define SPREG asm("%esi")
#define RPREG asm("%edi")
#if (__GNUC__>2)
#define IPREG asm("%ebx")
/* ebp leads to broken code; eax, ecx, edx produce compile errors */
#define TOSREG asm("%ecx")
/* ecx works only for TOS, and eax, edx don't work for anything */
#else
/* gcc-2.95 manages to fit ip into ebp by itself */
#define TOSREG asm("%ebx")
#endif
#else /* gcc-2.95 or later */
#define IPREG asm("%ebx")
#endif
#else
#if ((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=95) || (__GNUC__>2))
#define SPREG asm("%esi")
#define RPREG asm("%edi")
#if (__GNUC__>2)
#define IPREG asm("%ebx")
#endif
#else
#define SPREG asm("%ebx")
#endif
#endif /* USE_TOS && !CFA_NEXT */
#endif /* gcc-version */
#endif /* FORCE_REG */

/* #define ALIGNMENT_CHECK 1 */
