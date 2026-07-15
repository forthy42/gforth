\ video4linux2 capture

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

require unix/libc.fs
require unix/mmap.fs

cs-vocabulary v4l2

get-current >r also v4l2 definitions

require unix/v4l2.fs

0 Value video-fd

: open-video ( n -- )
    [: ." /dev/video" 0 .r ;] $tmp r/w open-file throw to video-fd ;
: close-video ( -- )
    video-fd close-file  0 to video-fd  throw ;

v4l2_query_ext_ctrl buffer: query-buf
v4l2_capability     buffer: cap-buf
v4l2_fmtdesc        buffer: fmtdesc-buf
v4l2_format         buffer: fmt-buf
v4l2_frmsizeenum    buffer: frmsize-buf
v4l2_frmivalenum    buffer: frmival-buf

: query ( -- buffer )
    video-fd fileno VIDIOC_QUERY_EXT_CTRL query-buf ioctl ?ior ;
: first-query ( -- ) V4L2_CTRL_FLAG_NEXT_CTRL query-buf l! query ;
: next-query ( -- )
    V4L2_CTRL_FLAG_NEXT_CTRL query-buf l@ or query-buf l! query ;
: .cstring ( addr -- )
    cstring>sstring type ;
: .query ( -- )
    ." Name: " query-buf v4l2_query_ext_ctrl-name .cstring cr
    ." id:   " query-buf v4l2_query_ext_ctrl-id l@ hex. cr
    ." min: " query-buf v4l2_query_ext_ctrl-minimum @ .
    ." max: " query-buf v4l2_query_ext_ctrl-maximum @ .
    ." step: " query-buf v4l2_query_ext_ctrl-step @ .
    ." default: " query-buf v4l2_query_ext_ctrl-default_value @ . cr ;
: querycap ( -- )
    video-fd fileno VIDIOC_QUERYCAP cap-buf ioctl ?ior ;
: enum-fmt ( index type -- )
    fmtdesc-buf v4l2_fmtdesc-type l!
    fmtdesc-buf v4l2_fmtdesc-index l!
    video-fd fileno VIDIOC_ENUM_FMT fmtdesc-buf ioctl ?ior ;
: enum-framesize ( index pxfmt -- )
    frmsize-buf v4l2_frmsizeenum-pixel_format l!
    frmsize-buf v4l2_frmsizeenum-index l!
    video-fd fileno VIDIOC_ENUM_FRAMESIZES frmsize-buf ioctl ?ior ;
: enum-frameival ( index pxfmt w h -- )
    frmival-buf v4l2_frmivalenum-height l!
    frmival-buf v4l2_frmivalenum-width l!
    frmival-buf v4l2_frmivalenum-pixel_format l!
    frmival-buf v4l2_frmivalenum-index l!
    video-fd fileno VIDIOC_ENUM_FRAMEINTERVALS frmival-buf ioctl ?ior ;
: .frame-ival ( -- )
    case
	frmival-buf v4l2_frmivalenum-type l@
	V4L2_FRMIVAL_TYPE_DISCRETE of
	    frmival-buf v4l2_frmivalenum-discrete
	    dup v4l2_fract-numerator l@ 0 .r ." /"
	    v4l2_fract-denominator l@ 0 .r ." s "
	endof
	V4L2_FRMIVAL_TYPE_CONTINUOUS of endof
	V4L2_FRMIVAL_TYPE_STEPWISE of endof
    endcase ;
