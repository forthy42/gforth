\ generic.fs implements generic assembler definitions		13aug97jaw

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

\ These are generic routines to build up a table-driven assembler
\ fo any modern (RISC)-CPU

\ Revision Log:
\
\ 13aug97jaw-14aug97	Initial Version -> V0.5
\			ToDo: operand count checking
\
\ 24apr10dk             Added documentation

\ general definitions

: clearstack ( k*x -- ) depth 0 ?DO drop LOOP ;

\ redefinitions to avoid conflicts

' ,    ALIAS dic,
' NOOP ALIAS X

\ ------------ Modes

[IFUNDEF] modes#
4 Constant modes#
[THEN]

Create modes modes# cells allot	\ Modes for differend operands are stored here
				\ Example:
				\ Offset 0: general modifier ( .B, .W, .L)
				\ Offset 1: addressing mode operand 1
				\ Offset 2: addressing mode operand 2

: Mode-Compare ( adr1 adr2 -- flag )
  modes# 
  BEGIN dup WHILE >r 2dup @ swap @ <> IF rdrop 2drop false EXIT THEN 
		cell+ swap cell+ r> 1- 
  REPEAT drop 2drop true ;

Variable Start-Depth
Variable Mode#

: reset  ( -- )
  \G End an opcode / star a new opcode.  
  modes modes# cells erase
  1 Mode# !
  depth Start-Depth ! ;

: Mode! ( x -- )
  \G Set current operand's mode to X
  Modes Mode# @ cells + ! ;

: +Mode! ( x -- )
  \G Logically OR X to current operand's mode 
  Modes Mode# @ cells + tuck @ or swap ! ;

: 0Mode! ( x -- )
  \G Set mode of operand #0.  Use operand #0 for an (optional) operands that
  \G can be placed anywhere, e.g. condition codes or operand lengths .B .W .L
  \G etc.
  Modes ! ;

: ,  ( -- )
  \G Advance to next operand
  1 Mode# +! ;

: Mode  ( x "name" -- )
  \G Define a new mode that logically ors X to current mode when executed
  Create dic, DOES> @ +Mode! ;

: 0Mode  ( x "name" -- )
  \G Define a new mode that sets operand #0 when executed
  Create dic, DOES> @ 0Mode! ;

: Reg  ( xt x "name" -- )
  \G Define a parametrized mode, that executes mode XT, then puts X onto the
  \G stack
  Create dic, dic, DOES> dup perform cell+ @ ;

\ --------- Instruction Latch

Create I-Latch 10 chars allot
Variable I-Len

: opc! ( adr len -- )
  \G Logically OR string of bytes into instruction latch
  dup I-Len @ max I-Len !
  I-Latch -rot bounds ?DO I c@ over c@ or over c! char+ LOOP drop ;

: I-Init  ( -- )  0 I-Len ! I-Latch 10 erase ;
: I-Flush  ( -- )
  \G Append contents of instruction latch to dictionary
  I-Latch I-len @ bounds DO i c@ X c, LOOP reset ;

: (g!) ( val addr n -1/1 -- )
  dup 0< IF rot 2 pick + 1- -rot THEN
  swap >r -rot r> 0 
  DO 2dup c! 2 pick + swap 8 rshift swap LOOP 
  2drop drop ;

: (g@) ( addr n -1/1 -- val )
  negate dup 0< IF rot 2 pick + 1- -rot THEN
  swap >r swap 0 swap r> 0 
  DO swap 8 lshift over c@ or swap 2 pick + LOOP
  drop nip ;

Variable ByteDirection	\ -1 = big endian; 1 = little endian

: g@  ( addr n -- val )
  \G read n-byte integer from addr using current endianess
  ByteDirection @ (g@) ;
: g!  ( val addr n -- )
  \G  write n-byte integer to addr using current endianess
  ByteDirection @ (g!) ;

\ ---------------- Tables

: >modes ( addr -- addr ) 5 cells + ;
: >data  ( addr -- addr ) >modes modes# cells + ;

0 Value Describ

: Table-Exec ( addr -- )
  to Describ
  Describ 2 cells + perform 	\ to store the opcode
  Describ 3 cells + perform	\ to store the operands
  Describ 4 cells + perform 	\ to flush the instruction
  ;

: 1st-mc   ( addr -- flag )
  \G mnemonic check?  check for matching operands.  if matching, execute code
  \G to encode mode and operands and return true, else return false
  dup >modes modes Mode-Compare
  IF 	Table-Exec
	true
  ELSE  false
  THEN ;

: 1st-always ( addr -- flag )
  \G Undconditionally encode operands and/or instruction used for instructions
  \G that do not have any operands.  Return true i.e. make the assembler stop
  \G looking for more instruction variants
  Table-Exec true ;

: 1st-thru
  \G Unconditionally encode, but return false to make assembler execute next
  \G table rows also.
  dup Table-Exec false ;

