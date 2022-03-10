\ RISC-V instruction table for 32 bit instructions

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

$00000037 u-type: lui
$00000017 u-type-pc: auipc
$0000006F j-type: jal
$00000067 l-type: jalr
$00000063 b-type: beq
$00001063 b-type: bne
$00004063 b-type: blt
$00005063 b-type: bge
$00006063 b-type: bltu
$00007063 b-type: bgeu
$00000003 l-type: lb
$00001003 l-type: lh
$00002003 l-type: lw
$00003003 l-type: ld
$00004003 l-type: lbu
$00005003 l-type: lhu
$00006003 l-type: lwu
$00000023 s-type: sb
$00001023 s-type: sh
$00002023 s-type: sw
$00003023 s-type: sd
$00000013 i-type: addi
$00002013 i-type: slti
$00003013 i-type: sltiu
$00004013 i-type: xori
$00006013 i-type: ori
$00007013 i-type: andi
$00001013 sh-type: slli
$00005013 sh-type: srli
$40005013 sh-type: srai
$0000101B sh-type: slliw
$0000501B sh-type: srliw
$4000501B sh-type: sraiw
$00000033 r-type: add
$40000033 r-type: sub
$00001033 r-type: sll
$00002033 r-type: slt
$00003033 r-type: sltu
$00004033 r-type: xor
$00005033 r-type: srl
$40005033 r-type: sra
$00006033 r-type: or
$00007033 r-type: and
$0000003B r-type: addw
$4000003B r-type: subw
$0000103B r-type: sllw
$0000503B r-type: srlw
$4000503B r-type: sraw
$0000000F fence-type: fence
$0000100F fence-type: fence.i \ Zifencei
$00000073 noarg-type: ecall
$00100073 noarg-type: ebreak
$00001073 csr-type: csrrw \ Zicsr
$00002073 csr-type: csrrs
$00003073 csr-type: csrrc
$00005073 csri-type: csrrwi
$00006073 csri-type: csrrsi
$00007073 csri-type: csrrci
$02000033 r-type: mul \ multiplication&division
$02001033 r-type: mulh
$02002033 r-type: mulhsu
$02003033 r-type: mulhu
$02004033 r-type: div
$02005033 r-type: divu
$02006033 r-type: rem
$02007033 r-type: remu
$0200003B r-type: mulw
$0200403B r-type: divw
$0200503B r-type: divuw
$0200603B r-type: remw
$0200703B r-type: remuw

$0000202F atom-type: amoadd.w
$0800202F atom-type: amoswap.w
$1000202F atom-type: lr.w
$1800202F atom-type: sc.w
$2000202F atom-type: amoxor.w
$4000202F atom-type: amoor.w
$6000202F atom-type: amoand.w
$8000202F atom-type: amomin.w
$A000202F atom-type: amomax.w
$C000202F atom-type: amominu.w
$E000202F atom-type: amomaxu.w

$0000302F atom-type: amoadd.d
$0800302F atom-type: amoswap.d
$1000302F atom-type: lr.d
$1800302F atom-type: sc.d
$2000302F atom-type: amoxor.d
$4000302F atom-type: amoor.d
$6000302F atom-type: amoand.d
$8000302F atom-type: amomin.d
$A000302F atom-type: amomax.d
$C000302F atom-type: amominu.d
$E000302F atom-type: amomaxu.d

$00002007 fl-type: flw
$00002027 fs-type: fsw
$00000043 fr4-type: fmadd.s
$00000047 fr4-type: fmsub.s
$0000004B fr4-type: fnmsub.s
$0000004F fr4-type: fnmadd.s
$00000053 fr-type: fadd.s
$08000053 fr-type: fsub.s
$10000053 fr-type: fmul.s
$18000053 fr-type: fdiv.s
$58000053 fr2-type: fsrqt.s
$20000053 fr-type: fsgnj.s
$20001053 fr-type: fsgnjn.s
$20002053 fr-type: fsgnjx.s
$28000053 fr-type: fmin.s
$28001053 fr-type: fmax.s
$C0000053 fri-type: fcvt.w.s
$C0100053 fri-type: fcvt.wu.s
$C0200053 fri-type: fcvt.l.s
$C0300053 fri-type: fcvt.lu.s
$E0000053 fir-type: fmv.x.w
$A0000053 fr-type: fle.s
$A0001053 fr-type: flt.s
$A0002053 fr-type: feq.s
$E0001053 fr-type: fclass.s
$D0000053 fir-type: fcvt.s.w
$D0100053 fir-type: fcvt.s.wu
$D0200053 fir-type: fcvt.s.l
$D0300053 fir-type: fcvt.s.lu
$F0000053 fri-type: fmv.w.x

$00003007 fl-type: fld
$00003027 fs-type: fsd
$02000043 fr4-type: fmadd.d
$02000047 fr4-type: fmsub.d
$0200004B fr4-type: fnmsub.d
$0200004F fr4-type: fnmadd.d
$02000053 fr-type: fadd.d
$0A000053 fr-type: fsub.d
$12000053 fr-type: fmul.d
$1A000053 fr-type: fdiv.d
$5A000053 fr2-type: fsrqt.d
$22000053 fr-type: fsgnj.d
$22001053 fr-type: fsgnjn.d
$22002053 fr-type: fsgnjx.d
$2A000053 fr-type: fmin.d
$2A001053 fr-type: fmax.d
$C2000053 fri-type: fcvt.w.d
$C2100053 fri-type: fcvt.wu.d
$C2200053 fri-type: fcvt.l.d
$C2300053 fri-type: fcvt.lu.d
$E2000053 fir-type: fmv.x.d
$A2000053 fr-type: fle.d
$A2001053 fr-type: flt.d
$A2002053 fr-type: feq.d
$E2001053 fr-type: fclass.d
$D2000053 fir-type: fcvt.d.w
$D2100053 fir-type: fcvt.d.wu
$D2200053 fir-type: fcvt.d.l
$D2300053 fir-type: fcvt.d.lu
$F2000053 fri-type: fmv.d.x
