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

0 Value sles-mono-player
0 Value sles-mono-playerq
0 Value sles-mono-play
0 Value sles-mono-playvol

0 Value sles-mono-recorder
0 Value sles-mono-recorderq
0 Value sles-mono-record

0 Value sles-stereo-player
0 Value sles-stereo-playerq
0 Value sles-stereo-play
0 Value sles-stereo-playvol

0 Value sles-stereo-recorder
0 Value sles-stereo-recorderq
0 Value sles-stereo-record

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

Sema sles-sema

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

: destroy-mix ( -- )
    sles-mix SLObjectITF-Destroy()  0 to sles-mix ;

: destroy-engine ( -- )
    sles-engine SLObjectITF-Destroy()  0 to sles-engine ;

#48000 Value sample-rate
#960 Value samples/frame

: mHz ( hz -- mhz ) #1000 * ;

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
sample-rate mHz l, \ sample rate in mHz
#16    l, \ bits per sample
#16    l, \ container size
3      l, \ speaker mask
2      l, \ little endian

Create PCM-format-mono
2      l, \ format=PCM
1      l, \ channels
sample-rate mHz l, \ sample rate in mHz
#16    l, \ bits per sample
#16    l, \ container size
4      l, \ speaker mask
2      l, \ little endian

: drain-play  ( -- )
    sles-play ?dup-IF  1 SLPlayItf-SetPlayState() ?sles-ior  THEN ;
: pause-play  ( -- )
    sles-play ?dup-IF  2 SLPlayItf-SetPlayState() ?sles-ior  THEN ;
: resume-play ( -- )
    sles-play ?dup-IF  3 SLPlayItf-SetPlayState() ?sles-ior  THEN ;

: drain-record  ( -- )
    sles-record ?dup-IF  1 SLRecordItf-SetRecordState() ?sles-ior  THEN ;
: pause-record  ( -- )
    sles-record ?dup-IF  2 SLRecordItf-SetRecordState() ?sles-ior  THEN ;
: resume-record ( -- )
    sles-record ?dup-IF  3 SLRecordItf-SetRecordState() ?sles-ior  THEN ;

: set-vol ( volume playvol -- ) swap SLVolumeItf-SetVolumeLevel() ?sles-ior ;
: get-vol ( playvol -- volume ) { | w^ volume }
    volume SLVolumeItf-GetVolumeLevel() ?sles-ior volume l@ ;

