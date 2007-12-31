/* cache flushing for the HP-PA architecture

  Copyright (C) 1995,1996,1997,1998,2003,2007 Free Software Foundation, Inc.

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

#include <stddef.h>

void cacheflush(void * address, size_t size, size_t linewidth)
{
  void *p=(void *)((size_t)address & (-linewidth));

  for(; p<address+size;)
    asm volatile("fdc (%0)\n\t"
		 "sync\n\t"
		 "fic,m %1(%0)\n\t"
		 "sync" : "+r"(p) : "r"(linewidth) : "memory" );
}

#if 0
void cacheflush(void * address, int size, int linewidth)
{
  int i;

  address=(void *)((int)address & (-linewidth));

  for(i=1-linewidth; i<size; i+=linewidth)
    asm volatile("fdc (%0)\n\t"
		 "sync\n\t"
		 "fic,m %1(%0)\n\t"
		 "sync" : : "r" (address), "r" (linewidth) : "memory" );
}
#endif
