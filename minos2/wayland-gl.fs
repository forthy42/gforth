\ Wayland window for GLES

\ Author: Bernd Paysan
\ Copyright (C) 2017,2019,2023 Free Software Foundation, Inc.

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
require unix/socket.fs
require unix/pthread.fs
require mini-oof2.fs
require struct-val.fs
require trigger-value.fs

[IFUNDEF] linux  : linux ;  [THEN]

also wayland

debug: wayland( \ )

$Variable window-title$ s" ΜΙΝΟΣ2 OpenGL Window" window-title$ $!
$Variable window-app-id$ s" ΜΙΝΟΣ2" window-app-id$ $!

0 Value dpy        \ wayland display
0 ' noop trigger-Value wl-compositor \ wayland compositor
0 Value wl-output
[IFDEF] wp_fractional_scale_v1_listener
    0 ' noop trigger-Value wp-fractional-scale-v1
    0 ' noop trigger-Value wp-fractional-scale-manager-v1
[THEN]
0 ' noop trigger-Value wl-shell   \ wayland shell
0 ' noop trigger-Value wl-pointer
0 Value wl-keyboard
0 Value wl-touch
0 Value xkb-ctx
0 Value xkb-state
0 Value keymap
0 Value registry
0 Value win
0 ' noop trigger-Value cursor-theme
0 ' noop trigger-Value cursor
0 ' noop trigger-Value cursor-surface
0 ' noop trigger-Value cursor-serial
0 ' noop trigger-Value wl-surface
0 ' noop trigger-Value wp-viewporter
0 ' noop trigger-Value wp-viewport
0 ' noop trigger-Value shell-surface
0 ' noop trigger-Value wl-seat
0 ' noop trigger-Value wl-shm
0 ' noop trigger-Value zwp-text-input-manager-v3
0 Value text-input
0 ' noop trigger-Value xdg-wm-base
0 ' noop trigger-Value xdg-surface
0 ' noop trigger-Value xdg-toplevel
0 ' noop trigger-Value zxdg-decoration-manager-v1
0 Value zxdg-decoration
0 ' noop trigger-Value wl-data-device-manager
0 ' noop trigger-Value data-device
0 ' noop trigger-Value data-source
0 ' noop trigger-Value zwp-primary-selection-device-manager-v1
0 ' noop trigger-Value primary-selection-device
0 ' noop trigger-Value primary-selection-source

\ set a cursor

:trigger-on( cursor-serial cursor-surface cursor wl-pointer )
    wayland( cursor-serial [: cr ." Set cursor, serial " h. ;] do-debug )
    cursor wl_cursor-images @ @ { image }
    wl-pointer cursor-serial cursor-surface
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

: shell-surface-ping ( data surface serial -- )
    wl_shell_surface_pong drop ;
: shell-surface-config { data surface edges w h -- }
    win w h 0 0 wl_egl_window_resize ;
: shell-surface-popup-done { data surface -- } ;

: <cb ( -- ) depth r> swap >r >r ;
: cb> ( xt1 .. xtn -- )
    Create depth r> r> swap >r - 0 ?DO , LOOP ;

${GFORTH_IGNLIB} "true" str= [IF]
    : ?cb ( xt -- 0 ) drop parse-name 2drop 0 ;
[ELSE]
    : ?cb ( xt "name" -- addr ) ;
[THEN]

<cb
' shell-surface-popup-done ?cb wl_shell_surface_listener-popup_done:
' shell-surface-config ?cb wl_shell_surface_listener-configure:
' shell-surface-ping ?cb wl_shell_surface_listener-ping:
cb> wl-shell-surface-listener

\ time handling

0 Value timeoffset
: XTime0 ( -- mtime ) utime #1000 ud/mod drop nip ;
: XTime ( -- mtime ) XTime0 timeoffset + ;
: XTime! ( mtime -- ) XTime0 - to timeoffset ;

\ geometry output listener

2Variable wl-metrics
[IFUNDEF] dpy-wh
    2Variable dpy-wh
[THEN]
2Variable dpy-unscaled-wh
1 Value wl-scale
#120 Value fractional-scale
0 Value screen-orientation

1e 256e f/ fconstant 1/256
: scale* ( n1 -- n2 )
    fractional-scale #60 */ 1+ 2/ ;
: scale*fixed ( n1 -- n2 )
    fractional-scale 8 lshift #60 */ 1+ 2/ ;
: coord>f ( fixed -- r )
    1/256 fm* fractional-scale #120 fm*/ ;
: n>coord ( n -- r )
    scale*fixed 1/256 fm* ;

: wl-out-geometry { data out x y pw ph subp d: make d: model transform -- }
    wayland( pw ph [: cr ." metrics: " . . ;] do-debug )
    pw ph wl-metrics 2! transform to screen-orientation ;
: wl-out-mode { data out flags w h r -- }
    w h dpy-wh 2! ;
: wl-out-done { data out -- } ;
: wl-out-scale { data out scale -- }
    wayland( scale [: cr ." scale: " . ;] do-debug )
    scale to wl-scale ;
