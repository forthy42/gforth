\ MINOS2 font style

\ Author: Bernd Paysan
\ Copyright (C) 2018,2019 Free Software Foundation, Inc.

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

get-current also minos definitions

Variable font[]     \ array of fonts
Variable fontname[] \ array of fontnames

0 Value font-size
0 Value font-shape
0 Value font-family
0 Value font-lang

12e FValue font-size# \ basic font size
16e FValue baseline#  \ basic baseline size
1e FValue pixelsize#  \ basic pixel size

: update-size# { f: lines -- }
    dpy-w @ s>f lines f/ fround to font-size#
    font-size# 16e f/ m2c:curminwidth% f!
    font-size# 133% f* fround to baseline#
    dpy-w @ s>f 1280e f/ to pixelsize# ;

: fontsize: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-size ;
: fontshape: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-shape ;
: fontfamily: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-family ;
: fontlang: ( n "name" -- n+1 )
    Create dup , 1+ DOES> @ to font-lang ;

Create font-size%
100% f, 33% f, 50% f, 70% f, 80% f, 90% f, 140% f, 200% f,
DOES> ( n -- ) swap floats + f@ ;

: current-font-size% ( -- float )
    font-size# font-size font-size% f* fround ;

0
fontsize: \normal
fontsize: \micro
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
fontlang: \icons
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
    over font[] $@ drop - cell/ fontnames[]# mod { idx }
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
    dup font[] $@ drop - cell/ dup >r fontnames[]# mod >r \ font index size 0
    atlas-bgra atlas r@ font-langs# mod [ ' \emoji >body @ ]L = select
    r@ fontname[] $[]@ 2dup d0= IF
	." font matrix: " r@
	font-langs# /mod font-shapes# /mod font-families# /mod . . . . cr
	true abort" No font specified"
    THEN
    font-size# 0 font-size% f* fround open-font fonts! rdrop
    drop r> font[] $[] ;

: ?font-load ( font-addr -- font-addr )
    dup @ 0= IF  font-load  THEN ;

\ font selector

: xc>font ( xc-addr font-addr -- xc-addr font )
    >r dup xc@
    cjk?   IF  drop r> cell+          ?font-load @  EXIT  THEN
    emoji? IF  drop r> [ 2 cells ]L + ?font-load @  EXIT  THEN
    icons? IF  drop r> [ 3 cells ]L + ?font-load @  EXIT  THEN
    drop r> ?font-load @ ;

' xc>font IS font-select
\ ' @ IS font-select

\ font indices

: font@ ( -- addr )
    font-size font-index font[] $[] ?font-load ;

: font@h ( -- height )
    font@ @ freetype-gl:texture_font_t-height  sf@ ;
: font@gap ( -- gap )
    font@ @ freetype-gl:texture_font_t-linegap sf@ ;

: current-baseline% ( -- float )
    baseline# font-size font-size% f* font@h fmax fround ;

\ font paths

Variable font-path
Variable font-prefix$

"GFORTHFONTS" getenv 2dup d0= [IF] 2drop "/usr/share/fonts/" [THEN]
font-prefix$ $!

: font-path+ ( "font" -- )
    parse-name [: font-prefix$ $. type ;] $tmp
    2dup open-dir 0= IF
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
    font-prefix$ $free
    font-path+ /system/fonts
    font-path+ /sdcard/gforth/current/minos2/fonts
[ELSE]
    font-path+ ttf/
    font-path+ truetype/
    font-path+ truetype/noto
    font-path+ truetype/droid
    font-path+ truetype/liberation
    font-path+ truetype/arphic-gkai00mp
    font-path+ truetype/emoji
    font-path+ opentype/
    font-path+ opentype/noto
[THEN]

Vocabulary fonts

also fonts definitions

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
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE]
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
[THEN]
\serif
[IFDEF] android
    \regular fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE]
    \regular fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \italic fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold-italic fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
[THEN]
\mono
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE]
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|gkai00mp.ttf
[THEN]

\emoji \sans
\regular fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\bold fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\italic fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\bold-italic fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

\serif
\regular fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\bold fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\italic fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\bold-italic fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

\mono
\regular fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\bold fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\italic fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf
\bold-italic fonts= NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf|SamsungColorEmoji.ttf

\icons
\sans
\regular fonts= fa-merged-900.ttf
\bold fonts= fa-merged-900.ttf
\italic fonts= fa-merged-900.ttf
\bold-italic fonts= fa-merged-900.ttf
\serif
\regular fonts= fa-merged-900.ttf
\bold fonts= fa-merged-900.ttf
\italic fonts= fa-merged-900.ttf
\bold-italic fonts= fa-merged-900.ttf
\mono
\regular fonts= fa-merged-900.ttf
\bold fonts= fa-merged-900.ttf
\italic fonts= fa-merged-900.ttf
\bold-italic fonts= fa-merged-900.ttf

\latin \sans \regular

previous previous set-current
