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

[IFUNDEF] no-file#
    2 Constant ENOENT
    #-512 ENOENT - Constant no-file#
[THEN]

require gl-terminal.fs
ctx 0= [IF] window-init [THEN]

require ftgl-helper.fs
require mini-oof2.fs
require config.fs

get-current
also [IFDEF] android android [THEN]
also opengl

vocabulary minos  also minos definitions

vocabulary m2c \ minos2 config
get-current also m2c definitions
Variable cursorcolor#
Variable selectioncolor#
FVariable curminwidth%
FVariable curminchars#
set-current

$000000FF cursorcolor# !
$7FFFFF9F selectioncolor# !
3e curminwidth% f!
0 curminchars# !

previous

Variable configured?
Variable config-file$  s" ~/.minos2rc" config-file$ $!

: ?.minos-config ( -- )  true configured? !@ ?EXIT
    s" MINOS2_CONF" getenv dup IF  config-file$ $!  ELSE  2drop  THEN
    config-file$ $@ 2dup file-status nip ['] m2c >body swap
    no-file# = IF  write-config  ELSE  read-config  THEN ;

?.minos-config

$01 Constant box-hflip#
$02 Constant box-vflip#
$03 Value box-flip#

object class
    value: caller-w
    method clicked ( rx ry bmask n -- ) \ processed clicks
    method touchdown ( $rxy*n bmask -- ) \ raw click
    method touchup ( $rxy*n bmask -- ) \ raw click
    method touchmove ( $rxy*n bmask -- ) \ raw click, bmask=0 is hover
    method ukeyed ( addr u -- ) \ printable unicode characters
    method ekeyed ( ekey -- ) \ non-printable keys
    method inside? ( rx ry -- flag )
    method focus ( -- )
    method defocus ( -- )
    method show ( -- )
    method hide ( -- )
    method get ( -- something )
    method set ( something -- )
    method show-you ( -- )
end-class actor

\ dummy methods for empty actor, used for inheritance
:noname 2drop fdrop fdrop ; actor is clicked
' 2drop actor is touchdown
' 2drop actor is touchup
' 2drop actor is touchmove
' 2drop actor is ukeyed
' drop actor is ekeyed
:noname fdrop fdrop false ; actor is inside?
' noop actor is focus
' noop actor is defocus
' noop actor is show
' noop actor is hide
' noop actor is get
' noop actor is set
' noop actor is show-you

object class
    value: next-w
    value: parent-w
    value: act
    sfvalue: x
    sfvalue: y
    sfvalue: w
    sfvalue: h \ above baseline
    sfvalue: d \ below baseline
    sfvalue: border \ surrounding border
    method draw-init ( -- ) \ init draw
    method draw-bg ( -- ) \ button background draw
    method draw-icon ( -- ) \ icons draw
    method draw-thumbnail ( -- ) \ thumbnails draw
    method draw-image ( -- ) \ image draw
    method draw-text ( -- ) \ text draw
    method draw-marking ( -- ) \ draw some marking
    method hglue ( -- rtyp rsub radd )
    method dglue ( -- rtyp rsub radd )
    method vglue ( -- rtyp rsub radd )
    method hglue@ ( -- rtyp rsub radd ) \ cached variant
    method dglue@ ( -- rtyp rsub radd ) \ cached variant
    method vglue@ ( -- rtyp rsub radd ) \ cached variant
    method xywh ( -- rx0 ry0 rw rh )
    method xywhd ( -- rx ry rw rh rd )
    method resize ( rx ry rw rh rd -- )
    method !size \ set your own size
end-class widget

:noname x y h f- w h d f+ ; widget to xywh
:noname x y w h d ; widget to xywhd
' noop widget to !size
:noname w border f2* f+ 0e fdup ; widget to hglue
:noname h border f+ 0e fdup ; widget to vglue
:noname d border f+ 0e fdup ; widget to dglue
: widget-resize to d to h to w to y to x ;
' widget-resize widget to resize
' hglue widget to hglue@
' vglue widget to vglue@
' dglue widget to dglue@

: dw* ( f -- f' ) dpy-w @ fm* ;
: dh* ( f -- f' ) dpy-h @ fm* ;

tex: style-tex \ 8 x 8 subimages, each sized 128x128
style-tex 1024 dup rgba-newtex

\ glues

begin-structure glues
    dffield: glue-t \ typical size
    dffield: glue-s \ shrink by
    dffield: glue-a \ add by
end-structure

widget class
    glues +field hglue-c
    glues +field dglue-c
    glues +field vglue-c
end-class glue

: df@+ ( addr -- u addr' )  dup df@ dfloat+ ;
: df!- ( addr -- u addr' )  dup df! [ 1 dfloats ]L - ;
: glue@ ( addr -- t s a )  df@+ df@+ df@ ;
: glue! ( t s a addr -- )  [ 2 dfloats ]L + df!- df!- df! ;
:noname hglue-c glue@ ; dup glue to hglue@ glue to hglue
:noname dglue-c glue@ ; dup glue to dglue@ glue to dglue
:noname vglue-c glue@ ; dup glue to vglue@ glue to vglue

\ tile widget

widget class
    value: frame-color
    value: frame#
    value: tile-glue \ glue object
end-class tile

:noname tile-glue .hglue { f: s f: a } border f2* f+ s a ; tile to hglue
:noname tile-glue .dglue { f: s f: a } border f+ s a ; tile to dglue
:noname tile-glue .vglue { f: s f: a } border f+ s a ; tile to vglue

8 Value style-w#
8 Value style-h#

: #>st ( x y frame -- ) \ using frame#
    style-w# /mod
    s>f f+ style-w# fm/ fswap
    s>f f+ style-h# fm/ fswap >st ;

: draw-rectangle { f: x1 f: y1 f: x2 f: y2 -- }
    i? frame-color frame# >v
    x1 y2 >xy over rgba>c n> 0e 1e dup #>st v+
    x2 y2 >xy over rgba>c n> 1e 1e dup #>st v+
    x2 y1 >xy over rgba>c n> 1e 0e dup #>st v+
    x1 y1 >xy swap rgba>c n> 0e 0e     #>st v+
    v> dup i, dup 1+ i, dup 2 + i, dup i, dup 2 + i, 3 + i, ;
: >xyxy ( rx ry rw rh -- rx0 ry0 rx1 ry1 )
    { f: w f: h } fover w f+ fover h f+ ;
: tile-draw ( -- )
    xywh >xyxy draw-rectangle GL_TRIANGLES draw-elements ;

' tile-draw tile is draw-bg

\ tile that doesn't draw

tile class
end-class glue-tile

' noop tile is draw-bg

\ image widget

tile class
    defer: image-tex
end-class image

' noop       image is draw-bg
:noname ( -- )
    1-bias set-color+ image-tex
    xywh >xyxy { f: x1 f: y1 f: x2 f: y2 -- }
    i0 v0 i?  frame-color >v
    x1 y2 >xy dup rgba>c n> 0e 1e >st v+
    x2 y2 >xy dup rgba>c n> 1e 1e >st v+
    x2 y1 >xy dup rgba>c n> 1e 0e >st v+
    x1 y1 >xy     rgba>c n> 0e 0e >st v+
    v> dup i, dup 1+ i, dup 2 + i, dup i, dup 2 + i, 3 + i,
    GL_TRIANGLES draw-elements ; image is draw-image

\ frame widget

tile class
end-class frame

Create button-st  0e sf, 0.25e sf, 0.75e sf, 1e sf,
DOES>  swap sfloats + sf@ ;
: button-border ( n -- gray )  dup 2/ xor ;
: >border ( rx rb i rw -- r ) { f: w }
    button-border dup
    1 and 0= IF fdrop 0e      THEN
    2 and    IF fnegate w f+  THEN  f+ ;

: frame-draw ( -- )
    frame# frame-color border xywh { f c f: b f: x f: y f: w f: h }
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

: }}glue ( glue -- o )
    glue-tile new >o to tile-glue o o> ;
: }}frame ( glue color border -- o )
    frame new >o to border to frame-color to tile-glue o o> ;
: }}image ( glue color texture -- o )
    image new >o to image-tex to frame-color to tile-glue o o> ;

