\ MINOS2 font style

\ Author: Bernd Paysan
\ Copyright (C) 2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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

require minos2/widgets.fs
require unix/cstr.fs

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
fontlang: \iconbrands
fontlang: \symbols
fontlang: \symbols2
fontlang: \math
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
    over texture_font_t-hb_font !
    ( dup texture_font_t-hb_font @ hb_ft_font_changed ) ;
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
	$FFFD of  0  endof
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
Variable font-ext$
Variable font-prefix

${GFORTHFONTS} 2dup d0= [IF] 2drop
    ${container} "flatpak" str=
    [IF] font-prefix path= /usr/share/fonts|/run/host/fonts
    [ELSE] font-prefix path= /usr/share/fonts/ [THEN]
[ELSE]
    ':' 0 substc font-prefix also-path
[THEN]
${GFORTHFONTEXT} 2dup d0= [IF] 2drop "ttf:otf:ttc:woff:woff2" [THEN]
font-ext$ $!

also freetype-gl
: font-path+ ( "font" -- )
    parse-name
    2dup absolute-file? 0= IF
	font-prefix open-path-file  ?EXIT  rot close-file throw  THEN
    2dup open-dir 0= IF
	close-dir throw font-path also-path
    ELSE  drop 2drop  THEN ;
: open-font-ofile ( -- fid ior )
    '.' ofile c$+!
    font-ext$ ':'
    ofile $@len 0 no-file# {: d^ result :} result [{: len result :}l
	result @ no-file# = IF
	    ofile $+! open-ofile dup IF  len ofile $!len  THEN
	    result 2!
	ELSE  2drop  THEN
    ;] $iter result 2@ ;

