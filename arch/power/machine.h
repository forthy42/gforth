/* This is the machine-specific part for the Power (incl. PPC) architecture

  Copyright (C) 1995,1996,1997,1998,2000,2003 Free Software Foundation, Inc.

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

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#include "../generic/machine.h"
#include <sys/types.h>

#ifndef THREADING_SCHEME
#ifdef DIRECT_THREADED
#define THREADING_SCHEME 5
#else
#define THREADING_SCHEME 6
#endif
#endif

extern void _sync_cache_range (caddr_t eaddr, size_t count);
#define FLUSH_ICACHE(addr,size)   _sync_cache_range(addr,size)
