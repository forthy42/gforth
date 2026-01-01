\ (back-)tick recognizer
\ `foo puts the xt of foo on the stack like ' foo does

\ Authors: Gerald Wodni, Anton Ertl
\ Copyright (C) 2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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

: rec-filter {: c-addr u xt: filter xt: rec -- translation :} \ gforth-experimental
    \G Execute @i{rec @code{( @i{c-addr u} -- @i{translation1} )}};
    \G @i{translation1} is then examined with @i{filter @code{(
    \G @i{translation1} -- @i{translation1 f} )}}.  If @i{f} is
    \G non-zero, @i{translation} is @i{translation1}, otherwise
    \G @i{translation} is @i{translate-none}.
    sp@ fp@ 2>r
    c-addr u rec filter 0= if
        2r@ fp! sp! translate-none then
    2rdrop ;

: rec-forth-nt? ( c-addr u -- nt | 0 ) \ gforth-experimental
    \G If @word{rec-forth} produces a result @i{nt
    \G @code{translate-name}}, return @i{nt}, otherwise 0.
    [: dup translate-name = ;] ['] rec-forth rec-filter 0= if
        0
    then ;

: rec-tick ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers}) @code{`@i{word}}.  If
    \G successful, @i{translation} represents pushing the execution
    \G token of @i{word} at run-time (see @word{translate-cell}).
    \G Example: @code{`dup} gives the xt of dup.
    2dup s" `" string-prefix? if
        1 /string rec-forth-nt? dup if
            ?compile-only name>interpret translate-cell exit
        then
        drop translate-none exit
    then
    rec-none ;

: rec-dtick ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers}) @code{``@i{word}}.
    \G If successful, @i{translation} represents pushing the name
    \G token of @i{word} at run-time (see @word{translate-cell}).
    \G Example: @code{``S"} gives the nt of @code{S"}.
    2dup "``" string-prefix? if
        2 /string rec-forth-nt? dup if
            translate-cell exit
        then
        drop translate-none exit
    then
    rec-none ;

' rec-dtick action-of rec-forth >back
' rec-tick action-of rec-forth >back
