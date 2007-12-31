\ WORDINFO.FS  V1.0                                    17may93jaw

\ Copyright (C) 1995,1996,1998,2000,2003,2007 Free Software Foundation, Inc.

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

\ May be cross-compiled
\ If you want check values then exclude comments,
\ but keep in mind that this can't be cross-compiled

require look.fs

\ Wordinfo is a tool that checks a nfa
\ and finds out what wordtype we have
\ it is used in SEE.FS

\ the old alias? did not work and it is not used, so I changed
\ it in many respects - anton
: alias? ( nfa1 -- nfa2|0 )
    \ if nfa1 is an alias, nfa2 is the name of the original word.
    \ if the original word has no name, return 0.
    dup cell+ @ alias-mask and 0=
    IF ( nfa1 )
	((name>)) @ >name
    ELSE
	drop 0
    THEN ;

: var?  ( nfa -- flag )
    ((name>)) >code-address dovar: = ;

: con?  ( nfa -- flag )
    ((name>)) >code-address docon: = ;

: user?  ( nfa -- flag )
    ((name>)) >code-address douser: = ;

: does? ( nfa -- flag )
    ((name>))
    >does-code 0<> ;

: defered? ( nfa -- flag )
    ((name>)) >code-address dodefer: = ;

: colon? ( nfa -- flag )
    ((name>)) >code-address docol: = ;

\ the above words could be factored with create-does>, but this would
\ probably make this file incompatible with cross.

[IFDEF] forthstart
: xtprim? ( xt -- flag )
    in-dictionary? 0= ; \ !! does not work for CODE words
[ELSE]
: xtprim? ( xt -- flag )
    dup >body swap >code-address = ; \ !! works only for indirect threaded code
				     \ !! does not work for primitives
[THEN]
: prim? ( nfa -- flag )
        name>int xtprim? ;

\ None nestable IDs:

1 CONSTANT Pri#         \ Primitives
2 CONSTANT Con#         \ Constants
3 CONSTANT Var#         \ Variables
4 CONSTANT Val#         \ Values

\ Nestabe IDs:

5 CONSTANT Doe#         \ Does part
6 CONSTANT Def#         \ Defer
7 CONSTANT Col#         \ Colon def
8 CONSTANT Use#         \ User variable

\ Nobody knows:

9 CONSTANT Ali#         \ Alias

10 CONSTANT Str#         \ Structure words

11 CONSTANT Com#        \ Compiler directives : ; POSTPONE

CREATE InfoTable
        ' Prim?    A, Pri# ,
        ' Alias?   A, Ali# ,
        ' Con?     A, Con# ,
        ' Var?     A, Var# ,
\        ' Value?  A, Val# ,
        ' Defered? A, Def# ,
        ' Does?    A, Doe# ,
        ' Colon?   A, Col# ,
	' User?    A, Use# ,
        0 ,

: WordInfo ( nfa --- code )
        InfoTable
        BEGIN  dup @ dup
        WHILE  swap 2 cells + swap
               2 pick swap execute
        UNTIL
        1 cells - @ nip
        ELSE
        2drop drop 0
        THEN ;

