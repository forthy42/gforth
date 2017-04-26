\ simple tests for widgets code

\ Copyright (C) 2014,2016 Free Software Foundation, Inc.

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

require widgets.fs

also minos

frame new value f1
frame new value f2
frame new value f3
frame new value f4
frame new value f5
frame new value f6
text new value f7
text new value f8

: !f1 ( -- ) f1 >o
    0e 0e  .25e dw*  0e  .5e dh* resize
    32e to border $FFFFFFFF to frame-color
    button2 o> ;

: !f2 ( -- ) f2 >o
    .5e dw*  0e  .5e dw*  0e  .95e dh* resize
    32e to border $FF7FFFFF to frame-color
    button3 o> ;

: !f3 ( -- ) f3 >o
    0e  .5e dh*  .5e dw*  0e  .25e dh* resize
    16e to border $FFFF7FFF to frame-color
    button1 o> ;

: !f4 ( -- ) f4 >o
    0e  .75e dh* .25e dw* 0e .2e dh* resize
    32e to border $FF7F7FFF to frame-color
    button1 o> ;

: !f5 ( -- ) f5 >o
    .25e dw* .75e dh* .25e dw* 0e .2e dh* resize
    8e to border $7FFF7FFF to frame-color
    button1 o> ;

: !f6 ( -- ) f6 >o
    .25e dw*  0e  .25e dw* 0e  .5e dh* resize
    16e to border $7FFFFFFF to frame-color
    button2 o> ;

also freetype-gl
48e FConstant fontsize#
atlas fontsize#
[IFDEF] android
    "/system/fonts/DroidSans.ttf"
[ELSE]
    "/usr/share/fonts/truetype/LiberationSans-Regular.ttf"
    2dup file-status nip [IF]
	2drop "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf"
    [THEN]
[THEN]
2dup file-status throw drop
texture_font_new_from_file Value font1

atlas fontsize#
[IFDEF] android  "/system/fonts/DroidSansFallback.ttf"
    2dup file-status nip [IF]
	2drop "/system/fonts/NotoSansSC-Regular.otf" \ for Android 6
	2dup file-status nip [IF]
	    2drop "/system/fonts/NotoSansCJK-Regular.ttc" \ for Android 7
	[THEN]
    [THEN]
[ELSE] "/usr/share/fonts/truetype/gkai00mp.ttf"
    2dup file-status nip [IF]
	2drop "/usr/share/fonts/truetype/arphic-gkai00mp/gkai00mp.ttf"
    [THEN]
[THEN]
2dup file-status throw drop
texture_font_new_from_file Value font2
previous

: !f7 ( -- )  f7 >o
    8e to x .25e dh* to y "Dös isch a Tägscht!" text-string $!
    $884400FF text-color !  font1 text-font ! o> ;

: !f8 ( -- ) f8 >o
    8e to x .625e dh* to y "这是一个文本：在德语说" text-string $!
    $004488FF text-color !  font2 text-font ! o> ;

: !widgets ( -- ) !f1 !f2 !f3 !f4 !f5 !f6 !f7 !f8 ;

: widgets-draw { xt -- }
    xt f1 .execute  xt f2 .execute  xt f3 .execute  xt f4 .execute
    xt f5 .execute  xt f6 .execute  xt f7 .execute  xt f8 .execute ;

: widgets-test
    <draw-init      ['] draw-init      widgets-draw draw-init>
    <draw-bg        ['] draw-bg        widgets-draw render>
    <draw-icon      ['] draw-icon      widgets-draw render>
    <draw-thumbnail ['] draw-thumbnail widgets-draw render>
    <draw-image     ['] draw-image     widgets-draw draw-image>
    <draw-text      ['] draw-text      widgets-draw render>
    sync ;

also [IFDEF] android android [THEN]

: widgets-demo ( -- )  [IFDEF] hidekb  hidekb [THEN]
    1 level# +!  !widgets widgets-test need-sync on
    BEGIN  >looper ?config-changer need-sync @ IF
	    !widgets widgets-test  need-sync off  THEN
    level# @ 0= UNTIL  need-sync on  need-show on screen-ops ;

previous

script? [IF] widgets-demo bye [THEN]