: wl-out-name { data out d: name -- }
    wayland( name [: cr ." output name: " type ;] do-debug )
;
: wl-out-description { data out d: description -- }
    wayland( description [: cr ." output description: " type ;] do-debug )
;

<cb
' wl-out-description ?cb wl_output_listener-description:
' wl-out-name ?cb wl_output_listener-name:
' wl-out-scale ?cb wl_output_listener-scale:
' wl-out-done ?cb wl_output_listener-done:
' wl-out-mode ?cb wl_output_listener-mode:
' wl-out-geometry ?cb wl_output_listener-geometry:
cb> wl-output-listener

require need-x.fs

Defer config-changed
:noname ( -- ) +sync +config ( getwh ) ; is config-changed
Defer screen-ops      ' noop IS screen-ops
Defer reload-textures ' noop is reload-textures

also opengl
: resize-widgets ( w h -- )
    dpy-wh 2!  config-changed ;
: rescale-win { width height -- }
    width height d0= ?EXIT
    width height dpy-unscaled-wh 2!
    win ?dup-IF  width scale* height scale* 0 0 wl_egl_window_resize  THEN
    xdg-surface 0 0 width height xdg_surface_set_window_geometry
    wp-viewport ?dup-IF
	wayland( height n>coord width n>coord
	[: cr ." source w h: " f. f. ;] do-debug )
	dup 0 0 width scale*fixed height scale*fixed wp_viewport_set_source
	width height wp_viewport_set_destination
    THEN
    wl-surface ?dup-IF  wl_surface_commit  THEN
    width scale* height scale* resize-widgets ;
previous

Defer rescaler ' noop is rescaler

[IFDEF] wp_fractional_scale_v1_listener
    <cb
    :noname { data fscale scale -- }
	wayland( scale [: cr ." fractional scale: " . ;] do-debug )
	scale to fractional-scale  rescaler
	dpy-unscaled-wh 2@ rescale-win
    ; ?cb wp_fractional_scale_v1_listener-preferred_scale:
    cb> wp-fractional-scale-v1-listener
[THEN]

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

Variable wl-time

<cb
:noname { data p axis disc -- }
; ?cb wl_pointer_listener-axis_relative_direction:
:noname { data p axis val -- }
    XTime axis val wl-scroll
; ?cb wl_pointer_listener-axis_value120:
:noname { data p axis disc -- }
; ?cb wl_pointer_listener-axis_discrete:
:noname { data p time axis -- } time XTime!
; ?cb wl_pointer_listener-axis_stop:
:noname { data p source -- } ; ?cb wl_pointer_listener-axis_source:
:noname { data p -- } ; ?cb wl_pointer_listener-frame:
:noname { data p time axis val -- } time XTime!
; ?cb wl_pointer_listener-axis:
:noname { data p ser time b mask -- }  time XTime!
    time b mask wl-button
; ?cb wl_pointer_listener-button:
:noname { data p time x y -- }  time XTime!
    time x y wl-motion
; ?cb wl_pointer_listener-motion:
:noname { data p s -- }
    wl-leave
; ?cb wl_pointer_listener-leave:
:noname { data p s x y -- }
    wayland( s [: cr ." cursor serial " h. ;] do-debug )
    s to cursor-serial \ on enter, we set the cursor
    x y wl-enter
; ?cb wl_pointer_listener-enter:
cb> wl-pointer-listener

\ keyboard listener

also xkbcommon

$200 Cells buffer: xkb-key>ekey#
: >xkb-key ( keycode number -- )
    $FE00 - cells xkb-key>ekey# + ;
k-backspace	XKB_KEY_BackSpace >xkb-key !
#tab	XKB_KEY_Tab >xkb-key !
#lf	XKB_KEY_Linefeed >xkb-key !
#esc	XKB_KEY_Escape >xkb-key !
k-enter	XKB_KEY_Return >xkb-key !
k-home	XKB_KEY_Home >xkb-key !
k-end	XKB_KEY_End >xkb-key !
k-left	XKB_KEY_Left >xkb-key !
k-up	XKB_KEY_Up >xkb-key !
k-right	XKB_KEY_Right >xkb-key !
k-down	XKB_KEY_Down >xkb-key !
k-insert	XKB_KEY_Insert >xkb-key !
k-delete	XKB_KEY_Delete >xkb-key !
k-prior	XKB_KEY_Prior >xkb-key !
k-next	XKB_KEY_Next >xkb-key !
k-f1	XKB_KEY_F1 >xkb-key !
k-f2	XKB_KEY_F2 >xkb-key !
k-f3	XKB_KEY_F3 >xkb-key !
k-f4	XKB_KEY_F4 >xkb-key !
k-f5	XKB_KEY_F5 >xkb-key !
k-f6	XKB_KEY_F6 >xkb-key !
k-f7	XKB_KEY_F7 >xkb-key !
k-f8	XKB_KEY_F8 >xkb-key !
k-f9	XKB_KEY_F9 >xkb-key !
k-f10	XKB_KEY_F10 >xkb-key !
k-f11	XKB_KEY_F12 >xkb-key !
k-f12	XKB_KEY_F12 >xkb-key !
k-pause	XKB_KEY_Pause >xkb-key !

