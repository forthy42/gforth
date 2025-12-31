\ freetype GL helper stuff

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

\ freetype stuff

require unix/freetype_gl.fs
require unix/harfbuzz.fs

debug: time(    \ +db time( \ )

also freetype-gl
also opengl

\ If you want to see warnings, uncomment this:
\ 1 freetype_gl_warnings l!

' FTGL_Error_String FTGL_ERR_MAX 1+ exceptions
>r : ?ftgl-ior ( addr -- addr )
    dup 0= IF  [ r> ]L freetype_gl_errno - throw  THEN ;

\ gl-init

$200 Value atlas# \ initial size of an atlas

0 Value atlas
0 Value atlas-bgra
tex: atlas-tex
tex: atlas-tex-bgra \ for color emojis, actually flipped to RGBA

: init-atlas
    atlas# dup 1 texture_atlas_new to atlas
    atlas# dup 4 texture_atlas_new to atlas-bgra
    atlas-tex      current-tex atlas      texture_atlas_t-id l!
    atlas-tex-bgra current-tex atlas-bgra texture_atlas_t-id l! ;

Variable fonts[] \ stack of used fonts

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
    r/o map-file-private texture_font_new_from_memory
    0 over texture_font_t-scaletex
    [ sizeof texture_font_t-scaletex 4 = ] [IF] l! [THEN]
    [ sizeof texture_font_t-scaletex 2 = ] [IF] w! [THEN]
    [ sizeof texture_font_t-scaletex 1 = ] [IF] c! [THEN] ;

' texture_font_clone alias clone-font ( rfontsize font -- font )

: alpha/rgba ( atlas -- )
    texture_atlas_t-depth @ 4 = >r
    GL_RGBA GL_ALPHA r> select ;
: upload-atlas-tex ( atlas -- ) >r
    GL_TEXTURE_2D 0 r@ alpha/rgba
    r@ texture_atlas_t-width @   r@ texture_atlas_t-height @
    0 r@ alpha/rgba GL_UNSIGNED_BYTE
    r@ texture_atlas_t-data @ glTexImage2D rdrop
    GL_TEXTURE0 glActiveTexture ;
: gen-atlas-tex ( -- )
    atlas-tex
    GL_TEXTURE_2D atlas texture_atlas_t-id l@ glBindTexture edge linear
    atlas upload-atlas-tex ;
: gen-atlas-tex-bgra ( -- )
    atlas-tex-bgra
    GL_TEXTURE_2D atlas-bgra texture_atlas_t-id l@ glBindTexture edge linear
    atlas-bgra upload-atlas-tex ;

:is reload-textures defers reload-textures
    gen-atlas-tex gen-atlas-tex-bgra ;

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

Defer xy,
Defer xy+

: xy,default { glyph -- dx dy }
    \ glyph texture_glyph_t-codepoint l@
    x-scale f-scale f* y-scale f-scale f* { f: xs f: ys }
    penxy sf@ penxy sfloat+ sf@ { f: xp f: yp }
    glyph texture_glyph_t-offset_x l@ l>s xs fm*
    glyph texture_glyph_t-offset_y l@ l>s ys fm* { f: xo f: yo }
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
    glyph texture_glyph_t-advance_x sf@ xs f*
    glyph texture_glyph_t-advance_y sf@ ys f* ;

: xy,mirror { glyph -- dx dy }
    \ glyph texture_glyph_t-codepoint l@
    x-scale f-scale f* y-scale f-scale f* { f: xs f: ys }
    penxy sf@ penxy sfloat+ sf@ { f: xp f: yp }
    glyph texture_glyph_t-offset_x l@ l>s xs fm*
    glyph texture_glyph_t-offset_y l@ l>s ys fm* { f: xo f: yo }
    glyph texture_glyph_t-width  2@ xs fm* ys fm* { f: w f: h }
    xp xo f+ fround 1/2 f-  yp yo f- fround 1/2 f- { f: x0 f: y0 }
    x0 w f+                 y0 h f+                { f: x1 f: y1 }
    glyph texture_glyph_t-s0
    \ over hex. dup $10 dump
    >v
    x0 y0 >xy n> xy-color i>c dup s1t0>st v+
    x1 y0 >xy n> xy-color i>c dup s0t0>st v+
    x0 y1 >xy n> xy-color i>c dup s1t1>st v+
    x1 y1 >xy n> xy-color i>c     s0t1>st v+
    v>
    glyph texture_glyph_t-advance_x sf@ xs f*
    glyph texture_glyph_t-advance_y sf@ ys f* ;

: xy,rotright { glyph -- dx dy }
    \ glyph texture_glyph_t-codepoint l@
    x-scale f-scale f* y-scale f-scale f* { f: xs f: ys }
    penxy sf@ penxy sfloat+ sf@ { f: xp f: yp }
    glyph texture_glyph_t-offset_x l@ l>s xs fm*
    glyph texture_glyph_t-offset_y l@ l>s ys fm* { f: yo f: xo }
    glyph texture_glyph_t-width  2@ xs fm* ys fm* { f: h f: w }
    xp w xo f- f- fround 1/2 f-  yp yo f+ fround 1/2 f- { f: x0 f: y0 }
    x0 w f+                      y0 h f+                { f: x1 f: y1 }
    glyph texture_glyph_t-s0
    \ over hex. dup $10 dump
    >v
    x1 y1 >xy n> xy-color i>c dup s1t0>st v+
    x1 y0 >xy n> xy-color i>c dup s0t0>st v+
    x0 y1 >xy n> xy-color i>c dup s1t1>st v+
    x0 y0 >xy n> xy-color i>c     s0t1>st v+
    v>
    glyph texture_glyph_t-advance_x sf@ xs f*
    glyph texture_glyph_t-advance_y sf@ ys f* ;

: xy,rotleft { glyph -- dx dy }
    \ glyph texture_glyph_t-codepoint l@
    x-scale f-scale f* y-scale f-scale f* { f: xs f: ys }
    penxy sf@ penxy sfloat+ sf@ { f: xp f: yp }
    glyph texture_glyph_t-offset_x l@ l>s xs fm*
    glyph texture_glyph_t-offset_y l@ l>s ys fm* { f: yo f: xo }
    glyph texture_glyph_t-width  2@ xs fm* ys fm* { f: h f: w }
    xp xo f- fround 1/2 f-       yp yo f- h f- fround 1/2 f- { f: x0 f: y0 }
    x0 w f+                      y0 h f+                { f: x1 f: y1 }
    glyph texture_glyph_t-s0
    \ over hex. dup $10 dump
    >v
    x0 y0 >xy n> xy-color i>c dup s1t0>st v+
    x0 y1 >xy n> xy-color i>c dup s0t0>st v+
    x1 y0 >xy n> xy-color i>c dup s1t1>st v+
    x1 y1 >xy n> xy-color i>c     s0t1>st v+
    v>
    glyph texture_glyph_t-advance_x sf@ xs f*
    glyph texture_glyph_t-advance_y sf@ ys f* ;

[IFUNDEF] sf+!
    : sf+! ( f addr -- )
	dup sf@ f+ sf! ;
[THEN]

: xy+default ( x y -- )
    penxy sfloat+ sf+!  penxy sf+! ;
: xy+rotright ( x y -- )
    fnegate fswap xy+default ;
: xy+rotleft ( x y -- )
    fswap fnegate xy+default ;

: xy-default ( -- )
    ['] xy,default is xy,
    ['] xy+default is xy+ ;
: xy-mirror ( -- )
    ['] xy,mirror  is xy,
    ['] xy+default is xy+ ;
: xy-rotright ( -- )
    ['] xy,rotright is xy,
    ['] xy+rotright is xy+ ;
: xy-rotleft ( -- )
    ['] xy,rotleft is xy,
    ['] xy+rotleft is xy+ ;
xy-default

: glyph, ( glyph -- dx dy )
    i>off  xy, 2 quad ;
: glyph+xy ( glyph -- )
    glyph, xy+ ;

: all-glyphs ( -- ) 0e atlas texture_atlas_t-width @ s>f { f: l# f: r# }
    i>off >v
    l# l# >xy n> color @ i>c 0e 0e >st v+
    r# l# >xy n> color @ i>c 1e 0e >st v+
    l# r# >xy n> color @ i>c 0e 1e >st v+
    r# r# >xy n> color @ i>c 1e 1e >st v+
    v> 2 quad ;

0 Value font
Variable last-font#

Defer font#-load ( font# -- font )
Defer font-select# ( xcaddr -- xcaddr num )
' false is font-select#
' noop is font#-load

: font-select ( xc-addr -- xc-addr font )
    font-select# dup last-font# ! font#-load ;

: font->t.i0 ( font -- )
    -2e to t.i0  color f@ to xy-color
    dup texture_font_t-scale sf@ to f-scale
    texture_font_t-atlas @ texture_atlas_t-depth @ 4 = IF
	2e +to xy-color -1e to t.i0  THEN ;

: atlas@wh*2 ( atlas -- w h )
    dup texture_atlas_t-width @ 2*
    swap texture_atlas_t-height @ 2* ;

: double-atlas ( font -- )
    dup texture_font_t-atlas @ atlas@wh*2
    texture_font_enlarge_texture
    atlas-scaletex atlas-bgra-scaletex ;

: glyph@ ( font xc-addr -- glyph )
    BEGIN  2dup texture_font_get_glyph dup 0=  WHILE
	    freetype_gl_errno FTGL_ERR_BASE =
	WHILE  drop over double-atlas  REPEAT  THEN
    ?ftgl-ior nip nip ;

: glyph-gi@ ( font glyph-index -- glyph )
    BEGIN  2dup texture_font_get_glyph_gi dup 0=  WHILE
	    freetype_gl_errno FTGL_ERR_BASE =
	WHILE  drop over double-atlas  REPEAT  THEN
    ?ftgl-ior nip nip ;

: xchar+xy (  xc-addrp xc-addr font -- )
    dup font->t.i0
    over glyph@ >r swap
    dup IF
	r@ swap texture_glyph_get_kerning f-scale f*
	penxy sf@ f+ penxy sf!
    ELSE  drop  THEN
    r> glyph+xy 0e to t.i0 ;

: ?mod-atlas ( -- )
    atlas texture_atlas_t-modified c@ IF
	GL_TEXTURE3 glActiveTexture
	gen-atlas-tex time( ." atlas: " .!time cr )
	0 atlas texture_atlas_t-modified c!
    THEN ;
: ?mod-atlas-bgra ( -- )
    atlas-bgra texture_atlas_t-modified c@ IF
	GL_TEXTURE2 glActiveTexture
	gen-atlas-tex-bgra time( ." atlas-bgra: " .!time cr )
	0 atlas-bgra texture_atlas_t-modified c!
    THEN ;

: render> ( -- )
    ?mod-atlas ?mod-atlas-bgra GL_TRIANGLES draw-elements vi0 ;

: ?flush-tris ( n -- ) >r
    i? r@ + points# 2* u>=
    v? r> + points# u>= or
    IF  render>  THEN ;

\ unihan support

$[]Variable >sc[]
$[]Variable >tc[]

: l~min! ( value addr -- )
    dup >r l@ ?dup-IF  umin  THEN  r> l! ;
: translate! ( from to addr -- )
    >r swap dup 8 rshift r> $[] dup
    >r @ 0= IF  { | zeros[ $400 ] } zeros[ $400 2dup erase r@ $!  THEN
    r> $@ rot $FF and sfloats /string drop l~min! ;
: translate@ ( from addr -- to )
    >r dup 8 rshift r> $[] dup
    >r @ 0= IF  rdrop  EXIT  THEN
    r> $@ third $FF and sfloats /string drop l@ tuck select ;

Defer >tc :is >tc ( from to -- ) >tc[] translate! ;
Defer >tc2 ( to -- ) ' drop is >tc2
Defer >sc :is >sc ( from to -- ) >sc[] translate! ;
: >tc@ ( from -- to ) >tc[] translate@ ;
: >sc@ ( from -- to ) >sc[] translate@ ;

$Variable $trans
Vocabulary translators
get-current also translators definitions
: tcify ( addr u -- addr' u' )
    [: BEGIN  dup  WHILE  xc@+? >tc@ xemit  REPEAT  2drop ;]
    $trans dup $free $exec  $trans $@ ;
: scify ( addr u -- addr' u' )
    [: BEGIN  dup  WHILE  xc@+? >sc@ xemit  REPEAT  2drop ;]
    $trans dup $free $exec  $trans $@ ;
previous set-current

Defer translator ' noop is translator

require unicode/unihan.fs
read-unihan
' 2drop ' >tc ' read-japanese wrap-xt
' 2drop ' >sc ' read-japanese-tc wrap-xt

require bidi.fs

\ text rendering

: ?soft-hyphen { addr u -- xaddr xs }
    addr u 2dup x-size { xs }
    "\u00AD" string-prefix?
    IF  u xs =
	IF  "-" drop  ELSE  addr u +x/string over swap x-size +to xs  THEN
    ELSE  addr  THEN  xs ;

: ?variant { addr u -- xaddr xs variant / xaddr xs -1 }
    addr u 2dup x-size { xs }
    over swap xs safe/string 3 u>= IF
	xc@ $FE00 - dup $10 u< IF  xs 3 + swap  EXIT  THEN
    THEN
    drop xs -1 ;

0 Value emoji-font#  \ patched later if found
0 Value symbol-font# \ patched later if found

: ?font-select# { addr u | xs -- xaddr font# xs }
    case  addr u ?variant
	-1 of  2drop  addr u ?soft-hyphen to xs  font-select#  endof
	$F of  to xs  emoji-font#   endof
	$E of  to xs  symbol-font#  endof
	drop 3 - to xs  font-select#  0
    endcase
    dup last-font# !  xs ;
: ?font-select ( addr u -- xaddr font xs )
    ?font-select# >r font#-load r> ;

-1 value bl/null?

Variable $splits<[]> \ stack of arrays
: $splits[] ( -- addr )
    $splits<[]> $@ drop ;

: $splits-top ( level -- addr )
    $splits<[]> $[]@ + cell- ;

: $splits-level-1 ( -- )
    $splits<[]> $@ + cell- dup cell- { src dest }
    src stack# 0 ?DO  src stack> dest >stack  LOOP
    src $free  $splits<[]> stack> drop ;

: $splits-level ( level -- )
    $splits<[]> $[]# 1- swap +DO  $splits-level-1  LOOP ;

: lang-split-string ( addr u -- )
    translator
    start-bidi 2dup >bidi bidi-algorithm
    -1 to bl/null?  last-font# off
    $splits<[]> $[]# IF  $splits[] $[]free  THEN
    $level-buffer $@ drop -1 { lbuf last-level }
    bounds ?DO
	last-font# @ { lf# }
	I delta-I ?font-select# { xs } lf# <>
	lbuf c@ last-level over to last-level <> or  IF
	    last-font# @ 2* last-level 1 and or { w^ font^ }
	    font^ 2 $make
	    last-level $splits-level
	    last-level $splits<[]> $[] >stack
	THEN
	xs last-level $splits-top $+!  1 +to lbuf
    xs +LOOP  0 $splits-level ;

also harfbuzz
Variable infos[]
Variable positions[]
Variable directions[]
Variable segment-lens[]

${GFORTH_IGNLIB} s" true" str= 0= [IF]
    hb_buffer_create Value hb-buffer
    hb-buffer hb_language_get_default hb_buffer_set_language
    hb-buffer HB_BUFFER_CLUSTER_LEVEL_MONOTONE_CHARACTERS hb_buffer_set_cluster_level
[ELSE]
    0 Value hb-buffer
[THEN]

0 Value numfeatures
#10 Constant maxfeatures
Create userfeatures maxfeatures hb_feature_t * allot
DOES> swap hb_feature_t * + ;

: hb-tag ( addr u -- tag )
    4 <> abort" hb-tags are 4 characters each" l@ lbe ;
: hb-feature! ( feature value addr -- ) dup
    >r hb_feature_t-tag l!
    r@ hb_feature_t-value l!
    0  r@ hb_feature_t-start l!
    -1 r> hb_feature_t-end l! ;

"dlig" hb-tag 1 0 userfeatures hb-feature!
"liga" hb-tag 1 1 userfeatures hb-feature!
2 to numfeatures

$100 buffer: font-bidi' \ 0: leave as guess, 4-7: set direction

: shape-splits { xt: setbuf -- }
    $splits[] stack# 0 ?DO
	hb-buffer I $splits[] $[]@ over w@
	dup 2/ { font# } 1 and 4 or { dir# } 2 /string
	font# font#-load dup { font }
	texture_font_activate_size ?ftgl-ior drop
	\ font texture_font_t-hb_font @ hb_ft_font_changed
	0 over hb_buffer_add_utf8
	hb-buffer hb_buffer_get_content_type HB_BUFFER_CONTENT_TYPE_UNICODE = IF
	    hb-buffer hb_buffer_guess_segment_properties
	    \ font# font-bidi' + c@ dir# over select to dir#
	    hb-buffer dir# hb_buffer_set_direction
	    hb-buffer setbuf
	    font texture_font_t-hb_font @ hb-buffer
	    0 userfeatures numfeatures hb_shape
	    dir# I directions[] $[] !
	    { | w^ glyph-count }
	    hb-buffer glyph-count hb_buffer_get_glyph_infos
	    glyph-count l@ hb_glyph_info_t * I infos[] $[]!
	    hb-buffer glyph-count hb_buffer_get_glyph_positions
	    glyph-count l@ hb_glyph_position_t * I positions[] $[]!
	ELSE \ invalid buffer
	    0 I directions[] $[] !
	    s" " I infos[] $[]!
	    s" " I positions[] $[]!
	THEN
	hb-buffer hb_buffer_reset
    LOOP ;

64e 1/f FConstant pos*h
64e 1/f FConstant pos*v

Defer render-string ( addr u -- ) \ minos2
\G Render a string
Defer layout-string ( addr u -- rw rd rh ) \ minos2
\G Layout a string, resulting in width @var{rw}, depth (below baseline)
\G in @var{rd} and height (above baseline) in @var{rh}
Defer curpos-string ( addr u pos -- rcurpos ) \ minos2
\G Translate cursor position pointer @var{pos} into distance @var{rcurpos}
\G from the start of the string
Defer pos-string
Defer pos-string-l2r
Defer get-glyphs

: render-shape-string ( addr u -- )
    lang-split-string ['] drop shape-splits
    $splits[] stack# 0 ?DO
	I $splits[] $[]@ drop w@ 2/ font#-load { font }
	font font->t.i0
	pos*h f-scale f* x-scale f*
	pos*v f-scale f* { f: xpos* f: ypos* }
	case  I directions[] $[] @
	    HB_DIRECTION_TTB  of  xy-rotright  endof
	    xy-default
	endcase
	I positions[] $[]@ drop
	I infos[] $[]@ { pos infos len }
	len 0 ?DO
	    6 ?flush-tris
	    pos I + hb_glyph_position_t-x_offset l@ l>s xpos* fm*
	    pos I + hb_glyph_position_t-y_offset l@ l>s ypos* fm* { f: xo f: yo }
	    xo yo xy+
	    font infos I + hb_glyph_info_t-codepoint l@ glyph-gi@
	    glyph,  fdrop fdrop
	    pos I + hb_glyph_position_t-x_advance l@ l>s xpos* fm* xo f-
	    pos I + hb_glyph_position_t-y_advance l@ l>s ypos* fm* yo f- xy+
	hb_glyph_info_t +LOOP
    LOOP ;

: render-simple-string ( addr u -- )
    -1 to bl/null?
    0 -rot  bounds ?DO
	6 ?flush-tris
	I delta-I ?font-select { xs } xchar+xy
    xs +LOOP  drop ;

: get-simple-glyphs ( addr u -- glyph1 .. glyphn )
    bounds ?DO
	I delta-I ?font-select { ft xs }
	ft font->t.i0
	ft swap glyph@
    xs +LOOP ;

: get-shape-glyphs ( addr u -- glyph1 .. glyphn )
    lang-split-string ['] drop shape-splits
    $splits[] stack# 0 ?DO
	I $splits[] $[]@ drop w@ 2/ font#-load { font }
	font font->t.i0
	I infos[] $[]@ { infos len }
	len 0 ?DO
	    font infos I + hb_glyph_info_t-codepoint l@ glyph-gi@
	hb_glyph_info_t +LOOP
    LOOP ;
previous

: render-us-string ( addr u mask -- )
    penxy sf@ fround 1/2 f+ { f: x0 mask }
    render-string  #12 ?flush-tris
    penxy dup sf@ fround 1/2 f+
    sfloat+ sf@ fround 1/2 f+ { f: x1 f: y }
    "g–" get-glyphs { gg g- }
    y
    gg texture_glyph_t-height   l@ l>s
    gg texture_glyph_t-offset_y l@ l>s - 20% fm*
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
		gg texture_glyph_t-height   l@ l>s
		gg texture_glyph_t-offset_y l@ l>s - -80% fm*
	    endof
	    2 of
		g- texture_glyph_t-offset_y l@ l>s s>f
	    endof
	    0e
	endcase  f- fround 1/2 f- to y0
    I +LOOP  0e to t.i0 ;

: xchar@xy ( fw fd fh xc-addrp xc-addr font -- xc-addr fw' fd' fh' )
    { f: fd f: fh }
    dup texture_font_t-scale sf@ { f: f-scale }
    over glyph@ >r swap
    dup IF
	r@ swap texture_glyph_get_kerning f-scale f* f+
    ELSE  drop  THEN
    r@ texture_glyph_t-advance_x sf@ f-scale f* f+
    r@ texture_glyph_t-offset_y l@ l>s f-scale fm*
    r> texture_glyph_t-height @ f-scale fm*
    fover f- fd fmax fswap fh fmax ;

: layout-simple-string ( addr u -- fw fd fh )
    \ depth is how far it goes down
    0 -rot  0e 0e 0e  bounds ?DO
	I delta-I ?font-select { xs } xchar@xy
    xs +LOOP  drop ;
also harfbuzz

cell 4 = [IF]
    ' sf! alias seg-len!
    ' sf@ alias seg-len@
[ELSE]
    ' df! alias seg-len!
    ' df@ alias seg-len@
[THEN]

: layout-shape-string ( addr u -- rw rd rh )
    \ rd: depth is how far it goes down
    lang-split-string ['] drop shape-splits
    { | f: fw f: fd f: fh }
    $splits[] stack# 0 ?DO
	I $splits[] $[]@ drop w@ 2/ font#-load { font }
	font font->t.i0
	pos*h f-scale f*
	pos*v f-scale f* { f: xpos* f: ypos* }
	0e  I positions[] $[]@ bounds ?DO
	    I hb_glyph_position_t-x_advance l@ l>s xpos* fm* f+
	hb_glyph_position_t +LOOP
	I segment-lens[] $[] seg-len!
	I positions[] $[]@ drop
	I infos[] $[]@ { pos infos len }
	len 0 ?DO
	    pos I + hb_glyph_position_t-y_offset l@ l>s ypos* fm* { f: yo }
	    [ false ] [IF] \ don't render glyph
		font infos I + hb_glyph_info_t-codepoint l@
		glyph-gi@ dup
		>r texture_glyph_t-offset_y l@ l>s f-scale fm* yo f+
		r> texture_glyph_t-height @ f-scale fm*
	    [ELSE]
		{ | ge[ hb_glyph_extents_t ] }
		font texture_font_t-hb_font @
		infos I + hb_glyph_info_t-codepoint l@
		ge[ hb_font_get_glyph_extents drop
		ge[ hb_glyph_extents_t-y_bearing l@ l>s ypos* fm* yo f+
		ge[ hb_glyph_extents_t-height l@ l>s ypos* fm* fnegate
	    [THEN]
	    fover f- fd fmax to fd fh fmax to fh
	    pos I + hb_glyph_position_t-x_advance l@ l>s xpos* fm* +to fw
	hb_glyph_info_t +LOOP
    LOOP
    fw fd fh ;

: pos-simple-string ( fx addr u -- curpos )
    fdup f0< IF  2drop fdrop 0  EXIT  THEN
    dup >r over >r
    0 -rot 0e bounds ?DO
	fdup 0e 0e  I delta-I ?font-select { xs } xchar@xy
	fdrop fdrop
	{ f: p f: n }
	fdup p f>= fdup n f< and IF
	    I p f- n p f- f2/ f> IF  xchar+  THEN
	    unloop r> - nip  rdrop  EXIT
	THEN  n
    xs +LOOP
    drop rdrop r> fdrop fdrop ;

: pos-shape-rest ( -- curpos ) { | offset }
    $splits[] stack# 0 ?DO
	I $splits[] $[]@ drop w@ 2/ font#-load { font }
	font font->t.i0
	pos*h f-scale f* x-scale f* { f: pos* }
	I positions[] $[]@ drop
	I infos[] $[]@ { pos infos len }
	len 0 ?DO
	    pos I + hb_glyph_position_t-x_advance l@ l>s pos* fm*
	    fover fover f2/ f< IF
		infos I + hb_glyph_info_t-cluster l@ offset +
		fdrop fdrop  unloop unloop  EXIT
	    THEN  f-
	hb_glyph_info_t +LOOP
	I $splits[] $[]@ 2 /string +to offset drop
    LOOP
    fdrop offset ;

: pos-shape-string ( addr u rx -- curpos )
    lang-split-string ['] drop shape-splits pos-shape-rest ;

: pos-shape-string-l2r ( addr u rx -- curpos )
    lang-split-string
    [: HB_DIRECTION_LTR hb_buffer_set_direction ;] shape-splits
    pos-shape-rest ;

: curpos-simple-string ( addr u pos -- rcurpos )
    umin layout-simple-string fdrop fdrop x-scale f* ;

: curpos-shape-string { addr u pos -- rcurpos }
    addr u layout-shape-string fdrop fdrop fdrop
    { | rtl? f: len f: lastlen }
    0 addr pos bounds ?DO
	dup directions[] $[] @ HB_DIRECTION_RTL = to rtl?
	dup segment-lens[] $[] seg-len@ fdup to lastlen +to len
	dup 1+ swap $splits[] $[]@ nip 2 -
    +LOOP  drop
    addr u pos umin layout-shape-string fdrop fdrop
    rtl? IF  lastlen fnegate +to len
	len f- lastlen fswap f- len f+  THEN
    x-scale f* ;

previous
	
: use-shaper ( -- )
    ['] render-shape-string is render-string
    ['] layout-shape-string is layout-string
    ['] pos-shape-string is pos-string
    ['] pos-shape-string-l2r is pos-string-l2r
    ['] get-shape-glyphs is get-glyphs
    ['] curpos-shape-string is curpos-string ;
: use-simple ( -- )
    ['] render-simple-string is render-string
    ['] layout-simple-string is layout-string
    ['] pos-simple-string is pos-string
    ['] pos-simple-string is pos-string-l2r
    ['] get-simple-glyphs is get-glyphs
    ['] curpos-simple-string is curpos-string ;

use-shaper

: load-glyph$ ( addr u -- )  layout-string fdrop fdrop fdrop ;

: load-ascii ( -- )
    "#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" load-glyph$ ;

[IFDEF] android  also android [THEN]
: ftgl-init ( -- )
    atlas ?EXIT  init-atlas
    [IFDEF] texture_font_default_mode
	MODE_FREE_CLOSE texture_font_default_mode
    [THEN]
    program init ;
' ftgl-init window-init,
[IFDEF] android  previous [THEN]

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
