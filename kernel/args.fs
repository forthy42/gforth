\ argument expansion

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook, Jens Wilke
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2006,2007,2012,2014,2016,2019,2021,2023,2024,2025 Free Software Foundation, Inc.

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

require ./io.fs

: cstring>sstring  ( c-addr -- c-addr u ) \ gforth	cstring-to-sstring
    \g @i{C-addr} is the start address of a zero-terminated string,
    \g @i{u} is its length.
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

' \ alias #! ( -- ) \ gforth   hash-bang
\g An alias for @code{\}
immediate

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

: clear-args ( -- )
    #0. pathstring 2! argv off ;

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

terminal-input 4 cells + @ \ terminal::save-input
terminal-input 3 cells + @ \ terminal::restore-input
:noname -4 ; \ source-id
:noname ( -- flag )
    argc @ 1 u> dup IF
	next-arg #tib ! tib ! >in off  1 loadline +!
    THEN ;     \ refill
evaluate-input 0 cells + @ \ evaluate::source
| Create arg-input   A, A, A, A, A,

\ processing args on Gforth startup
\ helper words

\ main words

: (process-option) ( addr u -- translation )
    \ process option, possibly consuming further arguments
    2dup s" -e"         str= >r
    2dup s" --evaluate" str= r> or if
	2drop refill IF  ['] interpret  ELSE  translate-none  THEN exit endif
    ['] required ;

Defer process-option ( addr u -- ... xt | 0 ) \ gforth
\G Recognizer that processes an option, returns an execute-only
\G xt to process the option
' (process-option) IS process-option

: process-args ( -- )
    arg-input cell new-tib  -4 loadfilename# !
    true to script?
    BEGIN
	refill WHILE
	    source 2dup input-lexeme!
	    process-option ?found execute
    REPEAT
    false to script?
    0 pop-file drop ;

: os-boot ( path n **argv argc -- )
    stdin  UTO infile-id
    stdout UTO outfile-id
    stderr UTO debug-fid
    argc ! argv ! pathstring 2! ;
