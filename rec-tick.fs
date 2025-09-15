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

?: forth-recognize-nt? ( c-addr u -- nt | 0 ) \ gforth-experimental
    \G If @word{forth-recognize} produces a result @i{nt
    \G @code{translate-name}}, return @i{nt}, otherwise 0.
    [: translate-name = dup if drop then ;] try-recognize ;

: rec-tick ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Define recognizers with existing translation
    \G tokens}) @code{`@i{word}}.  If successful, @i{translation}
    \G represents pushing the execution token of @i{word} at run-time
    \G (see @word{translate-cell}).  Example: @code{`dup} gives the xt
    \G of dup.
    over c@ '`' = if
        1 /string forth-recognize-nt? dup if
            ?compile-only name>interpret translate-cell then
        exit  then
    rec-none ;

: rec-dtick ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Define recognizers with existing translation
    \G tokens}) @code{``@i{word}}.  If successful, @i{translation}
    \G represents pushing the name token of @i{word} at run-time (see
    \G @word{translate-cell}).
    \G Example: @code{``S"} gives the nt of @code{S"}.
    2dup "``" string-prefix? if
        2 /string forth-recognize-nt? dup if
            translate-cell then
        exit  then
    rec-none ;

' rec-dtick action-of rec-forth >back
' rec-tick action-of rec-forth >back
