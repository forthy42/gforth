\ OpenGL terminal

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

\ opengl common stuff

\ :noname source type cr stdout flush-file throw ; is before-line

require ../unix/pthread.fs
require gl-helper.fs

also opengl [IFDEF] android also android also jni  [THEN]

GL_FRAGMENT_SHADER shader: TerminalShader
#precision
uniform vec3 u_LightPos;        // The position of the light in eye space.
uniform sampler2D u_Texture0;   // The input texture (font)
uniform sampler2D u_Texture1;   // The character input
uniform sampler2D u_Texture2;   // the available colors
uniform sampler2D u_Texture3;   // the available colors
uniform float u_Ambient;        // ambient lighting level
uniform float u_Saturate;       // ambient lighting level
uniform vec4 u_Coloradd0;       // color bias for texture
uniform vec4 u_Coloradd1;       // color bias for texture
uniform vec4 u_Coloradd2;       // color bias for texture
uniform vec4 u_Coloradd3;       // color bias for texture
uniform vec2 u_texsize;         // the screen texture size
 
varying vec3 v_Position;        // Interpolated position for this fragment.
varying vec4 v_Color;           // This is the color from the vertex shader interpolated across the
                                // triangle per fragment.
varying vec3 v_Normal;          // Interpolated normal for this fragment.
varying vec2 v_TexCoordinate;   // Interpolated texture coordinate per fragment.
varying vec2 v_Extras;          // extra attributes passed through
 
// The entry point for our fragment shader.
void main()
{
    vec4 chartex = texture2D(u_Texture1, v_TexCoordinate);
    vec4 fgcolor = texture2D(u_Texture2, vec2(chartex.z, 0.));
    vec4 bgcolor = texture2D(u_Texture2, vec2(chartex.w, 0.));
    vec2 charxy = chartex.xy + vec2(0.0625, 0.125)*u_texsize*mod(v_TexCoordinate, 1.0/u_texsize);
    // mix background and foreground colors by character ROM alpha value
    // and multiply by diffuse
    vec4 pixel = texture2D(u_Texture0, charxy);
    vec4 col = bgcolor*(1.0-pixel.a) + fgcolor*pixel.a;
    if(u_Saturate != 1.0) {
        float mid = (col.r + col.g + col.b) * 0.333333333333;
        vec3 mid3 = vec3(mid, mid, mid);
        col.rgb = (u_Saturate * (col.rgb - mid3)) + mid3;
    }
    if(u_Ambient != 1.0) {
        // Will be used for attenuation.
        float distance = length(u_LightPos - v_Position);
 
        // Get a lighting direction vector from the light to the vertex.
        vec3 lightVector = normalize(u_LightPos - v_Position);
 
        // Calculate the dot product of the light vector and vertex normal. If the normal and light vector are
        // pointing in the same direction then it will get max illumination.
        float diffuse = max(dot(v_Normal, lightVector), 0.0);
        diffuse = diffuse * diffuse;
 
        // Add attenuation.
        diffuse = diffuse * (1.0 / (1.0 + (0.10 * distance * distance)));
 
        // Add ambient lighting
        diffuse = (diffuse * (1.0 - u_Ambient)) + u_Ambient;
 
        gl_FragColor = vec4(diffuse, diffuse, diffuse, 1.0)*col;
    } else {
        gl_FragColor = col;
    }
}

0 Value texsize
0 Value terminal-program

: create-terminal-program ( -- program )
    ['] VertexShader ['] TerminalShader create-program ;

: terminal-init { program -- } program init
    program "u_texsize"  glGetUniformLocation to texsize ;

tex: chars-tex
tex: video-tex
tex: color-tex

\ Variables and constants

: le-l, ( n -- )  lle here 4 allot l! ;

Create color-matrix \ vt100 colors
\ RGBA, but this is little endian, so write ABGR ,
$ff000000 le-l, \ Black
$ff3030ff le-l, \ Red
$ff20ff20 le-l, \ Green
$ff00ffff le-l, \ Yellow
$ffff6020 le-l, \ Blue - complete blue is too dark
$ffff00ff le-l, \ Magenta
$ffffff00 le-l, \ Cyan
$ffffffff le-l, \ White
$ff404040 le-l, \ dimm Black
$ff4040bf le-l, \ dimm Red
$ff40bf40 le-l, \ dimm Green
$ff40bfbf le-l, \ dimm Yellow
$ffbf4040 le-l, \ dimm Blue
$ffbf40bf le-l, \ dimm Magenta
$ffbfbf40 le-l, \ dimm Cyan
$ffbfbfbf le-l, \ dimm White

