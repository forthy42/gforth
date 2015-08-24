\ simple tools to convert UTF-8 into UTF-16 and back

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

: .utf16 ( xchar -- )  0 { w^ ucs16 }
    dup $10000 u>= IF
	$10000 - >r r@ 10 rshift $3FF and $D800 +
	ucs16 w! ucs16 2 type \ high surrogate
	r> $3FF and $DC00 + \ low surrogate
    THEN
    ucs16 w! ucs16 2 type ;
: typeu16 ( addr u -- )
    \ type UTF-8 string as UTF-16 string, byte order: host, no BOM
    bounds ?DO
	I xc@ .utf16
    I I' over - x-size +LOOP ;
: >utf16 ( addr1 u1 -- addr2 u2 )
    \g convert UTF-8 string to UTF-16
    [: typeu16 0 .utf16 ;] $tmp 2 - ;

: typeu8 ( addr u -- )
    \g print UTF-16 string as UTF-8, byte order: host, BOM ignored
    bounds ?DO
	I uw@ dup $D800 $DC00 within IF
	    $3FF and 10 lshift I 2 + uw@
	    $3FF and or $10000 + xemit 4 \ no check for sanity
	ELSE  xemit 2  THEN
    +LOOP ;
: >utf8 ( addr1 u1 -- addr2 u2 )
    \g convert UTF-16 string to UTF-8
    [: typeu8 0 emit ;] $tmp 1- ;