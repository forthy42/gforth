\ MINOS2 waveform plotter

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

\ waveform plotter uses a canvas to draw waveforms
\ either x/y waveforms or y waveforms with a constant delta x

: lines> ( -- )
    opengl:GL_LINE_STRIP draw-elements ;

: flush-lines? ( n -- flag ) >r
    i? r@ + points# 2* u>=
    v? r> + points# u>= or
    IF  lines> vi0 true  ELSE  false  THEN ;

: minmax-x ( addr u -- ymin ymax )
    [ 1e 0e f/ ] FLiteral fdup fnegate
    { f: ymin f: ymax }
    bounds ?DO
	I f@ fdup ymin fmin to ymin ymax fmax to ymax
    [ 1 floats ]L +LOOP
    ymin ymax ;

: minmax-xy ( addr u -- xmin ymin xmax ymay )
    \G minmax for xy plot
    [ 1e 0e f/ ] FLiteral fdup fdup fnegate fdup
    { f: xmin f: ymin f: xmax f: ymax }
    bounds ?DO
	I        f@ fdup xmin fmin to xmin xmax fmax to xmax
	I float+ f@ fdup ymin fmin to ymin ymax fmax to ymax
    [ 2 floats ]L +LOOP
    xmin ymin xmax ymax ;

: minmax-dxy ( addr u -- xmin ymin xmax ymay )
    \G minmax for deltax y plot
    [ 1e 0e f/ ] FLiteral 0e fover fnegate
    { f: ymin f: xmax f: ymax }
    bounds ?DO
	I        f@ +to xmax
	I float+ f@ fdup ymin fmin to ymin ymax fmax to ymax
    [ 2 floats ]L +LOOP
    0e ymin xmax ymax ;

: plot-x-minmax ( addr u color ymin ymax -- )
    { color f: ymin f: ymax }
    w dup [ 1 floats ]L / fm/
    h ymax ymin f- f/ { f: xsc f: ysc }
    bounds ?DO
	i>off >v
	I            xsc fm* x f+
	I f@ ymin f- ysc f*  y f+ >xy
	color rgba>c n> 0.5e fdup frame# #>st v+ v>  i-off @ i,
	1 flush-lines? IF  0  ELSE  [ 1 floats ]L  THEN
    +LOOP ;

: plot-xy-minmax ( addr u color xmin xmax ymin ymax -- )
    { color f: xmin f: ymin f: xmax f: ymax }
    w xmax xmin f- f/  h ymax ymin f- f/ { f: xsc f: ysc }
    bounds ?DO
	i>off >v
	I        f@ xmin f- xsc f* x f+
	I float+ f@ ymin f- ysc f* y f+ >xy
	color rgba>c n> 0.5e fdup frame# #>st v+ v>  i-off @ i,
	1 flush-lines? IF  0  ELSE  [ 2 floats ]L  THEN
    +LOOP ;

: plot-dxy-minmax ( addr u color xmin xmax ymin ymax -- )
    { color f: xmin f: ymin f: xmax f: ymax }
    w xmax xmin f- f/  h ymax ymin f- f/ { f: xsc f: ysc }
    xmin to xmax
    bounds ?DO
	i>off >v
	xmax I f@ +to xmax  xsc f* x f+
	I float+ f@ ymin f- ysc f* y f+ >xy
	color rgba>c n> 0.5e fdup frame# #>st v+ v>  i-off @ i,
	1 flush-lines? IF  0  ELSE  [ 2 floats ]L  THEN
    +LOOP ;

: plot-x ( addr u color o:canvas -- )
    >r 2dup minmax-x r> plot-x-minmax ;

: plot-xy ( addr u color o:canvas -- )
    >r 2dup minmax-xy r> plot-xy-minmax ;

: plot-dxy ( addr u color o:canvas -- )
    >r 2dup minmax-dxy r> plot-dxy-minmax ;
