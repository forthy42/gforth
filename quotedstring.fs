\ Quoted string recognizer

\ Copyright (C) 2012,2013,2014,2015 Free Software Foundation, Inc.

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

' r:fail >code-address ' bl >code-address <> [IF]
    ' r:fail Constant r:fail
[THEN]

: slit,  postpone sliteral ;

' noop ' slit, dup recognizer: r:string

: rec:string ( addr u -- addr u' r:string | r:fail )
    2dup s\" \"" string-prefix?
    IF    drop source drop - 1+ >in !  \"-parse save-mem r:string
    ELSE  2drop r:fail  THEN ;

' rec:string get-recognizers 1+ set-recognizers

0 [IF] \ dot-quoted strings, we don't need them
: .slit slit, postpone type ;
' type ' .slit ' slit, recognizer: r:."

: rec:."  ( addr u -- addr u' r:." | addr u r:fail )
    2dup ".\"" string-prefix?
    IF    drop source drop - 2 + >in !  \"-parse save-mem r:."
    ELSE  r:fail  THEN ;

' rec:." get-recognizers 1+ set-recognizers
[THEN]