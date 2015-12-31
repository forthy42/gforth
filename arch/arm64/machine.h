/*
  This is the machine-specific part for ARM processors

  Copyright (C) 2000,2003,2004,2005,2007,2008,2011,2014,2015 Free Software Foundation, Inc.

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
#define USE_FTOS
#endif
#endif

#include "../generic/machine.h"
#include <sys/types.h>

/* this calls a dummy function in cacheflush0.S */
/* you can replace it through "./configure arm_cacheflush=<file>" */
/* if you know how to flush the icache on the arm in general, mail me */
#define FLUSH_ICACHE(addr,size) gforth_cacheflush(addr,size)
void gforth_cacheflush(void *p, size_t size);

#if defined(FORCE_REG) && !defined(DOUBLY_INDIRECT) && !defined(VM_PROFILING)
/* 31 64-bit general purpose registers R0-R30:
   R30		LR (link register)
   R29		FP (frame pointer)
   R19-R28	Callee-saved registers
   R18		The platform register; use as temporary register.
   R17		IP1 The second intra-procedure-call temporary register
		(can be used by call veneers and PLT code); otherwise use
		as a temporary register
   R16		IP0 The first intra-procedure-call temporary register (can
		be used by call veneers and PLT code); otherwise use as a
		temporary register
   R9-R15	Temporary registers
   R8		Structure value parameter / temporary register
   R0-R7	Parameter/result registers
   SP		stack pointer, encoded as X/R31 where permitted.
   ZR		zero register, encoded as X/R31 elsewhere
   32 x 128-bit floating-point/vector registers
   V16-V31	Caller-saved (temporary) registers
   V8-V15	Callee-saved registers
   V0-V7	Parameter/result registers
   The vector register V0 holds scalar B0, H0, S0 and D0 in its least
   significant bits.  Unlike AArch32 S1 is not packed into D0,
   etc.  */

/* untested registers, better not define any
#define RPREG asm("x28")
#define SPREG asm("x27")
#define FPREG asm("x26")
#define LPREG asm("x25")
*/
#endif
