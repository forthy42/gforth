\ (back-)tick recognizer
\ `foo puts the xt of foo on the stack like ' foo does

\ Authors: Gerald Wodni, Anton Ertl
\ Copyright (C) 2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

?: ?rec-nt ( addr u -- nt true / 0 )
    [: ['] translate-nt = ;] try-recognize ;

: rec-tick ( addr u -- xt translate-num | 0 ) \ gforth-experimental
    \G words prefixed with @code{`} return their xt.
    \G Example: @code{`dup} gives the xt of dup
    over c@ '`' = if
        1 /string ?rec-nt if
            ?compile-only name>interpret ['] translate-num exit  then
        0 exit  then
    2drop 0 ;

: rec-dtick ( addr u -- nt translate-num | 0 ) \ gforth-experimental
    \G words prefixed with @code{``} return their nt.
    \G Example: @code{``S"} gives the nt of @code{S"}
    2dup "``" string-prefix? if
	2 /string ?rec-nt if  ['] translate-num exit then
        0 exit  then
    2drop 0 ;

' rec-dtick action-of forth-recognize >back
' rec-tick action-of forth-recognize >back
