\ MINOS2 widget basis

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

\ A MINOS2 widget is composed of drawable elements, boxes and actors.
\ to make things easier, neither drawable elements nor boxes need an actor.

require gl-terminal.fs
ctx 0= [IF] window-init [THEN]

require ftgl-helper.fs
require mini-oof2.fs

get-current
also [IFDEF] android android [THEN]
also opengl

vocabulary minos  also minos definitions

0 Value layer \ drawing layer

object class
    field: caller-w
    method clicked
    method keyed
    method inside?
    method focus
    method defocus
    method show
    method hide
    method get
    method set
    method show-you
end-class actor

object class
    field: next-w
    field: parent-w
    field: x
    field: y
    field: w
    field: h \ above baseline
    field: d \ below baseline
    method draw-init ( -- ) \ init draw
    method draw-bg ( -- ) \ button background draw
    method draw-icon ( -- ) \ icons draw
    method draw-thumbnail ( -- ) \ thumbnails draw
    method draw-image ( -- ) \ image draw
    method draw-text ( -- ) \ text draw
    method hglue ( -- typ sub add )
    method dglue ( -- typ sub add )
    method vglue ( -- typ sub add )
    method hglue@ ( -- typ sub add ) \ cached variant
    method dglue@ ( -- typ sub add ) \ cached variant
    method vglue@ ( -- typ sub add ) \ cached variant
    method xywh ( -- x0 y0 w h )
    method xywhd ( -- x y w h d )
    method resize ( x y w h d -- )
    method !size \ set your own size
end-class widget

:noname x @ y @ h @ - w @ h @ d @ + ; widget to xywh
:noname x @ y @ w @ h @ d @ ; widget to xywhd
' noop widget to !size
:noname w @ 0 0 ; widget to hglue
:noname h @ 0 0 ; widget to vglue
:noname d @ 0 0 ; widget to dglue
:noname d ! h ! w ! y ! x ! ; widget to resize
' hglue widget to hglue@
' vglue widget to vglue@
' dglue widget to dglue@

tex: style-tex \ 8 x 8 subimages, each sized 128x128
style-tex 1024 dup rgba-newtex

\ glues

begin-structure glue-s
    cell +field glue-t \ typical size
    cell +field glue-s \ shrink by
    cell +field glue-a \ add by
end-structure

widget class
    3 cells +field hglue-c
    3 cells +field dglue-c
    3 cells +field vglue-c
end-class glue

