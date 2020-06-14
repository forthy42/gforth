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
\ +db pulse( \ )

0 Value sles-object
0 Value sles-engine
0 Value sles-mix
0 Value sles-player
0 Value sles-recorder
0 Value play-source
0 Value play-sink
0 Value record-source
0 Value record-sink

0 Value sles-task

: ?sles-error ( ior -- )
    case
	$0 of  ( success) endof
	$1 of  true abort" SLES preconditions violated"  endof
	$2 of  true abort" SLES parameter invalid"       endof
	$3 of  true abort" SLES memory failure"          endof
	$4 of  true abort" SLES resource error"          endof
	$5 of  true abort" SLES resource lost"           endof
	$6 of  true abort" SLES io error"                endof
	$7 of  true abort" SLES buffer insufficient"     endof
	$8 of  true abort" SLES content corrupted"       endof
	$9 of  true abort" SLES content unsupported"     endof
	$A of  true abort" SLES content not found"       endof
	$B of  true abort" SLES permission denied"       endof
	$C of  true abort" SLES feature unsupported"     endof
	$D of  true abort" SLES internal error"          endof
	$E of  true abort" SLES unknown error"           endof
	$F of  true abort" SLES operation aborted"       endof
	$10 of true abort" SLES control lost"            endof
	true abort" SLES unknown error"
    endcase ;

also jni
: ?audio-permissions ( -- )
    "android.permission.RECORD_AUDIO"
    "android.permission.PLAY_AUDIO" 2 ask-permissions ;
previous

: realize ( object -- )
    0 SLObjectItf-Realize() ?sles-error ;

: create-sles ( -- )
    addr sles-object 0 0 0 0 0 slCreateEngine ?sles-error
    sles-object realize ;

: create-engine ( -- )
    sles-object SL_IID_ENGINE addr sles-engine SLObjectItf-GetInterface()
    ?sles-error ;

: create-mix ( -- )
    sles-engine addr sles-mix 0 0 0 SLEngineItf-CreateOutputMix() ?sles-error
    sles-mix realize ;

: create-player ( -- )
    { | ids[ 2 cells ] reqs[ 2 4 * ] }
    SL_IID_ANDROIDCONFIGURATION SL_IID_BUFFERQUEUE ids[ 2!
    1 reqs[ l!  1 reqs[ 4 + l!
    sles-engine addr sles-player addr play-source addr play-sink 2
    ids[ reqs[ SLEngineItf-CreateAudioPlayer() ?sles-error ;

: create-recorder ( -- )
    { | ids[ 2 cells ] reqs[ 2 4 * ] }
    SL_IID_ANDROIDCONFIGURATION SL_IID_ANDROIDSIMPLEBUFFERQUEUE ids[ 2!
    1 reqs[ l!  1 reqs[ 4 + l!
    sles-engine addr sles-recorder addr record-source addr record-sink 2
    ids[ reqs[ SLEngineItf-CreateAudioRecorder() ?sles-error ;

: sles-init ( -- )
    ?audio-permissions
    stacksize4 NewTask4 to sles-task
    sles-task activate   debug-out debug-vector !  nothrow
    [:  create-sles create-engine create-mix
	BEGIN  stop  AGAIN ;] catch ?dup-IF  DoError  THEN ;

[IFUNDEF] l,
    : l, ( n -- ) here 4 allot l! ;
[THEN]

Create PCM-format-stereo
2      l, \ format=PCM
2      l, \ channels
#48000 l, \ sample rate
#16    l, \ bits per sample
#16    l, \ container size
3      l, \ speaker mask
2      l, \ little endian

Create PCM-format-mono
2      l, \ format=PCM
1      l, \ channels
#48000 l, \ sample rate
#16    l, \ bits per sample
#16    l, \ container size
4      l, \ speaker mask
2      l, \ little endian

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
