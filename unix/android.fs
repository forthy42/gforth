\ wrapper to load Swig-generated libraries

require struct0x.fs

\ public interface, C calls us through these

get-current also forth definitions

Defer ainput
Defer acmd
Defer akey

previous set-current

\ The rest is in the "android" vocabulary

Vocabulary android
get-current also android definitions

c-library android
    \c #include <android/input.h>
    \c #include <android/keycodes.h>
    \c #include <android/native_window.h>
    \c #include <android/native_window_jni.h>
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
    field: ke-fd0
    field: ke-fd1
    field: window \ native window
    end-structure
    
    s" android" add-lib
    
end-c-library

require unix/socket.fs

s" APP_STATE" getenv s>number drop Value app

get-current also forth definitions

require jni-helper.fs

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
AKEYCODE_PAGE_UP c, "\e[5~" $,
AKEYCODE_PAGE_DOWN c, "\e[6~" $,
AKEYCODE_ALT_LEFT c, 0 c,
AKEYCODE_ALT_RIGHT c, 0 c,
AKEYCODE_SHIFT_LEFT c, 0 c,
AKEYCODE_SHIFT_RIGHT c, 0 c,
0 c, 0 c,
DOES> ( akey -- addr u )
  swap >r
  BEGIN  count dup r@ <> and WHILE  count +  REPEAT
  count rdrop ;

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

: keycode>keys ( keycode -- addr u )
    case
	AKEYCODE_MENU of  togglekb s" "  endof
	AKEYCODE_BACK of  aback s" "   endof
	akey>ekey 0
    endcase ;

16 Value looper-to#
2Variable loop-event
0 Value poll-file

variable looperfds pollfd %size 8 * allot
: +fds ( fileno flag -- )
    looperfds dup @ pollfd %size * + cell+
    >r r@ events w!  r> fd l!
    1 looperfds +! ;
    
: ?poll-file ( -- )
    poll-file 0= IF  app ke-fd0 @ "r\0" drop fdopen to poll-file  THEN ;
: looper-init ( -- )  looperfds off
    app ke-fd0 @  POLLIN +fds
    stdin fileno  POLLIN +fds
    ?poll-file ;

: get-event ( -- )
    loop-event 2 cells poll-file read-file throw drop
    loop-event 2@ akey ;

: poll? ( ms -- flag )  looperfds dup cell+ swap @
    rot poll 0>
    IF	looperfds cell+ revents w@ POLLIN = dup >r
	IF  get-event  THEN  r>
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

: enum dup Constant 1+ ;
0
enum APP_CMD_INPUT_CHANGED
enum APP_CMD_INIT_WINDOW
enum APP_CMD_TERM_WINDOW
enum APP_CMD_WINDOW_RESIZED
enum APP_CMD_WINDOW_REDRAW_NEEDED
enum APP_CMD_CONTENT_RECT_CHANGED
enum APP_CMD_GAINED_FOCUS
enum APP_CMD_LOST_FOCUS
enum APP_CMD_CONFIG_CHANGED
enum APP_CMD_LOW_MEMORY
enum APP_CMD_START
enum APP_CMD_RESUME
enum APP_CMD_SAVE_STATE
enum APP_CMD_PAUSE
enum APP_CMD_STOP
enum APP_CMD_DESTROY
drop

Defer config-changed :noname [: ." App config changed" cr ;] $err ; IS config-changed
Defer window-init    :noname [: ." app window " app window @ hex. cr ;] $err ; IS window-init

:noname ( cmd -- )
    case
	APP_CMD_INIT_WINDOW of  window-init  endof
	APP_CMD_CONFIG_CHANGED of config-changed endof
	APP_CMD_SAVE_STATE of [: ." app save" cr ;] $err endof
	APP_CMD_TERM_WINDOW of app window off [: ." app window closed" cr ;] $err endof
	APP_CMD_GAINED_FOCUS of [: ." app window focus" cr ;] $err endof
	APP_CMD_LOST_FOCUS of [: ." app window lost focus" cr ;] $err endof
	APP_CMD_DESTROY of [: ." app window destroyed" cr ;] $err bye endof
	APP_CMD_PAUSE of [: ." app pause" cr ;] $err endof
	APP_CMD_RESUME of [: ." app resume" cr ;] $err endof
	APP_CMD_START of [: ." app start" cr ;] $err endof
	APP_CMD_STOP of [: ." app stop" cr ;] $err endof
	dup [: ." app cmd " . cr ;] $err
    endcase ; is acmd

Variable setstring
: insstring ( -- )  setstring $@ inskeys setstring $off ;

: android-characters ( string -- )  jstring>sstring
    insstring  inskeys jfree ;
: android-commit     ( string/0 -- )   ?dup-0=-IF  insstring  ELSE
	jstring>sstring inskeys jfree setstring $off  THEN ;
: android-setstring  ( string -- )  jstring>sstring setstring $! jfree ;
: android-unicode    ( uchar -- )   insstring  >xstring inskeys ;
: android-keycode    ( keycode -- ) insstring  keycode>keys inskeys ;

JValue key-event
JValue touch-event
JValue location
JValue sensor

: android-key ( event -- ) dup to key-event
    >o getAction dup 2 = IF  drop
	getKeyCode dup 0= IF
	    drop getCharacters android-characters
	ELSE
	    android-keycode
	THEN
    ELSE
	0= IF  getUnicodeChar dup 0>
	    IF    android-unicode
	    ELSE  drop  getKeyCode  android-keycode
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
here aevents - cell/
' drop ,
Constant max-event#

:noname ( event type -- )
    max-event# umin cells aevents + perform ; is akey

previous previous set-current
