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
2 Constant #clearme

also x11

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
; x11-handler to DoKeyPress
' noop x11-handler to DoKeyRelease
: samepos? ( x y -- flag )
    lastpos 2@ >r swap r> - >r - dup * r> dup * + samepos < ;
: send-clicks ( -- )
    lastpos 2@ swap s>f s>f buttonmask le-ul@ clicks top-act ?dup-IF
	.clicked
    ELSE  2drop fdrop fdrop  THEN
    flags #pending -bit ;
:noname ( -- )
    event-handler @ >o
    flags #pending bit@ IF
	XTime@ lasttime @ - twoclicks >= IF
	    send-clicks  flags #pending -bit
	THEN
    THEN
    flags #clearme bit@ IF
	XTime@ lasttime @ - twoclicks >= IF
	    0 to clicks
	THEN
    THEN
    o> ; is ?looper-timeouts
:noname ( -- )
    buttonmask e.button +bit  e.time lasttime !  e.x e.y lastpos 2!
    flags #lastdown bit@ 0= negate +to clicks
    flags #lastdown +bit  flags #pending +bit
; x11-handler to DoButtonPress
:noname ( -- )
    e.x e.y lastpos 2!  e.time lasttime !
    flags #lastdown bit@ IF  clicks 0= negate 1+ +to clicks  send-clicks  THEN  
    buttonmask e.button -bit
    flags #lastdown -bit  flags #pending -bit  flags #clearme +bit
; x11-handler to DoButtonRelease
:noname
    flags #pending bit@  e.x e.y samepos? 0= and IF
	send-clicks  flags #lastdown bit@ negate to clicks
    THEN  e.x e.y lastpos 2!
; x11-handler to DoMotionNotify
:noname ; x11-handler to DoEnterNotify
:noname ; x11-handler to DoLeaveNotify
:noname e.window focus-ic ; x11-handler to DoFocusIn
' noop x11-handler to DoFocusOut
' noop x11-handler to DoKeymapNotify
:noname top-widget .widget-draw ; x11-handler to DoExpose
:noname top-widget .widget-draw ; x11-handler to DoGraphicsExpose
' noop x11-handler to DoNoExpose
' noop x11-handler to DoVisibilityNotify
:noname e.w-width e.w-height resize-widgets ; x11-handler to DoCreateNotify
' noop x11-handler to DoDestroyNotify
' noop x11-handler to DoUnmapNotify
' noop x11-handler to DoMapNotify
' noop x11-handler to DoMapRequest
' noop x11-handler to DoReparentNotify
:noname e.c-width  e.c-height resize-widgets ; x11-handler to DoConfigureNotify
' noop x11-handler to DoConfigureRequest
' noop x11-handler to DoGravityNotify
:noname e.r-width  e.r-height resize-widgets ; x11-handler to DoResizeRequest
' noop x11-handler to DoCirculateNotify
' noop x11-handler to DoCirculateRequest
' noop x11-handler to DoPropertyNotify
' noop x11-handler to DoSelectionClear
' noop x11-handler to DoSelectionRequest
' noop x11-handler to DoSelectionNotify
' noop x11-handler to DoColormapNotify
' noop x11-handler to DoClientMessage
' noop x11-handler to DoMappingNotify
' noop x11-handler to DoGenericEvent

x11-handler new event-handler !

previous