\ text widget

-5% fvalue text-shrink%
5% fvalue text-grow%

widget class
    value: text-color
    sfvalue: text-w
    $value: text$
    value: text-font
end-class text

Variable glyphs$
Variable need-glyphs

: text! ( addr u font -- )
    to text-font to text$  need-glyphs on ;
: text-text ( -- )
    x border f+ penxy sf!  y penxy sfloat+ sf!
    text-font to font  text-color color !
    w border f2* f- text-w f/ to x-scale
    text$ render-string ;
: text-!size ( -- )
    text-font to font
    text$ layout-string
    border f+ to h
    border f+ to d
    fdup to text-w  border f2* f+ to w
\    ." text sized to: " x f. y f. w f. h f. d f. cr
;
: text-init
    need-glyphs @ IF
	text-font to font text$ load-glyph$
    THEN ;
' text-init text to draw-init
' text-text text to draw-text
' text-!size text to !size
:noname text-w border f2* f+
    text-w text-shrink% f* text-w text-grow% f* ; text to hglue
:noname h 0e fdup ; text to vglue
:noname d 0e fdup ; text to dglue

\ editable text widget

text class
    value: curpos
    value: cursize
    value: start-curpos \ selection mode
end-class edit

:noname text-init  need-glyphs @ IF
	cursize 0= setstring$ $@len and IF
	    setstring$ $@ load-glyph$
	THEN
    THEN ; edit to draw-init
