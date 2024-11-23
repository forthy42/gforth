\ Recognizer extensions

\ Authors: Bernd Paysan
\ Copyright (C) 2020,2021,2022,2023 Free Software Foundation, Inc.

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
s" No more translator slots free" exception constant translator-overflow

: >translate-method ( xt rectype translate-method -- )
    >body @ >body + ;
fold1: ( xt -- ) >body @ lit, postpone >body postpone + ;

' >translate-method defer-table to-class: translate-method-to

' >postpone make-latest
' translate-method-to set-to

: translate-method: ( "name" -- ) \ gforth-experimental
    \G create a new translate method, extending the translator table.
    \G You can assign an xt to an existing rectype by using
    \G @var{xt rectype} @code{to} @var{translator}.
    translator-offset translator-max-offset# u>=
    translator-overflow and throw
    ['] >postpone create-from reveal
    translator-offset ,  cell +to translator-offset ;

translate-method: >interpret ( translator -- ) \ gforth-experimental
\G perform interpreter action of translator
translate-method: >compile ( translator -- ) \ gforth-experimental
\G perform compile action of translator
\ we already have defined this in the kernel
\ translate-method: >postpone ( translator -- ) \ gforth-experimental
\ \G perform postpone action of translator
cell +to translator-offset

: translate-state ( xt -- ) \ gforth-experimental
    \G change the current state of the system so that executing
    \G a translator matches the translate-method passsed as @var{xt}
    dup >does-code [ ' >postpone >does-code ] Literal <> #-12 and throw
    >body @ cell/ negate state ! ;

: translate-state? ( xt -- flag ) \ gforth-experimental
    \G change the current state of the system so that executing
    \G a translator matches the translate-method passsed as @var{xt}
    dup >does-code [ ' >postpone >does-code ] Literal <> #-12 and throw
    >body @ cell/ state @ abs = ;
