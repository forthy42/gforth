\ gstreamer GL helper stuff

\ Author: Bernd Paysan
\ Copyright (C) 2018,2019 Free Software Foundation, Inc.

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

require unix/gstlib.fs

also gst

s" gstreamer error" exception constant !!gst!!

Variable gst-error
Variable pipeline
Variable vsink
Variable asink
Variable gst-pad
Variable g-loop
Variable g-context
Variable gst-display
Variable gl-context
$10 buffer: gst-state

\ set context from OpenGL ES

[IFDEF] use-egl
    : set-egl-context ( -- )
	egldpy gst_gl_display_egl_new_with_egl_display gst-display !
	gst-display @ gst_gl_display_egl_get_type g_type_check_instance_cast
	ctx GST_GL_PLATFORM_EGL GST_GL_API_GLES2
	gst_gl_context_new_wrapped gl-context ! ;
[THEN]

\ print errors

: .gst-error ( -- )
    gst-error @ ?dup-IF
	cell+ @ cstring>sstring [: type cr ;]
	error-color ['] color-execute do-debug
    THEN ;

: reshape-cb ( -- ) ." reshape-cb" cr ;
: draw-cb ( -- )    ." draw-cb" cr ;
: events-cb ( -- )  ." events-cb" cr ;
: query-cb          ." query-cb " { pad info u_d -- ok }
    info _GstPadProbeInfo-data @ { query }
    case query _GstQuery-type l@ dup .
	GST_QUERY_CONTEXT of
	    pipeline @ query gst-display @ 0 gl-context @
	    gst_gl_handle_context_query IF
		GST_PAD_PROBE_HANDLED EXIT
	    THEN
	endof
    endcase
    GST_PAD_PROBE_OK ;
: bus-cb { bus message u_d -- }
    case message _GstMessage-type l@
	GST_MESSAGE_EOS   of ." End-of-Stream reached" cr endof
	GST_MESSAGE_ERROR of
	    ." error received" cr
	    { | err debug-info }
	    message addr err addr debug-info gst_message_parse_error
	    err 8 + @ cstring>sstring type cr
	endof
	GST_MESSAGE_STATE_CHANGED of
	    message _GstMessage-src @
	    pipeline @ gst_object_get_type g_type_check_instance_cast
	    = IF
		." Pipeline state changed" cr
		{ | old-state new-state pending-state }
		message addr old-state addr new-state addr pending-state
		gst_message_parse_state_changed
		old-state gst_element_state_get_name cstring>sstring type cr
		new-state gst_element_state_get_name cstring>sstring type cr
	    THEN
	endof
    endcase ;

' reshape-cb reshapeCallback:         Constant reshape_cb
' draw-cb    drawCallback:            Constant draw_cb
' events-cb  GstPadEventFullFunction: Constant events_cb
' query-cb   GstPadEventFullFunction: Constant query_cb
' bus-cb     GstBusFunc:              Constant bus_cb

: gst-init ( -- )
    0 0 gst-error gst_init_check 0= IF  .gst-error !!gst!! throw  THEN ;
: gst-launch ( addr u -- )
    [: ." filesrc location=" type
	."  ! qtdemux name=dmx ! decodebin3 ! glimagesink name=vsink"
	."  audioconvert dmx. ! pulsesink name=asink" ;] $tmp
    gst-error gst_parse_launch pipeline ! .gst-error
    gst-error @ IF  !!gst!! throw  THEN ;

: gst-@bus ( -- )
    0 0 g_main_loop_new g-loop !
    g-loop @ g_main_loop_get_context g-context !
    pipeline @ gst_pipeline_get_bus { bus }
    bus bus_cb g-loop @ gst_bus_add_watch drop
    bus gst_object_unref ;

: gst-@sink ( -- )
    pipeline @ "vsink" gst_bin_get_by_name vsink !
    pipeline @ "asink" gst_bin_get_by_name asink ! ;

: gst-pad-probes ( -- )
    vsink @ "sink" gst_element_get_static_pad { gst-pad }
    vsink @ "client-reshape" reshape_cb gst-state g_signal_connect drop
    vsink @ "client-draw"    draw_cb    gst-state g_signal_connect drop
    gst-pad GST_PAD_PROBE_TYPE_EVENT_DOWNSTREAM events_cb gst-state 0
    gst_pad_add_probe drop
    gst-pad GST_PAD_PROBE_TYPE_QUERY_DOWNSTREAM query_cb gst-state 0
    gst_pad_add_probe drop
    gst-pad gst_object_unref ;

: init-pipeline ( addr u -- )
    gst-init  set-egl-context  gst-launch
    gst-@bus  gst-@sink  gst-pad-probes ;

: set-pipeline ( state -- )
    pipeline @ swap gst_element_set_state drop ;

: gst-play ( -- ) GST_STATE_PLAYING set-pipeline ;
: gst-pause ( -- ) GST_STATE_PAUSED set-pipeline ;
: gst-ready ( -- ) GST_STATE_READY set-pipeline ;

: iter? ( -- flag )
    g-context @ 0 g_main_context_iteration ;

:noname BEGIN  iter?  0= UNTIL defers looper-hook ; is looper-hook

: test !time "/home/bernd/Videos/ft2018/net2o.mp4" init-pipeline .time ;

previous
