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
4 Value font-shapes#

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

: font[]# ( -- n ) \ size of font array
    font-sizes# font-shapes# font-families# font-langs# * * * ;
: fontnames[]# ( -- n ) \ size of font array
    font-shapes# font-families# font-langs# * * ;

: font-index ( size -- index )
    font-langs#    * font-lang   +
    font-families# * font-family +
    font-shapes#   * font-shape  + ;

: fontname@ ( -- addr )
    0 font-index fontname[] $[] ;

: fonts! ( addr -- ) \ set current font for all sizes
    font-sizes# 0 U+DO
	dup I font-index font[] $[] !
	I 1+ I' <> IF
	    I 1+ font-size% font-size# f* fround clone-font  THEN
    LOOP  drop ;

: font@ ( -- addr )
    font-size font-index font[] $[]
    dup @ 0= IF
	atlas-bgra atlas font-lang [ ' \emoji >body @ ]L = select
	fontname@ $@ 2dup d0= IF
	    ." font matrix: " font-lang . font-family . font-shape . cr
	    true abort" No font specified"
	THEN
	font-size# 0 font-size% f* fround open-font fonts!
    THEN  @ ;

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
    font-path+ /usr/share/fonts/truetype/arphic-gkai00m
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

\chinese \sans \regular
[IFDEF] android
    fonts= DroidSansFallback.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[ELSE]
    fonts= gkai00mp.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[THEN]

\emoji
fonts= SamsungColorEmoji.ttf|NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf

previous set-current
