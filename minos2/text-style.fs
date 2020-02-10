\ MINOS2 text style

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

get-current also minos definitions

\ day mode colors
$000000FF text-color: blackish
$0000BFFF text-color: dark-blue
$00000000 text-color: transparent
$FFFFFFFF text-color: whitish
$000000FF $000000FF $40C0FFFF $FFFFFFFF text-emoji-fade-color: toggle-color
\ night mode colors
color-h 1 > [IF]
    1 new-theme
    dark-gui
    $FFFFFFFF re-text-color blackish
    $FFFFBBFF re-text-color dark-blue
    $000000FF re-text-color whitish
    $40C0FFFF $000000FF $000000FF $FFFFFFFF re-text-emoji-fade-color toggle-color
    $1F3F7F7F re-text-color selection-color
    light-gui
[THEN]

blackish

0e FValue x-border
: cbl ( -- )
    current-baseline% to x-baseline ;
: \skip ( -- )
    x-baseline 140% f* to x-baseline ;
: >bl ( o -- o' )
    >o x-baseline to baseline
    x-baseline gap% f* font@gap fmax to gap o o> cbl ;
: >bl' ( o -- o' )
    >bl >o x-baseline to baseline' o o> ;
: }}text ( addr u -- o )
    text new >o font@ text! x-color to text-color  x-border to border o o> ;
: }}text-us ( us addr u -- o )
    }}text >o to us-mask o o> ;
: }}i18n-text ( lsid -- o )
    text new >o
    font@ i18n-text! x-color to text-color  x-border to border o o> ;
Defer }}text' ' }}text IS }}text'
: }}smalltext ( addr u -- o )
    font-size >r \script }}text' r> to font-size ;
: }}emoji ( addr u -- o )
    text new >o font@ text! x-color to text-color  x-border to border o o> ;
: }}edit ( addr u -- o )
    edit new >o font@ edit! x-color to text-color  x-border to border o o> ;
: }}pw ( addr u -- o )
    pw-edit new >o font@ edit! x-color to text-color  x-border to border
    1 to pw-mode  o o> ;
: >bdr ( o -- o' )
    >o font-size# to border o o> ;
: /center ( o -- o' )
    >r {{ glue*l }}glue r> glue*l }}glue }}h box[] >bl ;
: /left ( o -- o' )
    >r {{ r> glue*l }}glue }}h box[] >bl ;
: \\ }}text' /left ;
: p\\ ( text -- )
    }}text' >r {{ r> glue*l }}glue }}p box[] >bl'
    dpy-w @ s>f font-size# 70% f* f- 1e text-shrink% f2/ f- f/ dup .par-split ;
: p2\\ ( text1 text2 -- )
    }}text' >r dark-blue }}text' blackish >r {{ r> }}z >r
    {{ r> r> over >r glue*l }}glue }}p box[] >bl'
    r> over >o to lhang o>
    dpy-w @ s>f font-size# 70% f* f- 1e text-shrink% f2/ f- f/ dup .par-split ;
: e\\ }}emoji >r }}text' >r {{ r> glue*l }}glue r> }}h box[] >bl ;
: /right ( o -- o' )
    >r {{ glue*l }}glue r> }}h box[] >bl ;
: /hflip ( o -- o )
    >o box-hflip# to box-flags o o> ;
: /vflip ( o -- o )
    >o box-vflip# to box-flags o o> ;
: /flip ( o -- o )
    >o box-flip# to box-flags o o> ;
: /hphantom ( o -- o )
    >o box-hphantom# to box-flags o o> ;
: /vphantom ( o -- o )
    >o box-vphantom# box-dphantom# or to box-flags o o> ;
: /phantom ( o -- o )
    >o box-phantom# to box-flags o o> ;
: /hfix ( o -- o )
    >o box-hfix# to box-flags o o> ;
: /vfix ( o -- o )
    >o box-vfix# to box-flags o o> ;
: /dfix ( o -- o )
    >o box-dfix# to box-flags o o> ;
: /flop ( o -- o )
    >o 0 to box-flags o o> ;
