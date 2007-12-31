/* DEC Alpha

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

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#define FLUSH_ICACHE(addr,size)		asm("call_pal 0x86") /* imb (instruction-memory barrier) */

#include "../generic/machine.h"

/* code padding */
#define CODE_ALIGNMENT 16
#define CODE_PADDING {0x1f, 0x04, 0xff, 0x47, 0x00, 0x00, 0xfe, 0x2f, \
                      0x1f, 0x04, 0xff, 0x47, 0x00, 0x00, 0xfe, 0x2f}
#define MAX_PADDING 12

#ifdef FORCE_REG
/* $9-$14 are callee-saved, $1-$8 and $22-$25 are caller-saved */
#define IPREG asm("$10")
#define SPREG asm("$11")
#define RPREG asm("$12")
#define LPREG asm("$13")
#define TOSREG asm("$14")
/* #define CFAREG asm("$22") egcs-1.0.3 crashes with any caller-saved
   register decl */
#endif /* FORCE_REG */
