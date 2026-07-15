\ video4linux2 capture window

\ Authors: Bernd Paysan
\ Copyright (C) 2026 Free Software Foundation, Inc.

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

require minos2/widgets.fs

[IFDEF] android
    also jni hidekb also android >changed hidestatus >changed previous previous
[THEN]

also minos

require minos2/font-style.fs
require minos2/text-style.fs
require minos2/presentation-support.fs
require minos2/v4l2.fs

gl-init
:noname 44e update-size# ; is rescaler
rescaler

tex: v4l2-img

: }}image-texture ( xt w h r -- o glue-o ) pixelsize# f*
    third image-tex[] >stack
    third execute
    glue new >o
    fdup fm* vglue-c df!  fm* hglue-c df!  o o> dup >r
    swap white# }}image r> ;

{{
    $000000FF $FFFFFFFF pres-frame
    {{
	glue*l }}glue
	' v4l2-img #1920 #1080 0.66e }}image-texture
	glue*l }}glue
    }}v box[]
}}z box[]
to top-widget

also v4l2

: redisplay-image ( addr u index -- ) >r
    v4l2-img img>mem >texture +sync
    r> bg-queue ;

script? [IF]
    0 open-video
    start-capture start-streaming
    ' redisplay-image bg-capture
    presentation bye
[THEN]

previous
