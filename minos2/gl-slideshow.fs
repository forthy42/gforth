\ Open GL slide show demo

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

require minos2/gl-helper.fs
require minos2/jpeg-exif.fs

also [IFDEF] android android [THEN]
also opengl

\ six slides in memory, cyclic wraparound

tex: slide0
tex: slide1
tex: slide2
tex: slide3
tex: slide4
tex: slide5

6 Constant slide#
20 Constant thumbs#

: slides:  Create  DOES> ( n -- addr ) swap slide# mod cells + ;
: thumbs:  Create  DOES> ( n -- addr ) swap thumbs# mod cells + ;

slides: slides
' slide0 , ' slide1 , ' slide2 ,
' slide3 , ' slide4 , ' slide5 ,
slides: slidex   -1 , -1 , -1 , -1 , -1 , -1 ,
slides: slidewh  -1 , -1 , -1 , -1 , -1 , -1 ,

\ 20 thumbs in memory, cyclic wraparound

tex: thumb00
tex: thumb01
tex: thumb02
tex: thumb03
tex: thumb04
tex: thumb05
tex: thumb06
tex: thumb07
tex: thumb08
tex: thumb09
tex: thumb10
tex: thumb11
tex: thumb12
tex: thumb13
tex: thumb14
tex: thumb15
tex: thumb16
tex: thumb17
tex: thumb18
tex: thumb19

thumbs: thumbs
' thumb00 , ' thumb01 , ' thumb02 , ' thumb03 , ' thumb04 ,
' thumb05 , ' thumb06 , ' thumb07 , ' thumb08 , ' thumb09 ,
' thumb10 , ' thumb11 , ' thumb12 , ' thumb13 , ' thumb14 ,
' thumb15 , ' thumb16 , ' thumb17 , ' thumb18 , ' thumb19 ,
thumbs: thumbx
-1 , -1 , -1 , -1 , -1 ,
-1 , -1 , -1 , -1 , -1 ,
-1 , -1 , -1 , -1 , -1 ,
-1 , -1 , -1 , -1 , -1 ,
thumbs: thumbwh
-1 , -1 , -1 , -1 , -1 ,
-1 , -1 , -1 , -1 , -1 ,
-1 , -1 , -1 , -1 , -1 ,
-1 , -1 , -1 , -1 , -1 ,

Variable slidelist

: read-in ( n -- )
    >r r@ dup slidex @ <> IF
	r@ slides perform
	r@ dup slidex !
	r@ slidelist $[]@ load-texture
	$10 lshift or r> slidewh !
    ELSE
	r> slides perform
    THEN ;

: read-thumb-in ( n -- )
    >r r@ dup thumbx @ <> IF
	r@ thumbs perform
	r@ dup thumbx !
	r@ slidelist $[]@ load-thumb
	$10 lshift or r> thumbwh !
    ELSE
	r> thumbs perform
    THEN ;

$FFFFFFFF Value slcolor
Variable xoff
Variable yoff

: sl-rectangle { f: x1 f: y1 f: x2 f: y2 -- }
    v0 i0 >v
    x1 xoff sf@ f+  y2 yoff sf@ f+ >xy slcolor rgba>c n> 0e 0e >st v+
    x2 xoff sf@ f+  y2 yoff sf@ f+ >xy slcolor rgba>c n> 1e 0e >st v+
    x2 xoff sf@ f+  y1 yoff sf@ f+ >xy slcolor rgba>c n> 1e 1e >st v+
    x1 xoff sf@ f+  y1 yoff sf@ f+ >xy slcolor rgba>c n> 0e 1e >st v+
    v> 0 i, 1 i, 2 i, 0 i, 2 i, 3 i, ;

: blend ( alpha -- ) $FF fm* f>s $FF and $FFFFFF00 or to slcolor ;
: xshift ( delta -- ) xoff sf! ;
: yshift ( delta -- ) yoff sf! ;

: <draw-slide ( -- )
    program init  unit-matrix MVPMatrix set-matrix
    0e fdup fdup 1.0e glClearColor clear
    Ambient 1 ambient% glUniform1fv ;
: draw-slide ( n -- )  read-in
    -1e -1e 1e 1e sl-rectangle GL_TRIANGLES draw-elements ;

4 Constant thumb#
2e thumb# fm/ FConstant 2/x
1e thumb# fm/ 0.9e f* FConstant 1/x \ with border

: draw-thumb { f: x f: y n -- }  n read-thumb-in
    x 1/x f- y 1/x f- x 1/x f+ y 1/x f+
    sl-rectangle GL_TRIANGLES draw-elements ;
: draw-thumbs { n -- }
    thumb# 1+ 0 DO
	thumb# 0 DO
	    n J thumb# * + I + dup slidelist $[]# u< IF
		I 2* thumb# 1- - 2/x fm* f2/
		thumb# 1- J 2* - 2/x fm* f2/ draw-thumb
	    ELSE  drop  THEN
	LOOP
    LOOP ;
    
: draw-slide> ( -- ) sync ;

[IFUNDEF] ftime
    : ftime ( -- r ) ntime d>f 1e-9 f* ;
[THEN]

: fade { n1 n2 f: delta-time -- } n1 n2 = ?EXIT
    ftime { f: startt }
    BEGIN  ftime startt f- delta-time f/ fdup 1e f<  WHILE
	    <draw-slide
	    1e blend n1 draw-slide
	    blend n2 draw-slide
	    draw-slide>  REPEAT
    <draw-slide 1e blend n2 draw-slide draw-slide>
    fdrop ;

