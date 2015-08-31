\ wrapper to load Swig-generated libraries

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

    include androidlib.fs
    
end-c-library

s" APP_STATE" getenv s>number drop Value app

get-current also forth definitions

require unix/socket.fs
require unix/pthread.fs
require unix/jni-helper.fs

set-current previous

Variable need-sync
Variable need-show

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
	AKEYCODE_BACK of  aback    s" "   endof
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
    poll-file 0= IF  app ke-fd0 l@ "r\0" drop fdopen to poll-file  THEN ;
: looper-init ( -- )  looperfds off
    app ke-fd0 l@   POLLIN +fds
    stdin fileno    POLLIN +fds
    epiper @ fileno POLLIN +fds
    ?poll-file ;

: get-event ( -- )
    loop-event 2 cells poll-file read-file throw drop
    loop-event 2@ akey ;

: poll? ( ms -- flag )  looperfds dup cell+ swap @
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
:noname
    need-show on  BEGIN  >looper key? screen-ops  UNTIL
    defers key dup #cr = key? and IF  key ?dup-IF unkey THEN THEN ;
IS key

Defer config-changed :noname [: ." App config changed" cr ;] $err ; IS config-changed
Defer window-init    :noname [: ." app window " app window @ hex. cr ;] $err ; IS window-init

Variable rendering  rendering on

Variable setstring

: insstring ( -- )  setstring $@ inskeys setstring $off ;

: android-characters ( string -- )  jstring>sstring
    ( insstring ) setstring $off  inskeys jfree ;
: android-commit     ( string/0 -- )   ?dup-0=-IF  insstring  ELSE
	jstring>sstring inskeys jfree setstring $off  THEN ;
: android-setstring  ( string -- )  jstring>sstring setstring $! jfree
    ctrl l unkey ;
: android-unicode    ( uchar -- )   insstring  >xstring inskeys ;
: android-keycode    ( keycode -- ) insstring  keycode>keys inskeys ;

JValue key-event
JValue touch-event
JValue location
JValue sensor

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

: android-active ( flag -- )
    \ >stderr ." active: " dup . cr
    dup rendering !  IF  need-show on screen-ops  THEN ;

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
here aevents - cell/
' drop ,
Constant max-event#

:noname ( event type -- )
    max-event# umin cells aevents + perform ; is akey

previous previous set-current
