\ simple tests for widgets code

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
    0 0  dpy-w @ 4 /  0  dpy-h @ 2/ resize
    32 border ! $FFFFFFFF frame-color !
    button2 o> ;

: !f2 ( -- ) f2 >o
    dpy-w @ 2/  0  dpy-w @ 2/  0  dpy-h @ 19 20 */ resize
    32 border ! $FF7FFFFF frame-color !
    button3 o> ;

: !f3 ( -- ) f3 >o
    0  dpy-h @ 2/  dpy-w @ 2/  0  dpy-h @ 2/ 2/ resize
    16 border ! $FFFF7FFF frame-color !
    button1 o> ;

: !f4 ( -- ) f4 >o
    0  dpy-h @ 3 4 */  dpy-w @ 4 /  0  dpy-h @ 5 / resize
    32 border ! $FF7F7FFF frame-color !
    button1 o> ;

: !f5 ( -- ) f5 >o
    dpy-w @ 4 /  dpy-h @ 3 4 */  dpy-w @ 4 /  0  dpy-h @ 5 / resize
    8 border ! $7FFF7FFF frame-color !
    button1 o> ;

: !f6 ( -- ) f6 >o
    dpy-w @ 4 /  0  dpy-w @ 4 /  0  dpy-h @ 2/ resize
    16 border ! $7FFFFFFF frame-color !
    button2 o> ;

also freetype-gl
48e FConstant fontsize#
atlas fontsize#
[IFDEF] android  "/system/fonts/DroidSans.ttf"
[ELSE] "/usr/share/fonts/truetype/LiberationSans-Regular.ttf" 2dup file-status nip [IF]
	2drop
	"/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf"
    [THEN]
[THEN]
texture_font_new_from_file Value font1

atlas fontsize#
[IFDEF] android  "/system/fonts/DroidSansFallback.ttf"
[ELSE] "/usr/share/fonts/truetype/gkai00mp.ttf" 2dup file-status nip [IF]
	2drop
	"/usr/share/fonts/truetype/arphic-gkai00mp/gkai00mp.ttf"
    [THEN]
[THEN]
texture_font_new_from_file Value font2
previous

: !f7 ( -- )  f7 >o
    8 x ! dpy-h @ 4 / y ! "Dös isch a Tägscht!" text-string $!
    $884400FF text-color !  font1 text-font ! o> ;

: !f8 ( -- ) f8 >o
    8 x ! dpy-h @ 5 8 */ y ! "这是一个文本：在德语说" text-string $!
    $004488FF text-color !  font2 text-font ! o> ;

: !widgets ( -- ) !f1 !f2 !f3 !f4 !f5 !f6 !f7 !f8 ;

: widgets-draw { xt -- }
    f1 >o xt execute o> f7 >o xt execute o>
    f2 >o xt execute o> f3 >o xt execute o>
    f8 >o xt execute o> f4 >o xt execute o>
    f5 >o xt execute o> f6 >o xt execute o> ;

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
    1 level# +!  !widgets  BEGIN  widgets-test >looper
	?config-changer need-sync @ IF  !widgets  need-sync off  THEN
    level# @ 0= UNTIL  need-sync on ;

previous

script? [IF] widgets-demo bye [THEN]