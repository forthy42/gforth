\ RISC-V assembler

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

get-current also assembler definitions also

: regs: ( -- )
    $20 0 DO I Constant LOOP ;
: fences: ( -- )
    $10 1 DO I Constant LOOP ;

regs: zero ra sp gp tp t0 t1 t2 s0 s1 a0 a1 a2 a3 a4 a5 a6 a7 s2 s3 s4 s5 s6 s7 s8 s9 s10 s11 t3 t4 t5 t6
regs: ft0 ft1 ft2 ft3 ft4 ft5 ft6 ft7 fs0 fs1 fa0 fa1 fa2 fa3 fa4 fa5 fa6 fa7 fs2 fs3 fs4 fs5 fs6 fs7 fs8 fs9 fs10 fs11 ft8 ft9 ft10 ft11
regs: x0 x1 x2 x3 x4 x5 x6 x7 x8 x9 x10 x11 x12 x13 x14 x15 x16 x17 x18 x19 x20 x21 x22 x23 x24 x25 x26 x27 x28 x29 x30 x31
regs: f0 f1 f2 f3 f4 f5 f6 f7 f8 f9 f10 f11 f12 f13 f14 f15 f16 f17 f18 f19 f20 f21 f22 f23 f24 f25 f26 f27 f28 f29 f30 f31
fences: w r rw o ow or orw i iw ir irw io iow ior iorw

