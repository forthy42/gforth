\ disasm.fs	disassembler file (for MIPS R3000)
\
\ Copyright (C) 2000,2007 Free Software Foundation, Inc.

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

\ this disassembler is based on data from the R4400 manual
\ http://www.mips.com/Documentation/R4400_Uman_book_Ed2.pdf, in
\ particular pages A3, A181, A182 (p. 471, 649, 650 in xpdf).
\ it is limited to the R3000 (MIPS-I) architecture, though.

\ test this with
\ gforth arch/mips/disasm.fs -e "here" arch/mips/testdisasm.fs -e "here over - disasm bye" |sed 's/([^)]*) //'|diff -u - arch/mips/testasm.fs

get-current
vocabulary disassembler
also disassembler definitions

\ instruction fields

: disasm-op ( w -- u )
    26 rshift ;

: disasm-rs ( w -- u )
    21 rshift $1F and ;

: disasm-rt ( w -- u )
    16 rshift $1f and ;

: disasm-rd ( w -- u )
    11 rshift $1f and ;

: disasm-shamt ( w -- u )
    \ shift amount field
    6 rshift $1f and ;

: disasm-funct ( w -- u )
    $3f and ;

: disasm-copz ( w -- u )
    disasm-op 3 and ;

: disasm-uimm ( w -- u )
    $ffff and ;

: disasm-imm ( w -- n )
    disasm-uimm dup 15 rshift negate 15 lshift or ;

: disasm-relative ( addr n -- w )
    \ compute printable form of relative address n relative to addr
    2 lshift nip ( + ) ;

\ decode tables

: disasm-illegal ( addr w -- )
    \ disassemble illegal/unknown instruction w at addr
    hex. ." , ( illegal inst ) " drop ;

