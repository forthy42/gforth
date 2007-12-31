/* support functions for vmgen example

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

#include <stdio.h>

typedef long Cell;
#ifdef __GNUC__
typedef void *Label;
typedef Label Inst; /* we could "typedef Cell Inst", removing the need
                       for casts in a few places, but requiring a few
                       casts etc. in other places */
#else
typedef long Label;
typedef long Inst;
#endif

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
extern int use_super;

/* generic vmgen support functions (e.g., wrappers) */
void gen_inst(Inst **vmcodepp, Inst i);
void init_peeptable(void);
void vm_disassemble(Inst *ip, Inst *endp, Inst prim[]);
void vm_count_block(Inst *ip);
struct block_count *block_insert(Inst *ip);
void vm_print_profile(FILE *file);

/* type change macros; these are specific to the types you use, so you
   have to change this part */
#define vm_Cell2i(_cell,x)	((x)=(long)(_cell))
#define vm_Cell2target(_cell,x)	((x)=(Inst *)(_cell))
#define vm_Cell2a(_cell,x)	((x)=(char *)(_cell))
#define vm_i2Cell(x,_cell)	((_cell)=(Cell)(x))
#define vm_target2Cell(x,_cell)	((_cell)=(Cell)(x))
#define vm_a2Cell(x,_cell)	((_cell)=(Cell)(x))
#define vm_Cell2Cell(_x,_y) ((_y)=(Cell)(_x))
/* the cast in vm_Cell2Cell is needed because the base type for
   inst-stream is Cell, but *IP is an Inst */

/* for future extensions */
#define IMM_ARG(access,value)		(access)

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
