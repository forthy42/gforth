/* support functions and main() for vmgen example

  Copyright (C) 2001,2003,2007 Free Software Foundation, Inc.

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
#include <stdio.h>
#include <unistd.h>
extern int optind;

#include <assert.h>
#include "mini.h"

void genarg_i(Inst **vmcodepp, Cell i)
{
  *((Cell *) *vmcodepp) = i;
  (*vmcodepp)++;
}

void genarg_target(Inst **vmcodepp, Inst *target)
{
  *((Inst **) *vmcodepp) = target;
  (*vmcodepp)++;
}

void printarg_i(Cell i)
{
  fprintf(vm_out, "%ld ", i);
}

void printarg_target(Inst *target)
{
  fprintf(vm_out, "%p ", target);
}

void printarg_a(char *a)
{
  fprintf(vm_out, "%p ", a);
}

void printarg_Cell(Cell i)
{
  fprintf(vm_out, "0x%lx ", i);
}

/* This language has separate name spaces for functions and variables;
   this works because there are no function variables, and the syntax
   makes it possible to differentiate between function and variable
   reference */

typedef struct functab {
  struct functab *next;
  char *name;
  Inst *start;
  int params;
  int nonparams;
} functab;

functab *ftab=NULL;

/* note: does not check for double definitions */
void insert_func(char *name, Inst *start, int locals, int nonparams)
{
  functab *node = malloc(sizeof(functab));

  node->next=ftab;
  node->name=name;
  node->start=start;
  node->params=locals-nonparams;
  node->nonparams=nonparams;
  ftab=node;
}

functab *lookup_func(char *name)
{
  functab *p;

  for (p=ftab; p!=NULL; p=p->next)
    if (strcmp(p->name,name)==0)
      return p;
  fprintf(stderr, "undefined function %s", name);
  exit(1);
}

Inst *func_addr(char *name)
{
  return lookup_func(name)->start;
}

Cell func_calladjust(char *name)
{
  return adjust(lookup_func(name)->nonparams);
}


typedef struct vartab {
  struct vartab *next;
  char *name;
  int index;
} vartab;

vartab* vtab;

/* no checking for double definitions */
void insert_local(char *name)
{
  vartab *node = malloc(sizeof(vartab));

  locals++;
  node->next=vtab;
  node->name=name;
  node->index=locals;
  vtab = node;
}

vartab *lookup_var(char *name)
{
  vartab *p;

  for (p=vtab; p!=NULL; p=p->next)
    if (strcmp(p->name,name)==0)
      return p;
  fprintf(stderr, "undefined local variable %s", name);
  exit(1);
}

Cell var_offset(char *name)
{
  return (locals - lookup_var(name)->index + 2)*sizeof(Cell);
}

#define CODE_SIZE 65536
#define STACK_SIZE 65536
typedef Cell (*engine_t)(Inst *ip0, Cell* sp, char* fp);

char *program_name;

int main(int argc, char **argv)
{
  int disassembling = 0;
  int profiling = 0;
  int c;
  Inst *vm_code=(Inst *)calloc(CODE_SIZE,sizeof(Inst));
  Inst *start;
  Cell *stack=(Cell *)calloc(STACK_SIZE,sizeof(Cell));
  engine_t runvm=engine;

  while ((c = getopt(argc, argv, "hdpt")) != -1) {
    switch (c) {
    default:
    case 'h':
    help:
      fprintf(stderr, "Usage: %s [options] file\nOptions:\n-h	Print this message and exit\n-d	disassemble VM program before execution\n-p	profile VM code sequences (output on stderr)\n-t	trace VM code execution (output on stderr)\n",
	      argv[0]);
      exit(1);
    case 'd':
      disassembling=1;
      break;
    case 'p':
      profiling=1;
      use_super=0; /* we don't want superinstructions in the profile */
      runvm = engine_debug;
      break;
    case 't':
      vm_debug=1;
      runvm = engine_debug;
      break;
    }
  }
  if (optind+1 != argc) 
    goto help;
  program_name = argv[optind];
  if ((yyin=fopen(program_name,"r"))==NULL) {
    perror(argv[optind]);
    exit(1);
  }

  /* initialize everything */
  vmcodep = vm_code;
  vm_out = stderr;
  (void)runvm(NULL,NULL,NULL); /* initialize vm_prim */
  init_peeptable();
  
  if (yyparse())
    exit(1);

  start=vmcodep;
  gen_main_end();
  vmcode_end=vmcodep;

  if (disassembling)
    vm_disassemble(vm_code, vmcodep, vm_prim);

  printf("result = %ld\n",runvm(start, stack+STACK_SIZE-1, NULL));

  if (profiling)
    vm_print_profile(vm_out);

  return 0;
}
