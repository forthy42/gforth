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
: redraw-status ( addr u -- )
    .\" \e7"
    0 rows 2 - at-xy   cols 2* spaces .\" \e[A\n"
    status-attr attr! type default-color attr!
    .\" \e8" ;
: .unstatus-line ( -- )
    .\" \e7"
    0 rows 1 - at-xy   cols spaces
    .\" \e8" ;
: .status-line ( -- ) { | w^ status$ }
    base @
    [:  ." gforth 😷 | " unused 1024 / 0 u.r
	." k free | order: " order
	." | base=" .
	." | " depth 0= IF ." ∅" ELSE  ...  THEN ;]
    [:  ." gforth 😷 | " unused 1024 / 0 u.r
	." k free|o " order
	." |b=" 0 .r
	." | " depth 0= IF ." ∅" ELSE  ...  THEN ;]
    cols 100 > select
    #10 ['] base-execute status$ $exec
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
    .\" \n\n\e[2A" status$ $@ redraw-status
    status$ $free ;

: +status ['] .status-line is .status ['] .unstatus-line is .unstatus ;
: -status ['] noop is .status ['] noop is .unstatus ;

+status
