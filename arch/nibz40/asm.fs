\ FORTH Assembler for nibz40
\ Copyright (C) 2006,2007,2008,2012 Free Software Foundation, Inc.

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
\ Autor:          Simon Jackson, BEng.
\ Information:
\ - Simple Assembler

\ only forth definitions

require asm/basic.fs

 also ASSEMBLER definitions

require asm/target.fs

 HERE                   ( Begin )

\ primary opcode constant writers

: BA 0 , ;
: FI 1 , ;
: RI 2 , ;
: SI 3 , ;

: GO 4 , ;
: DI 5 , ;
: BO 6 , ;
: SU 7 , ;

: RO 8 , ;
: FA 9 , ;
: RA 10 , ;
: SA 11 , ;

: SO 12 , ;
: FE 13 , ;
: RE 14 , ;
: SE 15 , ;

 HERE  SWAP -
 CR .( Length of Assembler: ) . .( Bytes ) CR
