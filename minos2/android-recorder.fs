\ video recorder, can be used to capture a video stream
\ or just the preview image

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

require minos2/gl-helper.fs
require unix/jni-media.fs

also opengl
also android
also jni

0 Value camera
0 Value parameters
0 Value recorder
0 Value cam-h
0 Value cam-w

0 Value oes-program
0 Value media-sf

: oes-init create-oes-program to oes-program ;

: rot>st ( n -- n' )  dup 2 and 2/ s>f dup 1- 2 and 2/ s>f >st 1+ ;

: cam-rectangle ( orientation -- )
    >v
    -1e  1e >xy n> rot>st  $FFFFFF00 rgba>c v+
     1e  1e >xy n> rot>st  $FFFFFF00 rgba>c v+
     1e -1e >xy n> rot>st  $FFFFFF00 rgba>c v+
    -1e -1e >xy n> rot>st  $FFFFFF00 rgba>c v+
    v>  drop  0 i, 1 i, 2 i, 0 i, 2 i, 3 i, ;

: camera-init ( -- )
    oes-program init
    unit-matrix MVPMatrix set-matrix
    unit-matrix MVMatrix set-matrix
    media-sft >o updateTexImage o>
    0e fdup fdup 1.0e glClearColor clear
    Ambient 1 ambient% glUniform1fv
    media-tex nearest-oes ;
: camera-frame ( -- ) camera-init
    v0 i0 screen-orientation cam-rectangle
    GL_TRIANGLES draw-elements ;

: max-area ( w h o:size -- w' h' )
    2dup m* width height m* d<  IF  2drop  width height  THEN ;

: max-size ( o:list -- w h )
    0 0 l-size 0 ?DO  I l-get >o width . height . cr max-area o>  LOOP ;

: create-camera ( -- )
    camera 0= IF  c-open-back to camera  THEN
    camera >o getParameters to parameters
      parameters >o
        ." Preview size:" cr
        getPreferredPreviewSizeForVideo >o width height o>
        2dup swap . . cr setPreviewSize
        ." Video sizes:" cr
        getSupportedVideoSizes >o max-size o> to cam-h to cam-w
        js" continuous-picture" setFocusMode
      o o>
    setParameters o> ;

: cam-prepare ( -- ) hidekb screen+keep create-camera create-sft
\    media-sft new-Surface to media-sf
    camera >o media-sft setPreviewTexture startPreview o>
    oes-init ;
: cam-end ( -- ) camera >o stopPreview c-release o> screen-keep ;

: record-prepare ( -- )
    recorder 0= IF  new-MediaRecorder to recorder  THEN
    recorder >o
    camera >o c-unlock o>
    camera setCamera
\    media-sf setPreviewDisplay
    5 setAudioSource  \ Camcorder
    1 setVideoSource  \ Camera

    1 cp-get setProfile \ high profile

    js" /storage/extSdCard/Filme/test.mp4" setOutputFile
    mr-prepare
    o> ;

: start-record ( -- )
    recorder >o mr-start o> ;

: stop-record ( -- )
    recorder >o mr-stop mr-release o> camera >o c-lock o> ;

: capture-size ( -- d )
    "/storage/extSdCard/Filme/test.mp4" r/o open-file throw >r
    r@ file-size throw r> close-file throw ;

: cam-loop ( -- )
    1 level# +!  BEGIN camera-frame sync >looper
	capture-size d. cr
    level# @ 0= UNTIL ;

: camera-test ( -- )
    cam-prepare camera-frame sync >looper ." First camera frame" cr
    record-prepare camera-frame sync >looper ." Second camera frame" cr
    start-record ." Start Loop" cr
    cam-loop ." Loop done" cr
    stop-record ." Stop recording" cr
    cam-end ;

previous previous previous