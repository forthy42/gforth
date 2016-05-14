\ OpenGL terminal

require ansi.fs \ we may want to support colorize.fs

\ opengl common stuff

\ :noname source type cr stdout flush-file throw ; is before-line

require gl-helper.fs

also [IFDEF] android android [THEN]

GL_FRAGMENT_SHADER shader: TerminalShader
#precision
uniform vec3 u_LightPos;        // The position of the light in eye space.
uniform sampler2D u_Texture;    // The input texture.
uniform float u_Ambient;        // ambient lighting level
uniform sampler2D u_Charmap;    // The character map
uniform sampler2D u_Colormap;   // the available colors
uniform vec2 u_texsize;         // the screen texture size
 
varying vec3 v_Position;        // Interpolated position for this fragment.
varying vec4 v_Color;           // This is the color from the vertex shader interpolated across the
                                // triangle per fragment.
varying vec3 v_Normal;          // Interpolated normal for this fragment.
varying vec2 v_TexCoordinate;   // Interpolated texture coordinate per fragment.
 
// The entry point for our fragment shader.
void main()
{
    // Will be used for attenuation.
    float distance = length(u_LightPos - v_Position);
 
    // Get a lighting direction vector from the light to the vertex.
    vec3 lightVector = normalize(u_LightPos - v_Position);
 
    // Calculate the dot product of the light vector and vertex normal. If the normal and light vector are
    // pointing in the same direction then it will get max illumination.
    float diffuse = max(dot(v_Normal, lightVector), 0.0);
 
    // Add attenuation.
    diffuse = diffuse * (1.0 / (1.0 + (0.10 * distance)));
 
    // Add ambient lighting
    diffuse = (diffuse * ( 1.0 - u_Ambient )) + u_Ambient;
 
    vec4 chartex = texture2D(u_Charmap, v_TexCoordinate);
    vec4 fgcolor = texture2D(u_Colormap, vec2(chartex.z, 0.));
    vec4 bgcolor = texture2D(u_Colormap, vec2(chartex.w, 0.));
    vec2 charxy = chartex.xy + vec2(0.0625, 0.125)*u_texsize*mod(v_TexCoordinate, 1.0/u_texsize);
    // mix background and foreground colors by character ROM alpha value
    // and multiply by diffuse
    vec4 pixel = texture2D(u_Texture, charxy);
    gl_FragColor = vec4(diffuse, diffuse, diffuse, 1.0)*(bgcolor*(1.0-pixel.a) + fgcolor*pixel.a);
    // gl_FragColor = diffuse*mix(bgcolor, fgcolor, pixel.a);
    // gl_FragColor = (v_Color * diffuse * pixcolor);
}

0 Value Charmap
0 Value Colormap
0 value texsize
0 Value terminal-program

: create-terminal-program ( -- program )
    ['] VertexShader ['] TerminalShader create-program ;

: terminal-init { program -- } program init
    program "u_Charmap\0" drop glGetUniformLocation to Charmap
    program "u_Colormap\0" drop glGetUniformLocation to Colormap
    program "u_texsize\0" drop glGetUniformLocation to texsize
    Charmap 1 glUniform1i
    Colormap 2 glUniform1i ;

tex: chars-tex
tex: video-tex
tex: color-tex

\ Variables and constants

[IFUNDEF] l, ' , Alias l, [THEN]

Create color-matrix \ vt100 colors
\ RGBA, but this is little endian, so write ABGR ,
$ff000000 l, \ Black
$ff0000ff l, \ Red
$ff00ff00 l, \ Green
$ff00ffff l, \ Yellow
$ffff0000 l, \ Blue
$ffff00ff l, \ Magenta
$ffffff00 l, \ Cyan
$ffffffff l, \ White
$ff404040 l, \ dimm Black
$ff4040bf l, \ dimm Red
$ff40bf40 l, \ dimm Green
$ff40bfbf l, \ dimm Yellow
$ffbf4040 l, \ dimm Blue
$ffbf40bf l, \ dimm Magenta
$ffbfbf40 l, \ dimm Cyan
$ffbfbfbf l, \ dimm White

