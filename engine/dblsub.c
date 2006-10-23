/* some routines for double-cell arithmetic
   only used if BUGGY_LONG_LONG

   Copyright (C) 1996,2000,2003 Free Software Foundation, Inc.
 * Copyright (C) 1995  Dirk Uwe Zoller
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License along with this library; if not, write to the Free
 * Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

 This has been adapted from pfe-0.9.14
 */

#include "config.h"
#include "forth.h"

/* !! a bit machine dependent */
#define HALFCELL_BITS	(CELL_BITS/2)
#define UH(x)		(((UCell)(x))>>HALFCELL_BITS)
#define LH(x)		((x)&((~(UCell)0)>>HALFCELL_BITS))
#define L2U(x)		(((UCell)(x))<<HALFCELL_BITS)
#define HIGHBIT(x)	(((UCell)(x))>>(CELL_BITS-1))
#define UD2D(ud)	({UDCell _ud=(ud); (DCell){_ud.hi,_ud.lo};})
#define D2UD(d)		({DCell _d=(d); (UDCell){_d.hi,_d.lo};})

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

UDCell umdiv (UDCell u, UCell v)
/* Divide unsigned double by single precision using shifts and subtracts.
   Return quotient in lo, remainder in hi. */
{
  int i = CELL_BITS, c = 0;
  UCell q = 0, h = u.hi, l = u.lo;
  UDCell res;

  if (v==0)
    throw(BALL_DIVZERO);
  if (h>=v)
    throw(BALL_RESULTRANGE);
  for (;;)
    {
      if (c || h >= v)
	{
	  q++;
	  h -= v;
	}
      if (--i < 0)
	break;
      c = HIGHBIT (h);
      h <<= 1;
      h += HIGHBIT (l);
      l <<= 1;
      q <<= 1;
    }
  res.hi = h;
  res.lo = q;
  return res;
}

DCell smdiv (DCell num, Cell denom)	/* symmetric divide procedure, mixed prec */
{
  DCell res;
  Cell numsign=num.hi;
  Cell denomsign=denom;

  if (numsign < 0)
    num = dnegate (num);
  if (denomsign < 0)
    denom = -denom;
  res = UD2D(umdiv (D2UD(num), denom));
  if ((numsign^denomsign)<0) {
    res.lo = -res.lo;
    if (((Cell)res.lo) > 0) /* note: == 0 is possible */
      throw(BALL_RESULTRANGE);
  } else {
    if (((Cell)res.lo) < 0)
      throw(BALL_RESULTRANGE);
  }
  if (numsign<0)
    res.hi = -res.hi;
  return res;
}

DCell fmdiv (DCell num, Cell denom)	/* floored divide procedure, mixed prec */
{
  /* I have this technique from Andrew Haley */
  DCell res;
  Cell denomsign=denom;
  Cell numsign;

  if (denom < 0) {
    denom = -denom;
    num = dnegate(num);
  }
  numsign = num.hi;
  if (numsign < 0)
    num.hi += denom;
  res = UD2D(umdiv(D2UD(num),denom));
  if ((numsign^((Cell)res.lo)) < 0)
    throw(BALL_RESULTRANGE);
  if (denomsign<0)
    res.hi = -res.hi;
  return res;
}
