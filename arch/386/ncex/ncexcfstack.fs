\  NCEX control flow stack
\
\  Copyright (C) 1998 Lars Krueger 
\
\  This file is part of FLK.
\
\  This is free software; you can redistribute it and/or
\  modify it under the terms of the GNU General Public License
\  as published by the Free Software Foundation, either version 3
\  of the License, or (at your option) any later version.
\
\  This program is distributed in the hope that it will be useful,
\  but WITHOUT ANY WARRANTY; without even the implied warranty of
\  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\  GNU General Public License for more details.
\
\  You should have received a copy of the GNU General Public License
\  along with this program; if not, write to the Free Software
\  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ Structure of one control flow stack item
\ Offset 	meaning
\ 0 		type
\ 1 byte 	type dependent data

\ Possible types of CF stack items
0 CONSTANT CFT-orig
1 CONSTANT CFT-dest
2 CONSTANT CFT-do
3 CONSTANT CFT-colon
4 CONSTANT CFT-case
5 CONSTANT CFT-callback

\ Data format for CFT-orig
\ Offset 	meaning
\ 0 		type
\ 1 byte 	allocator state
\ 3 cells 	addr of jump distance

\ Data format for CFT-dest
\ Offset 	meaning
\ 0 		type
\ 1 byte 	allocator state
\ 3 cells 	addr of jump target

\ Data format for CFT-do
\ Offset 	meaning
\ 0 		type
\ 1 byte 	last allocator state
\ 3 cells 	a1=addr. of inner code
\ 4 cells 	a3=fix-addr for ?DO
\ 5 cells 	last-leave

\ Data format for CFT-case
\ Offset 	meaning
\ 0 		type
\ 3 cells 	count

\ LEAVEs are stored as linked lists starting at last-leave (the global
\ variable)

\ Data format for CFT-colon (host)
\ Offset 	meaning
\ 0 		type
\ 3 cells 	header address
\ 4 cells 	primitive?

\ Data format for CFT-colon (target)
\ Offset 	meaning
\ 0 		type
\ 3 cells 	header address

\ Data format for CFT-callback
\ Offset 	meaning
\ 0 		type
\ 3 cells 	header address
\ 4 cells 	return type

\ Total size of one CF stack item: 6 CELLS

6 CELLS constant CF-SIZE
20 constant CF-ITEMS

\ Create the stack itself.
CREATE cf-stack CF-SIZE CF-ITEMS * ALLOT

\ The stack pointer
VARIABLE cf-sp

\ Find the n-th top of control flow stack item.
: (#-top-cf-item) 			( n -- addr )
( OK )
  cf-sp @ SWAP - 			\ offs
  DUP 0< IF -22 THROW THEN 		\ offs
  CF-SIZE * cf-stack + 			\ addr
;

: .cf-type 				( type -- )    
    CASE
      CFT-orig OF ." orig " ENDOF
      CFT-dest OF ." dest " ENDOF
      CFT-do OF ." do-sys " ENDOF
      CFT-colon OF ." colon-sys " ENDOF
      CFT-case OF ." case " ENDOF
      CFT-callback OF ." callback " ENDOF
      DUP ." unknown (" . ." ) "
    ENDCASE ;
    
\ Print the CF-stack.
: .CS 					( -- )
( OK )
  cf-sp @ -1 = IF ." CF stack empty." CR EXIT THEN
  cf-sp @ 1+ 0 DO
    I (#-top-cf-item)  		\ addr 
    C@ .cf-type
    DROP CR
  LOOP
;

\ Return the current cf-item.
: (curr-cf-item) 			( -- addr )
  cf-sp @ CF-SIZE * cf-stack + ;

\ Allocate a new item.
: (new-cs-item) 			( type -- )
  1 cf-sp +!
  cf-sp @ CF-ITEMS = IF .CS -52 THROW THEN
  (curr-cf-item) C! ;

\ Delete the current item.
: (delete-cs-item) 			( -- )
  -1 cf-sp +! ;

\ Check if the current item is of correct type.
: (check-cs-item) 			( type -- )
  (curr-cf-item) C@ 2DUP <> IF 
    .cf-type ." found, " .cf-type ." expected." CR
    .CS -22 THROW 
  ELSE 
    2DROP 
  THEN ;

\ See standard.
: (CS-PICK) 				( n -- )
( OK )
  (#-top-cf-item) 			\ addr
  0 (new-cs-item) 			\ addr 
  (curr-cf-item) 			\ from to
  CF-SIZE MOVE
;

\ See standard.
: (CS-ROLL) 				( u -- )
( OK )
\ cu cu-1 cu-2 ... c0 -- cu-1 cu-2 ... c0 cu 
  DUP (CS-PICK) 			\ u / cu cu-1 ... c0 cu
  DUP (#-top-cf-item) 			\ u addr-u-1(=from)
  OVER 1+ (#-top-cf-item) 		\ u from addr-u(=to)
  ROT 1+ CF-SIZE * MOVE 
  (delete-cs-item)
;

