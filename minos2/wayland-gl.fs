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

debug: wayland( \ )

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
0 Value data-device-manager
0 Value data-device
0 Value data-source
0 Value primary-selection-device-manager
0 Value primary-selection-device
0 Value primary-selection-source

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

: <cb ( -- ) depth r> swap >r >r ;
: cb> ( xt1 .. xtn -- )
    Create depth r> r> swap >r - 0 ?DO , LOOP ;

${GFORTH_IGNLIB} "true" str= [IF]
    : ?cb ( xt -- 0 ) drop parse-name 2drop 0 ;
[ELSE]
    : ?cb ( xt "name" -- addr ) ;
[THEN]

<cb
' sh-surface-popup-done ?cb wl_shell_surface_listener-popup_done:
' sh-surface-config ?cb wl_shell_surface_listener-configure:
' sh-surface-ping ?cb wl_shell_surface_listener-ping:
cb> wl-sh-surface-listener

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
1 Value wl-scale
0 Value screen-orientation

: wl-out-geometry { data out x y pw ph subp d: make d: model transform -- }
    pw ph wl-metrics 2! transform to screen-orientation ;
: wl-out-mode { data out flags w h r -- }
    w h dpy-wh 2! ;
: wl-out-done { data out -- } ;
: wl-out-scale { data out scale -- }
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
    s set-cursor \ on enter, we set the cursor
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

: ?setstring
    setstring$ $@len IF  setstring$ $free  THEN ;

<cb
:noname { data wl_keyboard rate delay -- }
; ?cb wl_keyboard_listener-repeat_info:
:noname { data wl_keyboard serial mods_depressed mods_latched mods_locked group -- }
    mods_depressed 5 and mods_depressed 8 and sfloat/ or to wl-meta
    wayland( mods_depressed mods_latched mods_locked
    [: cr ." modes: locked " hex. ." latched " hex. ." depressed " hex. wl-meta hex. ;]
    do-debug )
    xkb-state
    mods_depressed mods_latched mods_locked 0 0 group xkb_state_update_mask
; ?cb wl_keyboard_listener-modifiers:
:noname { data wl_keyboard serial time wl-key state -- }
    wayland( state wl-key [: cr ." wayland key: " h. h. ;] do-debug )
    state WL_KEYBOARD_KEY_STATE_PRESSED = IF
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
    0 size PROT_READ MAP_PRIVATE fd 0 mmap { buf }
    \ wayland( buf size [: cr ." xkbd map:" cr type ;] do-debug )
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
    text-input cursor-xywh 4 cells bounds DO  I @  cell  +LOOP
    zwp_text_input_v3_set_cursor_rectangle
    text-input s" " 0 0
    zwp_text_input_v3_set_surrounding_text
    text-input zwp_text_input_v3_commit ;

<cb
:noname { data text-input serial -- }
    text-input send-status-update
; ?cb zwp_text_input_v3_listener-done:
:noname { data text-input before_length after_length -- }
; ?cb zwp_text_input_v3_listener-delete_surrounding_text:
:noname { data text-input d: text -- }
    wayland( text [: cr ." wayland keys: '" type ''' emit ;] do-debug )
    text save-mem
    [{: d: text :}h1 ?setstring
	text wayland-keys text drop free drop ;] master-task send-event
    text-input zwp_text_input_v3_commit
; ?cb zwp_text_input_v3_listener-commit_string:
:noname { data text-input d: text cursor_begin cursor_end -- }
    wayland( text [: cr ." preedit: '" type ''' emit ;] do-debug )
    text save-mem [{: d: text :}h1
	setstring$ $@ text str=
	text setstring$ $! 0= IF  "" wayland-keys  THEN
	text drop free throw ;]
    master-task send-event
; ?cb zwp_text_input_v3_listener-preedit_string:
:noname { data text-input surface -- }
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

0 Value clipin-dest$
$Variable clipin$

$Variable clipout$
Variable clipout-offset

$Variable clipin-xts
$Variable clipout-xts

: eof-clipin ( -- )
    clipin-fd 0 to clipin-fd close-file throw
    0 clipin$ !@ clipin-dest$ !@ ?dup-IF  free throw  THEN
    wayland( [: cr ." read " clipin-dest$ id. ." with '" clipin-dest$ $@ type ." '" ;] do-debug )
    clipin-xts stack# IF  clipin-xts stack> execute  THEN ;

