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

Variable xy$
: >xy$ ( -- xy$ )
    getPointerCount 2* sfloats xy$ $!len
    0 xy$ $@ bounds ?DO
	dup getX I sf!  dup getY I sfloat+ sf!
    2 sfloats +LOOP  xy$ ;
: action-down ( -- )
    top-act IF  xy$ getButtonState top-act .touchdown  THEN ;
: action-up ( -- )
    top-act IF  xy$ getButtonState top-act .touchup  THEN ;
: action-move ( -- )
    top-act IF  xy$ getButtonState top-act .touchmove  THEN ;
: action-cancel ( -- ) ;
: action-outside ( -- ) ;
: action-ptr-down ( -- )
    top-act IF  xy$ getButtonState top-act .touchdown  THEN ;
: action-ptr-up ( -- )
    top-act IF  xy$ getButtonState top-act .touchup  THEN ;
: action-hover-move ( -- )
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

: enter-minos ( -- )
    ['] touch>action is android-touch ;
: leave-minos ( -- )
    ['] touch>event is android-touch
    need-sync on  need-show on ;