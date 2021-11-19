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
fontlang: \symbols
fontlang: \symbols2
fontlang: \armenian
fontlang: \hebrew
fontlang: \arabic
fontlang: \arabic#
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

: .ttf ( -- ) "ttf" "ext" replaces ;
: .ttc ( -- ) "ttc" "ext" replaces ;
: .otf ( -- ) "otf" "ext" replaces ;
.ttf

: try-fonts= ( "font1|font2|..." -- )
    >in @ >r
    [: fonts-parse .ttf fonts-scan ;] catch
    IF  r@ >in ! [: fonts-parse .otf fonts-scan ;] catch
	IF  r@ >in ! [: fonts-parse .ttc fonts-scan ;] catch
	    .ttf throw  THEN  THEN rdrop ;
: fonts= ( "font1|font2|..." -- )
    try-fonts= ['] bw-font   ?define-font ;
: color-fonts= ( "font1|font2|..." -- )
    try-fonts= ['] bgra-font ?define-font ;
: font=same ( -- )
    last-font font-index font[] $[] ! ;
: fonts[bi]=same ( -- )
    \bold font=same \italic font=same \bold-italic font=same \regular ;
: fonts[ssm]=same ( -- )
    fonts[bi]=same
    \serif font=same fonts[bi]=same
    \mono  font=same fonts[bi]=same
    \sans ;

"" "subset" replaces
"" "ui" replaces

: font-try ( -- throw-code )
    [:  .ttf
	"%family%%style%%lang%%ui%-%shape%%subset%.%ext%" fonts-scan ;] catch
    IF
	[:  .otf
	    "%family%%style%%lang%%ui%-%shape%%subset%.%ext%" fonts-scan ;] catch
	IF
	    [:  .ttc
		"%family%%style%%lang%%ui%-%shape%%subset%.%ext%" fonts-scan ;] catch .ttf  EXIT
	THEN
    THEN .ttf 0 ;

: font=%% ( -- )
    "" "ui" replaces font-try IF
	"UI" "ui" replaces font-try IF
	    "-VF" "ui" replaces font-try
	    "" "ui" replaces  throw  THEN  THEN
    "" "ui" replaces ['] bw-font ?define-font ;
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
: ?failed ( throwcode -- )
    IF  clearstack
	[: "%lang%" $substitute drop type ."  failed" cr ;]
	['] execute-theme-color do-debug
    THEN ;
: fonts=template[rb] ( range1 .. rangen n addr u -- ... throwcode )
    "lang" replaces
    [:  \sans  "Sans"  "style" replaces fonts=shapes[rb]
	\serif "Serif" "style" replaces fonts=shapes[rb]
	\mono  "Sans"  "style" replaces fonts=shapes[rb] drop ;] catch ?failed ;
: fonts=template[rb]sans ( range1 .. rangen n addr u -- )
    [: "lang" replaces "Sans"  "style" replaces .ttf
	\sans  fonts=shapes[rb]
	\serif fonts=shapes[rb]
	\mono  fonts=shapes[rb]
	drop ;] catch ?failed ;
: fonts=template[r] ( range1 .. rangen n addr u -- )
    [: "lang" replaces
	\sans  "Sans"  "style" replaces fonts=shapes[r] ;] catch ?failed ;

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
    font-path+ truetype/ancient-scripts
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
\regular fonts= NotoSans-Regular.%ext%|DroidSans.%ext%|Roboto-Medium.%ext%|DejaVuSans.%ext%|LiberationSans-Regular.%ext%
\italic fonts= NotoSans-Italic.%ext%|Roboto-Italic.%ext%|DejaVuSans-Oblique.%ext%|LiberationSans-Italic.%ext%
\bold fonts= NotoSans-Bold.%ext%|Roboto-Bold.%ext%|DejaVuSans-Bold.%ext%|LiberationSans-Bold.%ext%
\bold-italic fonts= NotoSans-BoldItalic.%ext%|Roboto-BoldItalic.%ext%|DejaVuSans-BoldOblique.%ext%|LiberationSans-BoldItalic.%ext%

\serif
\regular fonts= NotoSerif-Regular.%ext%|DejaVuSerif.%ext%|LiberationSerif-Regular.%ext%
\bold fonts= NotoSerif-Bold.%ext%|DejaVuSerif-Bold.%ext%|LiberationSerif-Bold.%ext%
\italic fonts= NotoSerif-Italic.%ext%|DejaVuSerif-Italic.%ext%|LiberationSerif-Italic.%ext%
\bold-italic fonts= NotoSerif-BoldItalic.%ext%|DejaVuSerif-BoldItalic.%ext%|LiberationSerif-BoldItalic.%ext%

\mono
\regular fonts= DejaVuSansMono.%ext%|LiberationMono-Regular.%ext%|NotoSansMono-Regular.%ext%|DroidSansMono.%ext%
\bold fonts= DejaVuSansMono-Bold.%ext%|LiberationMono-Bold.%ext%|NotoSansMono-Bold.%ext%|DroidSansMono.%ext%
\italic fonts= DejaVuSansMono-Oblique.%ext%|LiberationMono-Italic.%ext%|DroidSansMono.%ext%
\bold-italic fonts= DejaVuSansMono-BoldOblique.%ext%|LiberationMono-BoldItalic.%ext%|DroidSansMono.%ext%

120% to font-scaler
[TRY]
\simplified-chinese
2 font-lang >breakable
\sans
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    fonts[ssm]=same
{{  $A000  $2E80  $31390 $20000   $FB00  $F900   $FFF0  $FF00 }} 2/ +ranges
    \bold fonts= NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \italic fonts= NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold-italic fonts= NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
[ELSE] \ android
    \regular fonts= gkai00mp.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    fonts[ssm]=same
{{  $A000  $2E80  $31390 $20000   $FB00  $F900   $FFF0  $FF00 }} 2/ +ranges
    \bold fonts= gkai00mp.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \italic fonts= gkai00mp.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold-italic fonts= gkai00mp.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
[THEN] \ android
\serif
[IFDEF] android
    \regular fonts= NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold fonts= NotoSerifSC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \italic fonts= NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold-italic fonts= NotoSerifSC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
[ELSE] \ android
    \regular fonts= gkai00mp.%ext%|NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold fonts= gkai00mp.%ext%|NotoSerifSC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \italic fonts= gkai00mp.%ext%|NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold-italic fonts= gkai00mp.%ext%|NotoSerifSC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifSC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
[THEN] \ android
\mono
[IFDEF] android
    \regular fonts= NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold fonts= NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \italic fonts= NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold-italic fonts= NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
[ELSE] \ android
    \regular fonts= gkai00mp.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold fonts= gkai00mp.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \italic fonts= gkai00mp.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold-italic fonts= gkai00mp.%ext%|NotoSansSC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansSC-Regular.%ext%|NotoSansCJK-Regular.%ext%
[THEN] \ android
[THEN]

[TRY]
\traditional-chinese
2 font-lang >breakable
\sans
[IFDEF] android
    \regular fonts= NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold fonts= NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \italic fonts= NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold-italic fonts= NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
[ELSE] \ android
    \regular fonts= bkai00mp.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold fonts= bkai00mp.%ext%|NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \italic fonts= bkai00mp.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold-italic fonts= bkai00mp.%ext%|NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
[THEN] \ android
\serif
[IFDEF] android
    \regular fonts= NotoSerifTC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold fonts= NotoSerifTC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifTC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \italic fonts= NotoSerifTC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold-italic fonts= NotoSerifTC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifTC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
[ELSE] \ android
    \regular fonts= bkai00mp.%ext%|NotoSerifTC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \italic font=same
    \bold fonts= bkai00mp.%ext%|NotoSerifTC-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSerifTC-Regular.%ext%|NotoSerifCJK-Regular.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold-italic font=same
[THEN] \ android
\mono
[IFDEF] android
    \regular fonts= NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \italic font=same
    \bold fonts= NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
    \bold-italic font=same
[ELSE] \ android
    \regular fonts= bkai00mp.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \italic font=same
    \bold fonts= bkai00mp.%ext%|NotoSansTC-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansTC-Regular.%ext%|NotoSansCJK-Regular.%ext%
    \bold-italic font=same
[THEN] \ android
{{  $3130  $3100 }} 2/ +ranges \ bopomofo
:noname ( traditional simple -- )
    2dup = IF  2drop  EXIT  THEN
    drop dup 1+ swap 1 +ranges ; is >sc
' 2drop is >tc
' drop is >tc2
read-unihan
[THEN]

[TRY]
\japanese
2 font-lang >breakable
\sans
\regular fonts= gkai00mp.%ext%|NotoSansJP-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
\italic font=same
\bold fonts= gkai00mp.%ext%|NotoSansJP-Bold.%ext%|NotoSansCJK-Bold.%ext%|NotoSansJP-Regular.%ext%|NotoSansCJK-Regular.%ext%|DroidSansFallback.%ext%
\bold-italic font=same
\serif
\regular fonts= gkai00mp.%ext%|NotoSerifCJKjp-Regular.%ext%|NotoSerifCJK-Regular.%ext%|DroidSansFallback.%ext%
\italic font=same
\bold fonts= gkai00mp.%ext%|NotoSerifCJKjp-Bold.%ext%|NotoSerifCJK-Bold.%ext%|NotoSerifJP-Regular.%ext%|NotoSerifCJK-Regular.%ext%|DroidSansFallback.%ext%
\bold-italic font=same
\mono
{{ $3100 $3000  $3200 $31F0  $3244 $3220  $3380 $3280  $FFA0 $FF5F }} 2/ +ranges
[THEN]

[TRY]
\hangul
1 font-lang >breakable \ not breakable for ragged layout
\sans
\regular fonts= NotoSansKR-Regular.%ext%
fonts[ssm]=same
{{ $1200 $1100  $3190 $3130  $A980 $A960  $D7A4 $AC00  $D800 $D7B0 }} 2/ +ranges
\bold fonts= NotoSansKR-Bold.%ext%
\italic fonts= NotoSansKR-Regular.%ext%
\bold-italic fonts= NotoSansKR-Bold.%ext%
\serif
\regular fonts= NotoSerifCJKkr-Regular.%ext%
\bold fonts= NotoSerifCJKkr-Bold.%ext%
\italic fonts= NotoSerifCJKkr-Regular.%ext%
\bold-italic fonts= NotoSerifCJKkr-Bold.%ext%
\mono
\regular fonts= NotoSansMonoCJKkr-Regular.%ext%
\bold fonts= NotoSansMonoCJKkr-Bold.%ext%
\italic fonts= NotoSansMonoCJKkr-Regular.%ext%
\bold-italic fonts= NotoSansMonoCJKkr-Bold.%ext%
[THEN]

\ emojis and icons don't differ between different shapes and styles

110% to font-scaler

[TRY]
\emoji \regular
font-lang to emoji-font#
2 font-lang >breakable
\sans \regular
color-fonts= NotoColorEmoji.%ext%|emojione-android.%ext%|Twemoji.%ext%|SamsungColorEmoji.%ext%
fonts[ssm]=same
{{ $20000 $1F000 }} 2/ +ranges
[THEN]

100% to font-scaler
[TRY]
\icons \regular
2 font-lang >breakable
\sans \regular
fonts= fa-merged-900.%ext%
fonts[ssm]=same
{{ $F900 $F000 }} 2/ +ranges
[THEN]

"Noto" "family" replaces
[IFDEF] android
    "-Subsetted" "subset" replaces
    \symbols \sans \regular {{ $20D0 $21 bounds  $2100 $50 bounds  $2B5A $2190    $4DC0 $40 bounds  $10200 $10140  $1D250 $1D000  $1D380 $1D300  $1D400 $400 bounds  }} 2/ "Symbols" fonts=template[r]
    2 font-lang >breakable
    font-lang to symbol-font#
    "" "subset" replaces
[ELSE]
    \symbols \sans \regular {{ $2150 $40 bounds  $2190 #10 bounds  $2300 $100 bounds  $2460 $A0 bounds  $2600 $100 bounds $1F100 $AC bounds  $1F700 $80 bounds }} 2/ "Symbols" fonts=template[rb]sans
    \ Android: NotoSansSymbols-Regular-Subsetted.%ext% NotoSansSymbols-Regular-Subsetted2.%ext%
    2 font-lang >breakable
    font-lang to symbol-font#
    \symbols2 {{ $2316 $1 bounds  $2318 $1 bounds  $231A $2 bounds  $2324 $5 bounds $232B $1 bounds  $237B $1 bounds  $237D $3 bounds  $2394 $1 bounds  $23CE $2 bounds  $23E9 $2 bounds  $23ED $3 bounds  $23F1 $E bounds  $2400 $60 bounds  $25A0 $60 bounds  $2600 $A bounds  $260E $5 bounds  $2614 $10 bounds  $2630 $8 bounds  $263C $1 bounds  $2669 $2654   $267F $12 bounds  $269E $4 bounds  $26AA $3 bounds  $26CE $26BD  $26E1 $26CF  $2700 $C0 bounds  $2800 $100 bounds  $2B00 $100 bounds  $4DC0 $40 bounds  $10140 $C0 bounds  $102E0 $20 bounds  $10E60 $20 bounds  $1D300 $80 bounds }} 2/ "Symbols2" fonts=template[r]
    2 font-lang >breakable
[THEN]

[TRY]
\hebrew
\sans
\regular fonts= DejaVuSans.%ext%|LiberationSans-Regular.%ext%|NotoSansHebrew-Regular.%ext%|DroidSans.%ext%
fonts[ssm]=same
{{  $600  $590  $20AB $20AA  $FB50 $FB00 }} 2/ +ranges
\italic fonts= DejaVuSans-Oblique.%ext%|LiberationSans-Italic.%ext%|NotoSansHebrew-Italic.%ext%
\bold fonts= DejaVuSans-Bold.%ext%|LiberationSans-Bold.%ext%|NotoSansHebrew-Bold.%ext%
\bold-italic fonts= DejaVuSans-BoldOblique.%ext%|LiberationSans-BoldItalic.%ext%|NotoSansHebrew-BoldItalic.%ext%

\serif
\regular fonts= DejaVuSerif.%ext%|LiberationSerif-Regular.%ext%|NotoSerifHebrew-Regular.%ext%
\bold fonts= DejaVuSerif-Bold.%ext%|LiberationSerif-Bold.%ext%|NotoSerifHebrew-Bold.%ext%
\italic fonts= DejaVuSerif-Italic.%ext%|LiberationSerif-Italic.%ext%|NotoSerifHebrew-Italic.%ext%
\bold-italic fonts= DejaVuSerif-BoldItalic.%ext%|LiberationSerif-BoldItalic.%ext%|NotoSerifHebrew-BoldItalic.%ext%

\mono
\regular fonts= LiberationMono-Regular.%ext%|NotoSansHebrew-Regular.%ext%
\bold fonts= LiberationMono-Bold.%ext%|NotoSansHebrew-Bold.%ext%
\italic fonts= LiberationMono-Italic.%ext%|NotoSansHebrew-Italic.%ext%
\bold-italic fonts= LiberationMono-BoldItalic.%ext%|NotoSansHebrew-BoldItalic.%ext%
[THEN]

[TRY]
\arabic
\sans
\regular fonts= NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%|DejaVuSans.%ext%|LiberationSans-Regular.%ext%|DroidKufi-Regular.%ext%|DroidSans.%ext%
fonts[ssm]=same
{{  $700  $600   $780  $750   $900  $8A0  $FE00 $FB50  $FF00 $FE70  $1EF00 $1EE00 }} 2/ +ranges
\italic fonts= NotoNastaliqUrdu-Regular.%ext%|NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%|DejaVuSans-Oblique.%ext%|LiberationSans-Italic.%ext%|DroidKufi-Regular.%ext%
\bold fonts= NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Bold.%ext%|DejaVuSans-Bold.%ext%|LiberationSans-Bold.%ext%|DroidKufi-Bold.%ext%
\bold-italic fonts= NotoNastaliqUrdu-Regular.%ext%|NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Bold.%ext%|DejaVuSans-BoldOblique.%ext%|LiberationSans-BoldItalic.%ext%|DroidKufi-Bold.%ext%

\serif
\regular fonts= NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%|DroidNaskh-Regular.%ext%
\bold fonts= NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Regular.%ext%|DroidNaskh-Regular.%ext%
\italic fonts= NotoSansArabic-Italic.%ext%|NotoNaskhArabic-Bold.%ext%|DroidNaskh-Bold.%ext%
\bold-italic fonts= NotoSansArabic-BoldItalic.%ext%|NotoNaskhArabic-Bold.%ext%|DroidNaskh-Bold.%ext%

\mono
\regular fonts= DejaVuSansMono.%ext%|DroidKufi-Regular.%ext%|NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%
\bold fonts= DejaVuSansMono-Bold.%ext%|DroidKufi-Regular.%ext%|NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Regular.%ext%
\italic fonts= DejaVuSansMono-Oblique.%ext%|DroidKufi-Bold.%ext%|NotoSansArabic-Italic.%ext%|NotoNaskhArabic-Bold.%ext%
\bold-italic fonts= DejaVuSansMono-BoldOblique.%ext%|DroidKufi-Bold.%ext%|NotoSansArabic-BoldItalic.%ext%|NotoNaskhArabic-Bold.%ext%
[THEN]

[TRY]
\arabic#
harfbuzz:HB_DIRECTION_LTR font-bidi font-lang + c!
\sans
\regular fonts= NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%|DejaVuSans.%ext%|LiberationSans-Regular.%ext%|DroidKufi-Regular.%ext%|DroidSans.%ext%
fonts[ssm]=same
{{  $660 #13 bounds  $609 #2 bounds  $6F0 #10 bounds }} 2/ +ranges
\italic fonts= NotoSansArabic-Italic.%ext%|NotoNaskhArabic-Regular.%ext%|DejaVuSans-Oblique.%ext%|LiberationSans-Italic.%ext%|DroidKufi-Regular.%ext%
\bold fonts= NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Bold.%ext%|DejaVuSans-Bold.%ext%|LiberationSans-Bold.%ext%|DroidKufi-Bold.%ext%
\bold-italic fonts= NotoSansArabic-BoldItalic.%ext%|NotoNaskhArabic-Bold.%ext%|DejaVuSans-BoldOblique.%ext%|LiberationSans-BoldItalic.%ext%|DroidKufi-Bold.%ext%

\serif
\regular fonts= NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%|DroidNaskh-Regular.%ext%
\bold fonts= NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Regular.%ext%|DroidNaskh-Regular.%ext%
\italic fonts= NotoSansArabic-Italic.%ext%|NotoNaskhArabic-Bold.%ext%|DroidNaskh-Bold.%ext%
\bold-italic fonts= NotoSansArabic-BoldItalic.%ext%|NotoNaskhArabic-Bold.%ext%|DroidNaskh-Bold.%ext%

\mono
\regular fonts= DejaVuSansMono.%ext%|DroidKufi-Regular.%ext%|NotoSansArabic-Regular.%ext%|NotoNaskhArabic-Regular.%ext%
\bold fonts= DejaVuSansMono-Bold.%ext%|DroidKufi-Regular.%ext%|NotoSansArabic-Bold.%ext%|NotoNaskhArabic-Regular.%ext%
\italic fonts= DejaVuSansMono-Oblique.%ext%|DroidKufi-Bold.%ext%|NotoSansArabic-Italic.%ext%|NotoNaskhArabic-Bold.%ext%
\bold-italic fonts= DejaVuSansMono-BoldOblique.%ext%|DroidKufi-Bold.%ext%|NotoSansArabic-BoldItalic.%ext%|NotoNaskhArabic-Bold.%ext%
[THEN]

\ all fonts here are Noto

[TRY]
\syriac \sans
\regular fonts= NotoSansSyriacWestern-Regular.%ext%|NotoSansSyriacEstrangela-Regular.%ext%|NotoSansSyriacEastern-Regular.%ext%
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
\gurmukhi {{ $A80 $A00 }} 2/ "Gurmukhi" fonts=template[rb]sans
\gujarati {{ $B00 $A80 }} 2/ "Gujarati" fonts=template[rb]
\oriya {{ $B80 $B00 }} 2/ "Oriya" fonts=template[rb]sans
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
harfbuzz:HB_DIRECTION_TTB font-bidi' font-lang + c!
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
\kayahli {{ $A900 $30 bounds }} 2/ "KayahLi" fonts=template[r]
\rejang {{ $A960 $A930 }} 2/ "Rejang" fonts=template[r]
\javanese {{ $A9E0 $A980 }} 2/ "Javanese" fonts=template[r]
\cham {{ $AA00 $60 bounds }} 2/ "Cham" fonts=template[rb]sans
\taiviet {{ $AAE0 $AA80 }} 2/ "TaiViet" fonts=template[r]
\meeteimayek {{ $AC00 $ABC0  $AB00 $AAE0 }} 2/ "MeeteiMayek" fonts=template[r]
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
\kharoshthi {{ $10A00 $60 bounds }} 2/ "Kharoshthi" fonts=template[r]
\oldsoutharabian {{ $10A60 $20 bounds }} 2/ "OldSouthArabian" fonts=template[r]
\avestan {{ $10B00 $40 bounds }} 2/ "Avestan" fonts=template[r]
\inscriptionalparthian {{ $10B40 $20 bounds }} 2/ "InscriptionalParthian" fonts=template[r]
\inscriptionalpahlavi {{ $10B60 $20 bounds }} 2/ "InscriptionalPahlavi" fonts=template[r]
\oldturkic {{ $10C00 $50 bounds }} 2/ "OldTurkic" fonts=template[r]
\brahmi {{ $11000 $80 bounds }} 2/ "Brahmi" fonts=template[r]
\kaithi {{ $110D0 $11080 }} 2/ "Kaithi" fonts=template[r] \ Kaithi ASCII digits are special
\chakma {{ $11100 $50 bounds }} 2/ "Chakma" fonts=template[r]
120% to font-scaler
\yi {{ $A4D0 $A000 }} 2/ "Yi" fonts=template[r]
2 font-lang >breakable
\cuneiform {{ $12000 $550 bounds }} 2/ "Cuneiform" fonts=template[r]
2 font-lang >breakable
200% to font-scaler
[TRY]
\egyptianhieroglyphs
2 font-lang >breakable
\ Aegyptus is free only for personal use. We use it when you have it
\sans \regular fonts= Aegyptus.%ext%|AegyptusR_hint.%ext%|NotoSansEgyptianHieroglyphs-Regular.%ext%
{{ $13000 $440 bounds }} 2/ +ranges
fonts[ssm]=same
\sans \bold fonts= AegyptusBold.%ext%|AegyptusB_hint.%ext%
[THEN]
\anatolianhieroglyphs {{ $14400 $280 bounds }} 2/ "AnatolianHieroglyphs" fonts=template[r]
2 font-lang >breakable
100% to font-scaler
\adlam {{ $1E900 $60 bounds }} 2/ "Adlam" fonts=template[r]

\latin \sans \regular

previous r> set-current
