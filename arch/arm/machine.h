/*
  This is the machine-specific part for ARM processors

  Copyright (C) 2000,2003,2004,2005,2007,2008 Free Software Foundation, Inc.

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
#define FLUSH_ICACHE(addr,size) cacheflush(addr,size)
void cacheflush(void *p, size_t size);

#if defined(FORCE_REG) && !defined(DOUBLY_INDIRECT) && !defined(VM_PROFILING)
/*
according to http://mail-index.netbsd.org/port-arm/2003/05/17/0000.html:

R0-R3: argument passing/caller-saved
R4-R10: callee-saved
R12, R14: caller-saved
R11: frame pointer
R13: stack pointer
*/
/* works with gcc-2.95.2 */
#define RPREG asm("r7")
#endif
