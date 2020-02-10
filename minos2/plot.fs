\ MINOS2 waveform plotter

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

\ waveform plotter uses a canvas to draw waveforms
\ either x/y waveforms or y waveforms with a constant delta x

: line-strip> ( -- )
    opengl:GL_LINE_STRIP draw-elements ;

: lines> ( -- )
    opengl:GL_LINES draw-elements ;

: flush-lines? ( n -- flag ) >r
    i? r@ + points# 2* u>=
    v? r> + points# u>= or
    IF  line-strip> vi0 true  ELSE  false  THEN ;

: minmax' ( addr u xmin xmax -- xmin' xmax' )
    bounds ?DO
	I f@ fmax fswap I f@ fmin fswap
    [ 1 floats ]L +LOOP ;
: minmax-start ( -- -inf inf )
    [ 1e 0e f/ ] FLiteral fdup fnegate ;
: minmax ( addr u -- xmin xmax )
    minmax-start minmax' ;

: fsum ( addr u -- xmax )
    \G minmax for deltax y plot
    0e bounds ?DO  I f@ f+  [ 1 floats ]L +LOOP ;

: plot-x-minmax ( yaddr yu color ymin ymax -- )
    { f: color f: ymin f: ymax }
    vi0 -1e to t.i0
    w dup [ 1 floats ]L / 1- fm/ { f: xsc }
    h ymin ymax f- f/ { f: ysc }
    y ymin ysc f* f- { f: y0 } frame# { f# } x
    bounds ?DO
	fdup I f@ i>off >v
	ysc f*  y0 f+ >xy  xsc f+
	color i>c n> 1/2 fdup f# #>st v+> i-off @ i,
	1 flush-lines? IF  0  ELSE  [ 1 floats ]L  THEN
    +LOOP  fdrop line-strip> ;

: plot-xy-minmax ( addrx addry uy color xmin xmax ymin ymax -- )
    { f: color f: xmin f: xmax f: ymin f: ymax }
    vi0 -1e to t.i0
    w xmax xmin f- f/  h ymin ymax f- f/ { f: xsc f: ysc }
    x xmin xsc f* f-  y ymin ysc f* f- { f: x0 f: y0 } frame# { f# }
    bounds ?DO
	dup f@ xsc f* x0 f+ float+
	I   f@ ysc f* y0 f+
	i>off >v >xy
	color i>c n> 1/2 fdup f# #>st v+>  i-off @ i,
	1 flush-lines? IF  0  ELSE  [ 1 floats ]L  THEN
    +LOOP  drop line-strip> ;

: plot-dxy-minmax ( addrx addry uy color xmax ymin ymax -- )
    { f: color f: xmax f: ymin f: ymax }
    vi0 -1e to t.i0
    w xmax f/  h ymin ymax f- f/ { f: xsc f: ysc }
    y ymin ysc f* f- { f: y0 } frame# { f# } x
    bounds ?DO
	fdup
	dup f@ xsc f*    f+ fswap float+
	I   f@ ysc f* y0 f+
	i>off >v >xy
	color i>c n> 1/2 fdup f# #>st v+>  i-off @ i,
	1 flush-lines? IF  0  ELSE  [ 1 floats ]L  THEN
    +LOOP  fdrop drop line-strip> ;

: plot-x ( addr u color o:canvas -- )
    >r 2dup minmax r> plot-x-minmax ;

: plot-xy ( addrx addry uy color o:canvas -- )
    >r 2 pick over minmax 2dup minmax r> plot-xy-minmax ;

: plot-dxy ( addrdx addry uy color o:canvas -- )
    >r 2 pick over 1 floats - fsum 2dup minmax r> plot-dxy-minmax ;
