\ catch, throw, etc.

\ Copyright (C) 1999 Free Software Foundation, Inc.

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

Defer 'catch
Defer 'throw

' noop IS 'catch
' noop IS 'throw

\ has? backtrace [IF]
Defer store-backtrace
' noop IS store-backtrace
\ [THEN]

: (protect) ( -- )
    \ inline argument: address of the handler
    r>
    'catch
    dup dup @ + >r \ recovery address
    lp@ >r
    handler @ >r
    rp@ handler !
    backtrace-empty on
    cell+ >r ;

: protect ( compilation  -- orig ; run-time  -- ) \ gforth
    POSTPONE (protect) >mark ; immediate compile-only

: (endprotect) ( -- )
    \ end of protect block: restore handler, forget rest
    r>
    r> handler !
    rdrop \ lp
    rdrop \ recovery address
    >r ;

: endprotect ( compilation  orig -- ; run-time  -- x ) \ gforth
    0 POSTPONE literal
    POSTPONE (endprotect)
    POSTPONE then ; immediate compile-only

: catch-protect ( ... xt -- ... x )
    protect execute endprotect ;

: (try) ( -- )
    \ inline argument: address of the handler
    r>
    sp@ >r
    fp@ >r
    >r ;

: try ( compilation  -- orig ; run-time  -- ) \ gforth
    POSTPONE (try) POSTPONE (protect) >mark ; immediate compile-only

: (recover) ( -- )
    \ normal end of try block: restore handler, forget rest
    r>
    r> handler !
    rdrop \ lp
    rdrop \ recovery address
    rdrop \ fp
    rdrop \ sp
    >r ;

: (recover2) ( ... x -- ... x )
    \ restore sp and fp
    r>
    r> fp!
    r> -rot >r >r sp! drop r> ;

: recover ( compilation  orig -- ; run-time  -- ) \ gforth
    \ !! check using a special tag
    POSTPONE (endprotect) POSTPONE rdrop POSTPONE rdrop
    POSTPONE else
    POSTPONE (recover2) ; immediate compile-only

: endtry ( compilation  orig -- ; run-time  -- ) \ gforth
    POSTPONE then ; immediate compile-only

:noname ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
    try
	execute 0
    recover
        nip
    endtry ;
is catch

\ :noname ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
\     'catch
\     sp@ >r
\ \ [ has? floating [IF] ]
\     fp@ >r
\ \ [ [THEN] ]
\ \ [ has? glocals [IF] ]
\     lp@ >r
\ \ [ [THEN] ]
\     handler @ >r
\     rp@ handler !
\ \ [ has? backtrace [IF] ]
\     backtrace-empty on
\ \ [ [THEN] ]
\     execute
\     r> handler ! rdrop 
\ \ [ has? floating [IF] ]
\     rdrop
\ \ [ [THEN] ]
\ \ [ has? glocals [IF] ]
\     rdrop
\ \ [ [THEN] ]
\     0 ;
\ is catch

:noname ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
    ?DUP IF
	[ here forthstart 9 cells + ! ]
\ 	[ has? header [IF] here 9 cells ! [THEN] ] \ entry point for signal handler
\ [ has? backtrace [IF] ]
	store-backtrace
\ [ [THEN] ]
\ [ has? interpreter [IF] ]
	handler @ ?dup-0=-IF
\ [ has? os [IF] ]
	    cr .error cr
	    2 (bye)
\ [ [ELSE] ]
	    quit
\ [ [THEN] ]
	THEN
\ [ [THEN] ]
	rp!
	r> handler !
\ [ has? glocals [IF] ]
        r> lp!
\ [ [THEN] ]
	'throw
    THEN ;
is throw

