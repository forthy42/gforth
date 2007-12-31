/*
  cacheflush function for many ARM-Linux machines

  Copyright (C) 2007 Free Software Foundation, Inc.

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

/* adapted from
   http://b2.complang.tuwien.ac.at/cgi-bin/viewcvs.cgi/cacao/trunk/src/vm/jit/arm/asmpart.S?rev=7325&view=markup 
   this version works on iyonix (Xscale IOP 321 with 2.4.22 kernel) */
/* use this through
   ./configure arm_cacheflush=arch/arm/cacheflush2
 */
#include <stdlib.h>
void cacheflush(void *p, size_t size)
{
  asm("mov r0, %0; mov r1, %1; mov r2, #0; swi #0x9f0002" ::
      "g"(p), "g"(p+size):
      "r0", "r1", "r2");
}
