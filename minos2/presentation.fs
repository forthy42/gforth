\ Presentation on MINOS2 made in MINOS2

\ Copyright (C) 2017 Free Software Foundation, Inc.

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

require widgets.fs

also minos

also freetype-gl
dpy-h @ s>f 22.5e f/ fround FConstant fontsize#
fontsize# 2 3 fm*/ fround FConstant smallsize#
fontsize# f2* FConstant largesize#

[IFDEF] android
    "/system/fonts/DroidSans.ttf"
[ELSE]
    "/usr/share/fonts/truetype/LiberationSans-Regular.ttf"
    2dup file-status nip [IF]
	2drop "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf"
	2dup file-status nip [IF]
	    2drop "/usr/share/fonts/truetype/NotoSans-Regular.ttf"
	    2dup file-status nip [IF]
		2drop "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf"
	    [THEN]
	[THEN]
    [THEN]
[THEN]
2dup file-status throw drop 2Constant latin-font

[IFDEF] android
    "/system/fonts/DroidSansFallback.ttf"
    2dup file-status nip [IF]
	2drop "/system/fonts/NotoSansSC-Regular.otf" \ for Android 6
	2dup file-status nip [IF]
	    2drop "/system/fonts/NotoSansCJK-Regular.ttc" \ for Android 7
	[THEN]
    [THEN]
[ELSE]
    "/usr/share/fonts/truetype/gkai00mp.ttf"
    2dup file-status nip [IF]
	2drop "/usr/share/fonts/truetype/arphic-gkai00mp/gkai00mp.ttf"
	2dup file-status nip [IF]
	    "/usr/share/fonts/truetype/NotoSerifSC-Regular.otf"
	    2dup file-status nip [IF]
		2drop "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc"
	    [THEN]
	[THEN]
    [THEN]
[THEN]
2dup file-status throw drop 2constant chinese-font

atlas fontsize# latin-font open-font   Value font1
atlas smallsize# latin-font open-font  Value font1s
atlas largesize# latin-font open-font  Value font1l
atlas fontsize# chinese-font open-font Value font2
previous

$000000FF Value x-color
font1 Value x-font
largesize# FValue x-baseline
: small font1s to x-font ;
: medium font1 to x-font ;
: large font1l to x-font ;
: blackish $FF to x-color ;
: dark-blue $7FFF to x-color ;
0e FValue x-border
: }}text ( addr u -- o )
    text new >o x-font text! x-color to text-color  x-border to border o o> ;
: }}edit ( addr u -- o )
    edit new >o x-font edit! x-color to text-color  x-border to border o o> ;
: /center ( o -- o' )
    >r {{ glue*1 }}glue r> glue*1 }}glue }}h box[] >o
    x-baseline to baseline o o> ;
: /left ( o -- o' )
    >r {{ r> glue*1 }}glue }}h box[] >o
    x-baseline to baseline o o> ;
: \\ }}text /left ;
: /right ( o -- o' )
    >r {{ glue*1 }}glue r> }}h box[] >o
    x-baseline to baseline o o> ;
: /flip ( o -- o )
    >o box-hflip# box-flags ! o o> ;
: /flop ( o -- o )
    >o 0 box-flags ! o o> ;
: bb\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    dark-blue 2swap }}text >r blackish }}text >r
    {{ r> r> swap glue*1 }}glue }}h box[] >o
    x-baseline to baseline o o> ;

Variable slides[]
Variable slide#
: >slides ( o -- ) slides[] >stack ;

glue new Constant glue-left
glue new Constant glue-right
glue new Constant glue*wh

glue*wh >o 0e 0e dpy-w @ s>f 64e f- hglue-c glue! o>
glue*wh >o 0glue dglue-c glue! 1glue vglue-c glue! o>

