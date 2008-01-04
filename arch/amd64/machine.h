/*
  This is the machine-specific part for the AMD64 (née x86-64) architecture.

  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#ifndef USE_FTOS
#ifndef USE_NO_FTOS
#if 1 || defined(FORCE_REG)
#define USE_FTOS
#else
#define USE_NO_FTOS
#endif
#endif
#endif

#ifdef FORCE_LL
#define BUGGY_LL_D2F    /* to float not possible */
#define BUGGY_LL_F2D    /* from float not possible */
#define BUGGY_LL_SIZE   /* long long "too short", so we use something else */

#endif

#define ASM_SM_SLASH_REM(d1lo, d1hi, n1, n2, n3) \
	asm("idivq %4": "=a"(n3),"=d"(n2) : "a"(d1lo),"d"(d1hi),"g"(n1):"cc");

#define ASM_UM_SLASH_MOD(d1lo, d1hi, n1, n2, n3) \
	asm("divq %4": "=a"(n3),"=d"(n2) : "a"(d1lo),"d"(d1hi),"g"(n1):"cc");

#include "../generic/machine.h"

/* The architecture requires hardware consistency */
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
#define RPREG asm("%r13")
#define FPREG asm("%r12")
#define TOSREG asm("%r14")
#define SPREG asm("%r15")
#define IPREG asm("%rbx")
#if 0
#define LPREG asm("%rbp") /* doesn't work now */
#endif
#define FTOSREG asm("%xmm8")
#endif

#define GOTO_ALIGN asm(".p2align 4,,7");
