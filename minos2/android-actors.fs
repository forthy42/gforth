\ MINOS2 actors on Android

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2017,2018,2019 Free Software Foundation, Inc.

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

also jni

2 sfloats buffer: lastpos
2Variable lasttime
2Variable downtime
Variable flags
Variable buttonmask
0 Value clicks

: lasttime@ ( -- seconds ) lasttime 2@ d>f 1m f* ;

0 Constant #pending
1 Constant #lastdown
2 Constant #clearme

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
    flags #pending -bit
    grab-move? ?dup-IF  fswap gx-sum f+ fswap gy-sum f+
	[: .clicked ;] vp-needed<>| EXIT  THEN
    top-act    ?dup-IF  .clicked  EXIT  THEN
    2drop fdrop fdrop ;

: action-down ( -- )
    getButtonState buttonmask !
    grab-move? IF  xy$ >dxy buttonmask @ [: grab-move? .touchdown ;] vp-needed<>|  ELSE
    top-act    IF  xy$ buttonmask @ top-act .touchdown  THEN  THEN
    ?samepos  flags #lastdown +bit  flags #pending +bit ;
: action-up ( -- )
    ?samepos
    getButtonState buttonmask !
    grab-move? IF  xy$ >dxy buttonmask @ [: grab-move? .touchup ;] vp-needed<>|  ELSE
    top-act IF  xy$ buttonmask @ top-act .touchup  THEN  THEN
    flags #lastdown -bit@  IF
	1 +to clicks  flags #clearme +bit  send-clicks  THEN ;
: action-move ( -- )
    flags #pending bit@  0 getXY samepos? 0= and IF
	send-clicks  0 to clicks
    THEN
    grab-move?  IF  xy$ >dxy buttonmask @ grab-move? [: .touchmove ;] vp-needed<>|  EXIT
    THEN
    top-act     IF  xy$ buttonmask @ top-act    .touchmove  THEN ;
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
    grab-move?  IF  xy$ >dxy 0 grab-move? [: .touchmove ;] vp-needed<>|  EXIT  THEN
    top-act     IF  xy$ 0 top-act    .touchmove  THEN ;
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
AKEYCODE_VOLUME_UP   , ' k-volup  ,
AKEYCODE_VOLUME_DOWN , ' k-voldown ,
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
here >r DOES> ( akey -- ekey ) [ r> ]l swap DO
    dup I @ = IF  drop I cell+ perform  UNLOOP  EXIT  THEN
[ 2 cells ]L +LOOP
dup AKEYCODE_A AKEYCODE_Z 1+ within IF
    meta-key# @ AMETA_CTRL_ON and IF
	AKEYCODE_A - 1+  EXIT
    THEN
    meta-key# @ AMETA_SHIFT_ON and IF
	AKEYCODE_A - 'A' +  EXIT
    THEN
    meta-key# @ AMETA_ALT_ON and IF
	drop 0  EXIT
    THEN
    meta-key# @ AMETA_META_ON and IF
	drop 0  EXIT
    THEN
    AKEYCODE_A - 'a' +
ELSE
    ~~ unknown-key# ! 0
THEN ;

also android

: gui-inskey ( key -- )
    level# @ 0= IF  inskey  ELSE  top-act .ekeyed  THEN ;
' gui-inskey is android-inskey

previous

: key>action ( event -- )
    dup to key-event >o
    ke_getMetaState meta-key# !
    getAction dup 2 = IF  drop
	getKeyCode
	?dup-IF  keycode>ekey ?dup-IF top-act .ekeyed THEN
	ELSE  nostring getCharacters jstring>sstring
	    top-act .ukeyed jfree
	THEN
    ELSE
	0= IF
	    getUnicodeChar
	    ?dup-IF
		dup bl < IF  top-act .ekeyed \ pass control characters to ekeyed
		ELSE  >xstring top-act .ukeyed  THEN
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
    jstring>sstring setstring$ $! jfree +sync +config ;
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
: edit-context-menu ( n -- )
    case
	$0102001f of  k-sel   top-act .ekeyed  endof \ select all
	$01020020 of  ctrl X  top-act .ekeyed  endof \ cut
	$01020021 of  ctrl C  top-act .ekeyed  endof \ copy
	$01020022 of  ctrl V  top-act .ekeyed  endof \ paste
	$0102002c of  k-home  top-act .ekeyed  endof \ home
    endcase ;
previous

: enter-minos ( -- )
    edit-widget edit-out !
    ['] touch>action      is android-touch
    ['] key>action        is android-key
    ['] edit-setstring    is android-setstring
    ['] edit-commit       is android-commit
    ['] edit-context-menu is android-context-menu ;
: preserve ( "name" -- )
    ' dup defer@ lit, (to), ; immediate
: leave-minos ( -- )
    edit-terminal edit-out !
    preserve android-touch
    preserve android-key
    preserve android-setstring
    preserve android-commit
    preserve android-context-menu
    [IFDEF] terminal-program
	terminal-program terminal-init term-textures [THEN]
    +sync +config +show ;
