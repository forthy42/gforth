\ MINOS2 text style

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

0e FValue x-baseline
$000000FF Value x-color
: blackish $000000FF to x-color ;
: dark-blue $0000bFFF to x-color ;
0e FValue x-border
: cbl ( -- )
    current-baseline% to x-baseline ;
: \skip ( -- )
    x-baseline 140% f* to x-baseline ;
: >bl ( o -- o' )
    >o x-baseline to baseline o o> cbl ;
: }}text ( addr u -- o )
    text new >o font@ text! x-color to text-color  x-border to border o o> ;
: }}smalltext ( addr u -- o )
    font-size >r \script }}text r> to font-size ;
: }}emoji ( addr u -- o )
    font-lang >r
    \emoji emoji new >o font@ text! $FFFFFFFF to text-color  x-border to border o o>
    r> to font-lang ;
: }}edit ( addr u -- o )
    edit new >o font@ edit! x-color to text-color  x-border to border o o> ;
: >bdr ( o -- o' )
    >o font-size# to border o o> ;
: /center ( o -- o' )
    >r {{ glue*1 }}glue r> glue*1 }}glue }}h box[] >bl ;
: /left ( o -- o' )
    >r {{ r> glue*1 }}glue }}h box[] >bl ;
: \\ }}text /left ;
: e\\ }}emoji >r }}text >r {{ r> glue*1 }}glue r> }}h box[] >bl ;
: /right ( o -- o' )
    >r {{ glue*1 }}glue r> }}h box[] >bl ;
: /flip ( o -- o )
    >o box-hflip# box-flags ! o o> ;
: /flop ( o -- o )
    >o 0 box-flags ! o o> ;
: }}image-file ( xt addr u r -- o glue-o ) pixelsize# f*
    2 pick execute
    load-texture glue new >o
    s>f fover f* vglue-c df!
    s>f       f* hglue-c df! o o> dup >r
    $ffffffff rot }}image r> ;
: }}image-tex ( xt glue -- o )
    $ffffffff rot }}image ;

glue new Constant glue-left
glue new Constant glue-right
glue new Constant glue*wh
glue new Constant glue*b0
glue new Constant glue*b1
glue new Constant glue*b2

: update-glue
    glue*wh >o 0g 0g dpy-w @ s>f font-size# 140% f* f- hglue-c glue!
    0glue dglue-c glue! 1glue vglue-c glue! o>
    glue*b0 >o dpy-w @ s>f .05e f* 0g 0g hglue-c glue! o>
    glue*b1 >o dpy-w @ s>f .12e f* 0g 0g hglue-c glue! o>
    glue*b2 >o dpy-w @ s>f .20e f* 0g 0g hglue-c glue! o> ;

update-glue

0 Value bx-tab
glue new Constant glue*em
glue*em >o 1glue font-size# 0e 0e glue+ hglue-c glue! 0glue dglue-c glue! 1glue vglue-c glue! o>

: b0 ( addr1 u1 -- o )
    dark-blue }}text >r
    {{ glue*em }}glue r> }}h box[]
    >o bx-tab to aidglue o o> ;
: b\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    2swap b0 >r
    blackish }}text >r
    {{ r> r> swap glue*em }}glue }}h box[] >bl ;
: bbe\\ ( addr1 u1 addr2 u2 addr3 u3 -- o ) \ blue black emoji newline
    2rot b0 >r
    2swap blackish }}text >r
    }}emoji >r
    {{ r> r> r> swap rot glue*em }}glue }}h box[] >bl ;
: bi\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    2swap b0 >r
    blackish \italic }}text >r
    {{ r> r> swap glue*em }}glue }}h box[] >bl \regular ;
: bm\\ ( addr1 u1 addr2 u2 -- o ) \ blue black newline
    2swap b0 >r
    blackish \mono }}text >r
    {{ r> r> swap glue*em }}glue }}h box[] >bl \sans ;
: \LaTeX ( -- )
    "L" }}text
    "A" }}smalltext >o font-size# fdup -23% f* to raise -30% f* to kerning o o>
    "T" }}text >o font-size# -10% f* to kerning o o>
    "E" }}text >o font-size# -23% f* fdup fnegate to raise to kerning o o>
    "X" }}text >o font-size# -10% f* to kerning o o> ;
: vt{{ htab-glue new to bx-tab {{ ;
: }}vt \ vertical box with tab
    }}v >o bx-tab +aidglue o o> box[] ;

\ high level style

: /title ( addr u -- )
    \huge cbl \sans \latin \bold dark-blue }}text /center blackish
    \normal \regular x-baseline 80% f* to x-baseline ;
: /subtitle ( addr u -- ) \small dark-blue }}text /center blackish \normal ;
: /author ( addr u -- ) \normal \large \bold dark-blue }}text /center blackish
    \normal \regular \skip ;
: /location ( addr u -- ) \normal  dark-blue }}text /center blackish \normal ;
: /subsection ( addr u -- ) \normal \bold dark-blue \\ blackish \normal \regular ;
