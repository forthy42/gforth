\ bidi brackets file

\ Authors: Bernd Paysan
\ Copyright (C) 2021,2022,2024 Free Software Foundation, Inc.

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

Vocabulary bracket(
Vocabulary )bracket

: bracket: ( xc-const xt-name wordlist -- )
    get-current >r
    >wordlist set-current
    ['] xemit $tmp nextname Constant
    r> set-current ;

: rec-brackets ( addr u type -- )
    bounds xc@+ { open } xc@+ { close } 2drop
    close open ['] bracket( bracket:
    open close ['] )bracket bracket: ;

' rec-brackets Constant brackets-recognizer

[IFUNDEF] recognize-execute
    : recognize-execute ( xt recognizer -- )
	['] forth-recognize rot wrap-xt ;
[THEN]

s" brackets.db" ' included ' brackets-recognizer recognize-execute
s" quotation.db" ' included ' brackets-recognizer recognize-execute

' bracket( >wordlist ' )bracket >wordlist 2 rec-sequence: brackets

: ?notfound ( nt rectype-nt / 0 -- value )
    0= IF  0  ELSE  execute  THEN ;

: bracket<> ( xchar -- xchar' / 0 )
    ['] xemit $tmp brackets ?notfound ;
: bracket< ( xchar -- xchar' / 0 )
    ['] xemit $tmp [ ' bracket( >wordlist compile, ] ?notfound ;
: bracket> ( xchar -- xchar' / 0 )
    ['] xemit $tmp [ ' )bracket >wordlist compile, ] ?notfound ;
