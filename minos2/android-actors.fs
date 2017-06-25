\ MINOS2 actors on Android

\ Copyright (C) 2017 Free Software Foundation, Inc.

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

require bits.fs

2 sfloats buffer: lastpos
2Variable lasttime
2Variable downtime
Variable flags
Variable buttonmask
0 Value clicks

0 Constant #pending
1 Constant #lastdown
2 Constant #clearme

: top-act ( -- o ) top-widget .act ;

#250 Value twoclicks  \ every edge further apart than 150ms into separate clicks
#128e FValue samepos     \ position difference square-summed less than is same pos

: 2sf@ ( addr -- r1 r2 )
    dup sf@ sfloat+ sf@ ;
: 2sf! ( r1 r2 addr -- )
    dup sfloat+ sf! sf! ;
: samepos? ( rx ry -- flag )
    lastpos 2sf@ frot f- f**2 f-rot f- f**2 f+ samepos f< ;
: ?samepos ( -- )
    0 getX 0 getY fover fover samepos? 0= IF   0 to clicks  THEN  lastpos 2sf! ;

Variable xy$
: >xy$ ( -- xy$ )
    getPointerCount 2* sfloats xy$ $!len
    0 xy$ $@ bounds ?DO
	dup getX I sf!  dup getY I sfloat+ sf!  1+
    2 sfloats +LOOP  drop xy$
    getDownTime downtime 2!
    getEventTime lasttime 2! ;
: action-down ( -- )
    getButtonState buttonmask !
    top-act IF  xy$ buttonmask @ top-act .touchdown  THEN
    ?samepos  flags #lastdown +bit  flags #pending +bit ;
: action-up ( -- )
    ?samepos
    flags #lastdown -bit@  IF
	1 +to clicks  send-clicks  flags #clearme +bit  THEN
    getButtonState buttonmask !
    top-act IF  xy$ buttonmask @ top-act .touchup  THEN ;
: action-move ( -- )
    flags #pending bit@  0 getX 0 getY samepos? 0= and IF
	send-clicks  0 to clicks
    THEN
    top-act IF  xy$ buttonmask @ top-act .touchmove  THEN ;
: action-cancel ( -- ) ;
: action-outside ( -- ) ;
: action-ptr-down ( -- )
    getButtonState buttonmask !
    top-act IF  xy$ buttonmask @ top-act .touchdown  THEN ;
: action-ptr-up ( -- )
    top-act IF  xy$ buttonmask @ top-act .touchup  THEN
    getButtonState buttonmask ! ;
: action-hover-move ( -- )
    getButtonState buttonmask !
    top-act IF  xy$ 0 top-act .touchmove  THEN ;
: action-scroll ( -- ) ;
: action-henter ( -- ) ;
: action-hexit ( -- ) ;

Create actions
' action-down ,         \ 0
' action-up ,           \ 1
' action-move ,         \ 2
' action-cancel ,       \ 3
' action-outside ,      \ 4
' action-ptr-down ,     \ 5
' action-ptr-up ,       \ 6
' action-hover-move ,   \ 7
' action-scroll ,       \ 8
' action-henter ,       \ 9
' action-hexit ,        \ a

: touch>action ( event -- )  new-touch on
    dup to touch-event >o  >xy$ drop
    me-getAction $FF and dup $A <= IF
	cells actions + perform
    ELSE  drop  THEN
    o> ;

: send-clicks ( -- )
    lastpos 2sf@ buttonmask @
    clicks 2* flags #lastdown bit@ -
    top-act ?dup-IF
	.clicked
    ELSE  2drop fdrop fdrop  THEN
    flags #pending -bit ;

:noname ( -- )
    uptimeMillis lasttime 2@ d- twoclicks s>d d>= IF
	flags #pending -bit@ IF
	    send-clicks  flags #pending -bit
	THEN
	flags #clearme -bit@ IF
	    0 to clicks
	THEN
    THEN ; is ?looper-timeouts

: enter-minos ( -- )
    ['] touch>action is android-touch ;
: leave-minos ( -- )
    ['] touch>event is android-touch
    need-sync on  need-show on ;