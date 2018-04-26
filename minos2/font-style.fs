\ MINOS2 font style

\ Copyright (C) 2018 Free Software Foundation, Inc.

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

\ font array

Variable font[]     \ array of fonts
Variable fontname[] \ array of fontnames

0 Value font-size
0 Value font-shape
0 Value font-family
0 Value font-lang

12e FValue font-size# \ basic font size
16e FValue baseline#  \ basic baseline size
1e FValue pixelsize#  \ basic pixel size

: fontsize: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-size ;
: fontshape: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-shape ;
: fontfamily: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-family ;
: fontlang: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-lang ;

Create font-size%
100% f, 50% f, 70% f, 80% f, 90% f, 140% f, 200% f,
DOES> ( n -- ) swap floats + f@ ;

: current-font-size% ( -- float )
    font-size# font-size font-size% f* fround ;
: current-baseline% ( -- float )
    baseline# font-size font-size% f* fround ;

0
fontsize: \normal
fontsize: \tiny
fontsize: \script
fontsize: \footnote
fontsize: \small
fontsize: \large
fontsize: \huge
Value font-sizes#

\normal

0
fontshape: \regular
fontshape: \bold
fontshape: \italic
fontshape: \bold-italic
Value font-shapes#

\regular

0
fontfamily: \sans
fontfamily: \serif
fontfamily: \mono
Value font-families#

\sans

0
fontlang: \latin
fontlang: \chinese
fontlang: \emoji
Value font-langs#

\latin

\ font load on demand

: font-index ( size -- index )
    font-families# * font-family +
    font-shapes#   * font-shape  +
    font-langs#    * font-lang   + ;
: font[]# ( -- n ) \ size of font array
    font-sizes# font-shapes# font-families# font-langs# * * * ;
: fontnames[]# ( -- n ) \ size of font array
    font-shapes# font-families# font-langs# * * ;

also freetype-gl
: fonts! ( font-addr addr -- ) \ set current font for all sizes
    over 0 font[] $[] - cell/ fontnames[]# mod { idx }
    font-sizes# 0 U+DO
	dup I fontnames[]# * idx + font[] $[] !
	I 1+ I' <> IF
	    I 1+ font-size% font-size# f* fround clone-font
	THEN
    LOOP  drop ;
previous

: fontname@ ( -- addr )
    0 font-index fontname[] $[] ;

: font-load ( font-addr -- font-addr )
    dup 0 font[] $[] - cell/ fontnames[]# mod >r \ font index size 0
    atlas-bgra atlas r@ font-langs# mod [ ' \emoji >body @ ]L = select
    r@ fontname[] $[]@ 2dup d0= IF
	." font matrix: " r@
	font-langs# /mod font-shapes# /mod font-families# /mod . . . . cr
	true abort" No font specified"
    THEN
    font-size# 0 font-size% f* fround open-font fonts! rdrop ;

: ?font-load ( font-addr -- font-addr )
    dup @ 0= IF  font-load  THEN ;

\ font selector

: cjk? ( xchar -- xchar flag )
    \G true if CJK Unified Ideographs
    dup  $2E80  $A000 within ?dup-IF  EXIT  THEN \ Common
    dup $20000 $31390 within ?dup-IF  EXIT  THEN \ Ext B-E
    dup  $F900  $FB00 within ?dup-IF  EXIT  THEN \ Duplicates
    dup  $FF00  $FFF0 within ; \ fullwidth forms

: emoji? ( xchar -- xchar flag )
    dup  $2600  $27C0 within ?dup-IF  EXIT  THEN \ misc. symbols
    dup $1F000 $20000 within ;                   \ pictograms

: xc>font ( xc-addr font-addr -- xc-addr font )
    >r dup xc@
    cjk?   IF  drop r> cell+ ?font-load @  EXIT  THEN
    emoji? IF  drop r> cell+ cell+ ?font-load @  EXIT  THEN
    drop r> @ ;

' xc>font IS font-select
\ ' @ IS font-select

\ font indices

: font@ ( -- addr )
    font-size font-index font[] $[]
    dup @ 0= IF  font-load  THEN ;

\ font paths

Variable font-path
: font-path+ ( "font" -- )
    parse-name 2dup open-dir 0= IF
	close-dir throw font-path also-path
    ELSE  drop 2drop  THEN ;
: ?font ( addr u -- addr' u' true / false )
    font-path open-path-file 0= IF
	rot close-file throw true
    ELSE
	false
    THEN ;
: fonts= ( "font1|font2|..." -- addr u )
    parse-name  BEGIN  dup  WHILE  '|' $split 2swap ?font  UNTIL  2nip
    ELSE  true abort" No suitable font found"  THEN
    fontname@ $! ;

[IFDEF] android
    font-path+ /system/fonts
[ELSE]
    font-path+ /usr/share/fonts/truetype/
    font-path+ /usr/share/fonts/truetype/noto
    font-path+ /usr/share/fonts/truetype/droid
    font-path+ /usr/share/fonts/truetype/liberation
    font-path+ /usr/share/fonts/truetype/arphic-gkai00mp
    font-path+ /usr/share/fonts/truetype/emoji
    font-path+ /usr/share/fonts/opentype/
    font-path+ /usr/share/fonts/opentype/noto
[THEN]

Vocabulary fonts

get-current also fonts definitions

\ default font selection

\sans
\regular fonts= NotoSans-Regular.ttf|DroidSans.ttf|Roboto-Medium.ttf|DejaVuSansCondensed.ttf|LiberationSans-Regular.ttf
\italic fonts= NotoSans-Italic.ttf|Roboto-Italic.ttf|DejaVuSansCondensed-Oblique.ttf|LiberationSans-Italic.ttf
\bold fonts= NotoSans-Bold.ttf|Roboto-Bold.ttf|DejaVuSansCondensed-Bold.ttf|LiberationSans-Bold.ttf
\bold-italic fonts= NotoSans-BoldItalic.ttf|Roboto-BoldItalic.ttf|DejaVuSansCondensed-BoldOblique.ttf|LiberationSans-BoldItalic.ttf

\serif
\regular fonts= NotoSerif-Regular.ttf|LiberationSerif-Regular.ttf
\bold fonts= NotoSerif-Bold.ttf|LiberationSerif-Bold.ttf
\italic fonts= NotoSerif-Italic.ttf|LiberationSerif-Italic.ttf
\bold-italic fonts= NotoSerif-BoldItalic.ttf|LiberationSerif-BoldItalic.ttf

\mono
\regular fonts= LiberationMono-Regular.ttf|DroidSansMono.ttf
\bold fonts= LiberationMono-Bold.ttf|DroidSansMono.ttf
\italic fonts= LiberationMono-Italic.ttf|DroidSansMono.ttf
\bold-italic fonts= LiberationMono-BoldItalic.ttf|DroidSansMono.ttf

\chinese
\sans
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|DroidSansFallback.ttf
[ELSE]
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|gkai00mp.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|gkai00mp.ttf
[THEN]
\serif
[IFDEF] android
    \regular fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|DroidSansFallback.ttf
[ELSE]
    \regular fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc||gkai00mp.ttf
    \italic fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold-italic fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc||gkai00mp.ttf
[THEN]
\mono
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|DroidSansFallback.ttf
[ELSE]
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|gkai00mp.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|gkai00mp.ttf
[THEN]

\emoji \sans \regular
fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

\emoji \sans \bold
fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

\emoji \mono \regular
fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

\emoji \mono \bold
fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

previous set-current
