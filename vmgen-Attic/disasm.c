/* example wrapper for VM code disassembler */

/*
  Copyright (C) 2000 Free Software Foundation, Inc.

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

#include <stdio.h>
#include "../engine/forth.h"

typedef Label Inst;

void printarg_n(Inst n)
{
  printf(" %ld",(Cell)n);
}

void printarg_w(Inst n)
{
  printf(" $%lx",(Cell)n);
}

disasm_vm(Inst *start, int size, Inst *prim)
{
  Inst *ip;

  for (ip=start; ip<start+size; ) {
    printf("%lx: ",(long)ip);
    /* output of gforth -m 1000000 prims2x.fs -e "c-flag off s\" prim.b\" ' output-disasm process-file bye" */
#include "prim-disasm.i"
    /* else */ {
      printf("%8lx (not a VM instruction)", *ip++);
    }
    putchar('\n');
  }
}
