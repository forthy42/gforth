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

/* syscall macros not exported, unless __KERNEL__ set :( */
#define __KERNEL__
#include <linux/unistd.h>

#include <stddef.h>
#include <errno.h>

/* need __NR_ prefixed syscall number for syscall macros to work
   correctly */
#define __NR_ARM_cacheflush __ARM_NR_cacheflush	

static _syscall3(void, ARM_cacheflush, 
	  long, start, long, end, unsigned long, flags);

void cacheflush(void *p, size_t size) {
  ARM_cacheflush ((long)p, (long)p + size, 0);
}