: >rd ( reg inst -- inst' ) swap $1F and 7 lshift or ;
: >rs0 ( reg inst -- inst' ) swap $1F and 2 lshift or ;
: >rs1 ( reg inst -- inst' ) swap $1F and 15 lshift or ;
: >rs2 ( reg inst -- inst' ) swap $1F and 20 lshift or ;
: >shift ( amount inst -- inst' ) swap $3F and 20 lshift or ;
: >rs3 ( reg inst -- inst' ) swap $1F and 27 lshift or ;

s" not in compact register range" exception Constant no-reg
: ?c-reg ( reg -- c-reg )
    8 - dup $8 u>= no-reg and throw ;
: >rd' ( reg inst -- inst' ) swap ?c-reg 2 lshift or ;
: >rs1' ( reg inst -- inst' ) swap ?c-reg 7 lshift or ;

: >fence1 ( fence x -- x' ) swap $F and 24 lshift or ;
: >fence2 ( fence x -- x' ) swap $F and 20 lshift or ;

: >imm-4spn ( u x -- x' ) >r
    \ [5:4|9:6|2|3]
    dup 4 rshift 3 and >r
    dup 6 rshift $F and r> 4 lshift or >r
    dup 2 rshift 1 and r> 2* or >r
    3 rshift r> or 5 lshift r> or ;
: >imm-size ( imm size -- )
    -1 swap lshift >r dup 6 rshift r@ invert and swap r> and or ;
: >imm-1 ( imm x -- x' ) >r
    dup $20 and 12 5 - lshift r> or >r
    $1F and 2 lshift r> or ;
: >imm-1size ( imm x size -- x' ) swap >r >imm-size r> >imm-1 ;
: >imm-2 ( imm x -- x' ) >r
    dup $1C and 10 3 - lshift r> or >r
    $3 and 2 lshift r> or ;
: >imm-2size ( imm x size -- x' ) swap >r
    >r 2/ r> 1- >imm-size r> >imm-2 ;
: >imm-3 ( imm x -- x' ) >r
    dup $1F and 7 lshift r> or ;
: >imm-3size ( imm x size -- x' ) swap >r
    >imm-size r> >imm-3 ;
: >imm-cj ( imm x -- x' )  >r here -
    \ [11|4|9:8|10|6|7|3:1|5]
    dup 11 rshift 1 and >r
    dup 4 rshift 1 and r> 2* or >r
    dup 8 rshift 3 and r> 2* 2* or >r
    dup 10 rshift 1 and r> 2* or >r
    dup 6 rshift 1 and r> 2* or >r
    dup 7 rshift 1 and r> 2* or >r
    dup 1 rshift 7 and r> 2* 2* 2* or >r
    5 rshift 1 and r> 2* or
    2 lshift r> or ;
: >imm-beq ( imm x -- x' ) >r here -
    \ imm[8|4:3] rs1 imm[7:6|2:1|5]
    dup 8 rshift 1 and >r
    dup 3 rshift 3 and r> 2* 2* or >r
    dup 6 rshift 3 and r> 5 lshift >r
    dup 1 rshift 3 and r> 2* 2* or >r
    5 rshift 1 and r> 2* or
    2 lshift r> or ;
: >imm-16 ( imm x -- x' ) >r
    \  nzimm[9] 2 nzimm[4|6|8:7|5]
    dup 9 rshift 1 and >r
    dup 4 rshift 1 and r> 6 lshift or >r
    dup 6 rshift 1 and r> 2* or >r
    dup 7 rshift 3 and r> 2* 2* or >r
    5 rshift 1 and r> 2* or
    2 lshift r> or ;

: inst: ( "name" -- )
    : ]] Create , DOES> @ [[ ;
: c.Create ( "name" -- )
    parse-name [: ." c." type ;] $tmp nextname Create ;
: c.inst: ( "name" -- )
    : ]] c.Create , DOES> @ [[ ;

c.inst: c-noarg:     w, ;
c.inst: c-addi4spn:  >imm-4spn >rd' w, ;
c.inst: c-ldw:       2 >imm-2size >rs1' >rd' w, ;
c.inst: c-ldd:       3 >imm-2size >rs1' >rd' w, ;
synonym c-fldd: c-ldd:
c.inst: c-addi:      >imm-1 >rd w, ;
synonym c-sli: c-addi:
synonym c-li: c-addi:
c.inst: c-andi:      >imm-1 >rd' w, ;
synonym c-sri: c-andi:
c.inst: c-lui:       >r 12 rshift r> >imm-1 >rd' w, ;
c.inst: c-and:       >rs1' >rd' w, ;
c.inst: c-j:         >imm-cj w, ;
c.inst: c-beq:       >imm-beq >rs1' w, ;
c.inst: c-ldsp:      3 >imm-2size >rd w, ;
synonym c-fldsp: c-ldsp:
c.inst: c-lwsp:      2 >imm-3size >rd w, ;
c.inst: c-sdsp:      3 >imm-3size >rs0 w, ;
synonym c-fsdsp: c-sdsp:
c.inst: c-swsp:      2 >imm-3size >rs0 w, ;
c.inst: c-add:       >rs0 >rd w, ;
synonym c-mv: c-add:
c.inst: c-jr:        >rd w, ;
c.inst: c-addi16:    >imm-16 w, ;

: >imm-i ( imm x -- x' ) swap $FFF and 20 lshift or ;
: >imm-s ( imm x -- x' )
    over $1F and 7 lshift or swap $FE0 and 20 lshift or ;
: >imm-u ( imm x -- x' )
    swap $FFFFF000 and or ;

inst: atom-type: >rs1 >rs2 >rd l, ;
inst: b-type: l, ;
inst: fence-type: >fence2 >fence1 l, ;
inst: fr2-type: >rs1 >rd l, ;
inst: fr4-type: >rs3 >rs2 >rs1 >rd l, ;
inst: fri-type: >rs1 >rd l, ;
synonym fir-type: fri-type:
inst: i-type: >imm-i >rs1 >rd l, ;
synonym l-type: i-type:
synonym fl-type: i-type:
synonym csr-type: i-type:
synonym csri-type: i-type:
inst: j-type: l, ;
inst: noarg-type: l, ;
inst: r-type:   >rs2 >rs1 >rd l, ;
synonym fr-type: r-type:
inst: sh-type: >shift >rs1 >rd l, ;
inst: s-type: >imm-s >rs2 >rs1 l, ;
synonym fs-type: s-type:
inst: u-type: >imm-u >rd l, ;
inst: u-type-pc: swap here - swap >imm-u >rd l, ;

include ./inst16.fs
include ./inst32.fs

previous previous set-current
