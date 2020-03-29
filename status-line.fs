\ status line, inspired by seedForth

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

blue >bg white >fg or bold or Value status-attr
: .status-line ( -- ) { | w^ status$ }
    [:  ." gforth 😷 | free: " unused u.
	." | order: " order
	." | base: " base ['] ? #10 base-execute
	." | " depth 0= IF ." ∅" ELSE  ['] ... #10 base-execute  THEN ;]
    [:  ." gforth 😷 |f " unused u.
	." |o " order
	." |b " base ['] ? #10 base-execute
	." | " depth 0= IF ." ∅" ELSE  ['] ... #10 base-execute  THEN ;]
    cols 100 > select
    status$ $exec
    cols status$ $@ x-width - dup 0> IF
	['] spaces status$ $exec
    ELSE  0< IF
	    0 status$ $@ bounds U+DO
		I xc@+ swap >r
		dup #tab = IF  drop 1+ dfaligned  ELSE  xc-width +  THEN
		dup cols u> IF  rdrop I status$ $@ drop - status$ $!len
		    leave  THEN
	    r> I - +LOOP  drop
	THEN
    THEN
    .\" \n\n\e[2A\e7"
    0 rows 2 - at-xy   cols spaces  cr
    status-attr attr! status$ $. default-color attr!
    .\" \e8"
    status$ $free ;

' .status-line is .status
