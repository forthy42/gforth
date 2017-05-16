\ bit vectors, lsb first

\ Copyright (C) 2012,2014,2015,2016 Free Software Foundation, Inc.

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

: bits ( n -- n ) 1 swap lshift ;

: >bit ( addr n -- c-addr mask ) 8 /mod rot + swap bits ;
: +bit ( addr n -- )  >bit over c@ or swap c! ;
: +bit@ ( addr n -- flag )  >bit over c@ 2dup and >r
    or swap c! r> 0<> ;
: -bit ( addr n -- )  >bit invert over c@ and swap c! ;
: -bit@ ( addr n -- flag )  >bit over c@ 2dup and >r
    invert or invert swap c! r> 0<> ;
: bit! ( flag addr n -- ) rot IF  +bit  ELSE  -bit  THEN ;
: bit@ ( addr n -- flag )  >bit swap c@ and 0<> ;

: bittype ( addr base n -- )  bounds +DO
	dup I bit@ '+' '-' rot select emit  LOOP  drop ;

: bit-erase ( addr off len -- )
    dup 8 u>= IF
	>r dup 7 and >r 3 rshift + r@ bits 1- over andc!
	1+ 8 r> - r> swap -
	dup 7 and >r 3 rshift 2dup erase +
	0 r> THEN
    bounds ?DO  dup I -bit  LOOP  drop ;

: bit-fill ( addr off len -- )
    dup 8 u>= IF
	>r dup 7 and >r 3 rshift + r@ bits 1- invert over orc!
	1+ 8 r> - r> swap -
	dup 7 and >r 3 rshift 2dup $FF fill +
	0 r> THEN
    bounds ?DO  dup I +bit  LOOP  drop ;

