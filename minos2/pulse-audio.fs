\ Pulse audio driver

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

require unix/pulse.fs
require unix/pthread.fs
require unix/opus.fs

get-current opus also pulse also definitions

$100 buffer: pa-error$

: ?pa-ior ( n -- )
    ?dup-IF  [: pa_strerror pa-error$ place
	    pa-error$ "error \ "
	    ! -2  throw ;] do-debug
	THEN ;

debug: pulse( \ )
+db pulse( \ )

0 Value pa-ml
0 Value pa-api
0 Value pa-ctx
0 Value pa-task
$Variable app-name
"Gforth" app-name $!
$Variable request-queue
: >request ( addr -- ) request-queue >stack ;
: requests@ ( -- addr1 .. addrn n )
    request-queue get-stack
    request-queue $free ;

' execute pa_context_notify_cb_t: Constant pa-context-notify-cb
' execute pa_server_info_cb_t: Constant pa-server-info-cb
' execute pa_source_info_cb_t: Constant pa-source-info-cb
' execute pa_sink_info_cb_t: Constant pa-sink-info-cb
' execute pa_context_subscribe_cb_t: Constant pa-context-subscribe-cb
' execute pa_context_success_cb_t: Constant pa-context-success-cb
' execute pa_stream_request_cb_t: Constant pa-stream-request-cb
:noname free throw ; pa_free_cb_t: Constant pa-free-cb

0 Value pa-ready
Variable inputs[]
Variable input-descs[]
Variable def-input$
Variable outputs[]
Variable output-descs[]
Variable def-output$

: c$@ ( addr -- addr u )
    @ cstring>sstring ;

: .channels ( addr -- )
    ." Channels["
    dup pa_channel_map-channels c@ dup 0 u.r ." ]: "
    >r pa_channel_map-map r> sfloats bounds U+DO
	I l@ .
    1 sfloats +LOOP cr ;
: +input-list { ctx inputinfo eol -- }
    inputinfo IF
	inputinfo pa_source_info-name c$@ 
	inputinfo pa_source_info-index l@ inputs[] $[]!
	inputinfo pa_source_info-description c$@
	inputinfo pa_source_info-index l@ input-descs[] $[]!
	pulse(
	." Name: " inputinfo pa_source_info-name c$@ type cr
	." Desc: " inputinfo pa_source_info-description c$@ type cr
	." Index: " inputinfo pa_source_info-index l@ . cr
	inputinfo pa_source_info-channel_map .channels )
    THEN ;
: +output-list { ctx outputinfo eol -- }
    outputinfo IF
	outputinfo pa_sink_info-name c$@
	outputinfo pa_sink_info-index l@ outputs[] $[]!
	outputinfo pa_sink_info-description c$@
	outputinfo pa_sink_info-index l@ output-descs[] $[]!
	pulse(
	." Name: " outputinfo pa_sink_info-name c$@ type cr
	." Desc: " outputinfo pa_sink_info-description c$@ type cr
	." Index: " outputinfo pa_sink_info-index l@ . cr
	outputinfo pa_sink_info-channel_map .channels )
    THEN ;
: +server-info { ctx serverinfo -- }
    serverinfo IF
	serverinfo pa_server_info-default_source_name c$@ def-input$ $!
	serverinfo pa_server_info-default_sink_name c$@   def-output$ $!
	pulse(
	." Host: " serverinfo pa_server_info-host_name c$@ type cr
	." User: " serverinfo pa_server_info-user_name c$@ type cr
	." Def.source: " serverinfo pa_server_info-default_source_name c$@ type cr
	." Def.sink: " serverinfo pa_server_info-default_sink_name c$@ type cr )
    THEN ;

