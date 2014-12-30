/* a memcmp implementation

  Copyright (C) 1995,1998,2000,2003,2007,2014 Free Software Foundation, Inc.

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

#include "forth.h"

int memcmp(const void *s1, const void *s2, size_t n)
{
  Cell i;

  for (i=0; i<n; i++)
    if (((Char*)s1)[i] != ((Char*)s2)[i])
      return ((Char*)s1)[i]-((Char*)s2)[i];
  return 0;
}
