\ MINOS2 font style

\ Author: Bernd Paysan
\ Copyright (C) 2018,2019,2020 Free Software Foundation, Inc.

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

require cstr.fs

get-current >r also minos definitions

Variable font[]     \ array of fonts

0 Value font-size
0 Value font-shape
0 Value font-family
0 Value font-lang

12e FValue font-size# \ basic font size
133.333% FValue *baseline#
16e FValue baseline#  \ basic baseline size
1e FValue pixelsize#  \ basic pixel size
1280e FValue screenwidth#
16e 1/f FValue *curminwidth#

: update-size# { f: lines -- }
    dpy-w @ s>f lines f/ fround to font-size#
    font-size# *curminwidth# f* m2c:curminwidth% f@ fmax m2c:curminwidth% f!
    font-size# *baseline# f* fround to baseline#
    dpy-w @ s>f screenwidth# f/ to pixelsize# ;

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
fontlang: \simplified-chinese
fontlang: \traditional-chinese
fontlang: \japanese
fontlang: \hangul
fontlang: \emoji
fontlang: \icons
fontlang: \armenian
fontlang: \hebrew
fontlang: \arabic
fontlang: \syriac
fontlang: \thaana
fontlang: \nko
fontlang: \samaritan
fontlang: \mandaic
fontlang: \devanagari
fontlang: \bengali
fontlang: \gurmukhi
fontlang: \gujarati
fontlang: \oriya
fontlang: \tamil
fontlang: \telugu
fontlang: \kannada
fontlang: \malayalam
fontlang: \sinhala
fontlang: \thai
fontlang: \lao
fontlang: \tibetan
fontlang: \myanmar
fontlang: \georgian
fontlang: \ethiopic
fontlang: \cherokee
fontlang: \canadianaboriginal
fontlang: \ogham
fontlang: \runic
fontlang: \tagalog
fontlang: \hanunoo
fontlang: \buhid
fontlang: \tagbanwa
fontlang: \khmer
fontlang: \mongolian
fontlang: \limbu
fontlang: \taile
fontlang: \newtailue
fontlang: \buginese
fontlang: \taitham
fontlang: \balinese
fontlang: \sundanese
fontlang: \batak
fontlang: \lepcha
fontlang: \olchiki
fontlang: \glagolitic
fontlang: \coptic
fontlang: \tifinagh
fontlang: \yi
fontlang: \lisu
fontlang: \vai
fontlang: \bamum
fontlang: \sylotinagri
fontlang: \phagspa
fontlang: \saurashtra
fontlang: \kayahli
fontlang: \rejang
fontlang: \javanese
fontlang: \cham
fontlang: \taiviet
fontlang: \meeteimayek
fontlang: \lycian
fontlang: \carian
fontlang: \olditalic
fontlang: \gothic
fontlang: \ugaritic
fontlang: \oldpersian
fontlang: \deseret
fontlang: \shavian
fontlang: \osmanya
fontlang: \osage
fontlang: \cypriot
fontlang: \imperialaramaic
fontlang: \phoenician
fontlang: \lydian
fontlang: \kharoshthi
fontlang: \oldsoutharabian
fontlang: \avestan
fontlang: \inscriptionalparthian
fontlang: \inscriptionalpahlavi
fontlang: \oldturkic
fontlang: \brahmi
fontlang: \kaithi
fontlang: \chakma
fontlang: \cuneiform
fontlang: \egyptianhieroglyphs
fontlang: \anatolianhieroglyphs
fontlang: \adlam
Value font-langs#

\latin

\ font load on demand

: font-index ( -- index )
    font-family
    font-shapes#   * font-shape +
    font-langs#    * font-lang  + ;
font-shapes# font-families# font-langs# * * Constant font[]# ( -- n )
\G size of font array

also freetype-gl also harfbuzz
: referenced ( font -- font )
    dup texture_font_t-face @ hb_ft_font_create_referenced
    over texture_font_t-hb_font ! ;
previous previous

