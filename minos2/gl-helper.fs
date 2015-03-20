\ opengl common stuff

require unix/mmap.fs
require mini-oof2.fs

: w, ( n -- )  here 2 allot w! ;
: l, ( n -- )  here 4 allot l! ;

Variable dpy-w
Variable dpy-h
0 Value ctx

s" os-type" environment? [IF]
    2dup s" linux-android" str= [IF] 2drop
	require unix/opengles.fs
	
	also opengl
	
	require unix/android.fs
	also android

	align here
	EGL_SAMPLE_BUFFERS , 1 , \ multisample buffer
	EGL_SAMPLES , 4 , \ 4 samples antialiasing
	EGL_STENCIL_SIZE , $10 ,
	here
	EGL_SURFACE_TYPE , EGL_WINDOW_BIT , \ this is default
	EGL_RENDERABLE_TYPE , EGL_OPENGL_ES2_BIT ,
	EGL_BLUE_SIZE , 8 ,
	EGL_GREEN_SIZE , 8 ,
	EGL_RED_SIZE , 8 ,
	here
	EGL_NONE ,
	Constant attribs
	Constant attribs2
	Constant attribs3

	Create eglattribs
	EGL_CONTEXT_CLIENT_VERSION , 2 ,
	EGL_NONE ,

	: add-precision
	    s" precision mediump float;        // required for GLES 2.0" ;
    [ELSE]
	2dup s" darwin" str= >r s" linux-gnu" str= r> or [IF]
	    require unix/opengl.fs
	    
	    also opengl
	
	    require minos2/linux-gl.fs \ same voc stack effect as on android

	    align here
	    GLX_SAMPLE_BUFFERS  l, 1 l,
	    GLX_SAMPLES         l, 4 l,
	    here
	    GLX_RED_SIZE        l, 8 l,
	    GLX_GREEN_SIZE      l, 8 l,
	    GLX_BLUE_SIZE       l, 8 l,
	    GLX_ALPHA_SIZE      l, 8 l,
	    GLX_DOUBLEBUFFER    l, 1 l,
	    here
	    GL_NONE l,
	    Constant attrib
	    Constant attrib2
	    Constant attrib3
	[THEN]
	: add-precision s" " ;
    [THEN]
[THEN]

Variable configs
Variable numconfigs
Variable eglformat

: ??gl .s ." gl: " glGetError . ; ' ??gl is printdebugdata

[IFDEF] android
    : win app window @ ;
    
    0 Value egldpy
    0 Value surface
    
    : getwh ( -- )
	egldpy surface EGL_WIDTH dpy-w eglQuerySurface drop
	egldpy surface EGL_HEIGHT dpy-h eglQuerySurface drop
	0 0 dpy-w @ dpy-h @ glViewport ;

    : >screen-orientation ( -- )
	screen-orientation@
	dpy-w @ dpy-h @ > over xor 1 and + 3 and
	to screen-orientation ;
    
    : choose-config ( -- )
	0 eglGetDisplay to egldpy
	egldpy 0 0 eglInitialize drop
	egldpy attribs3 configs 1 numconfigs eglChooseConfig drop
	numconfigs @ 0= IF
	    egldpy attribs2 configs 1 numconfigs eglChooseConfig drop
	    numconfigs @ 0= IF
		egldpy attribs configs 1 numconfigs eglChooseConfig drop
		." default config only" EXIT
	    THEN
	THEN ;
    
    : create-context ( -- )
	win 0 0 eglformat ANativeWindow_setBuffersGeometry drop
	egldpy configs @ win 0 eglCreateWindowSurface to surface
	egldpy configs @ 0 eglattribs eglCreateContext to ctx
	egldpy surface dup ctx eglMakeCurrent drop ;

    : sync ( -- )
	egldpy surface eglSwapBuffers drop ;
[THEN]

