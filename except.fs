\ catch, throw, etc.

\ Copyright (C) 1999,2000,2003 Free Software Foundation, Inc.

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

\ Ok, here's the story about how we get to the native code for the
\ recovery code in case of a THROW, and why there is all this funny
\ stuff being compiled by TRY and RECOVER:

\ Upon a THROW, we cannot just return through the ordinary return
\ address, but have to use a different one, for code after the
\ RECOVER.  How do we do that, in a way portable between the various
\ threaded and native code engines?  In particular, how does the
\ native code engine learn about the address of the native recovery
\ code?

\ On the Forth level, we can compile only references to threaded code.
\ The only thing that translates a threaded code address to a native
\ code address is docol, which is only called with EXECUTE and
\ friends.  So we start the recovery code with a docol, and invoke it
\ with PERFORM; the recovery code then rdrops the superfluously
\ generated return address and continues with the proper recovery
\ code.

\ At compile time, since we cannot compile a forward reference (to the
\ recovery code) as a literal (backpatching does not work for
\ native-code literals), we produce a data cell (wrapped in AHEAD
\ ... THEN) that we can backpatch, and compile the address of that as
\ literal.

\ Overall, this leads to the following resulting code:

\   ahead
\ +><recovery address>-+
\ | then               |
\ +-lit                |
\   (try)              |
\   ...                |
\   (recover)          |
\   ahead              |
\   docol: <-----------+
\   rdrop
\   ...
\   then
\   ...

\ !! explain handler on-stack structure

: (try) ( ahandler -- )
    r>
    swap >r \ recovery address
    rp@ 'catch >r
    sp@ >r
    fp@ >r
    lp@ >r
    handler @ >r
    rp@ handler !
    backtrace-empty on
    >r ;

: try ( compilation  -- orig ; run-time  -- ) \ gforth
    \ !! does not work correctly for gforth-native
    POSTPONE ahead here >r >mark 1 cs-roll POSTPONE then
    r> POSTPONE literal POSTPONE (try) ; immediate compile-only

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
    POSTPONE else
    docol: here 0 , 0 , code-address! \ start a colon def 
    postpone rdrop                    \ drop the return address
; immediate compile-only

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
	    >stderr cr ." uncaught exception: " .error cr
	    2 (bye)
\	    quit
	THEN
	rp!
	r> handler !
	r> lp!
	r> fp!
	r> swap >r sp! drop r>
	rdrop 'throw r> perform
    THEN ;
is throw

