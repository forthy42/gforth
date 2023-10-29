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
require unix/wayland.fs
require unix/mmap.fs
require unix/xkbcommon.fs
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
0 Value xkb-ctx
0 Value xkb-state
0 Value keymap
0 Value registry
0 Value win
0 Value cursor-theme
0 Value cursor
0 Value cursor-surface
0 Value wl-surface
0 Value sh-surface
0 Value wl-seat
0 Value wl-shm
0 Value text-input-manager
0 Value text-input
0 Value xdg-wm-base
0 Value xdg-surface
0 Value xdg-toplevel
0 Value decoration-manager
0 Value zxdg-decoration

\ set a cursor

: set-cursor { serial -- }
    cursor wl_cursor-images @ @ { image }
    wl-pointer serial cursor-surface
    image wl_cursor_image-hotspot_x l@ l>s
    image wl_cursor_image-hotspot_y l@ l>s
    wl_pointer_set_cursor
    cursor-surface image wl_cursor_image_get_buffer
    0 0 wl_surface_attach
    cursor-surface 0 0
    image wl_cursor_image-width l@ l>s
    image wl_cursor_image-height l@ l>s
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

: wl-out-geometry { data out x y pw ph subp d: make d: model transform -- }
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

up@ Value master-task

: wl-scroll ( time axis val -- )
    [{: time axis val :}h1 time axis val b-scroll ;] master-task send-event ;
: wl-button ( time b mask -- )
    [{: time b mask :}h1 time b mask b-button ;] master-task send-event ;
: wl-motion ( time x y -- )
    [{: time x y :}h1 time x y b-motion ;] master-task send-event ;
: wl-enter  ( x y -- )
    [{: x y :}h1 x y b-enter ;] master-task send-event ;
