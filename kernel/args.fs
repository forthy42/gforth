\ argument expansion

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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
    \g returns the string for the @var{n}th command-line argument.
    cells argv @ + @ cstring>sstring ;
: #! ( -- ) \ gforth   hash-bang
    \g an alias for @code{\}
    postpone \ ;  immediate

Create pathstring 2 cells allot \ string
Create pathdirs   2 cells allot \ dir string array, pointer and count
Variable argv ( -- addr ) \ gforth
\g contains a pointer to a vector of pointers to the command-line
\g arguments (including the command-name). Each argument is
\g represented as a C-style string.
Variable argc ( -- addr ) \ gforth
\g contains the number of command-line arguments (including the command name)

0 Value script? ( -- flag )

: do-option ( addr1 len1 addr2 len2 -- n )
    2swap
    2dup s" -e"         compare  0= >r
    2dup s" --evaluate" compare  0= r> or
    IF  2drop dup >r evaluate
	r> >tib +!  2 EXIT  THEN
    2dup s" -h"         compare  0= >r
    2dup s" --help"     compare  0= r> or
    IF  ." Image Options:" cr
	."   FILE				    load FILE (with `require')" cr
	."   -e STRING, --evaluate STRING      interpret STRING (with `EVALUATE')" cr
	." Report bugs to <bug-gforth@gnu.ai.mit.edu>" cr
	bye
    THEN
    ." Unknown option: " type cr 2drop 1 ;

: (process-args) ( -- )
    true to script?
    >tib @ >r #tib @ >r >in @ >r
    argc @ 1
    ?DO
	I arg over c@ [char] - <>
	IF
	    2dup dup #tib ! >in ! >tib !
	    required 1
	ELSE
	    I 1+ argc @ =  IF  s" "  ELSE  I 1+ arg  THEN
	    do-option
	THEN
    +LOOP
    r> >in ! r> #tib ! r> >tib !
    false to script?
;

' (process-args) IS process-args
