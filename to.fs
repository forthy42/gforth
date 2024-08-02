\ user friendly interface to generate to-actions

\ Authors: Bernd Paysan
\ Copyright (C) 2023 Free Software Foundation, Inc.

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

: to-method: ( xt table "name" -- ) \ gforth-experimental to-method-colon
    \G create a to-method @i{name}, where @var{xt} computes the
    \G address to access the field, and @var{table} contains the
    \G operators to store to it.
    ['] value-to Create-from reveal 2, ;

: to-table: ( "name" "to-word" "+to-word" "addr-word" "action-of-word" "is-word" -- ) \ gforth-experimental to-table-colon
    \G create a table @i{name} with entries for @code{TO}, @code{+TO},
    \G @code{ADDR}, @code{ACTION-OF}, and @code{IS}.  The words for
    \G these entries are called with @i{xt} on the stack, where xt
    \G belongs to the word behind @code{to} (or @code{+to} etc.).  Use
    \G @code{n/a} to mark unsupported operations.  Unsupported
    \G operations can be left away at the end of the line.
    Create 0 BEGIN parse-name dup WHILE
	    forth-recognize '-error , 1+
    REPEAT 2drop
    \ here goes the number of methods supported
    to-table-size# swap U+DO ['] n/a , LOOP ;

: >to+addr-table: ( table-addr "name" -- ) \ gforth-experimental to-to-plus-addr-table-colon
    \G @i{name} is a copy of the table at @i{table-addr}, but in
    \G @i{name} the @code{ADDR}-method is supported
    create here to-table-size# cells move
    ['] [noop] here 2 cells + !  to-table-size# cells allot ;

\ new interpret/compile:, we need it already here

: interpret/compile: ( interp-xt comp-xt "name" -- ) \ gforth
    swap alias ,
    ['] i/c>comp set->comp
    ['] no-to set-to ;

\ Create TO variants by number

: to: ( u "name" -- ) \ gforth-internal to-colon
    \G create a new TO variant with the table position number @var{u}
    >r r@ to-table-size# u>= abort" Too many TO operators"
    :noname postpone record-name r@ postpone Literal
    postpone (') ['] (to) :, postpone ;
    :noname postpone record-name r> postpone Literal
    postpone (') postpone (to), postpone ;
    interpret/compile: ;