: term-load-textures ( addr u -- )
    chars-tex load-texture 2drop linear
    GL_TEXTURE2 glActiveTexture
    color-tex color-matrix $10 1 rgba-map nearest
    GL_TEXTURE0 glActiveTexture ;

Variable color-index
Variable error-color-index
$704000 dup color-index ! error-color-index !
Variable std-bg standard:field
1 pad ! pad c@ [IF] \ little endian
    2 cfield: fg-field
    cfield: bg-field drop
[ELSE]
    cell 3 - cfield: bg-field
    cfield: fg-field drop
[THEN]

$8F00 Value gl-default-color \ real default color

: ?default-fg ( n -- color ) dup 6 <= IF
	drop gl-default-color fg>  THEN  $F xor ;
: ?default-bg ( n -- color ) dup 6 <= IF
	drop gl-default-color bg>  THEN  $F xor ;
: fg! ( index -- )
    dup 0= IF  drop  EXIT  THEN  ?default-fg
    4 lshift color-index fg-field c! ;
: bg! ( index -- )
    dup 0= IF  drop  EXIT  THEN  ?default-bg
    4 lshift color-index bg-field c! ;
: err-fg! ( index -- ) ?default-fg
    4 lshift error-color-index fg-field c! ;
: err-bg! ( index -- ) ?default-bg
    4 lshift error-color-index bg-field c! ;
1e $130 fm/ FValue damp-light
: bg>clear ( index -- ) $F xor
    $F and sfloats color-matrix +
    count damp-light fm*
    count damp-light fm*
    count damp-light fm*
    c@    damp-light fm* glClearColor ;

: std-bg! ( index -- )  dup bg! dup std-bg ! bg>clear ;
Black White white? [IF] swap [THEN] fg! bg!

: >light light-mode White std-bg! White err-bg! Black fg! Red err-fg!
    White >bg Black >fg or to gl-default-color
    $70004000 dup color-index ! error-color-index ! ;
: >dark dark-mode Black std-bg! Black err-bg! White fg! Red err-fg!
    Black >bg White >fg or to gl-default-color
    $704000 dup color-index ! error-color-index ! ;
[IFDEF] android ' >dark window-init, [THEN]

256 Value videocols
0   Value videorows
0   Value actualrows

2Variable gl-xy  0 0 gl-xy 2!
2Variable gl-xy-save  0 0 gl-xy 2!
2Variable gl-wh 24 80 gl-wh 2!
Variable gl-lineend
Variable gl-lineend-save
Variable scroll-y
FVariable scroll-dest
FVariable scroll-source
FVariable scroll-time
[IFDEF] screen-xywh@     \ For Android
    2Variable screen-xy  \ top left position
    2Variable screen-wh' \ actually visible width and height
[THEN]

80 Value hcols
48 Value vcols

