/* preliminary machine file for DEC Alpha

  Copyright (C) 1995 Free Software Foundation, Inc.

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
  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/

/* Be careful: long long on Alpha are 64 bit :-(( */
#define LONG_LATENCY

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#ifdef DIRECT_THREADED
#warning direct threading not supported on the Alpha (yet)
#undefine DIRECT_THREADED
#endif

#define FLUSH_ICACHE(addr,size)		asm("call_pal 0x86") /* imb */

#include "32bit.h"
