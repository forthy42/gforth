\ (back-)tick recognizer
\ `foo puts the xt of foo on the stack like ' foo does

\ Authors: Gerald Wodni, Anton Ertl
\ Copyright (C) 2018,2019,2020 Free Software Foundation, Inc.

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

[IFUNDEF] ?rec-nt
    : ?rec-nt ( addr u -- xt true / something 0 )
	sp@ >in @ 2>r
	forth-recognize ['] recognized-nt = dup
	if  2r> 2over  else  2r> #0.  then  2>r >in ! sp!
	2drop 2r> ;
[THEN]

: rec-tick ( addr u -- xt rectype-num | rectype-null )
    \G words prefixed with @code{`} return their xt.
    \G Example: @code{`dup} gives the xt of dup
    over c@ '`' = if
	1 /string ?rec-nt
	if  name>interpret ['] recognized-num exit  then  0
    then
    2drop ['] notfound ;

: rec-dtick ( addr u -- nt rectype-num | rectype-null )
    \G words prefixed with @code{``} return their nt.
    \G Example: @code{``S"} gives the nt of @code{S"}
    2dup "``" string-prefix? if
	2 /string ?rec-nt if  ['] recognized-num exit then  0
	then
    2drop ['] notfound ;

' rec-dtick forth-recognizer >back
' rec-tick forth-recognizer >back
