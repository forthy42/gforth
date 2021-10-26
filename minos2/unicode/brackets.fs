\ bidi brackets file

\ Authors: Bernd Paysan
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

Vocabulary brackets

: bracket: ( xc-const xt-name -- )
    get-current >r
    ['] brackets >wordlist set-current
    ['] xemit $tmp nextname Constant
    r> set-current ;

: rec-brackets ( addr u type -- ) drop
    bounds xc@+ { open } xc@+ { close } 2drop
    close open bracket:
    open close bracket: ;

' rec-brackets Constant brackets-recognizer

[IFUNDEF] recognize-execute
    : recognize-execute ( xt recognizer -- )
	action-of forth-recognize >r  is forth-recognize
	catch  r> is forth-recognize  throw ;
[THEN]

s" brackets.db" ' included ' brackets-recognizer recognize-execute

: bracket<> ( xchar -- xchar' / 0 )
    ['] xemit $tmp ['] brackets >wordlist find-name-in
    ?dup-IF  execute  THEN ;
