\ gstreamer GL helper stuff

\ Copyright (C) 2018 Free Software Foundation, Inc.

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
$10 buffer: gst-state

: .gst-error ( -- )
    gst-error @ ?dup-IF
	cell+ @ cstring>sstring [: type cr ;]
	error-color ['] color-execute do-debug
    THEN ;

: reshape-cb ( -- ) ~~ ;
: draw-cb ( -- )    ~~ ;
: events-cb ( -- )  ~~ ;
: query-cb ( -- )   ~~ ;

' reshape-cb reshapeCallback:         Constant reshape_cb
' draw-cb    drawCallback:            Constant draw_cb
' events-cb  GstPadEventFullFunction: Constant events_cb
' query-cb   GstPadEventFullFunction: Constant query_cb

: gst-init ( -- )
    0 0 gst-error gst_init_check 0= IF  .gst-error !!gst!! throw  THEN ;
: gst-launch ( addr u -- )
    [: ." filesrc location=" type
	."  ! qtdemux name=dmx ! vaapidecodebin ! glimagesink name=vsink"
	."  audioconvert dmx. ! pulsesink name=asink" ;] $tmp
    gst-error gst_parse_launch pipeline ! .gst-error
    gst-error @ IF  !!gst!! throw  THEN ;

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
    gst-init gst-launch  gst-@sink  gst-pad-probes ;

: test !time "/home/bernd/Video/ft2018/net2o.mp4" init-pipeline .time ;

previous
