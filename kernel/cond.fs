\ Structural Conditionals                              12dec92py

\ Copyright (C) 1995,1996,1997,2000,2003,2004,2007 Free Software Foundation, Inc.

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

here 0 , \ just a dummy, the real value of locals-list is patched into it in glocals.fs
AConstant locals-list \ acts like a variable that contains
		      \ a linear list of locals names


variable dead-code \ true if normal code at "here" would be dead
variable backedge-locals
    \ contains the locals list that BEGIN will assume to be live on
    \ the back edge if the BEGIN is unreachable from above. Set by
    \ ASSUME-LIVE, reset by UNREACHABLE.

: UNREACHABLE ( -- ) \ gforth
    \ declares the current point of execution as unreachable
    dead-code on
    0 backedge-locals ! ; immediate

: ASSUME-LIVE ( orig -- orig ) \ gforth
    \ used immediatly before a BEGIN that is not reachable from
    \ above.  causes the BEGIN to assume that the same locals are live
    \ as at the orig point
    dup orig?
    2 pick backedge-locals ! ; immediate
    
\ Control Flow Stack
\ orig, etc. have the following structure:
\ type ( defstart, live-orig, dead-orig, dest, do-dest, scopestart) ( TOS )
\ address (of the branch or the instruction to be branched to) (second)
\ locals-list (valid at address) (third)

\ types
[IFUNDEF] defstart 
0 constant defstart	\ usally defined in comp.fs
[THEN]
1 constant live-orig
2 constant dead-orig
3 constant dest \ the loopback branch is always assumed live
4 constant do-dest
5 constant scopestart

: def? ( n -- )
    defstart <> abort" unstructured " ;

: orig? ( n -- )
 dup live-orig <> swap dead-orig <> and abort" expected orig " ;

: dest? ( n -- )
 dest <> abort" expected dest " ;

: do-dest? ( n -- )
 do-dest <> abort" expected do-dest " ;

: scope? ( n -- )
 scopestart <> abort" expected scope " ;

: non-orig? ( n -- )
 dest scopestart 1+ within 0= abort" expected dest, do-dest or scope" ;

: cs-item? ( n -- )
 live-orig scopestart 1+ within 0= abort" expected control flow stack item" ;

3 constant cs-item-size

: CS-PICK ( ... u -- ... destu ) \ tools-ext c-s-pick
 1+ cs-item-size * 1- >r
 r@ pick  r@ pick  r@ pick
 rdrop
 dup non-orig? ;

: CS-ROLL ( destu/origu .. dest0/orig0 u -- .. dest0/orig0 destu/origu ) \ tools-ext c-s-roll
 1+ cs-item-size * 1- >r
 r@ roll r@ roll r@ roll
 rdrop
 dup cs-item? ; 

: cs-push-part ( -- list addr )
 locals-list @ here ;

: cs-push-orig ( -- orig )
 cs-push-part dead-code @
 if
   dead-orig
 else
   live-orig
 then ;   

\ Structural Conditionals                              12dec92py

defer other-control-flow ( -- )
\ hook for control-flow stuff that's not handled by begin-like etc.

: ?struc      ( flag -- )       abort" unstructured " ;
: sys?        ( sys -- )        dup 0= ?struc ;
: >mark ( -- orig )
 cs-push-orig 0 , other-control-flow ;
: >resolve    ( addr -- )
    here swap !
    basic-block-end ;
: <resolve    ( addr -- )        , ;

: BUT
    1 cs-roll ;                      immediate restrict
: YET
    0 cs-pick ;                       immediate restrict

\ Structural Conditionals                              12dec92py

: AHEAD ( compilation -- orig ; run-time -- ) \ tools-ext
    POSTPONE branch  >mark  POSTPONE unreachable ; immediate restrict

: IF ( compilation -- orig ; run-time f -- ) \ core
 POSTPONE ?branch >mark ; immediate restrict

