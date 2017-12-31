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
    >r start-xt set->comp r> ['] execute ]] drop 2literal ; [[ ;

\ silly example:
\ :noname ." compiling" ;
\ : foo ." interpreting" ; set-compsem

: intsem: ( xt "name" -- )
    \G defines a word, which has a special compilation semantics
    \G provided as @var{xt} on the stack
    >r :noname r> ['] execute ]] drop 2Literal ; [[ >r
    : r> set->comp ;

\ silly example:
\ :noname ." compiling" ; special: foo ." interpreting" ;

0 Value xt-compsem
: compsem: ( -- )
    \G adds a non default compilation semantics to the last
    \G definition
    start-xt to xt-compsem
    [: xt-compsem set-compsem ;] colon-sys-xt-offset stick ;

\ silly example
\ : foo ." interpreting" ; compsem: ." compiling" ;