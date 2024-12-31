\ catch, throw, etc.

\ Authors: Anton Ertl, Bernd Paysan, Gerald Wodni
\ Copyright (C) 1999,2000,2003,2006,2007,2010,2013,2014,2015,2016,2017,2019,2020,2022,2023,2024 Free Software Foundation, Inc.

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

\ !! explain handler on-stack structure

[undefined] first-throw [if]
    User first-throw  \ contains true if the next throw is the first throw
[then]
User stored-backtrace ( addr -- )
\ contains the address of a cell-counted string that contains a copy
\ of the return stack at the throw

: nothrow ( -- ) \ gforth
    \G Use this (or the standard sequence @code{['] false catch 2drop})
    \G after a @code{catch} or @code{endtry} that does not rethrow;
    \G this ensures that the next @code{throw} will record a
    \G backtrace.
    first-throw on ;

: try ( compilation  -- orig ; run-time  -- R:sys1 ) \ gforth
    \G Start an exception-catching region.
    POSTPONE (try) >mark
; immediate compile-only

: iferror ( compilation  orig1 -- orig2 ; run-time  -- ) \ gforth
    \G Starts the exception handling code (executed if there is an
    \G exception between @code{try} and @code{endtry}).  This part has
    \G to be finished with @code{then}.
    \ !! check using a special tag
    POSTPONE else
; immediate compile-only

: restore ( compilation  orig1 -- ; run-time  -- ) \ gforth
    \G Starts restoring code, that is executed if there is an
    \G exception, and if there is no exception.
    POSTPONE iferror POSTPONE then
; immediate compile-only

: endtry ( compilation  -- ; run-time  R:sys1 -- ) \ gforth
    \G End an exception-catching region.
    POSTPONE uncatch
; immediate compile-only

: endtry-iferror ( compilation  orig1 -- orig2 ; run-time  R:sys1 -- ) \ gforth
    \G End an exception-catching region while starting
    \G exception-handling code outside that region (executed if there
    \G is an exception between @code{try} and @code{endtry-iferror}).
    \G This part has to be finished with @code{then} (or
    \G @code{else}...@code{then}).
    POSTPONE uncatch POSTPONE iferror POSTPONE uncatch
; immediate compile-only

0 Value catch-frame

:noname ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
    try
	execute [ here to catch-frame ] 0 uncatch exit
    iferror
	nip
    then endtry ;
is catch

: catch-nobt ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ gforth-experimental
    \G perform a catch that does not record backtraces on errors
    1 first-throw !@ >r
    die-on-signal @ dup >r IF  1 die-on-signal +!  THEN
    catch r> die-on-signal ! r> first-throw ! ;

Defer kill-task ( -- ) \ gforth-experimental
\G Terminate the current task.
' noop IS kill-task

Variable located-view
Variable located-len
variable bn-view      \ first contains located-view, but is updated by B and N
variable located-top  \ first line to display with l
variable located-bottom \ last line to display with l
2variable located-slurped \ the contents of the file in located-view, or 0 0

\ lines to show before and after locate
3 value before-locate ( -- u ) \ gforth
\G number of lines shown before current location (default 3).
12 value after-locate ( -- u ) \ gforth
\G number of lines shown after current location (default 12).

: view>filename# ( view -- u ) \ gforth-internal
    \G filename-number of view (obtained by @code{name>view}) see @code{filename#>str}
    23 rshift ;

: view>line ( view -- u ) \ gforth-internal
    \G line number in file of view (obtained by @code{name>view})
    8 rshift $7fff and ;

: set-located-view ( view len -- )
    located-len ! dup located-view ! dup bn-view !
    view>line
    dup before-locate - 0 max located-top !
    after-locate + located-bottom ! ;

: set-current-view ( -- )
    current-sourceview input-lexeme @ set-located-view ;

[IFDEF] ?set-current-view
    :noname error-stack $@len 0= IF  set-current-view  THEN ;
    is ?set-current-view
[THEN]

\ : set-current-view ( -- )
\    input-lexeme @ located-len ! current-sourceview located-view ! ;

:noname ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
    ?DUP-IF
	[ here throw-entry ! ]
	first-throw @ 0< IF
	    store-backtrace
	THEN
	handler @ IF
	    fast-throw THEN
	>stderr cr ." uncaught exception: " .error cr
	kill-task  2 (bye)
    THEN ;
is throw

0 [if]
    \ primitives for wrap..end-wrap

    \ removed from prim, because the benefits compared to
    \ catch...throw are slim
    \ <2017May30.172812@mips.complang.tuwien.ac.at>
    
pushwrap ( ... #a_recovery -- R:a_recovery R:a_sp R:c_op R:f_fp R:c_lp
    \ R:a_oldhandler ) gforth-internal
a_oldhandler = SPs->wraphandler;
a_sp = sp;
c_op = op;
f_fp = fp;
c_lp = lp;
SPs->wraphandler = rp;

dropwrap ( R:a_recovery R:a_sp R:c_op R:f_fp R:c_lp R:a_oldhandler -- ) gforth-internal
SPs->wraphandler = a_oldhandler;

exit-wrap ( ... -- ... ) gforth-experimental exit_wrap
rp = SPs->wraphandler;
SPs->wraphandler = (Cell *)rp[-6];
lp = (Address)rp[-5];
fp = (Float *)rp[-4];
op = (Char *)rp[-3];
sp = (Cell *)rp[-2];
SET_IP((Xt *)rp[-1]);
[then]
    
[defined] pushwrap [if]
\ usage: wrap ... end-wrap
\ or:    wrap ... wrap-onexit ... then
\ in combination with: exit-wrap

: wrap ( compilation: -- orig; run-time: -- r:sys ) \ gforth-experimental
    POSTPONE pushwrap >mark ; immediate compile-only

: end-wrap ( compilation: orig --; run-time: r:sys -- ) \ gforth-experimental
    POSTPONE dropwrap POSTPONE then ; immediate compile-only

: wrap-onexit ( compilation: orig --; run-time: r:sys -- ) \ gforth-experimental
    POSTPONE dropwrap POSTPONE else ; immediate compile-only
[then]

