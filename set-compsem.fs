\ set-compsem, dual-xt version to set compilation semantics

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2017,2019 Free Software Foundation, Inc.

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
    \G The current definition's compilation semantics are changed to
    \G perform its execution semantics (the word becomes immediate).
    \G Then its interpretation semantics are changed to perform the
    \G definition starting at the @code{intsem:}.  Note that if you
    \G then call @code{immediate}, the compilation semantics are
    \G changed to perform the word's new interpretation semantics.
    [: ['] execute ;] set->comp
    int-[: [: nip >r vt, wrap! r> [n:d nip ;] set->int ;]
    colon-sys-xt-offset stick ;

\ silly example:
\ : foo ." compiling" ; intsem: ." interpreting" ;

: compsem: ( -- )
    \G Changes the compilation semantics of the current definition to
    \G perform the definition starting at the @code{compsem:}.
    int-[: [: nip >r vt, wrap! r> set-compsem ;] colon-sys-xt-offset stick ;

\ silly example
\ : foo ." interpreting" ; compsem: ." compiling" ;
