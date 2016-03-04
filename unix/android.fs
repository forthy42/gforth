\ wrapper to load Swig-generated libraries

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

require struct0x.fs

\ public interface, C calls us through these

get-current also forth definitions

previous set-current

\ The rest is in the "android" vocabulary

Vocabulary android
get-current also android definitions

Defer akey

c-library android
    \c #include <android/input.h>
    \c #include <android/keycodes.h>
    \c #include <android/native_window.h>
    \c #include <android/native_window_jni.h>
    \c #include <android/native_activity.h>
    \c #include <android/looper.h>

    begin-structure app_input_state
	field: action
	field: flags
	field: metastate
	field: edgeflags
	field: pressure
	field: size
	2field: downtime
	field: tcount
	field: x0
	field: y0
	field: x1
	field: y1
	field: x2
	field: y2
	field: x3
	field: y3
	field: x4
	field: y4
	field: x5
	field: y5
	field: x6
	field: y6
	field: x7
	field: y7
	field: x8
	field: y8
	field: x9
	field: y9
    end-structure
    
    begin-structure startargs
	field: app-vm
	field: app-env
	field: obj
	field: cls
	field: thread-id
	lfield: ke-fd0
	lfield: ke-fd1
	field: window \ native window
    end-structure
    
    s" android" add-lib

    include unix/androidlib.fs
    
end-c-library

require unix/cpufeatureslib.fs \ load into Android vocabulary

s" APP_STATE" getenv s>number drop Value app

get-current also forth definitions

require unix/socket.fs
require unix/pthread.fs
require unix/jni-helper.fs

set-current previous

Variable need-sync need-sync on
Variable need-show need-show on

app_input_state buffer: *input

\ *input app userData !

require string.fs

4 buffer: xstring

: >xstring ( xchar -- addr u )
    xstring xc!+ xstring tuck - ;

\ keycode to escape sequence table

: $, ( addr u -- )  here over 1+ allot place ;

Variable unknown-key#
Variable meta-key#

Create akey>ekey
AKEYCODE_HOME c, "\e[H" $,
AKEYCODE_BACK c, "\b" $,
AKEYCODE_DPAD_UP c, "\e[A" $,
AKEYCODE_DPAD_DOWN c, "\e[B" $,
AKEYCODE_VOLUME_UP c, "\e[A" $,
AKEYCODE_VOLUME_DOWN c, "\e[B" $,
AKEYCODE_DPAD_LEFT c, "\e[D" $,
AKEYCODE_DPAD_RIGHT c, "\e[C" $,
AKEYCODE_TAB c, "\t" $,
AKEYCODE_ENTER c, "\r" $,
AKEYCODE_DEL c, "\b" $, \ is not delete, is backspace!
AKEYCODE_FORWARD_DEL c, "\e[3~" $, \ this is the real delete
AKEYCODE_PAGE_UP c, "\e[5~" $,
AKEYCODE_PAGE_DOWN c, "\e[6~" $,
AKEYCODE_MOVE_HOME c, "\e[H" $,
AKEYCODE_MOVE_END c, "\e[F" $,
AKEYCODE_INSERT c, "\e[2~" $,
AKEYCODE_F1  c, "\eOP"  $,
AKEYCODE_F2  c, "\eOQ"  $,
AKEYCODE_F3  c, "\eOR"  $,
AKEYCODE_F4  c, "\eOS"  $,
AKEYCODE_F5  c, "\e[15~" $,
AKEYCODE_F6  c, "\e[17~" $,
AKEYCODE_F7  c, "\e[18~" $,
AKEYCODE_F8  c, "\e[19~" $,
AKEYCODE_F9  c, "\e[20~" $,
AKEYCODE_F10 c, "\e[21~" $,
AKEYCODE_F11 c, "\e[23~" $,
AKEYCODE_F12 c, "\e[24~" $,
0 c,
DOES> ( akey -- addr u )
  swap >r
  BEGIN  count dup r@ <> and WHILE  count +
      dup c@ 0=  UNTIL  r@ unknown-key# !
  THEN  count rdrop ;

\ ainput implementation

Variable level#
Defer aback
:noname  -1 level# +!  level# @ 0< IF  bye  THEN ; IS aback

also jni

0 Value screen-orientation

: screen-orientation@ ( -- 0..3 )
    clazz >o getWindowManager >o getDefaultDisplay >o
    getRotation ref> ref> o> ;

$80 Constant FLAG_KEEP_SCREEN_ON

