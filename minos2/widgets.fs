\ MINOS2 widget basis

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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

require ../i18n.fs \ localization
require ../mini-oof2.fs
require ../config.fs
require gl-terminal.fs
require ftgl-helper.fs

[IFDEF] <provides <provides [THEN]

[IFUNDEF] no-file#
    2 Constant ENOENT
    #-512 ENOENT - Constant no-file#
[THEN]

debug: gui(     \ +db gui( \ )
debug: click(   \ +db click( \ )
debug: click-o( \ +db click-o( \ )
debug: resize(  \ +db resize( \ )
debug: dispose( \ +db dispose( \ )
debug: event(   \ +db event( \ )
debug: dnd(

Variable minos2-debug$
${MINOS2_DEBUGS} minos2-debug$ $!
minos2-debug$ ','
:noname ( addr u -- )
    [: type '(' emit ;] $tmp find-name ?dup-IF
	dup >does-code ['] debug-doer =
	IF  >body on  ELSE  drop  THEN
    THEN
; $iter

get-current
also [IFDEF] android android [THEN]
also opengl

: #Variable ( init -- )  Create , ;

vocabulary minos  also minos definitions

0e FValue x-color
: color> ( f -- ) f,  DOES> f@ to x-color ;
: color: ( rgba "name" -- ) \ minos2
    \G Create a (possibly shared) color index initialized with @var{rgba}
    Create color,                 color> ;
: new-color: ( rgba "name" -- ) \ minos2
    \G Create a unique color index initialized with @var{rgba}
    Create new-color,             color> ;
: text-color: ( rgba "name" -- ) \ minos2
    \G Create a unique text color index initialized with @var{rgba},
    \G the corresponding emoji color is set to white.
    Create text-color,            color> ;
: text-emoji-color: ( rgbatext rgbaemoji "name" -- ) \ minos2
    \G Create a unique text color index initialized with @var{rgbatext},
    \G the corresponding emoji color is set to @var{rgbaemoji}.
    Create text-emoji-color,      color> ;
: fade-color: ( rgba1 rgba2 "name" -- ) \ minos2
    \G Create a unique pair of text color index initialized with @var{rgba1}
    \G and @var{rgba2}, the corresponding emoji color is set to white. By
    \G slowly shifting the index from one to the next index, the object will
    \G shift its color using a linear interpolation when redrawn.
    Create fade-color,            color> ;
: text-emoji-fade-color: ( rgbatext1 ~2 rgbaemoji1 ~2 "name" -- ) \ minos2
    \G Create a unique pair of text color index initialized with
    \G @var{rgbatext1} and @var{~2}, the corresponding emoji color pair is set
    \G to @var{rgbaemoji1} to @var{~2}. By slowly shifting the index from one
    \G to the next index, the object will shift its color using a linear
    \G interpolation when redrawn.
    Create text-emoji-fade-color, color> ;

: (re-color) ( "name" xt -- )
    color,# >r
    ' >body f@ floor f>s to color,#  execute fdrop
    r> to color,# ;
: re-color ( rgba "name" -- ) \ minos2
    \G assign the named color index @var{"name"} in the current color scheme
    \G with the value @var{rgba}.
    ['] new-color, (re-color) ;
: re-text-color ( rgba "name" -- ) \ minos2
    \G assign the named text color index @var{"name"} in the current color
    \G scheme with the value @var{rgba}.
    ['] text-color, (re-color) ;
: re-emoji-color ( rgbatext rgbaemoji "name" -- ) \ minos2
    \G assign the named text and emoji color index @var{"name"} in the current
    \G color scheme with the value @var{rgbatext} and @var{rgbaemoji}.
   ['] text-emoji-color, (re-color) ;
: re-fade-color ( rgba1 rgba2 "name" -- ) \ minos2
    \G assign the named color index pair @var{"name"} in the current color
    \G scheme with the value @var{rgba1} and @var{rgba2}.
    ['] fade-color, (re-color) ;
: re-text-emoji-fade-color ( rgbatext1 ~2 rgbaemoji1 ~2 "name" -- ) \ minos2
    \G assign the named color index pair @var{"name"} in the current color
    \G scheme with the value @var{rgbatext1} and @var{~2}
    \G resp. @var{rgbaemoji1} and @var{~2}.
    ['] text-emoji-fade-color, (re-color) ;

vocabulary m2c \ minos2 config
get-current also m2c definitions

$000000FF #Variable cursorcolor#
$3F7FFF7F #Variable selectioncolor#
$FFFF7FFF #Variable setstring-color#
$1010107F #Variable shadow-color#
Variable curminchars#
Variable twoclicks#
Variable samepos#
FVariable curminwidth%
FVariable pwtime%
FVariable scale%
FVariable animtime%
$Variable translate$
set-current

0 curminchars# !
#200 twoclicks# !  \ every edge further apart than 200ms into separate clicks
#6 samepos# !      \ position difference square-summed less than is same pos
1e curminwidth% f!
0.5e pwtime% f!
1e scale% f!
0.3e animtime% f!

: twoclicks  twoclicks# @ ;
: samepos    samepos# @ ;

previous

Variable configured?
Variable config-file$  s" ~/.config/minos2rc" config-file$ $!

[IFUNDEF] !wrapper
    : !wrapper ( val addr xt -- .. ) { a xt -- .. }
	a !@ >r xt catch r> a ! throw ;
[THEN]

: ?.minos-config ( -- )  true configured? !@ ?EXIT
    ${XDG_CONFIG_HOME} dup IF
	config-file$ $! s" /minos2rc" config-file$ $+!  ELSE  2drop  THEN
    ${MINOS2_CONF} dup IF  config-file$ $!  ELSE  2drop  THEN
    config-file$ $@ 2dup file-status nip ['] m2c >wordlist swap
    no-file# = IF  write-config  ELSE
	config-throw >r  0 to config-throw
	['] read-config catch  r> to config-throw  throw
    THEN
    m2c:translate$ $@ ['] translators >wordlist find-name-in
    dup 0= IF  drop ['] noop  THEN  is translator ;

?.minos-config

\ helper for languages and splitting texts
\ cjk and emoji can be split at any letter

$[]Variable ranges>lang[]

: +range ( font# end start -- ) \ minos2
    \G Add a \var{font#} to a range from @var{start} to (excluding}
    \G @var{end} to the list of ranges.
    U+DO
	I 8 rshift ranges>lang[] $[] { range }
	I $FF and 0= delta-i $100 u>= and IF
	    range @ $100 u>= IF  range $free  THEN
	    dup range !
	ELSE
	    range @ 1+ $100 u<= IF
		{ | zeros[ $100 ] }
		zeros[ $100 2dup range @ fill
		range off  range $!
	    THEN
	    range $@ I $FF and /string delta-i umin third fill
	THEN
    $100 I $FF and - +LOOP  drop ;
: range@ ( codepoint -- font# ) \ minos2
    \G Get the @var{font#} for a specific @var{codepoint}.
    \G -1 as result means the font is meaningless
    dup >r 8 rshift ranges>lang[] $[]
    dup @ dup 1+ $100 u<= IF  nip rdrop  EXIT  THEN
    drop $@ drop r> $FF and + c@ dup $FF = or ;

-1 $00370 $00300 +range
-1 $01B00 $01AB0 +range
-1 $01E00 $01DC0 +range
-1 $02010 $02000 +range
-1 $02070 $0205F +range
-1 $02100 $020D0 +range
-1 $0FE10 $0FE00 +range
-1 $0FE30 $0FE20 +range
\ -1 $E01F0 $E0100 +range \ nobody really needs these variant selectors

$Variable split$ " !&,-_.\\/:;|<=>@­␣‧་‐‒–—―‖           　" split$ $!
$Variable spaces$ "            　" spaces$ $!

: xcs? ( xchar addr u -- flag ) rot { xc }
    bounds U+DO
	I xc@+ xc = IF  drop true  unloop  EXIT  THEN
    I - +LOOP  false ;
: split? ( xchar -- flag )  split$ $@ xcs? ;
: spaces? ( xchar -- flag )  spaces$ $@ xcs? ;
$100 buffer: breakable-langs
: >breakable ( n lang -- )
    breakable-langs + c! ;
1 Value break-threshold
: breakable? ( xchar -- flag )
    dup range@ breakable-langs + c@ break-threshold u>= >r split? r> or ;
: <split ( addr u -- addr u' )  dup 0= ?EXIT
    BEGIN  dup >r x\string- dup 0> WHILE
	    2dup + xc@ breakable? IF
		drop r>  EXIT  THEN  rdrop
    REPEAT  rdrop ;
: split> ( addr u total -- addr u' ) { t } dup t = ?EXIT
    BEGIN  dup >r over + xchar+ over - dup t u< WHILE
	    over r> + xc@ breakable? ?EXIT
    REPEAT  rdrop ;
: xc-trailing ( addr u -- addr u' )
    dup >r  BEGIN  rdrop dup >r dup WHILE
	x\string- 2dup + xc@ spaces? 0= UNTIL
	drop r>  ELSE  rdrop  THEN ;
: xc-leading ( addr u -- addr' u' )
    BEGIN  dup  WHILE
	    over xc@ spaces?  WHILE
		+x/string  REPEAT  THEN ;

\ base class

$01 Constant box-hflip#
$02 Constant box-vflip#
box-hflip# box-vflip# or Constant box-flip#
$08 Constant baseline-start#
$10 Constant box-hphantom#
$20 Constant box-vphantom#
$40 Constant box-dphantom#
$80 Constant box-defocus#

$0100 Constant box-hfix#
$0200 Constant box-vfix#
$0400 Constant box-dfix#
$0800 Constant vp-hfix#
$1000 Constant vp-vfix#
$2000 Constant vp-dfix#
$4000 Constant box-touched#
$8000 Constant box-resize#
$10 Constant vp-shadow>>#

box-hphantom# box-vphantom# or box-dphantom# or Constant box-phantom#
box-flip# box-phantom# or Constant box-visible#
box-hflip# box-hphantom# or Constant box-vvisible#
box-vflip# box-dphantom# box-vphantom# or or Constant box-hvisible#

object class
    value: caller-w ( -- optr ) \ minos2
    \G pointer back to the widget embedding the actor
    value: active-w ( -- optr ) \ minos2
    \G pointer to the active subwidget embedding the actor
    value: act-name$ ( -- addr u ) \ minos2 act-name-string
    \G Debugging aid: name of the actor
    method clicked ( rx ry bmask n -- ) \ minos2
    \G processed clicks
    method scrolled ( axis dir -- ) \ minos2
    \G process scrolling
    method touchdown ( $rxy*n bmask -- ) \ minos2
    \G raw click down
    method touchup ( $rxy*n bmask -- ) \ minos2
    \G raw click up
    method touchmove ( $rxy*n bmask -- ) \ minos2
    \G raw click, move. @var{bmask}=0 is hover
    method dndmove ( rx ry -- ) \ minos2
    \G drag&drop move, objects can show willingness to accept
    method dnddrop ( rx ry addr u -- ) \ minos2
    \G drag&drop drop, objects can insert data described by @var{addr u}
    method ukeyed ( addr u -- ) \ minos2
    \G key event, string of printable unicode characters
    method ekeyed ( ekey -- ) \ minos2
    \G key event, non-printable key
    method ?inside ( rx ry -- act / 0 ) \ minos2 query-inside
    \G check if coordinates are inside the widget
    method focus ( -- ) \ minos2
    \G put widget into focus
    method defocus ( -- ) \ minos2
    \G put widget out of focus
    method entered ( -- ) \ minos2
    \G react on cursor entering the widget area
    method left ( -- ) \ minos2
    \G react on cursor leaving the widget area
    method show ( -- ) \ minos2
    \G widget is shown
    method hide ( -- ) \ minos2
    \G widget is hidden
    method get ( -- something ) \ minos2
    \G getter for the value behind the widget
    method set ( something -- ) \ minos2
    \G setter for the value behind the widget
    method show-you ( -- ) \ minos2
    \G make widget visible
end-class actor ( -- class ) \ minos2
\G class for the actions bound to a component.

object class
    method hglue!@
    method dglue!@
    method vglue!@
    method aidglue0 \ zero glues
    method aidglue=
end-class helper-glue

' noop helper-glue is hglue!@
' noop helper-glue is vglue!@
' noop helper-glue is dglue!@
' noop helper-glue is aidglue0
' true helper-glue is aidglue= \ if equal, no need to rerun

\ dummy methods for empty actor, used for inheritance
:noname 2drop fdrop fdrop ;
dup actor is clicked
actor is dnddrop
actor :method dndmove fdrop fdrop ;
' 2drop actor is scrolled
' 2drop actor is touchdown
' 2drop actor is touchup
' 2drop actor is touchmove
' 2drop actor is ukeyed
' drop actor is ekeyed
' noop actor is focus
' noop actor is defocus
' noop actor is show
' noop actor is hide
' noop actor is get
' noop actor is set
' noop actor is show-you
' noop actor is entered
' noop actor is left

object class
    value: parent-w ( -- optr ) \ minos2
    \G pointer to parent widget
    value: act ( -- optr ) \ minos2
    \G pointer to actor
    addressable: $value: name$ ( -- addr u ) \ minos2 name-string
    \G Widget name for debugging and searching
    sfvalue: x ( -- r ) \ minos2
    \G widget x coordinate
    sfvalue: y ( -- r ) \ minos2
    \G widget y coordinate
    sfvalue: w ( -- r ) \ minos2
    \G widget width
    sfvalue: h ( -- r ) \ minos2
    \G widget height above baseline
    sfvalue: d ( -- r ) \ minos2
    \G widget depth below baseline
    sfvalue: gap ( -- r ) \ minos2
    \G gap between lines
    sfvalue: baseline ( -- r ) \ minos2
    \G minimun skip per line
    sfvalue: kerning ( -- r ) \ minos2
    \G add kerning
    sfvalue: raise ( -- r ) \ minos2
    \G raise/lower box
    sfvalue: border ( -- r ) \ minos2
    \G surrounding border, all directions
    sfvalue: borderv ( -- r ) \ minos2
    \G vertical border offset
    sfvalue: bordert ( -- r ) \ minos2
    \G top border offset
    sfvalue: borderl ( -- r ) \ minos2
    \G left border offset
    sfvalue: w-color ( -- r ) \ minos2
    \G widget color index (into color map), if any
    method draw-init ( -- ) \ minos2
    \G init draw
    method draw ( -- ) \ minos2
    \G draw widget
    method split ( firstflag rstart1 rx -- o rstart2 ) \ minos2
    \G split a widget into parts for typesetting paragraphs
    method lastfit ( -- ) \ minos2
    \G fit last widget element in a box
    method hglue ( -- rtyp rsub radd ) \ minos2
    \G calculate horizontal glue
    method dglue ( -- rtyp rsub radd ) \ minos2
    \G calculate vertical glue below baseline
    method vglue ( -- rtyp rsub radd ) \ minos2
    \G calculate vertical glue above baseline
    method hglue@ ( -- rtyp rsub radd ) \ minos2 hglue-fetch
    \G cached variant of @code{hglue}
    method dglue@ ( -- rtyp rsub radd ) \ minos2 dglue-fetch
    \G cached variant of @code{dglue}
    method vglue@ ( -- rtyp rsub radd ) \ minos2 vglue-fetch
    \G cached variant of @code{vglue}
    method xywh ( -- rx0 ry0 rw rh ) \ minos2
    \G widget bounding box, starting at the top left corner
    method xywhd ( -- rx ry rw rh rd ) \ minos2
    \G widget bounding box, starting at the left baseline point
    method !resize ( rx ry rw rh rd -- ) \ minos2 store-resize
    \G resize a widget
    method !size ( -- ) \ minos2 store-size
    \G let the widget self-determine its size
    method dispose-widget ( -- ) \ minos2
    \G get rid of a widget
    method .widget ( -- ) \ minos2 print-widget
    \G debugging: Print informations about the widget
    method par-split ( rw -- ) \ minos2
    \G split a paragraph by width @var{rw}
    method resized ( -- ) \ minos2
    \G widget is resized
end-class widget ( -- class ) \ minos2
\G class for visual components

0 Value w.indent#

widget :method resized ( -- )
    parent-w ?dup-IF  .resized \ upwards
    ELSE  !size xywhd !resize     \ downwards
    THEN ;

: inside? ( o:widget rx ry -- flag )
    y f- fdup d f< h fnegate f> and
    x f- fdup w f< f0> and
    and ;
actor :method ?inside ( rx ry -- act / 0 )
    caller-w .inside? o and ;

' fdrop widget is par-split

: w.widget ( -- ) w.indent# spaces name$ type ." : "
    x f. y f. w f. h f. d f. space
    baseline f. gap f. space
    kerning f. raise f. space
    border f. borderv f. bordert f. borderl f. ;
widget :method .widget w.widget cr ;

: name! ( o addr u -- )  third >o to name$ o> ;
: !act ( o:widget actor -- o:widget )
    to act o act >o to caller-w o> ;

widget :method xywh x y h f- w h d f+ ;
widget :method xywhd x y w h d ;
' noop widget is !size
widget :method hglue w border f2* f+ borderl f+ kerning f+ 0e fdup ;
widget :method vglue h border borderv f+ bordert f+ raise f- f+ 0e fdup ;
widget :method dglue d border borderv f+ raise f+ f+ 0e fdup ;
: widget-resize ( x y w h d -- )
    to d to h to w to y to x
    resize( w.indent# spaces name$ type ." : " x f. y f. w f. h f. d f. cr ) ;
' widget-resize widget is !resize
' hglue widget is hglue@
' vglue widget is vglue@
' dglue widget is dglue@
widget :method split ( firstflag rstart1 rx -- o rstart2 )
    !size hglue@ fdrop fdrop f>= or IF   o fdrop 1e
    ELSE  0  fdrop 0e  THEN ;
\ if rstart2 < 0, no split happened
Defer dispose-check ' noop is dispose-check
: dispose-nodict ( o:object -- )
\    o in-dictionary? 0= IF
	dispose( o h. name$ type ."  dispose" cr )
	addr name$ $free
	dispose dispose-check
\    ELSE  dispose( ." in dictionary, don't dispose" cr )  THEN
;
: dispose-act ( o:widget -- ) act ?dup-IF  .dispose-nodict  THEN ;
widget :method dispose-widget ( -- )
    dispose-nodict ;
' noop widget is lastfit

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
glue :method hglue@ hglue-c glue@ ; glue action-of hglue@ glue is hglue
glue :method dglue@ dglue-c glue@ ; glue action-of dglue@ glue is dglue
glue :method vglue@ vglue-c glue@ ; glue action-of vglue@ glue is vglue

\ tile widget

widget class
    synonym frame-color w-color
    value: tile-glue \ glue object
    value: frame#
    value: rotate#
end-class tile

: borderh ( -- b ) borderl border f2* f+ ;
tile :method hglue tile-glue .hglue { f: s f: a } borderh f+ s a ;
tile :method dglue tile-glue .dglue { f: s f: a } border borderv f+ f+ s a ;
tile :method vglue tile-glue .vglue { f: s f: a } border borderv f+ bordert f+ f+ s a ;

: x.glue ( addr -- ) '[' emit glue@ frot f. fswap f. f. ']' emit ;
: g.widget ( -- )
    w.widget
    tile-glue .hglue-c x.glue
    tile-glue .vglue-c x.glue
    tile-glue .dglue-c x.glue ;
tile :method .widget g.widget cr ;

begin-structure atlas-region
    slvalue: i.x
    slvalue: i.y
    slvalue: i.w
    slvalue: i.h
end-structure

: #>st ( x y frame -- )
    \ using frame#
    dup i.h fm* dup i.y s>f f+ fswap
    dup i.w fm*     i.x s>f f+ fswap >st ;

: 01minmax ( r -- r' )
    0e fmax 1e fmin ;
: interpol ( x1 x2 rel -- x1' )
    ftuck fnegate 1e f+ f* f-rot f* f+ ;
: draw-rectangle-part { f: start f: end f: x1 f: y1 f: x2 f: y2 -- }
    start 01minmax to start
    end   01minmax to end
    x1 x2 start interpol
    x1 x2 end   interpol  to x2 to x1
    frame# IF
	frame-color
	-1e to t.i0  6 ?flush-tris
	frame# i>off >v
	x1 y1 >xy fdup i>c n> start         0.125e f+ 0.125e dup #>st v+
	x2 y1 >xy fdup i>c n> end  0.75e f* 0.125e f+ 0.125e dup #>st v+
	x1 y2 >xy fdup i>c n> start         0.125e f+ 0.875e dup #>st v+
	x2 y2 >xy      i>c n> end  0.75e f* 0.125e f+ 0.875e     #>st v+
	v> 2 quad
    THEN ;
: >xyxy ( rx ry rw rh -- rx0 ry0 rx1 ry1 )
    { f: w f: h } fover w f+ fover h f+ ;
: tile-draw ( -- )
    0e 1e xywh >xyxy draw-rectangle-part ;

' tile-draw tile is draw

tile class
end-class thumbnail

Create rot-sts \ exif rotation
    0 c, 1 c, 2 c, 3 c, \ normal
    1 c, 0 c, 3 c, 2 c, \ flip horizontal
    3 c, 2 c, 1 c, 0 c, \ upside down
    2 c, 3 c, 0 c, 1 c, \ flip vertical
    0 c, 2 c, 1 c, 3 c, \ turn right+flip
    2 c, 0 c, 3 c, 1 c, \ turn right
    3 c, 1 c, 2 c, 0 c, \ turn left+flip
    1 c, 3 c, 0 c, 2 c, \ turn left
: rot>st ( n -- )
    $1F and rot-sts + c@ dup 1 and s>f 2/ 1 and s>f >st ;
: rot#>st ( frame n -- )
    $1F and rot-sts + c@ dup 1 and s>f 2/ 1 and s>f #>st ;
: xywh-rect ( fcolor -- )
    xywh >xyxy rotate# 2 lshift { f: x1 f: y1 f: x2 f: y2 fx# -- }
    6 ?flush-tris  i>off  >v
    x1 y1 >xy fdup i>c n> fx#     rot>st v+
    x2 y1 >xy fdup i>c n> fx# 1+  rot>st v+
    x1 y2 >xy fdup i>c n> fx# 2 + rot>st v+
    x2 y2 >xy      i>c n> fx# 3 + rot>st v+
    v> 2 quad ;

: draw-thumb ( -- )
    xywh >xyxy rotate# 2 lshift { f: x1 f: y1 f: x2 f: y2 fx# -- }
    frame# IF
       frame-color
       1e to t.i0  6 ?flush-tris
       frame# i>off >v
       x1 y1 >xy fdup i>c n> dup fx#     rot#>st v+
       x2 y1 >xy fdup i>c n> dup fx# 1+  rot#>st v+
       x1 y2 >xy fdup i>c n> dup fx# 2 + rot#>st v+
       x2 y2 >xy      i>c n>     fx# 3 + rot#>st v+
       v> 2 quad
    THEN ;

' draw-thumb thumbnail is draw

: }}thumb ( glue frame -- o )
    thumbnail new >o  "thumb" to name$
    white# to frame-color  to frame#  to tile-glue o o> ;

\ canvas widget

tile class
    defer: draw-canvas
    defer: text-canvas
    value: cv-data
end-class canvas

canvas :method draw render> draw-canvas text-canvas vi0 ;

\ tile that doesn't draw

tile class
end-class glue-tile

' noop glue-tile is draw

\ image widget

tile class
    defer: image-tex
    value: image-w
    value: image-h
end-class image

image :method draw ( -- )  render>
    0e to t.i0
    z-bias set-color+ image-tex  frame-color xywh-rect ;

\ frame widget

tile class
end-class frame

Create button-st  0e sf, 0.25e sf, 0.75e sf, 1e sf,
DOES>  swap sfloats + sf@ ;
: >border ( rx rb i rw bx -- r ) { f: w f: bx }
    case
	0 of  fdrop      endof
	1 of  bx f+ f+   endof
	2 of  f- w f+    endof
	3 of  fdrop w f+ endof
    endcase ;

: frame-draw ( -- )
    -1e to t.i0 
    frame# frame-color border fdup borderv f+ borderl bordert
    xywh { f f: c f: b f: bv f: bl f: bt f: x f: y f: w f: h }
    raise fdup +to y fnegate +to h
    #80 ?flush-tris  i>off >v
    4 0 DO
	4 0 DO
	    x b  I w bl >border
	    y bv J h bt >border >xy
	    c i>c  n>
	    I button-st J button-st f #>st v+
	LOOP
    LOOP
    v>
    9 0  DO
	4 quad  1 I 3 mod 2 = - i-off +!
    LOOP
; ' frame-draw frame is draw

: }}glue ( glue -- o )
    glue-tile new >o to tile-glue s" glu" to name$ o o> ;
: }}tile ( glue color -- o )
    tile new >o to frame-color to tile-glue s" tile" to name$ o o> ;
: }}frame ( glue color border -- o )
    frame new >o "frame" to name$ to border to frame-color to tile-glue o o> ;
: }}image ( glue rcolor texture-xt -- o )
    image new >o is image-tex to frame-color to tile-glue
    image-tex edge mipmap "image" to name$
    [IFDEF] cubic-mipmap cubic-mipmap [ELSE] linear-mipmap [THEN] o o> ;

\ text widget

5% fvalue text-shrink%
5% fvalue text-grow%

widget class
    synonym text-color w-color
    sfvalue: text-w
    value: text-font
    addressable: $value: text$
    value: l-text \ located text, placeholder to make sure part-text works
    value: orig-text \ part-text, placeholder to make sure part-edit works
    value: us-mask \ underline or strikethrough
    fvalue: start
    fvalue: end
end-class text

: text! ( addr u font -- )
    to text-font to text$ 0e to start 1e to end ;
: text-scale! ( w text-w -- ) { f: tx-w }
    borderh f- kerning f- tx-w fdup f0= IF  f*  ELSE  f/  THEN
    to x-scale ;
: text-xy! ( -- )
    x border kerning f+ borderl f+ f+ fround penxy         sf!
    y                        raise f+ fround penxy sfloat+ sf!
    text-font to font  text-color color f! ;
: text-text ( addr u -- )
    w text-w text-scale! text-xy!
    us-mask ?dup-IF  render-us-string  ELSE  render-string  THEN ;
: text$-part ( addr u rstart rend -- addr' u' )
    dup fover f- fm* fround f>s >r \ length to draw
    dup fm* fround f>s safe/string r> umin ; \ start to skip
: >text+border ( w d h -- )
    border borderv f+ bordert f+ f+ to h
    border borderv f+ f+ to d
    fdup to text-w  borderh f+ to w ;
: text-!size ( addr u -- )
    text-font to font
    \ [: 2dup type cr ;] do-debug
    layout-string >text+border ;
\    ." text sized to: " x f. y f. w f. h f. d f. cr ;
text :method draw text$ text-text ;
text :method !size text$ text-!size ;
text :method hglue w kerning f+
    text-w text-shrink% f* text-w text-grow% f* ;
text :method vglue h raise f- 0e fdup ;
text :method dglue d raise f+ 0e fdup ;
text :method dispose-widget addr text$ $free [ widget ] defers dispose-widget ;
: i18n-text-init
    l-text ?lang and IF
	l-text locale@ to text$
    THEN ;
' i18n-text-init text is draw-init
: t.widget ( -- )
    w.widget  '"' emit text$ type '"' emit cr ;
' t.widget text is .widget

text class
end-class part-text

: pos>fp ( addr -- r )  text$ -rot - s>f fm/ ;
: (text-split) { firstflag class f: start1 f: rx -- o rstart2 }
    text-font to font  1e to x-scale
    rx start1 1e text$ text$-part 2dup pos-string-l2r
    { t p } p t p <> IF
	<split dup 0= firstflag and IF
	    drop p t split>
	THEN
    THEN
    dup 0= IF
	2drop 0 0e  EXIT
    THEN
    2dup + >r dup t <> IF xc-trailing THEN 2dup + pos>fp
    firstflag IF  xc-leading over pos>fp to start1  THEN
    2drop
    start1 o text-font text-color act name$ us-mask
    class new >o to us-mask to name$
    to act to text-color to text-font to orig-text
    to start to end o o>
    r> pos>fp ;
: text-split ( firstflag rstart rx -- o rstart2 )
    part-text (text-split) ;
' text-split text is split
part-text :method split orig-text .split ;
part-text :method lastfit ( -- )
    start end orig-text .text$ text$-part xc-trailing +
    orig-text .pos>fp to end ;

part-text :method !size start end orig-text .text$ text$-part text-!size ;
part-text :method draw start end orig-text .text$ text$-part text-text ;

: tp.widget ( -- )
    w.widget  '"' emit start end orig-text .text$ text$-part type '"' emit cr ;
' tp.widget part-text is .widget

\ translated text

: i18n-text! ( lsid font -- )
    to text-font to l-text  +lang l-text locale@ to text$ ;

\ editable text widget

text class
    value: curpos
    value: cursize
    value: start-curpos \ selection mode
end-class edit

$3F7FFF7F text-color, FValue selection-color

: edit-marking ( -- )
    cursize 0< ?EXIT  text-font to font  w text-w text-scale!
    text$ curpos curpos-string 0e { f: ww f: wleft }
    setstring$ $@len cursize 0>= and IF
	setstring$ $@ dup curpos-string fdup to wleft +to ww  0e
    ELSE
	text$ curpos cursize m2c:curminchars# @ umax +
	curpos-string ww f-
    THEN  fdup f0= IF  fdrop m2c:curminwidth% f@ fdup f2/ fnegate
    ELSE  0e   THEN  { f: cw f: cw- }
    x cw- f+ ww f+ border f+ borderl f+ fround
    y d border borderv f+ f- f+ { f: x0 f: y0 }
    x0 cw f+ fround
    y h border borderv f+ bordert f+ f- f- { f: x1 f: y1 }
    -2e to t.i0  6 ?flush-tris  i>off
    cursize 0> IF  selection-color  ELSE  text-color  THEN >v
    x0 y0 >xy fdup i>c n> 2e 2e >st v+
    x1 y0 >xy fdup i>c n> 3e 2e >st v+
    x0 y1 >xy fdup i>c n> 2e 3e >st v+
    x1 y1 >xy      i>c n> 3e 3e >st v+
    v> 2 quad
    [IFDEF] >cursor-xyxy
	x0 wleft f- y0 x1 y1 >cursor-xyxy
    [THEN] ;

: edit-text ( -- )
    edit-marking
    text-xy!
    setstring$ $@len cursize 0>= and IF
	text$ curpos umin render-string
	setstring$ $@ 1 render-us-string
	text$ curpos cursize 0 max + safe/string render-string
    ELSE
	text$ start end text$-part render-string
    THEN ;
: edit-!size ( -- )
    text-font to font
    setstring$ $@len cursize 0>= and IF
	[: text$ curpos umin type setstring$ $.
	    text$ curpos cursize 0 max + safe/string type ;] $tmp
    ELSE
	text$
    THEN  start end text$-part layout-string
    0 font#-load freetype-gl:texture_font_t-ascender  sf@         fmax fswap
    0 font#-load freetype-gl:texture_font_t-descender sf@ fnegate fmax fswap
    >text+border ;
' edit-text edit is draw
' edit-!size edit is !size

: edit! ( addr u font -- )
    text!  text$ nip to curpos  -1 to cursize  -1 to start-curpos ;

\ multi-line edit

edit class
end-class part-edit

\ password editor

edit class
    cvalue: pw-mode \ 0: hidden, 1: show last char, 2: reveal
end-class pw-edit

e? max-xchar $FF = [IF] '*' [ELSE] '•' [THEN] Value pw-char

Variable *ins-o

: text$->* ( -- oldtext$ )
    text$ over curpos + dup cursize 0 max +  0 addr text$ !@ >r
    pw-mode c>s 0>= IF
	[: { cursor cur# } bounds over >r ?DO
		I c@ $C0 $80 within IF
		    I cur# = cursize 0>= and IF
		    text$ nip curpos - to cursize
		    THEN
		    I xchar+ cursor = IF
			*ins-o @ caller-w = IF
			    I dup xchar+ over - type
			ELSE  pw-char xemit  THEN
		    text$ nip to curpos
		    ELSE  pw-char xemit  THEN
		THEN
	    LOOP
	    r> cur#   = cursize 0>= and IF
		text$ nip curpos - to cursize  THEN
	;] addr text$ $exec
    THEN
    r> ;

: pw-xt { xt -- }
    cursize >r curpos >r
    pw-mode dup 0= IF  *ins-o off  THEN
    c>s 2 < IF
	text$->* >r xt catch r> addr text$ $!buf
	r> to curpos r> to cursize throw
    ELSE
	2rdrop  xt execute
    THEN ;
pw-edit :method draw ( -- ) ['] edit-text    pw-xt ;
pw-edit :method !size ( -- ) ['] edit-!size   pw-xt ;

\ thumb texture

also freetype-gl
0 Value thumb-rgba
tex: thumb-tex-rgba

: thumb-rgba-scaletex ( -- )
    thumb-rgba texscale-xy1 scaletex set-texscale1 ;
: gen-thumb-tex ( -- )
    GL_TEXTURE1 glActiveTexture
    thumb-tex-rgba
    GL_TEXTURE_2D thumb-rgba texture_atlas_t-id l@ glBindTexture edge linear
    thumb-rgba upload-atlas-tex ;

: ?mod-thumb ( -- )
    thumb-rgba texture_atlas_t-modified c@ IF
	GL_TEXTURE1 glActiveTexture
	gen-thumb-tex time( ." thumb: " .!time cr )
	0 thumb-rgba texture_atlas_t-modified c!
    THEN ;

: init-thumb-atlas ( -- )
    atlas# dup 4 texture_atlas_new to thumb-rgba
    thumb-tex-rgba current-tex thumb-rgba texture_atlas_t-id l! ;

' init-thumb-atlas window-init,

:is reload-textures defers reload-textures gen-thumb-tex
    level# @ 0> IF  program init  THEN ;

Variable thumbs[]

Defer free-thumbs

:is free-thumbs ( -- )
    thumbs[] $[]free
    thumb-rgba texture_atlas_delete
    init-thumb-atlas ;

previous

\ draw wrapper

also freetype-gl
: <draw-init ( -- )
    0>clear clear
    program glUseProgram
    -1e 1e >apxy  .01e 100e fdup >ap
    Ambient 1 ambient% glUniform1fv ;

: draw-init> ( -- ) ;
previous

: <draw ( -- )
    GL_TEXTURE4 glActiveTexture
    palette-tex
    GL_TEXTURE3 glActiveTexture
    w-bias set-color+3
    atlas-scaletex
    atlas-tex
    GL_TEXTURE2 glActiveTexture
    z-bias set-color+2
    atlas-bgra-scaletex
    atlas-tex-bgra
    GL_TEXTURE1 glActiveTexture
    z-bias set-color+1
    thumb-rgba-scaletex
    thumb-tex-rgba
    GL_TEXTURE0 glActiveTexture
    vi0 ; \ bg+text+marking draw, one draw call in total

\ load style into atlas-tex-bgra

atlas-region buffer: (ar)

also freetype-gl

: rgba>style { memimg w h atlas | ar[ atlas-region ] -- ivec4-addr }
    BEGIN
	ar[ atlas w 1+ h 1+ texture_atlas_get_region
	ar[ i.x ar[ i.y -1 -1 d= WHILE
	    atlas dup atlas@wh*2
	    texture_atlas_enlarge_texture
    REPEAT
    atlas ar[ i.x ar[ i.y ar[ i.w 1- ar[ i.h 1- memimg ar[ i.w 1- 2* 2*
    texture_atlas_set_region
    memimg free throw  ar[ (ar) atlas-region move (ar)
    GL_TEXTURE0 glActiveTexture ;
: (mem>style) ( addr u atlas -- ivec4-addr )
    >r img>mem r> rgba>style ;
: mem>style ( addr u -- ivec4-addr )
    over >r
    GL_TEXTURE2 glActiveTexture
    atlas-tex-bgra atlas-bgra (mem>style)
    GL_TEXTURE0 glActiveTexture
    r> free throw ;
: load-style ( addr u -- ivec4-addr )
    open-fpath-file throw 2drop slurp-fid mem>style ;

: mem>thumb ( addr u -- ivec4-addr )
    GL_TEXTURE1 glActiveTexture
    thumb-tex-rgba thumb-rgba (mem>style)
    GL_TEXTURE0 glActiveTexture ;
: $top[] ( $addr[] -- addr u / 0 0 )  $@ + cell- $@ ;
: load-thumb ( addr u -- w h thumb )
    mem>thumb dup >r i.w r@ i.h r>
    atlas-region thumbs[] $+[]!
    thumbs[] $top[] ;

previous

: style: ( addr u -- ) \ minos2
    file>fpath Create
    here 0 , $! here atlas-region dup allot erase
  DOES>
    cell+ >r
    r@ atlas-region 0 skip nip 0= IF
	r@ cell- $@ load-style r@ atlas-region move
    THEN
    r> to frame# ;

"white.png" style: white-tile
"button.png" style: button1
"button2.png" style: button2
"button3.png" style: button3
"lbubble.png" style: lbubble
"rbubble.png" style: rbubble

' button1 >body cell+ Value slider-frame# \ set the frame number to button2 style

: thumb: ( addr u r -- ) \ minos2
    file>fpath Create
    here 0 , $! here atlas-region dup allot erase  0 , f,
  DOES> ( -- o thumb-o )
    cell+ >r
    r@ atlas-region 0 skip nip 0= IF
	r@ cell- $@ open-fpath-file throw 2drop slurp-fid
	over >r load-thumb r> free throw
	r@ swap move
	r@ atlas-region + cell+ f@ fdup
	glue new >o
	fm* vglue-c df!
	fm* hglue-c df!  o o>
	r@ atlas-region + !
    THEN
    r@ atlas-region + @ r> }}thumb >o white# to frame-color o o> ;

: }}canvas ( glue color xt-lines xt-text -- o )
    canvas new >o
    is text-canvas is draw-canvas
    to frame-color to tile-glue o
    white-tile o> ;

\ boxes

glue class
    field: childs[] \ all children
    value: box-flags
    value: aidglue \ helper glue for tables
    method map
end-class box

1e20 fconstant 1fil
1fil fdup f* fconstant 1fill
1fil 1fill f* fconstant 1filll
1fil 1/f fconstant 0g \ minimum glue, needs to be bigger than zero to avoid 0/0

: .fil[l[l]] ( f -- )
    fdup 0g 1e10 f* f< IF  0g f/ f. 'g' emit space  EXIT  THEN
    fdup [ 1fil 1e10 f/ ] Fliteral f< IF  f.  EXIT  THEN
    fdup [ 1fill 1e10 f/ ] Fliteral f< IF  1fil f/ f. ." fil" EXIT  THEN
    fdup [ 1filll 1e10 f/ ] Fliteral f< IF  1fill f/ f. ." fill" EXIT  THEN
    1filll f/ f. ." filll" ;

: .glue { f: t f: s f: a -- }
    t f. s .fil[l[l]] space a .fil[l[l]] ;
: gdup ( glue -- glue glue ) fthird fthird fthird ;

: >glue0 ( -- )
    aidglue ?dup-IF  .aidglue0  THEN ;
: >hglue!@ ( glue -- glue' )
    resize( w.indent# spaces name$ type ." : h " gdup .glue cr )
    aidglue ?dup-IF  .hglue!@  THEN ;
: >vglue!@ ( glue -- glue' )
    resize( w.indent# spaces name$ type ." : v " gdup .glue cr )
    aidglue ?dup-IF  .vglue!@  THEN ;
: >dglue!@ ( glue -- glue' )
    resize( w.indent# spaces name$ type ." : d " gdup .glue cr )
    aidglue ?dup-IF  .dglue!@  THEN ;

: do-childs { xt: xt -- .. }
    childs[] $@ bounds U+DO
	I @ .xt
    cell +LOOP ;
\ : do-childs~~ { xt: xt -- .. }
\     ." Childs: " childs[] $@ bounds U+DO
\ 	I @ hex. I @ .name$ type space
\     cell +LOOP cr
\     childs[] $@ bounds U+DO
\ 	I @ .xt
\     cell +LOOP ;
: do-childs-?act { xt: xt -- .. }
    childs[] $@ bounds U+DO
	I @ >o act IF  xt  THEN  o>
    cell +LOOP ;
: ?do-childs { xt flag -- }
    box-flags flag and 0= IF  xt do-childs  THEN ;

: do-childs-act? ( xt flag -- ) \ minos2
    \G loop prevention: checks flag, sets flag, calls do-child-?act, resets flag
    caller-w >o
    dup box-flags and 0= IF
	dup >r    box-flags or  to box-flags  do-childs-?act
	r> invert box-flags and to box-flags
    ELSE
	click( ." box " name$ type ."  loop prevented" cr )
	2drop
    THEN o> ;

: ?=do-childs { xt flag -- }
    box-flags dup box-phantom# and 0= swap flag and or
    IF  xt do-childs  THEN ;
: do-childs-limits { start n xt -- .. }
    childs[] $@ start cells safe/string n cells umin bounds U+DO
	xt I @ .execute
    cell +LOOP ;
: do-lastchild ( xt -- .. )
    childs[] $[]# dup IF 1- childs[] $[] @ .execute ELSE  2drop  THEN ;
: do-firstchild ( xt -- .. )
    childs[] $[]# IF  0 childs[] $[] @ .execute ELSE  drop  THEN ;

: dispose-childs ( -- )
    ['] dispose-widget do-childs childs[] $free ;

: b.widget ( -- )
    w.widget hglue-c x.glue vglue-c x.glue dglue-c x.glue cr
    1 +to w.indent# ['] .widget box-visible# ?do-childs -1 +to w.indent# ;
' b.widget box is .widget

box :method dispose-widget ( -- )
    dispose-childs [ widget ] defers dispose-widget ;

: resize-me ( -- )
    \ make sure you get resized
    box-resize# box-flags or to box-flags ;
: resize-parents ( -- )
    o >o  BEGIN  parent-w dup WHILE  >o rdrop  resize-me  REPEAT  drop o> ;
: box-!size ( -- )
    resize-me
    ['] !size do-childs
    hglue hglue-c glue!
    dglue dglue-c glue!
    vglue vglue-c glue! ;
' box-!size box is !size

box :method draw-init ( -- ) ['] draw-init box-visible# ?do-childs ;
box :method draw ( -- ) ['] draw      box-visible# ?do-childs ;

: +child ( o -- ) o over >o to parent-w resize-parents o> childs[] >back ;
: child+ ( o -- ) o over >o to parent-w resize-parents o> childs[] >stack ;
: +childs ( o1 .. on n -- )
    \ [: ~~ ;] ['] do-debug $10 base-execute
    n>r childs[] get-stack { x } nr> x + childs[] set-stack
    o [: dup to parent-w resize-parents ;] do-childs drop ;

\ glue arithmetics

box class end-class hbox \ horizontal alignment
box class
    value: baseline-offset \ line which is used as outer baseline
end-class vbox \ vertical alignment
box class end-class zbox \ overlay alignment

: fils ( f -- f' ) 1fil f* ;
: fills ( f -- f' ) 1fill f* ;
: fillls ( f -- f' ) 1filll f* ;

: 0glue ( -- t s a ) 0e 0g 0g ;
: 1glue ( -- t s a ) 0e 0g 1fil ;
: 1kglue ( -- t s a ) 0e 0g 1fill ;
: 1Mglue ( -- t s a ) 0e 0g 1filll ;

: .rec { f: x f: y f: w f: h f: d -- }
    x f. y f. w f. h f. d f. ;

\ static-a to allocater
glue new Constant glue*l
glue*l >o 1glue hglue-c glue! 0glue dglue-c glue! 1glue vglue-c glue! o>
glue new Constant glue*ll
glue*ll >o 1kglue hglue-c glue! 0glue dglue-c glue! 1glue vglue-c glue! o>
glue new Constant glue*lll
glue*lll >o 1Mglue hglue-c glue! 0glue dglue-c glue! 1glue vglue-c glue! o>
glue new Constant glue*2
glue*2 >o 1glue f2* hglue-c glue! 0glue f2* dglue-c glue! 1glue f2* vglue-c glue! o>
\ dynamic-a to allocater

: g3>2 ( t s a -- min a ) fover f+ { f: a } f- a ;
: g3>2grow ( t s a -- min a ) fnip ;
: g3>2shrink ( t s a -- min a ) fdrop ;
: ?g3>2 ( t s a flag -- min a )
    IF  g3>2grow  ELSE  g3>2shrink  THEN ;

: glue+ { f: t1 f: s1 f: a1 f: t2 f: s2 f: a2 -- t3 s3 a3 } \ minos2
    \G stick two glues together
    t1 t2 f+ s1 s2 f+ a1 a2 f+ ;
: glue* { f: t1 f: s1 f: a1 f: t2 f: s2 f: a2 -- t3 s3 a3 } \ minos2
    \G overlay two glues together
    t1 s1 f- to s1  t1 a1 f+ to a1
    t2 s2 f- to s2  t2 a2 f+ to a2
    t1 t2 fmax  a1 a2 fmin fmin  s1 s2 fmax fmax
    s1 s2 fmax fover f- fnegate 0e fmax
    a1 a2 fmin 2 fpick f- 0e fmax ;
: glue-dup { f: t1 f: s1 f: a1 -- t1 s1 a1 t1 s1 a1 }
    t1 s1 a1 t1 s1 a1 ;
: glue-drop ( t s a -- ) fdrop fdrop fdrop ;
: baseglue ( -- b 0 max )
    baseline 0g 1filll ;

: bxx ( -- b )
    borderh kerning f+ ;
: bx ( -- b ) border borderl f+ kerning f+ ;
: byy ( -- b )
    borderv border f+ bordert f+ raise f- ;
: bdd ( -- b )
    borderv border f+ raise f+ ;
: byd ( -- b )
    borderv border f+ f2* bordert f+ ;

: hglue+ ( -- glue ) 0glue
    box-flags box-hflip# and ?EXIT
    box-flags dup box-phantom# and swap box-hphantom# and 0= and ?EXIT
    [: hglue@ glue+ ;] box-flip# ?do-childs  frot bxx f+ f-rot ;

: vglue1+ ( glue1 dglue flag -- glue2 dglue2 flag )
    vglue@ glue+
    frot  IF  gap f+ baseline fmax  THEN  f-rot
    glue+ dglue@  true ;
: dglue+ ( -- glue ) 0glue box-flags box-vflip# and ?EXIT
    box-flags dup box-phantom# and swap box-dphantom# and 0= and ?EXIT
    baseline-offset childs[] $[]# u>= IF
	[: glue-drop box-flags box-vflip# and
	    IF  0glue  ELSE  dglue@  THEN ;] do-lastchild \ last dglue
    ELSE
	baseline-offset childs[] $[] @ .dglue@
	false baseline-offset -1
	['] vglue1+ do-childs-limits
	drop glue+
    THEN
    frot bdd f+ f-rot ;
: vglue+ ( -- glue ) 0glue box-flags box-vflip# and ?EXIT
    box-flags dup box-phantom# and swap box-vphantom# and 0= and ?EXIT
    0glue box-flags baseline-start# and 0<> 0 baseline-offset
    ['] vglue1+ do-childs-limits
    glue-drop drop  frot byy f+ f-rot ;

: hglue* ( -- glue ) box-flags box-hflip# and IF  0glue  EXIT  THEN
    1glue [: hglue@ glue* ;] box-hphantom# ?=do-childs
    frot bxx f+ f-rot ;
: dglue* ( -- glue ) box-flags box-vflip# and IF  0glue  EXIT  THEN
    1glue [: dglue@ glue* ;] box-dphantom# ?=do-childs
    frot bdd f+ f-rot ;
: vglue* ( -- glue ) box-flags box-vflip# and IF  0glue  EXIT  THEN
    1glue [: vglue@ glue* ;] box-vphantom# ?=do-childs
    frot byy f+ f-rot ;

: hfix| ( -- ) box-flags box-hfix# and IF  fdrop fdrop 0e fdup  THEN ;
: vfix| ( -- ) box-flags box-vfix# and IF  fdrop fdrop 0e fdup  THEN ;
: dfix| ( -- ) box-flags box-dfix# and IF  fdrop fdrop 0e fdup  THEN ;

hbox :method hglue hglue+ hfix| >hglue!@ ;
hbox :method dglue dglue* dfix| >dglue!@ ;
hbox :method vglue vglue* vfix| >vglue!@ ;

vbox :method hglue hglue* hfix| >hglue!@ ;
vbox :method dglue dglue+ dfix| >dglue!@ ;
vbox :method vglue vglue+ vfix| >vglue!@ ;

zbox :method hglue hglue* hfix| >hglue!@ ;
zbox :method dglue dglue* dfix| >dglue!@ ;
zbox :method vglue vglue* vfix| >vglue!@ ;

:noname ( rw -- )
    borderh f- [{: f: rw :}l rw par-split ;] do-childs ; dup
vbox is par-split
zbox is par-split

\ tab helper glues

Variable tab-glues

helper-glue class
    glues +field htab-c
    glues +field htab-co
end-class htab-glue

htab-glue :method aidglue0 ( -- )
    1glue htab-co glue! ;
htab-glue :method aidglue= ( -- flag )
    htab-co df@ fdup htab-c df@ f= htab-c df! ;
htab-glue :method hglue!@ ( glue -- glue' )
    \ don't allow shrinking/growing
    htab-co glue@ glue* htab-co glue!
    htab-c df@ 0g fdup ;

: tabglues0 ( -- )
    tab-glues get-stack 0 ?DO  .aidglue0  LOOP ;
: tabglues= ( -- flag )  true { flag }
    tab-glues get-stack 0 ?DO  .aidglue= flag and to flag  LOOP
    flag ;

\ add glues up for hboxes

: hglue-step { f: gp/a f: rg f: rd f: rx -- gp/a rg' rd' rx' }
    \ gp/a: total additonal pixels to stretch into divided by total glue
    \ rg: running glue
    \ rd: running remaining pixels
    \ rx: running x
    gp/a  rx to x
    hglue@ gp/a f0> ?g3>2 +to rg { f: xmin }
    rg fdup gp/a f*
    fdup rd f- xmin f+  fdup to w  rx f+ ;

: box-resize? { f: x' f: y' f: w' f: h' f: d' -- flag }
    ?resizeall ?dup-0=-IF
	x' x f<> y' y f<> w' w f<> h' h f<> d' d f<> or or or or
	box-flags box-resize# and 0<> or  THEN
    dup IF  x' y' w' h' d' widget-resize
	tabglues= IF
	    box-flags box-resize# invert and to box-flags
	THEN
    THEN ;

: hbox-resize1 { f: y f: h f: d -- y h d } x y w h d !resize
\    ." hchild resized: " x f. y f. w f. h f. d f. cr
    y h d ;
: hbox-resize { f: x f: y f: w f: h f: d -- }
    x y w h d box-resize? 0= ?EXIT
    1 +to w.indent#
    hglue+ frot bxx f- f-rot  w bxx f- { f: wtotal }
    2 fpick wtotal f<= ?g3>2 { f: wmin f: a }
    wtotal wmin f- a f/ 0e fdup x bx f+
    ['] hglue-step box-hvisible# ?do-childs
    fdrop fdrop fdrop fdrop
    y h d ['] hbox-resize1 box-hvisible# ?do-childs  fdrop fdrop fdrop
    -1 +to w.indent# 
\    ." hbox sized to: " x f. y f. w f. h f. d f. cr
;

' hbox-resize hbox is !resize

: re-glue ( -- w h d )
    hglue fdrop fdrop  vglue fdrop fdrop  dglue fdrop fdrop ;
: par-init ( -- )
    \ set paragraph to maximum horizontal extent
    !size xywhd !resize ;

1e-10 FConstant split-fudge

: hbox-split { firstflag f: start f: rw -- o start' )
    childs[] $[]# { childs# } childs# start fm* split-fudge f+ to start
    start fdup floor f- split-fudge f- 0e fmax { f: startx }
    hbox new { newbox }
    act ?dup-IF  .clone newbox .!act  THEN
    childs# start floor f>s U+DO
	firstflag newbox .childs[] $[]# 0= and
	startx rw I childs[] $[] @ .split to startx
	0e { f: ow }
	?dup-IF
	    >o !size hglue@ fdrop fdrop o o> to ow
	    newbox .child+ \ add to children
	ELSE
	    newbox .childs[] dup $[]#
	    dup IF  1- swap $[] @ >o lastfit !size o>  ELSE  2drop  THEN
	THEN
	startx 1e f>= IF
	    ow fnegate +to rw
	    rw f0<= IF
		newbox I 1+ s>f childs# fm/
		UNLOOP  EXIT
	    THEN
	    0e to startx
	ELSE
	    newbox startx I s>f f+ childs# fm/
	    UNLOOP  EXIT
	THEN
	false to firstflag
    LOOP
    newbox 1e ;
' hbox-split hbox is split
hbox :method lastfit childs[] dup $[]# 1- swap $[] @ .lastfit ;

\ add glues up for vboxes

: vglue-step-h { f: gp/a f: rg f: rd f: ry f: od flag -- gp/a rg' rd' ry' }
    \ gp/a: total additonal pixels to stretch into
    \       by total glue to stretch into (so you can multiply with it)
    \ rg: running glue
    \ rd: running remaining pixels
    \ ry: running y
    \ od: previous descender
    gp/a
    vglue@ gp/a f0> ?g3>2 +to rg { f: ymin }
    rg fdup gp/a f* \ rd'
    fdup rd f- ymin f+   fdup to h
    flag IF  gap f+ baseline od f- fmax  THEN  ry f+ fdup to y ;

: vglue-step-d { f: gp/a f: rg f: rd f: ry -- gp/a rg' rd' ry' d' }
    \ gp/a: total additonal pixels to stretch into
    \       by total glue to stretch into (so you can multiply with it)
    \ rd: running remaining pixels
    \ rg: running glue
    \ rx: running y
    \ d': this descender
    gp/a
    dglue@ gp/a f0> ?g3>2  +to rg { f: ymin }
    rg fdup gp/a f*
    fdup rd f- ymin f+ fdup to d 
    fdup ry f+ fswap ;

: vglue-step ( gp/a rd rg ry od flag -- gp/a rd' rg' ry' od flag' )
    vglue-step-h vglue-step-d true ;

: vbox-resize1 { f: x f: w -- x w } x y w h d !resize
\    ." vchild resized: " x f. y f. w f. h f. d f. cr
    x w ;
: vbox-resize { f: x f: y f: w f: h f: d -- }
    x y w h d box-resize? 0= ?EXIT
    1 +to w.indent# 
    hglue* glue-drop  vglue+ dglue+ glue+ frot byd f- f-rot
    h d f+ byd f- { f: htotal }
    2 fpick htotal f<= ?g3>2 { f: hmin f: a }
    htotal hmin f- a f/ 0e fdup
    y byy f+ h f- 0e
    box-flags baseline-start# and 0<>
    ['] vglue-step box-vvisible# ?do-childs
    fdrop fdrop fdrop fdrop fdrop drop
    x bx f+ w bxx f-
    ['] vbox-resize1 box-vvisible# ?do-childs
    fdrop fdrop
    -1 +to w.indent# 
\    ." vbox sized to: " x f. y f. w f. h f. d f. cr
;

' vbox-resize vbox is !resize

: zbox-resize1 { f: x f: y f: w f: h f: d -- x y w h d }
    x y w h d !resize
\    ." zchild resized: " x f. y f. w f. h f. d f. cr
    x y w h d ;

: zbox-resize { f: x f: y f: w f: h f: d -- }
    x y w h d box-resize? 0= ?EXIT
    1 +to w.indent# 
    x bx f+ y byy f+ w bxx f-
    h byy f- d bdd f-
    ['] zbox-resize1 box-visible# ?do-childs
    fdrop fdrop fdrop fdrop fdrop
    -1 +to w.indent# 
\    ." zbox sized to: " x f. y f. w f. h f. d f. cr
;

' zbox-resize zbox is !resize

\ parbox

0e FValue x-baseline
10% FValue gap%

vbox class
    value: subbox \ hbox to be split into
    value: lhang  \ glue on the left side (second line onwards)
    value: rhang  \ glue on the right side (all lines)
    sfvalue: baseline'
end-class parbox

: p.widget ( -- )
    b.widget  w.indent# spaces subbox .name$ type ." : " cr
    1 +to w.indent# ['] .widget box-visible# subbox .?do-childs -1 +to w.indent# ;
' p.widget parbox is .widget

: dispose[] ( $addr[] -- )
    dup $@ bounds ?DO  I @ .dispose  cell +LOOP  $free ;

parbox :method dispose-widget ( -- )
    childs[] dispose[]
    subbox .dispose-widget ;

parbox :method par-split { f: w -- } \ split a hbox into chunks
    childs[] dispose[] 0e false
    BEGIN  w
	childs[] $[]# IF
	    lhang ?dup-IF  .hglue-c df@ f-  THEN  THEN
	rhang ?dup-IF  .hglue-c df@ f-  THEN
	subbox .split >r
	childs[] $[]# 0= IF  baseline gap
	ELSE  baseline' fdup gap% f*  THEN
	borderl bordert borderv border
	r@ >o to border to borderv to bordert to borderl
	to gap to baseline o>
	childs[] $[]# IF
	    lhang ?dup-IF  }}glue r@ .+child  THEN  THEN
	rhang ?dup-IF  }}glue r@ .child+  THEN
    r> o .child+ true fdup 1e f>=  UNTIL  fdrop drop ;

\ create boxes

$10 stack: box-depth \ this $10 here is no real limit
: {{ ( -- ) depth box-depth >stack ;
: }} ( n1 .. nm -- n1 .. nm m ) depth box-depth stack> - ;
: }}h ( n1 .. nm -- hbox ) }} hbox new >o resize-me "hbox" to name$ +childs o o> ;
: }}v ( n1 .. nm -- vbox ) }} vbox new >o resize-me "vbox" to name$ +childs
    -1 to baseline-offset box-flags baseline-start# or to box-flags o o> ;
: }}vtop ( n1 .. nm -- vbox ) }} vbox new >o +childs 1 to baseline-offset o o> ;
: }}z ( n1 .. nm -- zbox ) }} zbox new >o resize-me "zbox" to name$ +childs o o> ;
: }}p ( n1 .. nm -- parbox ) }}h parbox new >o resize-me to subbox subbox .par-init o o> ;
: unbox ( parbox -- n1 .. nm )
    >o baseline gap 0 childs[] $[] @ >o to gap to baseline o>
    childs[] get-stack  act IF  0 ?DO
	    I pick act .clone swap .!act
	LOOP
    ELSE  drop  THEN  o> ;

\ draw everything

: widget-init ( o:widget -- )
    [IFDEF] 0offset  0offset [THEN]
    <draw-init      draw-init      draw-init>   time( ." init:  " .!time cr )
;

: widget-draw ( o:widget -- )  time( ." draw:  " .!time cr )
    ?colors   IF  load-colors  THEN
    widget-init
    <draw      draw   ?mod-thumb render>  time( ." text:  " .!time cr )
    sync time( ." sync:  " .!time cr ) ;

\ viewport: Draw into a frame buffer

0 Value maxtexsize#
0 Value usetexsize#
: ?texsize ( -- )
    { | result[ 4 ] }
    GL_MAX_TEXTURE_SIZE result[ glGetIntegerv
    result[ l@ to maxtexsize#
    maxtexsize# dpy-w @ dpy-h @ max 2* 2* min to usetexsize# ;

' ?texsize window-init,

vbox class
    sfvalue: vp-x \ x offset of visible part of viewport
    sfvalue: vp-y \ y offset of visible part of viewport
    sfvalue: vp-w \ width inside viewport
    sfvalue: vp-h \ height inside viewport
    sfvalue: vt-x \ x offset of rendering texture
    sfvalue: vt-y \ y offset of rendering texture
    sfvalue: vt-w \ width of rendering texture
    sfvalue: vt-h \ height of rendering texture
    defer: vp-tex
    value: vp-fb   \ framebuffer
    value: vp-rb   \ renderbuffer
    value: vp-glue \ glue object
    value: vp-hslider \ hslider object
    value: vp-vslider \ vslider object
    field: vp-need
end-class viewport

\ scroll viewport to extremes

: vp-top ( o:vp -- ) \ minos2
    \G scroll viewport to top
    vp-h h f- fround to vp-y ;
: vp-bottom ( o:vp -- ) \ minos2
    \G scroll viewport to bottom
    0e to vp-y ;
: vp-left ( o:vp -- ) \ minos2
    \G scroll viewport to left
    0e to vp-x ;
: vp-right ( o:vp -- ) \ minos2
    \G scroll viewport to right
    vp-w w f- fround to vp-x ;

: vp-reslide ( o:vp -- ) \ minos2
    \G Adjust the sliders of a viewport after scrolling
    vp-hslider ?dup-IF  .parent-w >o !size xywhd !resize o>  THEN
    vp-vslider ?dup-IF  .parent-w >o !size xywhd !resize o>  THEN ;

$10 stack: vp<>

: vp-needed ( xt -- ) \ minos2
    \G collect needs in viewport's vp-need
    vp-need need-mask <> IF
	o vp<> >stack
	need-mask >r vp-need to need-mask
	catch r> to need-mask
	vp<> stack> >o rdrop  throw
    ELSE  drop  THEN ;

1 sfloats buffer: vp-ambient%  1.0e vp-ambient% sf!
1 sfloats buffer: vp-saturate% 1.0e vp-saturate% sf!

: <draw-vp ( -- )
    vt-h vt-w f>s f>s
    2dup vp-rb >renderbuffer  vp-fb >framebuffer
    Ambient 1 vp-ambient% glUniform1fv
    Saturate 1 vp-saturate% glUniform1fv
    0e fdup fdup fdup glClearColor clear
    -1e vt-x vt-w f/ f2* f-
    vp-h vt-h f- vt-y f- vt-h f/ f2* 1e f+ >apxy
    .01e 100e fdup vt-w f>s vt-h f>s >apwh ;
: draw-vp> ( -- )
    0>framebuffer
    Ambient 1 ambient% glUniform1fv
    Saturate 1 saturate% glUniform1fv
    0e fdup x-apos sf! y-apos sf!
    -1e 1e >apxy  .01e 100e fdup >ap ;

: search-tree ( ... array xt -- ... ) >r
    $@ 0 { a x x/2 }
    BEGIN  x cell u>  WHILE  x 2/ [ cell negate ]L and to x/2
	    r@  a x/2 + @ .execute IF
		a x x/2 /string
	    ELSE
		a x/2
	    THEN  to x  to a
    REPEAT  rdrop  a ;

: do-vp-childs { xt -- .. }
    vp-h vt-h f- vt-y f- 32e f- vt-h 64e f+ fover f+ { f: y0 f: y1 }
    box-flags box-flip# and ?EXIT
    y1 childs[] [: y h border borderv f+ bordert f+ f- f- fover f<
   ;] search-tree fdrop cell+ childs[] $@ + umin
    y0 childs[] [: y d border borderv f+ f- f+ fover f<
   ;] search-tree fdrop
    U+DO
	xt I @ .execute
    cell +LOOP ;

: draw-vpchilds ( -- )
    <draw-vp   ['] draw-init  do-vp-childs  draw-init>
    <draw      ['] draw       do-vp-childs  ?mod-thumb render>
    draw-vp> ;

viewport :method draw-init
    [: ?sync ?config or ;] vp-needed ?vpsync or IF
	[IFDEF] +offset  x vp-x f-  y vp-h vp-y f- f- +offset  [THEN]
	draw-vpchilds
	[IFDEF] +offset  vp-x x f-  vp-h vp-y f- y f- +offset  [THEN]
	[: -sync -config ;] vp-needed
    THEN ;
viewport :method draw ( -- )  render>
    0e to t.i0
    z-bias set-color+ vp-tex
    x fround y h f- fround w fround h d f+ fround
    >xyxy { f: x1 f: y1 f: x2 f: y2 -- }
    vp-x vt-x f- vt-w f/
    vp-y vt-y f- vt-h f/
    w fround vt-w f/
    h d f+ fround vt-h f/ >xyxy
    { f: s0 f: t0 f: s1 f: t1 }
    box-flags vp-shadow>># rshift c>s ?dup-IF
	s>f { f: shadow }
	i>off
	x1 shadow f+  y1 shadow f+  x2 shadow f+  y2 shadow f+
	{ f: x1a f: y1a f: x2a f: y2a }
	m2c:shadow-color# @ color, >v
	x1a y1a >xy fdup i>c n> s0 t1 >st v+
	x2a y1a >xy fdup i>c n> s1 t1 >st v+
	x1a y2a >xy fdup i>c n> s0 t0 >st v+
	x2a y2a >xy      i>c n> s1 t0 >st v+
	v> 2 quad
    THEN
    i>off
    white# >v
    x1 y1 >xy fdup i>c n> s0 t1 >st v+
    x2 y1 >xy fdup i>c n> s1 t1 >st v+
    x1 y2 >xy fdup i>c n> s0 t0 >st v+
    x2 y2 >xy      i>c n> s1 t0 >st v+
    v> 2 quad render-bgra> ;
: ?vpt-x ( -- flag )
    vp-x vt-x f< vp-x w f+ vt-x vt-w f+ f> or dup IF  drop
	vp-x vt-w w f- f2/ f- 0e fmax vp-w vt-w f- fmin
	fround fdup vt-x f<> to vt-x
    THEN ;
: ?vpt-y ( -- flag )
    vp-y vt-y f< vp-y h d f+ f+ vt-y vt-h f+ f> or dup IF  drop
	vp-y vt-h h d f+ f- f2/ f- 0e fmax vp-h vt-h f- fmin
	fround fdup vt-y f<> to vt-y
    THEN ;
: vp-!size ( -- )
    vp-need @ [ ' +resize >body @ ]L and IF
	['] !size do-childs
	[ vbox ] defers hglue hglue-c glue!
	[ vbox ] defers dglue dglue-c glue!
	[ vbox ] defers vglue vglue-c glue!
	w hglue-c df@ fmax fround
	fdup vp-w f<> to vp-w vp-w usetexsize# s>f fmin to vt-w
	h d f+ dglue-c df@ vglue-c df@ f+ fmax fround
	fdup vp-h f<> to vp-h vp-h usetexsize# s>f fmin to vt-h
	vp-h h d f+ f- vp-y fmin fround fdup vp-y f<> to vp-y
	vp-w w f- vp-x fmin fround fdup vp-x f<> to vp-x
	?vpt-x ?vpt-y
	or or or or or IF ['] +sync vp-needed THEN
    THEN ;
' vp-!size viewport is !size
viewport :method resized ( -- )
    ['] +sync vp-needed [ box ] defers resized ;

viewport :method !resize { f: x f: y f: w f: h f: d -- }
    x y w h d box-resize? 0= ?EXIT
    vp-!size  vp-tex
    ?textures IF  [: +textures +sync ;] vp-needed  THEN
    vt-w f>s vt-h f>s
    vp-fb  ?textures 0= and  IF
	2dup 0 -rot GL_RGBA texture-map \ just resize
	vp-rb >renderbuffer
    ELSE
	GL_RGBA new-textbuffer to vp-fb to vp-rb
    THEN
    0e vp-h vp-w vp-h 0e vbox-resize
    x y w h d widget-resize
    vp-need @ [ ' +resize >body @ ]L invert and vp-need ! ;
viewport :method hglue ( -- glue )
    box-flags vp-hfix# and IF  [ vbox ] defers hglue
    ELSE  vp-glue .hglue >hglue!@  THEN
    resize( ." vp.hglue: " gdup .glue cr ) ;
viewport :method dglue ( -- glue )
    box-flags vp-dfix# and IF  [ vbox ] defers dglue
    ELSE  vp-glue .dglue >dglue!@  THEN
    resize( ." vp.dglue: " gdup .glue cr ) ;
viewport :method vglue ( -- glue )
    box-flags vp-vfix# and IF  [ vbox ] defers vglue
    ELSE  vp-glue .vglue >vglue!@  THEN
    resize( ." vp.vglue: " gdup .glue cr ) ;
viewport :method hglue@ ( -- glue )
    box-flags vp-hfix# and IF  [ vbox ] defers hglue@
    ELSE  vp-glue .hglue@ THEN
    resize( ." vp.hglue@: " gdup .glue cr ) ;
viewport :method dglue@ ( -- glue )
    box-flags vp-dfix# and IF  [ vbox ] defers dglue@
    ELSE  vp-glue .dglue@  THEN
    resize( ." vp.dglue@: " gdup .glue cr ) ;
viewport :method vglue@ ( -- glue )
    box-flags vp-vfix# and IF  [ vbox ] defers vglue@
    ELSE   vp-glue .vglue@  THEN
    resize( ." vp.vglue@: " gdup .glue cr ) ;
: }}vp ( b:n1 .. b:nm glue vp-tex -- viewport ) { g t }
    }} viewport new >o -1 to baseline-offset "vp" to name$
    +childs t is vp-tex g to vp-glue [ ' +resize >body @ ]L vp-need !  o o> ;

\ slider (simple composit object)

tile class \ tile-glue here is the viewport link
end-class vslider-part \ slider part

vslider-part :method hglue w 0g fdup ;
vslider-part :method dglue d 0g fdup ;
vslider-part :method vglue d 0g tile-glue >o h d f+ o> ;
' frame-draw vslider-part is draw

vslider-part class
end-class vslider-partu \ upper part

vslider-part class
end-class vslider-partd \ lower part

' 0glue vslider-partu is hglue
' 0glue vslider-partu is dglue
vslider-partu :method vglue 0e fdup tile-glue >o vp-h vp-y f- h d f+ f- o> ;
' noop vslider-partu is draw

' 0glue vslider-partd is hglue
' 0glue vslider-partd is dglue
vslider-partd :method vglue 0e fdup tile-glue .vp-y ;
' noop vslider-partd is draw

\ vslider

Create vslider-parts
vslider-partu , vslider-part , vslider-partd ,

tile class \ tile-glue here is the viewport link
end-class hslider-part \ slider part

hslider-part :method hglue d f2* 0g tile-glue .w ;
hslider-part :method vglue h 0g fdup ;
hslider-part :method dglue d 0g fdup ;
' frame-draw hslider-part is draw

hslider-part class
end-class hslider-partl \ left part

hslider-partl :method hglue 0g fdup tile-glue .vp-x ;
' noop hslider-partl is draw

hslider-part class
end-class hslider-partr

hslider-partr :method hglue 0g fdup tile-glue >o vp-w vp-x f- w f- o> ;
' noop hslider-partr is draw

Create hslider-parts
hslider-partl , hslider-part , hslider-partr ,

\ slider top

$7F7F7FFF color, FValue slider-color
$7F7F7FFF color, FValue slider-fgcolor
8e FValue slider-border

: slider { parts viewport-link f: sw f: sd f: sh -- ou os od }
    parts 3 cells bounds DO
	I @ new >o slider-frame# to frame#
	slider-fgcolor to frame-color  slider-border to border  0e to baseline
	viewport-link to tile-glue  sw to w  sd to d  sh to h  o o>
    cell +LOOP ;

\ top widget and actors

0 Value top-widget
: top-act ( -- o ) top-widget .act ;

forward >animate

require actors.fs
require animation.fs

\ composite objects

: hslider ( viewport-link sd sh -- o )
    >r {{ glue*l slider-color slider-border }}frame dup .button3
    {{ hslider-parts r@ 0g frot f2/ frot f2/ slider
	r@ fourth hsliderleft[]
	r@ third hslider[]
	r> 1 pick hsliderright[]
    }}h box[]
    }}z box[] ;
: vslider ( viewport-link sw sd -- o )
    >r {{ glue*l slider-color slider-border }}frame dup .button3
    {{ vslider-parts r@ 0g slider
	r@ fourth vsliderup[]
	r@ third vslider[]
	r> 1 pick vsliderdown[]
    }}v box[]
    }}z box[] ;

[defined] screen-xywh@ [IFDEF] SDK_INT SDK_INT #35 >= and [THEN] [IF]
    : xywhd@ ( -- fx fy fw fh fd )
	screen-xywh@ { x y w h }
	x s>f  y h + s>f  w s>f  h s>f  0e ;
[ELSE]
    : xywhd@ ( -- fx fy fw fh fd )
	0e dpy-h @ s>f dpy-w @ s>f fover  0e ;
[THEN]

: htop-resize ( -- )
    +resizeall
    tabglues0        !size xywhd@ !resize
    tabglues= 0= IF  !size xywhd@ !resize  THEN
    -resizeall
    [IFDEF] ?sync-update
	wm_sync_value 8 wm_sync_value' over str= 0= IF
	    wm_sync_value wm_sync_value' 8 move
	    ?sync-update on
	THEN
    [THEN]
    time( ." resize: " .!time cr ) ;

Defer re-config ' noop is re-config

: widgets-redraw ( -- )
    ?config   IF  +resize re-config -config  THEN
    ?resize   IF  htop-resize -resize +sync  THEN
    ?sync     IF  widget-draw time( ." animate: " .!time cr ) -sync -vpsync
	[IFDEF] ?sync-update
	    0 ?sync-update !@ IF  sync-counter-update  THEN
	[THEN]
    THEN ;

also [IFDEF] android jni [THEN]
: widget-sync ( -- ) rendering @ -2 > ?EXIT
    ?config-changer
    ?lang         IF  top-widget .widget-init +resize +resizeall  THEN
    ?textures     IF  1+config    THEN
    anims[] $@len IF  animations  THEN
    top-widget .widgets-redraw
    [IFDEF] showkb
	?keyboard IF  showkb      THEN
    [THEN]
    -textures -lang -keyboard ;

previous

: !init-animation ( -- )
    0.1e o [: fdrop drop color-theme s>f 1/2 f+ ColorMode! +sync +vpsync ;]
    >animate ;
: widgets-looper ( -- )
    0 looper-to# anims[] $@len ?sync or select
    #looper  time( ." looper: " .!time cr ) ;
: widgets-loop ( -- ) depth fdepth { d fd }
    level# @ 0= IF  enter-minos  THEN
    1 level# +!@ >r  top-widget .widget-draw  !init-animation
    BEGIN
	time( ." looper: " ) widgets-looper time( .!time
	." sync: " ) widget-sync time( .!time cr )
	gui( depth d u>  fdepth fd u> or IF  ~~bt
	    depth to d  fdepth to fd  THEN )
    level# @ r@ = UNTIL  r> 0= IF  leave-minos  THEN ;

[IFDEF] looper-ekey
    Variable looper-keys

    : looper-do ( xt -- )
	[: BEGIN  term-prep? IF  (key?)  ELSE  0  THEN
	    WHILE  (key) looper-keys c$+!  REPEAT ;] is looper-ekey
	edit-widget edit-out !
	catch
	edit-terminal edit-out !
	['] noop is looper-ekey
	throw ;
    : looper-keyior ( -- key-ior )
	[: BEGIN  widgets-looper widget-sync
	    looper-keys $@len UNTIL ;] looper-do
	looper-keys $@ drop c@
	looper-keys 0 1 $del ;
[THEN]

previous previous previous
set-current

[IFDEF] provides> provides> [THEN]
