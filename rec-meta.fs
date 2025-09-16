\ meta-recognizer for disambiguation

\ Author: Bernd Paysan
\ Copyright (C) 2017,2019,2021,2024 Free Software Foundation, Inc.

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

: rec-meta ( addr u -- xt translate-to | 0 ) \ gforth
    \G Recognizes (@pxref{Defining recognizers})
    \G @code{@i{myrec}?@i{mystring}}.  Produces the result of passing
    \G @i{mystring} to @code{rec-@i{myrec}}.@*
    \G Example: @code{hex num?cafe} will be recognized as number even
    \G if a word @word{cafe} is in the search order.@*
    \G Example: @code{float?123.} will be recognized as float.@*

    '?' $split dup 0= IF  2drop
    ELSE
	2swap [: ." rec-" type ;] $tmp find-name ?dup-IF
	    name?int execute  EXIT
	THEN
    THEN
    rec-none ;

' rec-meta action-of rec-forth >back
