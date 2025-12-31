/* a memmove implementation

  Authors: Anton Ertl, Bernd Paysan
  Copyright (C) 1995,1998,2000,2003,2007,2017,2019,2025 Free Software Foundation, Inc.

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

#include "config.h"
#include "forth.h"

#if defined(_AIX) && defined(_NOTHROW)
void *_NOTHROW(memmove, (void *dest, const void *src, size_t n))
#else
void *memmove(void *dest, const void *src, Cell n)
#endif
{
  Cell i;

  if (dest<src)
    for (i=0; i<n; i++)
      ((Char*)dest)[i]=((Char*)src)[i];
  else
    for(i=n-1; i>=0; i--)
      ((Char*)dest)[i]=((Char*)src)[i];
  return dest;
}
