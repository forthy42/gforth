/*
  This is the machine-specific part for Intel ia64 compatible processors

  Copyright (C) 2000,2003,2004 Free Software Foundation, Inc.

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

#ifndef THREADING_SCHEME

#define THREADING_SCHEME 8
#endif

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

#if 0
/* if you know how to flush the icache on the arm, mail me */
extern void flush_icache_block(caddr_t eaddr, size_t count);
#define FLUSH_ICACHE(addr,size) flush_icache_block(addr, size)
#endif

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
