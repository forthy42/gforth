\ Linux bindings for GLES

\ Copyright (C) 2014,2016 Free Software Foundation, Inc.

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

require unix/x.fs
require mini-oof2.fs
require struct-val.fs

also x11

0 Value dpy
0 Value screen-struct
0 Value screen
0 Value win
0 Value ic
0 Value im
0 Value xim
0 Value fontset

Variable need-sync
Variable need-show
Variable need-config
Variable kbflag

XIMPreeditPosition XIMPreeditArea or
XIMPreeditNothing or XIMPreeditNone or Constant XIMPreedit

: best-im ( -- im )
    XSupportsLocale IF
	"XMODIFIERS" getenv dup IF
	    XSetLocaleModifiers 0= IF
		." Warning: Cannot set locale modifiers to '"
		"XMODIFIERS" getenv type  ." '" cr THEN
	ELSE  2drop  THEN
    THEN
    dpy 0 0 0 XOpenIM dup to xim
    ?dup-0=-IF  "@im=local" XSetLocaleModifiers drop
	dpy 0 0 0 XOpenIM dup to xim
	." Warning: can't open XMODIFIERS' IM, set to '@im=local' instead" cr
    THEN
    IF  0 { w^ styles } xim "queryInputStyle\0" drop styles XGetIMValues
	0<> ?EXIT \ didn't succeed
	0  styles @ cell+ @ styles @ w@ cells bounds ?DO
	    I @ dup XIMPreedit and 0<> swap XIMStatusNothing and 0<> and
	    IF  drop I @  LEAVE  THEN
	cell +LOOP  dup 0= IF ." No style found" cr  THEN
	styles @ XFree drop
    ELSE  0  THEN ;