[IFDEF] linux
    fpath+ gles2
    0 Value visual
    0 Value visuals
    Variable nitems

    \ I once had no luck with glXChooseVisual - this is a replacement:
    true [IF]
	Variable val
	: glXVisual? ( visinfo attrib -- flag ) true { flag }
	    BEGIN  dup l@  WHILE
		    2dup dpy -rot l@ val glXGetConfig 0= flag and to flag
		    dup 4 + l@ val @ u<= flag and to flag
		    8 +
	    REPEAT  2drop flag ;
	
	: glXChooseVisual' ( visinfo n attrib -- visinfo ) { attrib }
	    XVisualInfo * bounds ?DO
		I attrib glXVisual?  IF  I unloop  EXIT  THEN
	    XVisualInfo +LOOP 0 ;

	: choose-config ( -- ) \ visual ?EXIT
	    get-display dpy-h ! dpy-w !
	    dpy screen pad nitems XGetVisualInfo dup to visuals nitems @
	    2dup attrib3 glXChooseVisual' dup 0= IF  drop
		2dup attrib2 glXChooseVisual' dup 0= IF  drop
		    2dup attrib glXChooseVisual' dup
		    0= abort" Unable to choose Visual"
		THEN
	    THEN  to visual 2drop ;
    [ELSE]
	: choose-config ( -- ) \ visual ?EXIT
	    get-display dpy-h ! dpy-w !
	    dpy screen
	    2dup attrib3 glXChooseVisual dup 0= IF  drop
		2dup attrib2 glXChooseVisual dup 0= IF  drop
		    2dup attrib glXChooseVisual dup
		    0= abort" Unable to choose Visual"
		THEN
	    THEN  to visual 2drop ;
    [THEN]
    
    : create-context ( -- ) \ win ?EXIT
	default-events "GL-Window\0" drop dpy-w @ dpy-h @ simple-win
	dpy visual 0 1 glXCreateContext to ctx
	dpy win ctx glXMakeCurrent drop
	visuals Xfree drop 0 to visuals 0 to visual ;

    : >screen-orientation ;

    : sync ( -- )
	dpy win glXSwapBuffers ;
[THEN]

: init-opengl ( -- )
    choose-config create-context getwh ;

?looper \ init-opengl ." Screen size: " dpy-w ? dpy-h ? cr

\ gl shader program

: .glsl-error ( shader -- )
    $1000 pad pad cell+ glGetShaderInfoLog pad cell+ pad l@ #lf skip
    BEGIN  #lf $split dup  WHILE  2swap cr type  REPEAT  2drop
    dup IF  cr type  ELSE  2drop  THEN ;

: compile-shader ( source shadertype -- shader )
    \ ." Compile shader:" cr over @ cstring>sstring type
    glCreateShader dup >r IF
	r@ 1 rot 0 glShaderSource
	r@ glCompileShader
	r@ .glsl-error
	r> EXIT  THEN
    drop r> ;

: shader: ( type "name" -- )
    0 Value here >r dup ,
    here cell+ , \ pointer to the string - it's only one!
    BEGIN  refill  source nip 0<> and  WHILE
	    source s" #precision" str=
	    IF  add-precision  ELSE  source  THEN
	    here swap dup allot move #lf c,
    REPEAT  0 c, drop rdrop ;
\    r@ cell+ swap compile-shader r> cell- ! ;

: recompile-shader ( addr -- )  >body >r
    r@ 2 cells + r@ cell+ @ compile-shader r> ! ;

: shader>string ( xt -- addr u )
    >body 2 cells + @ cstring>sstring ;

: >word ( addr u -- addr' u' ) bounds ?DO
	I c@ bl > IF  I I' over - unloop  EXIT  THEN
    LOOP  s" " ;
: >wordend ( addr u -- addr' u' ) bounds ?DO
	I c@ bl <= IF  I I' over - unloop  EXIT  THEN
    LOOP  s" " ;

: >attrib ( addr u search u1 -- addr' u' )  2>r
    BEGIN  >word  dup WHILE  2dup >wordend 2swap
	2r@ string-prefix?  UNTIL
    THEN 2rdrop ;

