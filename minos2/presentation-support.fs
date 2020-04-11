\ Presentation support

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

uval-o slide-deck

Variable album-imgs[]

object uclass slide-deck
    cell uvar slides[]
    cell uvar imgs[]
    cell uvar slide#
    cell uvar slide#max
    cell uvar album/# \ index into album-imgs[]
    cell uvar glue-left
    cell uvar glue-right
    umethod load-img
end-class slide-class

slide-class new to slide-deck

' slurp-file is load-img

glue new glue-left !
glue new glue-right !

: >slides ( o -- ) slides[] >stack  1 slide#max +! ;
: >imgs ( o -- o ) dup imgs[] >stack ;

0 Value imgs-box

: swap-images ( -- )
    imgs-box .childs[] dup >r get-stack 2>r 2swap 2r> r> set-stack
    imgs[] get-stack >r 2swap r> imgs[] set-stack
    slides[] get-stack >r 2swap r> slides[] set-stack ;

: wh>tile-glue ( w h -- )
    tile-glue >o
    pixelsize# fm* fdup vglue-c df! dpy-h @ fm/
    pixelsize# fm* fdup hglue-c df! dpy-w @ fm/ fmax 1/f 2e fmin fdup
    vglue-c df@ f* vglue-c df!
    hglue-c df@ f* hglue-c df!
    o> ;

:noname defers re-config
    imgs[] $@ bounds U+DO
	I @ >o image-w image-h wh>tile-glue o>
    cell +LOOP ; is re-config

: album-image ( addr u n -- )
    imgs[] $[] @ >o image-tex
    2dup "\xFF\xD8\xFF" string-prefix?
    IF  mem-exif  [: 2dup >thumb-scan ;] catch drop nothrow
	img-orient @ 1- 0 max dup to rotate#  exif>
    ELSE  0  0 to rotate#  THEN  >r
    mem>texture
    r> 4 and IF  swap  THEN  dup to image-h over to image-w
    wh>tile-glue o> ;

