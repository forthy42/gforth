\ catch, throw, etc.

\ Copyright (C) 1999,2000 Free Software Foundation, Inc.

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

\ !! use a separate exception stack?           anton

\ user-definable rollback actions

Defer 'catch
Defer 'throw

' noop IS 'catch
' noop IS 'throw

\ has? backtrace [IF]
Defer store-backtrace
' noop IS store-backtrace
\ [THEN]

: (try) ( -- )
    \ inline argument: address of the handler
    r>
    dup @ >r \ recovery address
    rp@ 'catch >r
    sp@ >r
    fp@ >r
    lp@ >r
    handler @ >r
    rp@ handler !
    backtrace-empty on
    cell+ >r ;

: try ( compilation  -- orig ; run-time  -- ) \ gforth
    POSTPONE (try) >mark ; immediate compile-only

: (recover) ( -- )
    \ normal end of try block: restore handler, forget rest
    r>
    r> handler !
    rdrop \ lp
    rdrop \ fp
    rdrop \ sp
    r> rp!
    rdrop \ recovery address
    >r ;

: recover ( compilation  orig1 -- orig2 ; run-time  -- ) \ gforth
    \ !! check using a special tag
    POSTPONE (recover)
    POSTPONE else ; immediate compile-only

: endtry ( compilation  orig -- ; run-time  -- ) \ gforth
    POSTPONE then ; immediate compile-only

:noname ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
    try
	execute 0
    recover
        nip
    endtry ;
is catch

:noname ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
    ?DUP IF
	[ here forthstart 9 cells + ! ]
	store-backtrace
	handler @ ?dup-0=-IF
	    cr ." uncaught exception: " .error cr
	    2 (bye)
	    quit
	THEN
	rp!
	r> handler !
	r> lp!
	r> fp!
	r> swap >r sp! drop r>
	rdrop 'throw
    THEN ;
is throw