: read-stream { queue xt: read-record -- }
    read-record { w^ buf }
    BEGIN  buf $@len 0=  WHILE
	    pause \ give the other task a chance to do something
	    read-record { w^ buf2 }  buf2 $@len  WHILE
		buf2 $@ buf $+!  buf2 $free
	REPEAT  buf2 $free  THEN
    buf $@len IF
	queue buf $@ [: SLBufferQueueItf-Enqueue() ?sles-ior ;]
	sles-sema c-section
    ELSE
	['] pause-play sles-sema c-section
    THEN ;

Variable stream-bufs<>

: +stereo-buf { queue | w^ buf -- }
    samples/frame 2* 2* buf $!len
    queue buf $@ [: SLBufferQueueItf-Enqueue() ?sles-ior ;]
    sles-sema c-section
    buf @ stream-bufs<> >back ;

: write-stream { queue xt: write-record -- }
    stream-bufs<> stack> { w^ buf } buf $@ write-record buf $free
    queue +stereo-buf ;

\ in OpenSL ES, a player is an object.
\ It has a Play interface, a Bufferqueue interface, and a Volume interface
\ You can start/stop/pause the Play interface
\ You can write to the Bufferqueue interface
\ and you can set the volume on the volume interface

: create-player ( format rd player play playerq playvol -- )
    { rd player play playerq playvol | ids[ 2 cells ] mix[ 2 cells ] reqs[ 2 sfloats ] src[ 2 cells ] snk[ 2 cells ] }
    SL_IID_BUFFERQUEUE SL_IID_VOLUME ids[ 2!
    1 reqs[ l!  1 reqs[ sfloat+ l!
    loc-bufq src[ 2!
    sles-mix 0x00000004 ( SL_DATALOCATOR_OUTPUTMIX ) mix[ 2!
    0 mix[ snk[ 2!
    sles-engine player  src[  snk[
    2 ids[ reqs[ SLEngineItf-CreateAudioPlayer() ?sles-ior
    player @ realize
    player @ SL_IID_PLAY play
    SLObjectItf-GetInterface() ?sles-ior
    player @ SL_IID_BUFFERQUEUE playerq
    SLObjectItf-GetInterface() ?sles-ior
    player @ SL_IID_VOLUME playvol
    SLObjectItf-GetInterface() ?sles-ior
    playerq @ \ buffer-queue-cb rd [{: rd :}h rd read-stream ;]
    simple-buffer-cb rd
    SLBufferQueueItf-RegisterCallback() ?sles-ior ;

: destroy-player ( player -- )
    ?dup-IF
	SLObjectITF-Destroy()
    THEN ;

: mono-srate! ( rate -- )
    mhz PCM-format-mono 2 sfloats + l! ;
: stereo-srate! ( rate -- )
    mhz PCM-format-stereo 2 sfloats + l! ;

: play-mono ( rate read-record read-init -- ) >r
    pause-play
    sles-mono-player 0= IF
	swap mono-srate!
	PCM-format-mono swap
	addr sles-mono-player addr sles-mono-play
	addr sles-mono-playerq addr sles-mono-playvol create-player
    ELSE
	nip nip
    THEN
    5 ms sles-mono-playerq r> read-stream
    sles-mono-player    to sles-player
    sles-mono-play      to sles-play
    sles-mono-playerq   to sles-playerq
    sles-mono-playvol   to sles-playvol
    resume-play ;

: play-stereo ( rate read-record read-init -- ) >r
    pause-play
    sles-stereo-player 0= IF
	swap stereo-srate!
	PCM-format-stereo swap
	addr sles-stereo-player addr sles-stereo-play
	addr sles-stereo-playerq addr sles-stereo-playvol create-player
    ELSE
	nip nip
    THEN
    5 ms sles-stereo-playerq r> read-stream
    sles-stereo-player    to sles-player
    sles-stereo-play      to sles-play
    sles-stereo-playerq   to sles-playerq
    sles-stereo-playvol   to sles-playvol
    resume-play ;

: create-recorder ( format wr recorder record recorderq -- )
    { wr recorder record recorderq | ids[ 1 cells ] reqs[ 1 sfloats ] src[ 2 cells ] snk[ 2 cells ] }
    SL_IID_ANDROIDSIMPLEBUFFERQUEUE ids[ !
    1 reqs[ l!
    0 loc-dev src[ 2!
    loc-bufq snk[ 2!
    sles-engine recorder  src[  snk[
    1 ids[ reqs[ SLEngineItf-CreateAudioRecorder() ?sles-ior
    recorder @ realize
    recorder @ SL_IID_RECORD record
    SLObjectItf-GetInterface() ?sles-ior
    recorder @ SL_IID_ANDROIDSIMPLEBUFFERQUEUE recorderq
    SLObjectItf-GetInterface() ?sles-ior
    sles-recorderq @ buffer-queue-cb wr [{: wr :}h wr write-stream ;]
    SLBufferQueueItf-RegisterCallback() ?sles-ior
    4 0 DO  sles-recorderq @ +stereo-buf  LOOP
    record @ 3 SLRecordItf-SetRecordState() ?sles-ior ;

: destroy-recorder ( -- )
    sles-recorder ?dup-IF
	SLObjectITF-Destroy()
	0 to sles-recorder
	0 to sles-record
	0 to sles-recorderq
    THEN ;

: record-mono ( rate write-record -- )
    drain-record
    sles-mono-recorder 0= IF
	swap mono-srate!
	PCM-format-mono swap create-recorder
    ELSE  2drop  THEN
    sles-mono-recorder    to sles-recorder
    sles-mono-record      to sles-record
    sles-mono-recorderq   to sles-recorderq ;

: record-stereo ( rate write-record -- )
    drain-record
    sles-mono-recorder 0= IF
	swap stereo-srate!
	PCM-format-stereo swap create-recorder
    ELSE  2drop  THEN
    sles-stereo-recorder    to sles-recorder
    sles-stereo-record      to sles-record
    sles-stereo-recorderq   to sles-recorderq ;

: sles-init ( -- )
    ?audio-permissions
    create-sles create-engine create-mix ;

set-current
previous opensles

0 warnings !@
: bye ( -- )
    destroy-player destroy-recorder destroy-mix destroy-engine  bye ;
warnings !