: album-reload ( n -- )
    >r { | i# } album-imgs[] $@ album/# @ cells safe/string r> cells umin
    dup cell/ slide#max !
    bounds U+DO
	I $@ load-img  i# album-image
	1 +to i#
    cell +LOOP ;

: !album/# ( -- )
    album-imgs[] $[]# album/# @ - 4 min slide#max ! ;
: load-prev ( -- )
    album/# @ 0> IF
	swap-images  2 slide# +!
	-2 album/# +!  2 album-reload  !album/#
    THEN ;
: load-next ( -- )
    album-imgs[] $[]# album/# @ 4 + u>= IF
	4 album/# +!  2 album-reload  -2 album/# +!
	swap-images  -2 slide# +!  !album/#
    THEN ;

: glue0 ( -- ) 0e fdup
    glue-left  @ .hglue-c df!
    glue-right @ .hglue-c df! ;
: trans-frame ( o -- )
    >o transp# to frame-color o> ;
: solid-frame ( o -- )
    >o white# to frame-color o> ;
: !slides ( nprev n -- )
\   44e update-size#
    update-glue
    over slide# !
    slides[] $[] @ /flip drop
    slides[] $[] @ /flop drop glue0 ;
: slide-flipflop ( -- )
    4 0 DO
	I slides[] $[] @
	I slide# @ = IF  /flop  ELSE  /flip  THEN  drop
    LOOP ;
: fade-img ( r0..1 img1 img2 -- ) >r >r
    [ whitish x-color 1e f+ ] Fliteral fover f-
    r> >o to frame-color parent-w .parent-w /flop drop o>
    [ whitish x-color ] Fliteral f+
    r> >o to frame-color parent-w .parent-w /flop drop o> ;
: anim!slides ( r0..1 n -- )
    slides[] $[] @ /flop drop
    fdup fnegate dpy-w @ fm* glue-left  @ .hglue-c df!
    -1e f+       dpy-w @ fm* glue-right @ .hglue-c df! ;

: prev-anim ( n r0..1 -- )
    dup 0<= IF  drop fdrop  EXIT  THEN
    fdup 1e f>= IF  fdrop
	dup 1- swap !slides +sync +resize  EXIT
    THEN
    1e fswap f-
    1- sin-t anim!slides +sync +resize ;

: next-anim ( n r0..1 -- )
    dup slide#max @ 1- u>= IF  drop fdrop  EXIT  THEN
    fdup 1e f>= IF  fdrop
	dup 1+ swap !slides +sync +resize  EXIT
    THEN
    1+ sin-t anim!slides +sync +resize ;

1e FValue slide-time%

: prev-slide ( -- )
    slide# @ 0<= IF  load-prev  THEN
    slide-time% anims[] $@len IF  anim-end .2e f*  THEN
    slide# @ ['] prev-anim >animate ;
: next-slide ( -- )
    slide# @ 1+ slide#max @ u>= IF  load-next  THEN
    slide-time% anims[] $@len IF  anim-end .2e f*  THEN
    slide# @ ['] next-anim >animate ;

: slide-frame ( glue color -- o )
    font-size# 70% f* }}frame ;
: vp-frame ( color -- o ) \ drop $FFFFFFFF
    color, glue*wh slide-frame dup .button3 simple[] ;
: -25%b >o current-font-size% -25% f* to border o o> ;

box-actor class
end-class slide-actor

0 Value scroll<<

:noname ( axis dir -- ) nip
    0< IF  prev-slide  ELSE  next-slide  THEN ; slide-actor is scrolled
:noname ( rx ry b n -- )  dup 1 and 0= IF
	over $180 and IF  4 to scroll<<  THEN
	over $08 scroll<< lshift and IF  prev-slide  2drop fdrop fdrop  EXIT  THEN
	over $10 scroll<< lshift and IF  next-slide  2drop fdrop fdrop  EXIT  THEN
	over -$2 and 0= IF
	    fover caller-w >o x f- w f/ o>
	    fdup 0.1e f< IF  fdrop  2drop fdrop fdrop  prev-slide  EXIT
	    ELSE  0.9e f> IF  2drop fdrop fdrop  next-slide  EXIT  THEN  THEN
	THEN  THEN
    [ box-actor :: clicked ] +sync +resize ; slide-actor is clicked
:noname ( ekey -- )
    case
	k-up      of  prev-slide  endof
	k-down    of  next-slide  endof
	k-prior   of  prev-slide  endof
	k-next    of  next-slide  endof
	k-volup   of  prev-slide  endof
	k-voldown of  next-slide  endof
	s-k3 k-ctrl-mask or      of  1e ambient% sf!
	    Ambient 1 ambient% opengl:glUniform1fv  +sync endof
	s-k3      of  ambient% sf@ 0.1e f+ 1e fmin  ambient% sf!
	    Ambient 1 ambient% opengl:glUniform1fv  +sync endof
	k-f3      of  ambient% sf@ 0.1e f- 0e fmax  ambient% sf!
	    Ambient 1 ambient% opengl:glUniform1fv  +sync endof
	s-k4 k-ctrl-mask or     of  1e saturate% sf!
	    Saturate 1 saturate% opengl:glUniform1fv  +sync endof
	s-k4      of  saturate% sf@ 0.1e f+ 3e fmin saturate% sf!
	    Saturate 1 saturate% opengl:glUniform1fv  +sync endof
	k-f4      of  saturate% sf@ 0.1e f- 0e fmax saturate% sf!
	    Saturate 1 saturate% opengl:glUniform1fv  +sync endof
	k-f5 of  color-theme 0<> IF  anim-end 0.25e o
		[: 1e fswap f- fdup f>s to color-theme 1/2 f+ ColorMode! +sync +vpsync ;]
		>animate  THEN   endof
	k-f6 of  color-theme 0=  IF  anim-end 0.25e o
		[:             fdup f>s to color-theme 1/2 f+ ColorMode! +sync +vpsync ;]
		>animate  THEN   endof
	k-f1      of  top-widget ..widget  endof
	[ box-actor :: ekeyed ]  EXIT
    endcase +sync +resize ; slide-actor to ekeyed
:noname ( $xy b -- ) 2dup [ box-actor :: touchmove ] drop
    xy@ dpy-h @ s>f fswap f- dpy-h @ 2/ fm/ lightpos-xyz sfloat+ sf!
    dpy-w @ s>f f- dpy-w @ 2/ fm/ lightpos-xyz sf!
    3.0e lightpos-xyz 2 sfloats + sf!
    LightPos 1 lightpos-xyz opengl:glUniform3fv  +sync ; slide-actor is touchmove
: slide[] ( o -- o )
    >o slide-actor new to act o act >o to caller-w o> o o> ;

glue-left  @ >o 1glue vglue-c glue! 1glue dglue-c glue! o>
glue-right @ >o 1glue vglue-c glue! 1glue dglue-c glue! o>

: pres-frame ( colorday colornight -- o1 o2 )
    light-gui new-color, dark-gui -1 +to color,# new-color, fdrop light-gui
    glue*wh slide-frame dup .button1 simple[] ;

$10 stack: vp-tops

also opengl also also [IFDEF] android previous android also jni [THEN]

: >fullscreen ( -- )
    [IFDEF] set-fullscreen-hint
	set-fullscreen-hint 1 set-compose-hint
    [ELSE]
	[IFDEF] hidestatus hidekb hidestatus [THEN]
    [THEN] ;
: >normalscreen
    [IFDEF] reset-fullscreen-hint
	reset-fullscreen-hint 0 set-compose-hint
    [ELSE]
	[IFDEF] showstatus showstatus [THEN]
    [THEN] ;

: !pres-widgets ( -- )
    top-widget .htop-resize
    vp-tops get-stack 0 ?DO  .vp-top  LOOP
    1e ambient% sf! set-uniforms ;

: presentation ( -- )
    1config >fullscreen !pres-widgets widgets-loop
    >normalscreen ;
[IFDEF] looper-keyior
    : >presentation-key ( -- )  1 level# +!
	1config !pres-widgets enter-minos
	edit-terminal edit-out !
	top-widget .widget-draw
	['] looper-keyior is key-ior
	[: [: widgets-looper widget-sync ;] looper-do
	    looper-keys $@len 0> ;] is key? ;
[THEN]

previous previous previous
