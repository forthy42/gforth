\ catch, throw, etc.

\ Copyright (C) 1999,2000,2003,2006,2007 Free Software Foundation, Inc.

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

\ !! use a separate exception stack?           anton

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

Variable first-throw
: nothrow ( -- ) \ gforth
    \G Use this (or the standard sequence @code{['] false catch drop})
    \G after a @code{catch} or @code{endtry} that does not rethrow;
    \G this ensures that the next @code{throw} will record a
    \G backtrace.
    first-throw on ;

: (try) ( ahandler -- )
    first-throw on
    r>
    swap >r \ recovery address
    sp@ >r
    fp@ >r
    lp@ >r
    handler @ >r
    rp@ handler !
    >r ;

: try ( compilation  -- orig ; run-time  -- R:sys1 ) \ gforth
    \G Start an exception-catching region.
    POSTPONE ahead here >r >mark 1 cs-roll POSTPONE then
    r> POSTPONE literal POSTPONE (try) ; immediate compile-only

: (endtry) ( -- )
    \ normal end of try block: restore handler, forget rest
    r>
    r> handler !
    rdrop \ lp
    rdrop \ fp
    rdrop \ sp
    rdrop \ recovery address
    >r ;

: handler-intro, ( -- )
    docol: here 0 , 0 , code-address! \ start a colon def 
    postpone rdrop                    \ drop the return address
;

: iferror ( compilation  orig1 -- orig2 ; run-time  -- ) \ gforth
    \G Starts the exception handling code (executed if there is an
    \G exception between @code{try} and @code{endtry}).  This part has
    \G to be finished with @code{then}.
    \ !! check using a special tag
    POSTPONE else handler-intro,
; immediate compile-only

: restore ( compilation  orig1 -- ; run-time  -- ) \ gforth
    \G Starts restoring code, that is executed if there is an
    \G exception, and if there is no exception.
    POSTPONE iferror POSTPONE then
; immediate compile-only

: endtry ( compilation  -- ; run-time  R:sys1 -- ) \ gforth
    \G End an exception-catching region.
    POSTPONE (endtry)
; immediate compile-only

: endtry-iferror ( compilation  orig1 -- orig2 ; run-time  R:sys1 -- ) \ gforth
    \G End an exception-catching region while starting
    \G exception-handling code outside that region (executed if there
    \G is an exception between @code{try} and @code{endtry-iferror}).
    \G This part has to be finished with @code{then} (or
    \G @code{else}...@code{then}).
    POSTPONE (endtry) POSTPONE iferror POSTPONE (endtry)
; immediate compile-only

:noname ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
    try
	execute 0
    iferror
	nip
    then endtry ;
is catch

:noname ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
    ?DUP IF
	[ here forthstart 9 cells + ! ]
	first-throw @ IF
	    store-backtrace error-stack off
	    first-throw off
	THEN
	handler @ ?dup-0=-IF
	    >stderr cr ." uncaught exception: " .error cr
	    2 (bye)
\	    quit
	THEN
        dup rp! ( ... ball frame )
        cell+ dup @ lp!
        cell+ dup @ fp!
        cell+ dup @ ( ... ball addr sp ) -rot 2>r sp! drop 2r>
        cell+ @ perform
    THEN ;
is throw