: disasm-table ( n "name" -- )
    \ initialize table with n entries with disasm-illegal
    create 0 ?do
	['] disasm-illegal ,
    loop
does> ( u -- addr )
    swap cells + ;

$40 disasm-table opc-tab-entry     \ top-level decode table
$40 disasm-table funct-tab-entry   \ special function table
$20 disasm-table regimm-tab-entry  \ regim instructions rt table
$20 disasm-table copz-rs-tab-entry \ COPz instructions rs table
$20 disasm-table copz-rt-tab-entry \ COPz BC instructions rt table
$40 disasm-table cp0-tab-entry     \ COP0 CO instructions funct table

\ disassembler central decode cascade

dup set-current

: disasm-inst ( addr w -- )
    \G disassemble instruction w at addr (addr is used for computing
    \G branch targets)
    dup disasm-op opc-tab-entry @ execute ;

: disasm ( addr u -- ) \ gforth
    \G disassemble u aus starting at addr
    bounds u+do
	cr ." ( " i hex. ." ) " i i @ disasm-inst
	1 cells +loop
    cr ;

' disasm IS discode

definitions

: disasm-special ( addr w -- )
    \ disassemble inst with opcode special
    dup disasm-funct funct-tab-entry @ execute ;
' disasm-special 0 opc-tab-entry ! \ enter it for opcode special

: disasm-regimm ( addr w -- )
    \ disassemble regimm inst
    dup disasm-rt regimm-tab-entry @ execute ;
' disasm-regimm 1 opc-tab-entry ! \ enter it for opcode regimm

: disasm-copz-rs ( addr w -- )
    \ disassemble inst with opcode COPz
    dup disasm-rs copz-rs-tab-entry @ execute ;
' disasm-copz-rs $10 opc-tab-entry ! \ enter it for opcodes COPz
' disasm-copz-rs $11 opc-tab-entry !
' disasm-copz-rs $12 opc-tab-entry !

: disasm-copz-rt ( addr w -- )
    \ disassemble inst with opcode COPz, rs=BC
    dup disasm-rt copz-rt-tab-entry @ execute ;
' disasm-copz-rt $08 copz-rs-tab-entry ! \ into COPz-table for rs=BC

: disasm-cp0 ( addr w -- )
    \ disassemble inst with opcode COPz, rs=CO
    dup disasm-funct cp0-tab-entry @ execute ;
' disasm-cp0 $10 copz-rs-tab-entry ! \ into COPz-table for rs=CO

\ dummy words for insts.fs (words with these names are needed by asm.fs)

: asm-op ( -- ) ;
: asm-rs ( -- ) ;
: asm-rt ( -- ) ;

\ disassemble various formats

: disasm-J-target ( addr w -- )
    \ print jump target
    2 lshift $0fffffff and swap $f0000000 and or hex. ;

: disasm-I-rs,rt,imm ( addr w -- )
    dup disasm-rs .
    dup disasm-rt .
    disasm-imm disasm-relative . ;

: disasm-I-rs,imm ( addr w -- )
    dup disasm-rs .
    disasm-imm disasm-relative . ;

: disasm-rt,rs,imm ( addr w -- )
    dup disasm-rt .
    dup disasm-rs .
    disasm-imm .
    drop ;

: disasm-rt,rs,uimm ( addr w -- )
    dup disasm-rt .
    dup disasm-rs .
    disasm-uimm hex.
    drop ;

: disasm-rt,uimm ( addr w -- )
    dup disasm-rt .
    disasm-uimm hex.
    drop ;

: disasm-rt,imm,rs ( addr w -- )
    dup disasm-rt .
    dup disasm-imm .
    dup disasm-rs .
    2drop ;

: disasm-rd,rt,sa ( addr w -- )
    dup disasm-rd .
    dup disasm-rt .
    dup disasm-shamt .
    2drop ;

: disasm-rd,rt,rs ( addr w -- )
    dup disasm-rd .
    dup disasm-rt .
    dup disasm-rs .
    2drop ;

: disasm-rs. ( addr w -- )
    dup disasm-rs .
    2drop ;

: disasm-rd,rs ( addr w -- )
    dup disasm-rd .
    dup disasm-rs .
    2drop ;

: disasm-rd. ( addr w -- )
    dup disasm-rd .
    2drop ;

: disasm-rs,rt ( addr w -- )
    dup disasm-rs .
    dup disasm-rt .
    2drop ;

: disasm-rd,rs,rt ( addr w -- )
    dup disasm-rd .
    dup disasm-rs .
    dup disasm-rt .
    2drop ;

: disasm-rt,rd,z ( addr w -- )
    dup disasm-rt .
    dup disasm-rd .
    dup disasm-copz .
    2drop ;

: disasm-I-imm,z ( addr w -- )
    tuck disasm-imm disasm-relative .
    disasm-copz . ;

\ meta-defining word for instruction format disassembling definitions

\ The following word defines instruction-format words, which in turn
\ define anonymous words for disassembling specific instructions and
\ put them in the appropriate decode table.

: define-format ( disasm-xt table-xt -- )
    \ define an instruction format that uses disasm-xt for
    \ disassembling and enters the defined instructions into table
    \ table-xt
    create 2,
does> ( u "inst" -- )
    \ defines an anonymous word for disassembling instruction inst,
    \ and enters it as u-th entry into table-xt
    2@ swap here name string, ( u table-xt disasm-xt c-addr ) \ remember string
    noname create 2,      \ define anonymous word
    execute lastxt swap ! \ enter xt of defined word into table-xt
does> ( addr w -- )
    \ disassemble instruction w at addr
    2@ >r ( addr w disasm-xt R: c-addr )
    execute ( R: c-addr ) \ disassemble operands
    r> count type ; \ print name 

\ all the following words have the stack effect ( u "name" )
' disasm-J-target    ' opc-tab-entry 	 define-format asm-J-target
' disasm-I-rs,rt,imm ' opc-tab-entry 	 define-format asm-I-rs,rt,imm
' disasm-I-rs,imm    ' opc-tab-entry 	 define-format asm-I-rs,imm1
' disasm-rt,rs,imm   ' opc-tab-entry 	 define-format asm-I-rt,rs,imm
' disasm-rt,rs,uimm   ' opc-tab-entry 	 define-format asm-I-rt,rs,uimm
' disasm-rt,uimm      ' opc-tab-entry 	 define-format asm-I-rt,uimm
' disasm-rt,imm,rs   ' opc-tab-entry 	 define-format asm-I-rt,offset,rs
' disasm-rd,rt,sa    ' funct-tab-entry 	 define-format asm-special-rd,rt,sa
' disasm-rd,rt,rs    ' funct-tab-entry 	 define-format asm-special-rd,rt,rs
' disasm-rs.         ' funct-tab-entry 	 define-format asm-special-rs
' disasm-rd,rs       ' funct-tab-entry 	 define-format asm-special-rd,rs
' 2drop              ' funct-tab-entry 	 define-format asm-special-nothing
' disasm-rd.         ' funct-tab-entry 	 define-format asm-special-rd
' disasm-rs,rt       ' funct-tab-entry 	 define-format asm-special-rs,rt
' disasm-rd,rs,rt    ' funct-tab-entry 	 define-format asm-special-rd,rs,rt
' disasm-I-rs,imm    ' regimm-tab-entry  define-format asm-regimm-rs,imm
' 2drop              ' cp0-tab-entry     define-format asm-copz0
' disasm-rt,rd,z     ' copz-rs-tab-entry define-format asm-copz-rt,rd1
' disasm-I-imm,z     ' copz-rt-tab-entry define-format asm-copz-imm1

: asm-I-rs,imm ( u1 u2 "name" -- ; compiled code: addr w -- )
    nip asm-I-rs,imm1 ;

: asm-copz-rt,rd ( u1 u2 "name" -- )
    drop asm-copz-rt,rd1 ;

: asm-copz-rt,offset,rs ( u "name" -- )
    \ ignore these insts, we disassemble using  asm-I-rt,offset,rs
    drop name 2drop ;

: asm-copz-imm ( u1 u2 u3 "name" -- )
    drop nip asm-copz-imm1 ;

include ./insts.fs

previous set-current