: wl-leave  ( -- )
    ['] b-leave master-task send-event ; 

\ pointer listener

:noname { data p axis disc -- } ; wl_pointer_listener-axis_discrete:
:noname { data p time axis -- }  time XTime!
; wl_pointer_listener-axis_stop:
:noname { data p source -- } ; wl_pointer_listener-axis_source:
:noname { data p -- } ; wl_pointer_listener-frame:
:noname { data p time axis val -- }  time XTime!
    time axis val wl-scroll
; wl_pointer_listener-axis:
:noname { data p ser time b mask -- }  time XTime!
    time b mask wl-button
; wl_pointer_listener-button:
:noname { data p time x y -- }  time XTime!
    time x y wl-motion
; wl_pointer_listener-motion:
:noname { data p s -- }
    wl-leave
; wl_pointer_listener-leave:
:noname { data p s x y -- }
    s set-cursor \ on enter, we set the cursor
    x y wl-enter
; wl_pointer_listener-enter:
Create wl-pointer-listener  , , , , , , , , ,

\ keyboard listener

#65288	constant XK_BackSpace
#65289	constant XK_Tab
#65290	constant XK_Linefeed
#65291	constant XK_Clear
#65293	constant XK_Return
#65299	constant XK_Pause
#65300	constant XK_Scroll_Lock
#65301	constant XK_Sys_Req
#65307	constant XK_Escape
#65535	constant XK_Delete
#65312	constant XK_Multi_key
#65335	constant XK_Codeinput
#65340	constant XK_SingleCandidate
#65341	constant XK_MultipleCandidate
#65342	constant XK_PreviousCandidate
#65313	constant XK_Kanji
#65314	constant XK_Muhenkan
#65315	constant XK_Henkan_Mode
#65315	constant XK_Henkan
#65316	constant XK_Romaji
#65317	constant XK_Hiragana
#65318	constant XK_Katakana
#65319	constant XK_Hiragana_Katakana
#65320	constant XK_Zenkaku
#65321	constant XK_Hankaku
#65322	constant XK_Zenkaku_Hankaku
#65323	constant XK_Touroku
#65324	constant XK_Massyo
#65325	constant XK_Kana_Lock
#65326	constant XK_Kana_Shift
#65327	constant XK_Eisu_Shift
#65328	constant XK_Eisu_toggle
#65335	constant XK_Kanji_Bangou
#65341	constant XK_Zen_Koho
#65342	constant XK_Mae_Koho
#65360	constant XK_Home
#65361	constant XK_Left
#65362	constant XK_Up
#65363	constant XK_Right
#65364	constant XK_Down
#65365	constant XK_Prior
#65365	constant XK_Page_Up
#65366	constant XK_Next
#65366	constant XK_Page_Down
#65367	constant XK_End
#65368	constant XK_Begin
#65376	constant XK_Select
#65377	constant XK_Print
#65378	constant XK_Execute
#65379	constant XK_Insert
#65381	constant XK_Undo
#65382	constant XK_Redo
#65383	constant XK_Menu
#65384	constant XK_Find
#65385	constant XK_Cancel
#65386	constant XK_Help
#65387	constant XK_Break
#65406	constant XK_Mode_switch
#65406	constant XK_script_switch
#65407	constant XK_Num_Lock
#65408	constant XK_KP_Space
#65417	constant XK_KP_Tab
#65421	constant XK_KP_Enter
#65425	constant XK_KP_F1
#65426	constant XK_KP_F2
#65427	constant XK_KP_F3
#65428	constant XK_KP_F4
#65429	constant XK_KP_Home
#65430	constant XK_KP_Left
#65431	constant XK_KP_Up
#65432	constant XK_KP_Right
#65433	constant XK_KP_Down
#65434	constant XK_KP_Prior
#65434	constant XK_KP_Page_Up
#65435	constant XK_KP_Next
#65435	constant XK_KP_Page_Down
#65436	constant XK_KP_End
#65437	constant XK_KP_Begin
#65438	constant XK_KP_Insert
#65439	constant XK_KP_Delete
#65469	constant XK_KP_Equal
#65450	constant XK_KP_Multiply
#65451	constant XK_KP_Add
#65452	constant XK_KP_Separator
#65453	constant XK_KP_Subtract
#65454	constant XK_KP_Decimal
#65455	constant XK_KP_Divide
#65456	constant XK_KP_0
#65457	constant XK_KP_1
#65458	constant XK_KP_2
#65459	constant XK_KP_3
#65460	constant XK_KP_4
#65461	constant XK_KP_5
#65462	constant XK_KP_6
#65463	constant XK_KP_7
#65464	constant XK_KP_8
#65465	constant XK_KP_9
#65470	constant XK_F1
#65471	constant XK_F2
#65472	constant XK_F3
#65473	constant XK_F4
#65474	constant XK_F5
#65475	constant XK_F6
#65476	constant XK_F7
#65477	constant XK_F8
#65478	constant XK_F9
#65479	constant XK_F10
#65480	constant XK_F11
#65480	constant XK_L1
#65481	constant XK_F12

256 Cells buffer: wl-key>ekey#
: wl-key! ( keycode number -- )
    $FF00 - cells wl-key>ekey# + ! ;
#del 
XK_BackSpace wl-key!
#tab XK_Tab wl-key!
#lf XK_Linefeed wl-key!
#esc XK_Escape wl-key!
k-enter XK_Return wl-key!
k-home XK_Home wl-key!
k-end XK_End wl-key!
k-left XK_Left wl-key!
k-up XK_Up wl-key!
k-right XK_Right wl-key!
k-down XK_Down wl-key!
k-insert XK_Insert wl-key!
k-delete XK_Delete wl-key!
k-prior XK_Prior wl-key!
k-next XK_Next wl-key!
k-f1 XK_F1 wl-key!
k-f2 XK_F2 wl-key!
k-f3 XK_F3 wl-key!
k-f4 XK_F4 wl-key!
k-f5 XK_F5 wl-key!
k-f6 XK_F6 wl-key!
k-f7 XK_F7 wl-key!
k-f8 XK_F8 wl-key!
k-f9 XK_F9 wl-key!
k-f10 XK_F10 wl-key!
k-f11 XK_F12 wl-key!
k-f12 XK_F12 wl-key!
k-pause XK_Pause wl-key!

Defer wl-ekeyed ' drop is wl-ekeyed

also xkbcommon
:noname { data wl_keyboard rate delay -- }
; wl_keyboard_listener-repeat_info:
:noname { data wl_keyboard serial mods_depressed mods_latched mods_locked group -- }
    xkb-state
    mods_depressed mods_latched mods_locked 0 0 group xkb_state_update_mask
; wl_keyboard_listener-modifiers:
:noname { data wl_keyboard serial time wl-key state -- }
    wayland( state wl-key [: cr h. h. ;] do-debug )
    state WL_KEYBOARD_KEY_STATE_PRESSED = IF
	xkb-state wl-key 8 + xkb_state_key_get_one_sym
	\ wayland( [: dup h. ;] do-debug ) wl-ekeyed
	dup $FF00 and $FF00 = IF
	    $FF00 - cells wl-key>ekey# + @
	THEN  wl-ekeyed
    THEN
; wl_keyboard_listener-key:
:noname { data wl_keyboard serial surface -- }
; wl_keyboard_listener-leave:
:noname	{ data wl_keyboard serial surface keys -- }
; wl_keyboard_listener-enter:
:noname { data wl_keyboard format fd size -- }
    sp@ sp0 !
    0 size PROT_READ MAP_PRIVATE fd 0 mmap { buf }
    wayland( buf size [: cr ." xkbd map:" cr type ;] do-debug )
    XKB_CONTEXT_NO_FLAGS xkb_context_new dup to xkb-ctx
    buf size 1- XKB_KEYMAP_FORMAT_TEXT_V1 XKB_KEYMAP_COMPILE_NO_FLAGS
    xkb_keymap_new_from_string to keymap
    buf size munmap ?ior
    keymap xkb_state_new to xkb-state
; wl_keyboard_listener-keymap:
previous
Create wl-keyboard-listener , , , , , ,

\ seat listener

:noname { data seat d: name -- } ; wl_seat_listener-name:
:noname { data seat caps -- }
    caps WL_SEAT_CAPABILITY_POINTER and IF
	wl-seat wl_seat_get_pointer to wl-pointer
	wl-pointer wl-pointer-listener 0 wl_pointer_add_listener drop
    THEN
    caps WL_SEAT_CAPABILITY_KEYBOARD and IF
	wl-seat wl_seat_get_keyboard to wl-keyboard
	wl-keyboard wl-keyboard-listener 0 wl_keyboard_add_listener drop
    THEN
    caps WL_SEAT_CAPABILITY_TOUCH and IF
	wl-seat wl_seat_get_touch to wl-touch
    THEN ; wl_seat_listener-capabilities:
Create wl-seat-listener  , ,

\ xdg-wm-base-listener

:noname ( data xdg_wm_base serial -- )
    xdg_wm_base_pong drop ; xdg_wm_base_listener-ping:

Create xdg-wm-base-listener ,

\ input listener

Defer wayland-keys

:noname ( addr u -- ) inskeys ; is wayland-keys

Create cursor-xywh #200 , #300 , #1 , #10 ,

: send-status-update { text-input -- }
    text-input
    ZWP_TEXT_INPUT_V3_CONTENT_HINT_NONE
    ZWP_TEXT_INPUT_V3_CONTENT_PURPOSE_NORMAL
    zwp_text_input_v3_set_content_type
    text-input cursor-xywh 4 cells bounds DO  I @  cell  +LOOP
    zwp_text_input_v3_set_cursor_rectangle
    text-input s" " 0 0
    zwp_text_input_v3_set_surrounding_text
    text-input zwp_text_input_v3_commit ;

:noname { data text-input serial -- }
    text-input send-status-update
; zwp_text_input_v3_listener-done:
:noname { data text-input before_length after_length -- }
; zwp_text_input_v3_listener-delete_surrounding_text:
:noname { data text-input d: text -- }
    wayland( text [: cr ." wayland keys: '" type ''' emit ;] do-debug )
    text wayland-keys
    text-input zwp_text_input_v3_commit
; zwp_text_input_v3_listener-commit_string:
:noname { data text-input d: text cursor_begin cursor_end -- }
; zwp_text_input_v3_listener-preedit_string:
:noname { data text-input surface -- }
; zwp_text_input_v3_listener-leave:
:noname { data text-input surface -- }
    text-input zwp_text_input_v3_enable
    text-input send-status-update
; zwp_text_input_v3_listener-enter:
Create text-input-listener , , , , , ,

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
: zwp_text_input_manager_v3 ( registry name -- )
    zwp_text_input_manager_v3_interface 1 wl_registry_bind to text-input-manager
    text-input-manager wl-seat zwp_text_input_manager_v3_get_text_input to text-input
    text-input text-input-listener 0 zwp_text_input_v3_add_listener drop ;
: xdg_wm_base ( registry name -- )
    xdg_wm_base_interface 1 wl_registry_bind dup to xdg-wm-base
    xdg-wm-base xdg-wm-base-listener 0 xdg_wm_base_add_listener drop ;
: zxdg_decoration_manager_v1 ( registry name -- )
    zxdg_decoration_manager_v1_interface 1 wl_registry_bind to decoration-manager ;

set-current
    
: registry+ { data registry name d: interface version -- }
    sp@ sp0 ! rp@ cell+ rp0 !
    wayland( interface [: cr type ;] do-debug )
    interface wl-registry find-name-in ?dup-IF
	registry name rot name>interpret execute
    ELSE
	wayland( [: ."  unhandled" ;] do-debug )
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

\ xdg surface listener

0 Value mapped
0 Value configured

forward sync
forward clear

:noname { data xdg_surface serial -- }
    wayland( [: cr ." configured" ;] do-debug )
    true to mapped
    xdg_surface serial xdg_surface_ack_configure
    wl-surface wl_surface_commit
; xdg_surface_listener-configure:
Create xdg-surface-listener ,

: map-win ( -- )
    BEGIN  get-events mapped  UNTIL ;

Variable level#

:noname { data xdg_toplevel capabilities -- } ;
xdg_toplevel_listener-wm_capabilities:
:noname { data xdg_toplevel width height -- }
    wayland( height width [: cr ." toplevel bounds: " . . ;] do-debug ) ;
xdg_toplevel_listener-configure_bounds:
:noname { data xdg_toplevel -- }
    wayland( [: cr ." close" ;] do-debug )
    -1 level# +! ;
xdg_toplevel_listener-close:
:noname { data xdg_toplevel width height states -- }
    wayland( states wl_array-data @ states wl_array-size @
    height width [: cr ." toplevel-config: " . .
    dump ;] do-debug )
;
xdg_toplevel_listener-configure:
Create xdg-toplevel-listener , , , ,

:noname { data decoration mode -- }
    wayland( [: cr ." decorated" ;] do-debug )
    true to configured clear sync ;
zxdg_toplevel_decoration_v1_listener-configure: Create xdg-decoration-listener ,

: wl-eglwin { w h -- }
    wayland( h w [: cr ." eglwin: " . . ;] do-debug )
    xdg-wm-base wl-surface xdg_wm_base_get_xdg_surface dup to xdg-surface
    xdg-surface-listener 0 xdg_surface_add_listener drop
    xdg-surface xdg_surface_get_toplevel to xdg-toplevel
    xdg-surface 0 0 w h xdg_surface_set_window_geometry
    xdg-toplevel xdg-toplevel-listener 0 xdg_toplevel_add_listener drop
    xdg-toplevel s" ΜΙΝΟΣ2 OpenGL Window" xdg_toplevel_set_title
    xdg-toplevel s" ΜΙΝΟΣ2" xdg_toplevel_set_app_id
    xdg-toplevel xdg_toplevel_set_maximized
    decoration-manager xdg-toplevel
    zxdg_decoration_manager_v1_get_toplevel_decoration dup to zxdg-decoration
    dup xdg-decoration-listener 0
    zxdg_toplevel_decoration_v1_add_listener drop
    ZXDG_TOPLEVEL_DECORATION_V1_MODE_SERVER_SIDE
    zxdg_toplevel_decoration_v1_set_mode
    compositor wl_compositor_create_region { region }
    region 0 0 w h wl_region_add
    wl-surface region wl_surface_set_opaque_region
    wl-surface w h wl_egl_window_create to win
    wl-surface wl_surface_commit
    wayland( [: cr ." wl-eglwin done" ;] do-debug ) ;

also opengl
: getwh ( -- )
    0 0 dpy-wh 2@ glViewport ;
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

Variable rendering -2 rendering !
#16 Value config-change#

: ?looper ;

Defer window-init     ' noop is window-init
: window-init, ( xt -- )
    >r :noname action-of window-init compile, r@ compile,
    postpone ; is window-init
    ctx IF  r@ execute  THEN  rdrop ;
Defer config-changed  ' noop is config-changed
Defer screen-ops      ' noop IS screen-ops
Defer reload-textures ' noop is reload-textures

: gl-init ( -- ) \ minos2
    \G if not already opened, open window and initialize OpenGL
    ctx 0= IF window-init THEN ;

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
