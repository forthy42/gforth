\ input output basics				(extra since)	02mar97jaw

\ Copyright (C) 1995-1997 Free Software Foundation, Inc.

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

\ Output                                               13feb93py

has? os [IF]
0 Value outfile-id ( -- file-id ) \ gforth

: (type) ( c-addr u -- ) \ gforth
    outfile-id write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (emit) ( c -- ) \ gforth
    outfile-id emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;
[THEN]

Defer type ( c-addr u -- ) \ core
' (type) IS Type

Defer emit ( c -- ) \ core
' (Emit) IS Emit

Defer key ( -- c ) \ core
' (key) IS key

: (.")     "lit count type ;
: (S")     "lit count ;

\ Input                                                13feb93py

07 constant #bell ( -- c ) \ gforth
08 constant #bs ( -- c ) \ gforth
09 constant #tab ( -- c ) \ gforth
7F constant #del ( -- c ) \ gforth
0D constant #cr   ( -- c ) \ gforth
\ the newline key code
0C constant #ff ( -- c ) \ gforth
0A constant #lf ( -- c ) \ gforth

: bell  #bell emit [ has? os [IF] ] outfile-id flush-file drop [ [THEN] ] ;
: cr ( -- ) \ core
    \ emit a newline
[ has? crlf [IF] ]	#cr emit #lf emit 
[ [ELSE] ]		#lf emit
[ [THEN] ]
    ;

1 [IF]
\ space spaces		                                21mar93py
decimal
Create spaces ( u -- ) \ core
bl 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )
    swap
    0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
Create backspaces
08 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )
   swap
   0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
hex
: space ( -- ) \ core
    1 spaces ;
[ELSE]
: space bl emit ;
: spaces 0 max 0 ?DO space LOOP ;

[THEN]

