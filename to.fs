\ user friendly interface to generate to-actions

\ Authors: Bernd Paysan
\ Copyright (C) 2023,2024,2025 Free Software Foundation, Inc.

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

: to-class: ( xt table "name" -- ) \ gforth-experimental to-class-colon
    \G Create a to-class implementation @i{name}, where @var{xt}
    \G @code{( ... xt -- addr )} computes the address to access the
    \G data, and @var{table} (created with @code{to-table:}) contains
    \G the words for accessing it.
    ['] value-to Create-from reveal 2, ;

: to-table: ( "name" "to-word" "+to-word" "addr-word" "action-of-word" "is-word" -- ) \ gforth-experimental to-table-colon
    \G Create a table @i{name} with entries for @code{TO}, @code{+TO},
    \G @code{ACTION-OF}, @code{IS}, and @code{ADDR}.  The words for
    \G these entries are called with @i{xt} on the stack, where xt
    \G belongs to the word behind @code{to} (or @code{+to} etc.).  Use
    \G @code{n/a} to mark unsupported operations.  Default entries
    \G operations can be left away at the end of the line; the default
    \G is for the @code{addr} entry is @code{[noop]} while the default
    \G for the other entries is @code{n/a}.
    Create 0 BEGIN parse-name dup WHILE
	    rec-forth '-error , 1+
    REPEAT 2drop
    \ here goes the number of methods supported
    to-table-size# swap U+DO
        ['] n/a ['] [noop] I' I 1+ <> select ,
    LOOP ;

\ new interpret/compile:, we need it already here

: interpret/compile: ( int-xt comp-xt "name" -- ) \ gforth interpret-slash-compile-colon
    \G Defines @i{name}.@* @i{Name} execution: execute @i{int-xt}.@*
    \G @i{Name} compilation: execute @i{comp-xt}.
    swap alias ,
    ['] i/c>comp set->comp
    ['] n/a set-to ;

\ Create TO variants by number

: to: ( u "name" -- ) \ gforth-internal to-colon
    \G Create a new TO variant with the table position number @var{u}
    >r r@ to-table-size# u>= abort" Too many TO operators"
    :noname postpone record-name r@ postpone Literal
    postpone (') ['] (to) :, postpone ;
    :noname postpone record-name r> postpone Literal
    postpone (') postpone (to), postpone ;
    interpret/compile: ;

: to-access: ( n "name" -- ) \ gforth-internal to-access-colon
    ['] defer@ create-from , reveal ;
