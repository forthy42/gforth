\ WORDINFO.FS  V1.0                                    17may93jaw

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke
\ Copyright (C) 1995,1996,1998,2000,2003,2007,2012,2013,2014,2018,2019,2021,2023,2025 Free Software Foundation, Inc.

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

: var?  ( nfa -- flag )
    ((name>)) >code-address dovar: = ;

: con?  ( nfa -- flag )
    ((name>)) >code-address docon: = ;

: value? ( nfa -- flag )
    ((name>)) >code-address dovalue: = ;

: user?  ( nfa -- flag )
    ((name>)) >code-address douser: = ;

: does? ( nfa -- flag )
    ((name>))
    >does-code 0<> ;

: deferred? ( nfa -- flag )
    ((name>)) >code-address dodefer: = ;

: colon? ( nfa -- flag )
    ((name>)) >code-address docol: = ;

\ the above words could be factored with create-does>, but this would
\ probably make this file incompatible with cross.

: prim? ( nfa -- flag )
    >code-address cfaprim? ;
synonym xtprim? prim?

\ None nestable IDs:

theme-color: Pri-color         \ Primitives
theme-color: Con-color         \ Constants
theme-color: Var-color         \ Variables
theme-color: Val-color         \ Values

\ Nestabe IDs:

theme-color: Doe-color         \ Does part
theme-color: Def-color         \ Defer
theme-color: Col-color         \ Colon def
theme-color: Use-color         \ User variable

\ Nobody knows:

theme-color: Ali-color         \ Alias

theme-color: Str-color         \ Structure words

theme-color: Com-color        \ Compiler directives : ; POSTPONE

CREATE InfoTable
        ' Prim?    , ' Pri-color ,
        ' Alias?   , ' Ali-color ,
        ' Con?     , ' Con-color ,
        ' Var?     , ' Var-color ,
        ' Value?   , ' Val-color ,
       ' Deferred? , ' Def-color ,
        ' Does?    , ' Doe-color ,
        ' Colon?   , ' Col-color ,
	' User?    , ' Use-color ,
        0 ,

: WordInfo ( nfa --- xt )
        InfoTable
        BEGIN  dup @ dup
        WHILE  2 cells under+
               third swap execute
        UNTIL
        cell- @ nip
        ELSE
        2drop drop ['] default-color
        THEN ;

