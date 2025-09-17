\ -> (to/is replacement) recognizer

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

' (to) ' (to), ' 2lit, >postponer
translate: translate-to ( n xt -- translation ) \ gforth-experimental
\G @i{xt} belongs to a value-flavoured (or defer-flavoured) word,
\G @i{n} is the index into the @word{to-table:} for @i{xt}
\G (@pxref{Words with user-defined TO etc.}).@*
\G Interpreting run-time: @code{( @i{... -- ...} )}@*
\G Perform the to-action with index @i{n} in the @word{to-table:} of
\G @i{xt}.  Additional stack effects depend on @i{n} and @i{xt}.

Create to-slots here $100 dup allot $FF fill
0 "-+@='" bounds [DO] dup to-slots [I] c@ + c! 1+ [LOOP] drop

: rec-to ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers})
    \G @code{->@i{v}} (@code{TO @i{v}}), @code{+>@i{v}}
    \G (@code{+TO @i{v}}), @code{'>@i{v}} (@code{ADDR @i{v}}),
    \G @code{@@>@i{d}} (@code{ACTION-OF @i{d}}) and @code{=>@i{d}}
    \G (@code{IS @i{d}}), where @i{v} is a value-flavoured word and
    \G @i{d} is a defer-flavoured word.  If successful,
    \G @i{translation} represents performing the operation on
    \G @i{v}/@i{d} at run-time.
    dup 3 u< IF rec-none EXIT THEN
    over 1+ c@ '>' <> IF rec-none EXIT THEN
    over c@ to-slots + c@ dup $FF = IF drop rec-none EXIT THEN
    -rot 2 /string sp@ 3 cells + fp@ 2>r rec-forth
    translate-name? 0= IF 2r> fp! sp! translate-none EXIT THEN 2rdrop
    \ dup >namehm @ >hmto @ ['] n/a = IF rec-none EXIT THEN
    over 4 = IF ?addr THEN
    name>interpret translate-to ;

' rec-to action-of rec-forth >back
