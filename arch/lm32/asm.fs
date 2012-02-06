\ asm.fs	assembler for LatticeMico32 CPU
\
\ Copyright (C) 2012 Free Software Foundation, Inc.

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

\ Author: David Kühling
\ Created: Feb 2012

require ./../../code.fs

ALSO ASSEMBLER DEFINITIONS

100001 constant #register
100002 constant #csr

\ registers
: register:  ( n "name" -- )  #register 2CONSTANT ;
: regs:  ( n1 n2 "name..." -- )  DO  I register: LOOP ;

16  0 regs: r0 r1 r2 r3 r4 r5 r6 r7 r8 r9 r10 r11 r12 r13 r14 r15
32 16 regs: r16 r17 r18 r19 r20 r21 r22 r23 r24 r25 r26 r27 r28 r29 r30 r31 
32 26 regs: gb fp sp ra ea ba

\ control&status registers
: csr:  ( n "name" -- )  #csr 2CONSTANT ;
: csrs:  ( n1 n2 "name..." -- )  DO  I csr:  LOOP ;

8 0 csrs: IE IM IP ICC DCC CC CFG EBA
10 8 csrs: DC DEBA
20 14 csrs: JTX JRX BP0 BP1 BP2 BP3
28 24 csrs: WP0 WP1 WP2 WP3

\ instruction latch
0 VARIABLE latch
: latch,  ( x -- )  latch @ OR latch ! ;
: <inst  ( a-addr -- )  @ latch ! ;
: <opinst  ( a-addr -- )  @ 26 LSHIFT latch ! ;
: inst,   HERE 4 ALLOT be-l! ;
: inst>  ( -- )  latch @ inst, ;

\ instruction code components
: ?register  ( x u -- x )  #register <> ABORT" not a register" ;
: ?csr  ( x u -- x )  #csr <> ABORT" not a CSR" ;   
: csr,  ( x u -- )   ?csr  21 LSHIFT latch, ;
: regarg:  ( n "name" -- )  \ give bit-offset
   CREATE ,
  DOES> @ -ROT ?register  SWAP LSHIFT latch, ;

21 regarg: reg0,
16 regarg: reg1,
11 regarg: reg2,

: #imm16,  ( n -- )
   DUP -32768 32768 WITHIN 0= ABORT" 16-bit constant out of range"
   latch, ;
: #imm26,  ( n -- )
   DUP 1 25 LSHIFT DUP NEGATE SWAP WITHIN
   0= ABORT" 26-bit constant out of range"
   latch, ;

\ instruction formats
\ todo: do we need support for specifying absolute branch targets?
: rr#: CREATE ,  DOES> <opinst   #imm16, reg0, reg1, inst> ;
: r#r: CREATE ,  DOES> <opinst  reg1, #imm16, reg0,  inst> ;
: rrr: CREATE ,  DOES> <opinst  reg1, reg0, reg2, inst> ;
: rr: CREATE ,  DOES> <opinst  reg0, reg2, inst> ;
: r:  CREATE ,  DOES> <opinst  reg0, inst> ;
: rcsr: CREATE ,  DOES> <opinst  csr, reg2, inst> ;
: csrr: CREATE ,  DOES> <opinst  reg1, csr, inst> ;
: imm26:  CREATE ,  DOES> <opinst  #imm26, inst> ;
: name:  CREATE ,  DOES> <inst inst> ;

\ cheat sheet
\  lh r5, (r3+4)                  r5  r3 4 lh,
\  sh (r3+4), r5                  r3 4  r5 sh,

\ instruction table

$00 rr#: srui,    $01 rr#: nori,   $02 rr#: muli,    $03  r#r: sh,
$04 rr#: lb,      $05 rr#: sri,    $06 rr#: xori,    $07  rr#: lh,
$08 rr#: andi,    $09 rr#: xnori,  $0a rr#: lw,      $0b  rr#: lhu,
$0c r#r: sb,      $0d rr#: addi,   $0e rr#: ori,     $0f  rr#: sli,

$10 rr#: lbu,     $11 rr#: be,     $12 rr#: bg,      $13  rr#: bge,
$14 rr#: bgeu,    $15 rr#: bgu,    $16 r#r: sw,      $17  rr#: bne,
$18 rr#: andhi,   $19 rr#: cmpei,  $1a rr#: cmpgi,   $1b  rr#: cmpgei,
$1c rr#: cmpgeui, $1d rr#: cmpgui, $1e rr#: orhi,    $1f  rr#: cmpnei,

$20 rrr: sru,     $21 rrr: nor,    $22 rrr: mul,     $23 rrr: divu,
$24 rcsr: rcsr,   $25 rrr: sr,     $26 rrr: xor,     $27 rrr: div,
$28 rrr: and,     $29 rrr: xnor, ( $2a name: rsvd,)  ( $2b name: raise,)
$2c rr: sextb,    $2d rrr: add,    $2e rrr: or,      $2f rrr: sl,

$30 r: b,         $31 rrr: modu,   $32 rrr: sub,   ( $33 rrr: rsvd, )
$34 csrr: wcsr,   $35 rrr: mod,    $36 r: call,      $37 rr:  sexth,
$38 imm26: bi,    $39 rrr: cmpe,   $3a rrr: cmpg,    $3b rrr: cmpge,
$3c rrr: cmpgeu,  $3d rrr: cmpgu,  $3e imm26: calli, $3f rrr: cmpne,

$ac000002 name: break,
$ac000007 name: scall,

\ todo: add forth-style branch primitives if, ahead, etc.

\ todo: use get-current / set-current
PREVIOUS DEFINITIONS


\ \ Customize Emacs
\ 0 [IF]
\    Local Variables:
\    compile-command: "gforth ./testasm.fs -e bye"
\    End:
\ [THEN]
