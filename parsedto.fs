\ -> (to/is replacement) recognizer

\ Copyright (C) 2012 Free Software Foundation, Inc.

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

: r:to (int-to) ;
compile> drop (comp-to) ;
postpone> >r lit, r> post, ;

: to-recognizer ( addr u -- xt r:to | r:fail )
    2dup s" ->" string-prefix?  0= IF  2drop ['] r:fail  EXIT  THEN
    2 /string dup 0= IF  2drop ['] r:fail  EXIT  THEN
    find-name dup 0= IF  drop ['] r:fail  EXIT  THEN
    name>comp drop ['] r:to ;

' to-recognizer
forth-recognizer get-recognizers
1+ forth-recognizer set-recognizers
