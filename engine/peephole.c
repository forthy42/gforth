/* Peephole optimization routines and tables

  Copyright (C) 2001 Free Software Foundation, Inc.

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

#include "config.h"
#include "forth.h"
#include "stdlib.h"

/* the numbers in this struct are primitive indices */
typedef struct Combination {
  int prefix;
  int lastprim;
  int combination_prim;
} Combination;

Combination peephole_table[] = {
#include "peephole.i"
};

Xt *primtable(Label symbols[], Cell size)
{
#ifdef DIRECT_THREADED
  return symbols;
#else /* !defined(DIRECT_THREADED) */
  Xt *xts = (Xt *)malloc(size*sizeof(Xt));
  Cell i;

  for (i=0; i<size; i++)
    xts[i] = &symbols[i];
  return xts;
#endif /* !defined(DIRECT_THREADED) */
}

/* we are currently using a simple linear search; we can refine this
   once the interface has settled and this works */

Cell prepare_peephole_table(Xt xts[])
{
  return (Cell)xts;
}

Xt peephole_opt(Xt xt1, Xt xt2, Cell peeptable)
{
  Xt *xts = (Xt *)peeptable;
  Cell i;

  for (i=0; i<(sizeof(peephole_table)/sizeof(Combination)); i++) {
    Combination *c = &peephole_table[i];
    if (xt1 == xts[c->prefix] && xt2 == xts[c->lastprim])
      return xts[c->combination_prim];
  }
  return 0;
}
