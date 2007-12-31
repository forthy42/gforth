/* This is the machine-specific part for the 68000 and family

  Copyright (C) 1995,1996,1997,1998,2000,2003,2005,2007 Free Software Foundation, Inc.

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
#elif defined(__NetBSD__)
#  define FLUSH_ICACHE(addr,size)	do {				\
		register void *addr_ asm("a1") = (addr);		\
		register int size_ asm("d1") = (size);			\
		register int cmd_ asm("d0") = 0x80000004;		\
		asm volatile("	trap	#12"				\
			: "=a" (addr_), "=d" (size_), "=d" (cmd_)	\
			: "0" (addr_), "1" (size_), "2" (cmd_) : "a0");	\
	} while (0)
#elif defined(NeXT) || defined(sun)
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
