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
    drop 0 XGenericEvent-type sizeof XGenericEvent-type var e.type
    drop 0 XGenericEvent-serial sizeof XGenericEvent-serial var e.serial
    drop 0 XGenericEvent-send_event sizeof XGenericEvent-send_event var e.send_event
    drop 0 XGenericEvent-display sizeof XGenericEvent-display var e.display
    drop 0 XAnyEvent-window sizeof XAnyEvent-window var e.window
    drop 0 XExposeEvent-width sizeof XExposeEvent-width var e.r-width
    drop 0 XExposeEvent-height sizeof XExposeEvent-height var e.r-height
    drop 0 XMotionEvent-time sizeof XMotionEvent-time var e.time
    drop 0 XCreateWindowEvent-width sizeof XCreateWindowEvent-width var e.c-width
    drop 0 XCreateWindowEvent-height sizeof XCreateWindowEvent-height var e.c-height
    drop 0 XButtonEvent-x sizeof XButtonEvent-x var e.x
    drop 0 XButtonEvent-y sizeof XButtonEvent-y var e.y
    drop 0 XKeyEvent-state sizeof XKeyEvent-state var e.state
    drop 0 XKeyEvent-keycode sizeof XKeyEvent-keycode var e.code \ key and button
    drop 0 XEvent var event
    $100 var look_chars
    4 var look_key
    4 var comp_stat
    method DoNull \ doesn't exist
    method DoOne  \ doesn't exit, either
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

Create x-key>ekey \ very minimal set for a start
$FF08 , "\b" $,
$FF09 , "\t" $,
$FF0D , "\r" $,
$FF50 , "\e[H" $,
$FF51 , "\e[D" $,
$FF52 , "\e[A" $,
$FF53 , "\e[C" $,
$FF54 , "\e[B" $,
$FF55 , "\e[5~" $,
$FF56 , "\e[6~" $,
$FFFF , "\b" $, \ is not delete, is backspace!
0 , 0 c,
DOES> ( x-key -- addr u )
  swap >r
  BEGIN  dup cell+ swap @ dup r@ <> and WHILE  count +  REPEAT
  count rdrop ;

: getwh ( -- )
    0 0 dpy-w @ dpy-h @ glViewport ;
: screen-orientation ( -- 0/1 )
    dpy-w @ dpy-h @ > negate ;

:noname ; handler-class to DoNull \ doesn't exist
:noname ; handler-class to DoOne  \ doesn't exit, either
:noname  ic event look_chars $FF look_key comp_stat  XUtf8LookupString
    ?dup-IF  look_chars swap
    ELSE   look_key l@ x-key>ekey  THEN
    2dup "\e" str= IF  2drop -1 level# +!  ELSE  inskeys  THEN
; handler-class to DoKeyPress
:noname ; handler-class to DoKeyRelease
:noname  0 *input action ! 1 *input pressure !
    *input eventtime 2@ *input eventtime' 2!
    e.time @ s>d *input eventtime 2!  0. *input downtime 2!
    e.x l@ e.y l@ *input y0 ! *input x0 ! ; handler-class to DoButtonPress
:noname  1 *input action ! 0 *input pressure !
    *input eventtime 2@ *input eventtime' 2!
    e.time l@ s>d *input eventtime 2@ d- *input downtime 2!
    e.time @ s>d *input eventtime 2!
    e.x l@ *input x0 ! e.y l@ *input y0 ! ; handler-class to DoButtonRelease
:noname
    *input pressure @ IF
	2 *input action !
	e.time @ s>d *input eventtime 2@ d- *input downtime 2!
	e.x l@ e.y l@ *input y0 ! *input x0 !
    THEN ; handler-class to DoMotionNotify
:noname ; handler-class to DoEnterNotify
:noname ; handler-class to DoLeaveNotify
:noname e.window @ focus-ic ; handler-class to DoFocusIn
:noname ; handler-class to DoFocusOut
:noname ; handler-class to DoKeymapNotify
:noname exposed on ; handler-class to DoExpose
:noname exposed on ; handler-class to DoGraphicsExpose
:noname ; handler-class to DoNoExpose
:noname ; handler-class to DoVisibilityNotify
:noname ; handler-class to DoCreateNotify
:noname ; handler-class to DoDestroyNotify
:noname ; handler-class to DoUnmapNotify
:noname ; handler-class to DoMapNotify
:noname ; handler-class to DoMapRequest
:noname ; handler-class to DoReparentNotify
:noname  e.c-width l@ dpy-w ! e.c-height l@ dpy-h !
    ctx IF  config-changed  ELSE  getwh  THEN ; handler-class to DoConfigureNotify
:noname ; handler-class to DoConfigureRequest
:noname ; handler-class to DoGravityNotify
:noname  e.r-width l@ dpy-w ! e.r-height l@ dpy-h ! config-changed ; handler-class to DoResizeRequest
:noname ; handler-class to DoCirculateNotify
:noname ; handler-class to DoCirculateRequest
:noname ; handler-class to DoPropertyNotify
:noname ; handler-class to DoSelectionClear
:noname ; handler-class to DoSelectionRequest
:noname ; handler-class to DoSelectionNotify
:noname ; handler-class to DoColormapNotify
:noname ; handler-class to DoClientMessage
:noname ; handler-class to DoMappingNotify
:noname ; handler-class to DoGenericEvent

: handle-event ( -- ) e.type l@ cells o#+ [ -1 cells , ] @ + perform ;
: get-events ( -- )  event-handler @ >o
    BEGIN  dpy XPending  WHILE  dpy event XNextEvent drop
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

: #looper ( delay -- )
    >poll-events >r
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