Variable image-tex[]
Variable image-file[]
: }}image-file ( xt addr u r -- o glue-o ) pixelsize# f*
    2 pick image-tex[] >stack
    file>fpath $make dup image-file[] >stack dup cell+ swap @
    2 pick execute
    load-texture glue new >o
    fdup fm* vglue-c df!  fm* hglue-c df!  o o> dup >r
    swap white# }}image r> ;
: reload-images ( -- )
    image-tex[] $[]# 0 DO
	I image-tex[] $[] perform
	I image-file[] $[]@ load-texture 2drop
    LOOP ;
[IFDEF] android also android [THEN]
:noname defers reload-textures
    level# @ 0>  rendering @ -2 <= and  IF  reload-images  THEN ;
is reload-textures
[IFDEF] android previous [THEN]
: }}image-tex ( xt glue -- o )
    swap white# }}image ;

\ buttons

: 20%bt ( o -- o ) >o current-font-size% 20% f* to bordert o o> ;
: 25%b ( o -- o ) >o current-font-size% 25% f* fdup to border to gap o o> ;
: 25%bv ( o -- o ) >o current-font-size% 25% f* fdup to border fnegate to borderv o o> ;
: 40%b ( o -- o ) >o current-font-size% 40% f* to border o o> ;

: /center*ll ( o -- o' )
    >r {{ glue*ll }}glue r> glue*ll }}glue }}h box[] >bl ;
: /left*ll ( o -- o' )
    >r {{ r> glue*ll }}glue }}h box[] >bl ;

: }}button { text f: color -- o }
    {{
	glue*l color font-size# 40% f* }}frame dup .button2
	text }}text' 25%b /center
    }}z box[] ;

: }}tab-button { text f: color -- o }
    {{
	glue*l color font-size# 40% f* }}frame dup .button2
	text }}text' 25%b /center
	s" f|g" }}text 25%b /center /vphantom
    }}z box[] ;

: }}button1 { text f: color -- o }
    {{
	glue*l color font-size# 40% f* }}frame dup .button1
	text }}text' 25%b /center
    }}z box[] ;

: }}tile1 { text f: color -- o }
    {{
	glue*l color 0e }}frame dup .button1
	>o font-size# 40% f* to borderv o o>
	text }}text' 25%b /center
    }}z box[] ;

: }}button*ll { text f: color -- o }
    {{
	glue*ll color font-size# 40% f* }}frame dup .button2
	text }}text' 25%b /center*ll
    }}z box[] ;

: }}button-lit { d: text f: color -- o }
    {{
	glue*l color font-size# 40% f* }}frame dup .button2
	text }}text 25%b /center
    }}z box[] ;

: +text-color ( 0/0.999e o:widget -- )
    text-color 1/2 f- floor f+ 1/2 f+ to text-color ;

: toggle-bit, ( -- o )
    x-color toggle-color l" ⚪" }}text' to x-color ;

: }}toggle-bit ( mask addr -- o )
    toggle-bit, -rot
    [{: x a :}d
	IF    a @ x or a !         0e
	ELSE  a @ x invert and a ! 0.999e 
	THEN  caller-w >o +text-color o> +sync ;]
    true toggle[] ;

: }}tab ( text color -- o ) }}tab-button
    >o 0 childs[] $[] @
    >o font-size# 40% f* fdup fnegate to borderv to bordert o>
    o o> ;

: rgba> ( rgba -- r g b a ) >r
    r@ #24 rshift $FF and
    r@ #16 rshift $FF and
    r@ #08 rshift $FF and
    r>            $FF and ;
: >rgba ( r g b a -- rgba ) >r >r >r
    #8 lshift r> or
    #8 lshift r> or
    #8 lshift r> or ;

: darken ( color -- color' )
    1e f+ ;

: lighten ( color -- color' )
    1e f- ;

: >lowered ( percent o -- )
    >o 0 childs[] $[] @
    >o raise fover to raise
    fover fover f<> IF
	fover f< IF  frame-color darken  to frame-color
	ELSE   frame-color lighten to frame-color  THEN
    ELSE  fdrop  THEN o>
    1 childs[] $[] @ >o 1 childs[] $[] @
    >o      to raise o>
    o> o> ;

: }}tab' ( text color -- o ) }}tab
    dup font-size# 15% f* fround >lowered ;

