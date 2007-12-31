/* some routines for double-cell arithmetic
   only used if BUGGY_LONG_LONG

   Copyright (C) 1996,2000,2003,2006,2007 Free Software Foundation, Inc.
 * Copyright (C) 1995  Dirk Uwe Zoller
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free
 * Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

 This has been adapted from pfe-0.9.14
 */

#include "config.h"
#include "forth.h"

DCell dnegate(DCell d1)
{
  DCell res;

  res.hi = ~d1.hi + (d1.lo==0);
  res.lo = -d1.lo;
  return res;
}

UDCell ummul (UCell a, UCell b)	/* unsigned multiply, mixed precision */
{
  UDCell res;
  UCell m,ul,lu,uu;

  res.lo = a*b;
/*ll = LH(a)*LH(b); dead code */
  ul = UH(a)*LH(b);
  lu = LH(a)*UH(b);
  uu = UH(a)*UH(b);
  m = ul+lu;
  res.hi = (uu
	    + L2U(m<ul) /* the carry of ul+lu */
	    + UH(m)
	    + (res.lo<L2U(m)) /* the carry of ll+L2U(m) */
	    );
  return res;
}

DCell mmul (Cell a, Cell b)		/* signed multiply, mixed precision */
{
  DCell res;

  res = UD2D(ummul (a, b));
  if (a < 0)
    res.hi -= b;
  if (b < 0)
    res.hi -= a;
  return res;
}
