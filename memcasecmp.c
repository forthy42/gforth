/* case insensitive memory block comparison

  Copyright (C) 1996 Free Software Foundation, Inc.

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

/* this is mainly useful for testing for equality; to get a version
   that delivers the right < and > results in any locale, you would
   have to work with strcoll and some hackery */

#include <ctype.h>

int memcasecmp(const char *s1, const char *s2, long n)
{
  int i;

  for (i=0; i<n; i++) {
    char c1=toupper(s1[i]);
    char c2=toupper(s2[i]);
    if (c1 != c2)
      return c1-c2;
  }
  return 0;
}
