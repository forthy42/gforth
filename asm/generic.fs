\ generic.fs implements generic assembler definitions		13aug97jaw


\ These are generic routines to build up a table-driven assembler
\ fo any modern (RISC)-CPU

\ This file is copyritghted by JW-Datentechnik GmbH, Munich.
\ You have the right to use it together with GForth EC.
\ This file may copied and redistributed if it is not altered.
\ This is distributed without any warranty.
\ Send comments, suggestions, additions and bugfixes to: wilke@jwdt.com

\ Revision Log:
\
\ 13aug97jaw-14aug97	Initial Version -> V0.5
\			ToDo: operand count checking
\	

\ general definitions

: clearstack depth 0 ?DO drop LOOP ;

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

: reset
  modes modes# cells erase
  1 Mode# !
  depth Start-Depth ! ;

: Mode! ( n -- )
  Modes Mode# @ cells + ! ;

: +Mode! ( n -- )
  Modes Mode# @ cells + tuck @ or swap ! ;

: 0Mode! ( n -- )
  Modes ! ;

: ,
  1 Mode# +! ;

: Mode
  Create dic, DOES> @ +Mode! ;

: 0Mode
  Create dic, DOES> @ 0Mode! ;

: Reg
  Create dic, dic, DOES> dup perform cell+ @ ;

\ --------- Instruction Latch

Create I-Latch 10 chars allot
Variable I-Len

: opc! ( adr len -- )
  dup I-Len @ max I-Len !
  I-Latch -rot bounds DO I c@ over c@ or over c! char+ LOOP drop ;

: I-Init 0 I-Len ! I-Latch 10 erase ;
: I-Flush I-Latch I-len @ bounds DO i c@ X c, LOOP reset ;

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

: g@ ByteDirection @ (g@) ;
: g! ByteDirection @ (g!) ;

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
  dup >modes modes Mode-Compare
  IF 	Table-Exec
	true
  ELSE  false
  THEN ;

: 1st-always ( addr -- flag )
  Table-Exec true ;

: 1st-thru
  dup Table-Exec false ;

: 2nd-opc!
  Describ >data count opc! ;

: opcode,
  here 0 c,
  BEGIN bl word count dup WHILE s>number drop c,
  REPEAT 2drop here over - 1- swap c! ;	

: modes,
  modes# 0 DO I cells modes + @ dic, LOOP ;

0 Value Table-Link

: Table
  Reset 
  Create here to Table-Link 0 dic,
  DOES> I-Init
	BEGIN 	@ dup WHILE dup
		cell+ perform		\ first element is executed always
					\ makes check
		?EXIT
	REPEAT	-1 ABORT" no valid mode!"
  ;

: Follows
  ' >body @ Table-Link @ ! ;

: opc,
  modes, opcode, clearstack reset ;

: (Opc()
\ Opcode with Operands
  ['] 1st-mc dic,
  ['] 2nd-opc! dic,
  dic,
  ['] I-Flush dic,
  opc, ;

: (Opc)
\ Opcode without Operands
  ['] 1st-always dic,
  ['] 2nd-opc! dic,
  ['] Noop dic,
  ['] I-Flush dic,
  opc, ;

: Opc(
\ Opcode with Operands
  Table-Link linked
  (Opc() ;

: Opc
\ Opcode without Operands
  Table-Link linked
  (Opc) ;

: Opc+
\ Additional Opcode
  Table-Link linked
  ['] 1st-thru dic,
  ['] 2nd-opc! dic,
  ['] Noop dic,
  ['] Noop dic, 
  opc, ;

: Opc(+
\ Additional Opcode with Operands
  Table-Link linked
  ['] 1st-thru dic,
  ['] 2nd-opc! dic,
  dic,
  ['] Noop dic, 
  opc, ;

: End-Table ;

: alone
  Create 0 dic, ( Dummy Linkfield ) (opc)
  DOES> dup cell+ perform 0= ABORT" must work always!" ;

: alone(
  Create 0 dic, ( Dummy Linkfield ) (opc()
  DOES> dup cell+ perform 0= ABORT" must work always!" ;