: extract-name ( addr u -- addr1 u1 name u2 )
    2dup ';' scan 2dup 1 /string 2>r drop >r over r> swap - nip
    BEGIN  2dup >wordend  dup WHILE  2nip >wordend >word  REPEAT  2drop
    2r> 2swap ;

Variable $attrib

: >bindattrib ( xt program -- )  0 { prog idx } shader>string
    BEGIN  s" attribute " >attrib extract-name dup  WHILE
	    $attrib $!  0 $attrib c$+!
	    prog idx $attrib $@ drop glBindAttribLocation
	    idx 1+ to idx
    REPEAT  2drop 2drop ;

: >univattrib ( xt program -- )  { prog }
    shader>string
    BEGIN  s" uniform " >attrib extract-name dup  WHILE  $attrib $!
	    prog $attrib $@ over + 0 swap c! glGetUniformLocation ,
    REPEAT  2drop 2drop ;

: >univattribs { vs-xt fs-xt program -- locblock }
    here program ,
    vs-xt >univattrib  fs-xt >univattrib  0 , ;

GL_VERTEX_SHADER shader: VertexShader
uniform mat4 u_MVPMatrix;       // A constant representing the combined model/view/projection matrix.
uniform mat4 u_MVMatrix;        // A constant representing the combined model/view matrix.
 
attribute vec4 a_Position;      // Per-vertex position information we will pass in.
attribute vec4 a_Color;         // Per-vertex color information we will pass in.
attribute vec4 a_Normal;        // Per-vertex normal information we will pass in.
attribute vec2 a_TexCoordinate; // Per-vertex texture coordinate information we will pass in.
 
varying vec3 v_Position;        // This will be passed into the fragment shader.
varying vec4 v_Color;           // This will be passed into the fragment shader.
varying vec3 v_Normal;          // This will be passed into the fragment shader.
varying vec2 v_TexCoordinate;   // This will be passed into the fragment shader.
 
// The entry point for our vertex shader.
void main()
{
    // Transform the vertex into eye space.
    v_Position = vec3(u_MVMatrix * a_Position);
 
    // Pass through the color.
    v_Color = a_Color;
 
    // Pass through the texture coordinate.
    v_TexCoordinate = a_TexCoordinate;
 
    // Transform the normal's orientation into eye space.
    v_Normal = vec3(u_MVMatrix * a_Normal);
 
    // gl_Position is a special variable used to store the final position.
    // Multiply the vertex by the matrix to get the final point in normalized screen coordinates.
    gl_Position = u_MVPMatrix * a_Position;
}

GL_FRAGMENT_SHADER shader: FragmentShader
#precision
uniform vec3 u_LightPos;        // The position of the light in eye space.
uniform sampler2D u_Texture;    // The input texture.
uniform float u_Ambient;        // ambient lighting level
uniform vec4 u_Coloradd;        // color bias for texture
 
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
 
// Multiply the color by the diffuse illumination level and texture value to get final output color.
    vec4 texture = texture2D(u_Texture, v_TexCoordinate) + u_Coloradd;
    gl_FragColor = vec4(diffuse, diffuse, diffuse, 1.0) * v_Color * texture;
    // vec4 pixel = (v_Color * texture2D(u_Texture, v_TexCoordinate));
    // gl_FragColor = vec4(pixel.rgb, pixel.a);
    // gl_FragColor = texture2D(u_Texture, v_TexCoordinate);
    // gl_FragColor = (v_Color * diffuse);
    // gl_FragColor = v_Color;
}


0 Value MVPMatrix
0 Value MVMatrix
0 Value LightPos
0 Value Texture
0 Value ambient
0 Value Coloradd
0 Value program

: create-program { vs-xt fs-xt -- program }
    vs-xt recompile-shader
    fs-xt recompile-shader
    glCreateProgram dup >r IF
	r@ vs-xt execute glAttachShader
	r@ fs-xt execute glAttachShader
	vs-xt r@ >bindattrib
	r@ glLinkProgram THEN
    r> ;

