\ ERRORE.FS English error strings                      9may93jaw

\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2006,2007 Free Software Foundation, Inc.

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


\ The errors are defined by a linked list, for easy adding
\ and deleting. Speed is not neccassary at this point.

require ./io.fs
require ./nio.fs

AVariable ErrLink              \ Linked list entry point
NIL ErrLink !

decimal

\ error numbers between -256 and -511 represent signals
\ signals are handled with strsignal
\ but some signals produce throw-codes > -256, e.g., -28

\ error numbers between -512 and -2047 are for OS errors and are
\ handled with strerror

has? OS [IF]
: >stderr ( -- )
    r> outfile-id >r stderr to outfile-id
    >exec  r> to outfile-id ;
[THEN]

: .error ( n -- )
[ has? OS [IF] ]
    >stderr
[ [THEN] ]
    ErrLink
    BEGIN @ dup
    WHILE
	2dup cell+ @ =
	IF
	    2 cells + count type drop EXIT THEN
    REPEAT
    drop
[ has? os [IF] ]
    dup -511 -255 within
    IF
	256 + negate strsignal type EXIT
    THEN
    dup -2047 -511 within
    IF
	512 + negate strerror type EXIT
    THEN
[ [THEN] ]
    ." error " dec. ;
