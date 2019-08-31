\ prelude  higher-level words

\ Copyright (C) 2019 Free Software Foundation, Inc.

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

: prelude ( xt -- )
    \G prelude adds a prelude to the current definition without special
    \G compilation semantics.  a prelude is executed in the interpreter before
    \G the words compilation or interpretation or postpone semantics is used.
    dup [n:d execute ;] set->int
    [n:d execute default-name>comp ;] set->comp ;