: prev-anim ( n r0..1 -- )
    dup 0<= IF  drop fdrop  EXIT  THEN
    fdup 1e f>= IF  fdrop
	dup slides[] $[] @ /flip drop
	1- slide# !
	glue-left  >o 0glue hglue-c glue! o>
	glue-right >o 0glue hglue-c glue! o>  EXIT
    THEN
    sin-t
    1- slides[] $[] @ /flop drop
    1e fswap f-
    fdup fnegate dpy-w @ fm* glue-left  .hglue-c df!
    -1e f+       dpy-w @ fm* glue-right .hglue-c df! ;

: next-anim ( n r0..1 -- )
    dup slides[] $[]# 1- u>= IF  drop fdrop  EXIT  THEN
    fdup 1e f>= IF  fdrop
	dup slides[] $[] @ /flip drop
	1+ slide# !
	glue-left  >o 0glue hglue-c glue! o>
	glue-right >o 0glue hglue-c glue! o>  EXIT
    THEN
    sin-t
    1+ slides[] $[] @ /flop drop
    fdup fnegate dpy-w @ fm* glue-left  .hglue-c df!
    -1e f+       dpy-w @ fm* glue-right .hglue-c df! ;

: prev-slide ( -- )
    anims[] $@len IF  anim-end  THEN
    slide# @ ['] prev-anim .5e >animate ;
: next-slide ( -- )
    anims[] $@len IF  anim-end  THEN
    slide# @ ['] next-anim .5e >animate ;

box-actor class
end-class slide-actor
:noname
    over $8  and IF  prev-slide  2drop fdrop fdrop  EXIT  THEN
    over $10 and IF  next-slide  2drop fdrop fdrop  EXIT  THEN
    [ box-actor :: clicked ] ; slide-actor to clicked

: slide[] ( o -- o )
    >o slide-actor new to act o act >o to caller-w o> o o> ;

glue-left  >o 1glue vglue-c glue! 1glue dglue-c glue! o>
glue-right >o 1glue vglue-c glue! 1glue dglue-c glue! o>

{{
glue-left }}glue

\ page 1
{{
glue*wh $FFFFFFFF 32e }}frame dup .button1 simple[]
{{
dark-blue
glue*1 }}glue \ $FFFFFFFF 4e }}frame dup .button1
large "MINOΣ2 — A GUI for net2o" }}text /center
small "Widgets and Layout Engine" }}text /center
glue*2 }}glue \ $FFFFFFFF 4e }}frame dup .button1
medium "Bernd Paysan" }}text /center
"EuroForth 2017, Bad Vöslau" }}text /center
glue*1 }}glue \ $FFFFFFFF 4e }}frame dup .button1
}}v box[] >o o Value title-page o o>
}}z slide[] dup >slides

\ page 2
{{
glue*wh $FF7F7FFF 32e }}frame dup .button1 simple[]
{{
dark-blue
large "4 Years after Snowden" }}text /center
blackish
medium "What has changed?" \\
dark-blue "Politics " \\
fontsize# 1.33e f* to x-baseline
blackish
"    Fake News/Hate Speech as excuse for censorship #NetzDG" \\
"    Crypto Wars 4.0: Discuss about ban of cryptography" \\
"    Legalize it (dragnet surveillance)" \\
"    Kill the link (EuGH and LG Humbug)" \\
"    Privacy: nobody is forced to use the Interwebs (Jim Sensenbrenner)" \\
dark-blue "Competition" \\
blackish
"    faces Stasi–like Zersetzung (Tor project)" \\
dark-blue "Solutions" \\
blackish
"    net2o starts becoming useable" \\
glue*1 }}glue
}}v box[] >o o Value snowden-page fontsize# to border o o>
}}z slide[] /flip dup >slides

\ page 3
{{
glue*wh $BFFFBFFF 32e }}frame dup .button1 simple[]
{{
largesize# to x-baseline
large dark-blue "Outlook from 2013" }}text /center
medium blackish
"•  The next presentation should be rendered with MINOΣ2" \\
fontsize# 1.33e f* to x-baseline
"•  Texts, videos, and images should be get with net2o, shouldn’t be on the device" \\
"•  Typesetting engine with boxes and glues, line breaking and hyphenation missing" \\
"•  a lot less classes than MINOΣ — but more objects" \\
"•  add a zbox for vertical layering" \\
"•  integrated animations" \\
"•  combine the GLSL programs into one program?" \\
glue*1 }}glue
}}v box[] >o fontsize# to border o o>
}}z slide[] /flip dup >slides

