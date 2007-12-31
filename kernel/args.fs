\ argument expansion

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2006,2007 Free Software Foundation, Inc.

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

: cstring>sstring  ( cstring -- addr n ) \ gforth	cstring-to-sstring
    -1 0 scan 0 swap 1+ /string ;

: arg ( u -- addr count ) \ gforth
\g Return the string for the @i{u}th command-line argument; returns
\g @code{0 0} if the access is beyond the last argument.  @code{0 arg}
\g is the program name with which you started Gforth.  The next
\g unprocessed argument is always @code{1 arg}, the one after that is
\g @code{2 arg} etc.  All arguments already processed by the system
\g are deleted.  After you have processed an argument, you can delete
\g it with @code{shift-args}.
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
\g @code{Variable} -- a pointer to a vector of pointers to the
\g command-line arguments (including the command-name). Each argument
\g is represented as a C-style zero-terminated string.  Changed by
\g @code{next-arg} and @code{shift-args}.
    
Variable argc ( -- addr ) \ gforth
\g @code{Variable} -- the number of command-line arguments (including
\g the command name).  Changed by @code{next-arg} and @code{shift-args}.

    
0 Value script? ( -- flag )
    
: shift-args ( -- ) \ gforth
\g @code{1 arg} is deleted, shifting all following OS command line
\g parameters to the left by 1, and reducing @code{argc @@}.  This word
\g can change @code{argv @@}.
    argc @ 1 > if
	argv @ @ ( arg0 )
	-1 argc +!
	cell argv +!
	argv @ !
    endif ;

: next-arg ( -- addr u ) \ gforth
\g get the next argument from the OS command line, consuming it; if
\g there is no argument left, return @code{0 0}.
    1 arg shift-args ;

\ processing args on Gforth startup
\ helper words

: os-execute-parsing ( ... addr u xt -- ... )
    s" *OS command line*" execute-parsing-wrapper ;

: args-required1 ( addr u -- )
    2dup input-lexeme! required ;

: args-required ( i*x addr u -- i*x ) \ gforth
    2dup ['] args-required1 os-execute-parsing ;

: args-evaluate ( i*x addr u -- j*x ) \ gforth
    ['] interpret os-execute-parsing ;

\ main words

: process-option ( addr u -- )
    \ process option, possibly consuming further arguments
    2dup s" -e"         str= >r
    2dup s" --evaluate" str= r> or if
	2drop next-arg args-evaluate exit endif
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
	    next-arg over c@ [char] - <> IF
                args-required
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
