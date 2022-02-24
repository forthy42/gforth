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

cs-vocabulary v4l2

get-current >r also v4l2 definitions

require unix/v4l2.fs

: _IOC ( dir type nr size -- constant )
    >r swap 8 lshift or r> $10 lshift or swap $1E lshift or ;
: _IO ( type nr -- constant )
    0 -rot 0 _IOC ;
: _IOW ( type nr size -- constant )
    >r 1 -rot r> _IOC ;
: _IOR ( type nr size -- constant )
    >r 2 -rot r> _IOC ;
: _IOWR ( type nr size -- constant )
    >r 3 -rot r> _IOC ;

'V'   0 v4l2_capability		_IOR  Constant VIDIOC_QUERYCAP		
'V'   2 v4l2_fmtdesc		_IOWR Constant VIDIOC_ENUM_FMT
'V'   4 v4l2_format		_IOWR Constant VIDIOC_G_FMT		
'V'   5 v4l2_format		_IOWR Constant VIDIOC_S_FMT		
'V'   8 v4l2_requestbuffers	_IOWR Constant VIDIOC_REQBUFS		
'V'   9 v4l2_buffer		_IOWR Constant VIDIOC_QUERYBUF		
'V'  10 v4l2_framebuffer	_IOR  Constant VIDIOC_G_FBUF		
'V'  11 v4l2_framebuffer	_IOW  Constant VIDIOC_S_FBUF		
'V'  14 4			_IOW  Constant VIDIOC_OVERLAY		
'V'  15 v4l2_buffer		_IOWR Constant VIDIOC_QBUF		
'V'  16 v4l2_exportbuffer	_IOWR Constant VIDIOC_EXPBUF		
'V'  17 v4l2_buffer		_IOWR Constant VIDIOC_DQBUF		
'V'  18 4			_IOW  Constant VIDIOC_STREAMON		
'V'  19 4			_IOW  Constant VIDIOC_STREAMOFF	
'V'  21 v4l2_streamparm		_IOWR Constant VIDIOC_G_PARM		
'V'  22 v4l2_streamparm		_IOWR Constant VIDIOC_S_PARM		
'V'  23 8			_IOR  Constant VIDIOC_G_STD		
'V'  24 8			_IOW  Constant VIDIOC_S_STD		
'V'  25 v4l2_standard		_IOWR Constant VIDIOC_ENUMSTD		
'V'  26 v4l2_input		_IOWR Constant VIDIOC_ENUMINPUT	
'V'  27 v4l2_control		_IOWR Constant VIDIOC_G_CTRL		
'V'  28 v4l2_control		_IOWR Constant VIDIOC_S_CTRL		
'V'  29 v4l2_tuner		_IOWR Constant VIDIOC_G_TUNER		
'V'  30 v4l2_tuner		_IOW  Constant VIDIOC_S_TUNER		
'V'  33 v4l2_audio		_IOR  Constant VIDIOC_G_AUDIO		
'V'  34 v4l2_audio		_IOW  Constant VIDIOC_S_AUDIO		
'V'  36 v4l2_queryctrl		_IOWR Constant VIDIOC_QUERYCTRL	
'V'  37 v4l2_querymenu		_IOWR Constant VIDIOC_QUERYMENU	
'V'  38 4			_IOR  Constant VIDIOC_G_INPUT		
'V'  39 4			_IOWR Constant VIDIOC_S_INPUT		
'V'  40 v4l2_edid		_IOWR Constant VIDIOC_G_EDID		
'V'  41 v4l2_edid		_IOWR Constant VIDIOC_S_EDID		
'V'  46 4			_IOR  Constant VIDIOC_G_OUTPUT		
'V'  47 4			_IOWR Constant VIDIOC_S_OUTPUT		
'V'  48 v4l2_output		_IOWR Constant VIDIOC_ENUMOUTPUT	
'V'  49 v4l2_audioout		_IOR  Constant VIDIOC_G_AUDOUT		
'V'  50 v4l2_audioout		_IOW  Constant VIDIOC_S_AUDOUT		
'V'  54 v4l2_modulator		_IOWR Constant VIDIOC_G_MODULATOR	
'V'  55 v4l2_modulator		_IOW  Constant VIDIOC_S_MODULATOR	
'V'  56 v4l2_frequency		_IOWR Constant VIDIOC_G_FREQUENCY	
'V'  57 v4l2_frequency		_IOW  Constant VIDIOC_S_FREQUENCY	
'V'  58 v4l2_cropcap		_IOWR Constant VIDIOC_CROPCAP		
'V'  59 v4l2_crop		_IOWR Constant VIDIOC_G_CROP		
'V'  60 v4l2_crop		_IOW  Constant VIDIOC_S_CROP		
'V'  61 v4l2_jpegcompression	_IOR  Constant VIDIOC_G_JPEGCOMP	
'V'  62 v4l2_jpegcompression	_IOW  Constant VIDIOC_S_JPEGCOMP	
'V'  63 8			_IOR  Constant VIDIOC_QUERYSTD		
'V'  64 v4l2_format		_IOWR Constant VIDIOC_TRY_FMT		
'V'  65 v4l2_audio		_IOWR Constant VIDIOC_ENUMAUDIO	
'V'  66 v4l2_audioout		_IOWR Constant VIDIOC_ENUMAUDOUT	
'V'  67 4			_IOR  Constant VIDIOC_G_PRIORITY	 \ enum v4l2_priority
'V'  68 4			_IOW  Constant VIDIOC_S_PRIORITY	 \ enum v4l2_priority
'V'  69 v4l2_sliced_vbi_cap	_IOWR Constant VIDIOC_G_SLICED_VBI_CAP
'V'  70				_IO   Constant VIDIOC_LOG_STATUS
'V'  71 v4l2_ext_controls	_IOWR Constant VIDIOC_G_EXT_CTRLS	
'V'  72 v4l2_ext_controls	_IOWR Constant VIDIOC_S_EXT_CTRLS	
'V'  73 v4l2_ext_controls	_IOWR Constant VIDIOC_TRY_EXT_CTRLS	
'V'  74 v4l2_frmsizeenum	_IOWR Constant VIDIOC_ENUM_FRAMESIZES	
'V'  75 v4l2_frmivalenum	_IOWR Constant VIDIOC_ENUM_FRAMEINTERVALS
'V'  76 v4l2_enc_idx		_IOR  Constant VIDIOC_G_ENC_INDEX
'V'  77 v4l2_encoder_cmd	_IOWR Constant VIDIOC_ENCODER_CMD
'V'  78 v4l2_encoder_cmd	_IOWR Constant VIDIOC_TRY_ENCODER_CMD
'V'  82 v4l2_hw_freq_seek	_IOW  Constant VIDIOC_S_HW_FREQ_SEEK	
'V'  87 v4l2_dv_timings		_IOWR Constant VIDIOC_S_DV_TIMINGS
'V'  88 v4l2_dv_timings		_IOWR Constant VIDIOC_G_DV_TIMINGS
'V'  89 v4l2_event		_IOR  Constant VIDIOC_DQEVENT
'V'  90 v4l2_event_subscription	_IOW  Constant VIDIOC_SUBSCRIBE_EVENT
'V'  91 v4l2_event_subscription	_IOW  Constant VIDIOC_UNSUBSCRIBE_EVENT
'V'  92 v4l2_create_buffers	_IOWR Constant VIDIOC_CREATE_BUFS
'V'  93 v4l2_buffer		_IOWR Constant VIDIOC_PREPARE_BUF
'V'  94 v4l2_selection		_IOWR Constant VIDIOC_G_SELECTION
'V'  95 v4l2_selection		_IOWR Constant VIDIOC_S_SELECTION
'V'  96 v4l2_decoder_cmd	_IOWR Constant VIDIOC_DECODER_CMD
'V'  97 v4l2_decoder_cmd	_IOWR Constant VIDIOC_TRY_DECODER_CMD
'V'  98 v4l2_enum_dv_timings	_IOWR Constant VIDIOC_ENUM_DV_TIMINGS
'V'  99 v4l2_dv_timings		_IOR  Constant VIDIOC_QUERY_DV_TIMINGS
'V'  100 v4l2_dv_timings_cap	_IOWR Constant VIDIOC_DV_TIMINGS_CAP
'V'  101 v4l2_frequency_band	_IOWR Constant VIDIOC_ENUM_FREQ_BANDS	
'V'  103 v4l2_query_ext_ctrl	_IOWR Constant VIDIOC_QUERY_EXT_CTRL	

0 Value video-fd

: open-video ( n -- )
    [: ." /dev/video" 0 .r ;] $tmp r/w open-file throw to video-fd ;
: close-video ( -- )
    video-fd close-file  0 to video-fd  throw ;

v4l2_query_ext_ctrl buffer: query-buf
v4l2_capability     buffer: cap-buf
v4l2_fmtdesc        buffer: fmtdesc-buf
v4l2_format         buffer: fmt-buf

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

previous r> set-current
