\ prims2cl.fs	Primitives to c-library code

\ Copyright (C) 1998,1999,2001,2003,2007 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

\ Author: Jens Wilke
\ Revision Log
\ 09oct97jaw V1.0	Initial Version

include ./prims2x.fs

Create 	InputFile 130 chars allot

: c-code
    InputFile count ['] output-c-func ['] abort process-file
    ;

: c-names
    InputFile count ['] output-funclabel ['] abort process-file
    ;

: forth-names
    InputFile count ['] output-forthname dup process-file
    ;

: .\ 
    0 word count pad place pad count postpone sliteral postpone type postpone cr ; immediate
 
: c-header
	.\ #include "engine/forth.h"
	.\ extern char *cstr(Char *from, UCell size, int clear);
	.\ extern char *tilde_cstr(Char *from, UCell size, int clear);
	.\ 
	.\ #undef TOS
	.\ #define TOS sp[0]
	.\ #undef IF_TOS
	.\ #define IF_TOS(x)
	.\ #undef NEXT_P2
	.\ #define NEXT_P2 
	.\ #undef NEXT_P1
	.\ #define NEXT_P1
	.\ #undef NEXT_P0
	.\ #define NEXT_P0
	.\ #undef NAME
	.\ #define NAME(x)
	.\ #undef DEF_CA
	.\ #define DEF_CA
	.\ #undef I_
	.\ #define I_ I_
	.\
	.\ #define NAME_LEN 32
	.\ #define NULL 0
	.\
   ;

: catalog
	.\ void *catalog(int p)
	.\ {
	.\         static void  *ADDR_TABLE[]={
    c-names
	.\ };
	.\         static char NAME_TABLE[][NAME_LEN]={
    forth-names
	.\ };
	."         int funcs=" function-number @ s>d <# #S #> type ." ;" cr
	.\
	.\        static struct { void *func;
	.\              	  char len;
	.\                        char name[NAME_LEN];
	.\                        }f;
	.\
	.\        switch (p)
	.\        {       case -2:   	/*
	.\                             	 We return the table known words
	.\                               don't use this!!!
	.\                               */
	.\                              return (NAME_TABLE[0]);
	.\
	.\                case -1:	/*
	.\                          	 Return number of words in this module
	.\                               */
	.\                           	return ((void *) funcs);
	.\         }
	.\	        /*
	.\                Check for valid function number
	.\          */
	.\         if (p<0 || p>=funcs) return (0);
	.\         /*
	.\                Find matching forth word and return its address
	.\         */
	.\         strcpy(f.name,NAME_TABLE[p]);
	.\         f.len=strlen(f.name);
	.\         f.func=ADDR_TABLE[p];
	.\         return (&f);
	.\ }
    ;

: main
  c-header
  c-code
  catalog
  ;

: file
  bl word count InputFile place ;

