\ argument expansion

\ Copyright (C) 1995,1996,1997,1998,2000,2003 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

: cstring>sstring  ( cstring -- addr n ) \ gforth	cstring-to-sstring
    -1 0 scan 0 swap 1+ /string ;

: arg ( u -- addr count ) \ gforth
\g Return the string for the @i{u}th command-line argument; returns
\g @code{0 0} if the access is beyond the last argument.
    dup argc @ u< if
	cells argv @ + @ cstring>sstring
    else
	drop 0 0
    endif ;

: #! ( -- ) \ gforth   hash-bang
    \g An alias for @code{\}
    postpone \ ;  immediate

Create pathstring 2 cells allot \ string
Create pathdirs   2 cells allot \ dir string array, pointer and count

Variable argv ( -- addr ) \ gforth
\g @code{Variable} -- a pointer to a vector of pointers to the command-line
\g arguments (including the command-name). Each argument is
\g represented as a C-style string.
    
Variable argc ( -- addr ) \ gforth
\g @code{Variable} -- the number of command-line arguments (including the command name).
    
0 Value script? ( -- flag )
    
: shift-args ( -- ) \ gforth
\g @code{1 arg} is deleted, shifting all following OS command line
\g parameters to the left by 1, and reducing @code{argc @}.  This word
\g can change @code{argv @}.
    argc @ 1 > if
	argv @ @ ( arg0 )
	-1 argc +!
	cell argv +!
	argv @ !
    endif ;

: process-option ( addr u -- )
    \ process option, possibly consuming further arguments
    2dup s" -e"         str= >r
    2dup s" --evaluate" str= r> or if
	2drop 1 arg shift-args evaluate exit endif
    2dup s" -h"         str= >r
    2dup s" --help"     str= r> or if
	." Image Options:" cr
	."   FILE				    load FILE (with `require')" cr
	."   -e STRING, --evaluate STRING      interpret STRING (with `EVALUATE')" cr
	." Report bugs on <https://savannah.gnu.org/bugs/?func=addbug&group=gforth>" cr
	bye
    THEN
    ." Unknown option: " type cr ;

: (process-args) ( -- )
    true to script?
    BEGIN
	argc @ 1 > WHILE
	    1 arg shift-args over c@ [char] - <> IF
		required
	    else
		process-option
	    then
    repeat
    false to script? ;

: os-boot ( path n **argv argc -- )
    stdout TO outfile-id
    stdin  TO infile-id
    argc ! argv ! pathstring 2! ;

' (process-args) IS process-args