: read-clipin ( -- )
    clipin-fd check_read dup 0> IF \ data available
	dup clipin$ $+!len swap dup >r clipin-fd
	read-file throw
	    r> - dup 0< IF  clipin$ $+!len  THEN  drop
    ELSE
	drop eof-clipin
    THEN ;

[IFUNDEF] FIONBIO
    0x5421 Constant FIONBIO \ works for Linux, which is good enough for Wayland
[THEN]

: set-noblock ( fd -- )
    { | w^ arg }  1 arg l!
    dup FIONBIO arg ioctl ?ior ;

: write-clipout ( -- )
    clipout$ $@ clipout-offset @ safe/string
    clipout-fd -rot write dup -1 <> IF  clipout-offset +!
	clipout$ $@len clipout-offset @ u> ?EXIT
    ELSE
	drop
	-512 errno - [: cr ." Error writing clipboard pipe: " error$ type ;] do-debug
    THEN \ if we can't write, let's just abandon this operation
    wayland( [: cr ." wrote '" clipout$ $. ." ' to clipout" ;] do-debug )
    clipout-fd 0 to clipout-fd close -1 = IF
	-512 errno - [: cr ." Error closing clipboard pipe: " error$ type ;] do-debug
    THEN
    clipout$ $free
    clipout-xts stack# IF  clipout-xts stack> execute  THEN ;

: queue-clipin ( xt -- )
    clipin-fd IF  clipin-xts >stack  ELSE  master-task send-event  THEN ;
: queue-clipout ( xt -- )
    clipout-fd IF  clipout-xts >stack  ELSE  master-task send-event  THEN ;

