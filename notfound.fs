\ legacy notfound for people who liked the old interface

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2017,2018,2019,2021,2024 Free Software Foundation, Inc.

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

: notfound: ( "name" -- )
    \G special defer word to recover the input lexeme
    Defer ['] no.extensions latestxt defer! ;

notfound: interpret-notfound1 ( addr u -- )
\g Legacy hook for words not found during interpretation
notfound: compiler-notfound1 ( addr u -- )
\g Legacy hook for words not found during compilation
notfound: postpone-notfound1 ( addr u -- )
\g Legacy hook for words not found during postpone

' interpret-notfound1
' compiler-notfound1
' postpone-notfound1
translate: translate-none

: ?notfound ( token -- token  |  0 -- addr u notfound )
    \G if your code relies on @code{notfound} returned instead of 0, you can
    \G use @code{forth-recognize ?notfound}, which will deliver the historical
    \G stack effect
    dup 0= IF  drop input-lexeme 2@ translate-none  THEN ;

' bt-rp0-catch ' interpret [DO] [I] @ ' ?found = [IF] ' ?notfound [I] ! [THEN] cell [+LOOP]
