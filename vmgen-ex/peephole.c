/* Peephole optimization routines and tables

  Copyright (C) 2001,2002,2003,2007 Free Software Foundation, Inc.

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

#include <stdlib.h>
#include "mini.h"

/* the numbers in this struct are primitive indices */
typedef struct Combination {
  int prefix;
  int lastprim;
  int combination_prim;
} Combination;

Combination peephole_table[] = {
#include "mini-peephole.i"
#ifndef __GNUC__
  {-1,-1,-1} /* unnecessary; just to shut up lcc if the file is empty */
#endif
};

int use_super = 1; /* turned off by option -p */

typedef struct Peeptable_entry {
  struct Peeptable_entry *next;
  Inst prefix;
  Inst lastprim;
  Inst combination_prim;
} Peeptable_entry;

#define HASH_SIZE 1024
#define hash(_i1,_i2) (((((Cell)(_i1))^((Cell)(_i2)))>>4)&(HASH_SIZE-1))

Cell peeptable;

Cell prepare_peephole_table(Inst insts[])
{
  Cell i;
  Peeptable_entry **pt = (Peeptable_entry **)calloc(HASH_SIZE,sizeof(Peeptable_entry *));

  for (i=0; i<sizeof(peephole_table)/sizeof(peephole_table[0]); i++) {
    Combination *c = &peephole_table[i];
    Peeptable_entry *p = (Peeptable_entry *)malloc(sizeof(Peeptable_entry));
    Cell h;
    p->prefix =           insts[c->prefix];
    p->lastprim =         insts[c->lastprim];
    p->combination_prim = insts[c->combination_prim];
    h = hash(p->prefix,p->lastprim);
    p->next = pt[h];
    pt[h] = p;
  }
  return (Cell)pt;
}

void init_peeptable(void)
{
  peeptable = prepare_peephole_table(vm_prim);
}

Inst peephole_opt(Inst inst1, Inst inst2, Cell peeptable)
{
  Peeptable_entry **pt = (Peeptable_entry **)peeptable;
  Peeptable_entry *p;

  if (use_super == 0)
      return 0;
  for (p = pt[hash(inst1,inst2)]; p != NULL; p = p->next)
    if (inst1 == p->prefix && inst2 == p->lastprim)
      return p->combination_prim;
  return NULL;
}

Inst *last_compiled = NULL;

void gen_inst(Inst **vmcodepp, Inst i)
{
  if (last_compiled != NULL) {
    Inst combo = peephole_opt(*last_compiled, i, peeptable);
    if (combo != NULL) {
      *last_compiled = combo;
      return;
    }
  }
  last_compiled = *vmcodepp;
  **vmcodepp = i;
  (*vmcodepp)++;
}
