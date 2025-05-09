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

' (to) ' (to), ' 2lit, >postponer translate: translate-to

Create to-slots here $100 dup allot $FF fill
0 "-+@='" bounds [DO] dup to-slots [I] c@ + c! 1+ [LOOP] drop

: rec-to ( addr u -- xt n translate-to | 0 ) \ gforth-experimental
    \G words prefixed with @code{->} are treated as if preceeded by
    \G @code{TO}, with @code{+>} as @code{+TO}, with
    \G @code{'>} as @code{ADDR}, with @code{@@>} as @code{ACTION-OF}, and
    \G with @code{=>} as @code{IS}.
    dup 3 u< IF  2drop 0  EXIT  THEN
    over 1+ c@ '>' <> IF  2drop 0  EXIT  THEN
    over c@ to-slots + c@ dup $FF = IF  drop 2drop 0  EXIT  THEN
    -rot  2 /string sp@ 3 cells + fp@ 2>r forth-recognize
    translate-nt? 0= IF  2r> fp! sp! 0 EXIT  THEN  2rdrop
    \ dup >namehm @ >hmto @ ['] n/a = IF  2drop 0 EXIT  THEN
    over 4 = IF  ?addr  THEN
    name>interpret ['] translate-to ;

' rec-to action-of forth-recognize >back
