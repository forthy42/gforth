\ disasm.fs	disassembler file (for MIPS R3000)
\
\ Copyright (C) 1995-97 Martin Anton Ertl, Christian Pirker
\
\ This file is part of RAFTS.
\
\	RAFTS is free software; you can redistribute it and/or
\	modify it under the terms of the GNU General Public License
\	as published by the Free Software Foundation; either version 2
\	of the License, or (at your option) any later version.
\
\	This program is distributed in the hope that it will be useful,
\	but WITHOUT ANY WARRANTY; without even the implied warranty of
\	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\	GNU General Public License for more details.
\
\	You should have received a copy of the GNU General Public License
\	along with this program; if not, write to the Free Software
\	Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

: disasm-illegal ( addr w -- )
    \ disassemble illegal instruction w at addr
    hex. ." , ( illegal inst ) " drop ;

: init-disasm-table ( n -- )
    \ initialize table with n entries with disasm-illegal
    0 ?do
	['] disasm-illegal ,
    loop ;

create opc-table $40 init-disasm-table \ top-level decode table
create funct-table $40 init-disasm-table \ special function table
create regimm-table $20 init-disasm-table \ regim instructions rt field
create copz-rs-table $20 init-disasm-table \ COPz instructions rs field
create copz-rt-table $20 init-disasm-table \ COPz instructions rt field
create cp0-table $40 init-disasm-table \ COP0 function table

\ fields

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

: disasm-imm ( w -- n )
    $ffff and dup 15 rshift negate 15 lshift or ;

: disasm-relative ( addr n -- w )
    \ compute printable form of relative address n relative to addr
    nip ( + ) ;

\ disassembler central decode cascade

: disasm-inst ( addr w -- )
    \G disassemble instruction w at addr (addr is used for computing
    \G branch targets)
    dup disasm-op cells opc-table + @ execute ;

: disasm-dump ( addr u -- ) \ gforth
    \G disassemble u aus starting at addr
    bounds u+do
	cr ." ( " i hex. ." ) " i i @ disasm-inst
	1 cells +loop ;

: disasm-special ( addr w -- )
    \ disassemble inst with opcode special
    dup disasm-funct cells funct-table + @ execute ;
' disasm-special 0 cells opc-table + !

: disasm-regimm ( addr w -- )
    \ disassemble regimm inst
    dup disasm-rt cells regimm-table + @ execute ;
' disasm-regimm 1 cells opc-table + !

: disasm-copz-rs ( addr w -- )
    \ disassemble inst with opcode COPz
    dup disasm-rs cells copz-rs-table + @ execute ;
' disasm-copz-rs $10 cells opc-table + !
' disasm-copz-rs $11 cells opc-table + !
' disasm-copz-rs $12 cells opc-table + !

: disasm-copz-rt ( addr w -- )
    \ disassemble inst with opcode COPz, rs=BC
    dup disasm-rt cells copz-rt-table + @ execute ;
' disasm-copz-rt $08 cells copz-rs-table + !

: disasm-cp0 ( addr w -- )
    \ disassemble inst with opcode COPz, rs=CO
    dup disasm-funct cells cp0-table + @ execute ;
' disasm-cp0 $10 cells copz-rs-table + !

\ disassemble various formats

: asm-op ( -- ) ;

: disasm-J-target ( addr w -- )
    \ print jump target
    $3ffffff and swap $fc000000 and or hex. ;

: asm-J-target ( u "inst" -- ; compiled code: addr w -- )
    \ disassemble jump inst with opcode u
    :noname POSTPONE disasm-J-target
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells opc-table + ! ;

: disasm-I-rs,rt,imm ( addr w -- )
    dup disasm-rs .
    dup disasm-rt .
    disasm-imm disasm-relative . ;

: asm-I-rs,rt,imm ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-I-rs,rt,imm
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells opc-table + ! ;

: asm-rt ( -- ) ;

: disasm-I-rs,imm ( addr w -- )
    \ !! does not check for correctly set rt ( should be 0 )
    dup disasm-rs .
    disasm-imm disasm-relative . ;

: asm-I-rs,imm ( u1 u2 "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-I-rs,imm
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells opc-table + ! ;

: disasm-rt,rs,imm ( addr w -- )
    dup disasm-rt .
    dup disasm-rs .
    disasm-imm .
    drop ;

: asm-I-rt,rs,imm ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rt,rs,imm
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells opc-table + ! ;

: disasm-rt,imm ( addr w -- )
    dup disasm-rt .
    disasm-imm .
    drop ;

: asm-I-rt,imm ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rt,imm
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells opc-table + ! ;

: disasm-rt,imm,rs ( addr w -- )
    dup disasm-rt .
    dup disasm-imm .
    dup disasm-rs .
    2drop ;

: asm-I-rt,offset,rs ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rt,imm,rs
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells opc-table + ! ;

: disasm-rd,rt,sa ( addr w -- )
    dup disasm-rd .
    dup disasm-rt .
    dup disasm-shamt .
    2drop ;

: asm-special-rd,rt,sa ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rd,rt,sa
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: disasm-rd,rt,rs ( addr w -- )
    dup disasm-rd .
    dup disasm-rt .
    dup disasm-rs .
    2drop ;

: asm-special-rd,rt,rs ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rd,rt,rs
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: disasm-rs. ( addr w -- )
    dup disasm-rs .
    2drop ;

: asm-special-rs ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rs.
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: disasm-rd,rs ( addr w -- )
    dup disasm-rd .
    dup disasm-rs .
    2drop ;

: asm-special-rd,rs ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rd,rs
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: asm-special-nothing ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE 2drop
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: disasm-rd. ( addr w -- )
    dup disasm-rd .
    2drop ;

: asm-special-rd ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rd.
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: disasm-rs,rt ( addr w -- )
    dup disasm-rs .
    dup disasm-rt .
    2drop ;

: asm-special-rs,rt ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rs,rt
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: disasm-rd,rs,rt ( addr w -- )
    dup disasm-rd .
    dup disasm-rs .
    dup disasm-rt .
    2drop ;

: asm-special-rd,rs,rt ( u "name" -- ; compiled code: addr w -- )
    :noname POSTPONE disasm-rd,rs,rt
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells funct-table + ! ;

: asm-regimm-rs,imm ( u "name" -- )
    :noname POSTPONE disasm-I-rs,imm
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells regimm-table + ! ;

: asm-copz-rt,offset,rs ( u "name" -- )
    \ ignore these insts, we disassemble using  asm-I-rt,offset,rs
    drop name 2drop ;

: asm-copz0 ( u "name" -- )
    :noname POSTPONE 2drop
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells cp0-table + ! ;

$00 constant asm-copz-MF
$02 constant asm-copz-CF
$04 constant asm-copz-MT
$06 constant asm-copz-CT
$08 constant asm-copz-BC
$10 constant asm-copz-C0

$00 constant asm-copz-BCF
$01 constant asm-copz-BCT

: asm-rs ( -- ) ;

: disasm-rt,rd,z ( addr w -- )
    dup disasm-rt .
    dup disasm-rd .
    dup disasm-copz .
    2drop ;

: asm-copz-rt,rd ( u1 u2 "name" -- )
    drop :noname POSTPONE disasm-rt,rd,z
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells copz-rs-table + ! ;

: disasm-I-imm ( addr w -- )
    disasm-imm disasm-relative . ;

: asm-copz-imm ( u1 u2 u3 "name" -- )
    drop nip :noname POSTPONE disasm-I-imm
    name POSTPONE sliteral POSTPONE type POSTPONE ;
    swap cells copz-rt-table + ! ;

include ./insts.fs
