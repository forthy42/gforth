/* C-code generation for Gforth

  Copyright (C) 2010 Free Software Foundation, Inc.

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

#include "config.h"
#include "forth.h"

/* Assumptions: 

   1) the function corresponding to (the first BB in) a
   colon def is stored in the 2nd cell of the code field.

   2) the function corresponding to (the first BB of) a does-handler
   is stored in the second cell of the code field.

*/

/* run-time system */




/* defined types: 
   bb: basic block (or other unit fitting in C function;
   doer: docol etc. called from execute
*/

typedef void *(*Bb)();
typedef Bb (*Doer)(Bb,Cell *);
typedef void (*Gen)();

Cell translate_link(Cell *);

void control_loop(Bb next)
{
  for (;;)
    next = next();
}

/* Doers */

Bb docol(Bb next, Cell *cfa)
{
  *--gforth_RP = (Cell)next;
  return (Bb)cfa[1];
}

void translate_link_colon_def(Cell *cfa)
{
  cfa[1] = translate_link(PFA(cfa));
  cfa[0] = (Cell)docol;
}

Bb docol0(Bb next, Cell *cfa)
/* first invocation: compile on demand */
{
  translate_link_colon_def(cfa);
  return docol(next, cfa);
}

Bb docon(Bb next, Cell *cfa)
{
  *--gforth_SP = *PFA(cfa);
  return next;
}

Bb dovar(Bb next, Cell *cfa)
{
  *--gforth_SP = (Cell)PFA(cfa);
  return next;
}

Bb douser(Bb next, Cell *cfa)
{
  *--gforth_SP = (Cell)(gforth_UP + *(Cell *)PFA(cfa));
  return next;
}

Bb dodefer(Bb next, Cell *cfa)
{
  Cell *next_cfa = (Cell *)*PFA(cfa);
  Doer next_doer = *(Doer *)next_cfa;
  return next_doer(next, next_cfa);
}

Bb dofield(Bb next, Cell *cfa)
{
  *gforth_SP += *PFA(cfa);
  return next;
}

Bb dovalue(Bb next, Cell *cfa)
{
  *--gforth_SP = *PFA(cfa);
  return next;
}

Bb dodoes(Bb next, Cell *cfa)
{
  *--gforth_RP = (Cell)next;
  *--gforth_SP = (Cell)PFA(cfa);
  return (Bb)cfa[1];
}

void translate_link_does_handler(Cell *cfa)
{
  cfa[1] = translate_link((Cell *)cfa[1]);
  cfa[0] = (Cell)dodoes;
}

Bb dodoes0(Bb next, Cell *cfa)
{
  translate_link_does_handler(cfa);
  return dodoes(next,cfa);
}

Bb doabicode(Bb next, Cell *cfa)
{
  abifunc *f = (abifunc *)PFA(cfa);
  gforth_SP = (*f)(gforth_SP, &gforth_FP);
  return next;
}

Bb dosemiabicode(Bb next, Cell *cfa)
{
  Address body = (Address)PFA(cfa);
  semiabifunc *f = (semiabifunc *)DOES_CODE1(cfa);
  gforth_SP = (*f)(gforth_SP, &gforth_FP, body);
  return next;
}

Bb cgen_symbols[] = {
  docol0,
  docon,
  dovar,
  douser,
  dodefer,
  dofield,
  dovalue,
  dodoes0,
  doabicode,
  dosemiabicode
};
/* !! actually we need all the primitives for the code fields as well.
   Generate them?
 */
  
/* Translator */

/* primitive descriptions (static) */
typedef struct stack_effect {
  char in;   /* stack items on input */
  char out;  /* stack items on output */
  char dump; /* if true, dump this stack (apart from in, out) to memory */
} Stackeffect;

typedef struct prim {
  Stackeffect se[MAX_STACKS];
  Gen  gen;
  char end_bb;
} Prim;

Prim prims[] = {
};

typedef struct stackpoint {
  signed char depth;  /* current depth (relative to starting depth) */
  char loaded_start;  /* from which depth the loading starts */
  char loaded; /* how many stack items have been loaded from start to here */
  char stored; /* how many stack items will be stored from here to end */
  char new;    /* how many new stack items were accesses from start to here */
  char old;    /* how many existing stack items were accessed */
} Stackpoint;

typedef struct codepoint {
  Cell *tc;
  StackPoint stack[MAX_STACKS];
} Codepoint;


void forward_pass(Cell *tc, Codepoint *p, int n)
{
  for (j=0; j<MAX_STACKS; j++) {
    Stackpoint *sp = p[0].stack;
    sp->depth        = 0;
    sp->loaded       = 0;
    sp->loaded_start = 0;
    sp->new          = 0;
  }
  for (i=0; i<n; i++) {
    for (j=0; j<MAX_STACKS; j++) {
      Stackpoint *b = p[i].stack+j; /* before */
      Stackpoint *a = p[i].stack+j; /* after */
      Stackeffect *s = p[PRIM_NUM(*tc)].se+j;
      int depth = b->depth - s.in;
      if (depth<b->old)
	a->old=depth;
      if (s->dump) {
	a->loaded = 0;
	a->loaded_start = depth;
      } else {
	a->loaded = min(depth,b->loaded);
	a->loaded_start = min(depth,b->loaded_start);
      }
      depth += s.out;
      a->new = max(b->new, depth);
      a->depth = depth;
    }
  }
}



char *cgen(Cell *tc)
{
  int n = npoints(tc);
  Codepoint p[n+1];

  forward_pass(tc,p,n);
  backwards_pass(tc,p,n);
  decl_stack_items(p,n);
  gen_prims(p);
}




Cell translate_link(Cell *bb_tc)
{
  Bb bbfunc = lookup_bb(bb_tc);

  if (bbfunc == NULL) {
    char *filename_c  = cgen(bb_tc);
    char *filename_la = compile(filename_tc);
    lt_dlhandle lib = lt_dlopen(filename_la);
    bb_func = lt_dlsym(symbol(bb_tc));
  }
  return (Cell) bb_func;
}
