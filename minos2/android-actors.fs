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

#250 Value twoclicks  \ every edge further apart than 250ms into separate clicks
128e FValue samepos   \ position difference square-summed less than is same pos

: 2sf@ ( addr -- r1 r2 )
    dup sf@ sfloat+ sf@ ;
: 2sf! ( r1 r2 addr -- )
    dup sfloat+ sf! sf! ;
: getXY ( index -- rx ry )
    dup getX screen-xy cell+ @ s>f f-
        getY screen-xy       @ s>f f- ;
: samepos? ( rx ry -- flag )
    lastpos 2sf@ frot f- f**2 f-rot f- f**2 f+ samepos f< ;
: ?samepos ( -- )
    0 getXY fover fover samepos? 0= IF   0 to clicks  THEN  lastpos 2sf! ;

Variable xy$
: >xy$ ( -- xy$ )
    getPointerCount 2* sfloats xy$ $!len
    0 xy$ $@ bounds ?DO
	dup getXY I 2sf! 1+
    2 sfloats +LOOP  drop xy$
    getDownTime downtime 2!
    getEventTime lasttime 2! ;
: send-clicks ( -- )
    lastpos 2sf@ buttonmask @
    clicks 2* flags #lastdown bit@ -
    top-act ?dup-IF
	.clicked
    ELSE  2drop fdrop fdrop  THEN
    flags #pending -bit ;

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
    flags #pending bit@  0 getXY samepos? 0= and IF
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
    me-getAction $FF and dup $A u<= IF
	cells actions + perform
    ELSE  drop  THEN
    o> ;

: togglekb-0  togglekb 0 ;
: aback-0     aback    0 ;

Create keycode>ekey
AKEYCODE_HOME        , ' k-home   ,
AKEYCODE_DPAD_UP     , ' k-up     ,
AKEYCODE_DPAD_DOWN   , ' k-down   ,
AKEYCODE_VOLUME_UP   , ' k-up     ,
AKEYCODE_VOLUME_DOWN , ' k-down   ,
AKEYCODE_DPAD_LEFT   , ' k-left   ,
AKEYCODE_DPAD_RIGHT  , ' k-right  ,
AKEYCODE_TAB         , ' #tab     ,
AKEYCODE_ENTER       , ' #cr      ,
AKEYCODE_DEL         , ' #bs      ,
AKEYCODE_FORWARD_DEL , ' k-delete ,
AKEYCODE_PAGE_UP     , ' k-prior  ,
AKEYCODE_PAGE_DOWN   , ' k-next   ,
AKEYCODE_MOVE_HOME   , ' k-home   ,
AKEYCODE_MOVE_END    , ' k-end    ,
AKEYCODE_INSERT      , ' k-insert ,
AKEYCODE_F1          , ' k-f1     ,
AKEYCODE_F2          , ' k-f2     ,
AKEYCODE_F3          , ' k-f3     ,
AKEYCODE_F4          , ' k-f4     ,
AKEYCODE_F5          , ' k-f5     ,
AKEYCODE_F6          , ' k-f6     ,
AKEYCODE_F7          , ' k-f7     ,
AKEYCODE_F8          , ' k-f8     ,
AKEYCODE_F9          , ' k-f9     ,
AKEYCODE_F10         , ' k-f10    ,
AKEYCODE_F11         , ' k-f11    ,
AKEYCODE_F12         , ' k-f12    ,
AKEYCODE_MENU        , ' togglekb-0 ,
AKEYCODE_BACK        , ' aback-0  ,
0 , ' false ,
DOES> ( akey -- ekey )
  swap >r
  BEGIN  dup cell+ swap @ r@ <> WHILE
      dup cell+ swap @ 0= UNTIL  r@ unknown-key# !
  THEN  perform rdrop ;

also jni

: key>action ( event -- )
    dup to key-event >o
    ke_getMetaState meta-key# !
    getAction dup 2 = IF  drop
	getKeyCode
	?dup-IF  keycode>ekey ?dup-IF top-act .ekeyed THEN
	ELSE  nostring getCharacters jstring>sstring top-act .ukeyed jfree
	THEN
    ELSE
	0= IF
	    getUnicodeChar
	    ?dup-IF  >xstring top-act .ukeyed
	    ELSE  getKeyCode keycode>ekey ?dup-IF top-act .ekeyed THEN
	    THEN
	THEN
    THEN o> ;

:noname ( -- )
    uptimeMillis lasttime 2@ d- twoclicks s>d d>= IF
	flags #pending -bit@ IF
	    send-clicks  flags #pending -bit
	THEN
	flags #clearme -bit@ IF
	    0 to clicks
	THEN
    THEN ; is ?looper-timeouts

: edit-setstring ( string -- )
    jstring>sstring setstring$ $! jfree
    need-sync on  need-glyphs on ;
: edit-commit ( string/0 -- )  ?dup-IF
	jstring>sstring setstring$ $! jfree
    THEN
    setstring$ @ { w^ s$ } setstring$ off
    s$ $@
    BEGIN  dup  WHILE  over c@ #del =  WHILE
		2>r #bs top-act .ekeyed 2r> 1 /string  REPEAT  THEN
    BEGIN  dup  WHILE  2dup "\e[3~" string-prefix?  WHILE
		2>r k-delete top-act .ekeyed 2r> 4 /string  REPEAT  THEN
    ?dup-IF  top-act .ukeyed  ELSE  drop  THEN
    s$ $free ;

previous

: enter-minos ( -- )
    edit-widget edit-out !  need-ap on
    ['] touch>action   is android-touch
    ['] key>action     is android-key
    ['] edit-setstring is android-setstring
    ['] edit-commit    is android-commit ;
: leave-minos ( -- )
    edit-terminal edit-out !
    ['] touch>event is android-touch
    ['] key>event   is android-key
    [ action-of android-setstring ]L is android-setstring
    [ action-of android-commit ]L is android-commit
    need-sync on  need-show on ;