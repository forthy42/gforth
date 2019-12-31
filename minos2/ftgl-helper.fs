\ freetype GL helper stuff

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019 Free Software Foundation, Inc.

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

\ freetype stuff

require ../unix/freetype-gllib.fs

also freetype-gl
also opengl

ctx 0= [IF]  window-init  [THEN]

$200 Value atlas#
$200 Value atlas-bgra#

0 Value atlas
0 Value atlas-bgra
tex: atlas-tex
tex: atlas-tex-bgra \ for color emojis, actually flipped to RGBA

: init-atlas
    atlas#      dup 1 texture_atlas_new to atlas
    atlas-bgra# dup 4 texture_atlas_new to atlas-bgra
    atlas-tex      current-tex atlas      texture_atlas_t-id l!
    atlas-tex-bgra current-tex atlas-bgra texture_atlas_t-id l! ;

init-atlas

Variable fonts[] \ stack of used fonts

[IFDEF] texture_font_default_mode
    MODE_FREE_CLOSE texture_font_default_mode
[THEN]

[IFDEF] texture_font_t-scaletex
    Create texscale-xy0 1e sf, 1e sf,
    Create texscale-xy1 1e sf, 1e sf,
    Create texscale-xy2 1e sf, 1e sf,
    Create texscale-xy3 1e sf, 1e sf,
    
    : scaletex ( atlas dest -- dest ) >r
	1e dup texture_atlas_t-height @ fm/
	1e     texture_atlas_t-width  @ fm/
	r@ sf!+ sf! r> ;
    : atlas-scaletex ( -- )
	atlas texscale-xy3 scaletex set-texscale3 ;
    : atlas-bgra-scaletex ( -- )
	atlas-bgra texscale-xy2 scaletex set-texscale2 ;
[THEN]

: open-font ( atlas rfontsize addr u -- font )
    texture_font_new_from_file
    [IFDEF] texture_font_t-scaletex
	0 over texture_font_t-scaletex
	[ sizeof texture_font_t-scaletex 4 = ] [IF] l! [THEN]
	[ sizeof texture_font_t-scaletex 2 = ] [IF] w! [THEN]
	[ sizeof texture_font_t-scaletex 1 = ] [IF] c! [THEN]
    [ELSE]
	dup fonts[] >stack
    [THEN] ;

' texture_font_clone alias clone-font ( rfontsize font -- font )

: alpha/rgba ( atlas -- )
    GL_RGBA GL_ALPHA rot texture_atlas_t-depth @ 4 = select ;
: upload-atlas-tex ( atlas -- ) >r
    GL_TEXTURE_2D 0 r@ alpha/rgba
    r@ texture_atlas_t-width @   r@ texture_atlas_t-height @
    0 r@ alpha/rgba GL_UNSIGNED_BYTE
    r@ texture_atlas_t-data @ glTexImage2D rdrop ;
: gen-atlas-tex ( -- )
    atlas-tex
    GL_TEXTURE_2D atlas texture_atlas_t-id l@ glBindTexture edge linear
    atlas upload-atlas-tex ;
: gen-atlas-tex-bgra ( -- )
    atlas-tex-bgra
    GL_TEXTURE_2D atlas-bgra texture_atlas_t-id l@ glBindTexture edge linear
    atlas-bgra upload-atlas-tex ;

:noname defers reload-textures
    gen-atlas-tex gen-atlas-tex-bgra ; is reload-textures

\ render font into vertex buffers

2 sfloats buffer: penxy
FVariable color 0e color f!
color f@ FValue xy-color
1e FValue x-scale
1e FValue y-scale
1e FValue f-scale

: s0t0>st ( si ti addr -- ) dup     l@ t.s l!  4 + l@ t.t l! ;
: s1t0>st ( si ti addr -- ) dup 8 + l@ t.s l!  4 + l@ t.t l! ;
: s0t1>st ( si ti addr -- ) dup     l@ t.s l! 12 + l@ t.t l! ;
: s1t1>st ( si ti addr -- ) dup 8 + l@ t.s l! 12 + l@ t.t l! ;
: s0t0>st- ( si ti addr -- ) dup sf@ 75% f* dup 8 + sf@ 25% f* f+ t.s sf!  4 + l@ t.t l! ;
: s1t0>st- ( si ti addr -- ) dup sf@ 25% f* dup 8 + sf@ 75% f* f+ t.s sf!  4 + l@ t.t l! ;
: s0t1>st- ( si ti addr -- ) dup sf@ 75% f* dup 8 + sf@ 25% f* f+ t.s sf!  12 + l@ t.t l! ;
: s1t1>st- ( si ti addr -- ) dup sf@ 25% f* dup 8 + sf@ 75% f* f+ t.s sf!  12 + l@ t.t l! ;

