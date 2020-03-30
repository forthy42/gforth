\ simple draw-into-memory style canvas

\ Author: Bernd Paysan
\ Copyright (C) 2020 Bernd Paysan

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

require widgets.fs

also minos

ctx 0= [IF]  window-init  [THEN]

require minos2/font-style.fs

44e update-size#

require minos2/text-style.fs
require presentation-support.fs

tex: memimage

256 Value mem-w
256 Value mem-h

mem-w mem-h * sfloats Value mem-size
mem-size allocate throw Value mem-buf

: pixel! ( rgba x y -- )
    mem-w * + sfloats mem-buf + be-l! ;
: update-memimage ( -- )
    memimage mem-buf mem-w mem-h rgba-texture mipmap ;
: test-image ( -- )
    mem-w 0 DO
	mem-h 0 DO
	    I 24 lshift J 16 lshift + $FF + I J pixel!
	LOOP
    LOOP  update-memimage ;
test-image

glue new >o
mem-w 2* s>f hglue-c df!
mem-h 2* s>f vglue-c df!
o o> Constant memimage-glue

{{
    $000000FF $FFFFFFFF pres-frame
    {{
	glue*l }}glue
	{{
	    glue*l }}glue
	memimage-glue ' memimage white# }}image
	    glue*l }}glue
	}}h box[]
	glue*l }}glue
    }}v box[]
}}z box[] to top-widget

presentation
bye
