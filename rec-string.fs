\ Quoted string recognizer

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

: slit,  postpone sliteral ;

' noop ' slit, dup rectype: rectype-string

: rec-string ( addr u -- addr u' r:string | rectype-null )
    \G Convert strings enclosed in double quotes into string literals,
    \G escapes are treated as in @code{S\"}.
    2dup s\" \"" string-prefix?
    IF    drop source drop - 1+ >in !  \"-parse save-mem rectype-string
    ELSE  2drop rectype-null  THEN ;

' rec-string forth-recognizer >back

0 [IF] \ dot-quoted strings, we don't need them
: .slit slit, postpone type ;
' type ' .slit ' slit, recognizer rectype-."

: rec-."  ( addr u -- addr u' r:." | addr u rectype-null )
    2dup ".\"" string-prefix?
    IF    drop source drop - 2 + >in !  \"-parse save-mem rectype-."
    ELSE  rectype-null  THEN ;

' rec-." forth-recognizer >back
[THEN]