: xy, { glyph -- }
    \ glyph texture_glyph_t-codepoint l@
    x-scale f-scale f* y-scale f-scale f* { f: xs f: ys }
    penxy sf@ penxy sfloat+ sf@ { f: xp f: yp }
    glyph texture_glyph_t-offset_x sl@ xs fm*
    glyph texture_glyph_t-offset_y sl@ ys fm* { f: xo f: yo }
    glyph texture_glyph_t-width  2@ xs fm* ys fm* { f: w f: h }
    xp xo f+ fround 1/2 f-  yp yo f- fround 1/2 f- { f: x0 f: y0 }
    x0 w f+                 y0 h f+                { f: x1 f: y1 }
    glyph texture_glyph_t-s0
    \ over hex. dup $10 dump
    >v
    x0 y0 >xy n> xy-color i>c dup s0t0>st v+
    x1 y0 >xy n> xy-color i>c dup s1t0>st v+
    x0 y1 >xy n> xy-color i>c dup s0t1>st v+
    x1 y1 >xy n> xy-color i>c     s1t1>st v+
    v>
    xp glyph texture_glyph_t-advance_x sf@ xs f* f+ penxy sf!
    yp glyph texture_glyph_t-advance_y sf@ ys f* f+ penxy sfloat+ sf!
\    drop
;

: glyph+xy ( glyph -- )
    i>off  xy,  2 quad ;

: all-glyphs ( -- )
    i>off >v
    0e 0e >xy n> color @ i>c 0e 0e >st v+
    512e 0e >xy n> color @ i>c 1e 0e >st v+
    0e 512e >xy n> color @ i>c 0e 1e >st v+
    512e 512e >xy n> color @ i>c 1e 1e >st v+
    v> 2 quad ;