: term-load-textures ( addr u -- )
    chars-tex load-texture 2drop linear
    GL_TEXTURE2 glActiveTexture
    color-tex color-matrix $10 1 rgba-map nearest
    GL_TEXTURE0 glActiveTexture ;

Variable color-index
Variable err-color-index
bl dup $70 and 5 lshift or $F0F and 4 lshift
dup color-index ! err-color-index !
Variable std-bg

: ?default-fg ( n -- color ) dup 6 <= IF
	drop default-color fg>  THEN  $F xor ;
: ?default-bg ( n -- color ) dup 6 <= IF
	drop default-color bg>  THEN  $F xor ;
: fg! ( index -- )
    dup 0= IF  drop  EXIT  THEN  ?default-fg
    4 lshift color-index 2 + c! ;
: bg! ( index -- )
    dup 0= IF  drop  EXIT  THEN  ?default-bg
    4 lshift color-index 3 + c! ;
: err-fg! ( index -- ) ?default-fg
    4 lshift err-color-index 2 + c! ;
: err-bg! ( index -- ) ?default-bg
    4 lshift err-color-index 3 + c! ;
: bg>clear ( index -- ) $F xor
    $F and sfloats color-matrix +
    count s>f $FF fm/
    count s>f $FF fm/
    count s>f $FF fm/
    c@    s>f $FF fm/ glClearColor ;

: std-bg! ( index -- )  dup bg! dup std-bg ! bg>clear ;

: >extra-colors-bg ( -- ) >bg
    err-color  $F0F and over or to err-color
    info-color $F0F and over or to info-color
    warn-color $F0F and over or to warn-color drop ;

: >white White std-bg! White err-bg! Black fg! Red err-fg!
    White >extra-colors-bg White >bg Black >fg or to default-color ;
: >black Black std-bg! Black err-bg! White fg! Red err-fg!
    Black >extra-colors-bg Black >bg White >fg or to default-color ;

256 Value videocols
0   Value videorows

2Variable gl-xy  0 0 gl-xy 2!
2Variable gl-wh 24 80 gl-wh 2!
Variable gl-lineend
Variable scroll-y
FVariable scroll-dest
FVariable scroll-source
FVariable scroll-time

80 Value hcols
48 Value vcols

: form-chooser ( -- )
    screen-orientation 1 and  IF  hcols  ELSE  vcols  THEN
    dup dpy-h @ dpy-w @ 2* */ swap gl-wh 2! ;

