\ -> (to/is replacement) recognizer

\ Copyright (C) 2012,2013,2014 Free Software Foundation, Inc.

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

' (int-to) ' (comp-to) ' lit, recognizer: r:to

: rec:to ( addr u -- xt r:to | r:fail )
    2dup s" ->" string-prefix?  0= IF  2drop  r:fail  EXIT  THEN
    2 /string dup 0= IF  2drop  r:fail  EXIT  THEN
    do-recognizer dup r:fail = ?EXIT
    name>int/comp r:to ;

' rec:to get-recognizers 1+ set-recognizers