: 2nd-opc!  ( -- )
  \G encode opcode by ORing data column of current instruction row into
  \G instruction latch
  Describ >data count opc! ;

: opcode,  ( "<NNN> ..." -- )
  \G Append a counted string to dictionary, reading in every character as
  \G space-terminated numbers form the parser until the end of line is
  \G reached.
  here 0 c,
  BEGIN bl word count dup WHILE s>number drop c,
  REPEAT 2drop here over - 1- swap c! ;	

: modes,  ( -- )
  \G append contents of MODES to dictionary
  modes# 0 DO I cells modes + @ dic, LOOP ;

0 Value Table-Link

: Table  ( "name" -- )
  \G create table that lists allowed operand/mode combinations for opcode
  \G "name".  Note that during assembling, table will be scanned in reverse
  \G order!
  Reset 
  Create here to Table-Link 0 dic,
  DOES> I-Init
	BEGIN 	@ dup WHILE dup
		cell+ perform		\ first element is executed always
					\ makes check
		?EXIT
	REPEAT	-1 ABORT" no valid mode!"
  ;
 
: Follows  ( "name" -- )
  \G Link current instruction's table to execute all rows of table "name"
  \G (after executing all rows already defined).  Do not add any more rows to
  \G current table, after executing Follows.  Else you're going to modify
  \G "name"'s table!
  ' >body @ Table-Link @ ! ;

: opc,  ( k*x "<NNN> ..." -- )
  \G Append current modes and opcode given byte-wise on current input line to
  \G dictionary.  Clear forth stack to remove any data provided by the
  \G otherwise unused operands that wer used to set up the modes array.
  modes, opcode, clearstack reset ;

: (Opc()  ( k*x xt "<NNN> ..." -- )
  \G Fill table row for opcode with Operands.  XT will be executed by the
  \G assembler for encoding the operands using data from the stack.
  ['] 1st-mc dic,
  ['] 2nd-opc! dic,
  dic,
  ['] I-Flush dic,
  opc, ;

: (Opc)  ( k*x xt "<NNN> ..." -- )
\ Opcode without Operands
  ['] 1st-always dic,
  ['] 2nd-opc! dic,
  ['] Noop dic,
  ['] I-Flush dic,
  opc, ;

: Opc(  ( k*x xt "<NNN> ..." -- )
  \G Append a new table row for an opcode with Operands.  Use your assembler
  \G operands to fill the MODES array with data showing how the opcode is
  \G used.  Only the types of operands are recorded, any operand parameters
  \G passed on the stack are dropped.  The opcode's instruction code is read
  \G as 8-bit numbers from the current input line and stored as counted string
  \G in the table's opcode column
  \G
  \G When assembling an instruction, the assembler checks for matching
  \G operands.  If this row matches, first XT is called to consume operands
  \G parameters from the stack and encode them into the instruction latch.
  \G Then the opcode column is ORed to the instruction latch and the assembler
  \G quits assembly of the current instruction.
  Table-Link linked
  (Opc() ;

: Opc  ( k*x "<NNN> ..." -- )
  \G Append a new table row for an opcode without operand parameters.
  \G
  \G When assembling an instruction, and the assembler reaches this row, it
  \G will assume the opcode is fully assembled and quits assembly of the
  \G instruction, after endcoding the opcode.
  Table-Link linked
  (Opc) ;

: Opc+  ( k*x "<NNN> ..." -- )
  \G Append a new table row that encodes part of an opcode, but falls through
  \G to following lines.
  Table-Link linked
  ['] 1st-thru dic,
  ['] 2nd-opc! dic,
  ['] Noop dic,
  ['] Noop dic, 
  opc, ;

: Opc(+  ( k*x xt "<NNN> ..." -- )
  \G Like OPC+ but for a table row that has operands
  Table-Link linked
  ['] 1st-thru dic,
  ['] 2nd-opc! dic,
  dic,
  ['] Noop dic, 
  opc, ;

: End-Table ;

: alone  ( k*x "<NNN> ..." -- )
  \G Create a single-row instruction table for an instruction without operands.
  Create 0 dic, ( Dummy Linkfield ) (opc)
  DOES> dup cell+ perform 0= ABORT" must work always!" ;
    
: alone(   ( k*x xt "<NNN> ..." -- )
  \G Create a single-row instruction table for an instruction with operands.
  Create 0 dic, ( Dummy Linkfield ) (opc()
  DOES> dup cell+ perform 0= ABORT" must work always!" ;


\ Configure Emacs forth-mode to keep this file's formatting
0 [IF]
   Local Variables:
   forth-indent-level: 2
   forth-local-indent-words:
   (((";") (0 . -2) (0 . -2))
    (("does>") (0 . 0) (0 . 0)))
   End:
[THEN]
