\ ΜΙΝΩΣ2 Tutorial framework

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

88e update-size#

require minos2/text-style.fs
require minos2/presentation-support.fs

' }}i18n-text is }}text'

l"  " >r
: em-space ( -- o ) [ r> ]L }}text' ;

: include-tutorials ( -- )  false >r
    BEGIN
	next-arg 2dup d0<> WHILE
	    required
	    r@ IF  /flip  THEN  dup >slides
	    rdrop true >r
    REPEAT  2drop rdrop ;

{{
    {{
	glue-left @ }}glue
	include-tutorials
	glue-right @ }}glue
    }}h box[]
}}z box[] slide[] to top-widget

light-gui
presentation
bye