: set-fontset ( -- )
    dpy "-*-FreeSans-*-r-*-*-*-120-*-*-*-*-*-*,-misc-fixed-*-r-*-*-*-130-*-*-*-*-*-*" 0 0 0 { w^ misslist w^ miss# w^ defstring }
    misslist miss# defstring XCreateFontSet to fontset
    misslist @ XFreeStringList ;

: get-display ( -- w h )
    "DISPLAY" getenv XOpenDisplay to dpy
    dpy XDefaultScreenOfDisplay to screen-struct
    dpy XDefaultScreen to screen
    best-im to im  set-fontset
    dpy #38 0 XKeycodeToKeysym drop
    screen-struct screen-width l@
    screen-struct screen-height l@ ;

4 buffer: spot \ spot location, two shorts

: get-ic ( win -- ) xim 0= IF  drop  EXIT  THEN
    ic IF  >r ic "focusWindow\0" drop r> XSetICValues drop
	EXIT  THEN
    0 "fontSet\0" drop fontset "spotLocation\0" drop spot
    XVaCreateNestedList_2 { win list }
    xim "inputStyle\0" drop im "preeditAttributes\0" drop list
    "focusWindow\0" drop win XCreateIC_3 dup to ic
    list XFree drop
    ?dup-IF  XSetICFocus  THEN ;

: focus-ic ( win -- )  ic IF
	>r ic "focusWindow\0" drop r@ "clientWindow\0" drop r>
	XSetICValues_2 drop  ic XSetICFocus
    THEN ;

0
KeyPressMask or
KeyReleaseMask or
ButtonPressMask or
ButtonReleaseMask or
EnterWindowMask or
LeaveWindowMask or
PointerMotionMask or
ButtonMotionMask or
\ KeymapStateMask or
ExposureMask or
\ VisibilityChangeMask or
StructureNotifyMask or
\ ResizeRedirectMask or
SubstructureNotifyMask or
SubstructureRedirectMask or
FocusChangeMask or
PropertyChangeMask or
\ ColormapChangeMask or
\ OwnerGrabButtonMask or
Constant default-events

[IFUNDEF] linux  : linux ;  [THEN]

Defer window-init    ' noop is window-init
Defer config-changed
Defer screen-ops     ' noop IS screen-ops

#16 Value config-change#
:noname ( -- ) config-change# need-config ! ; is config-changed

: term-cr defers cr ;

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

app_input_state buffer: *input

Variable rendering -2 rendering !

Variable level#

\ handle X11 events

object class
    drop 0 XGenericEvent-type        lvalue: e.type
    drop 0 XGenericEvent-serial      value: e.serial
    drop 0 XGenericEvent-send_event  lvalue: e.send_event
    drop 0 XGenericEvent-display     value: e.display
    drop 0 XAnyEvent-window          value: e.window
    drop 0 XResizeRequestEvent-width     lvalue: e.r-width
    drop 0 XResizeRequestEvent-height    lvalue: e.r-height
    drop 0 XConfigureRequestEvent-width  lvalue: e.c-width
    drop 0 XConfigureRequestEvent-height lvalue: e.c-height
    drop 0 XCreateWindowEvent-width      lvalue: e.w-width
    drop 0 XCreateWindowEvent-height     lvalue: e.w-height
    drop 0 XExposeEvent-width            lvalue: e.e-width
    drop 0 XExposeEvent-height           lvalue: e.e-height
    drop 0 XMotionEvent-time         value: e.time
    drop 0 XButtonEvent-x            lvalue: e.x
    drop 0 XButtonEvent-y            lvalue: e.y
    drop 0 XButtonEvent-button       lvalue: e.button
    drop 0 XKeyEvent-state           lvalue: e.state
    drop 0 XKeyEvent-keycode         lvalue: e.code \ key and button
    drop 0 XEvent var event
    $100 var look_chars
    4 var look_key
    4 var comp_stat
    method DoNull \ doesn't exist
    method DoOne  \ doesn't exist, either
    method DoKeyPress
    method DoKeyRelease
    method DoButtonPress
    method DoButtonRelease
    method DoMotionNotify
    method DoEnterNotify
    method DoLeaveNotify
    method DoFocusIn
    method DoFocusOut
    method DoKeymapNotify
    method DoExpose
    method DoGraphicsExpose
    method DoNoExpose
    method DoVisibilityNotify
    method DoCreateNotify
    method DoDestroyNotify
    method DoUnmapNotify
    method DoMapNotify
    method DoMapRequest
    method DoReparentNotify
    method DoConfigureNotify
    method DoConfigureRequest
    method DoGravityNotify
    method DoResizeRequest
    method DoCirculateNotify
    method DoCirculateRequest
    method DoPropertyNotify
    method DoSelectionClear
    method DoSelectionRequest
    method DoSelectionNotify
    method DoColormapNotify
    method DoClientMessage
    method DoMappingNotify
    method DoGenericEvent
end-class handler-class

User event-handler  handler-class new event-handler !

Variable exposed

: $, ( addr u -- )  here over 1+ allot place ;

also x11

: xmeta@ ( state -- meta ) >r
    r@ ShiftMask   and 0<> 1 and
    r@ Mod1Mask    and 0<> 2 and or
    r> ControlMask and 0<> 4 and or ;

: +meta ( addr u -- addr' u' ) \ insert meta information
    >r over c@ #esc <> IF  rdrop  EXIT  THEN
    r> dup 0= IF  drop  EXIT  THEN  '1' + \ no meta, don't insert
    [: >r 1- 2dup + c@ >r
	over 1+ c@ '[' = IF
	    2dup 1- + c@ '9' 1+ '0' within
	    IF  type ." 1;"  ELSE  type ." ;"  THEN
	ELSE  type  THEN
    r> r> emit emit ;] $tmp ;

Create x-key>ekey \ very minimal set for a start
XK_BackSpace , "\x7F" $,
XK_Tab       , "\t" $,
XK_Linefeed  , "\n" $,
XK_Return    , "\r" $,
XK_Home      , "\e[H" $,
XK_Left      , "\e[D" $,
XK_Up        , "\e[A" $,
XK_Right     , "\e[C" $,
XK_Down      , "\e[B" $,
XK_Insert    , "\e[2~" $,
XK_Delete    , "\e[3~" $,
XK_Prior     , "\e[5~" $,
XK_Next      , "\e[6~" $,
XK_F1        , "\eOP" $,
XK_F2        , "\eOQ" $,
XK_F3        , "\eOR" $,
XK_F4        , "\eOS" $,
XK_F5        , "\e[15~" $,
XK_F6        , "\e[17~" $,
XK_F7        , "\e[18~" $,
XK_F8        , "\e[19~" $,
XK_F9        , "\e[20~" $,
XK_F10       , "\e[21~" $,
XK_F12       , "\e[22~" $,
XK_F12       , "\e[23~" $,
0 , 0 c,
DOES> ( x-key -- addr u )
  swap >r
  BEGIN  dup cell+ swap @ dup r@ <> and WHILE  count +  REPEAT
  count rdrop e.state xmeta@ +meta ;

previous

: getwh ( -- )
    0 0 dpy-w @ dpy-h @ glViewport ;
: screen-orientation ( -- 0/1 )
    dpy-w @ dpy-h @ > negate ;

' noop handler-class to DoNull \ doesn't exist
' noop handler-class to DoOne  \ doesn't exit, either
:noname  ic event look_chars $FF look_key comp_stat  XUtf8LookupString
    dup 1 = IF  look_chars c@ dup $7F = swap 8 = or +  THEN \ we want the other delete
    ?dup-IF  look_chars swap
    ELSE   look_key l@ x-key>ekey  THEN
    2dup "\e" str= IF  2drop -1 level# +!  ELSE  inskeys  THEN
; handler-class to DoKeyPress
' noop handler-class to DoKeyRelease
:noname  0 *input action ! 1 *input pressure !
    *input eventtime 2@ *input eventtime' 2!
    e.time s>d *input eventtime 2!  0. *input downtime 2!
    e.x e.y *input y0 ! *input x0 ! ; handler-class to DoButtonPress
:noname  1 *input action ! 0 *input pressure !
    *input eventtime 2@ *input eventtime' 2!
    e.time s>d 2dup *input eventtime 2@ d- *input downtime 2!
    *input eventtime 2!
    e.x *input x0 ! e.y *input y0 ! ; handler-class to DoButtonRelease
:noname
    *input pressure @ IF
	2 *input action !
	e.time s>d *input eventtime 2@ d- *input downtime 2!
	e.x e.y *input y0 ! *input x0 !
    THEN ; handler-class to DoMotionNotify
' noop handler-class to DoEnterNotify
' noop handler-class to DoLeaveNotify
:noname e.window focus-ic ; handler-class to DoFocusIn
' noop handler-class to DoFocusOut
' noop handler-class to DoKeymapNotify
:noname exposed on ; handler-class to DoExpose
:noname exposed on ; handler-class to DoGraphicsExpose
' noop handler-class to DoNoExpose
' noop handler-class to DoVisibilityNotify
' noop handler-class to DoCreateNotify
' noop handler-class to DoDestroyNotify
' noop handler-class to DoUnmapNotify
' noop handler-class to DoMapNotify
' noop handler-class to DoMapRequest
' noop handler-class to DoReparentNotify
:noname  e.c-width dpy-w ! e.c-height dpy-h !
    ctx IF  config-changed  ELSE  getwh  THEN ; handler-class to DoConfigureNotify
' noop handler-class to DoConfigureRequest
' noop handler-class to DoGravityNotify
:noname  e.r-width dpy-w ! e.r-height dpy-h ! config-changed ; handler-class to DoResizeRequest
' noop handler-class to DoCirculateNotify
' noop handler-class to DoCirculateRequest
' noop handler-class to DoPropertyNotify
' noop handler-class to DoSelectionClear
' noop handler-class to DoSelectionRequest
' noop handler-class to DoSelectionNotify
' noop handler-class to DoColormapNotify
' noop handler-class to DoClientMessage
' noop handler-class to DoMappingNotify
' noop handler-class to DoGenericEvent

0 Value timeoffset
: XTime ( -- ntime ) utime #1000 um/mod nip ;
: XTime@ ( -- ntime )
    XTime timeoffset + ;

: handle-event ( -- ) e.type cells o#+ [ -1 cells , ] @ + perform ;
: get-events ( -- )  event-handler @ >o
    BEGIN  dpy XPending  WHILE  dpy event XNextEvent drop
	    e.time XTime - to timeoffset
	    event 0 XFilterEvent 0= IF  handle-event  THEN
    REPEAT o> ;

\ polling of FDs

get-current also forth definitions

require unix/socket.fs
require unix/pthread.fs

previous set-current

User xptimeout  cell uallot drop
#16000000 Value xpoll-timeout# \ 16ms, don't sleep too long
xpoll-timeout# 0 xptimeout 2!
3 Value xpollfd#
User xpollfds
xpollfds pollfd xpollfd# * dup cell- uallot drop erase

: >poll-events ( delay -- n )
    0 xptimeout 2!
    epiper @ fileno POLLIN  xpollfds fds!+ >r
    dpy IF  dpy XConnectionNumber POLLIN  r> fds!+ >r  THEN
    infile-id fileno POLLIN  r> fds!+ >r
    r> xpollfds - pollfd / ;

: xpoll ( -- flag )
    [IFDEF] ppoll
	xptimeout 0 ppoll 0>
    [ELSE]
	xptimeout 2@ #1000 * swap #1000000 / + poll 0>
    [THEN] ;

Defer ?looper-timeouts ' noop is ?looper-timeouts

: #looper ( delay -- )
    ?looper-timeouts >poll-events >r
    dpy IF  dpy XPending IF  get-events ?events  rdrop EXIT  THEN  THEN
    xpollfds r> xpoll
    IF
	xpollfds          revents w@ POLLIN and IF  ?events  THEN
	dpy IF
	    xpollfds pollfd + revents w@ POLLIN and IF  get-events  THEN
	THEN
    THEN ;

: >looper ( -- )  xpoll-timeout# #looper ;
: >exposed  ( -- )  exposed off  BEGIN  >looper exposed @  UNTIL ;
: ?looper ( -- )  ;

: simple-win ( events string len w h -- )
    2>r dpy dup XDefaultRootWindow
    0 0 2r> 1 0 0 XCreateSimpleWindow  to win
    dpy win 2swap XStoreName drop
    dpy win rot XSelectInput drop
    dpy win XMapWindow drop
    win get-ic
    dpy 0 XSync drop >exposed ;

: x-key ( -- key )
    need-show on  key? IF  defers key-ior  EXIT  THEN
    BEGIN  >looper  key? screen-ops UNTIL  defers key-ior ;

0 warnings !@
: bye ( -- )
    ic ?dup-IF  XDestroyIC  THEN  0 to ic
    xim ?dup-IF  XCloseIM drop  THEN  0 to xim
    bye ;
warnings !
' x-key IS key-ior