:noname ( -- )
    cursize 0< ?EXIT  text-font to font
    w border f2* f- text-w f/ { f: scale }
    text$ curpos umin layout-string fdrop fdrop
    scale f* { f: w }
    setstring$ $@len IF
	setstring$ $@ layout-string fdrop fdrop scale f*
    ELSE
	text$ curpos cursize m2c:curminchars# @ umax + umin
	layout-string fdrop fdrop scale f* w f-
    THEN  m2c:curminwidth% f@ fmax { f: cw }
    x w f+ border f+  y d border f- f+ { f: x0 f: y0 }
    x0 cw f+ y h border f- f- { f: x1 f: y1 }
    i? m2c:selectioncolor# m2c:cursorcolor# cursize 0> select @ >v
    x0 y1 >xy dup rgba>c n> 0e 0e >st v+
    x1 y1 >xy dup rgba>c n> 1e 0e >st v+
    x1 y0 >xy dup rgba>c n> 0e 1e >st v+
    x0 y0 >xy     rgba>c n> 1e 1e >st v+
    v> dup i, dup 1+ i, dup 2 + i, dup i, dup 2 + i, 3 + i,
; edit to draw-marking
$FFFF7FFF Value setstring-color
: edit-text ( -- )
    x border f+ penxy sf!  y penxy sfloat+ sf!
    text-font to font  text-color color !
    w border f2* f- text-w f/ to x-scale
    cursize 0= setstring$ $@len and IF
	text$ curpos umin render-string
	setstring-color color !
	setstring$ $@ render-string
	text-color color !
	text$ curpos safe/string render-string
    ELSE
	text$ render-string
    THEN ;
: edit-!size ( -- )
    text-font to font
    cursize 0= setstring$ $@len and IF
	text$ curpos umin layout-string { f: d f: h }
	setstring$ $@ layout-string
	h fmax to h d fmax to d  f+
	text$ curpos safe/string layout-string
	h fmax to h d fmax to d  f+  d h
    ELSE
	text$ layout-string
    THEN
    border f+ to h
    border f+ to d
    fdup to text-w  border f2* f+ to w ;
' edit-text edit to draw-text
' edit-!size edit to !size

