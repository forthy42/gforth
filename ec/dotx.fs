\ dotx.fs a always (simple) hexadecimal .s

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



\ this is much simpler than the things needed for . and .s
\ so if you are debugging primitives and you don't get .s
\ to work use this version.

[IFUNDEF] 8>>
: 8>> 8 rshift ;
[THEN]

: .digit
  $0f and
   dup 9 u>
   IF   
        [ char A char 9 - 1- ] Literal +
   THEN 
  [char] 0 + (emit) ;

: .w
	dup 8>> 2/ 2/ 2/ 2/ .digit
	dup 8>> .digit
	dup 2/ 2/ 2/ 2/ .digit
	.digit ;

: .x 	
	dup 8>> 8>> .w .w $20 (emit) ;

: .sx
  depth
  dup [char] < emit .x [char] > emit dup
  0 ?DO dup pick .x 1- LOOP drop ;