: .framesize ( -- )
    ." index: " frmsize-buf v4l2_frmsizeenum-index l@ .
    ." type: " frmsize-buf v4l2_frmsizeenum-type l@ dup .
    case
	V4L2_FRMSIZE_TYPE_DISCRETE of
	    frmsize-buf v4l2_frmsizeenum-discrete v4l2_frmsize_discrete-width l@
	    dup 0 .r ." /"
	    frmsize-buf v4l2_frmsizeenum-discrete v4l2_frmsize_discrete-height l@ dup 0 .r space
	    frmsize-buf v4l2_frmsizeenum-pixel_format l@ { w h pxfmt }
	    10 0 DO
		I pxfmt w h ['] enum-frameival catch IF
		    2drop 2drop LEAVE
		ELSE  .frame-ival  THEN
	    LOOP cr
	endof
	V4L2_FRMSIZE_TYPE_STEPWISE of
	    ." ["
	    frmsize-buf v4l2_frmsizeenum-stepwise v4l2_frmsize_stepwise-min_width l@ 0 .r ." -"
	    frmsize-buf v4l2_frmsizeenum-stepwise v4l2_frmsize_stepwise-max_width l@ 0 .r ." :"
	    frmsize-buf v4l2_frmsizeenum-stepwise v4l2_frmsize_stepwise-step_width l@ 0 .r ." /"
	    frmsize-buf v4l2_frmsizeenum-stepwise v4l2_frmsize_stepwise-min_height l@ 0 .r ." -"
	    frmsize-buf v4l2_frmsizeenum-stepwise v4l2_frmsize_stepwise-max_height l@ 0 .r ." :"
	    frmsize-buf v4l2_frmsizeenum-stepwise v4l2_frmsize_stepwise-step_height l@ 0 .r ." ]"
	endof
    endcase ;
: enum-framesizes { pxfmt -- }
    100 0 DO
	I pxfmt ['] enum-framesize catch IF  2drop  LEAVE
	ELSE  .framesize  THEN
    LOOP ;
: get-fmt ( type -- )
    fmt-buf v4l2_format-type l!
    video-fd fileno VIDIOC_G_FMT fmt-buf ioctl ?ior ;
: .fmt-types { index -- }
    V4L2_BUF_TYPE_META_OUTPUT 1+ V4L2_BUF_TYPE_VIDEO_CAPTURE U+DO
	index i ['] enum-fmt catch IF  2drop  ELSE
	    ." format: " index . I . cr
	    ." desc:  " fmtdesc-buf v4l2_fmtdesc-description .cstring cr
	    ." flags: " fmtdesc-buf v4l2_fmtdesc-flags l@ hex. cr
	    ." pxl:   " fmtdesc-buf v4l2_fmtdesc-pixelformat 4 type cr
	    fmtdesc-buf v4l2_fmtdesc-pixelformat l@ enum-framesizes
	THEN
    LOOP ;
: .fmts ( -- )
    10 0 DO  I .fmt-types  LOOP ;
: .fmt ( -- )
    V4L2_BUF_TYPE_VIDEO_CAPTURE ['] get-fmt catch IF  drop
    ELSE  fmt-buf v4l2_format-fmt >r
	." w/h:   " r@ v4l2_pix_format-width l@ 0 .r '/' emit
	r@ v4l2_pix_format-height l@ 0 .r cr
	." size:  " r@ v4l2_pix_format-sizeimage l@ . cr
	." csp:   " r@ v4l2_pix_format-colorspace l@ . cr
	." pxfmt: " r@ v4l2_pix_format-pixelformat 4 type cr
	rdrop
    THEN ;
: .querycap ( -- )
    querycap
    ." driver:   " cap-buf v4l2_capability-driver .cstring cr
    ." card:     " cap-buf v4l2_capability-card .cstring cr
    ." bus:      " cap-buf v4l2_capability-bus_info .cstring cr
    ." caps:     " cap-buf v4l2_capability-capabilities l@ hex. cr
    ." dev-caps: " cap-buf v4l2_capability-device_caps l@ hex. cr ;
: .queries ( -- )  .querycap
    first-query  BEGIN  .query ['] next-query catch  UNTIL ;

\ Capture instructions from here:
\ https://www.marcusfolkesson.se/blog/capture-a-picture-with-v4l2/

: request-buffer ( n1 -- n2 )
    { | req[ v4l2_requestbuffers ] }
    req[ v4l2_requestbuffers erase
    req[ v4l2_requestbuffers-count l!
    V4L2_BUF_TYPE_VIDEO_CAPTURE req[ v4l2_requestbuffers-type l!
    V4L2_MEMORY_MMAP req[ v4l2_requestbuffers-memory l!
    video-fd fileno VIDIOC_REQBUFS req[ ioctl ?ior
    req[ v4l2_requestbuffers-count l@ ;
: query-buffer ( index -- addr u )
    { | buf[ v4l2_buffer ] }
    buf[ v4l2_buffer erase
    V4L2_BUF_TYPE_VIDEO_CAPTURE buf[ v4l2_buffer-type l!
    V4L2_MEMORY_MMAP buf[ v4l2_buffer-memory l!
    buf[ v4l2_buffer-index l!
    video-fd fileno VIDIOC_QUERYBUF buf[ ioctl ?ior
    0 buf[ v4l2_buffer-length l@ PROT_READ PROT_WRITE or MAP_SHARED
    video-fd fileno buf[ v4l2_buffer-m v4l2_buffer_m-offset l@ mmap
    buf[ v4l2_buffer-length l@ ;
: queue-buffer ( index -- used )
    { | buf[ v4l2_buffer ] }
    V4L2_BUF_TYPE_VIDEO_CAPTURE buf[ v4l2_buffer-type l!
    V4L2_MEMORY_MMAP buf[ v4l2_buffer-memory l!
    buf[ v4l2_buffer-index l!
    video-fd fileno VIDIOC_QBUF buf[ ioctl ?ior
    buf[ v4l2_buffer-bytesused l@ ;
: dequeue-buffer ( -- len index )
    { | buf[ v4l2_buffer ] }
    V4L2_BUF_TYPE_VIDEO_CAPTURE buf[ v4l2_buffer-type l!
    V4L2_MEMORY_MMAP buf[ v4l2_buffer-memory l!
    0 buf[ v4l2_buffer-index l!
    video-fd fileno VIDIOC_DQBUF buf[ ioctl ?ior
    buf[ v4l2_buffer-bytesused l@
    buf[ v4l2_buffer-index l@ ;
: start-streaming ( -- )
    { | type[ 4 ] }
    V4L2_BUF_TYPE_VIDEO_CAPTURE type[ l!
    video-fd fileno VIDIOC_STREAMON type[ ioctl ?ior ;
: stop-streaming ( -- )
    { | type[ 4 ] }
    V4L2_BUF_TYPE_VIDEO_CAPTURE type[ l!
    video-fd fileno VIDIOC_STREAMOFF type[ ioctl ?ior ;

8 Value buffers#
$Variable buffers[]

\ for testing, write to the file in videout-fd
0 Value videoout-fd
: write-video ( addr u -- flag )
    '.' emit videoout-fd write-file throw true ;

\ generic run-capture
: start-capture ( -- )
    buffers# request-buffer dup to buffers# 2* cells buffers[] $!len
    buffers# 0 ?DO
	I query-buffer buffers[] $@ I 2* cells safe/string drop 2!
	I queue-buffer drop
    LOOP ;
: run-capture { xt: runner -- }
    \G runner is ( addr u -- flag ), stops if flag is true
    BEGIN
	dequeue-buffer { length index }
	buffers[] $@ index 2* cells safe/string drop 2@ length umin runner
	index queue-buffer drop
    UNTIL ;

0 Value v4l2-task
: bg-queue ( index -- )
   [{: index :}h1 index queue-buffer drop ;] v4l2-task send-event ;
: bg-capture ( runner -- )
    \G runner is ( addr length index ), and needs to send index back with bg-queue
    1 stacksize4 newtask4 dup to v4l2-task pass { runner }
    BEGIN
	dequeue-buffer { length index }
	buffers[] $@ index 2* cells safe/string drop 2@ length umin index
	runner [{: a l index xt: runner :}h1 a l index runner ;]
	[ up@ ]L send-event stop
    AGAIN ;

: capture ( runner -- )
    start-capture start-streaming run-capture stop-streaming ;

previous r> set-current