[IFDEF] GL_TEXTURE_EXTERNAL_OES
GL_FRAGMENT_SHADER shader: oesShader
#extension GL_OES_EGL_image_external : require
#precision
uniform vec3 u_LightPos;        // The position of the light in eye space.
uniform samplerExternalOES u_Texture;
uniform float u_Ambient;        // ambient lighting level
uniform vec4 u_Colorad;         // color bias for texture
 
varying vec3 v_Position;        // Interpolated position for this fragment.
varying vec4 v_Color;           // This is the color from the vertex shader interpolated across the
                                // triangle per fragment.
varying vec3 v_Normal;          // Interpolated normal for this fragment.
varying vec2 v_TexCoordinate;   // Interpolated texture coordinate per fragment.
void main() {
    float distance = length(u_LightPos - v_Position);
    vec3 lightVector = normalize(u_LightPos - v_Position);
    float diffuse = max(dot(v_Normal, lightVector), 0.0);
    diffuse = diffuse * (1.0 / (1.0 + (0.10 * distance)));
    diffuse = (diffuse * ( 1.0 - u_Ambient )) + u_Ambient;
    gl_FragColor = (diffuse * (v_Color + texture2D(u_Texture, v_TexCoordinate)));
}

: create-oes-program ( -- program )
    ['] VertexShader ['] oesShader create-program ;
[THEN]

: .gl ( n -- )  glGetString cstring>sstring type ;

: .info ( -- )
    ." Version: " GL_VERSION .gl cr
    ." Vendor: " GL_VENDOR .gl cr
    ." Renderer: " GL_RENDERER .gl cr
    ." Extensions: " GL_EXTENSIONS .gl cr ;

: clear ( -- )
    GL_DEPTH_BUFFER_BIT GL_COLOR_BUFFER_BIT or glClear ;

