/* This is the machine-specific part for Loongson 3 processors

  Authors: Bernd Paysan
  Copyright (C) 2025 Free Software Foundation, Inc.

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

#if !defined(USE_FTOS) && !defined(USE_NO_FTOS)
#define USE_FTOS
#endif

/* cache flush stuff */
#include "../generic/machine.h"

#ifdef FORCE_REG
#define IPREG asm("$s0")
#define SPREG asm("$s1")
#define RPREG asm("$s2")
#define LPREG asm("$s3")
#define CFAREG asm("$s4")
#define TOSREG asm("$s5")
#define OPREG asm("$s6")
#define FPREG asm("$s7")
#define FTOSREG asm("$f24")
#endif /* FORCE_REG */
