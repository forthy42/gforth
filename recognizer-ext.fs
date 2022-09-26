\ Recognizer extensions

\ Authors: Bernd Paysan
\ Copyright (C) 2020,2021 Free Software Foundation, Inc.

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

0 Value translator-offset
#10 cells constant translator-max-offset#
"No more translator slots free" exception constant translator-overflow

: to-translator ( xt rectype translator -- )
    >body @ >body + ! ;
to-opt: ( xt -- ) >body @ lit, ]] >body + ! [[ ;

: translator: ( "name" -- ) \ gforth-experimental
    \G create a new translator, extending the translator table
    translator-offset translator-max-offset# u>=
    translator-overflow and throw
    Create translator-offset ,  cell +to translator-offset
    [: ( rec-type ) @ + >body @ ;] set-does>
    ['] to-translator set-to ;

translator: interpret-translator ( translator -- xt ) \ gforth-experimental
\G obtain interpreter action from translator
translator: compile-translator ( translator -- xt ) \ gforth-experimental
\G obtain compile action from translator
translator: postpone-translator ( translator -- xt ) \ gforth-experimental
\G obtain postpone action from translator
