\ (back-)tick recognizer
\ `foo puts the xt of foo on the stack like ' foo does

\ Authors: Gerald Wodni, Anton Ertl
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

: rec-tick ( addr u -- xt rectype-num | rectype-null )
    \G words prefixed with @code{`} return their xt.
    \G Example: @code{`dup} gives the xt of dup
    over c@ '`' = if
	1 /string find-name dup if
	    name>interpret rectype-num exit then
	dup then
    2drop rectype-null ;

' rec-tick forth-recognizer >back

: rec-dtick ( addr u -- nt rectype-num | rectype-null )
    \G words prefixed with @code{``} return their nt.
    \G Example: @code{``S"} gives the nt of @code{S"}
    2dup "``" string-prefix? if
	2 /string find-name dup if
	    rectype-num exit then
	dup then
    2drop rectype-null ;

' rec-dtick forth-recognizer >back