: tab[] ( o addr -- o )
    [:  [: o font-size# 15% f* fround >lowered ;] caller-w .parent-w .do-childs
	caller-w 0e >lowered
	[: o /vflip drop ;] data @ .parent-w .do-childs
	data @ /flop drop +lang +resize +sync
    ;] swap click[] ;

glue new Constant glue*wh

: update-glue
    glue*wh >o
    dpy-w @ s>f font-size# 140% f* f- 0g fdup hglue-c glue!
    dpy-h @ s>f font-size# 140% f* f- 0g fdup vglue-c glue!
    0glue dglue-c glue! o> ;

update-glue

:noname defers re-config update-glue ; is re-config

: tab-glue: ( glue "name" -- )
    Create , ['] value-to set-to
  DOES> @ swap >o to aidglue >glue0 o o> ;

0 tab-glue: bx-tab
glue new Constant glue*em
glue*em >o 1glue font-size# 0e 0e glue+ hglue-c glue! 0glue dglue-c glue! 1glue vglue-c glue! o>

: b0 ( addr1 u1 -- o )
    dark-blue }}text' >r
    {{ glue*em }}glue r> }}h box[] bx-tab
    blackish ;
: b\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    blackish }}text' >r
    b0 >r
    {{ r> r> glue*ll }}glue }}h box[] >bl ;
: bbe\\ ( addr1 u1 addr2 u2 addr3 u3 -- o ) \ blue black emoji newline
    }}emoji >r
    blackish }}text' >r
    b0 >r
    {{ r> r> r> glue*em }}glue }}h box[] >bl ;
: bi\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    blackish \italic }}text' >r
    \regular b0 >r
    {{ r> r> glue*em }}glue }}h box[] >bl ;
: _underline_ ( o -- o )
    >o 1 +to us-mask o o> ;
: __underline__ ( o -- o )
    >o 2 +to us-mask o o> ;
: ___underline___ ( o -- o )
    >o 3 +to us-mask o o> ;
: -strikethrough- ( o -- o )
    >o 4 +to us-mask o o> ;
: bm\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    dark-blue \mono }}text' _underline_ >r
    \sans b0 >r
    {{ r> r> glue*em }}glue }}h box[] >bl ;
: \LaTeX ( -- )
    "L" }}text font-size >r \script
    "A" }}text >o font-size# fdup -23% f* to raise -30% f* to kerning o o>
    r> to font-size
    "T" }}text >o font-size# -10% f* to kerning o o>
    "E" }}text >o font-size# -23% f* fdup fnegate to raise to kerning o o>
    "X" }}text >o font-size# -10% f* to kerning o o> ;
: new-htab ( -- )
    htab-glue new dup tab-glues >stack  ;
: nt ( -- ) new-htab to bx-tab ; \ new tab
: vt{{ nt {{ ;
: }}vt \ vertical box with tab
    }}v box[] ;

\ high level style

: /title ( addr u -- )
    \huge cbl \sans \latin \bold dark-blue }}text' /center blackish
    \normal \regular x-baseline 80% f* to x-baseline ;
: /subtitle ( addr u -- ) \small dark-blue }}text' /center blackish \normal ;
: /author ( addr u -- ) \regular \large \bold dark-blue }}text' /center blackish
    \normal \regular \skip ;
: /location ( addr u -- ) \normal \regular  dark-blue }}text' /center blackish \normal ;
: /subsection ( addr u -- ) \normal \bold dark-blue \\ blackish \normal \regular ;

\ link click

: link[] ( o addr u -- o )
    [: ." xdg-open " type ;] $tmp $make >r
    [: click( ." link[]" cr ) addr data $@ system ;] r> click[] ;

\ mode

: !i18n  ['] }}i18n-text IS }}text' ;
: !lit   ['] }}text IS }}text' ;

previous set-current