: >input-list ( -- )
    pa-ctx pa-source-info-cb ['] +input-list
    pa_context_get_source_info_list >request ;
: >output-list ( -- )
    pa-ctx pa-sink-info-cb ['] +output-list
    pa_context_get_sink_info_list >request ;
: >server-info ( -- )
    pa-ctx pa-server-info-cb ['] +server-info
    pa_context_get_server_info >request ;

: pa-success { ctx success -- }
    pulse( ." success: " success . cr ) ;

: >subscribe ( -- )
    pa-ctx PA_SUBSCRIPTION_MASK_ALL pa-context-success-cb ['] pa-success
    pa_context_subscribe >request ;

: pa-notify-state { ctx -- }
    case  ctx pa_context_get_state
	PA_CONTEXT_FAILED       of  2 to pa-ready  endof
	PA_CONTEXT_TERMINATED   of  3 to pa-ready  endof
	PA_CONTEXT_READY        of  1 to pa-ready
	    >input-list >output-list >server-info >subscribe
	endof
    endcase ;

: pa-subscribe { ctx ev-t idx -- }
    case ev-t pulse( ." event type: " dup . cr )
	PA_SUBSCRIPTION_EVENT_FACILITY_MASK and
	PA_SUBSCRIPTION_EVENT_SOURCE of
	    inputs[] $[]free  input-descs[] $[]free
	    >input-list   endof
	PA_SUBSCRIPTION_EVENT_SINK   of
	    outputs[] $[]free  output-descs[] $[]free
	    >output-list  endof
	PA_SUBSCRIPTION_EVENT_SERVER of  >server-info  endof
    endcase ;

: pulse-init ( -- )
    stacksize4 NewTask4 to pa-task
    pa-task activate   debug-out debug-vector !
    [:  pa_mainloop_new to pa-ml
	pa-ml pa_mainloop_get_api to pa-api
	pa-api app-name $@ pa_context_new to pa-ctx
	pa-ctx 0 0 PA_CONTEXT_NOAUTOSPAWN 0 pa_context_connect ?pa-ior
	pa-ctx pa-context-notify-cb ['] pa-notify-state
	pa_context_set_state_callback
	pa-ctx pa-context-subscribe-cb ['] pa-subscribe
	pa_context_set_subscribe_callback
	BEGIN
	    ?events { | w^ retval }
	    pa-ml 1 retval pa_mainloop_iterate drop \ ?pa-ior
	    requests@ 0 ?DO
		dup pa_operation_get_state
		PA_OPERATION_RUNNING = IF  >request  ELSE  drop  THEN
	    LOOP
	AGAIN ;] catch DoError ;

event: :>exec ( xt -- ) execute ;
event: :>execq ( xt -- ) dup >r execute r> >addr free throw ;
: pulse-exec ( xt -- ) { xt }
    <event xt elit, :>exec  pa-task event>
    pa-ml pa_mainloop_wakeup ;
: pulse-exec# ( lit xt -- ) { xt }
    <event elit, xt elit, :>exec  pa-task event>
    pa-ml pa_mainloop_wakeup ;
: pulse-execq ( xt -- ) { xt }
    <event xt elit, :>execq pa-task event>
    pa-ml pa_mainloop_wakeup ;

: pa-sample! ( rate channels ss -- ) { ss }
    ss pa_sample_spec-channels c!
    ss pa_sample_spec-rate l!
    PA_SAMPLE_S16LE ss pa_sample_spec-format l! ;

0 Value stereo-rec
0 Value mono-rec
0 Value stereo-play
0 Value mono-play

\ Opus en/decoder

: opus-encoder ( -- encoder ) { | w^ err }
    #48000 1 OPUS_APPLICATION_VOIP err opus_encoder_create ;
: opus-decoder ( -- decoder ) { | w^ err }
    #48000 1 err opus_decoder_create ;

opus-encoder Value opus-enc
opus-decoder Value opus-dec

#100 Value frames/s

"test.opus" r/w create-file throw Value rec-file
"test.idx" r/w create-file throw Value index-file

Defer write-record

Variable write$
Variable idx$
Variable write-opus
Variable read-opus

: >amplitude ( addr u -- avgamp )
    0 -rot bounds ?DO
	I sw@ abs max
    2 +LOOP ;

\ index file for fast seeking:
\ One block per second. Block format:
\ Magic | 8b channels | 8b subblocks# | 16b samples/subblock | 64b index |
\ { 16b amplitude | 16b len }*

begin-structure idx-head
    4 +field idx-magic
    cfield: idx-channels
    cfield: idx-frames
    wfield: idx-samples
    8 +field idx-pos
end-structure

begin-structure idx-tuple
    wfield: idx-amp
    wfield: idx-len
end-structure

frames/s 4 * $10 + Constant /idx-block

: write-idx ( -- )
    idx$ $@ /idx-block umin index-file write-file throw
    idx$ $free ;
: w$+! ( value addr -- )  2 swap $+!len le-w! ;
: >idx-frame ( dpos -- )
    "Opus"   idx$ $!
    1        idx$ c$+!
    frames/s idx$ c$+!
    #480     idx$ w$+!
    8 idx$ $+!len le-xd! ;
: >idx-block ( len amp -- )
    idx$ w$+!
    idx$ w$+! ;

:noname ( addr u -- )
    write$ $+!
    BEGIN
	write$ $@len #480 2* u>= WHILE
	    #480 2* write-opus $!len
	    opus-enc write$ $@ drop #480 2*
	    2dup >amplitude >r write-opus $@ opus_encode write-opus $!len
	    idx$ $@len 0= IF
		rec-file file-position throw >idx-frame
	    THEN
	    write-opus $@len r> >idx-block
	    write-opus $@ rec-file write-file throw
	    idx$ $@len /idx-block u>= IF  write-idx  THEN
	    write$ 0 #480 2* $del
    REPEAT ; is write-record

0 Value play-file
0 Value play-idx
Variable idx-block
0 Value idx-pos#
Variable opus-out
Variable opus-buffer
#480 12 * 2 * opus-buffer $!len

: $alloc ( u string -- addr u )
    over >r $+!len r> ;

: ?opus-ior ( n -- )
    dup 0< IF  [: opus_strerror pa-error$ place
	    pa-error$ "error \ "
	    ! -2  throw ;] do-debug
    THEN ;
: read-idx-block ( -- )
    $10 idx-block $!len
    idx-block $@ play-idx read-file throw drop
    idx-block $@ drop idx-frames c@ 4 * idx-block $alloc
    play-idx read-file throw drop
    idx-block $@ drop idx-pos le-uxd@
    play-file reposition-file throw ;
: read-opus-block ( n -- )
    4 * idx-block $@ drop idx-head + + idx-len w@ read-opus $!len
    read-opus $@ play-file read-file throw drop ;
: dec-opus-block ( -- )
    opus-dec read-opus $@ opus-buffer $@ 0 opus_decode ?opus-ior
    opus-buffer $@ drop swap opus-out $+! ;

Defer read-record
:noname ( addr u -- )  2>r
    BEGIN  opus-out $@len r@ u<  WHILE
	    idx-pos# frames/s mod dup 0= IF
		read-idx-block
	    THEN
	    read-opus-block dec-opus-block
	    1 +to idx-pos#
    REPEAT
    opus-out $@ 2r@ rot umin move
    opus-out 0 r> $del rdrop
; is read-record
: open-play ( addr-play u addr-idx u -- )
    r/o open-file throw to play-idx
    r/o open-file throw to play-file ;

: record@ ( stream -- ) { | w^ data w^ n }
    dup data n pa_stream_peek ?pa-ior
    data @ n @ write-record
    n @ IF  pa_stream_drop ?pa-ior  THEN ;

: write-stream { stream bytes -- }
    stream record@ ;

: rec-buffer! ( size buffer -- )
    >r r@ pa_buffer_attr $FF fill
    frames/s / r> pa_buffer_attr-fragsize l! ;

: record-mono ( rate -- )
    { | ss[ pa_sample_spec ] cm[ pa_channel_map ] ba[ pa_buffer_attr ] }
    dup ba[ rec-buffer!
    1 ss[ pa-sample!
    pa-ctx "mono-rec" ss[ cm[ pa_channel_map_init_mono
    pa_stream_new to mono-rec
    mono-rec pa-stream-request-cb ['] write-stream
    pa_stream_set_read_callback
    mono-rec def-input$ $@ ba[ PA_STREAM_ADJUST_LATENCY
    pa_stream_connect_record ?pa-ior
    !time ;

: record-stereo ( rate -- )
    { | ss[ pa_sample_spec ] cm[ pa_channel_map ] ba[ pa_buffer_attr ] }
    dup 2* ba[ rec-buffer!
    2 ss[ pa-sample!
    pa-ctx "stereo-rec" ss[ cm[ pa_channel_map_init_stereo
    pa_stream_new to stereo-rec
    stereo-rec pa-stream-request-cb ['] write-stream
    pa_stream_set_read_callback
    stereo-rec def-input$ $@ ba[ PA_STREAM_ADJUST_LATENCY
    pa_stream_connect_record ?pa-ior
    !time ;

: record! ( stream -- ) { | w^ data w^ n }
    data @ n @
    n @ IF  pa_stream_drop ?pa-ior  THEN ;

: read-stream over { stream bytes -- }
    bytes allocate throw >r
    r@ bytes read-record
    stream r> bytes pa-free-cb #0. PA_SEEK_RELATIVE
    pa_stream_write ?pa-ior ;

: play-buffer! ( size buffer -- )
    >r r@ pa_buffer_attr $FF fill
    frames/s / r> pa_buffer_attr-tlength l! ;

: play-mono ( rate -- )
    { | ss[ pa_sample_spec ] cm[ pa_channel_map ] ba[ pa_buffer_attr ] }
    dup ba[ play-buffer!
    1 ss[ pa-sample!
    pa-ctx "mono-play" ss[ cm[ pa_channel_map_init_mono
    pa_stream_new to mono-play
    mono-play pa-stream-request-cb ['] read-stream
    pa_stream_set_write_callback
    mono-play def-output$ $@ ba[ PA_STREAM_ADJUST_LATENCY 0 0
    pa_stream_connect_playback ?pa-ior
    !time ;

: play-stereo ( rate -- )
    { | ss[ pa_sample_spec ] cm[ pa_channel_map ] ba[ pa_buffer_attr ] }
    dup 2* ba[ play-buffer!
    2 ss[ pa-sample!
    pa-ctx "stereo-play" ss[ cm[ pa_channel_map_init_stereo
    pa_stream_new to stereo-play
    stereo-play pa-stream-request-cb ['] read-stream
    pa_stream_set_write_callback
    stereo-play def-output$ $@ ba[ PA_STREAM_ADJUST_LATENCY 0 0
    pa_stream_connect_playback ?pa-ior
    !time ;

previous previous pulse set-current