: hslide { n1 n2 f: delta-time -- } n1 n2 = ?EXIT
    ftime { f: startt }
    BEGIN  ftime startt f- delta-time f/ fdup 1e f<  WHILE
	    <draw-slide
	    pi f* fcos 1e f- [ pi f2/ fnegate ] FLiteral f* fcos 1e f-
	    fdup n1 n2 > IF fnegate  THEN xshift n1 draw-slide
	    2e f+ n1 n2 > IF fnegate  THEN xshift n2 draw-slide
	    draw-slide>  REPEAT
    <draw-slide n2 draw-slide draw-slide>
    fdrop ;

: vslide { n1 n2 f: delta-time -- } n1 n2 = ?EXIT
    ftime { f: startt }
    BEGIN  ftime startt f- delta-time f/ fdup 1e f<  WHILE
	    <draw-slide
	    pi f* fcos 1e f- [ pi f2/ fnegate ] FLiteral f* fcos 1e f-
	    fdup n1 n2 < IF fnegate  THEN yshift n1 draw-slide
	    2e f+ n1 n2 < IF fnegate  THEN yshift n2 draw-slide
	    draw-slide>  REPEAT
    <draw-slide n2 draw-slide draw-slide>
    fdrop ;

: slideshow-init ( -- )  ctx 0= IF  helper-init  THEN
    ['] VertexShader ['] FragmentShader create-program to program program init
    unit-matrix MVPMatrix set-matrix ;

Variable current-slide

: prefetch ( -- )
    current-slide @ 1+ slidelist $[]# 1- min read-in
    current-slide @ 1- 0 max read-in ;
: prefetch-thumb ( -- ) ;

: +slide ( n -- n1 n2 )
    current-slide @ tuck + 0 max slidelist $[]# 1- min dup current-slide ! ;

: thumb-slide ( n -- ) +slide
    <draw-slide current-slide @ draw-thumbs draw-slide> ;

: thumb-yr ( -- float )  current-slide @ thumb# / s>f
    y-pos sf@ f2/ thumb# fm* f+ ;

: thumb-scroll ( r -- ) fdup floor fdup f>s thumb# * current-slide ! f-
    f2* thumb# fm/ >y-pos  0 thumb-slide ;

: +thumb-slide ( delta -- )
    thumb-yr f+ slidelist $[]# thumb# / 1 - s>f fmin
    0e fmax thumb-scroll 2drop ;

: ?esc ( -- )  key? IF  key #esc = IF  -1 level# +!  THEN  THEN ;

: slide-reshow ( -- )
    0e >y-pos <draw-slide current-slide @ draw-slide draw-slide> ;

: slide-input ( -- )
    >looper ?esc
    *input >r r@ IF
	case  r@ action @  r@ action on
	    1 ( AMOTION_EVENT_ACTION_UP ) of
		short? IF
		    3 3 click-regions
		    BEGIN
			2dup 1 0 d= IF  2drop -1 +slide 1e  vslide  LEAVE  THEN
			2dup 1 2 d= IF  2drop  1 +slide 1e  vslide  LEAVE  THEN
			2dup 0 1 d= IF  2drop -1 +slide 1e  hslide  LEAVE  THEN
			2dup 2 1 d= IF  2drop  1 +slide 1e  hslide  LEAVE  THEN
			2dup 1 1 d= IF  2drop  1 +slide .5e fade    LEAVE  THEN
			2drop
		    DONE
		THEN
	    endof
	    0 of  !click  endof
	endcase
    THEN  rdrop
    need-sync @ IF  slideshow-init slide-reshow  need-sync off  THEN ;

: (reshow) ( -- )
    slide-reshow
    BEGIN  prefetch slide-input  level# @ 0= UNTIL  need-sync on ;

: reshow ( -- ) [IFDEF] android  kbflag @ IF  togglekb  THEN [THEN]
    1 level# +! slideshow-init (reshow) ;

: up-down ( -- )
    ekey? IF  ekey case
	    k-up    of  [ thumb# dup * negate ]L thumb-slide  endof
	    k-down  of  [ thumb# dup *        ]L thumb-slide  endof
	    #esc    of  -1 level# +!                          endof
	endcase
    THEN ;

: thumb-input ( -- ) up-down
    >looper
    *input >r r@ IF
	r@ action @
	case  1 of
		short? IF
		    thumb# dup click-regions
		    thumb# * + current-slide +!
		    (reshow)  1 level# +!
		    0 thumb-slide
		THEN
		r@ action on
	    endof
	    abs 1 <> IF
		thumb# r@ y0 @ last-y0 motion-y0 ['] +thumb-slide do-motion
	    ELSE
		last-y0 motion-y0 ['] +thumb-slide drag-motion
	    THEN  0
	endcase
    THEN  rdrop
    need-sync @ IF  slideshow-init 0 thumb-slide  need-sync off  THEN ;

: rethumb ( -- ) [IFDEF] android  kbflag @ IF  togglekb  THEN [THEN] 0e >y-pos
    1 level# +! slideshow-init 0 thumb-slide
    BEGIN  prefetch-thumb  thumb-input  level# @ 0= UNTIL  need-sync on ;
: slide-show ( addr u -- )
    slidelist $[]slurp-file current-slide off reshow ;
: thumb-show ( addr u -- )
    slidelist $[]slurp-file current-slide off rethumb ;

previous previous

\ s" slide.lst" slide-show

win 0= [IF] window-init [THEN]
