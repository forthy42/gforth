/* This is the machine-specific part for the Power (incl. PPC) architecture

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
#include <sys/types.h>

extern void _sync_cache_range (caddr_t eaddr, size_t count);
#define FLUSH_ICACHE(addr,size)   _sync_cache_range(addr,size)
