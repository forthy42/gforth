\ LOOK.FS      xt -> lfa                               22may93jaw

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ Look checks first if the word is a primitive. If yes then the
\ vocabulary in the primitive area is beeing searched, meaning
\ creating for each word a xt and comparing it...

\ If a word is no primitive look searches backwards to find the nfa.
\ Problems: A compiled xt via compile, might be created with noname:
\           a noname: leaves now a empty name field

decimal

\ look                                                  17may93jaw

\ rename to discover!!!

: (look)  ( xt startlfa -- lfa flag )
        false swap
        BEGIN @ dup
        WHILE dup name>int
              3 pick = IF nip dup THEN
        REPEAT
        drop nip
        dup 0<> ;


\ !!! nicht optimal!
[IFUNDEF] look
has? ec [IF]

has-rom 
[IF]
: look
    dup [ unlock rom-dictionary area lock ] 
    literal literal within
    IF
	>name dup ?? <>
    ELSE
	forth-wordlist @ (look)
    THEN ;
[ELSE]
: look ( cfa -- lfa flag )
    >name dup ??? <> ;
[THEN]

[ELSE]

: PrimStart ['] true >name ;

: look ( cfa -- lfa flag )
    dup dictionary-end forthstart within
    IF
	PrimStart (look)
    ELSE
	>name dup ??? <>
    THEN ;

[THEN]
[THEN]
