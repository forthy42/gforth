\ smart .s                                             09mar2012py

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

\ idea: Gerald Wodni

: addr? ( addr -- flag )
    TRY  c@  IFERROR  2drop  false nothrow  ELSE  drop  true  THEN   ENDTRY ;

: string? ( addr u -- flag )
    TRY  dup #80 u> throw  bounds ?DO  I c@ bl < IF  -1 throw  THEN  LOOP
	IFERROR  2drop drop false nothrow ELSE  true  THEN  ENDTRY ;

: .string. ( addr u -- )
    .\" s\" " type '"' emit space ;
: .addr. ( addr -- )  hex. ;

Variable smart.s-skip

: smart.s. ( n -- )
    smart.s-skip @  smart.s-skip off IF  drop  EXIT  THEN
    over r> i swap >r -
    dup 1 = IF  false  ELSE  pick  2dup string?  THEN  IF
	.string. smart.s-skip on
    ELSE  drop dup addr? IF  .addr.
	ELSE  .  THEN
    THEN ;

' smart.s. IS .s.
