\ ΜΙΝΩΣ2 Tutorial framework

\ Authors: Bernd Paysan
\ Copyright (C) 2022,2024 Free Software Foundation, Inc.

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
require minos2/widgets.fs

also minos

require minos2/font-style.fs
require minos2/text-style.fs
require minos2/presentation-support.fs

window-init
88e update-size#

' }}i18n-text is }}text'

l"  " >r
: em-space ( -- o ) [ r> ]L }}text' ;

: include-tutorials ( -- )  false >r
    BEGIN
	script? IF  next-arg  ELSE  parse-name  THEN
	dup 0<> WHILE
	    required
	    r@ IF  /flip  THEN  dup >slides
	    rdrop true >r
    REPEAT  2drop rdrop ;

$EEEE3344 text-color: redish
light-gui

: tutorials ( "name1" .. "namen" -- )
    [ sourcefilename extractpath ] SLiteral fpath also-path

    {{
	{{
	    glue-left @ }}glue
	    include-tutorials
	    glue-right @ }}glue
	}}h box[]
	[ true ] [IF]
	    {{
		\Large
		{{
		    glue*l }}glue
		    l" 〈" redish x-color blackish }}button
		    glue*l }}glue
		}}v
		glue*ll }}glue
		{{
		    also [IFDEF] android android [THEN]
		    [: -1 level# +! ;] over click[]
		    previous
		    glue*l }}glue
		    l" 〉" redish x-color blackish }}button
		    glue*l }}glue
		}}v
	    }}h
	    {{
		{{
		    glue*ll }}glue
		    l" ❌️" redish x-color blackish }}button
		}}h
		glue*ll }}glue
	    }}v
	    \normal
	[THEN]
    }}z box[] slide[] to top-widget

    fpath $@ 0 -scan fpath $!
    
    presentation ;

script? [IF]  tutorials bye  [THEN]
