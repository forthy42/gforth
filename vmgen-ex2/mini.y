/* front-end compiler for vmgen example

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

/* I use yacc/bison here not because I think it's the best tool for
   the job, but because it's widely available and popular; it's also
   (barely) adequate for this job. */

%{
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "mini.h"

/* BB_BOUNDARY is needed on basic blocks without a preceding VM branch */
#define BB_BOUNDARY (last_compiled = NULL,  /* suppress peephole opt */ \
                     block_insert(vmcodep)) /* for accurate profiling */

Label *vm_prim;
Inst *vmcodep;
FILE *vm_out;
int vm_debug;

void yyerror(char *s)
{
#if 1
  /* for pure flex call */
  fprintf(stderr, "%s: %s\n", program_name, s);
#else
  /* lex or flex -l supports yylineno */
  fprintf (stderr, "%s: %d: %s\n", program_name, yylineno, s);
#endif
}

#include "mini-gen.i"

void gen_main_end(void)
{
  gen_call(&vmcodep, func_addr("main"), func_calladjust("main"));
  gen_end(&vmcodep);
  BB_BOUNDARY; /* for profiling; see comment in mini.vmg:end */
}

int locals=0;
int nonparams=0;

int yylex();
%}


%token FUNC RETURN END VAR IF THEN ELSE WHILE DO BECOMES PRINT NUM IDENT

%union {
  long num;
  char *string;
  Inst *instp;
}

%type <string> IDENT;
%type <num> NUM;
%type <instp> elsepart;


%%
program: program function
       | ;

function: FUNC IDENT { locals=0; nonparams=0; } '(' params ')' 
          vars                  { insert_func($2,vmcodep,locals,nonparams); } 
          stats RETURN expr ';'
          END FUNC ';'          { gen_return(&vmcodep, -adjust(locals)); }
        ;

params: IDENT ',' { insert_local($1); } params 
      | IDENT     { insert_local($1); }
      | ;

vars: vars VAR IDENT ';' { insert_local($3); nonparams++; }
    | ;

stats: stats stat ';'
     | ;

stat: IF expr THEN { gen_zbranch(&vmcodep, 0); $<instp>$ = vmcodep; }
      stats { $<instp>$ = $<instp>4; } 
      elsepart END IF { BB_BOUNDARY; vm_target2Cell(vmcodep, $<instp>7[-1]); }
    | WHILE   { BB_BOUNDARY; $<instp>$ = vmcodep; } 
      expr DO { gen_zbranch(&vmcodep, 0); $<instp>$ = vmcodep; }
      stats END WHILE { gen_branch(&vmcodep, $<instp>2); vm_target2Cell(vmcodep, $<instp>5[-1]); }
    | IDENT BECOMES expr	{ gen_storelocal(&vmcodep,  var_offset($1)); }
    | PRINT expr		{ gen_print(&vmcodep); }
    | expr                      { gen_drop(&vmcodep); }
    ;

elsepart: ELSE { gen_branch(&vmcodep, 0); $<instp>$ = vmcodep; vm_target2Cell(vmcodep, $<instp>0[-1]); }
          stats { $$ = $<instp>2; }
        | { $$ = $<instp>0; }
        ;

expr: term '+' term	 { gen_add(&vmcodep); }
    | term '-' term	 { gen_sub(&vmcodep); }
    | term '*' term	 { gen_mul(&vmcodep); }
    | term '&' term	 { gen_and(&vmcodep); }
    | term '|' term	 { gen_or(&vmcodep); }
    | term '<' term	 { gen_lessthan(&vmcodep); }
    | term '=' term	 { gen_equals(&vmcodep); }
    | '!' term		 { gen_not(&vmcodep); }
    | '-' term		 { gen_negate(&vmcodep); }
    | term
    ;

term: '(' expr ')'
    | IDENT '(' args ')' { gen_call(&vmcodep, func_addr($1), func_calladjust($1)); }
    | IDENT		 { gen_loadlocal(&vmcodep, var_offset($1)); }
    | NUM		 { gen_lit(&vmcodep, $1); }
    ;

/* missing: argument counting and checking against called function */
args: expr ',' args
    | expr 
    | ;
%%
int yywrap(void)
{
  return 1;
}

#include "lex.yy.c"
