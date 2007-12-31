/* replacement for asinh, acosh, and atanh */

/* 
  Copyright (C) 1996,2000,2003,2007 Free Software Foundation, Inc.

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

#include <math.h>

double atanh(double r1)
{
  double r2=r1 < 0 ? -r1 : r1;
  double r3=log((r2/(1.0-r2)*2)+1)/2;

  return r1 < 0 ? -r3 : r3;
  /* fdup f0< >r fabs 1. d>f fover f- f/  f2* flnp1 f2/
     r> IF  fnegate  THEN ;
     */
}

double asinh(double r1)
{
  return atanh(r1/sqrt(1.0+r1*r1));
  /* fdup fdup f* 1. d>f f+ fsqrt f/ fatanh ; */
}

double acosh(double r1)
{
  return(log(r1+sqrt(r1*r1-1.0)));
  /* fdup fdup f* 1. d>f f- fsqrt f+ fln ; */
}
