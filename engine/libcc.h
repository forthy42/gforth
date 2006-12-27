/* header file for libcc-generated C code

  Copyright (C) 2006 Free Software Foundation, Inc.

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

typedef CELL_TYPE Cell;
typedef double Float;

#define Clongest long long
typedef unsigned Clongest UClongest;

extern Cell *gforth_SP;
extern Float *gforth_FP;

#define CELL_BITS	(sizeof(Cell) * 8)

#define gforth_d2ll(lo,hi) \
  ((sizeof(Cell) < sizeof(Clongest)) \
   ? (((UClongest)(lo))|(((UClongest)(hi))<<CELL_BITS)) \
   : (lo))

#define gforth_ll2d(ll,lo,hi) \
  do { \
    UClongest _ll = (ll); \
    (lo) = (Cell)_ll; \
    (hi) = ((sizeof(Cell) < sizeof(Clongest)) \
            ? (_ll >> CELL_BITS) \
            : 0); \
  } while (0);
