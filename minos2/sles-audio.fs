\ OpenSLES audio driver

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

require unix/opensles.fs
require unix/pthread.fs

get-current opensles also definitions

debug: sles( \ )
\ +db sles( \ )

0 Value sles-object
0 Value sles-engine
0 Value sles-mix
0 Value sles-player
0 Value sles-playerq
0 Value sles-play
0 Value sles-playvol
0 Value sles-recorder
0 Value sles-recorderq
0 Value sles-record

0 Value sles-task

"SLES preconditions violated" exception 1+ >r
"SLES parameter invalid"      exception drop
"SLES memory failure"         exception drop
"SLES resource error"         exception drop
"SLES resource lost"          exception drop
"SLES io error"               exception drop
"SLES buffer insufficient"    exception drop
"SLES content corrupted"      exception drop
"SLES content unsupported"    exception drop
"SLES content not found"      exception drop
"SLES permission denied"      exception drop
"SLES feature unsupported"    exception drop
"SLES internal error"         exception drop
"SLES unknown error"          exception drop
"SLES operation aborted"      exception drop
"SLES control lost"           exception drop

: ?sles-ior ( ior -- )
    ?dup-IF  [ r> ]L swap - throw  THEN ;

: ev-exec ( caller pcontext event -- )  swap execute ;
: ch-exec ( caller pcontext deviceid numinputs isnew -- ) 3 roll execute ;

' execute slBufferQueueCallback: Constant buffer-queue-cb
' ev-exec slPlayCallback: Constant play-cb
' ev-exec slRecordCallback: Constant record-cb
' ev-exec slPrefetchCallback: Constant prefetch-cb

also jni
: ?audio-permissions ( -- )
    "android.permission.RECORD_AUDIO"
    "android.permission.PLAY_AUDIO" 2 ask-permissions ;
previous

: realize ( object -- )
    0 SLObjectItf-Realize() ?sles-ior ;

: create-sles ( -- )
    addr sles-object 0 0 0 0 0 slCreateEngine ?sles-ior
    sles-object realize ;

: create-engine ( -- )
    sles-object SL_IID_ENGINE addr sles-engine SLObjectItf-GetInterface()
    ?sles-ior ;

: create-mix ( -- )
    sles-engine addr sles-mix 0 0 0 SLEngineItf-CreateOutputMix() ?sles-ior
    sles-mix realize ;

#48000 Value sample-rate
#960 Value samples/frame

Create loc-bufq \ for a buffer
0x800007BD ( SL_DATALOCATOR_ANDROIDSIMPLEBUFFERQUEUE ) l,
2 l,
Create loc-dev  \ for a device
0x00000003 ( SL_DATALOCATOR_IODEVICE ) l,
0x00000001 ( SL_IODEVICE_AUDIOINPUT ) l,
0xFFFFFFFF ( SL_DEFAULTDEVICEID_AUDIOINPUT ) l,
align 0 ,

Create PCM-format-stereo
2      l, \ format=PCM
2      l, \ channels
sample-rate #1000 * l, \ sample rate in mHz
#16    l, \ bits per sample
#16    l, \ container size
3      l, \ speaker mask
2      l, \ little endian

Create PCM-format-mono
2      l, \ format=PCM
1      l, \ channels
sample-rate #1000 * l, \ sample rate in mHz
#16    l, \ bits per sample
#16    l, \ container size
4      l, \ speaker mask
2      l, \ little endian

: drain-stream  ( stream -- ) 1 SLPlayItf-SetPlayState() ?sles-ior ;
: pause-stream  ( stream -- ) 2 SLPlayItf-SetPlayState() ?sles-ior ;
: resume-stream ( stream -- ) 3 SLPlayItf-SetPlayState() ?sles-ior ;

: read-stream { stream xt: read-record -- }
    read-record { w^ buf }
    BEGIN  buf $@len 0=  WHILE
	    pause \ give the other task a chance to do something
	    read-record { w^ buf2 }  buf2 $@len  WHILE
		buf2 $@ buf $+!  buf2 $free
    REPEAT  THEN
    buf $@len IF
	stream buf $@ SLBufferQueueItf-Enqueue() ?sles-ior
    ELSE
	stream pause-stream
    THEN ;

Variable stream-bufs<>

: +stereo-buf { stream | w^ buf -- }
    samples/frame 2* 2* buf $!len
    stream buf $@ SLBufferQueueItf-Enqueue() ?sles-ior
    buf @ stream-bufs<> >back ;

: write-stream { stream xt: write-record -- }
    stream-bufs<> stack> { w^ buf } buf $@ write-record buf $free
    stream +stereo-buf ;

: create-player ( format rd -- )
    { rd | ids[ 2 cells ] mix[ 2 cells ] reqs[ 2 sfloats ] src[ 2 cells ] snk[ 2 cells ] }
    SL_IID_BUFFERQUEUE SL_IID_VOLUME ids[ 2!
    1 reqs[ l!  1 reqs[ sfloat+ l!
    loc-bufq src[ 2!
    sles-mix 0x00000004 ( SL_DATALOCATOR_OUTPUTMIX ) mix[ 2!
    0 mix[ snk[ 2!
    sles-engine addr sles-player  src[  snk[
    2 ids[ reqs[ SLEngineItf-CreateAudioPlayer() ?sles-ior
    sles-player realize
    sles-player SL_IID_PLAY addr sles-play
    SLObjectItf-GetInterface() ?sles-ior
    sles-player SL_IID_BUFFERQUEUE addr sles-playerq
    SLObjectItf-GetInterface() ?sles-ior
    sles-player SL_IID_VOLUME addr sles-playvol
    SLObjectItf-GetInterface() ?sles-ior
    sles-playerq buffer-queue-cb rd [{: rd :}h rd read-stream ;]
    SLBufferQueueItf-RegisterCallback() ?sles-ior ;

: create-recorder ( format wr -- )
    { wr | ids[ 1 cells ] reqs[ 1 sfloats ] src[ 2 cells ] snk[ 2 cells ] }
    SL_IID_ANDROIDSIMPLEBUFFERQUEUE ids[ !
    1 reqs[ l!
    0 loc-dev src[ 2!
    loc-bufq snk[ 2!
    sles-engine addr sles-recorder  src[  snk[
    1 ids[ reqs[ SLEngineItf-CreateAudioRecorder() ?sles-ior
    sles-recorder realize
    sles-recorder SL_IID_RECORD addr sles-record
    SLObjectItf-GetInterface() ?sles-ior
    sles-recorder SL_IID_ANDROIDSIMPLEBUFFERQUEUE addr sles-recorderq
    SLObjectItf-GetInterface() ?sles-ior
    sles-recorderq buffer-queue-cb wr [{: wr :}h wr write-stream ;]
    SLBufferQueueItf-RegisterCallback() ?sles-ior ;

: sles-init ( -- )
    ?audio-permissions
    stacksize4 NewTask4 to sles-task
    sles-task activate   debug-out debug-vector !  nothrow
    [:  create-sles create-engine create-mix
	BEGIN  stop  AGAIN ;] catch ?dup-IF  DoError  THEN ;

event: :>kill-sles ( -- )
    0 to sles-task  kill-task ;

: kill-sles ( -- )
    sles-task IF
	<event :>kill-sles sles-task event>
	5 0 DO  sles-task 0= ?LEAVE  1 ms  LOOP
    THEN ;

set-current
previous opensles

0 warnings !@
: bye ( -- )
    kill-sles bye ;
warnings !
