\ Wayland window for GLES

\ Author: Bernd Paysan
\ Copyright (C) 2017,2019,2023,2024,2025 Free Software Foundation, Inc.

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

debug: wayland(
debug: serial(

$Variable window-title$ s" ΜΙΝΟΣ2 OpenGL Window" window-title$ $!
$Variable window-app-id$ s" ΜΙΝΟΣ2" window-app-id$ $!

0 Value dpy        \ wayland display
0 ' noop trigger-Value wl-compositor \ wayland compositor
0 ' noop trigger-Value wl-subcompositor
0 ' noop trigger-Value wl-output
0 ' noop trigger-Value xdg-activation-v1
0 ' noop trigger-Value zxdg-output-manager-v1
0 Value zxdg-output-v1
[IFDEF] zwp_idle_inhibit_manager_v1_get_version
    0 ' noop trigger-Value zwp-idle-inhibit-manager-v1
[THEN]
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
1 ' noop trigger-Value cursor-type
0 ' noop trigger-Value wp-cursor-shape-manager-v1
0 ' noop trigger-Value wp-cursor-shape-device-v1
0 ' noop trigger-Value last-serial
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
0 ' noop trigger-Value wp-tearing-control-manager-v1
0 ' noop trigger-Value wp-tearing-control-v1

\ set a cursor

:trigger-on( cursor-serial cursor-surface cursor wl-pointer )
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

$Variable cb-prefix
: >cb-class ( "name" -- addr u )
    parse-name [: cb-prefix $. ." _listener-" type ':' emit ;] $tmp ;
: >cb-offset ( "name" -- addr u )
    parse-name [: cb-prefix $. ." _listener-" type ;] $tmp ;
: (cb') ( "name" -- xt )
    >cb-class rec-forth '-error ;
: (cb#) ( "name" -- n )
    >cb-offset rec-forth '-error ;

: _2- ( addr u -- addr' u' )
    [: bounds
	?DO  I c@ '-' over '_' <> select emit
	LOOP ;] $tmp ;
: <cb ( name -- )
    parse-name cb-prefix $!
    cb-prefix $@ _2- [: type ." -listener" ;] $tmp nextname Create
    cb-prefix $@ [: type ." _listener" ;] $tmp evaluate allot ;
: cb> ( -- ) ;

${GFORTH_IGNLIB} "true" str= [IF]
    : ?cb ( xt -- 0 ) drop >cb-class 2drop ;
    : :cb ( "name" -- colon-sys )
	:noname colon-sys-xt-offset n>r drop
	record-name (cb') drop ['] drop nr> drop ;
[ELSE]
    : cb! ( xt callback offset -- )
	>r execute latest-name r> execute ! ;
    : cb-pair ( "name" -- callback offset )
	record-name >in @ >r (cb') r> >in ! (cb#) ;
    : ?cb ( xt "name" -- ) cb-pair cb! ;
    : :cb ( "name" -- colon-sys )
	:noname colon-sys-xt-offset n>r drop cb-pair ['] cb! nr> drop ;
[THEN]

<cb wl_shell_surface
:cb ping ( data surface serial -- )
    serial( dup [: cr ." ping serial: " h. ;] do-debug )
    dup to last-serial
    wl_shell_surface_pong drop ;
:cb configure { data surface edges w h -- }
    win w h 0 0 wl_egl_window_resize ;
:cb popup_done { data surface -- } ;
cb>

<cb wl_callback
:cb done { data callback done -- } ;
cb>

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
2Variable dpy-xy
2Variable dpy-raw-wh
2Variable dpy-unscaled-wh
1 Value wl-scale
#120 Value fractional-scale
0 Value screen-orientation
0 Value registered
60e FValue dpy-rate

1e 256e f/ fconstant 1/256
: scale* ( n1 -- n2 )
    fractional-scale #60 */ 1+ 2/ ;
: scale/ ( n1 -- n2 )
    #240 fractional-scale */ 1+ 2/ ;
: f>coordi ( r -- n )
    #120 fractional-scale fm*/ fround f>s ;
: scale*fixed ( n1 -- n2 )
    fractional-scale 8 lshift #60 */ 1+ 2/ ;
: coord>f ( fixed -- r )
    1/256 fm* fractional-scale #120 fm*/ ;
: n>coord ( n -- r )
    scale*fixed 1/256 fm* ;

User xptimeout  cell uallot drop
#4 Value looper-to# \ 4ms, don't sleep too long
: re-timeout ( -- )
    looper-to# #1000000 um* xptimeout 2! ;
re-timeout

<cb wl_output
:cb description { data out d: description -- }
    wayland( description [: cr ." output description: " type ;] do-debug ) ;
:cb name { data out d: name -- }
    wayland( name [: cr ." output name: " type ;] do-debug ) ;
:cb scale { data out scale -- }
    wayland( scale [: cr ." scale: " . ;] do-debug )
    scale to wl-scale ;
:cb done { data out -- } ;
:cb mode { data out flags w h r -- }
    wayland( r 1m fm* h w flags [: cr ." mode: flags" h. ." w=" . ." h=" . ." rate=" f. ;] do-debug )
    flags WL_OUTPUT_MODE_CURRENT and IF
	w h dpy-wh 2! r 1m fm* to dpy-rate true to registered
	dpy-rate 1/f #1000 fm* f>s to looper-to# re-timeout
    THEN ;
:cb geometry { data out x y pw ph subp d: make d: model transform -- }
    wayland( pw ph [: cr ." metrics: " . . ;] do-debug )
    pw ph wl-metrics 2! transform to screen-orientation ;
cb>

[IFDEF] zxdg_output_v1_listener
    <cb zxdg_output_v1
    :cb description { data out d: description -- }
	wayland( description [: cr ." xdg description: " type ;] do-debug ) ;
    :cb name { data out d: name -- }
	wayland( name [: cr ." xdg name: " type ;] do-debug ) ;
    :cb done { data out -- } ;
    :cb logical_size { data out w h -- }
	wayland( h w [: cr ." xdg size: " . . ;] do-debug )
	w h dpy-raw-wh 2! ;
    :cb logical_position { data out x y -- }
	wayland( y x [: cr ." xdg position: " . . ;] do-debug )
	x y dpy-xy 2! ;
    cb>
[THEN]

<cb xdg_activation_token_v1
:cb done { data token d: name -- }
    wayland( name [: cr ." activation token: " type ;] do-debug ) ;
cb>

require need-x.fs

Defer config-changed
:is config-changed ( -- ) +sync +config ( getwh ) ;
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
    <cb wp_fractional_scale_v1
    :cb preferred_scale { data fscale scale -- }
	wayland( scale [: cr ." fractional scale: " . ;] do-debug )
	scale to fractional-scale  rescaler
	dpy-unscaled-wh 2@ rescale-win
    ;
    cb>
[THEN]

[IFDEF] wp_color_manager_v1_listener
    0 ' noop trigger-Value wp-color-manager-v1
    0 ' noop trigger-Value wp-color-management-output-v1
    0 ' noop trigger-Value wp-color-management-surface-v1
    0 ' noop trigger-Value wp-color-management-surface-feedback-v1

    0 Value color-features
    0 Value color-tfs
    0 Value color-primaries
    0 Value color-intents
    <cb wp_color_manager_v1
    :cb done { data xdg-activation-token d: token -- }
	wayland( token [: cr ." done " type ;] do-debug ) ;
    :cb supported_feature { data manager feature -- }
	wayland( feature [: cr ." color feature " h. ;] do-debug )
	1 feature lshift color-features or to color-features ;
    :cb supported_tf_named { data manager tf -- }
	wayland( tf [: cr ." color tf " h. ;] do-debug )
	1 tf lshift color-tfs or to color-tfs ;
    :cb supported_primaries_named { data manager primaries -- }
	wayland( primaries [: cr ." color primaries " h. ;] do-debug )
	1 primaries lshift color-primaries or to color-primaries ;
    :cb supported_intent { data manager intent -- }
	wayland( intent [: cr ." color intent " h. ;] do-debug )
	1 intent lshift color-intents or to color-intents ;
    cb>

    0 Value di-r_x
    0 Value di-g_x
    0 Value di-b_x
    0 Value di-w_x
    0 Value di-r_y
    0 Value di-g_y
    0 Value di-b_y
    0 Value di-w_y

    0 Value di-min-lum
    0 Value di-max-lum
    0 Value di-ref-lum
    
    0 Value dit-r_x
    0 Value dit-g_x
    0 Value dit-b_x
    0 Value dit-w_x
    0 Value dit-r_y
    0 Value dit-g_y
    0 Value dit-b_y
    0 Value dit-w_y
    
    0 Value dit-min-lum
    0 Value dit-max-lum
    0 Value dit-max-cll
    0 Value dit-max-fall
    <cb wp_image_description_info_v1
    :cb target_max_fall { data desc-info max-fall -- }
	wayland( max-fall [: cr ." target max-fall: " h. ;] do-debug )
	max-fall to dit-max-fall ;
    :cb target_max_cll { data desc-info max-cll -- }
	wayland( max-cll [: cr ." target max-cll: " h. ;] do-debug )
	max-cll to dit-max-cll ;
    :cb target_luminance { data desc-info min-lum max-lum -- }
	wayland( max-lum min-lum [: cr ." target luminance: " h. 1 backspaces ." .. " h. ;] do-debug )
	max-lum to dit-max-lum
	min-lum to dit-min-lum ;
    :cb target_primaries { data desc-info r_x r_y g_x g_y b_x b_y w_x w_y -- }
	wayland( w_y w_x b_y b_x g_y g_x r_y r_x
	[: cr ." target primaries: " . . . . . . . . ;] do-debug )
	r_x to dit-r_x r_y to dit-r_y
	g_x to dit-g_x g_y to dit-g_y
	b_x to dit-b_x b_y to dit-b_y
	w_x to dit-w_x w_y to dit-w_y ;
    :cb luminances { data desc-info min-lum max-lum ref-lum -- }
	wayland( ref-lum max-lum min-lum [: cr ." luminance: " h. 1 backspaces ." .. " h. ." ref: " h. ;] do-debug )
	max-lum to di-max-lum
	min-lum to di-min-lum
	ref-lum to di-ref-lum ;
    :cb tf_named { data desc-info tf -- }
	wayland( tf [: cr ." tf_named: " h. ;] do-debug ) ;
    :cb tf_power { data desc-info eexp -- }
	wayland( eexp [: cr ." tf_power: " h. ;] do-debug ) ;
    :cb primaries_named { data desc-info primaries -- }
	wayland( primaries [: cr ." primaries named: " h. ;] do-debug ) ;
    :cb primaries { data desc-info r_x r_y g_x g_y b_x b_y w_x w_y -- }
	wayland( w_y w_x b_y b_x g_y g_x r_y r_x
	[: cr ." primaries: " . . . . . . . . ;] do-debug )
	r_x to di-r_x r_y to di-r_y
	g_x to di-g_x g_y to di-g_y
	b_x to di-b_x b_y to di-b_y
	w_x to di-w_x w_y to di-w_y ;
    :cb icc_file { data desc-info icc size -- }
	wayland( icc size [: cr ." icc file size: " . ."  fd: " h. ;] do-debug ) ;
    :cb done { data desc-info -- }
	wayland( [: cr ." description done" ;] do-debug ) ;
    cb>
    
    0 Value image-description-id
    0 Value image-description-info
    <cb wp_image_description_v1
    :cb ready { data img-desc id -- }
	wayland( id [: cr ." Image description ready " h. ;] do-debug )
	id to image-description-id
	img-desc wp_image_description_v1_get_information
	dup to image-description-info
	wp_image_description_info_v1_listener 0
	wp_image_description_info_v1_add_listener drop ;
    :cb failed { data img-desc cause d: string -- }
	wayland( cause string [: cr ." Image description failed for reason:"
	cr type cr ." cause: " h. ;] do-debug ) ;
    cb>
    
    0 Value image-description
    <cb wp_color_management_output_v1
    :cb image_description_changed { data output-manager -- }
	image-description ?dup-IF  wp_image_description_v1_destroy  THEN
	output-manager wp_color_management_output_v1_get_image_description
	dup to image-description
	wp-image-description-v1-listener 0
	wp_image_description_v1_add_listener drop ;
    cb>

    0 Value preferred-image-id
    <cb wp_color_management_surface_feedback_v1
    :cb preferred_changed { data feddback id -- }
	to preferred-image-id ;
    cb>
[THEN]

\ As events come in callbacks, push them to an event queue

?: 3drop 2drop drop ;

\ for simplified events, the app_input_state is used

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

Defer b-scroll ' 3drop is b-scroll
Defer b-button
:is b-button ( time b mask -- )
    *input eventtime 2@ *input eventtime' 2!
    1 and dup *input pressure ! dup 1 xor *input action !
    IF    drop s>d *input eventtime 2@ d- *input downtime 2!
    ELSE  drop s>d *input eventtime 2! #0. *input downtime 2!  THEN ;
Defer b-motion
:is b-motion ( time x y -- )
    *input pressure @ IF
	8 rshift *input y0 !  8 rshift *input x0 !
	2 *input action !
	s>d *input eventtime 2@ d- *input downtime 2!
    ELSE  2drop drop  THEN ;
Defer b-enter
:is b-enter  ( x y -- )  8 rshift *input y0 !  8 rshift *input x0 ! ;
Defer b-leave  ' noop  is b-leave

up@ Value master-task

\ pointer listener

Variable wl-time

<cb wl_pointer
:cb axis_relative_direction { data p axis disc -- } ;
:cb axis_value120 { data p axis val -- }
    XTime axis val
    [{: time axis val :}h1 time axis val b-scroll ;] master-task send-event ;
:cb axis_discrete { data p axis disc -- } ;
:cb axis_stop { data p time axis -- } time XTime! ;
:cb axis_source { data p source -- } ;
:cb frame { data p -- } ;
:cb axis { data p time axis val -- } time XTime! ;
:cb button { data p serial time b mask -- }  time XTime!
    serial( serial [: cr ." button serial: " h. ;] do-debug )
    serial to last-serial
    time b mask [{: time b mask :}h1 time b mask b-button ;] master-task send-event ;
:cb motion { data p time x y -- }  time XTime!
    time x y [{: time x y :}h1 time x y b-motion ;] master-task send-event ;
:cb leave { data p s surface -- }
    ['] b-leave master-task send-event ; 
:cb enter { data p s surface x y -- }
    serial( s [: cr ." cursor-serial: " . ;] do-debug )
    s to cursor-serial \ on enter, we set the cursor
    x y [{: x y :}h1 x y b-enter ;] master-task send-event ;
cb>

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
k-home	XKB_KEY_KP_Home >xkb-key !
k-end	XKB_KEY_KP_End >xkb-key !
k-left	XKB_KEY_KP_Left >xkb-key !
k-up	XKB_KEY_KP_Up >xkb-key !
k-right	XKB_KEY_KP_Right >xkb-key !
k-down	XKB_KEY_KP_Down >xkb-key !
k-insert	XKB_KEY_KP_Insert >xkb-key !
k-delete	XKB_KEY_KP_Delete >xkb-key !
k-prior	XKB_KEY_KP_Prior >xkb-key !
k-next	XKB_KEY_KP_Next >xkb-key !
k-enter	XKB_KEY_KP_Enter >xkb-key !
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

: search-ekey ( key nt -- xt key flag )
    2dup @ = IF  rot drop swap false  ELSE  drop true  THEN ;
Defer wl-ekeyed
:is wl-ekeyed
    >r 0 r@ ['] search-ekey esc-sequences traverse-wordlist
    drop ?dup-IF  #esc inskey name>string inskeys
    ELSE
	0 r@ $8FFFFFFF and ['] search-ekey esc-sequences traverse-wordlist
	drop ?dup-IF
	    #esc inskey name>string over c@ inskey 1 safe/string
	    r@ mask-shift# rshift 7 and s" 1;" inskeys '0' + inskey inskeys
	THEN
    THEN  rdrop ;
Defer wl-ukeyed ' inskeys is wl-ukeyed

0 Value wl-meta

Variable prev-preedit$

: ?setstring
    setstring$ $@len IF  setstring$ $free  THEN ;

<cb wl_keyboard
:cb repeat_info { data wl_keyboard rate delay -- } ;
:cb modifiers { data wl_keyboard serial mods_depressed mods_latched mods_locked group -- }
    serial( serial [: cr ." kb modifiers serial: " h. ;] do-debug )
    mods_depressed 5 and mods_depressed 8 and sfloat/ or to wl-meta
    wayland( mods_depressed mods_latched mods_locked
    [: cr ." modes: locked " h. ." latched " h. ." depressed " h. wl-meta h. ;]
    do-debug )
    xkb-state
    mods_depressed mods_latched mods_locked 0 0 group xkb_state_update_mask ;
:cb key { data wl_keyboard serial time wl-key state -- }
    wayland( state wl-key [: cr ." wayland key: " h. h. ;] do-debug )
    serial( serial [: cr ." kb key serial: " h. ;] do-debug )
    serial to last-serial
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
    THEN ;
:cb leave { data wl_keyboard serial surface -- }
    serial( serial [: cr ." kb leave serial: " h. ;] do-debug )
    serial to last-serial ;
:cb enter	{ data wl_keyboard serial surface keys -- }
    serial( serial [: cr ." kb enter serial: " h. ;] do-debug )
    serial to last-serial ;
:cb keymap { data wl_keyboard format fd size -- }
    \ sp@ sp0 !
    wayland( fd size [: cr ." xkbd mmap file: " swap . h. ;] do-debug )
    0 size PROT_READ MAP_PRIVATE fd 0 mmap { buf }
    wayland( buf size [: cr ." xkbd map: " swap h. h. ;] do-debug )
    XKB_CONTEXT_NO_FLAGS xkb_context_new dup to xkb-ctx
    buf size 1- XKB_KEYMAP_FORMAT_TEXT_V1 XKB_KEYMAP_COMPILE_NO_FLAGS
    xkb_keymap_new_from_buffer to keymap
    buf size munmap ?ior
    keymap xkb_state_new to xkb-state ;
previous
cb>

\ seat listener

<cb wl_seat
:cb name { data seat d: name -- }
    wayland( name [: cr ." seat: " type ;] do-debug ) ;
:cb capabilities { data seat caps -- }
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
    THEN ;
cb>

\ xdg-wm-base-listener

<cb xdg_wm_base
:cb ping ( data xdg_wm_base serial -- )
    serial( dup [: cr ." pong serial: " h. ;] do-debug )
    dup to last-serial
    xdg_wm_base_pong drop ;
cb>

\ input listener

Defer wayland-keys

' inskeys is wayland-keys

Create old-cursor-xywh #-4200 , #3800 , #-5 , #-100 ,
Create cursor-xywh #200 , #300 , #1 , #10 ,
0e+0ei ZValue xy-offset

: point>coord ( rx ry -- x y )
    f>coordi f>coordi swap ;
: +offset ( x y -- )  +to xy-offset ;
: 0offset ( -- )
    0e fdup to xy-offset ;

: send-status-update { text-input -- }
    [IFDEF] zwp_text_input_v3_add_listener
	text-input
	ZWP_TEXT_INPUT_V3_CONTENT_HINT_NONE
	ZWP_TEXT_INPUT_V3_CONTENT_PURPOSE_NORMAL
	zwp_text_input_v3_set_content_type
	cursor-xywh 4 cells old-cursor-xywh over str= 0= IF
	    text-input
	    cursor-xywh 2@  cursor-xywh 2 th 2@
	    zwp_text_input_v3_set_cursor_rectangle
	    cursor-xywh old-cursor-xywh 4 cells move
	THEN
	text-input s" " 0 0
	zwp_text_input_v3_set_surrounding_text
	text-input zwp_text_input_v3_commit
    [THEN] ;

: >cursor-xyxy { f: x0 f: y0 f: x1 f: y1 -- }
    wayland( y1 x1 y0 x0 [: cr ." >cursor-xyxy " f. f. f. f. ." offset " xy-offset z. ;] do-debug )
    x0 y1 xy-offset z+ point>coord cursor-xywh 2!
    x1 y0  x0 y1 z- point>coord cursor-xywh 2 th 2!
    text-input ?dup-IF  send-status-update  THEN ;

Defer sync+config ' noop is sync+config

[IFDEF] zwp_text_input_v3_add_listener
<cb zwp_text_input_v3
:cb done { data text-input serial -- }
    wayland( serial [: cr ." input done: " . ;] do-debug ) ;
:cb delete_surrounding_text { data text-input before after -- }
    wayland( after before [: cr ." delete surrounding: " . . ;] do-debug ) ;
:cb commit_string { data text-input d: text -- }
    wayland( text [: cr ." wayland keys: '" type ''' emit ;] do-debug )
    prev-preedit$ $free  text save-mem
    [{: d: text :}h1 ?setstring
	text wayland-keys text drop free drop ;] master-task send-event ;
:cb preedit_string { data text-input d: text cursor_begin cursor_end -- }
    text prev-preedit$ $@ str= 0= IF
	text prev-preedit$ $!
	wayland( text [: cr ." preedit: '" type ''' emit ;] do-debug )
	text save-mem [{: d: text :}h1
	    text setstring$ $! "\x0C" wayland-keys
	    text drop free throw ;] master-task send-event
    THEN ;
:cb leave { data text-input surface -- }
    text-input zwp_text_input_v3_disable
    text-input zwp_text_input_v3_commit ;
:cb enter { data text-input surface -- }
    text-input zwp_text_input_v3_enable
    text-input send-status-update ;
cb>
[THEN]

\ data offer listener

[IFUNDEF] FIONBIO
    0x5421 Constant FIONBIO \ works for Linux, which is good enough for Wayland
[THEN]

: set-noblock ( fd -- )
    { | w^ arg }  1 arg l!
    dup FIONBIO arg ioctl ?ior ;

0 Value current-serial
$[]Variable mime-types[]
$[]Variable ds-mime-types[]
$[]Variable liked-mime[]

$Variable clipboard$
$Variable dnd$
$Variable primary$

false Value my-clipboard
false Value my-primary
false Value my-dnd

"UTF8_STRING"              liked-mime[] $+[]!
"text/plain;charset=utf-8" liked-mime[] $+[]!
"text/uri-list"            liked-mime[] $+[]!

"UTF8_STRING"              ds-mime-types[] $+[]!
"text/plain;charset=utf-8" ds-mime-types[] $+[]!

: ?mime-type ( addr u -- flag )
    false -rot
    mime-types[] [: 2over str= IF  rot drop true -rot  THEN ;] $[]map
    2drop ;

object class
    method ?inout
    method +inout
end-class inout-r/w

inout-r/w class
    field: in$
    field: in<<
    value: public$
    value: in-fd
    defer: my-in
    method read-in
    method eof-in
    method set-in
end-class in-reader

: in-reader: ( xt $var "name" -- )
    in-reader ['] new static-a with-allocater Constant
    latestxt execute >o to public$  is my-in  in$ $saved o> ;

' my-clipboard  clipboard$ in-reader: clipin$
' my-dnd        dnd$       in-reader: dndin$
' my-primary    primary$   in-reader: psin$

inout-r/w class
    field: out$
    field: out<<
    value: out-fd
    field: out-offset
    value: out-name
    method write-out
    method eof-out
    method set-out
end-class out-writer

: out-writer: ( "name" -- )
    out-writer ['] new static-a with-allocater Constant
    latestxt execute >o out$ $saved latestxt to out-name o> ;

out-writer: clipout$
out-writer: dndout$
out-writer: psout$

20 Constant maxiter# \ wait 20ms at most

: $free0 ( addr -- )
    dup $@len 0= IF  $free  ELSE  drop  THEN ;

in-reader :method set-in ( fd -- )
    in-fd IF  in<< >back  ELSE  to in-fd  THEN ;
in-reader :method eof-in ( -- )
    in-fd 0 to in-fd close-file throw
    0 in$ !@
    my-in 0= IF
	public$ !@ ?dup-IF  free throw  THEN
    ELSE  ?dup-IF  free throw  THEN  THEN
    in<< $@len IF
	in<< stack> to in-fd
	in<< $free0
    THEN
    wayland( [: cr ." read " public$ id. ." with '" public$ $@ type ." '" ;] do-debug ) ;
in-reader :method read-in { flagaddr -- }
    in-fd check_read dup 0> IF \ data available
	wayland( [: cr dup . ." bytes available in " public$ id. ;] do-debug )
	dup in$ $+!len swap dup >r in-fd read-file throw
	r> - dup 0< IF  in$ $+!len  THEN  drop
    ELSE
	?dup-IF
	    -512 + [: cr ." Error checking pipe: " error$ type ;] do-debug
	ELSE
	    wayland( [: cr ." zero bytes in " public$ id. ;] do-debug )
	    in-fd fileno set-noblock
	    pagesize dup in$ $+!len swap dup >r in-fd read-file
	    dup -512 EAGAIN - = IF  drop
		\ clean POLLHUP flag, it's EAGAIN
		flagaddr w@ POLLHUP invert and flagaddr w!
	    ELSE  throw  THEN
	    r> - dup 0< IF  in$ $+!len  THEN  drop
	THEN
    THEN ;
in-reader :method ?inout ( addr -- addr' )
    in-fd IF  >r
	wayland( r@ [: cr public$ id. 1 backspaces ." : " w@ h. ;] do-debug )
	r@ w@ POLLIN  and IF  r@ read-in  THEN
	r@ w@ POLLHUP and IF  eof-in   THEN
	r> pollfd +
    THEN ;
in-reader :method +inout ( addr -- addr' )
    in-fd ?dup-IF  fileno POLLIN POLLHUP or  rot fds!+  THEN ;

out-writer :method +inout ( addr -- addr' )
    out-fd ?dup-IF  POLLOUT POLLHUP rot fds!+  THEN ;

out-writer :method write-out ( -- )
    out$ $@ out-offset @ safe/string
    out-fd -rot write dup -1 <> IF  out-offset +!
	out-fd out$ $@len out-offset @ u> ?EXIT drop
    ELSE
	drop errno EAGAIN = ?EXIT
	-512 errno - [: cr ." Error writing clipboard pipe: " error$ type ;] do-debug
    THEN \ if we can't write, let's just abandon this operation
    wayland( out$ [: cr ." wrote '" $. ." ' to clipout" ;] do-debug )
    eof-out ;
out-writer :method eof-out ( -- )
    out-fd close -1 = IF
	-512 errno - [: cr ." Error closing clipboard pipe: " error$ type ;] do-debug
    THEN
    out<< $@len 2 cells u>= IF
	out<< stack>
	out<< stack> $@ out$ $!
	out<< $free0
    ELSE  out$ $free 0  THEN
    to out-fd  out-offset off ;

out-writer :method ?inout ( addr -- addr' )
    out-fd IF  >r
	wayland( r@ [: cr o id. 1 backspaces ." : " w@ h. ;] do-debug )
	r@ w@ POLLHUP and IF  eof-out
	ELSE  r@ w@ POLLOUT and IF  write-out  THEN  THEN
	r> pollfd +
    THEN ;

out-writer :method set-out ( addr fd -- )
    out-fd IF  out<< >back out<< >back  EXIT  THEN
    to out-fd  $@ out$ $!  out-offset off
    wayland( [: cr ." set out " out-name id. ." to '" out$ $. ." '" ;] do-debug )
    out-fd set-noblock  write-out ;

: accept+receive { offer d: mime-type object | fds[ 2 cells ] -- }
    offer current-serial mime-type wl_data_offer_accept
    fds[ create_pipe
    offer mime-type fds[ cell+ @ fileno wl_data_offer_receive
    fds[ cell+ @ close-file throw
    fds[ @ object .set-in ;

: ps-accept+receive { offer d: mime-type | fds[ 2 cells ] -- }
    fds[ create_pipe
    offer mime-type fds[ cell+ @ fileno zwp_primary_selection_offer_v1_receive
    fds[ cell+ @ close-file throw
    fds[ @ psin$ .set-in ;

: >liked-mime { xt: xt -- }
    liked-mime[] $[]# 0 ?DO
	I liked-mime[] $[]@ ?mime-type IF
	    I liked-mime[] $[]@
	    wayland( [: cr ." accept: " 2dup type ;] do-debug )
	    xt  LEAVE
	THEN
    LOOP ;

<cb wl_data_offer
:cb action { data offer dnd-actions -- }
    wayland( dnd-actions [: cr ." dnd-actions: " h. ;] do-debug ) ;
:cb source_actions { data offer source-actions -- }
    wayland( source-actions [: cr ." source-actions: " h. ;] do-debug )
    offer dndin$ clipin$ source-actions select
    [{: offer in$ :}l offer -rot in$ accept+receive ;] >liked-mime ;
:cb offer { data offer d: mime-type -- }
    wayland( mime-type [: cr ." mime-type: " type ;] do-debug )
    mime-type mime-types[] $+[]! ;
cb>

\ data device listener

2Variable dnd-xy
Defer dnd-move
Defer dnd-drop

0 Value old-id

<cb wl_data_device
:cb selection { data data-device id -- }
    wayland( id [: cr ." selection id: " h. ;] do-debug )
    id ?dup-IF  [{: id :}l id -rot clipin$ accept+receive ;] >liked-mime  THEN ;
:cb drop { data data-device -- }
    wayland( [: cr ." drop" ;] do-debug )
    [: dnd-xy 2@ dnd$ $@ dnd-drop ;] master-task send-event ;
:cb motion { data data-device time x y -- }
    wayland( y x time [: cr ." motion [time,x,y] " . . . ;] do-debug )
    x y dnd-xy 2!
    x y [{: x y :}h1 x y dnd-move ;] master-task send-event ;
:cb leave { data data-device -- }
    wayland( [: cr ." leave" ;] do-debug ) ;
:cb enter { data data-device serial surface x y id -- }
    serial( serial [: cr ." device enter serial: " h. ;] do-debug )
    wayland( id y x surface [: cr ." enter [surface,x,y,id] " h. . . h. ;] do-debug )
    serial to current-serial
    serial to last-serial ;
:cb data_offer { data data-device id -- }
    wayland( id [: cr ." offer: " h. ;] do-debug )
    old-id ?dup-IF  wl_data_offer_destroy  THEN
    id to old-id
    mime-types[] $[]free
    id ?dup-IF  wl-data-offer-listener 0 wl_data_offer_add_listener drop  THEN ;
cb>

\ primary selection offer listener

<cb zwp_primary_selection_offer_v1
:cb offer { data offer d: mime-type -- }
    wayland( mime-type [: cr ." primary mime-type: " type ;] do-debug )
    mime-type mime-types[] $+[]! ;
cb>

\ primary selection device listener

0 Value old-ps-id

<cb zwp_primary_selection_device_v1
:cb selection { data data-device id -- }
    wayland( id [: cr ." primary selection id/device/mydevice: " h. ;] do-debug )
    my-primary 0= IF
	id ?dup-IF  [{: id :}l id -rot ps-accept+receive ;] >liked-mime  THEN
    THEN ;
:cb data_offer { data data-device id -- }
    wayland( id [: cr ." primary offer: " h. ;] do-debug )
    old-ps-id ?dup-IF  zwp_primary_selection_offer_v1_destroy  THEN
    id to old-ps-id
    mime-types[] $[]free  0 to my-primary
    id ?dup-IF  zwp-primary-selection-offer-v1-listener 0
	zwp_primary_selection_offer_v1_add_listener  THEN ;
cb>

\ data source listener

<cb wl_data_source
:cb action { data source dnd-action -- }
    wayland( dnd-action [: cr ." ds action: " h. ;] do-debug ) ;
:cb dnd_finished { data source -- } ;
:cb dnd_drop_performed { data source -- } ;
:cb cancelled { data source -- }
    wayland( [: cr ." ds cancelled" ;] do-debug )
    0 to data-source  0 to my-clipboard
    source wl_data_source_destroy ;
:cb send { data source d: mime-type fd -- }
    wayland( mime-type data [: cr ." send " id. ." type " type ;] do-debug )
    data fd clipout$ .set-out ;
:cb target { data source d: mime-type -- }
    wayland( data mime-type [: cr ." ds target: " type space id. ;] do-debug ) ;
cb>

\ primary selection source listener

<cb zwp_primary_selection_source_v1
:cb cancelled { data source -- }
    wayland( [: cr ." ps cancelled" ;] do-debug )
    0 to primary-selection-source  0 to my-primary
    source zwp_primary_selection_source_v1_destroy ;
:cb send { data source d: mime-type fd -- }
    wayland( fd mime-type data [: cr ." ps send " id. ." type: " type ."  fd: " h. ;] do-debug )
    data fd psout$ .set-out ;
cb>

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
    data-device wl-data-device-listener 0 wl_data_device_add_listener drop ;
: ?dd-source ( -- )
    data-source 0= IF
	wl-data-device-manager
	wl_data_device_manager_create_data_source
	to data-source THEN ;
:trigger-on( data-source )
    data-source wl-data-source-listener clipboard$ wl_data_source_add_listener drop
    ds-mime-types[] [: data-source -rot wl_data_source_offer ;] $[]map ;

:trigger-on( zwp-primary-selection-device-manager-v1 wl-seat )
    primary-selection-device ?EXIT
    zwp-primary-selection-device-manager-v1
    wl-seat zwp_primary_selection_device_manager_v1_get_device to primary-selection-device ;
:trigger-on( primary-selection-device )
    primary-selection-device zwp-primary-selection-device-v1-listener
    0 zwp_primary_selection_device_v1_add_listener drop ;
: ?ps-source ( -- )
    primary-selection-source 0= IF
	zwp-primary-selection-device-manager-v1
	zwp_primary_selection_device_manager_v1_create_source
	to primary-selection-source THEN ;
:trigger-on( primary-selection-source )
    primary-selection-source zwp-primary-selection-source-v1-listener
    primary$ zwp_primary_selection_source_v1_add_listener drop
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
1 wl: wl_subcompositor
1 wl: wl_shell
:trigger-on( wl-shell wl-surface )
    wl-shell wl-surface wl_shell_get_shell_surface to shell-surface ;
1 wl: xdg_activation_v1
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
[IFDEF] wp_cursor_shape_manager_v1_interface
    1 wl: wp_cursor_shape_manager_v1
    :trigger-on( wp-cursor-shape-manager-v1 wl-pointer )
	wp-cursor-shape-manager-v1 wl-pointer wp_cursor_shape_manager_v1_get_pointer
	to wp-cursor-shape-device-v1 ;
    :trigger-on( wp-cursor-shape-device-v1 cursor-serial cursor-type )
	wp-cursor-shape-device-v1 cursor-serial cursor-type wp_cursor_shape_device_v1_set_shape ;
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
[IFDEF] zxdg_output_v1_add_listener
    3 wl: zxdg_output_manager_v1
    :trigger-on( zxdg-output-manager-v1 wl-output )
	zxdg-output-manager-v1 wl-output zxdg_output_manager_v1_get_xdg_output dup to zxdg-output-v1
	zxdg-output-v1-listener 0 zxdg_output_v1_add_listener drop ;
[THEN]
8 wlal: wl_seat
1 wl: wl_shm
:trigger-on( wl-shm )
    cursor-theme$ $@ cursor-size @
    wayland( [: cr ." load cursor theme " third third type ."  size " dup . ;] do-debug )
    wl-shm wl_cursor_theme_load dup to cursor-theme
    s" default" wl_cursor_theme_get_cursor to cursor ;
[IFDEF] zwp_text_input_v3_add_listener
    1 wl: zwp_text_input_manager_v3
    :trigger-on( zwp-text-input-manager-v3 wl-seat )
	zwp-text-input-manager-v3 wl-seat zwp_text_input_manager_v3_get_text_input dup to text-input
	zwp-text-input-v3-listener 0 zwp_text_input_v3_add_listener drop ;
[THEN]
6 wlal: xdg_wm_base
1 wl: zxdg_decoration_manager_v1
3 wl: wl_data_device_manager
1 wl: zwp_primary_selection_device_manager_v1
1 wl: zwp_idle_inhibit_manager_v1
[IFDEF] wp_tearing_control_manager_v1_interface
    1 wl: wp_tearing_control_manager_v1
    :trigger-on( wp-tearing-control-manager-v1 wl-surface )
	wp-tearing-control-manager-v1 wl-surface
	wp_tearing_control_manager_v1_get_tearing_control
	dup to wp-tearing-control-v1
	WP_TEARING_CONTROL_V1_PRESENTATION_HINT_VSYNC
	wp_tearing_control_v1_set_presentation_hint ;
[THEN]
[IFDEF] wp_color_manager_v1_interface_xxx \ disable for now
    1 wlal: wp_color_manager_v1
    :trigger-on( wp-color-manager-v1 wl-output wl-surface )
	wp-color-manager-v1 wl-output wp_color_manager_v1_get_output
	dup to wp-color-management-output-v1
	wp-color-management-output-v1-listener 0
	wp_color_management_output_v1_add_listener drop
	wp-color-manager-v1 wl-surface wp_color_manager_v1_get_surface
	to wp-color-management-surface-v1
	wp-color-manager-v1 wl-surface wp_color_manager_v1_get_surface_feedback
	dup to wp-color-management-surface-feedback-v1
	wp-color-management-surface-feedback-v1-listener 0
	wp_color_management_surface_feedback_v1_add_listener drop ;
[THEN]
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

<cb wl_registry
' registry- ?cb global_remove
' registry+ ?cb global
cb>

: get-events ( -- )
    dpy wl_display_roundtrip drop ;

: get-display ( -- w h )
    ${SNAP} d0<> IF \ Snap workaround
	${XDG_RUNTIME_DIR} file-status nip 0< IF
	    [: ${XDG_RUNTIME_DIR} dirname type ${WAYLAND_DISPLAY} type ;] $tmp
	    wayland( cr [: ." wayland workaround display for snap: " 2dup type ;] do-debug )
	ELSE  0 0  THEN
    ELSE  0 0  THEN  wl_display_connect to dpy
    dpy 0= IF
	[:  ." no wayland display in " ${XDG_RUNTIME_DIR} type
	    ." /" ${WAYLAND_DISPLAY} type cr ;] do-debug
	true abort" no wayland display connected"
    THEN
    dpy wl_display_get_registry to registry
    registry 0= abort" no wayland registry"
    registry wl-registry-listener 0 wl_registry_add_listener drop
    false to registered
    BEGIN  get-events  registered UNTIL  dpy-wh 2@ ;

\ xdg surface listener

0 Value mapped
0 Value configured

forward sync
forward clear
2Variable toplevel-wh &640 &400 toplevel-wh 2!
Variable toplevel-states 0 toplevel-states !

<cb xdg_surface
:cb configure { data xdg_surface serial -- }
    serial( serial [: cr ." surface configure serial: " h. ;] do-debug )
    serial to last-serial
    wayland( serial [: cr ." configured, serial " h. ;] do-debug )
    true to mapped
    xdg_surface serial xdg_surface_ack_configure
    toplevel-wh 2@ rescale-win
    wl-surface wl_surface_commit ;
cb>

: map-win ( -- )
    BEGIN  get-events mapped  UNTIL ;

[IFUNDEF] level#  Variable level#  [THEN]

2Variable wl-min-size &640 &400 wl-min-size 2!
[IFUNDEF] rendering  Variable rendering  [THEN]
-2 rendering !

: .states ( addr u -- )
    bounds U+DO  I l@ case
	    1 of ." maximized " endof
	    2 of ." fullscreen " endof
	    3 of ." resizing " endof
	    4 of ." activated " endof
	    5 of ." tiled-left " endof
	    6 of ." tiled-right " endof
	    7 of ." tiled-top " endof
	    8 of ." tiled-bottom " endof
	    9 of ." suspended " endof
	    dup .
    endcase 4 +LOOP ;

<cb xdg_toplevel
:cb wm_capabilities { data xdg_toplevel capabilities -- }
    wayland( capabilities [: cr ." wm capabilities: " h. ;] do-debug ) ;
:cb configure_bounds { data xdg_toplevel width height -- }
    wayland( height width [: cr ." toplevel bounds: " . . ;] do-debug )
    xdg_toplevel wl-min-size 2@ xdg_toplevel_set_min_size
    xdg_toplevel width height xdg_toplevel_set_max_size ;
:cb close { data xdg_toplevel -- }
    wayland( [: cr ." close" ;] do-debug )
    -1 level# +! ;
:cb configure { data xdg_toplevel width height states -- }
    wayland( states height width [: cr ." toplevel-config: " . .
    cr ." states: " >r r@ wl_array-data @ r> wl_array-size @ .states ;] do-debug )
    width height toplevel-wh 2!
    0 states wl_array-data @ states wl_array-size @ bounds ?DO
	1 I l@ lshift or
    4 +LOOP  dup toplevel-states !
    wayland( [: cr ." display state: " dup h. ;] do-debug )
    \ if suspended, stop rendering!
    1 XDG_TOPLEVEL_STATE_SUSPENDED lshift and 0= 1- rendering ! ;
cb>

<cb zxdg_toplevel_decoration_v1
:cb configure { data decoration mode -- }
    wayland( [: cr ." decorated" ;] do-debug )
    true to configured clear sync ;
cb>

:trigger-on( xdg-wm-base wl-surface )
    xdg-wm-base wl-surface xdg_wm_base_get_xdg_surface to xdg-surface
    xdg-surface xdg-surface-listener 0 xdg_surface_add_listener drop
    xdg-surface xdg_surface_get_toplevel to xdg-toplevel ;
:trigger-on( xdg-toplevel )
    xdg-toplevel xdg-toplevel-listener 0 xdg_toplevel_add_listener drop
\    xdg-toplevel 0 xdg_toplevel_set_parent
    xdg-toplevel window-title$ $@ xdg_toplevel_set_title
    xdg-toplevel window-app-id$ $@ xdg_toplevel_set_app_id
    xdg-toplevel xdg_toplevel_set_maximized
    wl-surface wl_surface_commit ;

: wl-eglwin { w h -- }
    wayland( h w [: cr ." eglwin: " . . ;] do-debug )
    xdg-surface ?dup-IF  0 0 w h xdg_surface_set_window_geometry  THEN
    wl-surface w h wl_egl_window_create to win
    wl-surface wl_surface_commit
    wayland( [: cr ." wl-eglwin done" ;] do-debug ) ;

:trigger-on( zxdg-decoration-manager-v1 xdg-toplevel )
    zxdg-decoration-manager-v1 xdg-toplevel
    zxdg_decoration_manager_v1_get_toplevel_decoration dup to zxdg-decoration
    dup zxdg-toplevel-decoration-v1-listener 0
    zxdg_toplevel_decoration_v1_add_listener drop
    ZXDG_TOPLEVEL_DECORATION_V1_MODE_SERVER_SIDE
    zxdg_toplevel_decoration_v1_set_mode
    wl-surface wl_surface_commit ;

also opengl
: getwh ( -- )
    0 0 dpy-wh 2@ glViewport ;
previous

\ looper

get-current also forth definitions

previous set-current

9 Value xpollfd#
\ events, wayland, clip read, clip write, ps read, ps write, dnd read, dnd write, infile
User xpollfds
xpollfds pollfd xpollfd# * dup cell- uallot drop erase

Create inout$s
clipin$ ,  psin$ ,  dndin$ ,  clipout$ , psout$ , dndout$ ,
here latestxt - >r
DOES> { xt: do-it array } array [ r> ]L bounds DO  I @ .do-it  cell +LOOP ;

: >poll-events ( delay -- addr u )
    0 xptimeout 2!  xpollfds dup
    dpy ?dup-IF  wl_display_get_fd POLLIN  rot fds!+  THEN
    ['] +inout inout$s
    epiper @ fileno POLLIN  rot fds!+
    xpollfds - pollfd / ;

: xpoll ( -- flag )
    [IFDEF] ppoll
	xptimeout dup @ 0< IF  drop 0  THEN  0 ppoll 0>
    [ELSE]
	xptimeout 2@ #1000 * swap #1000000 / + poll 0>
    [THEN] ;

Defer ?looper-timeouts ' noop is ?looper-timeouts

: ?dpy ( addr -- addr' )
    dpy IF  >r
	r@ w@ POLLIN and IF  dpy wl_display_dispatch drop  THEN
	r> pollfd +
    THEN ;

: #looper ( delay -- ) #1000000 *
    ?looper-timeouts >poll-events xpoll
    IF
	xpollfds revents ?dpy ['] ?inout inout$s w@ POLLIN and IF  ?events  THEN
    ELSE
\	dpy wl_display_sync wl-callback-listener 0 wl_callback_add_listener drop
    THEN ;

: >looper ( -- )  looper-to# #looper ;

\ android similarities

require need-x.fs

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

: term-key? ( -- flag )
    stdin isfg IF  defers key?  ELSE  key-buffer $@len 0>  THEN ;
: wl-key? ( -- flag ) 0 #looper
    term-key? dup 0= ?sync and IF  screen-ops  THEN ;
: wl-key ( -- key )
    +show  key? IF  defers key-ior  EXIT  THEN
    BEGIN  >looper  key? UNTIL  defers key-ior ;
: wl-deadline ( dtime -- )
    up@ [ up@ ]L = IF screen-ops THEN  defers deadline ;
' wl-deadline IS deadline

' wl-key IS key-ior
' wl-key? IS key?

: clipboard! ( addr u -- ) clipboard$ $!
    ?dd-source
    data-device data-source
    last-serial wl_data_device_set_selection
    true to my-clipboard ;
: clipboard@ ( -- addr u ) clipboard$ $@ ;

: primary! ( addr u -- ) primary$ $!
    primary$ $@len IF
	?ps-source  true to my-primary
    ELSE
	primary-selection-source ?dup-IF
	    zwp_primary_selection_source_v1_destroy
	    0 to primary-selection-source
	THEN
	0 to my-primary
    THEN
    primary-selection-device primary-selection-source
    last-serial zwp_primary_selection_device_v1_set_selection ;
: primary@ ( -- addr u ) primary$ $@ ;
: dnd@ ( -- addr u ) dnd$ $@ ;

also OpenGL
\\\
Local Variables:
forth-local-words:
    (
	((":cb") definition-starter (font-lock-keyword-face . 1)
	 "[ \t\n]" t name (font-lock-function-name-face . 3))
    )
forth-local-indent-words:
    (
        ((":cb")
	 (0 . 2) (0 . 2) non-immediate)
    )
End:
