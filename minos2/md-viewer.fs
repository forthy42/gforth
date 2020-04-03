\ MINOS2 markdown viewer

\ Author: Bernd Paysan
\ Copyright (C) 2019 Free Software Foundation, Inc.

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

\ Inspiration: wf.fs, a markdown-like parser, which converts to HTML

require jpeg-exif.fs
require user-object.fs
require text-style.fs
require presentation-support.fs

[IFDEF] update-gsize# update-gsize# [THEN]

get-current also minos definitions

Defer .char

uval-o md-style

object uclass md-style
    cell uvar md-text$
    cell uvar preparse$
    cell uvar last-cchar
    cell uvar last-emph-flags
    cell uvar emph-flags \ emphasis flags
    cell uvar up-emph
    cell uvar count-emph
    cell uvar us-state
    cell uvar imgs#
    umethod .md-text
    umethod .h1
    umethod .h2
    umethod .h3
    umethod .#.
    umethod .##.
    umethod .item
    umethod .desc
    umethod .image
    umethod .link
    umethod .pagebreak
end-class md-styler

: reset-emph ( -- )
    last-emph-flags off
    last-cchar off
    emph-flags off
    up-emph off
    count-emph off
    us-state off ;

0 Value md-p-box \ paragraph box
0 Value md-box \ vertical box

[IFUNDEF] bits:
    : bit ( n "name" -- n*2 )   dup Constant 2* ;
    : bits: ( start n "name1" .. "namen" -- )
	0 ?DO bit LOOP drop ;
[THEN]

1 8 bits: italic underline 2underline sitalic bold mono strikethrough #dark-blue

: +emphs ( flags -- )
    \regular \sans
    dup [ underline 2underline or ]L and 2/  us-state !
    dup strikethrough and 4 rshift us-state +!
    dup mono and IF  \mono  THEN
    dup #dark-blue and IF  dark-blue  ELSE  blackish  THEN
    [ italic sitalic bold or or ]L and
    dup 1 and swap 3 rshift xor
    case
	1 of  \italic       endof
	2 of  \bold         endof
	3 of  \bold-italic  endof
    endcase ;

: md-text+ ( -- )
    md-text$ $@len IF  bl md-text$ c$+!  THEN ;
glue new Constant glue*\\
glue*\\ >o 0e 0g 1fill hglue-c glue! 0glue dglue-c glue! 1glue vglue-c glue! o>
: .\\ ( -- )
    glue*\\ }}glue md-p-box .child+ x-baseline md-p-box .parent-w >o to baseline' o> ;
: +p-box ( -- )
    {{ }}p box[] >bl dup md-box .child+
    dup >o "p-box" to name$ o>
    dup .subbox >o to parent-w "subbox" to name$ o o> box[] to md-p-box ;

: /source ( -- addr u )
    source >in @ safe/string ;

: +link ( o -- o )
    /source IF  c@ '(' =  IF  1 >in +! ')' parse link[]  THEN
    ELSE  drop  THEN ;

: jpeg? ( addr u -- flag )
    dup 4 - 0 max safe/string ".jpg" str= ;
: img-orient? ( addr u -- flag )
    2dup jpeg? IF
	>thumb-scan  img-orient @ 1- 0 max
    ELSE  2drop 0  THEN ;

\ album viewer

slide-deck Constant default-sd

slide-class uclass slide-deck
end-class album-slide-class

album-slide-class new Value album-sd
album-sd to slide-deck

glue new glue-left !
glue new glue-right !

: /mid ( o -- o' ) >r
    {{  glue*wh }}glue >o font-size# 70% f* to border o o>
	{{ glue*l }}glue r> /center glue*l }}glue }}v box[] >bl
    }}z box[] ;

0 Value album-viewer
0 Value md-frame

also [IFDEF] android android [THEN]
: album-close ( -- )
    album-viewer .parent-w .childs[] stack> drop
    md-frame .act >o 0 to active-w o>
    default-sd to slide-deck
    [IFDEF] aback  [ action-of aback ]L is aback  [THEN]
    >normalscreen +sync +resize ;
previous

$000000E0 new-color, FValue album-bg-col#

box-actor class
end-class album-actor

simple-actor class
end-class album-scroll-actor

:noname ( key -- )
    case
	ctrl P k-ctrl-mask or   of  prev-slide   endof
	ctrl N k-ctrl-mask or   of  next-slide   endof
	k-left    of  prev-slide   endof
	k-right   of  next-slide   endof
	k-volup   of  prev-slide   endof
	k-voldown of  next-slide   endof
	#esc      of  album-close  endof
    endcase ; album-actor is ekeyed
: album[] ( o -- o )
    >o album-actor new to act o act >o to caller-w o> o o> ;

:noname ( fx fy b n -- )
    over $180 and IF  4 to scroll<<  THEN
    over $08 scroll<< lshift and IF  prev-slide  2drop fdrop fdrop  EXIT  THEN
    over $10 scroll<< lshift and IF  next-slide  2drop fdrop fdrop  EXIT  THEN
    2drop fdrop fdrop ; album-scroll-actor is clicked
: album-scroll[] ( o -- o )
    >o album-scroll-actor new to act o act >o to caller-w o> o o> ;

{{
    glue*wh album-bg-col# slide-frame dup .button1
    {{
	glue-left @ }}glue
	tex: img0 ' img0 "thumb.png" 0.666e }}image-file drop >imgs
	/mid        dup >slides
	tex: img1 ' img1 "thumb.png" 0.666e }}image-file drop >imgs
	/mid /hflip dup >slides
	tex: img2 ' img2 "thumb.png" 0.666e }}image-file drop >imgs
	/mid /hflip dup >slides
	tex: img3 ' img3 "thumb.png" 0.666e }}image-file drop >imgs
	/mid /hflip dup >slides
	glue-right @ }}glue
    }}h dup to imgs-box
    {{  \huge \sans \regular blackish
	{{
	    glue*ll }}glue
	    "    " }}text
	    glue*ll }}glue
	}}v ' prev-slide 0 click[]
	glue*ll }}glue album-scroll[]
	{{
	    glue*ll }}glue
	    "    " }}text
	    glue*ll }}glue
	}}v ' next-slide 0 click[]
    }}h box[]
    {{ \Large
	{{ "❌" }}text }}h 25%b
	' album-close 0 click[] /right
	glue*ll }}glue
    }}v box[]
    \normal
}}z album[] to album-viewer

default-sd to slide-deck

: album-prepare ( n -- )
    >fullscreen  album-sd to slide-deck
    dup 3 and slide# !  -4 and album/# !  slide-flipflop ;

also [IFDEF] android android [THEN]
: >md-album-viewer ( n -- )
    album-prepare  ['] slurp-file is load-img  4 album-reload
    md-frame album-viewer >o to parent-w o>
    album-viewer md-frame .childs[] >stack
    [IFDEF] aback  [: #esc rdrop ;] is aback [THEN]
    album-viewer engage
    +sync +resize ;
previous

\ image/thumb loader

-1 Value imgs#max

: l+! ( n addr -- )  dup >r l@ + r> l! ;

: fix-thumb-wh { w h thumb -- w h thumb }
    1e w h fm*/ img-h @ img-w @ fm*/ fdup 1e f< IF
	h dup fm* f>s dup to h
	dup thumb $C + l!  - 2/ thumb 4 + l+!
    ELSE  1/f
	w dup fm* f>s dup to w
	dup thumb 8 + l!  - 2/ thumb l+!
    THEN
    w h thumb ;

: load/thumb { w^ fn$ -- w h res flag }
    imgs# @ imgs#max u>=
    fn$ $@ jpeg? IF  thumbnail@ nip 0<> and  THEN
    IF
	thumbnail@ load-thumb  drop  fix-thumb-wh  true
    ELSE
	tex-xt dup >r image-tex[] >stack r@ execute
	fn$ @ image-file[] >stack
	fn$ $@ slurp-file mem>texture r> false
    THEN
    fn$ @ album-imgs[] >stack
    1 imgs# +! ;

66 Value maxcols#

: wh>glue ( w h w% h% o:glue -- ) { f: w% f: h% }
    2dup dpy-h @ s>f fm/ h% f* dpy-w @ s>f fm/ w% f* fmin
    \ not bigger than x% of screen
    fdup fm* vglue-c df!  fm* hglue-c df! ;

: default-imgwh% ( -- w h )
    dpy-w @ s>f font-size# maxcols# fm* f/ 1/f 100% fmin 100% ;

: }}image-file' ( addr u hmax vmax -- o ) { | w^ fn$ }
    file>fpath fn$ $!
    fn$ $@ img-orient? { img-rot# }
    fn$ @ load/thumb 2swap
    img-rot# 4 and IF  swap  THEN
    imgs# @ imgs#max u>  IF  15% f* fswap 20% f* fswap  THEN
    glue new >o wh>glue o o>
    -rot IF  }}thumb  ELSE  white# }}image  THEN
    >o img-rot# to rotate# o o>
    [: data album-imgs[] $[]@ data >md-album-viewer ;] imgs# @ 1- click[]
    exif-close ;