Defer wl-ekeyed ' drop is wl-ekeyed
Defer wl-ukeyed ' 2drop is wl-ukeyed

0 Value wl-meta

Variable prev-preedit$

: ?setstring
    setstring$ $@len IF  setstring$ $free  THEN ;

<cb
:noname { data wl_keyboard rate delay -- }
; ?cb wl_keyboard_listener-repeat_info:
:noname { data wl_keyboard serial mods_depressed mods_latched mods_locked group -- }
    mods_depressed 5 and mods_depressed 8 and sfloat/ or to wl-meta
    wayland( mods_depressed mods_latched mods_locked
    [: cr ." modes: locked " h. ." latched " h. ." depressed " h. wl-meta h. ;]
    do-debug )
    xkb-state
    mods_depressed mods_latched mods_locked 0 0 group xkb_state_update_mask
; ?cb wl_keyboard_listener-modifiers:
:noname { data wl_keyboard serial time wl-key state -- }
    wayland( state wl-key [: cr ." wayland key: " h. h. ;] do-debug )
    state WL_KEYBOARD_KEY_STATE_PRESSED = IF
	prev-preedit$ $free
	{: | keys[ $10 ] :}
	xkb-state wl-key 8 + keys[ $10 xkb_state_key_get_utf8 ?dup-IF
	    keys[ swap save-mem
	    [{: d: wl-keys :}h1 ?setstring wl-keys wl-ukeyed
	    wl-keys drop free drop ;] master-task send-event
	    EXIT  THEN
	xkb-state wl-key 8 + xkb_state_key_get_one_sym
	\ wayland( [: dup h. ;] do-debug ) wl-ekeyed
	dup $FE00 $10000 within IF
	    >xkb-key @ ?dup-IF  wl-meta mask-shift# lshift or
		[{: wl-key :}h1 ?setstring wl-key wl-ekeyed ;] master-task send-event
	    THEN
	ELSE  drop  THEN
    THEN
; ?cb wl_keyboard_listener-key:
:noname { data wl_keyboard serial surface -- }
; ?cb wl_keyboard_listener-leave:
:noname	{ data wl_keyboard serial surface keys -- }
; ?cb wl_keyboard_listener-enter:
:noname { data wl_keyboard format fd size -- }
    \ sp@ sp0 !
    wayland( fd size [: cr ." xkbd mmap file: " swap . h. ;] do-debug )
    0 size PROT_READ MAP_PRIVATE fd 0 mmap { buf }
    wayland( buf size [: cr ." xkbd map: " swap h. h. ;] do-debug )
    XKB_CONTEXT_NO_FLAGS xkb_context_new dup to xkb-ctx
    buf size 1- XKB_KEYMAP_FORMAT_TEXT_V1 XKB_KEYMAP_COMPILE_NO_FLAGS
    xkb_keymap_new_from_buffer to keymap
    buf size munmap ?ior
    keymap xkb_state_new to xkb-state
; ?cb wl_keyboard_listener-keymap:
previous
cb> wl-keyboard-listener

\ seat listener

<cb
:noname { data seat d: name -- } ; ?cb wl_seat_listener-name:
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
    THEN ; ?cb wl_seat_listener-capabilities:
cb> wl-seat-listener

\ xdg-wm-base-listener

<cb
:noname ( data xdg_wm_base serial -- )
    xdg_wm_base_pong drop ; ?cb xdg_wm_base_listener-ping:
cb> xdg-wm-base-listener

\ input listener

Defer wayland-keys

:noname ( addr u -- ) inskeys ; is wayland-keys

Create old-cursor-xywh #-4200 , #3800 , #-5 , #-100 ,
Create cursor-xywh #200 , #300 , #1 , #10 ,
Create xy-offset 0e f, 0e f,

: >cursor-xyxy { f: x0 f: y0 f: x1 f: y1 -- }
    cursor-xywh
    x0 xy-offset f@ f+ f>s over ! cell+
    y1 xy-offset float+ f@ f+ f>s over ! cell+
    x1 x0 f- f>s over ! cell+
    y0 y1 f- f>s swap ! ;
: +offset { f: x f: y -- }
    xy-offset
    dup f@ x f+ dup f! float+
    dup f@ y f+ f! ;
: 0offset ( -- )
    0e fdup xy-offset f! xy-offset float+ f! ;

: send-status-update { text-input -- }
    text-input
    ZWP_TEXT_INPUT_V3_CONTENT_HINT_NONE
    ZWP_TEXT_INPUT_V3_CONTENT_PURPOSE_NORMAL
    zwp_text_input_v3_set_content_type
    cursor-xywh 4 cells old-cursor-xywh over str= 0= IF
	text-input cursor-xywh 4 cells bounds DO  I @  cell  +LOOP
	zwp_text_input_v3_set_cursor_rectangle
	cursor-xywh old-cursor-xywh 4 cells move
    THEN
    text-input s" " 0 0
    zwp_text_input_v3_set_surrounding_text
    text-input zwp_text_input_v3_commit ;

Defer sync+config ' noop is sync+config

<cb
:noname { data text-input serial -- }
    text-input send-status-update
; ?cb zwp_text_input_v3_listener-done:
:noname { data text-input before_length after_length -- }
    text-input send-status-update
; ?cb zwp_text_input_v3_listener-delete_surrounding_text:
:noname { data text-input d: text -- }
    wayland( text [: cr ." wayland keys: '" type ''' emit ;] do-debug )
    prev-preedit$ $free  text save-mem
    [{: d: text :}h1 ?setstring
	text wayland-keys text drop free drop ;] master-task send-event
; ?cb zwp_text_input_v3_listener-commit_string:
:noname { data text-input d: text cursor_begin cursor_end -- }
    text prev-preedit$ $@ str= 0= IF
	text prev-preedit$ $!
	wayland( text [: cr ." preedit: '" type ''' emit ;] do-debug )
	text save-mem [{: d: text :}h1
	    text setstring$ $! sync+config
	    text drop free throw ;]
	master-task send-event
    THEN
; ?cb zwp_text_input_v3_listener-preedit_string:
:noname { data text-input surface -- }
    text-input zwp_text_input_v3_commit
; ?cb zwp_text_input_v3_listener-leave:
:noname { data text-input surface -- }
    text-input zwp_text_input_v3_enable
    text-input send-status-update
; ?cb zwp_text_input_v3_listener-enter:
cb> text-input-listener

\ data offer listener

0 Value current-serial
$[]Variable mime-types[]
$[]Variable ds-mime-types[]
$[]Variable liked-mime[]

$Variable clipboard$
$Variable primary$
$Variable dnd$

false Value my-clipboard
false Value my-primary
false Value my-dnd

"text/plain;charset=utf-8" liked-mime[] $+[]!
"UTF8_STRING"              liked-mime[] $+[]!
"text/uri-list"            liked-mime[] $+[]!

"text/plain;charset=utf-8" ds-mime-types[] $+[]!
"UTF8_STRING"              ds-mime-types[] $+[]!

: ?mime-type ( addr u -- flag )
    false -rot
    mime-types[] [: 2over str= IF  rot drop true -rot  THEN ;] $[]map
    2drop ;

0 Value clipin-fd
0 Value clipout-fd
0 Value psin-fd
0 Value psout-fd

$Variable clipin$
$Variable clipout$
Variable clipout-offset

$Variable psin$
$Variable psout$
Variable psout-offset

: eof-clipin ( -- )
    clipin-fd 0 to clipin-fd close-file throw
    0 clipin$ !@ clipboard$ !@ ?dup-IF  free throw  THEN
    wayland( [: cr ." read clipboard$ with '" clipboard$ $@ type ." '" ;] do-debug ) ;

: read-clipin ( -- )
    clipin-fd check_read dup 0> IF \ data available
	dup clipboard$ $+!len swap dup >r clipin-fd
	read-file throw
	r> - dup 0< IF  clipin$ $+!len  THEN  drop
    ELSE
	drop \ eof-clipin
    THEN ;

: eof-psin ( -- )
    psin-fd 0 to psin-fd close-file throw
    0 psin$ !@ primary$ !@ ?dup-IF  free throw  THEN
    wayland( [: cr ." read primary$ with '" primary$ $@ type ." '" ;] do-debug ) ;

: read-psin ( -- )
    psin-fd check_read dup 0> IF \ data available
	dup psin$ $+!len swap dup >r psin-fd
	read-file throw
	r> - dup 0< IF  psin$ $+!len  THEN  drop
    ELSE
	drop \ eof-psin
    THEN ;

[IFUNDEF] FIONBIO
    0x5421 Constant FIONBIO \ works for Linux, which is good enough for Wayland
[THEN]

: set-noblock ( fd -- )
    { | w^ arg }  1 arg l!
    dup FIONBIO arg ioctl ?ior ;

: write-out { out$ offset fd -- fd }
    out$ $@ offset @ safe/string
    fd -rot write dup -1 <> IF  offset +!
	fd out$ $@len offset @ u> ?EXIT drop
    ELSE
	drop errno EAGAIN <> IF
	    -512 errno - [: cr ." Error writing clipboard pipe: " error$ type ;] do-debug
	ELSE
	    fd  EXIT
	THEN
    THEN \ if we can't write, let's just abandon this operation
    wayland( out$ [: cr ." wrote '" $. ." ' to clipout" ;] do-debug )
    fd close -1 = IF
	-512 errno - [: cr ." Error closing clipboard pipe: " error$ type ;] do-debug
    THEN
    clipout$ $free 0 ;

: write-clipout ( -- )
    clipout$ clipout-offset clipout-fd write-out to clipout-fd ;
: write-psout ( -- )
    psout$ psout-offset psout-fd write-out to psout-fd ;

: master-task send-event ( xt -- )
    wayland( [: cr ." queue clipin: " dup h. ;] do-debug )
    master-task send-event ;
: queue-clipout ( xt -- )
    wayland( [: cr ." queue clipout: " dup h. ;] do-debug )
    master-task send-event ;

: accept+receive { offer d: mime-type | fds[ 2 cells ] -- }
    offer current-serial mime-type wl_data_offer_accept
    fds[ create_pipe
    offer mime-type fds[ cell+ @ fileno wl_data_offer_receive
    fds[ cell+ @ close-file throw
    fds[ @ to clipin-fd ;

: >liked-mime { xt: xt -- }
    liked-mime[] $[]# 0 ?DO
	I liked-mime[] $[]@ ?mime-type IF
	    I liked-mime[] $[]@
	    wayland( [: cr ." accept: " 2dup type ;] do-debug )
	    xt  LEAVE
	THEN
    LOOP ;

<cb
:noname { data offer dnd-actions -- }
    wayland( dnd-actions [: cr ." dnd-actions: " h. ;] do-debug )
; ?cb wl_data_offer_listener-action
:noname { data offer source-actions -- }
    wayland( source-actions [: cr ." source-actions: " h. ;] do-debug )
    my-clipboard 0= IF
	offer source-actions [{: offer actions :}l offer -rot
	    actions IF  dnd$  ELSE  clipboard$  THEN
	    accept+receive ;] >liked-mime
    THEN
; ?cb wl_data_offer_listener-source_actions:
:noname { data offer d: mime-type -- }
    wayland( mime-type [: cr ." mime-type: " type ;] do-debug )
    mime-type mime-types[] $+[]!
; ?cb wl_data_offer_listener-offer:
cb> data-offer-listener

\ data device listener

2Variable dnd-xy
Defer dnd-move
Defer dnd-drop

0 Value old-id

<cb
:noname { data data-device id -- }
    wayland( id [: cr ." selection id: " h. ;] do-debug )
; ?cb wl_data_device_listener-selection:
:noname { data data-device -- }
    wayland( [: cr ." drop" ;] do-debug )
    [: dnd-xy 2@ dnd$ $@ dnd-drop ;]
    master-task send-event
; ?cb wl_data_device_listener-drop:
:noname { data data-device time x y -- }
    wayland( y x time [: cr ." motion [time,x,y] " . . . ;] do-debug )
    x y dnd-xy 2!
    x y [{: x y :}h1 x y dnd-move ;]
    master-task send-event
; ?cb wl_data_device_listener-motion:
:noname { data data-device -- }
    wayland( [: cr ." leave" ;] do-debug )
; ?cb wl_data_device_listener-leave:
:noname { data data-device serial surface x y id -- }
    wayland( id y x surface [: cr ." enter [surface,x,y,id] " h. . . h. ;] do-debug )
    serial to current-serial
; ?cb wl_data_device_listener-enter:
:noname { data data-device id -- }
    wayland( id [: cr ." offer: " h. ;] do-debug )
    old-id ?dup-IF  wl_data_offer_destroy  THEN
    id to old-id
    mime-types[] $[]free
    id data-offer-listener 0 wl_data_offer_add_listener drop
; ?cb wl_data_device_listener-data_offer:
cb> data-device-listener

\ primary selection offer listener

: ps-accept+receive { offer d: mime-type | fds[ 2 cells ] -- }
    fds[ create_pipe
    offer mime-type fds[ cell+ @ fileno zwp_primary_selection_offer_v1_receive
    fds[ cell+ @ close-file throw
    fds[ @ to psin-fd ;

<cb
:noname { data offer d: mime-type -- }
    wayland( mime-type [: cr ." primary mime-type: " type ;] do-debug )
    mime-type mime-types[] $+[]!
; ?cb zwp_primary_selection_offer_v1_listener-offer:
cb> primary-selection-offer-listener

\ primary selection device listener

0 Value old-ps-id

<cb
:noname { data data-device id -- }
    wayland( id [: cr ." primary selection id: " h. ;] do-debug )
    my-primary 0= IF
	id  [{: id :}l id -rot ps-accept+receive ;] >liked-mime
    THEN
; ?cb zwp_primary_selection_device_v1_listener-selection:
:noname { data data-device id -- }
    wayland( id [: cr ." primary offer: " h. ;] do-debug )
    old-ps-id ?dup-IF  zwp_primary_selection_offer_v1_destroy  THEN
    id to old-ps-id
    mime-types[] $[]free  0 to my-primary
    id primary-selection-offer-listener 0 zwp_primary_selection_offer_v1_add_listener
; ?cb zwp_primary_selection_device_v1_listener-data_offer:
cb> primary-selection-listener

\ data source listener

<cb
:noname { data source dnd-action -- }
    wayland( dnd-action [: cr ." ds action: " h. ;] do-debug )
; ?cb wl_data_source_listener-action:
:noname { data source -- }
; ?cb wl_data_source_listener-dnd_finished:
:noname { data source -- }
; ?cb wl_data_source_listener-dnd_drop_performed:
:noname { data source -- }
; ?cb wl_data_source_listener-cancelled:
:noname { data source d: mime-type fd -- }
    wayland( mime-type data [: cr ." send " id. ." type " type ;] do-debug )
    data $@ clipout$ $! fd to clipout-fd  clipout-offset off
    fd set-noblock  write-clipout
; ?cb wl_data_source_listener-send:
:noname { data source d: mime-type -- }
    wayland( data mime-type [: cr ." ds target: " type space id. ;] do-debug )
; ?cb wl_data_source_listener-target:
cb> data-source-listener

\ primary selection source listener

<cb
:noname { data source -- }
    wayland( [: cr ." ps cancelled" ;] do-debug )
; ?cb zwp_primary_selection_source_v1_listener-cancelled:
:noname { data source d: mime-type fd -- }
    wayland( fd mime-type data [: cr ." ps send " id. ." type: " type ."  fd: " h. ;] do-debug )
    fd to psout-fd  data $@ psout$ $!  psout-offset off
    fd set-noblock  write-psout
; ?cb zwp_primary_selection_source_v1_listener-send:
cb> primary-selection-source-listener

\ registry listeners: the interface string is searched in a table

$Variable cursor-theme$ "Breeze_Light" cursor-theme$ $!
Variable cursor-size #24 cursor-size !

: read-kde-cursor-theme ( -- )
    "~/.config/kcminputrc" r/o open-file IF  drop  EXIT  THEN
    [: BEGIN  refill  WHILE
		source "cursorTheme=" string-prefix? IF
		    source #12 safe/string cursor-theme$ $!
		THEN
		source "cursorSize=" string-prefix? IF
		    source #11 safe/string s>number drop $10 max cursor-size !
		THEN
	REPEAT ;] execute-parsing-file
    wayland( [: cr ." cursor: " cursor-theme$ $. ."  size: " cursor-size ?
    ;] do-debug ) ;

: read-gnome-cursor-theme ( -- )
    "~/.config/gtk-4.0/settings.ini" r/o open-file IF  drop  EXIT  THEN
    [: BEGIN  refill  WHILE
		source "gtk-cursor-theme-name=" string-prefix? IF
		    source #22 safe/string cursor-theme$ $!
		THEN
		source "gtk-cursor-theme-size=" string-prefix? IF
		    source #22 safe/string s>number drop $10 max cursor-size !
		THEN
	REPEAT ;] execute-parsing-file
    wayland( [: cr ." cursor: " cursor-theme$ $. ."  size: " cursor-size ?
    ;] do-debug ) ;

: read-cursor-theme ( -- )
    wayland( [: cr ." Read " ${XDG_CURRENT_DESKTOP} type ."  Theme" ;] do-debug )
    ${XDG_CURRENT_DESKTOP} "KDE" str= IF  read-kde-cursor-theme  EXIT  THEN
    ${XDG_CURRENT_DESKTOP} "GNOME" str= IF  read-gnome-cursor-theme  EXIT  THEN ;

:trigger-on( wl-data-device-manager wl-seat )
    data-device ?EXIT
    wl-data-device-manager
    wl-seat wl_data_device_manager_get_data_device to data-device ;
:trigger-on( data-device )
    data-device data-device-listener 0 wl_data_device_add_listener drop
    wl-data-device-manager wl_data_device_manager_create_data_source
    to data-source ;
:trigger-on( data-source )
    data-source data-source-listener clipboard$ wl_data_source_add_listener drop
    ds-mime-types[] [: data-source -rot wl_data_source_offer ;] $[]map ;

:trigger-on( zwp-primary-selection-device-manager-v1 wl-seat )
    primary-selection-device ?EXIT
    zwp-primary-selection-device-manager-v1
    wl-seat zwp_primary_selection_device_manager_v1_get_device to primary-selection-device ;
:trigger-on( primary-selection-device )
    primary-selection-device primary-selection-listener 0 zwp_primary_selection_device_v1_add_listener drop
    zwp-primary-selection-device-manager-v1 zwp_primary_selection_device_manager_v1_create_source
    to primary-selection-source ;
:trigger-on( primary-selection-source )
    primary-selection-source primary-selection-source-listener primary$ zwp_primary_selection_source_v1_add_listener drop
    ds-mime-types[] [: primary-selection-source -rot zwp_primary_selection_source_v1_offer ;] $[]map ;

: >wl-replaces ( version "name -- )
    s>d <# #s #> "minver" replaces
    parse-name 2dup "wl_name" replaces
    2dup bounds ?DO  I c@ '_' = IF  '-' I c!  THEN  LOOP "wl-name" replaces ;

: wl-macro1 ( -- )
    ": %wl_name% %wl_name%_interface swap %minver% umin wl_registry_bind to %wl-name%"
    $substitute drop evaluate ;
: wl-macro2 ( -- )
    "%wl-name% %wl-name%-listener 0 %wl_name%_add_listener drop"
    $substitute drop evaluate ;
: wl: ( version "name" -- )
    >wl-replaces wl-macro1 postpone ; ;
: wlal: ( version "name" -- )
    >wl-replaces wl-macro1 wl-macro2 postpone ; ;

table Constant wl-registry

get-current

wl-registry set-current

5 wl: wl_compositor
:trigger-on( wl-compositor )
    wl-compositor wl_compositor_create_surface to wl-surface
    wl-compositor wl_compositor_create_surface to cursor-surface ;
1 wl: wl_shell
:trigger-on( wl-shell wl-surface )
    wl-shell wl-surface wl_shell_get_shell_surface to shell-surface ;
[IFDEF] wp_fractional_scale_v1_listener
    1 wl: wp_fractional_scale_manager_v1
    :trigger-on( wp-fractional-scale-manager-v1 wl-surface )
	wp-fractional-scale-manager-v1 wl-surface
	wp_fractional_scale_manager_v1_get_fractional_scale
	to wp-fractional-scale-v1 ;
    :trigger-on( wp-fractional-scale-v1 )
	wp-fractional-scale-v1 wp-fractional-scale-v1-listener
	0 wp_fractional_scale_v1_add_listener drop ;
[THEN]
:trigger-on( shell-surface )
    shell-surface wl-shell-surface-listener 0 wl_shell_surface_add_listener drop
    shell-surface wl_shell_surface_set_toplevel
    shell-surface window-title$ $@ wl_shell_surface_set_title
    shell-surface window-app-id$ $@ wl_shell_surface_set_class ;
1 wl: wp_viewporter
:trigger-on( wp-viewporter wl-surface )
    wp-viewporter wl-surface wp_viewporter_get_viewport to wp-viewport ;
4 wlal: wl_output
8 wlal: wl_seat
1 wl: wl_shm
:trigger-on( wl-shm )
    cursor-theme$ $@ cursor-size @
    wayland( [: cr ." load cursor theme " third third type ."  size " dup . ;] do-debug )
    wl-shm wl_cursor_theme_load dup to cursor-theme
    s" default" wl_cursor_theme_get_cursor to cursor ;
1 wl: zwp_text_input_manager_v3
:trigger-on( zwp-text-input-manager-v3 wl-seat )
    zwp-text-input-manager-v3 wl-seat zwp_text_input_manager_v3_get_text_input dup to text-input
    text-input-listener 0 zwp_text_input_v3_add_listener drop
    text-input send-status-update ;
4 wlal: xdg_wm_base
1 wl: zxdg_decoration_manager_v1
3 wl: wl_data_device_manager
1 wl: zwp_primary_selection_device_manager_v1

set-current

: registry+ { data registry name d: interface version -- }
    \ sp@ sp0 ! rp@ cell+ rp0 !
    wayland( version interface [: cr type space 0 .r ;] do-debug )
    interface wl-registry find-name-in ?dup-IF
	>r registry name version r> name>interpret catch
	?dup-IF [: cr ." Callback error: " h. ;] do-debug drop  THEN
    ELSE
	wayland( [: ."  unhandled" ;] do-debug )
    THEN ;
: registry- { data registry name -- } ;

<cb
' registry- ?cb wl_registry_listener-global_remove:
' registry+ ?cb wl_registry_listener-global:
cb> registry-listener

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

<cb
:noname { data xdg_surface serial -- }
    wayland( serial [: cr ." configured, serial " h. ;] do-debug )
    true to mapped
    xdg_surface serial xdg_surface_ack_configure
    wl-surface wl_surface_commit
; ?cb xdg_surface_listener-configure:
cb> xdg-surface-listener

: map-win ( -- )
    BEGIN  get-events mapped  UNTIL ;

Variable level#

2Variable wl-min-size &640 &400 wl-min-size 2!

<cb
:noname { data xdg_toplevel capabilities -- }
wayland( capabilities [: cr ." wm capabilities: " h. ;] do-debug ) ;
?cb xdg_toplevel_listener-wm_capabilities:
:noname { data xdg_toplevel width height -- }
    wayland( height width [: cr ." toplevel bounds: " . . ;] do-debug )
    xdg_toplevel wl-min-size 2@ xdg_toplevel_set_min_size
    xdg_toplevel width height xdg_toplevel_set_max_size ;
?cb xdg_toplevel_listener-configure_bounds:
:noname { data xdg_toplevel -- }
    wayland( [: cr ." close" ;] do-debug )
    -1 level# +! ;
?cb xdg_toplevel_listener-close:
:noname { data xdg_toplevel width height states -- }
    wayland( height width [: cr ." toplevel-config: " . . ;] do-debug )
    width height rescale-win ;
?cb xdg_toplevel_listener-configure:
cb> xdg-toplevel-listener

<cb
:noname { data decoration mode -- }
    wayland( [: cr ." decorated" ;] do-debug )
    true to configured clear sync ;
?cb zxdg_toplevel_decoration_v1_listener-configure:
cb> xdg-decoration-listener

:trigger-on( xdg-wm-base wl-surface )
    xdg-wm-base wl-surface xdg_wm_base_get_xdg_surface to xdg-surface
    xdg-surface xdg-surface-listener 0 xdg_surface_add_listener drop
    xdg-surface xdg_surface_get_toplevel to xdg-toplevel ;
:trigger-on( xdg-toplevel )
    xdg-toplevel xdg-toplevel-listener 0 xdg_toplevel_add_listener drop
    xdg-toplevel window-title$ $@ xdg_toplevel_set_title
    xdg-toplevel window-app-id$ $@ xdg_toplevel_set_app_id
    xdg-toplevel xdg_toplevel_set_maximized ;

: wl-eglwin { w h -- }
    wayland( h w [: cr ." eglwin: " . . ;] do-debug )
    xdg-surface ?dup-IF  0 0 w h xdg_surface_set_window_geometry  THEN
    wl-surface w h wl_egl_window_create to win
    wl-surface wl_surface_commit
    wayland( [: cr ." wl-eglwin done" ;] do-debug ) ;

:trigger-on( zxdg-decoration-manager-v1 xdg-toplevel )
    zxdg-decoration-manager-v1 xdg-toplevel
    zxdg_decoration_manager_v1_get_toplevel_decoration dup to zxdg-decoration
    dup xdg-decoration-listener 0
    zxdg_toplevel_decoration_v1_add_listener drop
    ZXDG_TOPLEVEL_DECORATION_V1_MODE_SERVER_SIDE
    zxdg_toplevel_decoration_v1_set_mode ;

also opengl
: getwh ( -- )
    0 0 dpy-wh 2@ glViewport ;
previous

\ looper

get-current also forth definitions

previous set-current

User xptimeout  cell uallot drop
#16 Value looper-to# \ 16ms, don't sleep too long
looper-to# #1000000 um* xptimeout 2!
7 Value xpollfd#
\ events, wayland, clip read, clip write, ps read, ps write, infile
User xpollfds
xpollfds pollfd xpollfd# * dup cell- uallot drop erase

: >poll-events ( delay -- n )
    0 xptimeout 2!  xpollfds >r
    epiper @ fileno POLLIN  r> fds!+ >r
    dpy ?dup-IF  wl_display_get_fd POLLIN  r> fds!+ >r  THEN
    clipin-fd ?dup-IF  fileno POLLIN POLLHUP or  r> fds!+ >r  THEN
    clipout-fd ?dup-IF  POLLOUT  r> fds!+ >r  THEN
    psin-fd ?dup-IF  fileno POLLIN POLLHUP or  r> fds!+ >r  THEN
    psout-fd ?dup-IF  POLLOUT  r> fds!+ >r  THEN
    infile-id fileno POLLIN  r> fds!+ >r
    r> xpollfds - pollfd / ;

: xpoll ( -- flag )
    [IFDEF] ppoll
	xptimeout dup @ 0< IF  drop 0  THEN  0 ppoll 0>
    [ELSE]
	xptimeout 2@ #1000 * swap #1000000 / + poll 0>
    [THEN] ;

Defer ?looper-timeouts ' noop is ?looper-timeouts

: #looper ( delay -- ) #1000000 *
    ?looper-timeouts >poll-events >r
    xpollfds r> xpoll
    IF
	xpollfds revents pollfd + >r
	dpy IF
	    r@ w@ POLLIN and IF  get-events  THEN
	    r> pollfd + >r
	THEN
	clipin-fd IF
	    wayland( r@ [: cr ." clipin: " w@ h. ;] do-debug )
	    r@ w@ POLLIN and IF  read-clipin  THEN
	    r@ w@ POLLHUP and IF  eof-clipin  THEN
	    r> pollfd + >r
	THEN
	clipout-fd IF
	    r@ w@ POLLOUT POLLHUP or and IF  write-clipout  THEN
	    r> pollfd + >r
	THEN
	psin-fd IF
	    wayland( r@ [: cr ." psin: " w@ h. ;] do-debug )
	    r@ w@ POLLIN and IF  read-psin  THEN
	    r@ w@ POLLHUP and IF  eof-psin  THEN
	    r> pollfd + >r
	THEN
	psout-fd IF
	    r@ w@ POLLOUT POLLHUP or and IF  write-psout  THEN
	    r> pollfd + >r
	THEN
	rdrop
    ELSE
	dpy IF  get-events  THEN
    THEN
    ?events ;

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

: gl-init ( -- ) \ minos2
    \G if not already opened, open window and initialize OpenGL
    ctx 0= IF  read-cursor-theme  window-init THEN ;

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

: clipboard! ( addr u -- ) clipboard$ $!
    true to my-clipboard
    data-device data-source 0 wl_data_device_set_selection
;
: clipboard@ ( -- addr u ) clipboard$ $@ ;

0 Value primary-serial#

: primary! ( addr u -- ) primary$ $!
    primary$ $@len 0<> to my-primary
    1 +to primary-serial#
    primary-selection-device
    primary-selection-source primary$ $@len 0<> and
    primary-serial# zwp_primary_selection_device_v1_set_selection ;
: primary@ ( -- addr u ) primary$ $@ ;
: dnd@ ( -- addr u ) dnd$ $@ ;

also OpenGL
