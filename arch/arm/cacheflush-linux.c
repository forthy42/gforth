/*
  ARM icache flush support using Linux syscall.  

  Copyright (C) 2000,2008 Free Software Foundation, Inc.

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

  If this compiles without error, cacheflush() should be guaranteed to
  work.  The nice thing about using linux/unistd.h for the syscall
  implementation is, that it uses the correct syscall ABI for the
  system.  Yes, on ARM there are at least two different ABIs out
  there, which can be selected at kernel compile time.
*/

void cacheflush(void *p, size_t size) 
{
   /* For details, please see the Linux sources, files
    * arch/arm/kernel/traps.c, arch/arm/kernel/entry-common.S and
    * asm-arm/unistd.h
    * 
    * This syscall is supported by all 2.4 and 2.6 Linux kernels.  2.2 linux
    * kernels got first support with version 2.2.18.
    */

   asm("mov r0, %0\n"
       "mov r1, %1\n"
       "mov r2, #0\n"
#    if defined (__ARM_EABI__) || defined (__thumb__)
       /* EABI or Thumb syscall: syscall number passed in 'r7' (syscall base
	* number is 0x0).  Note that Thumb and EABI are generally not the
	* same.  It just happens that the simple cacheflush syscall doesn't
	* expose any of differences in calling conventions.
        */
       "mov r7, #0xf0002\n"
       "swi #0\n"      
#    else
       /* OABI syscall: syscall number passed as part of 'swi' instruction,
	* base number is 0x900000 */
       "swi #0x9f0002\n" ::
#     endif
       "g"(p), "g"((long)p+size) :
       "r0", "r1", "r2", "r7");
}

