/* This is the machine-specific part for the 68000 and family

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

#ifndef THREADING_SCHEME
#define THREADING_SCHEME 3
#endif

/* define this if the processor cannot exploit instruction-level
   parallelism (no pipelining or too few registers) */
#define CISC_NEXT

#if !defined USE_NO_TOS && !defined USE_TOS
#define USE_TOS
#endif

#include "../generic/machine.h"
#include <sys/types.h>

/* Clearing the whole cache is a bit drastic, but this is the only
 *    cache control available on the apollo and NeXT
 */
#if defined(apollo)
#  define FLUSH_ICACHE(addr,size)    cache_$clear()
#elif defined(NeXT)
#  define FLUSH_ICACHE(addr,size)     asm("trap #2");
#elif defined(hpux)
#  include <sys/cache.h>
#  define FLUSH_ICACHE(addr,size) cachectl(CC_IPURGE,(addr),(size))
#elif defined(linux)
#include <asm/cachectl.h>
extern int cacheflush(void *, int, int, size_t);
#define FLUSH_ICACHE(addr,size) \
  cacheflush(addr, FLUSH_SCOPE_LINE, FLUSH_CACHE_INSN, (size_t)(size) + 15)
#elif defined(amigaos)
#  define FLUSH_ICACHE(addr,size) \
  asm(" move.l a6,-(sp); \
        move.l _SysBase,a6; \
        jsr -636(a6); \
        move.l (sp)+,a6; \
  ")
#else
#  warning no FLUSH_ICACHE defined.  Dynamic native code generation disabled.
#  warning CODE words will not work.
#endif

#ifdef FORCE_REG /* highly recommended */
#if defined(amigaos)
#  define IPREG asm("%a6")
#else
#  define IPREG asm("%a5")
#endif
#define SPREG asm("%a4")
#define RPREG asm("%a3")
#define CFAREG asm("%a2")
#define TOSREG asm("%d3")
#define LPREG asm("%d2")
#endif
