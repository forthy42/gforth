\ nesting.fs displays nesting for primitive trace	12jun97jaw

\ Copyright (C) 1998,2000,2003,2007 Free Software Foundation, Inc.

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

Variable nestlevel

: main
  cr
  0 nestlevel !
  BEGIN
	key dup 9 u> WHILE
	dup
	CASE	': OF 	cr nestlevel @ spaces 1 nestlevel +! emit ENDOF
		'; OF	cr -1 nestlevel +! nestlevel @ spaces emit 
			cr nestlevel @ spaces ENDOF
		dup OF	dup 31 u> IF emit THEN ENDOF
	ENDCASE
  REPEAT drop bye ;	
