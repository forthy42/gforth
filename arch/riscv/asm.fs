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

regs: zero ra sp gp tp t0 t1 t2 s0 s1 a0 a1 a2 a3 a4 a5 a6 a7 s2 s3 s4 s5 s6 s7 s8 s9 s10 s11 t3 t4 t5 t6
regs: ft0 ft1 ft2 ft3 ft4 ft5 ft6 ft7 fs0 fs1 fa0 fa1 fa2 fa3 fa4 fa5 fa6 fa7 fs2 fs3 fs4 fs5 fs6 fs7 fs8 fs9 fs10 fs11 ft8 ft9 ft10 ft11
regs: x0 x1 x2 x3 x4 x5 x6 x7 x8 x9 x10 x11 x12 x13 x14 x15 x16 x17 x18 x19 x20 x21 x22 x23 x24 x25 x26 x27 x28 x29 x30 x31
regs: f0 f1 f2 f3 f4 f5 f6 f7 f8 f9 f10 f11 f12 f13 f14 f15 f16 f17 f18 f19 f20 f21 f22 f23 f24 f25 f26 f27 f28 f29 f30 f31

: >rd ( destreg inst -- inst' ) swap $1F and 7 lshift or ;
: >rs0 ( destreg inst -- inst' ) swap $1F and 2 lshift or ;
: >rs1 ( destreg inst -- inst' ) swap $1F and 15 lshift or ;
: >rs2 ( destreg inst -- inst' ) swap $1F and 20 lshift or ;
: >rs3 ( destreg inst -- inst' ) swap $1F and 27 lshift or ;

s" not in compact register range" exception no-reg
: >c-reg ( reg -- c-reg )
    8 - dup $8 u>= no-reg and throw ;
: >rd' ( destreg inst -- inst' ) swap >c-reg 2 lshift or ;
: >rs1' ( destreg inst -- inst' ) swap >c-reg 7 lshift or ;

previous previous set-current