: +image ( o -- o )
    /source IF  c@ '(' =  IF  1 >in +! ')' parse
	    2dup "file:" string-prefix? IF  5 /string
	    ELSE
		2dup "http:" string-prefix? >r
		2dup "https:" string-prefix? r> or IF  link[]  EXIT  THEN
	    THEN
	    default-imgwh% }}image-file'
	    >r {{ glue*l }}glue r> glue*l }}glue }}v box[]
	    swap .dispose-widget  THEN
    ELSE  drop  THEN ;

: >lhang ( o -- o )
    md-p-box .parent-w >o dup to lhang o> ;

\ style class implementation

md-styler new Constant default-md-styler
default-md-styler to md-style

:noname ( -- )
    md-text$ $@len IF
	us-state @ md-text$ $@ }}text-us md-p-box .child+ md-text$ $free
    THEN ; is .md-text

\ interpretation

: default-char ( char -- )
    emph-flags @ last-emph-flags @ over last-emph-flags ! <> IF
	.md-text emph-flags @ +emphs
    THEN
    md-text$ c$+!  last-cchar off ;

: wspace ( -- ) ' ' xemit ;
: wspaces ( n -- ) 0 ?DO wspace LOOP ;

' default-char is .char

Create do-char $100 0 [DO] ' .char , [LOOP]

: md-char ( xt "char" -- )
    source >in @ /string drop c@ cells do-char + !  1 >in +! ;
: md-char: ( "char" -- )
    depth >r :noname depth r> - 1- roll md-char ;

: ?count-emph ( flag char -- )
    last-cchar @ over last-cchar ! <> IF  count-emph off
	emph-flags @ and 0= up-emph !
    ELSE  1 count-emph +!  drop  THEN ;

: render-line ( addr u attr -- )
    \G render a line
    emph-flags @ >r dup emph-flags ! +emphs
    [: BEGIN  /source  WHILE  1 >in +!
		c@ dup cells do-char + perform
	REPEAT  drop ;] execute-parsing
    r> emph-flags ! ;

: ]-parse ( -- addr u )
    /source drop
    BEGIN  ']' parse  dup IF  2dup + 1- c@ '\' =  ELSE  false  THEN  WHILE
	    2drop  REPEAT  + over - ;

Vocabulary md-tokens

md-char: * ( char -- )
    [ sitalic bold or ]L swap ?count-emph
    sitalic up-emph @ 0= IF  negate  THEN  emph-flags +! ;
md-char: _ ( char -- )
    [ italic underline 2underline or or ]L swap ?count-emph
    italic up-emph @ 0= IF  negate  THEN  emph-flags +! ;
