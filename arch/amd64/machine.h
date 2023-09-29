/*
  This is the machine-specific part for the AMD64 (née x86-64) architecture.

  Authors: Anton Ertl, Bernd Paysan
  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2011,2013,2014,2015,2016,2018,2019,2020 Free Software Foundation, Inc.

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
#if (__GNUC__<4 || (__GNUC==4 && __GNUC_MINOR__ < 2))
#define BUGGY_LL_D2F    /* to float not possible */
#define BUGGY_LL_F2D    /* from float not possible */
#endif
#define BUGGY_LL_SIZE   /* long long "too short", so we use something else */

#endif

#define ASM_SM_SLASH_REM(d1lo, d1hi, n1, n2, n3) \
	asm("idivq %4": "=a"(n3),"=d"(n2) : "a"(d1lo),"d"(d1hi),"g"(n1):"cc");

#define ASM_UM_SLASH_MOD(d1lo, d1hi, n1, n2, n3) \
	asm("divq %4": "=a"(n3),"=d"(n2) : "a"(d1lo),"d"(d1hi),"g"(n1):"cc");
#define DIVISION_SIGNAL

#include "../generic/machine.h"

/* The architecture requires hardware consistency */
#ifndef FLUSH_ICACHE
# define FLUSH_ICACHE(addr,size)
#endif

/* globals are accessed in a PC-relative way and therefore make
   primitives that access them nonrelocatable.  If GLOBALS_NONRELOC is
   defined, the engine accesses these variables through a local. */
/* #define GLOBALS_NONRELOC 1 */
/* The effect of GLOBALS_NONRELOC is as follows (gcc-4.2.0):
    3GHz Xeon 5160              2.2GHz Athlon 64 X2   
sieve bubble matrix  fib    sieve bubble matrix  fib   GLOBALS_NONRELOC
 0.304 0.412  0.200 0.668    0.608 0.720  0.396 0.792      defined
 0.284 0.388  0.176 0.472    0.588 0.760  0.860 0.884    undefined
The problem seems to be that the local is in memory, even with
explicit register allocation and efforts to stop coalescing.
*/

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
/* tested with gcc 4.4, 4.8, 4.9, 7.4, 8.3 */

/* If the compiler allocates a register to the variable by itself, it
   tends to produce fewer instructions (in particular, fewer mov
   instructions); the compiler tends to allocate a register to
   frequently-used variables like ip, sp, spTOS, rp, so we don't
   define explicit registers for them.  gcc-8.3 fails to allocate
   registers to lp and FTOS, possibly because they occur in too few
   places in engine().  So we allocate registers for them explicitly.
   We also allocate a register for fp explicitly, because if FTOS does
   not occur frequently enough, fp seems to be endangered, too */

#define FPREG asm("%r12")
#if ((__GNUC__==4 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=6) || (__GNUC__>=5))
#define LPREG asm("%rbp") /* inefficient with gcc-4.4 */
#endif
#define FTOSREG asm("%xmm15")
#ifdef __clang__
/* maybe we need some other options for clang */
/* but so far, clang doesn't support manual register allocation */
#endif

/* Results of explicit register allocation

for i in 4.4 4.9 7.4 8.3; do echo $i; perf stat -x' ' -e cycles:u -e instructions:u gforth-fast-reg-$i onebench.fs >/dev/null; perf stat -x' ' -e cycles:u -e instructions:u gforth-fast-newreg-$i onebench.fs >/dev/null; done

For various gcc versions; the upper results are with the old explicit
register allocation (with explicit registers for ip, sp, spTOS, rp),
the lower results with the present one:

Haswell     Skylake     Zen2        Goldmont                 
4.4         4.4         4.4         4.4                      
1471993782  1416141953  1331162435  2338608502 cycles:u      
3829917395  3829922109  3828147564  3829372879 instructions:u
1431364615  1336941234  1274677422  2228754145 cycles:u      
3372490900  3372496012  3370798243  3371941456 instructions:u
4.9         4.9         4.9         4.9                      
1388148914  1309576145  1242016559  2169850504 cycles:u      
3295249015  3295190837  3293099054  3294788843 instructions:u
1403757388  1327685590  1267331033  2185304203 cycles:u      
3400541439  3400536165  3398124383  3400032204 instructions:u
7.4         7.4         7.4         7.4                      
1433156560  1339449495  1252978107  2212397812 cycles:u      
3376331020  3376264573  3374174822  3375864973 instructions:u
1411925157  1325356723  1261224513  2137450203 cycles:u      
3216957194  3216899609  3214914874  3216485013 instructions:u
8.3         8.3         8.3         8.3                      
1386077004  1309633582  1239162675  2170227276 cycles:u      
3295249024  3295190826  3293098814  3294788948 instructions:u
1386562707  1307231176  1253242147  2152653048 cycles:u      
3204296317  3204244893  3202150063  3203823888 instructions:u

Dynamic native code size (note that they are only comparable if the
same primitives are relocatable):

for i in 4.4 4.9 7.4 8.3; do echo $i `gforth-fast-reg-$i --print-metrics -e bye 2>&1 |awk '/code size/ {print $4}'` `gforth-fast-newreg-$i --print-metrics -e bye 2>&1 |awk '/code size/ {print $4}'`; done
    old    present
4.4 587607 522052
4.9 503129 500224
7.4 506729 463706
8.3 503129 472987

Number of non-relocatable primitives:
for i in 4.4 4.9 7.4 8.3; do echo $i `gforth-fast-reg-$i --debug -e bye 2>&1 |grep non_reloc|wc -l` `gforth-fast-newreg-$i --debug -e bye 2>&1 |grep non_reloc|wc -l`; done
    old pres
4.4 152 151
4.9 117 149
7.4 117 117
8.3 117 117

The differences in primitives between old and prev with gcc-4.9 come
from uw@ and friends.  With the new register allocation, gcc-4.9
actually compiles the call to memcpy into a call to memcpy.
*/
#endif

#define GOTO_ALIGN asm(".p2align 4,,7");
/* GCC 12 and further combine 2! into one move through an xmm register
   which defeats the store-to-load facility of modern amd64 processors.
   I.e. it looks nice on paper, but actually is a lot slower. */
