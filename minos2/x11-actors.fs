\ MINOS2 actors on X11

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

also x11

Create x-key>ekey#
XK_BackSpace , #del ,
XK_Tab       , #tab ,
XK_Linefeed  , #lf  ,
XK_Return    , k-enter ,
XK_Home      , k-home ,
XK_Left      , k-left ,
XK_Up        , k-up   ,
XK_Right     , k-right ,
XK_Down      , k-down ,
XK_Insert    , k-insert ,
XK_Delete    , k-delete ,
XK_Prior     , k-prior ,
XK_Next      , k-next ,
XK_F1        , k-f1 ,
XK_F2        , k-f2 ,
XK_F3        , k-f3 ,
XK_F4        , k-f4 ,
XK_F5        , k-f5 ,
XK_F6        , k-f6 ,
XK_F7        , k-f7 ,
XK_F8        , k-f8 ,
XK_F9        , k-f9 ,
XK_F10       , k-f10 ,
XK_F12       , k-f11 ,
XK_F12       , k-f12 ,
0            , 0 ,
DOES> ( x-key -- addr u )
  swap >r
  BEGIN  dup cell+ swap @ dup r@ <> and WHILE  cell+  REPEAT
  @ rdrop e.state xmeta@ mask-shift# lshift or ;

:noname  ic event look_chars $FF look_key comp_stat  XUtf8LookupString
    dup 1 = IF  look_chars c@ dup $7F = swap bl < or +  THEN \ we want the other delete
    ?dup-IF  look_chars swap top-act .ukeyed
    ELSE   look_key l@ x-key>ekey# ?dup-IF  top-act .ekeyed  THEN  THEN
; handler-class to DoKeyPress
:noname ; handler-class to DoKeyRelease
:noname  0 *input action ! 1 *input pressure !
    *input eventtime 2@ *input eventtime' 2!
    e.time @ s>d *input eventtime 2!  0. *input downtime 2!
    e.x l@ e.y l@ *input y0 ! *input x0 ! ; handler-class to DoButtonPress
:noname  1 *input action ! 0 *input pressure !
    *input eventtime 2@ *input eventtime' 2!
    e.time @ s>d 2dup *input eventtime 2@ d- *input downtime 2!
    *input eventtime 2!
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

previous
