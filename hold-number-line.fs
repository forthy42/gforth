\ hold#line

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2025 Free Software Foundation, Inc.

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

[IFUNDEF] holds
    : holds ( addr u -- )
	tuck + swap 0 ?DO  1- dup c@ hold  LOOP  drop ;
[THEN]

: hold#line ( -- c-addr u )
    '"' hold sourcefilename holds '"' hold bl hold
    base @ >r decimal #s r> base !
    s" #line " holds ;
