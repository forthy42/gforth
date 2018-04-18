\ freetype GL helper stuff

\ Copyright (C) 2014,2016,2017 Free Software Foundation, Inc.

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

require unix/freetype-gllib.fs

also freetype-gl
also opengl

ctx 0= [IF]  window-init  [THEN]

$200 Value atlas#
$100 Value atlas-bgra#

0 Value atlas
0 Value atlas-bgra
tex: atlas-tex
tex: atlas-tex-bgra \ for color emojis

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
    Create text-texscale 2 sfloats allot
    
    : atlas-scaletex ( -- )
	1e atlas texture_atlas_t-height @ fm/
	1e atlas texture_atlas_t-width  @ fm/
	text-texscale sf!+ sf!
	text-texscale set-texscale ;
    : atlas-bgra-scaletex ( -- )
	1e atlas-bgra texture_atlas_t-height @ fm/
	1e atlas-bgra texture_atlas_t-width  @ fm/
	text-texscale sf!+ sf!
	text-texscale set-texscale ;
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

\ render font into vertex buffers

2 sfloats buffer: penxy
Variable color $FFC0A0FF color !
1e FValue x-scale
1e FValue y-scale
0.5e FConstant 1/2

: xy, { glyph -- }
    penxy sf@ penxy sfloat+ sf@ { f: xp f: yp }
    glyph texture_glyph_t-offset_x sl@ x-scale fm*
    glyph texture_glyph_t-offset_y sl@ y-scale fm* { f: xo f: yo }
    glyph texture_glyph_t-width  @ x-scale fm*
    glyph texture_glyph_t-height @ y-scale fm* { f: w f: h }
    xp xo f+ fround 1/2 f-  yp yo f- fround 1/2 f- { f: x0 f: y0 }
    x0 w f+                 y0 h f+                { f: x1 f: y1 }
    [IFDEF] texture_font_t-scaletex
	glyph texture_glyph_t-s0 sf@ { f: s0 }
	glyph texture_glyph_t-t0 sf@ { f: t0 }
	glyph texture_glyph_t-s1 sf@ { f: s1 }
	glyph texture_glyph_t-t1 sf@ { f: t1 }
    [ELSE]
	atlas# 2* s>f 1/f { f: fixup }
	glyph texture_glyph_t-s0 sf@ fixup f- { f: s0 }
	glyph texture_glyph_t-t0 sf@ fixup f- { f: t0 }
	glyph texture_glyph_t-s1 sf@ fixup f- { f: s1 }
	glyph texture_glyph_t-t1 sf@ fixup f- { f: t1 }
    [THEN]
    >v
    x0 y0 >xy n> color @ rgba>c s0 t0 >st v+
    x1 y0 >xy n> color @ rgba>c s1 t0 >st v+
    x0 y1 >xy n> color @ rgba>c s0 t1 >st v+
    x1 y1 >xy n> color @ rgba>c s1 t1 >st v+
    v>
    xp glyph texture_glyph_t-advance_x sf@ x-scale f* f+ penxy sf!
    yp glyph texture_glyph_t-advance_y sf@ y-scale f* f+ penxy sfloat+ sf! ;

: glyph+xy ( glyph -- )
    i>off  xy,  2 quad ;

: all-glyphs ( -- )
    i>off >v
    0e 0e >xy n> color @ rgba>c 0e 0e >st v+
    512e 0e >xy n> color @ rgba>c 1e 0e >st v+
    0e 512e >xy n> color @ rgba>c 0e 1e >st v+
    512e 512e >xy n> color @ rgba>c 1e 1e >st v+
    v> 2 quad ;

0 Value font

: double-atlas ( -- )
    freetype_gl_errno $100 = IF
	atlas# 2* to atlas#
	font atlas# dup texture_font_enlarge_texture
	[IFUNDEF] texture_font_t-scaletex
	    fonts[] get-stack 0 ?DO
	    0.5e 0.5e texture_font_enlarge_glyphs
	    LOOP
	[THEN]
    THEN ;

: xchar+xy ( xc-addrp xc-addr -- xc-addr )
    tuck font swap
    BEGIN  2dup texture_font_get_glyph dup 0= WHILE
	    drop double-atlas  REPEAT  >r 2drop
    dup IF  r@ swap texture_glyph_get_kerning
	penxy sf@ f+ penxy sf!
    ELSE  drop  THEN
    r> glyph+xy ;

: render-string ( addr u -- )
    0 -rot  bounds ?DO
	I xchar+xy
    I I' over - x-size +LOOP  drop ;

: xchar@xy ( fw fd fh xc-addrp xc-addr -- xc-addr fw' fd' fh' )
    { f: fd f: fh }
    tuck font swap
    BEGIN  2dup texture_font_get_glyph dup 0= WHILE
	    drop double-atlas  REPEAT  >r 2drop
    dup IF  r@ swap texture_glyph_get_kerning  f+
    ELSE  drop  THEN
    r@ texture_glyph_t-advance_x sf@ f+
    r@ texture_glyph_t-offset_y sl@ s>f
    r> texture_glyph_t-height @ s>f
    fover f- fd fmax fswap fh fmax ;

: layout-string ( addr u -- fw fd fh ) \ depth is ow far it goes down
    0 -rot  0e 0e 0e  bounds ?DO
	I xchar@xy
    I I' over - x-size +LOOP  drop ;
: pos-string ( fx addr u -- curpos )
    fdup f0< IF  2drop fdrop 0  EXIT  THEN  dup >r over >r
    0 -rot 0e bounds ?DO
	fdup 0e 0e I xchar@xy fdrop fdrop
	{ f: p f: n }
	fdup p f>= fdup n f< and IF
	    I p f- n p f- f2/ f> IF  xchar+  THEN
	    unloop r> - nip  rdrop  EXIT
	THEN  n
    I I' over - x-size +LOOP
    drop rdrop r> fdrop fdrop ;

: load-glyph$ ( addr u -- )
    bounds ?DO  font I I' over - texture_font_load_glyphs
	dup IF  double-atlas  THEN
	I' I - swap -
    +LOOP ;

: load-ascii ( -- )
    "#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" load-glyph$ ;

program init

: <render ( -- )
    program glUseProgram
    w-bias set-color+
    .01e 100e 100e >ap
    atlas-tex v0 i0 ;

: render> ( -- )
    GL_TRIANGLES draw-elements ;
: render-bgra> ( -- )
    GL_ONE GL_ONE_MINUS_SRC_ALPHA glBlendFunc
    GL_TRIANGLES draw-elements
    GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA glBlendFunc ;

previous previous