md-char: ` ( char -- )
    mono swap ?count-emph
    mono up-emph @ 0= IF  negate  THEN  emph-flags +! ;
md-char: ~ ( char -- )
    strikethrough swap ?count-emph
    /source "~" string-prefix? IF
	1 >in +!
	strikethrough up-emph @ 0= IF  negate  THEN  emph-flags +!
    ELSE  '~' .char  THEN ;
md-char: \ ( char -- )
    drop /source IF  c@ .char  1 >in +!  ELSE  drop  THEN ;
md-char: ! ( char -- )
    /source "[" string-prefix? IF
	drop 1 >in +! ]-parse .image
    ELSE  .char  THEN ;
:noname ( desc-addr u1 img-addr u2 -- )
    .md-text ( dark-blue )
    dup 0= IF  2drop " "  THEN
    1 -rot }}text-us +image md-p-box .child+ ( blackish ) ; is .image
md-char: [ ( char -- )
    drop ]-parse 2dup "![" search nip nip IF
	drop ')' parse 2drop ]-parse + over -  THEN
    .link ;
:noname ( link-addr u1 desc-addr u2 -- )
    .md-text
    dup 0= IF  2drop " "  THEN
    us-state @ >r md-p-box >r {{ }}h box[] to md-p-box
    [ underline #dark-blue or ]L render-line .md-text
    md-p-box r> to md-p-box r> us-state ! blackish
    +link md-p-box .child+ ; is .link
md-char: : ( char -- )
    drop /source ":" string-prefix? IF
	>in @ >r
	1 >in +! ':' parse /source ":" string-prefix? IF
	    ['] md-tokens >body find-name-in ?dup-IF
		name?int execute
		rdrop EXIT  THEN  THEN
	r> >in !
    THEN  ':' .char ;
md-char: 	 ( tab -- )
    drop dark-blue ['] wspace md-text$ $exec
    " " md-text$ 0 $ins md-text$ $@ .desc  md-text$ $free ;
:noname ( addr u -- ) 2>r
    {{
	{{ us-state @ 2r> }}text-us glue*l }}glue }}h box[]
    }}z box[] bx-tab >lhang
    md-p-box .child+ blackish ; is .desc

$10 cells buffer: indent#s
0 Value cur#indent

: indent# ( n -- ) cur#indent cells indent#s + @ ;

: >indent ( n -- )
    >in @ + source rot umin  0 -rot
    bounds U+DO  I c@ #tab = 4 and I c@ bl = 1 and or +  LOOP
    2/ dup to cur#indent
    cells >r indent#s [ $10 cells ]L r> /string
    over 1 swap +! [ 1 cells ]L /string erase ;

: bullet-char ( n -- xchar )
    "•‣‧‧‧‧‧‧‧‧‧‧‧"
    drop swap 0 ?DO xchar+ LOOP  xc@ ;
0 warnings !@

Vocabulary markdown

get-current also markdown definitions

\ headlines limited to h1..h3
: # ( -- )
    /source 2dup + 2 - 2 " #" str= -2 and + .h1 ;
:noname
    \huge cbl bold render-line .md-text .\\ \normal \regular ; is .h1
: ## ( -- )
    /source 2dup + 3 - 3 " ##" str= -3 and + .h2 ;
:noname
    \large cbl bold render-line .md-text .\\ \normal \regular ; is .h2
: ### ( -- )
    /source 2dup + 4 - 4 " ###" str= -4 and + .h3 ;
:noname
    \normal cbl bold render-line .md-text .\\ \normal \regular ; is .h3
: 1. ( -- )
    \ render counted line
    -3 >indent .#. ;
:noname ( -- ) dark-blue
    {{ 0 [: cur#indent 2* 2 + spaces indent# 0 .r ." . " ;]
	$tmp }}text-us
    }}z /hfix box[] >lhang md-p-box .child+ blackish
    /source 0 render-line .md-text .\\ ; is .#.
10 2 [DO] [I] 0 <# '.' hold #S #> nextname synonym 1. [LOOP]

: 10. ( -- )
    \ render counted line
    -4 >indent .##. ;
:noname ( -- ) dark-blue
    {{ 0 [: cur#indent 2* 1+ spaces indent# 0 .r ." . " ;]
    $tmp }}text-us }}z /hfix box[] >lhang md-p-box .child+ blackish
    /source 0 render-line .md-text .\\ ; is .##.
100 11 [DO] [I] 0 <# '.' hold #S #> nextname synonym 10. [LOOP]

: * ( -- )33
    -2 >indent cur#indent bullet-char .item ;
:noname { bchar -- } dark-blue
    {{ 0 bchar [: cur#indent 1+ wspaces xemit wspace ;] $tmp }}text-us
    }}z /hfix box[] >lhang md-p-box .child+
    blackish /source 0 render-line .md-text .\\ ; is .item
: +  ( -- ) -2 >indent '+' .item ;
: -  ( -- ) -2 >indent '–' .item ;
: ±  ( -- ) -2 >indent '±' .item ;
: > ( -- )  -2 >indent '|' .item ;
: ::album:: ( -- )
    imgs# @ 1+ to imgs#max ;
: --- ( -- )
    .pagebreak ;
synonym *** ---
synonym ___ ---

previous set-current

warnings !

md-styler uclass md-style
end-class md-presenter

md-presenter new Constant presenter-md-styler

: presentation-size
    44e update-size# ;

get-current also markdown definitions
: ::presentation::
    presentation-size
    presenter-md-styler to md-style ;
presenter-md-styler to md-style
previous set-current

84e update-size#

\ generic formatting

: p-format ( rw -- )
    [{: f: rw :}l rw par-split ;] md-box .do-childs ;

: ?md-token ( -- token )
    parse-name [ ' markdown >body ]L find-name-in ;
: ===/---? ( -- )
    source nip 0<> IF
	source '=' skip nip 0= IF  "# " preparse$ 0 $ins " #" preparse$ $+!
	    true  EXIT  THEN
	source '-' skip nip 0= IF  "## " preparse$ 0 $ins " ##" preparse$ $+!
	    true  EXIT  THEN
    THEN  false ;

: read-par ( -- )  0 >r
    BEGIN   r@ 1 = IF  ===/---?  IF  rdrop  refill drop  EXIT  THEN  THEN
	source dup  WHILE
	    preparse$ $@len IF  bl preparse$ c$+!  THEN
	    r@ IF  bl skip  THEN
	preparse$ $+! r> 1+ >r  source + 1- c@ '\' =  refill 0= or UNTIL
	rdrop  EXIT  THEN  rdrop 2drop ;
: read-pre ( -- )
    source 4 /string preparse$ $!  refill drop ;
: hang-source ( -- addr u hang )
    source dup >r bl skip r> over - >r
    dup >r #tab skip r> over - 2* 2* r> max ;

: hang-read ( -- )
    hang-source >r 2drop source preparse$ $+!
    BEGIN  refill  WHILE
	    hang-source r@ u>=  over 0> and  ?md-token 0=  and  WHILE
		bl preparse$ c$+!  preparse$ $+!
	REPEAT  2drop
    THEN  rdrop ;
: reset-hang ( -- )
    indent#s [ $10 cells ]L erase ;

: refill-empty ( -- flag )
    BEGIN  source nip 0=  WHILE  refill 0=  UNTIL  false  ELSE  true  THEN ;

: typeset ( -- )
    +p-box  preparse$ $@
    \ ." typesetting: '" 2dup type ''' emit cr
    [: ?md-token ?dup-IF   name?int execute
	ELSE  >in off  source 0 render-line .md-text .\\  THEN ;]
    execute-parsing  preparse$ $free ;

: pre-typeset ( -- )
    +p-box preparse$ $@ mono +emphs md-text$ $! .md-text .\\
    preparse$ $free ;

: markdown-loop ( -- )
    BEGIN  refill-empty  WHILE  reset-emph >in off
	    ?md-token  IF  hang-read typeset
	    ELSE  reset-hang
		source "    " string-prefix? IF
		    read-pre pre-typeset  ELSE
		    read-par typeset  THEN
	    THEN
    REPEAT ;

: markdown-parse ( addr u -- )
    default-md-styler to md-style
    -1 to imgs#max  imgs# off
    {{ }}v box[] to md-box  0 to md-p-box
    nt open-fpath-file throw
    ['] markdown-loop execute-parsing-named-file
    reset-emph \regular \sans \normal ;

previous set-current

\\\
Local Variables:
forth-local-words:
    (
     (("md-char:") definition-starter (font-lock-keyword-face . 1)
      "[ \t\n]" t name (font-lock-function-name-face . 3))
    )
forth-local-indent-words:
    (
     (("md-char:") (0 . 2) (0 . 2) non-immediate)
    )
End:
