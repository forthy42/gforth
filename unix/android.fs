\ Android based stuff, including wrapper to androidlib.fs

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

Defer reload-textures ' noop is reload-textures

\ The rest is in the "android" vocabulary

Vocabulary android
get-current also android definitions

Defer akey

also c-lib
:noname open-path-lib drop ; is prefetch-lib
previous

begin-structure app_input_state
    field: action
    field: flags
    field: metastate
    field: edgeflags
    field: pressure
    field: size
    2field: downtime
    2field: eventtime
    2field: eventtime'
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

include unix/androidlib.fs

s" APP_STATE" getenv s>number drop Value app

Defer >looper

get-current also forth definitions

require unix/cpu.fs
require unix/socket.fs
require unix/pthread.fs
require unix/jni-helper.fs
require minos2/need-x.fs

set-current previous

+sync +show +keyboard

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
AKEYCODE_DPAD_UP c, "\e[A" $,
AKEYCODE_DPAD_DOWN c, "\e[B" $,
AKEYCODE_VOLUME_UP c, "\eVU" $,
AKEYCODE_VOLUME_DOWN c, "\eVD" $,
AKEYCODE_VOLUME_MUTE c, "\eVM" $,
AKEYCODE_DPAD_LEFT c, "\e[D" $,
AKEYCODE_DPAD_RIGHT c, "\e[C" $,
AKEYCODE_TAB c, "\t" $,
AKEYCODE_ENTER c, "\r" $,
AKEYCODE_DEL c, "\x7f" $, \ is not delete, is backspace!
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
\ keys for non-letters
AKEYCODE_COMMA  c, "," $,
AKEYCODE_PERIOD c, "." $,
AKEYCODE_SPACE  c, " " $,
AKEYCODE_STAR   c, "*" $,
AKEYCODE_POUND  c, "#" $,
AKEYCODE_GRAVE  c, "`" $,
AKEYCODE_MINUS  c, "-" $,
AKEYCODE_EQUALS c, "=" $,
AKEYCODE_LEFT_BRACKET c, "[" $,
AKEYCODE_RIGHT_BRACKET c, "]" $,
AKEYCODE_BACKSLASH c, "\\" $,
AKEYCODE_SEMICOLON c, ";" $,
AKEYCODE_APOSTROPHE c, "'" $,
AKEYCODE_SLASH  c, "/" $,
AKEYCODE_AT     c, "@" $,
AKEYCODE_PLUS   c, "+" $,
0 c,  0 c,
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
JValue psize
newPoint to psize
JValue dmetrics
newDisplayMetrics to dmetrics
JValue screenrect
newRect to screenrect

: screen-orientation@ ( -- 0..3 )
    clazz >o getWindowManager >o getDefaultDisplay >o
    getRotation ref> ref> o> ;
: screen-metric@ ( -- )
    clazz >o getWindowManager >o getDefaultDisplay >o
    dmetrics getMetrics ref> ref> o> ;
: screen-size@ ( -- w h )
    screen-metric@
    dmetrics >o widthPixels heightPixels o> ;
: screen-xywh@ ( -- x y w h )
    clazz >o getWindow >o getDecorView >o
    screenrect getWindowVisibleDisplayFrame ref> ref> o>
    screenrect >o left top right bottom o> ;

$80 Constant FLAG_KEEP_SCREEN_ON

false value wake-lock \ doesn't work, why?

: hidestatus ( -- ) ['] rhidestatus post-it ;
: showstatus ( -- ) ['] rshowstatus post-it ;
: screen+keep ( -- )  ['] rkeepscreenon post-it ;
: screen-keep ( -- )  ['] rkeepscreenoff post-it ;
: screen+secure ( -- )  ['] rsecurescreenon post-it ;
: screen-secure ( -- )  ['] rsecurescreenoff post-it ;

\ callbacks

: $err ( xt -- )  $tmp stderr write-file throw ;

\ event handling

Create direct-key# 0 c,

: meta@ ( -- char ) \ return meta in vt100 form
    0
    meta-key# @ AMETA_SHIFT_ON and 0<> 1 and  or
    meta-key# @ AMETA_ALT_ON   and 0<> 2 and  or
    meta-key# @ AMETA_CTRL_ON  and 0<> 4 and  or ;

: +meta ( addr u -- addr' u' ) \ insert meta information
    >r over c@ #esc <> IF  rdrop  EXIT  THEN
    r> dup 0= IF  drop  EXIT  THEN  '1' + \ no meta, don't insert
    [: >r 1- 2dup + c@ >r
	over 1+ c@ '[' = IF
	    2dup 1- + c@ '9' 1+ '0' within
	    IF  type ." 1;"  ELSE  type ." ;"  THEN
	ELSE  type  THEN
    r> r> emit emit ;] $tmp ;

: keycode>keys ( keycode -- addr u )
    dup AKEYCODE_A AKEYCODE_Z 1+ within IF
	AKEYCODE_A -  ctrl A
	'A' 'a' meta-key# @ AMETA_SHIFT_ON and select
	meta-key# @ AMETA_CTRL_ON and select
	+ direct-key# c! direct-key# 1 EXIT
    THEN
    dup AKEYCODE_0 AKEYCODE_9 1+ within IF
	AKEYCODE_0 - '0' + direct-key# c! direct-key# 1 EXIT
    THEN
    case
	AKEYCODE_MENU of  togglekb s" "  endof
	AKEYCODE_BACK of  aback    s" "  endof
	akey>ekey meta@ +meta 0
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
    epiper @ fileno  POLLIN +fds
    infile-id ?dup-IF  fileno POLLIN +fds  THEN
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
	looperfds cell+ pollfd + revents w@ POLLIN and
	IF  ?events  THEN
	r>
    ELSE  false
    THEN ;

Defer ?looper-timeouts ' noop is ?looper-timeouts

: #looper  looper-init
    BEGIN  ?looper-timeouts  0 poll? 0=  UNTIL  poll? drop ;
:noname ( -- ) looper-to# #looper ; is >looper
: ?looper  BEGIN  >looper  app window @ UNTIL ;

\ : >looper  BEGIN  0 poll_looper 0<  UNTIL looper-to# poll_looper drop ;
\ : ?looper  BEGIN >looper app window @ UNTIL ;

Defer screen-ops ' noop IS screen-ops

:noname  0 poll? drop
    key-buffer $@len 0<>  infile-id ?dup-IF  key?-file  or  THEN
    screen-ops
; IS key?

: android-key-ior ( -- key / ior )
    ?keyboard IF  showkb -keyboard  THEN
    +show
    BEGIN  >looper key? winch? @ or  UNTIL
    winch? @ IF  EINTR  ELSE
	infile-id IF
	    defers key-ior dup #cr = key? and
	    IF  key-ior ?dup-IF inskey THEN THEN
	ELSE  inskey@  THEN
    THEN ;
' android-key-ior IS key-ior

: android-deadline ( dtime -- )
    up@ [ up@ ]L = IF screen-ops THEN  defers deadline ;
' android-deadline IS deadline

: android-everyline ( -- )
    defers everyline restartkb ;
' android-everyline is everyline

Defer config-changed
Defer window-init    :noname [: ." app window " app window @ hex. cr ;] $err ; IS window-init
screen-ops     ' noop IS screen-ops

:noname ( -- ) +sync +config ; is config-changed

Variable rendering  -2 rendering ! \ -2: on, -1: pause, 0: stop

: nostring ( -- ) setstring$ $off ;
: insstring ( -- )  setstring$ $@ 0 skip inskeys nostring ;

: android-characters ( string -- )  jstring>sstring
    nostring 0 skip inskeys jfree ;
Defer android-commit
:noname     ( string/0 -- ) ?dup-0=-IF  insstring  ELSE
	jstring>sstring 0 skip inskeys jfree setstring$ $off
    THEN ; is android-commit
Defer android-setstring
Defer android-inskey ' inskey is android-inskey
:noname  ( string -- ) jstring>sstring setstring$ $! jfree
    ctrl L android-inskey ; is android-setstring
: android-unicode    ( uchar -- )   >xstring inskeys ;
: android-keycode    ( keycode -- ) keycode>keys inskeys ;

: xcs ( addr u -- n )
    \G number of xchars in a string
    0 -rot bounds ?DO  1+ I I' over - x-size +LOOP ;

: android-edit-update ( span addr pos1 -- span addr pos1 )
    xedit-update
    clazz IF
	2dup xcs swap >r >r
	2dup swap make-jstring r> 0 clazz .setEditLine r>
    THEN ;
' android-edit-update is edit-update

: android-setcur ( +n -- ) setcur# ! ;
: android-setsel ( +n -- ) setsel# ! ctrl S android-inskey ;

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

: key>event ( event -- )
    dup to key-event >o
    ke_getMetaState meta-key# !
    getAction dup 2 = IF  drop
	getKeyCode
	?dup-IF  android-keycode
	ELSE  getCharacters android-characters
	THEN
    ELSE
	0= IF
	    getUnicodeChar
	    ?dup-IF  android-unicode
	    ELSE  getKeyCode android-keycode
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
    *input eventtime 2@ *input eventtime' 2!
    getEventTime 2dup *input eventtime 2!
    getDownTime d- *input downtime 2!
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
Defer android-key   ' key>event is recurse

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
Defer clipboard-changed ( 0 -- ) ' drop is recurse
: android-config! ( n -- ) to screen-orientation config-changed ;

Defer android-active

:noname ( flag -- )
    \ >stderr ." active: " dup . cr
    dup rendering !  IF
	16 to looper-to#
	rendering @ -2 <= IF  reload-textures
	    +show +sync +config +textures screen-ops  THEN
    ELSE  16000 to looper-to#  THEN ; is android-active

Defer android-alarm ( 0 -- ) ' drop is android-alarm
Defer android-network ( metered -- )
( :noname drop .network cr ; ) ' drop is android-network
Defer android-notification ( intent -- )
( :noname drop ." Got intent" cr ; ) ' drop is android-notification
Defer android-context-menu ( id -- )
:noname ( n -- )
    case
	$0102001f of  "\e[S"  inskeys endof \ select all
	$01020020 of  ctrl X  inskey  endof \ cut
	$01020021 of  ctrl C  inskey  endof \ copy
	$01020022 of  ctrl V  inskey  endof \ paste
	$0102002c of  ctrl A  inskey  endof \ home
    endcase ; is android-context-menu
Defer android-permission# ( n -- )
:noname to android-perm# ; is android-permission#
Defer android-permission-result ( jstring -- )
Variable android-permissions[]
:noname ( jstring -- )
    jstring>sstring android-permissions[] $+[]! jfree ;
is android-permission-result

Create aevents
(  0 ) ' android-key ,
(  1 ) ' android-touch ,
(  2 ) ' android-location ,
(  3 ) ' android-sensor ,
(  4 ) ' android-surface-created ,
(  5 ) ' android-surface-changed ,
(  6 ) ' android-surface-redraw ,
(  7 ) ' android-surface-destroyed ,
(  8 ) ' android-global-layout ,
(  9 ) ' android-video-size ,
( 10 ) ' android-log# ,
( 11 ) ' android-log$ ,
( 12 ) ' android-commit ,
( 13 ) ' android-setstring ,
( 14 ) ' android-w! ,
( 15 ) ' android-h! ,
( 16 ) ' clipboard-changed , \ primary clipboard changed
( 17 ) ' android-config! ,
( 18 ) ' android-active ,
( 19 ) ' android-setcur ,
( 20 ) ' android-setsel ,
( 21 ) ' android-alarm ,
( 22 ) ' android-network ,
( 23 ) ' android-notification ,
( 24 ) ' android-context-menu ,
( 25 ) ' android-permission# ,
( 26 ) ' android-permission-result ,
here aevents - cell/
' drop ,
Constant max-event#

:noname ( event type -- )
    max-event# umin cells aevents + perform ; is akey

previous previous set-current
