\ argument expansion

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

: cstring>sstring  ( cstring -- addr n ) \ gforth	cstring-to-sstring
    -1 0 scan 0 swap 1+ /string ;
: arg ( n -- addr count ) \ gforth
    cells argv @ + @ cstring>sstring ;
: #!       postpone \ ;  immediate

Create pathstring 2 cells allot \ string
Create pathdirs   2 cells allot \ dir string array, pointer and count
Variable argv
Variable argc

0 Value script? ( -- flag )

: process-path ( addr1 u1 -- addr2 u2 )
    \ addr1 u1 is a path string, addr2 u2 is an array of dir strings
    align here >r
    BEGIN
	over >r 0 scan
	over r> tuck - ( rest-str this-str )
	dup
	IF
	    2dup 1- chars + c@ [char] / <>
	    IF
		2dup chars + [char] / swap c!
		1+
	    THEN
	    2,
	ELSE
	    2drop
	THEN
	dup
    WHILE
	1 /string
    REPEAT
    2drop
    here r> tuck - 2 cells / ;

: do-option ( addr1 len1 addr2 len2 -- n )
    2swap
    2dup s" -e"         compare  0= >r
    2dup s" --evaluate" compare  0= r> or
    IF  2drop dup >r ['] evaluate catch
	?dup IF  dup >r DoError r> negate (bye)  THEN
	r> >tib +!  2 EXIT  THEN
    ." Unknown option: " type cr 2drop 1 ;

: process-args ( -- )
    true to script?
    >tib @ >r
    argc @ 1
    ?DO
	I arg over c@ [char] - <>
	IF
	    required 1
	ELSE
	    I 1+ argc @ =  IF  s" "  ELSE  I 1+ arg  THEN
	    do-option
	THEN
    +LOOP
    r> >tib !
    false to script?
;

