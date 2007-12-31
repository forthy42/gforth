/* rint replacement

  Copyright (C) 2002,2007 Free Software Foundation, Inc.

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

#ifdef i386
#define X (1024.*1024.*1024.*1024.*1024.*1024.*16.)
#else /* !defined(386) */
#define X (1024.*1024.*1024.*1024.*1024.*8.)
#endif /* !defined(386) */
double rint(double r)
{
  if (r<0.0)
    return (r+X)-X;
  else
    return (r-X)+X;
}

