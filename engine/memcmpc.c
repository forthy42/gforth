/* case insensitive memory block comparison

  Copyright (C) 1996,1998 Free Software Foundation, Inc.

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
*/

/* this is mainly useful for testing for equality; to get a version
   that delivers the right < and > results in any locale, you would
   have to work with strcoll and some hackery */

#include "forth.h"
#include <ctype.h>

Cell memcasecmp(const Char *s1, const Char *s2, Cell n)
{
  Cell i;

  for (i=0; i<n; i++) {
    Char c1=toupper(s1[i]);
    Char c2=toupper(s2[i]);
    if (c1 != c2) {
      if (c1 < c2)
	return -1;
      else
	return 1;
    }
  }
  return 0;
}
