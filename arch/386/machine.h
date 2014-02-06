/*
  This is the machine-specific part for Intel 386 compatible processors

  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2012,2013 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.
*/

#ifdef HAVE_LIBKERNEL32
#ifdef i386
#define SYSCALL     __attribute__ ((stdcall))
#endif
#endif

#if (((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=95) || (__GNUC__==3))) && defined(FORCE_REG)
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

#define ASM_SM_SLASH_REM(d1lo, d1hi, n1, n2, n3) \
	asm("idivl %4": "=a"(n3),"=d"(n2) : "a"(d1lo),"d"(d1hi),"g"(n1):"cc");

#define ASM_UM_SLASH_MOD(d1lo, d1hi, n1, n2, n3) \
	asm("divl %4": "=a"(n3),"=d"(n2) : "a"(d1lo),"d"(d1hi),"g"(n1):"cc");

/* 386 and below have no cache, 486 has a shared cache, and the
   Pentium and later employ hardware cache consistency, so
   flush-icache is a noop */
#define FLUSH_ICACHE(addr,size)

/* code padding */
#define CODE_ALIGNMENT 16
#define CODE_PADDING {0x66, 0x66, 0x66, 0x90, 0x66, 0x66, 0x66, 0x90, \
                      0x66, 0x66, 0x66, 0x90, 0x66, 0x66, 0x66, 0x90}
#define MAX_PADDING 4
/* results for various maxpaddings:
   3GHz Xeon 5160                     2.2GHz Athlon 64 X2
   sieve bubble matrix  fib  padding sieve bubble matrix  fib 
    0.132 0.216  0.072 0.228    0     0.260 0.300  0.108 0.344
    0.132 0.216  0.072 0.228    1     0.268 0.300  0.112 0.344
    0.132 0.216  0.072 0.248    2     0.256 0.300  0.108 0.344
    0.136 0.216  0.072 0.248    3     0.252 0.300  0.108 0.344
    0.132 0.220  0.072 0.240    4     0.252 0.300  0.112 0.340
    0.136 0.216  0.072 0.248    5     0.252 0.300  0.108 0.344
    0.132 0.216  0.072 0.244    6     0.256 0.300  0.108 0.344
    0.132 0.216  0.072 0.244    7     0.264 0.300  0.108 0.344
    0.136 0.216  0.072 0.244    8     0.268 0.296  0.108 0.340
*/

#if defined(FORCE_REG) && !defined(DOUBLY_INDIRECT) && !defined(VM_PROFILING)
# if (__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__==5)
/* i.e. gcc-2.5.x */
/* this works with 2.5.7; nothing works with 2.5.8 */
#  define IPREG asm("%esi")
#  define SPREG asm("%edi")
#  if 0
#   ifdef USE_TOS
#    define CFAREG asm("%ecx")
#   else
#    define CFAREG asm("%edx")
#   endif
#  endif
# else /* !gcc-2.5.x */
/* this works with 2.6.3 (and quite well, too) */
/* since this is not very demanding, it's the default for other gcc versions */
#  if defined(USE_TOS) && !defined(CFA_NEXT)
#   if ((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=95) || (__GNUC__==3))
     /* gcc 2.95 has a better register allocater */
#    define SPREG asm("%esi")
#    define RPREG asm("%edi")
#    ifdef NO_IP
#     define spbREG asm("%ebx")
#    else
#     define IPREG asm("%ebx")
#    endif
/* ebp leads to broken code (gcc-3.0); eax, ecx, edx produce compile errors */
#    define TOSREG asm("%ecx")
/* ecx works only for TOS, and eax, edx don't work for anything (gcc-3.0) */
#   else /* !(gcc-2.95 or gcc-3.x) */
#    if (__GNUC__==4 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=2)
#     if defined(PIC) || defined(__ANDROID__)
#      define SPREG asm("%esi")
#      define IPREG asm("%edi")
#     else
#      ifndef __APPLE__
#       define IPREG asm("%ebx")
#       define SPREG asm("%esi")
#       define RPREG asm("%edi")
#       if(__GNUC_MINOR__>=6)
#        define TOSREG asm("%ebp")
#       else
#        define TOSREG asm("%ecx")
#        define TOS_CLOBBERED
#       endif
#      else
#       define IPREG asm("%edi")
#       define SPREG asm("%esi")
#       if(__GNUC_MINOR__>=6)
#        define TOSREG asm("%ebp")
#       else
#        define TOSREG asm("%ecx")
#        define TOS_CLOBBERED
#       endif
#      endif
#     endif
#    endif /* (gcc-4.2 or later) */
#   endif /* !(gcc-2.95 or later) */
#  else /* !defined(USE_TOS) || defined(CFA_NEXT) */
#   if ((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=95) || (__GNUC__>2))
#    define SPREG asm("%esi")
#    define RPREG asm("%edi")
#    ifdef NO_IP
#     define spbREG asm("%ebx")
#    else
#     define IPREG asm("%ebx")
#    endif
#   else /* !(gcc-2.95 or later) */
#    define SPREG asm("%ebx")
#   endif  /* !(gcc-2.95 or later) */
#  endif /* !defined(USE_TOS) || defined(CFA_NEXT) */
# endif /* !gcc-2.5.x */
#endif /* defined(FORCE_REG) && !defined(DOUBLY_INDIRECT) && !defined(VM_PROFILING) */

/* #define ALIGNMENT_CHECK 1 */

#if defined(USE_TOS) && defined(TOS_CLOBBERED)
#define CLOBBER_TOS_WORKAROUND_START sp[0]=spTOS; __asm__ __volatile__ ("" ::: "memory");
#define CLOBBER_TOS_WORKAROUND_END   __asm__ __volatile__ ("" ::: "memory"); spTOS=sp[0];
#endif

#include "../generic/machine.h"