s" No font specified" exception constant !!no-font!!
s" No suitable font found" exception constant !!no-suitable-font!!

0 font[]# 1- [?DO] ' !!no-font!! [I] font[] $[] ! -1 [+LOOP]

\ font selector

\ other combiners are within their language block, and don't need
\ special care

: xc>font# ( xc-addr -- xc-addr font# )
    dup ['] xc@ catch IF  drop 0  EXIT  THEN
    case
	dup bl/null? = swap range@ or
	-1 of  last-font# @  endof
	dup 2 4 within IF  -1  ELSE  bl  THEN  to bl/null?
	0 endcase ;

: font-load ( index -- font )
    font[]# /mod swap cells font[] $@ drop + perform ;
:noname ( font# -- font )
    font + font-load ; is font#-load
' xc>font# IS font-select#
\ ' @ IS font-select

\ font indices

: font@ ( -- index )
    font-size font[]# * font-index + ;

: font@h ( -- height )
    font@ font-load freetype-gl:texture_font_t-height  sf@ ;
: font@gap ( -- gap )
    font@ font-load freetype-gl:texture_font_t-linegap sf@ ;

: current-baseline% ( -- float )
    baseline# font-size font-size% f* font@h fmax fround ;

\ font paths

Variable font-path
Variable font-prefix$

"GFORTHFONTS" getenv 2dup d0= [IF] 2drop "/usr/share/fonts/" [THEN]
font-prefix$ $!

also freetype-gl
: font-path+ ( "font" -- )
    parse-name
    2dup absolut-path? 0= IF  [: font-prefix$ $. type ;] $tmp  THEN
    2dup open-dir 0= IF
	close-dir throw font-path also-path
    ELSE  drop 2drop  THEN ;
: ?font ( addr u -- addr' u' true / false )
    font-path open-path-file 0= IF
	rot close-file throw tilde_cstr cstring>sstring true
    ELSE
	false
    THEN ;
: font-loader ( fontaddr atlas -- fontaddr )
    swap { fontdes }
    fontdes cell+ $@
    0 font-size# font-size% f* fontdes 2 cells + f@ f*
    open-font referenced { font }
    font 0 fontdes $[] !
    font-sizes# 1 ?DO
	font I font-size# font-size% f* fontdes 2 cells + f@ f*
	clone-font referenced I fontdes $[] !
    LOOP
    fontdes ;
cs-Vocabulary fonts

100% FValue font-scaler

: font: ( addr u -- )
    get-current >r ['] fonts >wordlist set-current
    Create 0 , here 0 , $! font-scaler f, r> set-current ;
: bw-font ( addr -- font )
    dup @ 0= IF  atlas      font-loader  THEN  $[] @ ;
: bgra-font ( addr -- font )
    dup @ 0= IF  atlas-bgra font-loader  THEN  $[] @ ;
0 Value last-font
: ?define-font ( addr u xt -- ) >r
    2dup basename 2dup ['] fonts >wordlist find-name-in dup IF
	>r 2drop 2drop r> rdrop
    ELSE  drop nextname font: r> set-does> latestxt  THEN
    dup to last-font font-index font[] $[] ! ;
: fonts-parse ( "[<">]font1|font2|...[<">]" -- addr u )
    >in @ >r  parse-name
    over c@ '"' = IF  2drop r@ >in !  '"' parse 2drop '"' parse  THEN  rdrop ;
: fonts-scan ( addr u -- )
    $substitute drop
    BEGIN  dup  WHILE  '|' $split 2swap ?font  UNTIL  2nip
    ELSE  !!no-suitable-font!! throw  THEN ;

: fonts= ( "font1|font2|..." -- )
    fonts-parse fonts-scan ['] bw-font   ?define-font ;
: color-fonts= ( "font1|font2|..." -- )
    fonts-parse fonts-scan ['] bgra-font ?define-font ;
: font=same ( -- )
    last-font font-index font[] $[] ! ;
: fonts[bi]=same ( -- )
    \bold font=same \italic font=same \bold-italic font=same \regular ;
: fonts[ssm]=same ( -- )
    fonts[bi]=same
    \serif font=same fonts[bi]=same
    \mono  font=same fonts[bi]=same
    \sans ;
: font=%% ( -- )
    "%family%%style%%lang%-%shape%.ttf" fonts-scan ['] bw-font ?define-font ;
: +ranges ( range1 .. rangen n -- )
    0 ?DO  font-lang -rot +range  LOOP ;
: fonts=shapes[rb] ( range1 .. rangen n -- 0 )
    \regular "Regular" "shape" replaces font=%%
    ?dup-IF  fonts[ssm]=same  +ranges  THEN  0
    \italic font=same
    \bold "Bold" "shape" replaces font=%%
    \bold-italic font=same ;
: fonts=shapes[r] ( range1 .. rangen n -- 0 )
    \regular "Regular" "shape" replaces font=%%
    +ranges fonts[ssm]=same ;
: fonts=template[rb] ( range1 .. rangen n addr u -- )
    [: "lang" replaces
	\sans  "Sans"  "style" replaces fonts=shapes[rb]
	\serif "Serif" "style" replaces fonts=shapes[rb]
	\mono  "Sans"  "style" replaces fonts=shapes[rb] drop ;] catch
    IF  clearstack "%lang%" $substitute drop type ."  failed" cr THEN ;
: fonts=template[rb]sans ( range1 .. rangen n addr u -- )
    [: "lang" replaces "Sans"  "style" replaces
	\sans  fonts=shapes[rb]
	\serif fonts=shapes[rb]
	\mono  fonts=shapes[rb] drop ;] catch
    IF  clearstack "%lang%" $substitute drop type ."  failed" cr THEN ;
: fonts=template[r] ( range1 .. rangen n addr u -- )
    [: "lang" replaces
	\sans  "Sans"  "style" replaces fonts=shapes[r] ;] catch
    IF  clearstack
	[: error-color "%lang%" $substitute drop type ."  failed" cr ;]
	['] execute-theme-color do-debug  THEN ;

previous

font-path+ ~/.fonts

[IFDEF] android
    font-prefix$ $free
    font-path+ /system/fonts
    "minos2/fonts" open-fpath-file 0=
    [IF]  font-path also-path close-file throw  [THEN]
[ELSE]
    font-path+ ttf/
    font-path+ truetype/
    font-path+ truetype/dejavu
    font-path+ truetype/noto
    font-path+ truetype/droid
    font-path+ truetype/liberation
    font-path+ truetype/arphic-gkai00mp
    font-path+ truetype/arphic-bkai00mp
    font-path+ truetype/emoji
    font-path+ opentype/
    font-path+ opentype/noto
[THEN]

\ skip over fonts when we can't load the font

: [TRY] ( -- )
    [: BEGIN  refill  WHILE
		source "[THEN]" str= 0= WHILE
		    interpret line-end-hook  REPEAT  THEN ;] catch
    IF  postpone [ELSE]  THEN ;

\ default font selection

\sans
\regular fonts= DejaVuSans.ttf|LiberationSans-Regular.ttf|NotoSans-Regular.ttf|DroidSans.ttf|Roboto-Medium.ttf
\italic fonts= DejaVuSans-Oblique.ttf|LiberationSans-Italic.ttf|NotoSans-Italic.ttf|Roboto-Italic.ttf
\bold fonts= DejaVuSans-Bold.ttf|LiberationSans-Bold.ttf|NotoSans-Bold.ttf|Roboto-Bold.ttf
\bold-italic fonts= DejaVuSans-BoldOblique.ttf|LiberationSans-BoldItalic.ttf|NotoSans-BoldItalic.ttf|Roboto-BoldItalic.ttf

\serif
\regular fonts= DejaVuSerif.ttf|LiberationSerif-Regular.ttf|NotoSerif-Regular.ttf
\bold fonts= DejaVuSerif-Bold.ttf|LiberationSerif-Bold.ttf|NotoSerif-Bold.ttf
\italic fonts= DejaVuSerif-Italic.ttf|LiberationSerif-Italic.ttf|NotoSerif-Italic.ttf
\bold-italic fonts= DejaVuSerif-BoldItalic.ttf|LiberationSerif-BoldItalic.ttf|NotoSerif-BoldItalic.ttf

\mono
\regular fonts= DejaVuSansMono.ttf|LiberationMono-Regular.ttf|DroidSansMono.ttf
\bold fonts= DejaVuSansMono-Bold.ttf|LiberationMono-Bold.ttf|DroidSansMono.ttf
\italic fonts= DejaVuSansMono-Oblique.ttf|LiberationMono-Italic.ttf|DroidSansMono.ttf
\bold-italic fonts= DejaVuSansMono-BoldOblique.ttf|LiberationMono-BoldItalic.ttf|DroidSansMono.ttf

120% to font-scaler
[TRY]
\simplified-chinese
2 font-lang >breakable
\sans
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    fonts[ssm]=same
{{  $A000  $2E80  $31390 $20000   $FB00  $F900   $FFF0  $FF00 }} 2/ +ranges
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE] \ android
    \regular fonts= gkai00mp.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    fonts[ssm]=same
{{  $A000  $2E80  $31390 $20000   $FB00  $F900   $FFF0  $FF00 }} 2/ +ranges
    \bold fonts= gkai00mp.ttf|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \italic fonts= gkai00mp.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold-italic fonts= gkai00mp.ttf|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[THEN] \ android
\serif
[IFDEF] android
    \regular fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE] \ android
    \regular fonts= gkai00mp.ttf|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold fonts= gkai00mp.ttf|NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \italic fonts= gkai00mp.ttf|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold-italic fonts= gkai00mp.ttf|NotoSerifSC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifSC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[THEN] \ android
\mono
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE] \ android
    \regular fonts= gkai00mp.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold fonts= gkai00mp.ttf|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \italic fonts= gkai00mp.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold-italic fonts= gkai00mp.ttf|NotoSansSC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[THEN] \ android
[THEN]

[TRY]
\traditional-chinese
2 font-lang >breakable
\sans
[IFDEF] android
    \regular fonts= NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE] \ android
    \regular fonts= bkai00mp.ttf|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold fonts= bkai00mp.ttf|NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \italic fonts= bkai00mp.ttf|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold-italic fonts= bkai00mp.ttf|NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
[THEN] \ android
\serif
[IFDEF] android
    \regular fonts= NotoSerifTC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold fonts= NotoSerifTC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifTC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic fonts= NotoSerifTC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic fonts= NotoSerifTC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifTC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
[ELSE] \ android
    \regular fonts= bkai00mp.ttf|NotoSerifTC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \italic font=same
    \bold fonts= bkai00mp.ttf|NotoSerifTC-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSerifTC-Regular.otf|NotoSerifCJK-Regular.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold-italic font=same
[THEN] \ android
\mono
[IFDEF] android
    \regular fonts= NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \italic font=same
    \bold fonts= NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
    \bold-italic font=same
[ELSE] \ android
    \regular fonts= bkai00mp.ttf|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \italic font=same
    \bold fonts= bkai00mp.ttf|NotoSansTC-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansTC-Regular.otf|NotoSansCJK-Regular.ttc
    \bold-italic font=same
[THEN] \ android
{{  $3130  $3100 }} 2/ +ranges \ bopomofo
:noname ( traditional simple -- )
    drop dup 1+ swap 1 +ranges ; is >sc
:noname ( simple traditional -- )
    2dup = IF  2drop  EXIT  THEN
    nip dup 1+ swap 1 +ranges ; is >tc
:noname ( traditional -- )
    dup 1+ swap 1 +ranges ; is >tc2
include unihan.fs
[THEN]

[TRY]
\japanese
2 font-lang >breakable
\sans
\regular fonts= gkai00mp.ttf|NotoSansJP-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
\italic font=same
\bold fonts= gkai00mp.ttf|NotoSansJP-Bold.otf|NotoSansCJK-Bold.ttc|NotoSansJP-Regular.otf|NotoSansCJK-Regular.ttc|DroidSansFallback.ttf
\bold-italic font=same
\serif
\regular fonts= gkai00mp.ttf|NotoSerifCJKjp-Regular.otf|NotoSerifCJK-Regular.ttc|DroidSansFallback.ttf
\italic font=same
\bold fonts= gkai00mp.ttf|NotoSerifCJKjp-Bold.otf|NotoSerifCJK-Bold.ttc|NotoSerifJP-Regular.otf|NotoSerifCJK-Regular.ttc|DroidSansFallback.ttf
\bold-italic font=same
\mono
{{ $3100 $3000  $3200 $31F0  $3244 $3220  $3380 $3280  $FFA0 $FF5F }} 2/ +ranges
[THEN]

[TRY]
\hangul
1 font-lang >breakable \ not breakable for ragged layout
\sans
\regular fonts= NotoSansKR-Regular.otf
fonts[ssm]=same
{{ $1200 $1100  $3190 $3130  $A980 $A960  $D7A4 $AC00  $D800 $D7B0 }} 2/ +ranges
\bold fonts= NotoSansKR-Bold.otf
\italic fonts= NotoSansKR-Regular.otf
\bold-italic fonts= NotoSansKR-Bold.otf
\serif
\regular fonts= NotoSerifCJKkr-Regular.otf
\bold fonts= NotoSerifCJKkr-Bold.otf
\italic fonts= NotoSerifCJKkr-Regular.otf
\bold-italic fonts= NotoSerifCJKkr-Bold.otf
\mono
\regular fonts= NotoSansMonoCJKkr-Regular.otf
\bold fonts= NotoSansMonoCJKkr-Bold.otf
\italic fonts= NotoSansMonoCJKkr-Regular.otf
\bold-italic fonts= NotoSansMonoCJKkr-Bold.otf
[THEN]

\ emojis and icons don't differ between different shapes and styles

110% to font-scaler

[TRY]
\emoji \regular
2 font-lang >breakable
\sans \regular
color-fonts= NotoColorEmoji.ttf|emojione-android.ttf|Twemoji.ttf|SamsungColorEmoji.ttf
fonts[ssm]=same
{{  $2C00  $2600  $20000 $1F000 }} 2/ +ranges
[THEN]

100% to font-scaler
[TRY]
\icons \regular
2 font-lang >breakable
\sans \regular
fonts= fa-merged-900.ttf
fonts[ssm]=same
{{ $F900 $F000 }} 2/ +ranges
[THEN]

[TRY]
\hebrew
\sans
\regular fonts= DejaVuSans.ttf|LiberationSans-Regular.ttf|NotoSansHebrew-Regular.ttf|DroidSans.ttf
fonts[ssm]=same
{{  $600  $590  $20AB $20AA  $FB50 $FB00 }} 2/ +ranges
\italic fonts= DejaVuSans-Oblique.ttf|LiberationSans-Italic.ttf|NotoSansHebrew-Italic.ttf
\bold fonts= DejaVuSans-Bold.ttf|LiberationSans-Bold.ttf|NotoSansHebrew-Bold.ttf
\bold-italic fonts= DejaVuSans-BoldOblique.ttf|LiberationSans-BoldItalic.ttf|NotoSansHebrew-BoldItalic.ttf

\serif
\regular fonts= DejaVuSerif.ttf|LiberationSerif-Regular.ttf|NotoSerifHebrew-Regular.ttf
\bold fonts= DejaVuSerif-Bold.ttf|LiberationSerif-Bold.ttf|NotoSerifHebrew-Bold.ttf
\italic fonts= DejaVuSerif-Italic.ttf|LiberationSerif-Italic.ttf|NotoSerifHebrew-Italic.ttf
\bold-italic fonts= DejaVuSerif-BoldItalic.ttf|LiberationSerif-BoldItalic.ttf|NotoSerifHebrew-BoldItalic.ttf

\mono
\regular fonts= DejaVuSansMono.ttf|LiberationMono-Regular.ttf|NotoSansHebrew-Regular.ttf
\bold fonts= DejaVuSansMono-Bold.ttf|LiberationMono-Bold.ttf|NotoSansHebrew-Bold.ttf
\italic fonts= DejaVuSansMono-Oblique.ttf|LiberationMono-Italic.ttf|NotoSansHebrew-Italic.ttf
\bold-italic fonts= DejaVuSansMono-BoldOblique.ttf|LiberationMono-BoldItalic.ttf|NotoSansHebrew-BoldItalic.ttf
[THEN]

[TRY]
\arabic
\sans
\regular fonts= DejaVuSans.ttf|LiberationSans-Regular.ttf|NotoSansArabic-Regular.ttf|DroidSans.ttf
fonts[ssm]=same
{{  $700  $600   $780  $750   $900  $8A0  $FE00 $FB50  $FF00 $FE70  $1EF00 $1EE00 }} 2/ +ranges
\italic fonts= DejaVuSans-Oblique.ttf|LiberationSans-Italic.ttf|NotoSansArabic-Italic.ttf
\bold fonts= DejaVuSans-Bold.ttf|LiberationSans-Bold.ttf|NotoSansArabic-Bold.ttf
\bold-italic fonts= DejaVuSans-BoldOblique.ttf|LiberationSans-BoldItalic.ttf|NotoSansArabic-BoldItalic.ttf

\serif
\regular fonts= DejaVuSerif.ttf|LiberationSerif-Regular.ttf|NotoSansArabic-Regular.ttf
\bold fonts= DejaVuSerif-Bold.ttf|LiberationSerif-Bold.ttf|NotoSansArabic-Bold.ttf
\italic fonts= DejaVuSerif-Italic.ttf|LiberationSerif-Italic.ttf|NotoSansArabic-Italic.ttf
\bold-italic fonts= DejaVuSerif-BoldItalic.ttf|LiberationSerif-BoldItalic.ttf|NotoSansArabic-BoldItalic.ttf

\mono
\regular fonts= DejaVuSansMono.ttf|LiberationMono-Regular.ttf|NotoSansArabic-Regular.ttf
\bold fonts= DejaVuSansMono-Bold.ttf|LiberationMono-Bold.ttf|NotoSansArabic-Bold.ttf
\italic fonts= DejaVuSansMono-Oblique.ttf|LiberationMono-Italic.ttf|NotoSansArabic-Italic.ttf
\bold-italic fonts= DejaVuSansMono-BoldOblique.ttf|LiberationMono-BoldItalic.ttf|NotoSansArabic-BoldItalic.ttf
[THEN]

\ all fonts here are Noto
"Noto" "family" replaces

[TRY]
\syriac \sans
\regular fonts= NotoSansSyriacWestern-Regular.ttf|NotoSansSyriacEstrangela-Regular.ttf|NotoSansSyriacEastern-Regular.ttf
fonts[ssm]=same
{{ $750 $700  $870 $860 }} 2/ +ranges
[THEN]

\armenian {{ $590 $530 }} 2/ "Armenian" fonts=template[rb]
\thaana {{ $7C0 $780 }} 2/ "Thaana" fonts=template[r]
\nko {{ $800 $7C0 }} 2/ "NKo" fonts=template[r]
\samaritan {{ $840 $800 }} 2/ "Samaritan" fonts=template[r]
\mandaic {{ $860 $840 }} 2/ "Mandaic" fonts=template[r]
\devanagari
{{ $980  $900  $1D00 $1CD0  $20BA $20B9  $A840 $A830  $A900 $A8E0 }} 2/
"Devanagari" fonts=template[rb]sans
\bengali {{ $A00 $980 }} 2/ "Bengali" fonts=template[rb]
\gurmukhi {{ $A80 $A00 }} 2/ "Gurmukhi" fonts=template[r]
\gujarati {{ $B00 $A80 }} 2/ "Gujarati" fonts=template[rb]
\oriya {{ $B80 $B00 }} 2/ "Oriya" fonts=template[r]
\tamil {{ $C00 $B80 }} 2/ "Tamil" fonts=template[rb]
\telugu {{ $C80 $C00 }} 2/ "Telugu" fonts=template[rb]
\kannada {{ $D00 $C80 }} 2/ "Kannada" fonts=template[rb]
\malayalam {{ $D80 $D00 }} 2/ "Malayalam" fonts=template[rb]
\sinhala {{ $E00 $D80 }} 2/ "Sinhala" fonts=template[rb]
\thai {{ $E80 $E00 }} 2/ "Thai" fonts=template[rb]
\lao {{ $F00 $E80 }} 2/ "Lao" fonts=template[rb]
\tibetan {{ $1000 $F00 }} 2/ "Tibetan" fonts=template[r]
\myanmar {{ $10A0 $1000 }} 2/ "Myanmar" fonts=template[rb]
\georgian {{ $1100 $10A0  $1CC0 $1C90 }} 2/ "Georgian" fonts=template[rb]
\ethiopic {{ $13A0 $1200 }} 2/ "Ethiopic" fonts=template[rb]
\cherokee {{ $1400 $13A0 }} 2/ "Cherokee" fonts=template[r]
\canadianaboriginal {{ $1680 $1400  $1900 $18B0 }} 2/ "CanadianAboriginal" fonts=template[r]
\ogham {{ $16A0 $1680 }} 2/ "Ogham" fonts=template[r]
\runic {{ $1700 $16A0 }} 2/ "Runic" fonts=template[r]
\tagalog {{ $1720 $1700 }} 2/ "Tagalog" fonts=template[r]
\hanunoo {{ $1740 $1720 }} 2/ "Hanunoo" fonts=template[r]
\buhid {{ $1760 $1740 }} 2/ "Buhid" fonts=template[r]
\tagbanwa {{ $1780 $1760 }} 2/ "Tagbanwa" fonts=template[r]
\khmer {{ $1800 $1780  $1A00 $19E0 }} 2/ "Khmer" fonts=template[rb]
\mongolian {{ $18B0 $1800 }} 2/ "Mongolian" fonts=template[r]
\limbu {{ $1950 $1900 }} 2/ "Limbu" fonts=template[r]
\taile {{ $1980 $1950 }} 2/ "TaiLe" fonts=template[r]
\newtailue {{ $19E0 $1980 }} 2/ "NewTaiLue" fonts=template[r]
\buginese {{ $1A20 $1A00 }} 2/ "Buginese" fonts=template[r]
\taitham {{ $1AB0 $1A20 }} 2/ "TaiTham" fonts=template[r]
\balinese {{ $1B80 $1B00 }} 2/ "Balinese" fonts=template[r]
\sundanese {{ $1BC0 $1B80  $1CD0 $1CC0 }} 2/ "Sundanese" fonts=template[r]
\batak {{ $1C00 $1BC0 }} 2/ "Batak" fonts=template[r]
\lepcha {{ $1C50 $1C00 }} 2/ "Lepcha" fonts=template[r]
\olchiki {{ $1C80 $1C50 }} 2/ "OlChiki" fonts=template[r]
\glagolitic {{ $2C00 $60 bounds $1E000 $30 bounds }} 2/ "Glagolitic" fonts=template[r]
\coptic {{ $2C80 $80 bounds $370 $10 bounds }} 2/ "Coptic" fonts=template[r]
\tifinagh {{ $2D30 $40 bounds }} 2/ "Tifinagh" fonts=template[r]
\lisu {{ $A4D0 $30 bounds }} 2/ "Lisu" fonts=template[r]
\vai {{ $A640 $A500 }} 2/ "Vai" fonts=template[r]
120% to font-scaler
\bamum {{ $A700 $A6A0  $16800 $240 bounds }} 2/ "Bamum" fonts=template[r]
100% to font-scaler
\sylotinagri {{ $A800 $30 bounds }} 2/ "SylotiNagri" fonts=template[r]
\phagspa {{ $A840 $40 bounds }} 2/ "PhagsPa" fonts=template[r]
\saurashtra {{ $A8E0 $A880 }} 2/ "Saurashtra" fonts=template[r]
\kayahli {{ $A900 $30 bounds }} 2/ "Kharoshthi" fonts=template[r]
\rejang {{ $A960 $A930 }} 2/ "Rejang" fonts=template[r]
\javanese {{ $A9E0 $A980 }} 2/ "Javanese" fonts=template[r]
\cham {{ $AA00 $60 bounds }} 2/ "Cham" fonts=template[rb]sans
\taiviet {{ $AAE0 $AA80 }} 2/ "TaiViet" fonts=template[r]
\meeteimayek {{ $ABC0 $AC00  $AAE0 $AB00 }} 2/ "MeeteiMayek" fonts=template[r]
\lycian {{ $10280 $20 bounds }} 2/ "Lydian" fonts=template[r]
\carian {{ $102A0 $40 bounds }} 2/ "Carian" fonts=template[r]
\olditalic {{ $10300 $30 bounds }} 2/ "OldItalic" fonts=template[r]
\gothic {{ $10330 $20 bounds }} 2/ "Gothic" fonts=template[r]
\ugaritic {{ $10380 $20 bounds }} 2/ "Ugaritic" fonts=template[r]
\oldpersian {{ $103A0 $40 bounds }} 2/ "OldPersian" fonts=template[r]
\deseret {{ $10400 $50 bounds }} 2/ "Deseret" fonts=template[r]
\shavian {{ $10450 $30 bounds }} 2/ "Shavian" fonts=template[r]
\osmanya {{ $104B0 $10480 }} 2/ "Osmanya" fonts=template[r]
\osage {{ $10500 $104B0 }} 2/ "Osage" fonts=template[r]
\cypriot {{ $10800 $40 bounds }} 2/ "Cypriot" fonts=template[r]
\imperialaramaic {{ $10840 $20 bounds }} 2/ "ImperialAramaic" fonts=template[r]
\phoenician {{ $10900 $20 bounds }} 2/ "Phoenician" fonts=template[r]
\lydian {{ $10920 $20 bounds }} 2/ "Lydian" fonts=template[r]
\kharoshthi {{ $10A00 $60 bounds }} 2/ "Lydian" fonts=template[r]
\oldsoutharabian {{ $10A60 $20 bounds }} 2/ "OldSouthArabian" fonts=template[r]
\avestan {{ $10B00 $40 bounds }} 2/ "Avestan" fonts=template[r]
\inscriptionalparthian {{ $10B40 $20 bounds }} 2/ "InscriptionalParthian" fonts=template[r]
\inscriptionalpahlavi {{ $10B60 $20 bounds }} 2/ "InscriptionalPahlavi" fonts=template[r]
\oldturkic {{ $10C00 $50 bounds }} 2/ "OldTurkic" fonts=template[r]
\brahmi {{ $11000 $80 bounds }} 2/ "Brahmi" fonts=template[r]
\kaithi {{ $110D0 $11080 }} 2/ "Kaithi" fonts=template[r]
\chakma {{ $11100 $50 bounds }} 2/ "Chakma" fonts=template[r]
120% to font-scaler
\yi {{ $A4D0 $A000 }} 2/ "Yi" fonts=template[r]
200% to font-scaler
\cuneiform {{ $12000 $550 bounds }} 2/ "Cuneiform" fonts=template[r]
\egyptianhieroglyphs {{ $13000 $430 bounds }} 2/ "EgyptianHieroglyphs" fonts=template[r]
\anatolianhieroglyphs {{ $14400 $280 bounds }} 2/ "AnatolianHieroglyphs" fonts=template[r]
100% to font-scaler
\adlam {{ $1E900 $60 bounds }} 2/ "Adlam" fonts=template[r]

\latin \sans \regular

previous r> set-current