: edit! ( addr u font -- )
    text!  text$ nip to curpos  -1 to cursize  -1 to start-curpos ;

\ draw wrapper

: <draw-init ( -- )
    -1e 1e >apxy
    .01e 100e 100e >ap
    0.01e 0.02e 0.15e 1.0e glClearColor
    Ambient 1 ambient% glUniform1fv
    0 to box-flip#
;
: draw-init> ( -- )
    [IFDEF]  texture_atlas_t-modified
	atlas texture_atlas_t-modified c@ IF
	    gen-atlas-tex
	    0 atlas texture_atlas_t-modified c!
	THEN
    [ELSE]
	need-glyphs @ IF  gen-atlas-tex  THEN
    [THEN]
    clear  need-glyphs off
    3 to box-flip#
;

: <draw-bg ( -- ) v0 i0
    z-bias set-color+
    program glUseProgram  style-tex ;

: <draw-icon ( -- )  ; \ icon draw, one draw call in total
: <draw-thumbnail ( -- )  ; \ icon draw, one draw call in total
: <draw-image ( -- ) ; \ image draw, one draw call per image
: draw-image> ( -- ) ;
: <draw-text ( -- )
    1-bias set-color+
    atlas-tex v0 i0 ; \ text draw, one draw call in total
: <draw-marking ( -- )
    z-bias set-color+
    none-tex v0 i0 ;

Variable style-i#

: load-style ( addr u -- n )  style-tex
    style-i# @ 8 /mod 128 * >r 128 * r> 2swap load-subtex 2drop
    style-i# @ 1 style-i# +! ;
: style: load-style Create , DOES> @ to frame# ;

"button.png" style: button1
"button2.png" style: button2
"button3.png" style: button3

\ boxes

glue class
    value: child-w
    field: box-flags
    method resized
    method map
end-class box

