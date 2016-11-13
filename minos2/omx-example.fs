\ OpenMAX AL example

\ Copyright (C) 2014 Free Software Foundation, Inc.

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

require unix/openmax.fs
require mkv-tools.fs
require unix/jni-media.fs

also jni

JValue mediaplayer0
JValue media-sf

previous

also openmax

0 Value mp-object
0 Value engine
0 Value mix

"Media Player failed" exception Constant !!fail!!

: ?success ( value -- )  ]] dup XA_RESULT_SUCCESS <> IF
	~~ !!fail!! throw
    ELSE  drop  THEN [[ ; immediate

: realize ( object -- ) 0 XAObjectItf-Realize() ?success ;

: create-object ( -- )
    ['] mp-object >body 0 0 0 0 0 xaCreateEngine ?success
    mp-object realize ;

: create-engine ( -- )
    mp-object XA_IID_ENGINE ['] engine >body XAObjectItf-GetInterface()
    ?success ;

: create-mix ( -- )
    engine ['] mix >body  0 0 0 XAEngineItf-CreateOutputMix() ?success
    mix realize ;

: create-stuff ( -- )
    create-object create-engine create-mix ;

\ media player

0 Value player

8 Constant NB_BUFFERS
3 Constant NB_MAXAL_INTERFACES

Create loc_abq XA_DATALOCATOR_ANDROIDBUFFERQUEUE l, NB_BUFFERS l,
Create format_mime
XA_DATAFORMAT_MIME l,
align XA_ANDROID_MIME_MP2TS ,
XA_CONTAINERTYPE_MPEG_TS l,
Create loc_mix XA_DATALOCATOR_OUTPUTMIX l, align mix ,
Create loc_nd  XA_DATALOCATOR_NATIVEDISPLAY l, align 0 , 0 ,

Create audio>  loc_mix , 0 ,
Create video>  loc_nd  , 0 ,
Create data<   loc_abq , format_mime ,
Create iidArray XA_IID_PLAY ,
                XA_IID_ANDROIDBUFFERQUEUESOURCE ,
                XA_IID_STREAMINFORMATION ,
Create req     XA_BOOLEAN_TRUE l, XA_BOOLEAN_TRUE l, XA_BOOLEAN_TRUE l,
Create items   XA_ANDROID_ITEMKEY_DISCONTINUITY l, 0 l, 0 c,

0 Value playitf
0 Value BQitf
0 Value infoitf
0 Value volitf

188 Constant MPEG2_TS_PACKET_SIZE
128 Constant PACKETS_PER_BUFFER
MPEG2_TS_PACKET_SIZE PACKETS_PER_BUFFER * Constant BUFFER_SIZE
BUFFER_SIZE NB_BUFFERS * allocate throw Value cache

Variable bq-cb#

Defer read-ts
: read-ts-file ( addr u -- u )
    ts-fd read-file throw ;
' read-ts-file is read-ts

: buffer-queue-cb { caller cbctx bctx addr len used items ilen -- success }
    1 bq-cb# +!
    addr BUFFER_SIZE read-ts
    dup MPEG2_TS_PACKET_SIZE mod - { size }
    caller 0 addr size 0 0 XAAndroidBufferQueueItf-Enqueue() ;

' buffer-queue-cb xaAndroidBufferQueueCallback: Constant c-buffer-queue-cb

Variable domain
XAVideoStreamInformation buffer: videoinfo
Variable si-cb#
Variable eventid#

: stream-info-cb { caller eventid stream edata ctx -- success }
    1 si-cb# +! eventid eventid# !
    case eventid
	XA_STREAMCBEVENT_PROPERTYCHANGE of
	    caller stream domain
	    XAStreamInformationItf-QueryStreamType() ?success
	    case domain l@
		XA_DOMAINTYPE_VIDEO of
		    caller stream videoinfo
		    XAStreamInformationItf-QueryStreamInformation() ?success
		endof
	    endcase
	endof
    endcase
    XA_RESULT_SUCCESS ;

' stream-info-cb xaStreamEventChangeCallback: Constant c-stream-info-cb

: clear-queue ( -- ) BQitf XAAndroidBufferQueueItf-Clear() ?success ;

: init-enqueue { flag -- }
    flag IF  clear-queue  THEN
    cache BUFFER_SIZE NB_BUFFERS * read-ts
    dup MPEG2_TS_PACKET_SIZE mod - cache swap bounds U+DO
	BQitf 0 I BUFFER_SIZE I' I - umin
	flag IF  items 8  0 to flag  ELSE  0 0  THEN
	XAAndroidBufferQueueItf-Enqueue() ?success
    BUFFER_SIZE +LOOP ;

: get-interfaces ( -- )
    player XA_IID_PLAY ['] playitf >body
    XAObjectItf-GetInterface() ?success
    player XA_IID_STREAMINFORMATION ['] infoitf >body
    XAObjectItf-GetInterface() ?success
    player XA_IID_VOLUME ['] volitf >body
    XAObjectItf-GetInterface() ?success
    player XA_IID_ANDROIDBUFFERQUEUESOURCE ['] BQItf >body
    XAObjectItf-GetInterface() ?success ;

: set-callbacks ( -- )
    BQitf XA_ANDROIDBUFFERQUEUEEVENT_PROCESSED
    XAAndroidBufferQueueItf-SetCallbackEventsMask() ?success
    BQitf c-buffer-queue-cb 0
    XAAndroidBufferQueueItf-RegisterCallback() ?success
    infoitf c-stream-info-cb 0
    XAStreamInformationItf-RegisterStreamChangeCallback() ?success ;

: create-sf ( -- )
    media-sf  >o o IF  release     THEN o>
    media-sft >o o IF  st-release  THEN o>
    create-sft  media-sft new-Surface to media-sf ;

: create-media ( -- )
    new-MediaPlayer to mediaplayer0  create-sf ;

also jni also android

: create-player ( -- )
    env media-sf ANativeWindow_fromSurface loc_nd cell+ !
    mix loc_mix cell+ !
    engine ['] player >body
    data< 0 audio> video> 0 0 NB_MAXAL_INTERFACES iidArray req
    XAEngineITF-CreateMediaPlayer() ?success
    player realize ;

: destroy-player ( -- )
    player XAObjectItf-Destroy()  0 to player ;

previous previous

: init-mediaplayer ( -- )
    mediaplayer0 >o media-sf setSurface  mp-start o> ;

Variable playstate
: pplay? ( -- flag ) playitf playstate XAPlayItf-GetPlayState() ?success
    playstate @ XA_PLAYSTATE_PLAYING = ;
: ppause ( -- )
    playitf XA_PLAYSTATE_PAUSED XAPlayItf-SetPlayState() ?success ;
: pplay ( -- )
    playitf XA_PLAYSTATE_PLAYING XAPlayItf-SetPlayState() ?success ;
: pvol ( n -- )
    volitf swap XAVolumeItf-SetVolumeLevel() ?success ;

: queue-flush ( -- )
    cues>mts-run? IF
	<event ->cue-abort cue-task event>
    THEN  clear-queue ;

: setup-player ( -- )  player IF
	destroy-player
    ELSE
	create-stuff
    THEN
    create-media create-player
    get-interfaces set-callbacks
    init-mediaplayer  ;
: stop-player ( -- )  mediaplayer0 >o mp-pause o> queue-flush ;

FVariable first-timestamp  0e first-timestamp f!
FVariable prev-timestamp
0 Value pvol#

: start-file ( -- )  0e first-timestamp f!  0e mkv-time-off f!
    setup-player  true init-enqueue  ppause pvol# pvol ;

also opengl

0 Value oes-program

: omx-init ( -- ) create-oes-program to oes-program  oes-program init ;

also android

\ vertices

0.005e Fconstant m-lr

: get-deltat ( -- f )
    media-sft >o getTimestamp o> d>f 1e-9 f*
    first-timestamp f@ f- ;
: set-deltat ( -- f ) get-deltat mkv-time-off f@ f- first-timestamp f! ;

: get-m ( -- r% ) ts-fd 0= IF
	get-deltat mkv-file-o >o total-duration f@ o> f/
	EXIT  THEN
    ts-fd file-size throw ts-fd file-position throw d>f d>f f/ ;

: triangle ( c% -- )
    fdup fnegate get-m 1.1e f/ 0.05e f+ fdup f2* 1e f-
    { f: c f: -c f: r% f: rx } 
    >v
    rx m-lr f2* f- -0.96e >xy n> r% m-lr f- 0.98e >st c 0e -c 1e f>c v+
    rx             -0.9e  >xy n> r%         0.95e >st -c c 0e 1e f>c v+
    rx m-lr f2* f+ -0.96e >xy n> r% m-lr f+ 0.98e >st 0e -c c 1e f>c v+
    v> 4 i, 5 i, 6 i, ;

: rectangle ( -- )
    i>off >v
    -1e  1e >xy n> 0e 0e >st  $000000FF rgba>c v+
     1e  1e >xy n> 1e 0e >st  $000000FF rgba>c v+
     1e -1e >xy n> 1e 1e >st  $000000FF rgba>c v+
    -1e -1e >xy n> 0e 1e >st  $000000FF rgba>c v+
    v> 0 i, 1 i, 2 i, 0 i, 2 i, 3 i, ;

\ player

2Variable lastseek
500.000 2Constant delta-seek \ 0.5 seconds
5.000.000 2Constant hide-cursor

true value show-mcursor

: init-frame ( -- )
    oes-program glUseProgram
    unit-matrix MVPMatrix set-matrix
    unit-matrix MVMatrix set-matrix
    0e fdup fdup 1.0e glClearColor
    Ambient 1 ambient% glUniform1fv
    media-tex nearest-oes ;

: draw-frame ( -- )
    init-frame clear
    media-sft >o getTimestamp d>f 1e-9 f* prev-timestamp f!
    updateTexImage o>
    prev-timestamp f@ first-timestamp f@ f<> IF
	first-timestamp f@ f0=
	IF  set-deltat  THEN
	v0 i0 rectangle
	show-mcursor IF
	    hide-cursor utime lastseek 2@ d- 2over dmin d-
	    d>f hide-cursor d>f f/ triangle
	THEN
	GL_TRIANGLES draw-elements  sync
    THEN ;

: >pos ( r -- )
    ts-fd IF  >rai  ELSE
	0e first-timestamp f!
	mkv-file-o >o >cue o>
	<event elit, ->cues cue-task event>  THEN ;

: check-input ( -- )
    >looper
    ekey? IF ekey CASE
	    k-up   of pvol# 200 + 0 min dup to pvol# pvol endof 
	    k-down of pvol# 200 - dup       to pvol# pvol endof 
	ENDCASE THEN 
    *input >r r@ IF
	r@ action @ AMOTION_EVENT_ACTION_MOVE =
	r@ action @ AMOTION_EVENT_ACTION_UP = or IF
	    \ ." move/click at " r@ x0 @ . r@ y0 @ . cr
	    utime lastseek 2@ delta-seek d+ du> IF
		5 3 click-regions
		2dup 2 1 d= IF  pplay? IF
			ppause
			get-deltat mkv-time-off f! 0e first-timestamp f!
		    ELSE pplay THEN  THEN
		dup 2 = IF
		    r@ x0 @ s>f dpy-w @ fm/ 1.1e f* 0.05e f- 0e fmax 1e fmin
		    pplay? IF  ppause  THEN
		    queue-flush
		    >pos  true init-enqueue pplay
		THEN
		2drop
		utime lastseek 2!
	    THEN
	THEN  r@ action off
    THEN
    rdrop ;

: play-loop ( -- )
    hidekb >changed
    hidestatus >changed
    screen+keep pplay >changed
    omx-init init-frame 1 level# +!
    BEGIN
	?config-changer draw-frame check-input
	cues>mts-run? 0= pplay? and  IF  ppause  THEN
    level# @ 0= UNTIL
    ppause screen-keep  showstatus ;
: play-ts ( addr u -- ) ['] read-ts-file is read-ts
    open-mts start-file play-loop ;
: set-mkv ( addr u -- )
    ['] pull-queue is read-ts
    <event e$, ->open-mkv 0 elit, ->cues cue-task event> ;
: play-mkv ( addr u -- )
    set-mkv start-file play-loop stop-player ;
: replay% ( r -- )  >pos  true init-enqueue play-loop ;
: replay ( -- )
    cue-task IF
	<event 0 elit, ->cues cue-task event>
    ELSE
	0. ts-fd reposition-file throw
    THEN
    true init-enqueue play-loop stop-player ;

: gs "/storage/extSdCard/Filme/gangnamstyle.mkv" play-mkv ;
: jb "/storage/extSdCard/Filme/jb.mkv" play-mkv ;

cue-converter \ start task a bit ahead of game

previous previous previous