: ?font ( addr u -- addr' u' true / false )
    ['] open-font-ofile font-path execute-path-file 0= IF
	rot close-file throw tilde_cstr cstring>sstring true
    ELSE
	false
    THEN ;
: font-loader ( fontaddr atlas -- fontaddr )
    swap { fontdes }
    fontdes cell+ $@
    0 font-size# font-size% f* fontdes 2 th f@ f*
    open-font referenced { font }
    font 0 fontdes $[] !
    font-sizes# 1 ?DO
	font I font-size# font-size% f* fontdes 2 th f@ f*
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

: try-fonts= ( "font1|font2|..." -- )
    fonts-parse fonts-scan ;
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
    [: "%family%%style%%lang%%ui%-%shape%%subset%" fonts-scan ;] catch ;

: font=%% ( -- )
    "" "ui" replaces font-try IF
	"UI" "ui" replaces font-try IF
	    "" "ui" replaces  "VF" "shape" replaces  font-try
	    throw  THEN  THEN
    "" "ui" replaces ['] bw-font ?define-font ;
: +ranges ( range1 .. rangen n -- )
    0 ?DO  font-lang -rot +range  LOOP ;
: fonts=shapes[rb] ( range1 .. rangen n -- 0 )
    2* n>r \regular "Regular" "shape" replaces font=%%
    r@ IF  fonts[ssm]=same nr> 2/ +ranges  ELSE  rdrop  THEN  0
    \italic font=same
    \bold "Bold" "shape" replaces font=%%
    \bold-italic font=same ;
: fonts=shapes[r] ( range1 .. rangen n -- 0 )
    2* n>r \regular "Regular" "shape" replaces font=%%
    nr> 2/ +ranges fonts[ssm]=same ;
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
    [: "lang" replaces "Sans"  "style" replaces
	\sans  fonts=shapes[rb]
	\serif fonts=shapes[rb]
	\mono  fonts=shapes[rb]
	drop ;] catch ?failed ;
: fonts=template[r] ( range1 .. rangen n addr u -- )
    [: "lang" replaces
	\sans  "Sans"  "style" replaces fonts=shapes[r] ;] catch ?failed ;
: fonts=template[rb]?serif ( range1 .. rangen n addr u -- )
    "lang" replaces
    dup 2* 1+ 0 DO  I' 1- pick  LOOP  2* n>r
    [:	"Sans"  "style" replaces
	\sans  fonts=shapes[rb]
	\serif fonts=shapes[rb]
	\mono  fonts=shapes[rb]
	drop ;] catch >r clearstack r> IF
	nr> 2/
	[: "Serif"  "style" replaces
	    \sans  fonts=shapes[rb]
	    \serif fonts=shapes[rb]
	    \mono  fonts=shapes[rb]
	    drop ;] catch ?failed
    ELSE
	nr> 2/ 0 ?DO  2drop  LOOP
    THEN ;

previous

font-path+ ~/.fonts

[IFDEF] android
    font-prefix clear-path
    font-path+ /system/fonts
    "minos2/fonts" open-fpath-file 0=
    [IF]  font-path also-path close-file throw  [THEN]
[ELSE]
    font-path+ truetype
    font-path+ truetype/dejavu
    font-path+ truetype/noto
    font-path+ truetype/droid
    font-path+ truetype/liberation
    font-path+ truetype/arphic-gkai00mp
    font-path+ truetype/arphic-bkai00mp
    font-path+ truetype/emoji
    font-path+ truetype/ancient-scripts
    font-path+ opentype
    font-path+ opentype/noto
    font-path+ texlive-fontawesome5
    font-path+ ttf
    font-path+ noto
    font-path+ dejavu
    font-path+ openemoji
    font-path+ ttf-dejavu
    font-path+ TTF
[THEN]

\ skip over fonts when we can't load the font

: [TRY] ( -- )
    [: BEGIN  refill  WHILE
		source "[THEN]" str= source "[ELSE]" str= d0= WHILE
		    interpret line-end-hook  REPEAT  THEN ;] catch
    IF  postpone [ELSE]  THEN ;

0 warnings !@ get-current
[struct]-voc set-current
"[IF]" [struct]-voc find-name-in Alias [TRY] immediate
set-current warnings !

\ default font selection

\sans
\regular fonts= NotoSans-Regular|SourceSansPro-Regular|DroidSans|Roboto-Medium|DejaVuSans|LiberationSans-Regular
\italic fonts= NotoSans-Italic|SourceSansPro-Italic|Roboto-Italic|DejaVuSans-Oblique|LiberationSans-Italic
\bold fonts= NotoSans-Bold|SourceSansPro-Bold|Roboto-Bold|DejaVuSans-Bold|LiberationSans-Bold
\bold-italic fonts= NotoSans-BoldItalic|SourceSansPro-BoldItalic|Roboto-BoldItalic|DejaVuSans-BoldOblique|LiberationSans-BoldItalic

\serif
\regular fonts= NotoSerif-Regular|DejaVuSerif|LiberationSerif-Regular
\bold fonts= NotoSerif-Bold|DejaVuSerif-Bold|LiberationSerif-Bold
\italic fonts= NotoSerif-Italic|DejaVuSerif-Italic|LiberationSerif-Italic
\bold-italic fonts= NotoSerif-BoldItalic|DejaVuSerif-BoldItalic|LiberationSerif-BoldItalic

\mono
\regular fonts= DejaVuSansMono|LiberationMono-Regular|NotoSansMono-Regular|DroidSansMono
\bold fonts= DejaVuSansMono-Bold|LiberationMono-Bold|NotoSansMono-Bold|DroidSansMono
\italic fonts= DejaVuSansMono-Oblique|LiberationMono-Italic|DejaVuSansMono|NotoSansMono-Regular|DroidSansMono
\bold-italic fonts= DejaVuSansMono-BoldOblique|LiberationMono-BoldItalic|DejaVuSansMono-Bold|NotoSansMono-Bold|DroidSansMono

120% to font-scaler
[IFDEF] android
[TRY]
\simplified-chinese
2 font-lang >breakable
\sans
    \regular fonts= NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    fonts[ssm]=same
    {{  $A000  $2E80  $31390 $20000   $FB00  $F900   $FFF0  $FF00 }} 2/ +ranges
[ELSE]
    "Simplified Chinese failed\n" true ' type ?warning
[THEN]
[TRY]
    \bold fonts= NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \italic fonts= NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold-italic fonts= NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
\serif
    \regular fonts= NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold fonts= NotoSerifSC-Bold|NotoSerifCJK-Bold|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \italic fonts= NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold-italic fonts= NotoSerifSC-Bold|NotoSerifCJK-Bold|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
\mono
    \regular fonts= NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold fonts= NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \italic fonts= NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold-italic fonts= NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular|DroidSansFallback
[THEN]
[THEN]

[IFUNDEF] android
[TRY]
\simplified-chinese
2 font-lang >breakable
\sans
    \regular fonts= gkai00mp|NotoSansSC-Regular|NotoSansCJK-Regular|NotoSansCJKsc-VF.otf
    fonts[ssm]=same
    {{  $A000  $2E80  $31390 $20000   $FB00  $F900   $FFF0  $FF00 }} 2/ +ranges
[ELSE]
    "Simplified Chinese failed\n"  true ' type ?warning
[THEN]
[TRY]
    \bold fonts= gkai00mp|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular
    \italic fonts= gkai00mp|NotoSansSC-Regular|NotoSansCJK-Regular
    \bold-italic fonts= gkai00mp|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular
\serif
    \regular fonts= gkai00mp|NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular
    \bold fonts= gkai00mp|NotoSerifSC-Bold|NotoSerifCJK-Bold|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular
    \italic fonts= gkai00mp|NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular
    \bold-italic fonts= gkai00mp|NotoSerifSC-Bold|NotoSerifCJK-Bold|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSerifSC-Regular|NotoSerifCJK-Regular|NotoSansSC-Regular|NotoSansCJK-Regular
\mono
    \regular fonts= gkai00mp|NotoSansSC-Regular|NotoSansCJK-Regular
    \bold fonts= gkai00mp|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular
    \italic fonts= gkai00mp|NotoSansSC-Regular|NotoSansCJK-Regular
    \bold-italic fonts= gkai00mp|NotoSansSC-Bold|NotoSansCJK-Bold|NotoSansSC-Regular|NotoSansCJK-Regular
[THEN]
[THEN]

[IFDEF] android
[TRY]
\traditional-chinese
2 font-lang >breakable
\sans
    \regular fonts= NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold fonts= NotoSansTC-Bold|NotoSansCJK-Bold|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \italic fonts= NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold-italic fonts= NotoSansTC-Bold|NotoSansCJK-Bold|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
\serif
    \regular fonts= NotoSerifTC-Regular|NotoSerifCJK-Regular|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold fonts= NotoSerifTC-Bold|NotoSerifCJK-Bold|NotoSansTC-Bold|NotoSansCJK-Bold|NotoSerifTC-Regular|NotoSerifCJK-Regular|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \italic fonts= NotoSerifTC-Regular|NotoSerifCJK-Regular|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold-italic fonts= NotoSerifTC-Bold|NotoSerifCJK-Bold|NotoSansTC-Bold|NotoSansCJK-Bold|NotoSerifTC-Regular|NotoSerifCJK-Regular|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
\mono
    \regular fonts= NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \italic font=same
    \bold fonts= NotoSansTC-Bold|NotoSansCJK-Bold|NotoSansTC-Regular|NotoSansCJK-Regular|DroidSansFallback
    \bold-italic font=same
{{  $3130  $3100 }} 2/ +ranges \ bopomofo
:noname ( traditional simple -- )
    2dup = IF  2drop  EXIT  THEN
    drop dup 1+ swap 1 +ranges ; is >sc
' 2drop is >tc
' drop is >tc2
read-unihan
[ELSE]
    "Traditional Chinese failed\n"  true ' type ?warning
[THEN]
[THEN]

[IFUNDEF] android
[TRY]
\traditional-chinese
2 font-lang >breakable
\sans
    \regular fonts= bkai00mp|NotoSansTC-Regular|NotoSansCJK-Regular|NotoSansCJKtc-VF.otf
    \bold fonts= bkai00mp|NotoSansTC-Bold|NotoSansCJK-Bold|NotoSansTC-Regular|NotoSansCJK-Regular
    \italic fonts= bkai00mp|NotoSansTC-Regular|NotoSansCJK-Regular
    \bold-italic fonts= bkai00mp|NotoSansTC-Bold|NotoSansCJK-Bold|NotoSansTC-Regular|NotoSansCJK-Regular
\serif
    \regular fonts= bkai00mp|NotoSerifTC-Regular|NotoSerifCJK-Regular|NotoSansTC-Regular|NotoSansCJK-Regular
    \italic font=same
    \bold fonts= bkai00mp|NotoSerifTC-Bold|NotoSerifCJK-Bold|NotoSansTC-Bold|NotoSansCJK-Bold|NotoSerifTC-Regular|NotoSerifCJK-Regular|NotoSansTC-Regular|NotoSansCJK-Regular
    \bold-italic font=same
\mono
    \regular fonts= bkai00mp|NotoSansTC-Regular|NotoSansCJK-Regular
    \italic font=same
    \bold fonts= bkai00mp|NotoSansTC-Bold|NotoSansCJK-Bold|NotoSansTC-Regular|NotoSansCJK-Regular
    \bold-italic font=same
{{  $3130  $3100 }} 2/ +ranges \ bopomofo
:noname ( traditional simple -- )
    2dup = IF  2drop  EXIT  THEN
    drop dup 1+ swap 1 +ranges ; is >sc
' 2drop is >tc
' drop is >tc2
read-unihan
[ELSE]
    "Traditional Chinese failed\n"  true ' type ?warning
[THEN]
[THEN]

[TRY]
\japanese
2 font-lang >breakable
\sans
\regular fonts= gkai00mp|NotoSansJP-Regular|NotoSansCJK-Regular|NotoSansCJKjp-VF.otf|DroidSansFallback
\italic font=same
\bold fonts= gkai00mp|NotoSansJP-Bold|NotoSansCJK-Bold|NotoSansJP-Regular|NotoSansCJK-Regular|DroidSansFallback
\bold-italic font=same
\serif
\regular fonts= gkai00mp|NotoSerifCJKjp-Regular|NotoSerifCJK-Regular|DroidSansFallback
\italic font=same
\bold fonts= gkai00mp|NotoSerifCJKjp-Bold|NotoSerifCJK-Bold|NotoSerifJP-Regular|NotoSerifCJK-Regular|DroidSansFallback
\bold-italic font=same
\mono
{{ $3100 $3000  $3200 $31F0  $3244 $3220  $3380 $3280  $FFA0 $FF5F }} 2/ +ranges
[ELSE]
    "Japanese failed\n"  true ' type ?warning
[THEN]

[TRY]
\hangul
1 font-lang >breakable \ not breakable for ragged layout
\sans
\regular fonts= NotoSansKR-Regular|NotoSansCJKkr-VF
fonts[ssm]=same
{{ $1200 $1100  $3190 $3130  $A980 $A960  $D7A4 $AC00  $D800 $D7B0 }} 2/ +ranges
[ELSE]
    "Hangul failed\n"  true ' type ?warning
[THEN]
[TRY]
\bold fonts= NotoSansKR-Bold
\italic fonts= NotoSansKR-Regular
\bold-italic fonts= NotoSansKR-Bold
\serif
\regular fonts= NotoSerifCJKkr-Regular
\bold fonts= NotoSerifCJKkr-Bold
\italic fonts= NotoSerifCJKkr-Regular
\bold-italic fonts= NotoSerifCJKkr-Bold
\mono
\regular fonts= NotoSansMonoCJKkr-Regular
\bold fonts= NotoSansMonoCJKkr-Bold
\italic fonts= NotoSansMonoCJKkr-Regular
\bold-italic fonts= NotoSansMonoCJKkr-Bold
[THEN]

\ emojis and icons don't differ between different shapes and styles

110% to font-scaler

[TRY]
\emoji \regular
2 font-lang >breakable
\sans \regular
\ color-fonts= NotoColorEmoji-SVG|EmojiOneColor-SVGinOT|TwitterColorEmoji-SVGinOT
color-fonts= NotoColorEmojiLegacy|NotoColorEmoji-PNG|NotoColorEmoji-Regular|NotoColorEmoji|emojione-android|Twemoji|SamsungColorEmoji|OpenMoji-Color
fonts[ssm]=same
{{ $20000 $1F000 }} 2/ +ranges
font-lang to emoji-font#
[ELSE]
    "EmojiColor Font failed\n"  true ' type ?warning
[THEN]

100% to font-scaler
[TRY]
\icons \regular
2 font-lang >breakable
\sans \regular
fonts= fa-merged-900
fonts[ssm]=same
{{ $F900 $F000 }} 2/ +ranges
[ELSE]
[TRY]
    \icons \regular
    2 font-lang >breakable
    \sans \regular
    fonts= fa-solid-900|FontAwesome5Free-Solid-900
    fonts[ssm]=same
    {{ $F900 $F000 }} 2/ +ranges \ catch all
    \iconbrands \regular
    2 font-lang >breakable
    \sans \regular
    fonts= fa-brands-400|FontAwesome5Brands-Regular-400
    fonts[ssm]=same
    \ (echo -n " "; fc-query --format='%{charset}\n' fa-brands-400.ttf) | sed -e 's/\([0-9a-f][0-9a-f][0-9a-f][0-9a-f]\)-\([0-9a-f][0-9a-f][0-9a-f][0-9a-f]\)/ $\2 1+ $\1 /g' -e 's/ \([0-9a-f][0-9a-f][0-9a-f][0-9a-f]\)/ $\1 1 bounds /g'
    {{ $f082 1+ $f081  $f08c 1 bounds  $f092 1 bounds   $f09b 1+ $f099   $f0d5 1+ $f0d2  $f0e1 1 bounds  $f113 1 bounds  $f136 1 bounds   $f13c 1+ $f13b  $f15a 1 bounds   $f169 1+ $f167   $f16e 1+ $f16b   $f171 1+ $f170   $f174 1+ $f173   $f17e 1+ $f179   $f181 1+ $f180  $f184 1 bounds   $f18d 1+ $f189  $f194 1 bounds  $f198 1 bounds   $f19b 1+ $f19a  $f19e 1 bounds   $f1aa 1+ $f1a0   $f1b7 1+ $f1b4   $f1be 1+ $f1bc   $f1cc 1+ $f1ca   $f1d7 1+ $f1d0   $f1e9 1+ $f1e7   $f1ee 1+ $f1ed   $f1f5 1+ $f1f0   $f203 1+ $f202   $f209 1+ $f208   $f20e 1+ $f20d   $f216 1+ $f210   $f232 1+ $f231  $f237 1 bounds   $f23e 1+ $f23a   $f24c 1+ $f24b  $f25e 1 bounds   $f26b 1+ $f260   $f26e 1+ $f26d  $f270 1 bounds   $f27e 1+ $f27c   $f282 1+ $f280   $f28a 1+ $f284   $f294 1+ $f293   $f299 1+ $f296   $f2a6 1+ $f2a5   $f2ae 1+ $f2a9   $f2b4 1+ $f2b0  $f2b8 1 bounds   $f2c6 1+ $f2c4   $f2da 1+ $f2d5   $f2de 1+ $f2dd  $f2e0 1 bounds  $f35c 1 bounds   $f375 1+ $f368   $f37d 1+ $f378   $f380 1+ $f37f   $f385 1+ $f383  $f388 1 bounds   $f38f 1+ $f38b   $f397 1+ $f391   $f39a 1+ $f399   $f39f 1+ $f39d   $f3a4 1+ $f3a1   $f3b2 1+ $f3a6   $f3bd 1+ $f3b4  $f3c0 1 bounds   $f3c4 1+ $f3c3   $f3c8 1+ $f3c6   $f3cc 1+ $f3ca  $f3d0 1 bounds   $f3dc 1+ $f3d2  $f3df 1 bounds   $f3e4 1+ $f3e1   $f3ec 1+ $f3e6   $f3ef 1+ $f3ee  $f3f3 1 bounds   $f3f9 1+ $f3f5  $f3fe 1 bounds   $f405 1+ $f402   $f40d 1+ $f407   $f421 1+ $f411  $f423 1 bounds   $f431 1+ $f425  $f44d 1 bounds  $f452 1 bounds  $f457 1 bounds  $f459 1 bounds  $f4d5 1 bounds   $f4e5 1+ $f4e4   $f4f9 1+ $f4e7   $f514 1+ $f50a  $f592 1 bounds  $f59e 1 bounds  $f5a3 1 bounds  $f5a8 1 bounds  $f5b2 1 bounds  $f5b5 1 bounds  $f5be 1 bounds  $f5c6 1 bounds  $f5cc 1 bounds  $f5cf 1 bounds  $f5f1 1 bounds  $f5f7 1 bounds  $f5fa 1 bounds  $f60f 1 bounds  $f612 1 bounds  $f63f 1 bounds  $f642 1 bounds  $f69d 1 bounds  $f6af 1 bounds   $f6ca 1+ $f6c9  $f6cc 1 bounds  $f6dc 1 bounds  $f704 1 bounds   $f731 1+ $f730  $f75d 1 bounds  $f778 1 bounds   $f77b 1+ $f77a  $f785 1 bounds  $f789 1 bounds  $f78d 1 bounds   $f791 1+ $f790   $f799 1+ $f797   $f7b1 1+ $f7af  $f7b3 1 bounds   $f7bc 1+ $f7bb  $f7c6 1 bounds  $f7d3 1 bounds  $f7d6 1 bounds   $f7e1 1+ $f7df  $f7e3 1 bounds }} 2/ +ranges
2 font-lang >breakable
[ELSE]
    "Icons failed\n"  true ' type ?warning
[THEN]
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
    \ Android: NotoSansSymbols-Regular-Subsetted NotoSansSymbols-Regular-Subsetted2
    2 font-lang >breakable
    font-lang to symbol-font#
    \symbols2 {{ $2316 $1 bounds  $2318 $1 bounds  $231A $2 bounds  $2324 $5 bounds $232B $1 bounds  $237B $1 bounds  $237D $3 bounds  $2394 $1 bounds  $23CE $2 bounds  $23E9 $2 bounds  $23ED $3 bounds  $23F1 $E bounds  $2400 $60 bounds  $25A0 $60 bounds  $2600 $A bounds  $260E $5 bounds  $2614 $10 bounds  $2630 $8 bounds  $263C $1 bounds  $2669 $2654   $267F $12 bounds  $269E $4 bounds  $26AA $3 bounds  $26CE $26BD  $26E1 $26CF  $2700 $C0 bounds  $2800 $100 bounds  $2B00 $100 bounds  $4DC0 $40 bounds  $10140 $C0 bounds  $102E0 $20 bounds  $10E60 $20 bounds  $1D300 $80 bounds }} 2/ "Symbols2" fonts=template[r]
    2 font-lang >breakable
[THEN]

[TRY]
\math
\sans
\regular fonts= STIXGeneral-Regular
\bold fonts= STIXGeneral-Bold
\italic fonts= STIXGeneral-Italic
\bold-italic fonts= STIXGeneral-BoldItalic
\serif
\regular fonts= STIXGeneral-Regular
\bold fonts= STIXGeneral-Bold
\italic fonts= STIXGeneral-Italic
\bold-italic fonts= STIXGeneral-BoldItalic
\mono
\regular fonts= STIXGeneral-Regular
\bold fonts= STIXGeneral-Bold
\italic fonts= STIXGeneral-Italic
\bold-italic fonts= STIXGeneral-BoldItalic
{{ $2300 $2190  $25A0 $60 bounds  $27C0 $40 bounds $2900 $200 bounds  $2B4D $2B12  $2B55 $2B50  $1D400 $400 bounds }} 2/ +ranges
[THEN]

[TRY]
\hebrew
\sans
\regular fonts= DejaVuSans|LiberationSans-Regular|NotoSansHebrew-Regular|DroidSans
fonts[ssm]=same
{{  $600  $590  $20AB $20AA  $FB50 $FB00 }} 2/ +ranges
\italic fonts= DejaVuSans-Oblique|LiberationSans-Italic|NotoSansHebrew-Italic
\bold fonts= DejaVuSans-Bold|LiberationSans-Bold|NotoSansHebrew-Bold
\bold-italic fonts= DejaVuSans-BoldOblique|LiberationSans-BoldItalic|NotoSansHebrew-BoldItalic

\serif
\regular fonts= DejaVuSerif|LiberationSerif-Regular|NotoSerifHebrew-Regular
\bold fonts= DejaVuSerif-Bold|LiberationSerif-Bold|NotoSerifHebrew-Bold
\italic fonts= DejaVuSerif-Italic|LiberationSerif-Italic|NotoSerifHebrew-Italic
\bold-italic fonts= DejaVuSerif-BoldItalic|LiberationSerif-BoldItalic|NotoSerifHebrew-BoldItalic

\mono
\regular fonts= LiberationMono-Regular|NotoSansHebrew-Regular
\bold fonts= LiberationMono-Bold|NotoSansHebrew-Bold
\italic fonts= LiberationMono-Italic|NotoSansHebrew-Italic
\bold-italic fonts= LiberationMono-BoldItalic|NotoSansHebrew-BoldItalic
[THEN]

[TRY]
\arabic
\sans
\regular fonts= NotoSansArabic-Regular|NotoNaskhArabic-Regular|DejaVuSans|LiberationSans-Regular|DroidKufi-Regular|DroidSans
fonts[ssm]=same
{{  $700  $600   $780  $750   $900  $8A0  $FE00 $FB50  $FF00 $FE70  $1EF00 $1EE00 }} 2/ +ranges
\italic fonts= NotoNastaliqUrdu-Regular|NotoSansArabic-Regular|NotoNaskhArabic-Regular|DejaVuSans-Oblique|LiberationSans-Italic|DroidKufi-Regular
\bold fonts= NotoSansArabic-Bold|NotoNaskhArabic-Bold|DejaVuSans-Bold|LiberationSans-Bold|DroidKufi-Bold
\bold-italic fonts= NotoNastaliqUrdu-Regular|NotoSansArabic-Bold|NotoNaskhArabic-Bold|DejaVuSans-BoldOblique|LiberationSans-BoldItalic|DroidKufi-Bold

\serif
\regular fonts= NotoSansArabic-Regular|NotoNaskhArabic-Regular|DroidNaskh-Regular
\bold fonts= NotoSansArabic-Bold|NotoNaskhArabic-Regular|DroidNaskh-Regular
\italic fonts= NotoSansArabic-Italic|NotoNaskhArabic-Bold|DroidNaskh-Bold
\bold-italic fonts= NotoSansArabic-BoldItalic|NotoNaskhArabic-Bold|DroidNaskh-Bold

\mono
\regular fonts= DejaVuSansMono|DroidKufi-Regular|NotoSansArabic-Regular|NotoNaskhArabic-Regular
\bold fonts= DejaVuSansMono-Bold|DroidKufi-Regular|NotoSansArabic-Bold|NotoNaskhArabic-Regular
\italic fonts= DejaVuSansMono-Oblique|DroidKufi-Bold|NotoSansArabic-Italic|NotoNaskhArabic-Bold
\bold-italic fonts= DejaVuSansMono-BoldOblique|DroidKufi-Bold|NotoSansArabic-BoldItalic|NotoNaskhArabic-Bold
[THEN]

\ all fonts here are Noto

[TRY]
\syriac \sans
\regular fonts= NotoSansSyriacWestern-Regular|NotoSansSyriacEstrangela-Regular|NotoSansSyriacEastern-Regular
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
\tibetan {{ $1000 $F00 }} 2/ "Tibetan" fonts=template[rb]?serif
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
\sans \regular fonts= Aegyptus|AegyptusR_hint|NotoSansEgyptianHieroglyphs-Regular
{{ $13000 $440 bounds }} 2/ +ranges
fonts[ssm]=same
\sans \bold fonts= AegyptusBold|AegyptusB_hint
[THEN]
\anatolianhieroglyphs {{ $14400 $280 bounds }} 2/ "AnatolianHieroglyphs" fonts=template[r]
2 font-lang >breakable
100% to font-scaler
\adlam {{ $1E900 $60 bounds }} 2/ "Adlam" fonts=template[r]

\latin \sans \regular

previous r> set-current