: do-childs { xt -- .. }
    box-flags @ box-flip# and ?EXIT
    child-w >o
    BEGIN  xt execute  next-w o>  dup  WHILE  >o  REPEAT
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
:noname ( -- ) ['] draw-marking   do-childs ; box to draw-marking

:noname ( -- )
    parent-w ?dup-IF  .resized \ upwards
    ELSE  !size xywhd resize     \ downwards
    THEN ; widget to resized

: +child ( o -- )
    child-w o 2 pick >o to parent-w to next-w o> to child-w ;
: +childs ( o1 .. on n -- ) 0 +DO  +child  LOOP ;

\ glue arithmetics

box class end-class hbox \ horizontal alignment
box class
    sfvalue: baseline \ minimun skip per line
end-class vbox \ vertical alignment
box class end-class zbox \ overlay alignment

1e20 fconstant 1fil
1e40 fconstant 1fill
1e60 fconstant 1filll

: fils ( f -- f' ) 1fil f* ;
: fills ( f -- f' ) 1fill f* ;
: fillls ( f -- f' ) 1filll f* ;

: 0glue ( -- t s a ) 0e 0e 0e ;
: 1glue ( -- t s a ) 0e 0e 1fil ;

: .fil[l[l]] ( f -- )
    fdup 1fil f< IF  f.  EXIT  THEN
    1fil f/ fdup 1fil f< IF  f. ." fil" EXIT  THEN
    1fil f/ fdup 1fil f< IF  f. ." fill" EXIT  THEN
    1fil f/ f. ." filll" ;

: .glue { f: t f: s f: a -- }
    t f. s f. a .fil[l[l]] ;
: .rec { f: x f: y f: w f: h f: d -- }
    x f. y f. w f. h f. d f. ;

glue new Constant glue*1
glue new Constant glue*2
glue*1 >o 1glue hglue-c glue! 1glue dglue-c glue! 1glue vglue-c glue! o>
glue*2 >o 1glue f2* hglue-c glue! 1glue f2* dglue-c glue! 1glue f2* vglue-c glue! o>

: g3>2 ( t s a -- min a ) fover f+ { f: a } f- a ;

: glue+ { f: t1 f: s1 f: a1 f: t2 f: s2 f: a2 -- t3 s3 a3 }
    t1 t2 f+ s1 s2 f+ a1 a2 f+ ;
: glue* { f: t1 f: s1 f: a1 f: t2 f: s2 f: a2 -- t3 s3 a3 }
    t1 t2 fmax
    t1 s1 f- t2 s2 f- fmax fover f- 0e fmax
    t1 a1 f+ t2 a2 f+ fmin 2 fpick f- 0e fmax ;
: baseglue ( -- b 0 max )
    baseline 0e 1fil ;
: glue-drop ( t s a -- ) fdrop fdrop fdrop ;

: hglue+ 0glue box-flags @ box-hflip# and ?EXIT [: hglue@ glue+ ;] do-childs ;
: dglue+ 0glue box-flags @ box-vflip# and ?EXIT [: glue-drop dglue@ ;] do-childs ; \ last dglue
: vglue+ 0glue box-flags @ box-vflip# and ?EXIT 0glue [: vglue@ glue+ baseglue glue* glue+ dglue@ ;] do-childs
    glue-drop ;

: hglue* box-flags @ box-hflip# and IF  0glue  EXIT  THEN
    1glue [: hglue@ glue* ;] do-childs ;
: dglue* box-flags @ box-hflip# and IF  0glue  EXIT  THEN
 1glue [: dglue@ glue* ;] do-childs ;
: vglue* box-flags @ box-hflip# and IF  0glue  EXIT  THEN
 1glue [: vglue@ glue* ;] do-childs ;

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

:noname defers printdebugdata cr f.s ; is printdebugdata

: hglue-step { f: gp f: ga f: rd f: rg f: rx -- gp ga rd' rg' rx' }
    gp ga  rx to x
    hglue@ g3>2 { f: xmin f: xa }
    rg xa f+ gp f* ga f/ fdup rd f- fswap rg xa f+
    frot xmin f+  fdup to w  rx f+ ;

: hbox-resize1 { f: y f: h f: d -- y h d } x y w h d resize
\    ." hchild resized: " x f. y f. w f. h f. d f. cr
    y h d ;
: hbox-resize { f: x f: y f: w f: h f: d -- }
    x y w h d widget-resize
    hglue@ g3>2 { f: wmin f: a }
    w wmin f- a 0e 0e x ['] hglue-step do-childs  fdrop fdrop fdrop fdrop fdrop
    y h d ['] hbox-resize1 do-childs  fdrop fdrop fdrop
\    ." hbox sized to: " x f. y f. w f. h f. d f. cr
;

' hbox-resize hbox is resize

\ add glues up for vboxes

: vglue-step-h { f: gp f: ga f: rd f: rg f: ry f: od -- gp ga rd' rg' ry' }
    gp ga
    vglue@ baseline od f- 0e 1fil glue* g3>2 { f: ymin f: ya }
    rg ya f+ gp f* ga f/ fdup rd f- fswap rg ya f+
    frot ymin f+  baseline od f- fmax fdup to h 
    ry f+ fdup to y ;

: vglue-step-d { f: gp f: ga f: rd f: rg f: ry -- gp ga rd' rg' ry' od' }
    gp ga
    dglue@ g3>2 { f: ymin f: ya }
    rg ya f+ gp f* ga f/ fdup rd f- fswap rg ya f+
    frot ymin f+ fdup to d 
    fdup ry f+ fswap ;

: vglue-step ( gp ga rd rg ry od -- gp ga rd' rg' ry' od )
    vglue-step-h vglue-step-d ;

: vbox-resize1 { f: x f: w -- x w } x y w h d resize
\    ." vchild resized: " x f. y f. w f. h f. d f. cr
    x w ;
: vbox-resize { f: x f: y f: w f: h f: d -- }
    x y w h d widget-resize
    vglue@ dglue@ glue+ g3>2 { f: hmin f: a }
    h border f- hmin f- a 0e 0e
    y border f+ h border f- f- 0e ['] vglue-step do-childs
    fdrop fdrop fdrop fdrop fdrop fdrop
    x border f+ w border f2* f- ['] vbox-resize1 do-childs fdrop fdrop
\    ." vbox sized to: " x f. y f. w f. h f. d f. cr
;

' vbox-resize vbox is resize

: zbox-resize1 { f: x f: y f: w f: h f: d -- x y w h d }
    x y w h d resize
\    ." zchild resized: " x f. y f. w f. h f. d f. cr
    x y w h d ;

: zbox-resize { f: x f: y f: w f: h f: d -- }
    x y w h d widget-resize
    x border f+ y border f+ w border f2* f- h border f- d border f-
    ['] zbox-resize1 do-childs
    fdrop fdrop fdrop fdrop fdrop
\    ." zbox sized to: " x f. y f. w f. h f. d f. cr
;

' zbox-resize zbox is resize

$10 stack: box-depth
: {{ ( -- ) depth box-depth >stack ;
: }} ( n1 .. nm -- n1 .. nm m ) depth box-depth stack> - ;
: }}h ( n1 .. nm -- hbox ) }} hbox new >o +childs o o> ;
: }}v ( n1 .. nm -- hbox ) }} vbox new >o +childs o o> ;
: }}z ( n1 .. nm -- hbox ) }} zbox new >o +childs o o> ;

