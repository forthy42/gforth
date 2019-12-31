\ Wayland window for GLES

\ Author: Bernd Paysan
\ Copyright (C) 2017,2019 Free Software Foundation, Inc.

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

require unix/opengles.fs
require unix/waylandlib.fs
require mini-oof2.fs
require struct-val.fs

[IFUNDEF] linux  : linux ;  [THEN]

also wayland

debug: wayland(
\ +db wayland( \ )

0 Value dpy        \ wayland display
0 Value compositor \ wayland compositor
0 Value wl-output
0 Value wl-shell   \ wayland shell
0 Value wl-egl-dpy \ egl display
0 Value wl-pointer
0 Value wl-keyboard
0 Value wl-touch
0 Value registry
0 Value win
0 Value cursor-theme
0 Value cursor
0 Value cursor-surface
0 Value wl-surface
0 Value sh-surface
0 Value wl-seat
0 Value wl-shm

\ set a cursor

: set-cursor { serial -- }
    cursor wl_cursor-images @ @ { image }
    wl-pointer serial cursor-surface
    image wl_cursor_image-hotspot_x sl@
    image wl_cursor_image-hotspot_y sl@
    wl_pointer_set_cursor
    cursor-surface image wl_cursor_image_get_buffer
    0 0 wl_surface_attach
    cursor-surface 0 0
    image wl_cursor_image-width sl@
    image wl_cursor_image-height sl@
    wl_surface_damage
    cursor-surface wl_surface_commit ;

\ shell surface listener

: sh-surface-ping ( data surface serial -- )
    wl_shell_surface_pong drop ;
: sh-surface-config { data surface edges w h -- }
    win w h 0 0 wl_egl_window_resize ;
: sh-surface-popup-done { data surface -- } ;

' sh-surface-popup-done wl_shell_surface_listener-popup_done:
' sh-surface-config wl_shell_surface_listener-configure:
' sh-surface-ping wl_shell_surface_listener-ping:
Create wl-sh-surface-listener , , ,

\ time handling

0 Value timeoffset
: XTime0 ( -- mtime ) utime #1000 ud/mod drop nip ;
: XTime ( -- mtime ) XTime0 timeoffset + ;
: XTime! ( mtime -- ) XTime0 - to timeoffset ;

\ geometry output listener

2Variable wl-metrics
2Variable dpy-wh
1 Value wl-scale
0 Value screen-orientation

: wl-out-geometry { data out x y pw ph subp make model transform -- }
    pw ph wl-metrics 2! transform to screen-orientation ;
: wl-out-mode { data out flags w h r -- }
    w h dpy-wh 2! ;
: wl-out-done { data out -- } ;
: wl-out-scale { data out scale -- }
    scale to wl-scale ;

' wl-out-scale wl_output_listener-scale:
' wl-out-done wl_output_listener-done:
' wl-out-mode wl_output_listener-mode:
' wl-out-geometry wl_output_listener-geometry:
Create wl-output-listener , , , ,

\ As events come in callbacks, push them to an event queue

: 3drop 2drop drop ;

Defer b-scroll ' 3drop is b-scroll
Defer b-button ' 3drop is b-button
Defer b-motion ' 3drop is b-motion
Defer b-enter  ' 2drop is b-enter
Defer b-leave  ' noop  is b-leave
event: :>scroll ( time axis val -- ) b-scroll ;
event: :>button ( time b mask -- )   b-button ; 
event: :>motion ( time x y -- )      b-motion ; 
event: :>enter  ( x y -- )           b-enter ; 
event: :>leave  ( -- )               b-leave ; 

\ pointer listener

:noname { data p axis disc -- } ; wl_pointer_listener-axis_discrete:
:noname { data p time axis -- }  time XTime!
; wl_pointer_listener-axis_stop:
:noname { data p source -- } ; wl_pointer_listener-axis_source:
:noname { data p -- } ; wl_pointer_listener-frame:
:noname { data p time axis val -- }  time XTime!
    <event time elit, axis elit, val elit, :>scroll [ up@ ]L event>
; wl_pointer_listener-axis:
:noname { data p ser time b mask -- }  time XTime!
    <event time elit, b elit, mask elit, :>button [ up@ ]L event>
; wl_pointer_listener-button:
:noname { data p time x y -- }  time XTime!
    <event time elit, x elit, y elit, :>motion [ up@ ]L event>
; wl_pointer_listener-motion:
:noname { data p s -- }
    <event :>leave [ up@ ]L event>
; wl_pointer_listener-leave:
:noname { data p s x y -- }
    s set-cursor \ on enter, we set the cursor
    <event x elit, y elit, :>enter [ up@ ]L event>
; wl_pointer_listener-enter:
Create wl-pointer-listener  , , , , , , , , ,

\ keyboard listener

Create wl-keyboard-listener

\ seat listener

:noname { data seat name -- } ; wl_seat_listener-name:
:noname { data seat caps -- }
    caps WL_SEAT_CAPABILITY_POINTER and IF
	wl-seat wl_seat_get_pointer to wl-pointer
	wl-pointer wl-pointer-listener 0 wl_pointer_add_listener drop
    THEN
    caps WL_SEAT_CAPABILITY_KEYBOARD and IF
	wl-seat wl_seat_get_keyboard to wl-keyboard
    THEN
    caps WL_SEAT_CAPABILITY_TOUCH and IF
	wl-seat wl_seat_get_touch to wl-touch
    THEN ; wl_seat_listener-capabilities:
Create wl-seat-listener  , ,

\ registry listeners: the interface string is searched in a table

table Constant wl-registry

get-current

wl-registry set-current

: wl_compositor ( registry name -- )
    wl_compositor_interface 1 wl_registry_bind to compositor
    compositor wl_compositor_create_surface to wl-surface
    compositor wl_compositor_create_surface to cursor-surface ;
: wl_shell ( registry name -- )
    wl_shell_interface 1 wl_registry_bind to wl-shell
    wl-shell wl-surface wl_shell_get_shell_surface to sh-surface
    sh-surface wl-sh-surface-listener 0 wl_shell_surface_add_listener drop
    sh-surface wl_shell_surface_set_toplevel ;
: wl_output ( registry name -- )
    wl_output_interface 1 wl_registry_bind to wl-output
    wl-output wl-output-listener 0 wl_output_add_listener drop ;
: wl_seat ( registry name -- )
    wl_seat_interface 1 wl_registry_bind to wl-seat
    wl-seat wl-seat-listener 0 wl_seat_add_listener drop ;
: wl_shm ( registry name -- )
    wl_shm_interface 1 wl_registry_bind to wl-shm
    s" Breeze_Snow" 16 wl-shm wl_cursor_theme_load to cursor-theme
    cursor-theme s" left_ptr" wl_cursor_theme_get_cursor to cursor ;
\ : zwp_text_input_manager_v2 ( registry name -- ) ;

set-current
    
: registry+ { data registry name interface version -- }
    interface cstring>sstring wl-registry find-name-in ?dup-IF
	registry name rot name?int execute
    ELSE
	wayland( interface cstring>sstring type cr )
    THEN ;
: registry- { data registry name -- } ;

' registry- wl_registry_listener-global_remove:
' registry+ wl_registry_listener-global:
Create registry-listener , ,

: get-events ( -- )
    dpy wl_display_roundtrip drop ;

: get-display ( -- w h )
    0 0 wl_display_connect to dpy
    dpy wl_display_get_registry to registry
    registry registry-listener 0 wl_registry_add_listener drop
    get-events  get-events  dpy-wh 2@ ;

: wl-eglwin ( w h -- )
    wl-surface -rot wl_egl_window_create to win ;

also opengl
: getwh ( -- )
    0 0 dpy-w @ dpy-h @ glViewport ;
previous

\ looper

get-current also forth definitions

require unix/socket.fs
require unix/pthread.fs

previous set-current

User xptimeout  cell uallot drop
#16 Value looper-to# \ 16ms, don't sleep too long
looper-to# #1000000 um* xptimeout 2!
3 Value xpollfd#
User xpollfds
xpollfds pollfd xpollfd# * dup cell- uallot drop erase

: >poll-events ( delay -- n )
    0 xptimeout 2!
    epiper @ fileno POLLIN  xpollfds fds!+ >r
    dpy IF  dpy wl_display_get_fd POLLIN  r> fds!+ >r  THEN
    infile-id fileno POLLIN  r> fds!+ >r
    r> xpollfds - pollfd / ;

: xpoll ( -- flag )
    [IFDEF] ppoll
	xptimeout 0 ppoll 0>
    [ELSE]
	xptimeout 2@ #1000 * swap #1000000 / + poll 0>
    [THEN] ;

Defer ?looper-timeouts ' noop is ?looper-timeouts

: #looper ( delay -- ) #1000000 *
    ?looper-timeouts >poll-events >r
    xpollfds r> xpoll
    IF
	xpollfds          revents w@ POLLIN and IF  ?events  THEN
	dpy IF
	    xpollfds pollfd + revents w@ POLLIN and IF  get-events  THEN
	THEN
    ELSE
	dpy IF  get-events  THEN
    THEN ;

: >looper ( -- )  looper-to# #looper ;

\ android similarities

require need-x.fs

Variable level#
Variable rendering -2 rendering !
#16 Value config-change#

: ?looper ;

Defer window-init    ' noop is window-init
Defer config-changed ' noop is config-changed
Defer screen-ops     ' noop IS screen-ops

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

: clipboard! ( addr u -- ) 2drop ; \ stub
: clipboard@ ( -- addr u ) s" " ;

also OpenGL