: accept+receive { offer d: mime-type dest$ | fds[ 2 cells ] -- }
    offer current-serial mime-type wl_data_offer_accept
    fds[ create_pipe
    offer mime-type fds[ cell+ @ fileno wl_data_offer_receive
    fds[ cell+ @ close-file throw
    fds[ @ dest$ [{: fd dest$ :}h1
	fd to clipin-fd dest$ to clipin-dest$ ;]
    queue-clipin ;

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
    mime-types[] $[]free
    id data-offer-listener 0 wl_data_offer_add_listener drop
; ?cb wl_data_device_listener-data_offer:
cb> data-device-listener

\ primary selection offer listener

: ps-accept+receive { offer d: mime-type dest$ | fds[ 2 cells ] -- }
    fds[ create_pipe
    offer mime-type fds[ cell+ @ fileno zwp_primary_selection_offer_v1_receive
    fds[ cell+ @ close-file throw
    fds[ @ dest$ [{: fd dest$ :}h1
	fd to clipin-fd dest$ to clipin-dest$ ;]
    queue-clipin ;

<cb
:noname { data offer d: mime-type -- }
    wayland( mime-type [: cr ." primary mime-type: " type ;] do-debug )
    mime-type mime-types[] $+[]!
; ?cb zwp_primary_selection_offer_v1_listener-offer:
cb> primary-selection-offer-listener

\ primary selection device listener

<cb
:noname { data data-device id -- }
    wayland( id [: cr ." primary selection id: " h. ;] do-debug )
    my-primary 0= IF
	id  [{: id :}l id -rot primary$ ps-accept+receive ;] >liked-mime
    THEN
; ?cb zwp_primary_selection_device_v1_listener-selection:
:noname { data data-device id -- }
    wayland( id [: cr ." primary offer: " h. ;] do-debug )
    mime-types[] $[]free
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
    data fd [{: data fd :}h1
	fd to clipout-fd  data $@ clipout$ $!  clipout-offset off
	write-clipout ;] queue-clipout
; ?cb wl_data_source_listener-send:
:noname { data source d: mime-type -- }
    wayland( data mime-type [: cr ." ds target: " type space id. ;] do-debug )
; ?cb wl_data_source_listener-target:
cb> data-source-listener

\ primary selection source listener

<cb
:noname { data source -- }
; ?cb zwp_primary_selection_source_v1_listener-cancelled:
:noname { data source d: mime-type fd -- }
    wayland( fd mime-type data [: cr ." ps send " id. ." type: " type ."  fd: " h. ;] do-debug )
    data fd [{: data fd :}h1
	fd to clipout-fd  data $@ clipout$ $!  clipout-offset off
	write-clipout ;] queue-clipout
; ?cb zwp_primary_selection_source_v1_listener-send:
cb> primary-selection-source-listener

\ registry listeners: the interface string is searched in a table

$Variable cursor-theme$ "Breeze_Snow" cursor-theme$ $!
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
	REPEAT ;] execute-parsing-file ;

: read-gnome-cursor-theme ( -- )
    "~/.config/gtk-4.0/settings.ini" r/o open-file IF  drop  EXIT  THEN
    [: BEGIN  refill  WHILE
		source "gtk-cursor-theme-name=" string-prefix? IF
		    source #22 safe/string cursor-theme$ $!
		THEN
		source "gtk-cursor-theme-size=" string-prefix? IF
		    source #22 safe/string s>number drop $10 max cursor-size !
		THEN
	REPEAT ;] execute-parsing-file ;

: read-cursor-theme ( -- )
    ${XDG_CURRENT_DESKTOP} "KDE" str= IF  read-kde-cursor-theme  EXIT  THEN
    ${XDG_CURRENT_DESKTOP} "GNOME" str= IF  read-gnome-cursor-theme  EXIT  THEN ;

read-cursor-theme

table Constant wl-registry

get-current

wl-registry set-current

: wl_compositor ( registry name version -- )
    wl_compositor_interface swap 5 umin wl_registry_bind dup to compositor
    dup wl_compositor_create_surface to wl-surface
    wl_compositor_create_surface to cursor-surface ;
: wl_shell ( registry name version -- )
    wl_shell_interface swap 1 umin wl_registry_bind dup to wl-shell
    wl-surface wl_shell_get_shell_surface to sh-surface
    sh-surface wl-sh-surface-listener 0 wl_shell_surface_add_listener drop
    sh-surface wl_shell_surface_set_toplevel ;
: wl_output ( registry name version -- )
    wl_output_interface swap 4 umin wl_registry_bind dup to wl-output
    wl-output-listener 0 wl_output_add_listener drop ;
: wl_seat ( registry name version -- )
    wl_seat_interface swap 8 umin wl_registry_bind dup to wl-seat
    wl-seat-listener 0 wl_seat_add_listener drop ;
: wl_shm ( registry name version -- )
    wl_shm_interface swap 1 umin wl_registry_bind to wl-shm
    cursor-theme$ $@ cursor-size @
    wl-shm wl_cursor_theme_load dup to cursor-theme
    s" left_ptr" wl_cursor_theme_get_cursor to cursor ;
: zwp_text_input_manager_v3 ( registry name version -- )
    zwp_text_input_manager_v3_interface swap 1 umin wl_registry_bind
    dup to text-input-manager
    wl-seat zwp_text_input_manager_v3_get_text_input dup to text-input
    text-input-listener 0 zwp_text_input_v3_add_listener drop
    text-input send-status-update ;
: xdg_wm_base ( registry name version -- )
    xdg_wm_base_interface swap 4 umin wl_registry_bind dup to xdg-wm-base
    xdg-wm-base-listener 0 xdg_wm_base_add_listener drop ;
: zxdg_decoration_manager_v1 ( registry name version -- )
    zxdg_decoration_manager_v1_interface swap 1 umin wl_registry_bind
    to decoration-manager ;
: wl_data_device_manager ( registry name version -- )
    wl_data_device_manager_interface swap 3 umin wl_registry_bind
    dup to data-device-manager
    wl-seat wl_data_device_manager_get_data_device dup to data-device
    data-device-listener 0 wl_data_device_add_listener drop
    data-device-manager wl_data_device_manager_create_data_source
    dup to data-source
    data-source-listener clipboard$ wl_data_source_add_listener drop
    ds-mime-types[] [: data-source -rot wl_data_source_offer ;] $[]map
;
: zwp_primary_selection_device_manager_v1 ( registry name version -- )
    zwp_primary_selection_device_manager_v1_interface swap 1 umin wl_registry_bind
    dup to primary-selection-device-manager
    wl-seat zwp_primary_selection_device_manager_v1_get_device dup to primary-selection-device
    primary-selection-listener 0 zwp_primary_selection_device_v1_add_listener drop
    primary-selection-device-manager zwp_primary_selection_device_manager_v1_create_source
    dup to primary-selection-source
    primary-selection-source-listener primary$ zwp_primary_selection_source_v1_add_listener drop
    ds-mime-types[] [: primary-selection-source -rot zwp_primary_selection_source_v1_offer ;] $[]map
;
set-current

: registry+ { data registry name d: interface version -- }
    \ sp@ sp0 ! rp@ cell+ rp0 !
    wayland( version interface [: cr type space 0 .r ;] do-debug )
    interface wl-registry find-name-in ?dup-IF
	>r registry name version r> name>interpret execute
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
    wayland( [: cr ." configured" ;] do-debug )
    true to mapped
    xdg_surface serial xdg_surface_ack_configure
    wl-surface wl_surface_commit
; ?cb xdg_surface_listener-configure:
cb> xdg-surface-listener

: map-win ( -- )
    BEGIN  get-events mapped  UNTIL ;

Variable level#

require need-x.fs

Defer config-changed
:noname ( -- ) +sync +config ( getwh ) ; is config-changed
Defer screen-ops      ' noop IS screen-ops
Defer reload-textures ' noop is reload-textures

: resize-widgets ( w h -- )
    dpy-wh 2!  config-changed ;

<cb
:noname { data xdg_toplevel capabilities -- } ;
?cb xdg_toplevel_listener-wm_capabilities:
:noname { data xdg_toplevel width height -- }
    wayland( height width [: cr ." toplevel bounds: " . . ;] do-debug )
    xdg_toplevel &640 &400 xdg_toplevel_set_min_size
    xdg_toplevel width height xdg_toplevel_set_max_size ;
?cb xdg_toplevel_listener-configure_bounds:
:noname { data xdg_toplevel -- }
    wayland( [: cr ." close" ;] do-debug )
    -1 level# +! ;
?cb xdg_toplevel_listener-close:
also opengl
:noname { data xdg_toplevel width height states -- }
    height width d0= ?EXIT
    wayland( height width [: cr ." toplevel-config: " . . ;] do-debug )
    xdg-surface 0 0 width height xdg_surface_set_window_geometry
    win width height 0 0 wl_egl_window_resize
    wl-surface wl_surface_commit
    width height resize-widgets ;
previous
?cb xdg_toplevel_listener-configure:
cb> xdg-toplevel-listener

<cb
:noname { data decoration mode -- }
    wayland( [: cr ." decorated" ;] do-debug )
    true to configured clear sync ;
?cb zxdg_toplevel_decoration_v1_listener-configure:
cb> xdg-decoration-listener

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
5 Value xpollfd#
\ events, wayland, infile, selection read, selection write
User xpollfds
xpollfds pollfd xpollfd# * dup cell- uallot drop erase

: >poll-events ( delay -- n )
    0 xptimeout 2!
    epiper @ fileno POLLIN  xpollfds fds!+ >r
    dpy ?dup-IF  wl_display_get_fd POLLIN  r> fds!+ >r  THEN
    clipin-fd ?dup-IF  fileno POLLIN  r> fds!+ >r  THEN
    clipout-fd ?dup-IF  POLLOUT  r> fds!+ >r  THEN
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
	xpollfds revents >r
	r@ w@ POLLIN and event? or  IF  ?events  THEN
	r> pollfd + >r
	dpy IF
	    r@ w@ POLLIN and IF  get-events  THEN
	    r> pollfd + >r
	THEN
	clipin-fd IF
	    r@ w@ POLLIN and IF  read-clipin  THEN
	    r@ w@ POLLHUP and IF  eof-clipin  THEN
	    r> pollfd + >r
	THEN
	clipout-fd IF
	    r@ w@ POLLOUT POLLHUP or and IF  write-clipout  THEN
	    r> pollfd + >r
	THEN
	rdrop
    ELSE
	?events
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

: clipboard! ( addr u -- ) clipboard$ $!
    true to my-clipboard
    data-device data-source 0 wl_data_device_set_selection
;
: clipboard@ ( -- addr u ) clipboard$ $@ ;
: primary! ( addr u -- ) primary$ $!
    true to my-primary
    primary-selection-device primary-selection-source 0 zwp_primary_selection_device_v1_set_selection
;
: primary@ ( -- addr u ) primary$ $@ ;
: dnd@ ( -- addr u ) dnd$ $@ ;

also OpenGL