: ?DUP-IF ( compilation -- orig ; run-time n -- n| ) \ gforth	question-dupe-if
\G This is the preferred alternative to the idiom "@code{?DUP IF}", since it can be
\G better handled by tools like stack checkers. Besides, it's faster.
    POSTPONE ?dup-?branch >mark ;       immediate restrict

: ?DUP-0=-IF ( compilation -- orig ; run-time n -- n| ) \ gforth	question-dupe-zero-equals-if
    POSTPONE ?dup-0=-?branch >mark ;       immediate restrict

Defer then-like ( orig -- )
: cs>addr ( orig/dest -- )  drop >resolve drop ;
' cs>addr IS then-like

: THEN ( compilation orig -- ; run-time -- ) \ core
    dup orig?  then-like ; immediate restrict

' THEN alias ENDIF ( compilation orig -- ; run-time -- ) \ gforth
immediate restrict
\ Same as "THEN". This is what you use if your program will be seen by
\ people who have not been brought up with Forth (or who have been
\ brought up with fig-Forth).

: ELSE ( compilation orig1 -- orig2 ; run-time -- ) \ core
    POSTPONE ahead
    1 cs-roll
    POSTPONE then ; immediate restrict

Defer begin-like ( -- )
' noop IS begin-like

: BEGIN ( compilation -- dest ; run-time -- ) \ core
    begin-like cs-push-part dest
    basic-block-end ; immediate restrict

Defer again-like ( dest -- addr )
' nip IS again-like

: AGAIN ( compilation dest -- ; run-time -- ) \ core-ext
    dest? again-like  POSTPONE branch  <resolve ; immediate restrict

Defer until-like ( list addr xt1 xt2 -- )
:noname ( list addr xt1 xt2 -- )
    drop compile, <resolve drop ;
IS until-like

: UNTIL ( compilation dest -- ; run-time f -- ) \ core
    dest? ['] ?branch ['] ?branch-lp+!# until-like ; immediate restrict

: WHILE ( compilation dest -- orig dest ; run-time f -- ) \ core
    POSTPONE if
    1 cs-roll ; immediate restrict

: REPEAT ( compilation orig dest -- ; run-time -- ) \ core
    POSTPONE again
    POSTPONE then ; immediate restrict

\ counted loops

\ leave poses a little problem here
\ we have to store more than just the address of the branch, so the
\ traditional linked list approach is no longer viable.
\ This is solved by storing the information about the leavings in a
\ special stack.

\ !! remove the fixed size limit. 'Tis not hard.
20 constant leave-stack-size
create leave-stack  60 cells allot
Avariable leave-sp  leave-stack 3 cells + leave-sp !

: clear-leave-stack ( -- )
    leave-stack leave-sp ! ;

\ : leave-empty? ( -- f )
\  leave-sp @ leave-stack = ;

: >leave ( orig -- )
    \ push on leave-stack
    leave-sp @
    dup [ leave-stack 60 cells + ] Aliteral
    >= abort" leave-stack full"
    tuck ! cell+
    tuck ! cell+
    tuck ! cell+
    leave-sp ! ;

: leave> ( -- orig )
    \ pop from leave-stack
    leave-sp @
    dup leave-stack <= IF
       drop 0 0 0  EXIT  THEN
    cell - dup @ swap
    cell - dup @ swap
    cell - dup @ swap
    leave-sp ! ;

: DONE ( compilation orig -- ; run-time -- ) \ gforth
    \ !! the original done had ( addr -- )
    drop >r drop
    begin
	leave>
	over r@ u>=
    while
	POSTPONE then
    repeat
    >leave rdrop ; immediate restrict

: LEAVE ( compilation -- ; run-time loop-sys -- ) \ core
    POSTPONE ahead
    >leave ; immediate restrict

: ?LEAVE ( compilation -- ; run-time f | f loop-sys -- ) \ gforth	question-leave
    POSTPONE 0= POSTPONE if
    >leave ; immediate restrict