: sf,  ( float -- )  here 1 sfloats allot sf! ;
: sf!+ ( float addr -- addr' )  dup sf! sfloat+ ;

Create z-bias
0e sf, 0e sf, 0e sf, 0e sf,
Create 1-bias
1e sf, 1e sf, 1e sf, 0e sf,

Create unit-matrix
1.0e sf, 0.0e sf, 0.0e sf, 0.0e sf,
0.0e sf, 1.0e sf, 0.0e sf, 0.0e sf,
0.0e sf, 0.0e sf, 1.0e sf, 0.0e sf,
0.0e sf, 0.0e sf, 0.0e sf, 1.0e sf,

unit-matrix 12 sfloats + Constant x-pos
unit-matrix 13 sfloats + Constant y-pos

Create ap-matrix
1.0e sf, 0.0e sf, 0.0e sf, 0.0e sf,
0.0e sf, 1.0e sf, 0.0e sf, 0.0e sf,
0.0e sf, 0.0e sf, 1.0e sf, -1.0e sf,
0.0e sf, 0.0e sf, 0.0e sf, 1.0e sf,

ap-matrix 12 sfloats + Constant x-apos
ap-matrix 13 sfloats + Constant y-apos

: .matrix ( addr -- ) 5 set-precision
    $10 sfloats bounds DO
	I 4 sfloats bounds DO
	    I sf@ f.
	1 sfloats +LOOP cr
    4 sfloats +LOOP ;

: set-matrix ( addr handle -- ) swap >r
    1 false r> glUniformMatrix4fv ;

: >x-pos ( r -- ) x-pos sf!  unit-matrix MVPMatrix set-matrix ;
: >y-pos ( r -- ) y-pos sf!  unit-matrix MVPMatrix set-matrix ;

: set-color+ ( addr -- )  Coloradd 1 rot glUniform4fv ;

: >ortho { f: near f: far f: left f: right f: top f: bottom -- }
    ap-matrix
    near f2* right left f- f/ sf!+ 0e sf!+ 0e sf!+ 0e sf!+
    0e sf!+ near f2* top bottom f- f/ sf!+ 0e sf!+ 0e sf!+
    right left f+ right left f- f/ sf!+
    top bottom f+ top bottom f- f/ sf!+
    near far f+ near far f- f/ sf!+ -1e sf!+
    sfloat+ sfloat+ near far f* f2* near far f- f/ sf!+ 0e sf!+
    drop ;

: ap-set ( -- )
    ap-matrix MVPMatrix set-matrix
    ap-matrix MVMatrix set-matrix ;

: >ap ( near far scale -- ) f2* 1/f { f: scale }
    scale dpy-w @ fm* fdup fnegate fswap
    scale dpy-h @ fm* fdup fnegate fswap >ortho
    \ cr ap-matrix .matrix
    ap-set ;
: >apxy ( xoff yoff -- )  y-apos sf!  x-apos sf! ;

\ textures

Create white-texture \ aabbggrr
  $ffffffff l,  $ffffffff l,
  $ffffffff l,  $ffffffff l,

: rgba-texture { addr w h -- }
    GL_TEXTURE_2D 0 GL_RGBA w h
    0 GL_RGBA GL_UNSIGNED_BYTE addr glTexImage2D
    GL_TEXTURE_2D GL_TEXTURE_WRAP_S GL_REPEAT glTexParameteri
    GL_TEXTURE_2D GL_TEXTURE_WRAP_T GL_REPEAT glTexParameteri ;

: rgba-map { addr w h -- }
    GL_TEXTURE_2D 0 GL_RGBA w h
    0 GL_RGBA GL_UNSIGNED_BYTE addr glTexImage2D ;

: rgba-subtex { addr x y w h -- }
    GL_TEXTURE_2D 0 x y w h
    GL_RGBA GL_UNSIGNED_BYTE addr glTexSubImage2D ;

: rgba-newtex { w h -- }
    w h * 2* 2* dup allocate throw { len addr }  addr len erase
    GL_TEXTURE_2D 0 GL_RGBA w h
    0 GL_RGBA GL_UNSIGNED_BYTE addr glTexImage2D
    addr free throw ;

: wrap ( -- )
    GL_TEXTURE_2D GL_TEXTURE_WRAP_S GL_REPEAT glTexParameteri
    GL_TEXTURE_2D GL_TEXTURE_WRAP_T GL_REPEAT glTexParameteri ;
: mipmap ( -- )  GL_TEXTURE_2D glGenerateMipmap ;
: linear ( -- )
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR glTexParameteri
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR glTexParameteri ;
: nearest ( -- )
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_NEAREST glTexParameteri
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_NEAREST glTexParameteri ;
[IFDEF] GL_TEXTURE_EXTERNAL_OES
: nearest-oes ( -- )
    GL_TEXTURE_EXTERNAL_OES GL_TEXTURE_MAG_FILTER GL_LINEAR  glTexParameteri
    GL_TEXTURE_EXTERNAL_OES GL_TEXTURE_MIN_FILTER GL_NEAREST glTexParameteri
    GL_TEXTURE_EXTERNAL_OES GL_TEXTURE_WRAP_S GL_CLAMP_TO_EDGE glTexParameteri
    GL_TEXTURE_EXTERNAL_OES GL_TEXTURE_WRAP_T GL_CLAMP_TO_EDGE glTexParameteri ;
[THEN]

: no-texture ( -- )  white-texture 2 2 rgba-texture wrap nearest ;

0 Value current-tex

\ use texture

$100 Constant max-texture#
max-texture# cells buffer: textureID
Variable tex-index

: texture-init max-texture# textureId glGenTextures ;

: tex@ ( index -- texture )  cells textureID + @ dup to current-tex ;
: tex[] ( index -- )  tex@ GL_TEXTURE_2D swap glBindTexture ;
: tex: ( "name" -- )
    Create tex-index @ ,  1 tex-index +!
    DOES> @ tex[] ;

tex: none-tex

\ require png-texture.fs
require soil-texture.fs

[IFDEF] GL_TEXTURE_EXTERNAL_OES
    : ext-tex[] ( index -- )  tex@ GL_TEXTURE_EXTERNAL_OES swap glBindTexture ;
    : ext-tex: ( "name" -- )  tex: DOES> @ ext-tex[] ;
    ext-tex: media-tex
[THEN]

1 sfloats buffer: ambient%  1.0e ambient% sf!

: init { program -- }
    GL_DITHER glEnable
    [IFDEF] GL_MULTISAMPLE  GL_MULTISAMPLE glEnable  [THEN]
    GL_BLEND glEnable
    GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA glBlendFunc
    program glUseProgram
    program "u_MVPMatrix\0" drop glGetUniformLocation to MVPMatrix
    program "u_MVMatrix\0" drop glGetUniformLocation to MVMatrix
    program "u_LightPos\0" drop glGetUniformLocation to LightPos
    program "u_Texture\0" drop glGetUniformLocation to Texture
    program "u_Ambient\0" drop glGetUniformLocation to Ambient
    program "u_Coloradd\0" drop glGetUniformLocation to Coloradd
    GL_UNPACK_ALIGNMENT 1 glPixelStorei
    GL_TEXTURE0 glActiveTexture
    none-tex no-texture
    Texture 0 glUniform1i
    Ambient 1 ambient% glUniform1fv
    LightPos 0.0e 0.0e -0.3e glUniform3f
    z-bias set-color+
    .01e 100e dpy-w @ dpy-h @ min s>f f2/ 100 fm* >ap ;

\ glDrawElements helper

$8 cells buffer: gl-buffers
: buf@ ( n -- buf ) cells gl-buffers + @ ;
: bind-buf ( type n -- ) buf@ glBindBuffer ;

0 Value array-buf
0 Value index-buf

$1000 Value points# \ 4k points

object class
    sffield: v.x
    sffield: v.y
    sffield: v.z
    sffield: v.t

    sffield: c.r
    sffield: c.g
    sffield: c.b
    sffield: c.a

    sffield: n.x
    sffield: n.y
    sffield: n.z
    sffield: n.t

    sffield: t.s
    sffield: t.t
    sffield: m.i \ matrix index, unused
    sffield: t.i \ texture index, unused
    0 +field next-vertex
end-class vertex-c
vertex-c >osize @ Constant vertex#

: vertex-init ( -- ) 0 >o
    0 glEnableVertexAttribArray
    0 4 GL_FLOAT GL_FALSE vertex# v.x glVertexAttribPointer \ vertex
    1 glEnableVertexAttribArray
    1 4 GL_FLOAT GL_FALSE vertex# c.r glVertexAttribPointer \ color
    2 glEnableVertexAttribArray
    2 4 GL_FLOAT GL_FALSE vertex# n.x glVertexAttribPointer \ normal
    3 glEnableVertexAttribArray
    3 2 GL_FLOAT GL_FALSE vertex# t.s glVertexAttribPointer \ texture
    o> ;

: buffer-init ( -- )
    index-buf 0= IF  points# 2* alloc+guard to index-buf  THEN
    array-buf 0= IF  points# vertex# * alloc+guard to array-buf  THEN
    gl-buffers @ 0= IF  4 gl-buffers glGenBuffers  THEN
    GL_ELEMENT_ARRAY_BUFFER 1 bind-buf
    GL_ELEMENT_ARRAY_BUFFER points# 2* index-buf GL_DYNAMIC_DRAW
    glBufferData
    GL_ARRAY_BUFFER 0 bind-buf
    GL_ARRAY_BUFFER points# vertex# * array-buf GL_DYNAMIC_DRAW
    glBufferData
    vertex-init ;

array-buf Value buf^
index-buf Value index^

: draw-elements ( type -- )
    GL_ARRAY_BUFFER 0 buf^ array-buf - array-buf glBufferSubData
    GL_ELEMENT_ARRAY_BUFFER 0 index^ index-buf - index-buf glBufferSubData
    index^ index-buf - 2/ GL_UNSIGNED_SHORT 0 glDrawElements ;

: v0 ( -- ) array-buf to buf^ ;
: i0 ( -- ) index-buf to index^ ;

: >v ( -- o:vertex0 )  ]] buf^ >o [[ ; immediate compile-only
: v+ ( o:vertex -- o:vertex' )
    next-vertex >o rdrop ;
: v> ( o:vertex -- )  ]] o ->buf^ o> [[ ; immediate compile-only
: i? ( -- n )  buf^ array-buf - vertex# / ;
: i, ( n -- )
    index^ dup 2 + to index^ w! ;
Variable i-off
: i>off ( -- )  i? i-off ! ;
: ltri ( off -- )  i-off @ dup i, dup 1+ i, + i, ;
: rtri ( off -- )  i-off @ dup 1+ i, + dup i, 1+ i, ;
: quad ( off -- )  dup ltri rtri ;

: >xyz ( x y z -- )  v.z sf! v.y sf! v.x sf! 1e v.t sf! ;
\ note: this is a right hand system, therefore the z coordinate is negative
: >xy ( x y -- )  -1e >xyz ;
: color! ( rgba addr -- rgb ) >r dup $FF and s>f $FF fm/ r> sf! 8 rshift ;
: rgba>c ( rgba -- )  c.a color! c.b color! c.g color! c.r color! drop ;
: abgr>c ( abgr -- )  c.r color! c.g color! c.b color! c.a color! drop ;
: f>c ( r g b a -- )  c.a sf! c.b sf! c.g sf! c.r sf! ;
: n>xyz ( x y z -- ) n.z sf! n.y sf! n.x sf! 1e n.t sf! ;
: n> ( -- ) -1e n.z sf! 0e n.y sf! 0e n.x sf! 1e n.t sf! ;
: >st ( s t -- ) t.t sf! t.s sf! ;

\ window closed/reopened

: helper-init  init-opengl texture-init
    ['] VertexShader ['] FragmentShader create-program to program
    program init  buffer-init ;

:noname  defers window-init helper-init ; IS window-init

\ click region stuff

: click-regions ( w h -- x' y' )  >r >r
    *input x0 @ r> dpy-w @ */
    *input y0 @ r> dpy-h @ */ ;

\ toggle and drag time stuff

: ftime ( -- r ) ntime d>f 1e-9 f* ;

0.5e FConstant rel-drag
0.3e FConstant abs-drag
FVariable drag-time
: f!@ ( r1 addr -- r2 ) dup f@ fswap f! ;
: delta-tc  ( -- r ) *input downtime 2@ d>f 1e-3 f* ;
: delta-t ( -- r ) *input action @ 0< IF  ftime  ELSE  delta-tc  THEN
    fdup drag-time f!@ f- fdup 1e f> IF  fdrop 0e  THEN ;

FVariable motion-x0
FVariable motion-y0
Variable last-x0  -100 last-x0 !
Variable last-y0  -100 last-y0 !
0.01e FConstant glitch-click#
0.2e  FConstant short-click#
0.5e  FConstant long-click#

: short? ( -- flag )
    delta-tc fdup glitch-click# f> short-click# f< and ;
: long? ( -- flag )
    delta-tc long-click# f> ;
: !click ( -- )  0e motion-x0 f! 0e motion-y0 f! ftime drag-time f! ;
[IFUNDEF] togglekb : togglekb ; [THEN]
: ?toggle ( -- )
    short? motion-y0 f@ 2e f< and IF  togglekb need-show off  THEN ;

: do-motion { rows cur old motion xt -- }
    old @ -100 = IF
	cur old !
    ELSE
	cur old @ over old ! swap -
	s>f dpy-h @ s>f rows fm/ f/ fdup f2/ motion f@ f2/ f+ motion f!
	xt execute
    THEN ;

: drag-motion { old motion xt -- } delta-t { f: dt }
    motion f@ rel-drag dt f** f*
    fdup f0< fabs abs-drag dt f* f- 0e fmax IF fnegate THEN
    fdup motion f! fdup f0<> IF  xt execute  ELSE  fdrop  THEN
    -100 old ! ;

previous
