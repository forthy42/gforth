\ bitmask.fs Generic Bitmask compiler          			13aug97jaw

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

\ This is a tool for building up assemblers.
\ In modern CPU's instrutions there are often some bitfields that
\ sepcify a register, a addressing mode, an immediate value.
\ A value in an instruction word might be represented in one bitfield
\ or several bitfields.
\ If you code it yourself, you have to think about the right shifting
\ operators. E.g. if you want to store a 2-bit value at bit position 3
\ you would code: ( value opcode -- opcode ) swap 3 lshift or
\ If the value is stored at bit-position 2-3 and 5-6 it gets more difficult:
\ ( value opcode -- opcode ) swap dup 3 and 2 lshift rot or swap 3 and 5 lshift or
\ This is no fun! This can be created automatically by: "maskinto %bitfield".
\ This compiles some code like above into the current definition.
\ This code has the same stack-effect then our examples.
\ Additional things compiled: A check whether the value could be represented
\ by the bitfield, the area of the bitfield is cleared in the opcode.

\ Code Compliance:
\
\ This is for 32 bit and 64 bit systems and for GForth only.
\ 

\ Revision Log:
\
\ 13aug97 Jens Wilke	Creation

decimal

: ?bitexceed ( u1 u2 -- u1 )
\G if u1 is greater than u2 the value could not be represented in the bitfield
  over u< ABORT" value exceeds bitfield!" ;

: bitset# ( u -- )
\G returns the number of bits set in a cell
  0 swap 64 0 DO dup 1 and IF swap 1+ swap THEN 1 rshift LOOP drop ;

: max/bits ( u -- u2 )
\G returns the highes number that could be represented by u bits
  1 swap lshift 1- ;

Variable mli	\ masked last i
Variable mst	\ masked state

: (maskinto) ( n -- )
  0 mst !
  0 mli !
  [ -1 bitset# ] literal 0
  DO	mst @
	IF	dup 1 and 0=
		IF I mli @ - ?dup 
		   IF  	postpone dup max/bits mli @ lshift
			postpone literal postpone and postpone rot 
			postpone or postpone swap
		   THEN
		   I mli ! 0 mst !
		THEN
	ELSE	dup 1 and
		IF I mli @ - ?dup
		   IF postpone literal postpone lshift THEN
		   I mli ! 1 mst !
		THEN
	THEN
	1 rshift 
  LOOP drop 
  postpone drop ;

: maskinto ( <mask> )
  name s>number drop
  \ compile: clear maskarea
  dup invert 
  postpone literal postpone and postpone swap
  \ compile: make check
  dup bitset# max/bits
  postpone literal postpone ?bitexceed
  (maskinto) ; immediate

\ : test maskinto %110010 ;