: DO ( compilation -- do-sys ; run-time w1 w2 -- loop-sys ) \ core
    POSTPONE (do)
    POSTPONE begin drop do-dest
    ( 0 0 0 >leave ) ; immediate restrict

: ?do-like ( -- do-sys )
    ( 0 0 0 >leave )
    >mark >leave
    POSTPONE begin drop do-dest ;

: ?DO ( compilation -- do-sys ; run-time w1 w2 -- | loop-sys )	\ core-ext	question-do
    POSTPONE (?do) ?do-like ; immediate restrict

: +DO ( compilation -- do-sys ; run-time n1 n2 -- | loop-sys )	\ gforth	plus-do
    POSTPONE (+do) ?do-like ; immediate restrict

: U+DO ( compilation -- do-sys ; run-time u1 u2 -- | loop-sys )	\ gforth	u-plus-do
    POSTPONE (u+do) ?do-like ; immediate restrict

: -DO ( compilation -- do-sys ; run-time n1 n2 -- | loop-sys )	\ gforth	minus-do
    POSTPONE (-do) ?do-like ; immediate restrict

: U-DO ( compilation -- do-sys ; run-time u1 u2 -- | loop-sys )	\ gforth	u-minus-do
    POSTPONE (u-do) ?do-like ; immediate restrict

: FOR ( compilation -- do-sys ; run-time u -- loop-sys )	\ gforth
    POSTPONE (for)
    POSTPONE begin drop do-dest
    ( 0 0 0 >leave ) ; immediate restrict

\ LOOP etc. are just like UNTIL

: loop-like ( do-sys xt1 xt2 -- )
    >r >r 0 cs-pick swap cell - swap 1 cs-roll r> r> rot do-dest?
    until-like  POSTPONE done  POSTPONE unloop ;

: LOOP ( compilation do-sys -- ; run-time loop-sys1 -- | loop-sys2 )	\ core
 ['] (loop) ['] (loop)-lp+!# loop-like ; immediate restrict

: +LOOP ( compilation do-sys -- ; run-time loop-sys1 n -- | loop-sys2 )	\ core	plus-loop
 ['] (+loop) ['] (+loop)-lp+!# loop-like ; immediate restrict

\ !! should the compiler warn about +DO..-LOOP?
: -LOOP ( compilation do-sys -- ; run-time loop-sys1 u -- | loop-sys2 )	\ gforth	minus-loop
 ['] (-loop) ['] (-loop)-lp+!# loop-like ; immediate restrict

\ A symmetric version of "+LOOP". I.e., "-high -low ?DO -inc S+LOOP"
\ will iterate as often as "high low ?DO inc S+LOOP". For positive
\ increments it behaves like "+LOOP". Use S+LOOP instead of +LOOP for
\ negative increments.
: S+LOOP ( compilation do-sys -- ; run-time loop-sys1 n -- | loop-sys2 )	\ gforth	s-plus-loop
 ['] (s+loop) ['] (s+loop)-lp+!# loop-like ; immediate restrict

: NEXT ( compilation do-sys -- ; run-time loop-sys1 -- | loop-sys2 ) \ gforth
 ['] (next) ['] (next)-lp+!# loop-like ; immediate restrict

\ Structural Conditionals                              12dec92py

Defer exit-like ( -- )
' noop IS exit-like

: EXIT ( compilation -- ; run-time nest-sys -- ) \ core
\G Return to the calling definition; usually used as a way of
\G forcing an early return from a definition. Before
\G @code{EXIT}ing you must clean up the return stack and
\G @code{UNLOOP} any outstanding @code{?DO}...@code{LOOP}s.
    exit-like
    POSTPONE ;s
    basic-block-end
    POSTPONE unreachable ; immediate restrict

: ?EXIT ( -- ) ( compilation -- ; run-time nest-sys f -- | nest-sys ) \ gforth
     POSTPONE if POSTPONE exit POSTPONE then ; immediate restrict

