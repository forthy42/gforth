\ set-compsem, dual-xt version to set compilation semantics

\ Copyright (C) 2015,2017 Free Software Foundation, Inc.

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

: set-compsem ( xt -- )
    \G change compilation semantics of the last defined word
    [n:d nip ['] execute ;] set->comp ;

\ silly example:
\ :noname ." compiling" ;
\ : foo ." interpreting" ; set-compsem

: intsem: ( -- )
    \G changes the current semantics to be the non-default compilation
    \G semantics, and adds another interpretation semantics to the last
    \G definition
    [: ['] execute ;] set->comp
    int-[: [: nip >r vt, wrap! r> [n:d nip ;] set->int ;]
    colon-sys-xt-offset stick ;

\ silly example:
\ : foo ." compiling" ; intsem: ." interpreting" ;

: compsem: ( -- )
    \G adds a non default compilation semantics to the last
    \G definition
    int-[: [: nip >r vt, wrap! r> set-compsem ;] colon-sys-xt-offset stick ;

\ silly example
\ : foo ." interpreting" ; compsem: ." compiling" ;