: form-chooser ( -- )
    screen-orientation 1 and  IF  hcols  ELSE  vcols  THEN  dup
    [ [defined] screen-wh' [IFDEF] SDK_INT SDK_INT #35 >= and [THEN] ]
    [IF] screen-wh' 2@ swap
	2dup d0= IF  2drop dpy-h @ dpy-w @  THEN
    [ELSE] dpy-h @ dpy-w @ [THEN]
    2* */ swap gl-wh 2! ;

: show-rows ( -- n ) videorows scroll-y @ - rows 1+ min ;
$40 Value minpow2#
: nextpow2 ( n -- n' )
    minpow2#  BEGIN  2dup u>  WHILE 2*  REPEAT  nip ;

[IFDEF] android SDK_INT #35 >= [ELSE] false [THEN]
[IF]
    : >rectangle ( -- )
	show-rows s>f rows fm/ -2e f* 1e f+
	screen-size@ screen-xywh@ { sw sh x y w h }
	y h + sh 2/ tuck - swap fm*/
	>v
	-1e fover >xy n> v+
	-1e 1e sh 2/ y - sh 2/ fm*/ >xy n> v+
	1e  1e sh 2/ y - sh 2/ fm*/ >xy n> v+
	1e  fswap >xy n> v+ o> ;
[ELSE]
    : >rectangle ( -- )
	show-rows s>f rows fm/ -2e f* 1e f+
	>v
	-1e fover >xy n> v+
	-1e 1e >xy n> v+
	1e  1e >xy n> v+
	1e  fswap >xy n> v+ o> ;
[THEN]

: >texcoords ( -- )
    cols s>f videocols fm/  show-rows dup s>f nextpow2 dup fm/
    { f: tx f: ty }
    scroll-y @ over + videorows umin over - scroll-y @ - s>f fm/ fnegate
    { f: ox }
    >v
    0e ty ox f+ >st v+
    0e    ox    >st v+
    tx    ox    >st v+
    tx ty ox f+ >st v+ v> ;

0 Value videomem

\ : blank-screen ( -- )
\     color-index @ videomem videocols videorows * sfloats bounds ?DO
\ 	dup I l!
\     1 sfloats +LOOP  drop ;
\ blank-screen

: gl-mem-erase ( addr u -- )
    color-index @ -rot bounds U+DO
	dup lle I l!
    [ 1 sfloats ]L +LOOP drop ;
: resize-screen ( -- )
    gl-wh @ { height }
    gl-xy @ 1+ actualrows max to actualrows
    height videocols u> gl-xy @ videorows u>= or IF
	videorows videocols * sfloats >r
	height nextpow2 videocols max to videocols
	gl-xy @ 1+ nextpow2 videorows max to videorows
	videomem videocols videorows * sfloats dup >r
	videorows sfloats + resize throw
	to videomem
	videomem r> r> /string gl-mem-erase
    THEN ;

2 sfloats buffer: texsize.xy

: draw-now ( -- )
    GL_TEXTURE1 glActiveTexture
    video-tex
    show-rows nextpow2 s>f  videocols s>f texsize.xy sf!+ sf!
    texsize 1 texsize.xy glUniform2fv
    show-rows nextpow2 >r
    videomem scroll-y @ r@ + videorows umin r@ -
    videocols * sfloats +
    videocols r> rgba-map wrap-texture nearest

    vi0 >rectangle >texcoords
    GL_TEXTURE0 glActiveTexture
    chars-tex
    0 i, 1 i, 2 i, 0 i, 2 i, 3 i,
    GL_TRIANGLES draw-elements ;

: screen-scroll ( r -- )  fdup floor fdup f>s scroll-y ! f-
    f2* rows fm/ >y-pos  +sync ;

: gl-char' ( -- addr )
    gl-xy 2@ videocols * + sfloats videomem + ;

: gl-form ( -- h w ) gl-wh 2@ ;

Variable gl-emit-buf

: gl-cr ( -- )
    gl-lineend @ 0= IF
	gl-xy @ 1+ 0 swap gl-xy 2! THEN
    resize-screen  +sync +show  out off ;

xc-vector @ set-encoding-utf-8
: xchar>glascii ( xchar -- 0..7F )
    case
	'▄' of $0 endof
	'•' of 1 endof
	'°' of 2 endof
	'ß' of 3 endof
	'Ä' of 4 endof
	'Ö' of 5 endof
	'Ü' of 6 endof
	'ä' of 7 endof
	'ö' of 8 endof
	'ü' of 9 endof
	'µ' of 10 endof
	'✔' of 11 endof
	'✘' of 12 endof
	'▀' of $10 endof
	'©' of $11 endof
	dup wcwidth -1 = IF  drop $7F
	ELSE  dup wcwidth 2 = IF  drop  13  ELSE  $7F umin  THEN
	THEN
    0 endcase ;
xc-vector !

: (gl-atxy) ( x y -- )
    >r gl-wh @ 1- min 0 max r> gl-xy 2!
    gl-xy cell+ @ out !  gl-lineend off ;

: gl-at-deltaxy ( x y -- )
    gl-xy 2@ rot + 0 max >r + 0 max r>
    \ >r s>d screenw @ sm/rem r> +
    (gl-atxy) ;

: gl-atxy ( x y -- )
    gl-wh cell+ @ 1- min 0 max
    scroll-y @ gl-xy @ gl-wh cell+ @ 2 - - 0 max max
    + (gl-atxy) ;

: (gl-emit) ( char color -- )
    over 7 = IF  2drop  EXIT  THEN
    over #bs = IF  2drop -1 0 gl-at-deltaxy  EXIT  THEN
    over #lf = IF  2drop gl-cr  EXIT  THEN
    over #cr = IF  2drop gl-cr  EXIT  THEN
    over #tab = IF  >r drop bl gl-xy cell+ @ dup 1+ dfaligned swap - 0
    ELSE
	>r
	dup max-single-byte u< IF \ fast path for ASCII
	    xchar>glascii 1
	ELSE \ slow path for composed xchars
	    gl-emit-buf c$+! "\x80\x80\x80" gl-emit-buf $+! \ pad up
	    gl-emit-buf $@ xc@+? nip nip invalid-char = IF \ really invalid
		$7F 1
	    ELSE
		gl-emit-buf $@len 3 - gl-emit-buf $!len \ unpad
		gl-emit-buf $@ xc@+? dup invalid-char = \ incomplete
		IF  drop 2drop rdrop  EXIT  THEN
		nip nip xchar>glascii
		gl-emit-buf $@ x-width abs
	    THEN
	    gl-emit-buf $free
	THEN  $10
    THEN  { n m }

    n out +!
    resize-screen  +sync
    dup $70 and 5 lshift or $F0F and 4 lshift r> $FFFF0000 and or
    n 0 ?DO
	dup lle gl-char' l!
	gl-xy 2@ >r 1+ dup cols u>= dup gl-lineend !
	IF  drop 0 r> 1+ gl-xy 2! resize-screen
	ELSE  r> gl-xy 2!  THEN  m +
    LOOP  drop ;

${GFORTH_IGNLIB} s" true" str= 0= [IF]
    Sema gl-sema
[ELSE]
    Create gl-sema  1 pthread-mutexes allot
[THEN]

: gl-emit ( char -- ) [: color-index @ (gl-emit) ;] gl-sema c-section ;
: gl-emit-err ( char -- )
    dup (err-emit) \ errors also go to the error log
    [: error-color-index @ (gl-emit) ;] gl-sema c-section ;
: gl-cr-err ( -- )
    #lf (err-emit)  gl-cr ;

: gl-type ( addr u -- )
    [: bounds ?DO  I c@ color-index @ (gl-emit)  LOOP ;] gl-sema c-section ;

: gl-type-err ( addr u -- )  2dup (err-type)
    [: bounds ?DO  I c@ error-color-index @ (gl-emit)  LOOP ;] gl-sema c-section ;

: gl-page ( -- ) [: 0 0 (gl-atxy)  0 to videorows  0 to actualrows
    0e screen-scroll  0e fdup scroll-source f! scroll-dest f!
    resize-screen +sync ;] gl-sema c-section ;

: ?invers ( attr -- attr' ) dup invers and IF
    dup $F000 and 4 rshift over $F00 and 4 lshift or swap $FF and or  THEN ;
: >default ( attr -- attr' )
    dup  bg> 6 <= $F and >bg
    over fg> 6 <= $F and >fg or
    gl-default-color -rot mux ;
: gl-attr! ( attribute -- )
    [: dup attr ! >default ?invers  dup bg> bg! fg> fg! ;]
    gl-sema c-section ;
: gl-err-attr! ( attribute -- )
    [: dup attr ! >default ?invers  dup bg> err-bg! fg> err-fg! ;]
    gl-sema c-section ;
: gl-control-sequence ( u char -- )
    [: case
	    0 of
		case
		    7 of  gl-xy 2@  gl-xy-save 2!
			gl-lineend @ gl-lineend-save !  endof \ save curpos
		    8 of  gl-xy-save 2@  gl-xy 2!
			gl-lineend-save @ gl-lineend !  endof \ restore curpos
		endcase
	    endof
	    'A' of  negate 0 swap gl-at-deltaxy  endof
	    'B' of         0 swap gl-at-deltaxy  endof
	    'E' of         gl-xy @ + 0 swap (gl-atxy)  endof
	    'F' of  negate gl-xy @ + 0 swap (gl-atxy)  endof
	    'J' of  >r
		videomem videocols videorows * sfloats
		gl-xy 2@ videocols * + sfloats
		case r>
		    0 of  safe/string  endof \ cursor + below
		    1 of  nip          endof \ above cursor
		    2 of  drop         endof \ erase all
		    nip
		endcase
		gl-mem-erase
	    endof
	    nip
	endcase ;] gl-sema c-section ;

4e FConstant scroll-deltat
: >scroll-pos ( -- 0..1 )
    ftime scroll-time f@ f- scroll-deltat f*
    1e fmin 0e fmax 0.5e f- pi f* fsin 1e f+ f2/ ;

: set-scroll ( r -- )
    scroll-y @ s>f y-pos sf@ f2/ rows fm* f+ scroll-source f!
    scroll-dest f!  ftime scroll-time f! ;

: scroll-slide ( -- )  -sync scroll-dest f@ scroll-source f@ f= ?EXIT
    >scroll-pos fdup 1e f= IF  scroll-dest f@ scroll-source f!  THEN
    fdup scroll-dest f@ f* 1e frot f- scroll-source f@ f* f+ screen-scroll ;

: col>< ( addr -- )
    dup w@ w>< swap w! ;

: screen->gl ( -- )
    videomem 0= IF  resize-screen  THEN
    std-bg @ bg>clear clear
    terminal-program glUseProgram
    unit-matrix MVPMatrix set-matrix
    gl-char' 2 + { cursor-addr }
    cursor-addr col><
    draw-now
    cursor-addr col><
    sync ;

: show-cursor ( -- )  ?show 0= ?EXIT
    rows ( kbflag @ IF  dup 10 / - 14 -  THEN ) >r
    gl-xy @ scroll-y @ dup r@ status-offset - + within 0= IF
       gl-xy @ 1+ r@ status-offset - - 0 max s>f set-scroll
    THEN  rdrop  -show ;

[IFUNDEF] win : win app window @ ; [THEN]

[IFDEF] android
    JValue metrics \ screen metrics
    
    : >metrics ( -- )
	newDisplayMetrics dup to metrics
	clazz .getWindowManager .getDefaultDisplay .getMetrics ;
    
    : screen-wh ( -- rw rh ) \ minos2
	\G get screen @var{rw rh} in mm
	metrics ?dup-0=-IF  >metrics metrics  THEN >o
	widthPixels  xdpi 1/f fm* 25.4e f*      \ width in mm
	heightPixels ydpi 1/f fm* 25.4e f* o> ; \ height in mm
    : screen-pwh ( -- w h ) \ minos2
	\G get screen @var{w h} in pixel
	metrics ?dup-0=-IF  >metrics metrics  THEN >o
	widthPixels heightPixels o> ;
[ELSE]
    [IFDEF] x11
	also x11 also xrandr
	: screen-pwh ( -- w h ) \ minos2
	    \G get screen @var{w h} in pixel
	    rr-crt0 XRRCrtcInfo-width l@
	    rr-crt0 XRRCrtcInfo-height l@ ;
	: screen-wh ( -- rw rh ) \ minos2
	    \G get screen @var{rw rh} in mm
	    screen-pwh
	    dpy-h @ rr-out0 XRROutputInfo-mm_height l@ s>f fm*/
	    dpy-w @ rr-out0 XRROutputInfo-mm_width  l@ s>f fm*/ fswap ;
	previous previous
    [ELSE]
	: screen-pwh ( -- w h )
	    dpy-wh 2@ ;
	: screen-wh ( -- rw rh )
	    wl-metrics 2@ swap s>f s>f ;
    [THEN]
[THEN]

5.5555e 25.4e f* FValue default-diag \ 5.5555" inches as default
1e FValue default-scale

: screen-diag ( -- rdiag )
    screen-wh f**2 fswap f**2 f+ fsqrt ;   \ diagonal in mm

: terminal-scale-me ( -- )
    \ smart scaler, scales using square root relation
    level# @ 0= IF
	default-diag screen-diag f/ fsqrt default-scale f*
	1/f #80 fdup fm* f>s to hcols #48 fm* f>s to vcols
	resize-screen config-changed screen->gl  THEN ;

Defer scale-me ' terminal-scale-me is scale-me

: config-changer ( -- )
[IFDEF] screen-xywh@
    screen-xywh@ screen-wh' 2! screen-xy 2!
[THEN]
    getwh  >screen-orientation  scale-me
    form-chooser ;
: ?config-changer ( -- )
    ?config IF
	gl-wh 2@ 2>r config-changer gl-wh 2@ 2r> d<>
	IF   winch? on +resize  THEN  +sync -config
    THEN ;

Variable render#

: screen-sync ( -- )
    rendering @ -2 > ?EXIT \ don't render if paused
    ?config-changer
    win level# @ 0<= and IF
	?sync IF  -sync show-cursor screen->gl  THEN
    THEN ;

: gl-fscale ( f -- ) to default-scale
    1+config screen-ops ;
: gl-scale ( n -- ) s>f gl-fscale ;

: >changed-to ( ms -- ) #1000000 um* ntime d+ { d: t-o }
    +config
    BEGIN  >looper screen-sync ?config 0=
    ntime t-o du>= or  UNTIL ;

: >changed ( -- ) #1000 >changed-to ;

: 1*scale   1 gl-scale ;
: 2*scale   2 gl-scale ;
: 4*scale   4 gl-scale ;

: scroll-yr ( -- float )  scroll-y @ s>f
    y-pos sf@ f2/ rows fm* f+ ;

: +scroll ( f -- )
    scroll-yr f+ actualrows 1 - s>f fmin
    0e fmax screen-scroll ;

: scrolling ( y0 -- )
    rows swap last-y0 motion-y0 ['] +scroll do-motion
    \ long? IF  kbflag @ IF  togglekb  THEN  THEN
    -show ;

#20. 2Value glitch#

: screen-slide ( -- )
    *input dup
    >r IF
	r@ action @ \ dup -1 <> IF  dup .  THEN
	case
	    1 of
		r@ downtime 2@ glitch# d>
		IF  ?toggle  THEN
		r@ action on  endof
	    3 of r@ action on  endof \ cancel
	    9 of r@ action on  endof \ hover
	    abs 1 <> IF  r@ y0 @ scrolling  
	    ELSE  last-y0 motion-y0 ['] +scroll drag-motion  THEN
	    0
	endcase
    THEN  rdrop ;

:noname ( flag -- flag ) level# @ 0> ?EXIT
    screen-sync screen-slide scroll-slide ; IS screen-ops

' gl-type     ' gl-emit     ' gl-cr     ' gl-form output: out>screen
' gl-type-err ' gl-emit-err ' gl-cr-err ' gl-form output: err>screen

out>screen
' gl-atxy IS at-xy
' gl-at-deltaxy IS at-deltaxy
' gl-page IS page
' gl-attr! IS attr!
' gl-control-sequence IS control-sequence
' (theme-color!) is theme-color!

err>screen
' gl-atxy IS at-xy
' gl-at-deltaxy IS at-deltaxy
' gl-page IS page
' gl-err-attr! IS attr!
' gl-control-sequence IS control-sequence
' (theme-color!) is theme-color!

default-out op-vector !

: >screen ( -- )
    ctx 0= IF  window-init  [IFDEF] map-win map-win [THEN] config-changer  THEN
    err>screen op-vector @ debug-vector !  out>screen
    white? IF  >light  ELSE  >dark  THEN  config-changed ;

\ initialize

: term-textures ( -- )
    s" minos2/ascii.png" term-load-textures ;

:noname defers reload-textures  term-textures ; is reload-textures

: term-init ( -- )
\    [IFDEF] clazz [ also jni ] ['] hideprog post-it [ previous ] [THEN]
    >screen-orientation
    create-terminal-program to terminal-program
    terminal-program terminal-init
    term-textures form-chooser
    scale-me ;

' term-init window-init,

[IFDEF] android host? [IF] >dark [THEN] [THEN] \ make black default

\ window-init

previous [IFDEF] android previous previous [THEN] \ remove opengl from search order

\ print system and sh outputs on gl terminal

0 warnings !@
: system ( addr u -- )
    r/o open-pipe throw 0 { fd w^ string }
    fd string $[]slurp string $[]. string $[]free ;
: sh '#' parse cr system ;
warnings !