0 Value font
Defer font-select ( xcaddr font -- xcaddr font' )
' noop is font-select

: font->t.i0 ( font -- )
    -2e to t.i0  color f@ to xy-color
    texture_font_t-atlas @ texture_atlas_t-depth @ 4 = IF
	2e +to xy-color -1e to t.i0  THEN ;

: double-atlas ( xc-addr -- xc-addr )
    freetype_gl_errno $100 = IF
	font font-select
	dup texture_font_t-atlas @ texture_atlas_t-depth @ 4 = IF
	    atlas-bgra# 2* dup >r to atlas-bgra#
	ELSE
	    atlas# 2* dup >r to atlas#
	THEN
	r> dup texture_font_enlarge_texture
	atlas-scaletex atlas-bgra-scaletex
    THEN ;

: glyph@ ( font xc-addr -- font xc-addr glyph )
    BEGIN  2dup texture_font_get_glyph dup 0= WHILE
	    drop double-atlas  REPEAT ;

: xchar+xy ( xc-addrp xc-addr -- xc-addr )
    tuck font font-select \ xc-addr xc-addrp xc-addr font
    dup font->t.i0
    dup texture_font_t-scale sf@ to f-scale
    swap glyph@ \ xc-addr xc-addrp font xc-addr
    >r 2drop \ xc-addr xc-addrp r:glyph
    dup IF
	r@ swap texture_glyph_get_kerning f-scale f*
	penxy sf@ f+ penxy sf!
    ELSE  drop  THEN
    r> glyph+xy 0e to t.i0 ;

: ?mod-atlas ( -- )
    atlas texture_atlas_t-modified c@ IF
	gen-atlas-tex time( ." atlas: " .!time cr )
	0 atlas texture_atlas_t-modified c!
    THEN ;
: ?mod-atlas-bgra ( -- )
    atlas-bgra texture_atlas_t-modified c@ IF
	gen-atlas-tex-bgra time( ." atlas-bgra: " .!time cr )
	0 atlas-bgra texture_atlas_t-modified c!
    THEN ;

: render> ( -- )
    ?mod-atlas ?mod-atlas-bgra GL_TRIANGLES draw-elements vi0 ;

: ?flush-tris ( n -- ) >r
    i? r@ + points# 2* u>=
    v? r> + points# u>= or
    IF  render>  THEN ;

$AD Constant 'soft-hyphen'

: ?soft-hyphen { I' I -- xaddr xs }
    I I' over - x-size { xs }
    I xc@ 'soft-hyphen' = IF  I xs + I' =
	IF  "-" drop  ELSE  I xchar+ dup I' over - x-size +to xs  THEN
    ELSE  I  THEN  xs ;

: render-string ( addr u -- )
    0 -rot  bounds ?DO
	6 ?flush-tris I' I ?soft-hyphen { xs }
	xchar+xy
    xs +LOOP  drop ;

: render-us-string ( addr u mask -- )
    penxy sf@ fround 1/2 f+ { f: x0 mask }
    render-string  #12 ?flush-tris
    penxy dup sf@ fround 1/2 f+
    sfloat+ sf@ fround 1/2 f+ { f: x1 f: y }
    s" g" drop font font-select { ft } drop
    ft font->t.i0
    ft "–" drop glyph@ { g- } 2drop
    ft "g" drop glyph@ { gg } 2drop
    y
    gg texture_glyph_t-height   sl@
    gg texture_glyph_t-offset_y sl@ - 20% fm*
    f+ fround 1/2 f- { f: y0 }
    g- texture_glyph_t-height @ s>f { f: y1 }
    8 1 DO
	mask I and IF
	    g- texture_glyph_t-s0
	    i>off  >v
	    x0 y0       >xy n> xy-color i>c dup s0t0>st- v+
	    x1 y0       >xy n> xy-color i>c dup s1t0>st- v+
	    x0 y0 y1 f+ >xy n> xy-color i>c dup s0t1>st- v+
	    x1 y0 y1 f+ >xy n> xy-color i>c     s1t1>st- v+
	    v> 2 quad
	THEN
	case I  y
	    1 of
		gg texture_glyph_t-height   sl@
		gg texture_glyph_t-offset_y sl@ - -80% fm*
	    endof
	    2 of
		g- texture_glyph_t-offset_y sl@ s>f
	    endof
	    0e
	endcase  f- fround 1/2 f- to y0
    I +LOOP  0e to t.i0 ;

: xchar@xy ( fw fd fh xc-addrp xc-addr -- xc-addr fw' fd' fh' )
    { f: fd f: fh }
    tuck font font-select
    dup texture_font_t-scale sf@ { f: f-scale }
    swap
    BEGIN  2dup texture_font_get_glyph dup 0= WHILE
	    drop double-atlas  REPEAT  >r 2drop
    dup IF
	r@ swap texture_glyph_get_kerning f-scale f* f+
    ELSE  drop  THEN
    r@ texture_glyph_t-advance_x sf@ f-scale f* f+
    r@ texture_glyph_t-offset_y sl@ f-scale fm*
    r> texture_glyph_t-height @ f-scale fm*
    fover f- fd fmax fswap fh fmax ;

: layout-string ( addr u -- fw fd fh ) \ depth is ow far it goes down
    0 -rot  0e 0e 0e  bounds ?DO
	I' I ?soft-hyphen { xs } xchar@xy
    xs +LOOP  drop ;
: pos-string ( fx addr u -- curpos )
    fdup f0< IF  2drop fdrop 0  EXIT  THEN  dup >r over >r
    0 -rot 0e bounds ?DO
	fdup 0e 0e I' I ?soft-hyphen { xs }
	xchar@xy
	fdrop fdrop
	{ f: p f: n }
	fdup p f>= fdup n f< and IF
	    I p f- n p f- f2/ f> IF  xchar+  THEN
	    unloop r> - nip  rdrop  EXIT
	THEN  n
    xs +LOOP
    drop rdrop r> fdrop fdrop ;

: load-glyph$ ( addr u -- )
    bounds ?DO  I font font-select nip
	I texture_font_get_glyph
	0= IF  I double-atlas drop 0
	ELSE  I I' over - x-size  THEN
    +LOOP ;

: load-ascii ( -- )
    "#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" load-glyph$ ;

program init

: <render ( -- )
    program glUseProgram
    z-bias set-color+
    .01e 100e 100e >ap
    GL_TEXTURE3 glActiveTexture
    atlas-tex atlas-scaletex
    GL_TEXTURE0 glActiveTexture
    vi0 ;

 : render-bgra> ( -- )
     GL_ONE GL_ONE_MINUS_SRC_ALPHA glBlendFunc
     GL_TRIANGLES draw-elements
     GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA glBlendFunc ;

previous previous