false value wake-lock \ doesn't work, why?

: screen+keep ( -- )  wake-lock IF
	clazz >o getWindow o> >o FLAG_KEEP_SCREEN_ON addFlags ref> THEN ;
: screen-keep ( -- )  wake-lock IF
	clazz >o getWindow o> >o FLAG_KEEP_SCREEN_ON clearFlags ref> THEN ;

\ callbacks

: $err ( xt -- )  $tmp stderr write-file throw ;

\ event handling

Create ctrl-key# 0 c,

: meta@ ( -- char ) \ return meta in vt100 form
    0
    meta-key# @ AMETA_SHIFT_ON and 0<> 1 and  or
    meta-key# @ AMETA_ALT_ON   and 0<> 2 and  or
    meta-key# @ AMETA_CTRL_ON  and 0<> 4 and  or  '1' + ;

: +meta ( addr u -- addr' u' ) \ insert meta information
    over c@ #esc <> ?EXIT
    meta@ dup '1' = IF  drop  EXIT  THEN \ no meta, don't insert
    [: >r 1- 2dup + c@ >r
	over 1+ c@ '[' = IF
	    2dup 1- + c@ '9' 1+ '0' within
	    IF  type ." 1;"  ELSE  type ." ;"  THEN
	ELSE  type  THEN
    r> r> emit emit ;] $tmp ;

: keycode>keys ( keycode -- addr u )
    dup AKEYCODE_A AKEYCODE_Z 1+ within IF
	meta-key# @ AMETA_CTRL_ON and IF
	    AKEYCODE_A - ctrl A + ctrl-key# c!
	    ctrl-key# 1 EXIT  THEN
    THEN
    case
	AKEYCODE_MENU of  togglekb s" "  endof
	AKEYCODE_BACK of  aback    s" "  endof
	akey>ekey +meta 0
    endcase ;

16 Value looper-to#
2Variable loop-event
0 Value poll-file

variable looperfds pollfd 8 * allot
: +fds ( fileno flag -- )
    looperfds dup @ pollfd * + cell+ fds!+ drop
    1 looperfds +! ;
    
: ?poll-file ( -- )
    poll-file 0= IF  app ke-fd0 l@ "r" fdopen to poll-file  THEN ;
: looper-init ( -- )  looperfds off
    app ke-fd0 l@    POLLIN +fds
    infile-id fileno POLLIN +fds
    epiper @ fileno  POLLIN +fds
    ?poll-file ;

: get-event ( -- )
    loop-event 2 cells poll-file read-file throw drop
    loop-event 2@ akey ;

: poll? ( ms -- flag )
    poll-file key?-file IF  get-event drop true  EXIT  THEN
    looperfds dup cell+ swap @
    rot poll 0>
    IF	looperfds cell+ revents w@ POLLIN and dup >r
	IF  get-event  THEN
	looperfds cell+ pollfd 2* + revents w@ POLLIN and
	IF  ?events  THEN
	r>
    ELSE  false
    THEN ;

: >looper  looper-init
    BEGIN  0 poll? 0=  UNTIL  looper-to# poll? drop ;
: ?looper  BEGIN  >looper  app window @ UNTIL ;
	    
\ : >looper  BEGIN  0 poll_looper 0<  UNTIL looper-to# poll_looper drop ;
\ : ?looper  BEGIN >looper app window @ UNTIL ;

:noname  0 poll? drop  defers key? ; IS key?
Defer screen-ops ' noop IS screen-ops

true Value firstkey
:noname
    firstkey IF  showkb false to firstkey  THEN
    need-show on  BEGIN  >looper key? screen-ops  UNTIL
    defers key-ior dup #cr = key? and IF  key-ior ?dup-IF inskey THEN THEN ;
IS key-ior

Defer config-changed :noname [: ." App config changed" cr ;] $err ; IS config-changed
Defer window-init    :noname [: ." app window " app window @ hex. cr ;] $err ; IS window-init

Variable rendering  -2 rendering ! \ -2: on, -1: pause, 0: stop

: nostring ( -- ) setstring $off ;
: insstring ( -- )  setstring $@ inskeys nostring ;

: android-characters ( string -- )  jstring>sstring
    nostring inskeys jfree ;
: android-commit     ( string/0 -- ) ?dup-0=-IF  insstring  ELSE
	jstring>sstring inskeys jfree setstring $off  THEN ;
: android-setstring  ( string -- ) jstring>sstring setstring $! jfree
    ctrl L inskey ;
