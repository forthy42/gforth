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

\ >NAME PRIMSTART                                       22may93jaw

\ : >name ( xt -- nfa )
\         BEGIN   1 chars -
\                 dup c@ alias-mask and
\         UNTIL ;

: PrimStart ['] true >name ;

\ look                                                  17may93jaw

: (look)  ( xt startlfa -- lfa flag )
        false swap
        BEGIN @ dup
        WHILE dup name>int
              3 pick = IF nip dup THEN
        REPEAT
        drop nip
        dup 0<> ;

: look ( cfa -- lfa flag )
        dup forthstart <
        IF PrimStart (look)
        ELSE >name true THEN ;

