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
text new value t1
text new value t2
text new value t3
{{ {{
{{ f3 t2 }}z dup value z2
{{ {{ f1 t1 }}z dup value z1 f2 t3 }}h dup value h1
{{ f4 f5 }}h dup value h2
}}v dup value h3
f6 }}h Value htop

: !f1 ( -- ) f1 >o
    32e border sf! $FFFFFFFF frame-color ! glue*2 tile-glue !
    button2 o> ;

: !f2 ( -- ) f2 >o
    32e border sf! $FF7FFFFF frame-color ! glue*2 tile-glue !
    button3 o> ;

: !f3 ( -- ) f3 >o
    16e border sf! $FFFF7FFF frame-color ! glue*1 tile-glue !
    button1 o> ;

: !f4 ( -- ) f4 >o
    32e border sf! $FF7F7FFF frame-color ! glue*1 tile-glue !
    button1 o> ;

: !f5 ( -- ) f5 >o
    8e border sf! $7FFF7FFF frame-color ! glue*1 tile-glue !
    button1 o> ;

: !f6 ( -- ) f6 >o
    16e border sf! $7FFFFFFF frame-color ! glue*2 tile-glue !
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

: !t1 ( -- ) t1 >o
    "Dös isch a Tägscht!" font1 text!  32e border sf!
    $884400FF text-color ! o> ;

: !t2 ( -- ) t2 >o
    "这是一个文本：在德语说" font2 text!  32e border sf!
    $004488FF text-color ! o> ;

: !t3 ( -- ) t3 >o
    "..." font1 text!  32e border sf!
    $004488FF text-color ! o> ;

: htop-resize ( -- )
    !size 0e 1e dh* 1e dw* 1e dh* 0e resize ;
: !widgets ( -- ) !f1 !f2 !f3 !f4 !f5 !f6 !t1 !t2 !t3 htop .htop-resize ;

: widgets-test ( -- ) htop .widget-draw ;

also [IFDEF] android android [THEN]

: widgets-demo ( -- )  [IFDEF] hidekb  hidekb [THEN]
    1 level# +!  !widgets widgets-test  BEGIN  >looper
	?config-changer need-sync @ IF
	    !widgets widgets-test  need-sync off  THEN
    level# @ 0= UNTIL  need-sync on  need-show on ;

previous

script? [IF] widgets-demo bye [THEN]