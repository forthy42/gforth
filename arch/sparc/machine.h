/* This is the machine-specific part for a SPARC

  Copyright (C) 1995,1996,1997,1998,2000,2003,2005,2007,2008 Free Software Foundation, Inc.

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

#include "../generic/machine.h"

#define FLUSH_ICACHE(addr,size) \
  ({void *_addr=(addr); void *_end=_addr+((Cell)(size)); \
    for (_addr=(void *)(((long)_addr)&~7); _addr<_end; _addr += 8) \
       asm("iflush %0+0"::"r"(_addr)); \
   })
/* the +0 in the iflush instruction is needed by gas */
