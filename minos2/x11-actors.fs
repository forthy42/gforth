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

require bits.fs

handler-class class
    2field: lastpos
    field: lasttime
    field: buttonmask
    field: flags
    value: clicks
end-class x11-handler

0 Constant #pending
1 Constant #lastdown

also x11

#060 Value sameclick  \ every edge within 60ms is merged into one click
#150 Value twoclicks  \ every edge further apart than 150ms into separate clicks
#6 Value samepos      \ position difference square-summed less than is same pos

Create x-key>ekey#
XK_BackSpace , #del ,
XK_Tab       , #tab ,
XK_Linefeed  , #lf  ,
XK_Escape    , #esc ,
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

: top-act ( -- o ) top-widget .act ;
: resize-widgets ( w h -- )
    dpy-h ! dpy-w !
    getwh  config-changed
    top-widget >o !size 0e 1e dh* 1e dw* 1e dh* 0e resize widget-draw o> ;
:noname  ic event look_chars $FF look_key comp_stat  XUtf8LookupString
    dup 1 = IF  look_chars c@ dup $7F = swap bl < or +  THEN \ we want the other delete
    ?dup-IF  look_chars swap top-act ?dup-IF  .ukeyed  ELSE  2drop  THEN
    ELSE   look_key l@ x-key>ekey# ?dup-IF
	    top-act ?dup-IF  .ekeyed  ELSE  #esc = level# +!  THEN  THEN  THEN
; handler-class to DoKeyPress
' noop handler-class to DoKeyRelease
: samepos? ( x y -- flag )
    lastpos 2@ >r swap r> - >r - dup * r> dup * + samepos < ;
: sametime? ( deltatime edge -- flag )
    >r sameclick twoclicks r> select < ;
: send-clicks ( -- )
    lastpos 2@ buttonmask @ clicks top-act ?dup-IF  .clicked
    ELSE  2drop 2drop  THEN ;
: sendclick ( -- )  flags #pending +bit
    e.x e.y lastpos 2! ;
:noname ( -- )
    e.time dup lasttime !@ - true sametime?
    IF  e.x e.y samepos?
	IF  flags #lastdown bit@
	    IF    e.button buttonmask +bit
	    ELSE  send-clicks  flags #lastdown +bit
	    THEN  EXIT  THEN   e.x e.y lastpos 2!  flags #pending -bit
    THEN  flags #pending bit@  IF  1 +to clicks  THEN
    1 sendclick flags #lastdown +bit
; handler-class to DoButtonPress
:noname ( -- )
    e.time dup lasttime !@ - false sametime?
    IF  e.x e.y samepos?
	IF  flags #lastdown bit@ 0=
	    IF    e.button buttonmask -bit
	    ELSE  send-clicks  flags #lastdown -bit
	    THEN  EXIT  THEN   e.x e.y lastpos 2!  flags #pending -bit
    THEN  flags #pending bit@  IF  1 +to clicks  THEN
    1 sendclick flags #lastdown -bit
; handler-class to DoButtonRelease
:noname
    *input pressure @ IF
	2 *input action !
	e.time @ s>d *input eventtime 2@ d- *input downtime 2!
	e.x l@ e.y l@ *input y0 ! *input x0 !
    THEN ; handler-class to DoMotionNotify
' noop handler-class to DoEnterNotify
' noop handler-class to DoLeaveNotify
:noname e.window focus-ic ; handler-class to DoFocusIn
' noop handler-class to DoFocusOut
' noop handler-class to DoKeymapNotify
:noname top-widget .widget-draw ; handler-class to DoExpose
:noname top-widget .widget-draw ; handler-class to DoGraphicsExpose
' noop handler-class to DoNoExpose
' noop handler-class to DoVisibilityNotify
:noname e.w-width e.w-height resize-widgets ; handler-class to DoCreateNotify
' noop handler-class to DoDestroyNotify
' noop handler-class to DoUnmapNotify
' noop handler-class to DoMapNotify
' noop handler-class to DoMapRequest
' noop handler-class to DoReparentNotify
:noname e.c-width  e.c-height resize-widgets ; handler-class to DoConfigureNotify
' noop handler-class to DoConfigureRequest
' noop handler-class to DoGravityNotify
:noname e.r-width  e.r-height resize-widgets ; handler-class to DoResizeRequest
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

handler-class new event-handler !

previous
