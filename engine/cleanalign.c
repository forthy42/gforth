/* alignment-clean replacements for library functions

  Copyright (C) 1995,1997,2000,2003,2007 Free Software Foundation, Inc.

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

void *memcpy(void *dest, const void *src, int n)
{
  int i;
  char *s, *t;
  for (s=dest, t=src, i=0; i<n; i++)
    *s++=*t++;
  return dest;
}

char *memmove(char *dest, const char *src, long n)
{
  int i;

  if (dest<src)
    for (i=0; i<n; i++)
      dest[i]=src[i];
  else
    for(i=n-1; i>=0; i--)
      dest[i]=src[i];
  return dest;
}