\ page 4
{{
glue*wh $BFBFFFFF 32e }}frame dup .button1 simple[]
{{
largesize# to x-baseline
large dark-blue "MINOΣ2 vs. MINOΣ" }}text /center
medium blackish
"Rendering:" " OpenGL (ES) instead of Xlib, Vulkan backend planned" bb\\
fontsize# 1.33e f* to x-baseline
"Coordinates:" " Single float instead of Ineger, origin bottom left (Xlib: top left)" bb\\
"Typesetting:" " Boxes&Glues closer to LaTeX — including ascender&descender" bb\\
"    Glues can shrink, not just grow" \\
"Object System:" " Mini–OOF2 instead of BerndOOF" bb\\
"Class number:" " Fewer classes, more combinations" bb\\
glue*1 }}glue
}}v box[] >o fontsize# to border o o>
}}z slide[] /flip dup >slides

\ page 5
{{
glue*wh $FFBFFFFF 32e }}frame dup .button1 simple[]
{{
largesize# to x-baseline
large dark-blue "MINOΣ2 Widgets" }}text /center
medium blackish
"Design principle is a Lego–style combination of many extremely simple objects" \\
fontsize# 1.33e f* to x-baseline
"actor" " base class that reacts on all actions (clicks, touchs, keys)" bb\\
"widget" " base class for all visible objects" bb\\
"glue" " base class for flexible objects" bb\\
"tile" " colored rectangle" bb\\
"frame" " colored rectangle with borders" bb\\
"test" " text element" bb\\
{{ dark-blue "edit" }}text blackish " editable text element " }}text "(text with cursor)" }}edit dup Value edit-field glue*1 }}glue }}h edit-field edit[] >o x-baseline to baseline o o>
"icon" " image from an icon texture" bb\\
"image" " larger image" bb\\
glue*1 }}glue
}}v box[] >o fontsize# to border o o>
}}z slide[] /flip dup >slides

\ page 6
{{
glue*wh $BFFFFFFF 32e }}frame dup .button1 simple[]
{{
largesize# to x-baseline
large dark-blue "MINOΣ2 Boxes" }}text /center
medium blackish
"Just like LaTeX: Boxes arrange widgets/text" \\
fontsize# 1.66e f* to x-baseline
"hbox" " Horizontal box, common baseline" bb\\
fontsize# 1.33e f* to x-baseline
"vbox" " Vertical box, minimum distance a baselineskip (of the hboxes below)" bb\\
"zbox" " Overlapping several boxes" bb\\
"grid" " Free widget placements (TBD)" bb\\
fontsize# 1.66e f* to x-baseline
"There will be some more variants for tables and wrapped paragraphs" \\
glue*1 }}glue
}}v box[] >o fontsize# to border o o>
}}z slide[] /flip dup >slides

\ page 7
{{
glue*wh $FFFFBFFF 32e }}frame dup .button1 simple[]
{{
largesize# to x-baseline
large dark-blue "MINOΣ2 Displays" }}text /center
medium blackish
"Render into different kinds of displays" \\
fontsize# 1.66e f* to x-baseline
"texture" " Into a texture, which can be used as image, also used as viewport (TBD)" bb\\
fontsize# 1.33e f* to x-baseline
"display" " To the actual display" bb\\
glue*1 }}glue
}}v box[] >o fontsize# to border o o>
}}z slide[] /flip dup >slides

glue-right }}glue
}}h box[]
to top-widget

: !widgets ( -- ) top-widget .htop-resize ;

also [IFDEF] android android [THEN]

: widgets-demo ( -- )
    !widgets widgets-loop ;

previous

script? [IF] widgets-demo bye [THEN]