\ draw everything

: widget-draw ( o:widget -- )
    <draw-init      draw-init      draw-init>
    <draw-bg        draw-bg        render>
    <draw-icon      draw-icon      render>
    <draw-thumbnail draw-thumbnail render>
    <draw-image     draw-image     draw-image>
    <draw-marking   draw-marking   render>
    <draw-text      draw-text      render>
    sync ;

\ viewport: Draw into a frame buffer

box class
    sfvalue: vp-xoff
    sfvalue: vp-yoff
    sfvalue: vp-w
    sfvalue: vp-h
    defer: vp-tex
end-class viewport

:noname vp-w vp-h vp-tex >framebuffer
    child-w .widget-draw
    0>framebuffer ; viewport to draw-init
:noname ( -- )
    1-bias set-color+ vp-tex
    xywh >xyxy { f: x1 f: y1 f: x2 f: y2 -- }
    x vp-w f/ y vp-h f/ w vp-w f/ h vp-h f/ { f: s0 f: t0 f: s1 f: t1 }
    i0 v0 i?  frame-color >v
    x1 y2 >xy dup rgba>c n> s0       t0 t1 f+ >st v+
    x2 y2 >xy dup rgba>c n> s0 s1 f+ t0 t1 f+ >st v+
    x2 y1 >xy dup rgba>c n> s0 s1 f+ t0       >st v+
    x1 y1 >xy     rgba>c n> s0       t0       >st v+
    v> dup i, dup 1+ i, dup 2 + i, dup i, dup 2 + i, 3 + i,
    GL_TRIANGLES draw-elements ; viewport is draw-image
' noop viewport is draw-bg
' noop viewport is draw-icon
' noop viewport is draw-thumbnail
' noop viewport is draw-marking
' noop viewport is draw-text

\ top widget and actors

0 Value top-widget

require actors.fs
require animation.fs

: htop-resize ( -- )
    !size 0e 1e dh* 1e dw* 1e dh* 0e resize ;

: widgets-loop ( -- )
    [IFDEF] hidekb  hidekb [THEN]  enter-minos
    1 level# +! top-widget .widget-draw
    BEGIN  >looper
	[IFDEF] android  ?config-changer  [THEN]
	anims[] $@len IF  animations  THEN
	need-sync @ IF
	    top-widget >o htop-resize widget-draw o>  need-sync off  THEN
	need-keyboard @ IF
	    [IFDEF] showkb showkb [THEN]
	    need-keyboard off  THEN
    level# @ 0= UNTIL  leave-minos  need-sync on ;

previous previous previous
set-current
