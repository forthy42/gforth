/*
  dummy cacheflush function (does nothing)

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

/* if you know how to do cache flushing for your platform, put it in a
   file cacheflush.c here and call configure like this:
   ./configure arm_cacheflush=arch/arm/cacheflush
 */
#include <stdlib.h>
void cacheflush(void *p, size_t size)
{
}
