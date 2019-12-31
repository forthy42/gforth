\ opengl example

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019 Free Software Foundation, Inc.

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

require gl-helper.fs

also opengl
also [IFDEF] android android [THEN]
also [IFDEF] jni jni [THEN]
also [IFDEF] x11 x11 [THEN]

tex: ascii-tex

: load-textures ( -- )
    ascii-tex s" ascii.png" load-texture 2drop ;

:noname defers reload-textures load-textures ; is reload-textures

\ triangle example

buffer-init

: set-triangle ( angle -- ) pi 1.5e f/ { f: angle f: 120deg }  >v
    angle           fsincos angle 6 fm* fcos .1e f* -1e f+ >xyz v+
    angle 120deg f- fsincos angle 6 fm* fcos .1e f* -1e f+ >xyz v+
    angle 120deg f+ fsincos angle 6 fm* fcos .1e f* -1e f+ >xyz v+ o> ;

\ 0 set-triangle

: colors ( -- ) >v
    red#   i>c v+
    green# i>c v+
    blue#  i>c v+ v> ;

: colors' ( -- ) >v
    blue#  i>c v+
    red#   i>c v+
    green# i>c v+ v> ;

: texcoords >v
    2e0 0e0 >st v+
    1e0 2e0 >st v+
    0e0 0e0 >st v+ o> ;

: normals >v
    3 0 DO  0e 0e -1e n>xyz v+  LOOP  o> ;

Variable new-config
FVariable touch -100e touch f!
FVariable motion 0.01e motion f!

: draw-tri-angle ( f -- )
    0.01e 0.02e 0.15e 1.0e glClearColor
    clear  program init
    Ambient 1 ambient% glUniform1fv
    vi0  set-triangle
    normals texcoords colors
    none-tex
    0 i, 1 i, 2 i,
    GL_TRIANGLES draw-elements
    v0 colors'
    ascii-tex
    GL_TRIANGLES draw-elements
    sync ;

: draw-tri { f: angle -- angle' }
    [IFDEF] ?config-changer ?config-changer [THEN]
    angle draw-tri-angle
    >looper default>ap
    *input >r r@ IF
	r@ action @ abs 1 u> IF
	    \ ." Touch at " r@ x0 ? r@ y0 ? cr
	    \ r@ x0 @ 20 < r@ y0 @ 20 < and IF -1 (bye) THEN
	    r@ x0 @ dpy-w @ 2/ - s>f dpy-h @ 2/ fm/
	    r@ y0 @ dpy-h @ 2/ - s>f dpy-h @ 2/ fm/
	    fover fover fnegate 1.5e lightpos glUniform3f
	    fatan2
	    touch f@ -100e f= IF
		angle f- touch f!
	    ELSE
		touch f@ f-
		fdup angle f-
		fdup pi f> IF  pi f2* f-  THEN
		fdup pi fnegate f< IF  pi f2* f+  THEN
		fdup f0= IF  fdrop  ELSE
		    ( .2e f* motion f@ .8e f* f+ ) motion f!  THEN
		to angle
	    THEN
	    ambient% sf@ 0.05e f- 0.0e fmax ambient% sf!
	ELSE
	    ambient% sf@ 0.05e f+ 1.0e fmin ambient% sf!
	    angle motion f@ f+ to angle
	    motion f@ 0.01e f- 0.95e f* 0.01e f+ motion f!
	    -100e touch f!
	THEN
	\ ." Motion: " motion f@ f. cr 0 -1 at-deltaxy
    THEN
    rdrop angle ;

: tri-loop ( -- ) 1 level# +! 0e  BEGIN draw-tri level# @ 0= UNTIL fdrop ;

: gl-sample ( -- )
    [IFDEF] hidekb  hidekb  +config [THEN]
    [IFDEF] map-win exposed @ 0= IF  map-win  THEN [THEN]
    ['] VertexShader ['] FragmentShader create-program to program
    program init load-textures .info
    tri-loop ;

previous previous previous previous

win 0= [IF] window-init [THEN]

gl-sample
script? [IF] bye [THEN]
