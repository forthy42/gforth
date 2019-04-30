\ smart .s                                             09mar2012py

\ Copyright (C) 2012,2018 Free Software Foundation, Inc.

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
: .var? ( addr -- flag )
    TRY  body> @ dovar: <> throw  IFERROR  2drop false nothrow
	ELSE  true  THEN   ENDTRY ;

: string? ( addr u -- flag )
    TRY  dup #80 #1 within throw  bounds ?DO  I c@ bl < IF  -1 throw  THEN  LOOP
	IFERROR  2drop drop false nothrow ELSE  true  THEN  ENDTRY ;

: .string. ( addr u -- )
    '"' emit type '"' emit space ;
: .addr. ( addr -- )
    dup xt? if
	dup name>string dup if
	    ." `" type space drop exit
	else
	    2drop
	then
    then
    dup in-dictionary? if
	forthstart over [ 1 maxaligned negate ]L and U-DO
	    I body> xt? if
		I body> name>string dup if
		    '<' emit type I - ?dup-if
			." +$" 0 ['] u.r $10 base-execute  then
		    '>' emit space unloop  EXIT
		else  2drop  then
	    then
	[ 1 maxaligned ]L -LOOP
    then
    hex. ;

: .var. ( addr -- )
    dup body> >name dup IF  .name drop  ELSE  drop hex.  THEN ;

Variable smart.s-skip

: smart.s. ( n -- )
    smart.s-skip @  smart.s-skip off IF  drop  EXIT  THEN
    over r> i swap >r - \ we access the .s loop counter
    dup 1 = IF  false  ELSE  pick  2dup string?  THEN  IF
	.string. smart.s-skip on
    ELSE  drop dup addr? IF  dup .var? IF  .var.  ELSE  .addr.  THEN
	ELSE  .  THEN
    THEN ;

' smart.s. is .s.

: ... ( x1 .. xn -- x1 .. xn )
    action-of .s. >r
    ['] smart.s. IS .s. .s
    r> IS .s. ;


