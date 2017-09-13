\ -> (to/is replacement) recognizer

\ Copyright (C) 2012,2013,2014,2015,2016 Free Software Foundation, Inc.

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

' (int-to) ' (comp-to) ' lit, rectype: rectype-to

: rec-to ( addr u -- xt r:to | rectype-null )
    \G words prefixed with @code{->} are treated as if preceeded by
    \G @code{TO} or @code{IS}
    dup 3 u< IF  2drop rectype-null  EXIT  THEN
    over 1+ c@ '>' <> IF  2drop rectype-null  EXIT  THEN
    case  over c@
	'-' of   0 to-style# !  endof
	'+' of   1 to-style# !  endof
	''' of  -1 to-style# !  endof
	drop 2drop rectype-null  EXIT
    endcase
    2 /string forth-recognizer recognize
    dup rectype-null = IF  to-style# off  EXIT  THEN
    name?int rectype-to ;

' rec-to forth-recognizer >back
