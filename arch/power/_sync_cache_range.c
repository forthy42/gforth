/* cache flushing for the PPC

  Copyright (C) 1998 Free Software Foundation, Inc.

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

#include <stddef.h>
#include <sys/types.h>

/* the name is from an AIX (4.3) call (thanks to Dan Prener
   <prener@watson.ibm.com> for this information) */
void _sync_cache_range(caddr_t addr, size_t size)
{
  size_t cache_block_size=32;
  caddr_t p=(caddr_t)(((long)addr)&-cache_block_size);

  /* this works for a single-processor PPC 604e, but may have
     portability problems for other machines; the ultimate solution is
     a system call, because the architecture is pretty shoddy in this
     area */
  for (; p < (addr+size); p+=cache_block_size)
    asm("dcbst 0,%0; sync; icbi 0,%0"::"r"(p));
  asm("sync; isync"); /* PPC 604e needs the additional sync
			 according to Tim Olson */
}