: @+ ( addr -- u addr' )  dup >r @ r> cell+ ;
: !- ( addr -- u addr' )  dup >r ! r> cell- ;
: glue@ ( addr -- t s a )  @+ @+ @ ;
: glue! ( t s a addr -- )  2 cells + !- !- ! ;
:noname hglue-c glue@ ; dup glue to hglue@ glue to hglue
:noname dglue-c glue@ ; dup glue to dglue@ glue to dglue
:noname vglue-c glue@ ; dup glue to vglue@ glue to vglue

\ tile widget

widget class
    field: frame#
    field: frame-color
    field: tile-glue \ glue object
end-class tile

:noname tile-glue @ .hglue ; tile to hglue
:noname tile-glue @ .dglue ; tile to dglue
:noname tile-glue @ .vglue ; tile to vglue

8 Value style-w#
8 Value style-h#

: #>st ( x y frame -- ) \ using frame#
    style-w# /mod
    s>f f+ style-w# fm/ fswap
    s>f f+ style-h# fm/ fswap >st ;

: draw-rectangle { f: x1 f: y1 f: x2 f: y2 -- }
    i? >v
    x1 y2 >xy frame-color @ rgba>c n> 0e 1e frame# @ #>st v+
    x2 y2 >xy frame-color @ rgba>c n> 1e 1e frame# @ #>st v+
    x2 y1 >xy frame-color @ rgba>c n> 1e 0e frame# @ #>st v+
    x1 y1 >xy frame-color @ rgba>c n> 0e 0e frame# @ #>st v+
    v> dup i, dup 1+ i, dup 2 + i, dup i, dup 2 + i, 3 + i, ;
: tile-draw ( -- )
    xywh { x y w h }
    x s>f y s>f x w + s>f y h + s>f
    draw-rectangle GL_TRIANGLES draw-elements ;

' tile-draw tile is draw-bg

\ frame widget

tile class
    field: border
end-class frame

Create button-st  0e sf, 0.25e sf, 0.75e sf, 1e sf,
DOES>  swap sfloats + sf@ ;
: button-border ( n -- gray )  dup 2/ xor ;
: >border ( x b i w -- r ) >r
    button-border >r
    r@ 1 and 0= IF drop 0       THEN
    r> 2 and    IF negate r@ +  THEN  + s>f  rdrop ;

: frame-draw ( -- )
    frame# @ frame-color @ border @ xywh { f c b x y w h }
    i>off >v
    4 0 DO
	4 0 DO
	    x b I w >border  y b J h >border >xy
	    c rgba>c  n>
	    I button-st J button-st f #>st v+
	LOOP
    LOOP
    v>
    9 0  DO
	4 quad  1 I 3 mod 2 = - i-off +!
    LOOP
; ' frame-draw frame is draw-bg

\ text widget

widget class
    field: text-string
    field: text-font
    field: text-color
    field: text-border
end-class text

Variable glyphs$

: text-init ( -- )
    text-font @ to font text-string $@ glyphs$ $+! ;
: text-text ( -- )
    x @ text-border @ + s>f penxy sf!  y @ s>f penxy sfloat+ sf!
    text-font @ to font  text-color @ color !
    text-string $@ render-string ;
: text-!size ( -- )
    text-string $@ layout-string
    f>s text-border @ + d ! f>s text-border @ + h ! f>s text-border @ 2* + w ! ;
' text-init text to draw-init
' text-text text to draw-text
' text-!size text to !size

\ draw wrapper

: <draw-init ( -- )
    -1e 1e >apxy
    .01e 100e 100e >ap
    s" " glyphs$ $!
    0.01e 0.02e 0.15e 1.0e glClearColor
    Ambient 1 ambient% glUniform1fv ;
: draw-init> ( -- ) clear
    glyphs$ $@ load-glyph$ ;

: <draw-bg ( -- ) v0 i0
    z-bias set-color+
    program glUseProgram  style-tex ;

: <draw-icon ( -- )  ; \ icon draw, one draw call in total
: <draw-thumbnail ( -- )  ; \ icon draw, one draw call in total
: <draw-image ( -- )  ; \ image draw, one draw call per image
: draw-image> ( -- ) ;
: <draw-text ( -- )  <render ; \ text draw, one draw call in total

Variable style-i#

: load-style ( addr u -- n )  style-tex
    style-i# @ 8 /mod 128 * >r 128 * r> 2swap load-subtex 2drop
    style-i# @ 1 style-i# +! ;
: style: load-style Create , DOES> @ frame# ! ;

"button.png" style: button1
"button2.png" style: button2
"button3.png" style: button3

\ boxes

glue class
    field: child-w
    field: act
    method resized
    method map
end-class box

: do-childs { xt -- .. }
    child-w @ >o
    BEGIN  xt execute  next-w @ o>  dup  WHILE  >o  REPEAT
    drop ;

:noname ( -- )
    ['] !size do-childs
    hglue hglue-c glue!
    dglue dglue-c glue!
    vglue vglue-c glue! ; box to !size

:noname ( -- ) ['] draw-init      do-childs ; box to draw-init
:noname ( -- ) ['] draw-bg        do-childs ; box to draw-bg
:noname ( -- ) ['] draw-icon      do-childs ; box to draw-icon
:noname ( -- ) ['] draw-thumbnail do-childs ; box to draw-thumbnail
:noname ( -- ) ['] draw-image     do-childs ; box to draw-image
:noname ( -- ) ['] draw-text      do-childs ; box to draw-text

:noname ( -- )
    parent-w @ ?dup-IF  .resized \ upwards
    ELSE  !size xywhd resize     \ downwards
    THEN ; widget to resized

: +child ( o -- )
    child-w @ o 2 pick >o parent-w ! next-w ! o> child-w ! ;
: +childs ( o1 .. on n -- ) 0 +DO  +child  LOOP ;

\ glue arithmetics

box class end-class hbox \ horizontal alignment
box class
    field: baseline \ minimun skip per line
end-class vbox \ vertical alignment
box class end-class zbox \ overlay alignment

: 0glue ( -- t s a ) 0 0 0 ;
: 1glue ( -- t s a ) 0 0 [ -1 8 rshift ]L ; \ can have 128 1glues in a row

glue new Constant glue*1
glue new Constant glue*2
glue*1 >o 1glue hglue-c glue! 1glue dglue-c glue! 1glue vglue-c glue! o>
glue*2 >o 1glue 2* hglue-c glue! 1glue 2* dglue-c glue! 1glue 2* vglue-c glue! o>

: g3>2 ( t s a -- min a ) over + >r - r> ;

: glue+ { t1 s1 a1 t2 s2 a2 -- t3 s3 a3 }
    t1 t2 + s1 s2 + a1 a2 + ;
: glue* { t1 s1 a1 t2 s2 a2 -- t3 s3 a3 }
    t1 t2 max
    t1 s1 - t2 s2 - max over - 0 max
    t1 a1 + t2 a2 + min 2 pick - 0 max ;
: baseglue ( -- b 0 max )
    baseline @ 0 [ -1 1 rshift ]L ;
: glue-drop ( t s a -- )  2drop drop ;

: hglue+ 0glue [: hglue@ glue+ ;] do-childs ;
: dglue+ 0glue [: glue-drop dglue@ ;] do-childs ; \ last dglue
: vglue+ 0glue 0glue [: vglue@ glue+ baseglue glue* glue+ dglue@ ;] do-childs
    glue-drop ;

: hglue* 1glue [: hglue@ glue* ;] do-childs ;
: dglue* 1glue [: dglue@ glue* ;] do-childs ;
: vglue* 1glue [: vglue@ glue* ;] do-childs ;

' hglue+ hbox is hglue
' dglue* hbox is dglue
' vglue* hbox is vglue

' hglue* vbox is hglue
' dglue+ vbox is dglue
' vglue+ vbox is vglue

' hglue* zbox is hglue
' dglue* zbox is dglue
' vglue* zbox is vglue

\ add glues up for hboxes

: hglue-step { gp ga rd rg rx -- gp ga rd' rg' rx' }
    gp ga  rx x !
    hglue@ g3>2 { xmin xa }
    rg xa + gp ga */ rd - dup rd + rg xa +
    rot xmin +  dup x @ - w ! ;

: hbox-resize1 { y h d -- y h d } x @ y w @ h d resize  y h d ;
: hbox-resize { x y w h d -- }
    hglue g3>2 { wmin a }
    w wmin - a 0 0 x ['] hglue-step do-childs 2drop 2drop drop
    y h d ['] hbox-resize1 do-childs drop 2drop ;

' hbox-resize hbox is resize

\ add glues up for vboxes

: vglue-step { gp ga rd rg ry td sd ad -- gp ga rd' rg' ry' td' sd' ad' }
    gp ga baseglue
    vglue@ td sd ad glue+ glue* g3>2 { ymin ya }
    rg ya + gp ga */ rd - dup rd + rg ya +
    rot ymin baseline @ max +  dup ry !  dglue@ ;

: vbox-resize1 { x w -- x w } x y @ w h @ d @ resize  x w ;
: vbox-resize { x y w h d -- }
    vglue g3>2 { hmin a }
    h hmin - a 0 0 y 0 0 0 ['] vglue-step do-childs 2drop 2drop 2drop 2drop
    x w ['] vbox-resize1 do-childs 2drop ;

$10 stack: box-depth
: {{ ( -- ) depth box-depth >stack ;
: }} ( n1 .. nm -- n1 .. nm m ) depth box-depth stack> - ;
: }}h ( n1 .. nm -- hbox ) }} hbox new >o +childs o o> ;
: }}v ( n1 .. nm -- hbox ) }} vbox new >o +childs o o> ;
: }}z ( n1 .. nm -- hbox ) }} zbox new >o +childs o o> ;

previous previous previous set-current