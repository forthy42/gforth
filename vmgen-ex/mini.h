/* support functions for vmgen example

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

#include <stdio.h>

#ifdef __GNUC__
typedef void *Label;
typedef void *Inst; /* for direct threading, the same as Label */
#else
typedef long Label;
typedef long Inst;
#endif
typedef long Cell;

extern Inst *vm_prim;
extern int locals;
extern Cell peeptable;
extern int vm_debug;
extern FILE *yyin;
extern int yylineno;
extern char *program_name;
extern FILE *vm_out;
extern Inst *vmcodep;
extern Inst *last_compiled;
extern Inst *vmcode_end;

/* generic vmgen support functions (e.g., wrappers) */
void gen_inst(Inst **vmcodepp, Inst i);
void init_peeptable(void);
void vm_disassemble(Inst *ip, Inst *endp, Inst prim[]);
void vm_count_block(Inst *ip);
struct block_count *block_insert(Inst *ip);
void vm_print_profile(FILE *file);
#define VM_IS_INST(inst, n) ((inst) == vm_prim[n])

/* mini type-specific support functions */
void genarg_i(Inst **vmcodepp, Cell i);
void printarg_i(Cell i);
void genarg_target(Inst **vmcodepp, Inst *target);
void printarg_target(Inst *target);
void printarg_a(char *a);
void printarg_Cell(Cell i);

/* engine functions (type not fixed) */
Cell engine(Inst *ip0, Cell *sp, char *fp);
Cell engine_debug(Inst *ip0, Cell *sp, char *fp);

/* other generic functions */
int yyparse(void);

/* mini-specific functions */
void insert_func(char *name, Inst *start, int locals, int nonparams);
Inst *func_addr(char *name);
Cell func_calladjust(char *name);
void insert_local(char *name);
Cell var_offset(char *name);
void gen_main_end(void);

/* stack pointer change for a function with n nonparams */
#define adjust(n)  ((n) * -sizeof(Cell))