: android-unicode    ( uchar -- )   >xstring inskeys ;
: android-keycode    ( keycode -- ) keycode>keys inskeys ;

: xcs ( addr u -- n )
    \G number of xchars in a string
    0 -rot bounds ?DO  1+ I I' over - x-size +LOOP ;

: android-edit-update ( span addr pos1 -- span addr pos1 )
    2dup xcs swap >r >r
    2dup swap make-jstring r> clazz .setEditLine r> ;
' android-edit-update is edit-update

: ins-esc# ( n char -- ) swap 0 max 1+
    [: .\" \e[;" 0 .r emit ;] $tmp inskeys ;
: android-setcur ( n -- ) 'H' ins-esc# ;
: android-setsel ( n -- ) 'S' ins-esc# ;

JValue key-event
JValue touch-event
JValue location
JValue sensor
JValue cmanager

: network-info ( -- o/0 )
    cmanager 0= IF  clazz .connectivityManager to cmanager  THEN
    cmanager .getActiveNetworkInfo ;

: .network-info ( o -- ) >o toString xref> .jstring ;

: .network ( -- )  network-info
    ?dup-IF  .network-info  ELSE  ." no active network"  THEN cr ;

: android-key ( event -- )
    dup to key-event >o
    ke_getMetaState meta-key# !
    getAction dup 2 = IF  drop
	getKeyCode dup 0= IF
	    drop getCharacters android-characters
	ELSE
	    android-keycode
	THEN
    ELSE
	0= IF  getUnicodeChar dup 0>
	    IF    android-unicode
	    ELSE  drop  getKeyCode android-keycode
	    THEN
	THEN
    THEN o> ;

Variable new-touch

: touch>event ( event -- )  new-touch on
    dup to touch-event >o
    me-getAction *input action !
    getFlags *input flags !
    getMetaState *input metastate !
    getEdgeFlags *input edgeflags !
    0 getPressure f>s *input pressure !
    0 getSize f>s *input size !
    getEventTime getDownTime d- *input downtime 2!
    getPointerCount dup *input tcount !
    *input x0 swap
    0 ?DO
	I getY f>s
	I getX f>s
	rot dup >r 2! r> 2 cells +
    LOOP  drop
    o> ;

Defer android-location ( location -- )
:noname to location ; IS android-location
Defer android-sensor ( sensor -- )
:noname to sensor ; IS android-sensor

\ stubs, "is recurse" assigns to last defined word

Defer android-surface-changed ' ]gref is android-surface-changed
Defer android-surface-redraw ' ]gref is recurse
Defer android-video-size ' ]gref is recurse
Defer android-touch ' touch>event is recurse

: android-surface-created ( surface -- )
    app window @ 0= IF
	>o  env o ANativeWindow_fromSurface app window !  gref>
	window-init
    ELSE  ]gref  THEN ;
: android-surface-destroyed ( surface -- )
    >o  app window off  gref> ;
: android-global-layout ( 0 -- ) drop config-changed ;
: android-log# ( n -- ) ." log: " . cr ;
: android-log$ ( string -- )  jstring>sstring ." log: " type cr jfree ;
Defer android-w! ( n -- ) ' drop is recurse
Defer android-h! ( n -- ) ' drop is recurse
Defer clipboard! ( 0 -- ) ' drop is recurse
: android-config! ( n -- ) to screen-orientation config-changed ;

Defer android-active
:noname ( flag -- )
    \ >stderr ." active: " dup . cr
    dup rendering !  IF
	16 to looper-to#
	need-show on need-sync on screen-ops
    ELSE  16000 to looper-to#  THEN ; is android-active

Defer android-alarm ( 0 -- ) ' drop is recurse
Defer android-network ( metered -- )
( :noname drop .network cr ; ) ' drop is android-network

Create aevents
' android-key ,
' android-touch ,
' android-location ,
' android-sensor ,
' android-surface-created ,
' android-surface-changed ,
' android-surface-redraw ,
' android-surface-destroyed ,
' android-global-layout ,
' android-video-size ,
' android-log# ,
' android-log$ ,
' android-commit ,
' android-setstring ,
' android-w! ,
' android-h! ,
' clipboard! , \ primary clipboard changed
' android-config! ,
' android-active ,
' android-setcur ,
' android-setsel ,
' android-alarm ,
' android-network ,
here aevents - cell/
' drop ,
Constant max-event#

:noname ( event type -- )
    max-event# umin cells aevents + perform ; is akey

previous previous set-current