: show-rows ( -- n ) videorows scroll-y @ - rows 1+ min ;
: nextpow2 ( n -- n' )
    1 BEGIN  2dup u>  WHILE 2*  REPEAT  nip ;

: >rectangle ( -- )
    show-rows s>f rows fm/ -2e f* 1e f+
    >v
    -1e fover >xy n> v+
    -1e 1e >xy n> v+
    1e  1e >xy n> v+
    1e  fswap >xy n> v+ o> ;

: >texcoords ( -- )
    cols s>f videocols fm/  show-rows dup s>f nextpow2 fm/
    { f: tx f: ty }
    >v
    0e ty >st v+
    0e 0e >st v+
    tx 0e >st v+
    tx ty >st v+ v> ;

0 Value videomem

\ : blank-screen ( -- )
\     color-index @ videomem videocols videorows * sfloats bounds ?DO
\ 	dup I l!
\     1 sfloats +LOOP  drop ;
\ blank-screen

: resize-screen ( -- )
    gl-wh @ videocols >= gl-xy @ videorows >= or IF
	gl-wh @ nextpow2 videocols max to videocols
	videomem videocols gl-xy @ 1+ nextpow2 * sfloats dup >r
	resize throw
	to videomem
	color-index @
	videomem r>
	videocols videorows * sfloats /string bounds ?DO
	    dup I l!
	1 sfloats +LOOP drop
	gl-xy @ 1+ to videorows
    THEN ;

2 sfloats buffer: texsize.xy

: draw-now ( -- )
    GL_TEXTURE1 glActiveTexture
    video-tex
    show-rows nextpow2 s>f  videocols s>f texsize.xy sf!+ sf!
    texsize 1 texsize.xy glUniform2fv
    videomem scroll-y @ videocols * sfloats +
    videocols show-rows nextpow2 rgba-map wrap nearest

    v0 >rectangle >texcoords
    GL_TEXTURE0 glActiveTexture
    chars-tex
    i0 0 i, 1 i, 2 i, 0 i, 2 i, 3 i,
    GL_TRIANGLES draw-elements ;

: screen-scroll ( r -- )  fdup floor fdup f>s scroll-y ! f-
    f2* rows fm/ >y-pos  need-sync on ;

: gl-char' ( -- addr )
    gl-xy 2@ videocols * + sfloats videomem + ;

: gl-form ( -- h w ) gl-wh 2@ ;

Variable gl-emit-buf

: gl-cr ( -- )
    gl-lineend @ 0= IF
	gl-xy 2@ 1+ nip 0 swap gl-xy 2! THEN
    resize-screen  need-sync on ;

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
	dup wcwidth -1 = IF  drop $7F
	ELSE  dup wcwidth 2 = IF  drop  13  ELSE  $7F umin  THEN
	THEN
    0 endcase ;

: (gl-emit) ( char color -- )
    over 7 = IF  2drop  EXIT  THEN
    over #lf = IF  2drop gl-cr  EXIT  THEN
    over #cr = IF  2drop gl-cr  EXIT  THEN
    >r
    gl-emit-buf c$+!  gl-emit-buf $@ tuck x-size u< IF  rdrop  EXIT  THEN
    gl-emit-buf $@ drop xc@ xchar>glascii
    gl-emit-buf $@ x-width abs { n }
    gl-emit-buf $off
    
    resize-screen  need-sync on
    dup $70 and 5 lshift or $F0F and 4 lshift r> $FFFF0000 and or
    n 0 ?DO
	dup gl-char' l!
	gl-xy 2@ >r 1+ dup cols = dup gl-lineend !
	IF  drop 0 r> 1+ >r  THEN
	r> gl-xy 2!  $10 +
    LOOP  drop ;

: gl-emit ( char -- )  color-index @ (gl-emit) ;
: gl-emit-err ( char -- )  err-color-index @ (gl-emit) ;

: gl-type ( addr u -- )
    bounds ?DO  I c@ gl-emit  LOOP ;

: gl-type-err ( addr u -- )
    bounds ?DO  I c@ gl-emit-err  LOOP ;

: gl-atxy ( x y -- )
    >r gl-wh @ min 0 max r> gl-xy 2! ;

: gl-at-deltaxy ( x y -- )
    >r s>d screenw @ sm/rem r> +
    gl-xy 2@ rot + >r + r> gl-atxy ;

: gl-page ( -- )  0 0 gl-atxy  0 to videorows
    0e screen-scroll  0e fdup scroll-source f! scroll-dest f!
    videomem videocols sfloats resize throw to videomem
    resize-screen need-sync on ;

: ?invers ( attr -- attr' ) dup invers and IF  $778 xor  THEN ;
: >default ( attr -- attr' )
    dup  bg> 6 <= $F and >bg
    over fg> 6 <= $F and >fg or
    default-color -rot mux ;
: gl-attr! ( attribute -- )
    >default ?invers  dup bg> bg! fg> fg! ;
: gl-err-attr! ( attribute -- )
    >default ?invers  dup bg> err-bg! fg> err-fg! ;

0.25e FConstant scroll-deltat
: >scroll-pos ( -- 0..1 )
    ftime scroll-time f@ f- scroll-deltat f/
    1e fmin 0.5e f- pi f* fsin 1e f+ f2/ ;

: set-scroll ( r -- )
    scroll-y @ s>f y-pos sf@ f2/ rows fm* f+ scroll-source f!
    scroll-dest f!  ftime scroll-time f! ;

: scroll-slide ( -- )  scroll-dest f@ scroll-source f@ f= ?EXIT
    >scroll-pos fdup 1e f= IF  scroll-dest f@ scroll-source f!  THEN
    fdup scroll-dest f@ f* 1e frot f- scroll-source f@ f* f+ screen-scroll ;

: screen->gl ( -- )
    videomem 0= IF  resize-screen  THEN
    std-bg @ bg>clear clear
    terminal-program glUseProgram
    gl-char' 2 + dup be-uw@ swap le-w!
    draw-now
    gl-char' 2 + dup be-uw@ swap le-w!
    sync ;

: show-cursor ( -- )  need-show @ 0= ?EXIT
    rows ( kbflag @ IF  dup 10 / - 14 -  THEN ) >r
    gl-xy @ scroll-y @ dup r@ + within 0= IF
       gl-xy @ 1+ r@ - 0 max s>f set-scroll
    THEN  rdrop  need-show off ;

[IFUNDEF] win : win app window @ ; [THEN]
: screen-sync ( -- )  rendering @ -2 > ?EXIT \ don't render if paused
    need-sync @ win and level# @ 0<= and IF
	getwh  show-cursor screen->gl need-sync off  THEN ;

: config-changer ( -- )
    >screen-orientation need-sync on ;
\    ." config changed to: " w ? h ? cr

:noname
    config-changer form-chooser  winch? on  screen-sync ;
is config-changed

: gl-fscale ( f -- )
    1/f 80 fdup fm* f>s to hcols 48 fm* f>s to vcols
    resize-screen config-changed ;
: gl-scale ( n -- ) s>f gl-fscale ;

: 1*scale   1 gl-scale ;
: 2*scale   2 gl-scale ;
: 4*scale   4 gl-scale ;

: scroll-yr ( -- float )  scroll-y @ s>f
    y-pos sf@ f2/ rows fm* f+ ;

: +scroll ( f -- f' )
    scroll-yr f+ videorows 1 - s>f fmin
    0e fmax screen-scroll ;

: scrolling ( y0 -- )
    rows swap last-y0 motion-y0 ['] +scroll do-motion
    long? IF  kbflag @ IF  togglekb  THEN  THEN
    need-show off ;

: screen-slide ( -- )
    *input >r
    r@ IF
	r@ action @ \ dup -1 <> IF  dup .  THEN
	case
	    1 of  ?toggle  r@ action on  endof
	    3 of           r@ action on  endof \ cancel
	    9 of           r@ action on  endof \ hover
	    abs 1 <> IF  r@ y0 @ scrolling  
	    ELSE  last-y0 motion-y0 ['] +scroll drag-motion  THEN
	    0
	endcase
    THEN  rdrop ;

:noname ( flag -- flag ) level# @ 0> ?EXIT
    screen-sync screen-slide scroll-slide ; IS screen-ops

' gl-type     ' gl-emit     ' gl-cr ' gl-form output: out>screen
' gl-type-err ' gl-emit-err ' gl-cr ' gl-form output: err>screen

out>screen
' gl-atxy IS at-xy
' gl-at-deltaxy IS at-deltaxy
' gl-page IS page
' gl-attr! IS attr!

err>screen
' gl-atxy IS at-xy
' gl-at-deltaxy IS at-deltaxy
' gl-page IS page
' gl-err-attr! IS attr!

default-out op-vector !

: >screen  err>screen op-vector @ debug-vector ! out>screen ;

\ initialize

: term-init ( -- )
    [IFDEF] clazz [ also jni ] ['] hideprog post-it [ previous ] [THEN]
    >screen-orientation
    create-terminal-program to terminal-program
    terminal-program terminal-init
    s" minos2/ascii.png" term-load-textures form-chooser
    unit-matrix MVPMatrix set-matrix ;

:noname  defers window-init term-init config-changer ; IS window-init

window-init

previous previous \ remove opengl from search order

\ print system and sh outputs on gl terminal

0 warnings !@
: system ( addr u -- )
    r/o open-pipe throw 0 { fd w^ string }
    fd string $[]slurp string $[]. string $[]off ;
: sh '#' parse cr system ;
warnings !

>black \ make black default
