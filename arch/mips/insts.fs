\ MIPS instruction encoding descriptions common to asm.fs and disasm.fs

\ Copyright (C) 2000,2007,2010 Free Software Foundation, Inc.

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

$00 constant asm-copz-MF
$02 constant asm-copz-CF
$04 constant asm-copz-MT
$06 constant asm-copz-CT
$08 constant asm-copz-BC
$10 constant asm-copz-C0

$00 constant asm-copz-BCF
$01 constant asm-copz-BCT

\ MIPS1 Instructions that take an immediate argument 
$00 asm-regimm-rs,rel			bltz,
$01 asm-regimm-rs,rel			bgez,
$10 asm-regimm-rs,rel			bltzal,
$11 asm-regimm-rs,rel			bgezal,
$04 asm-op asm-I-rs,rt,imm		beq,
$05 asm-op asm-I-rs,rt,imm		bne,
$00 $06 asm-op asm-rt asm-I-rs,rel	blez,
$00 $07 asm-op asm-rt asm-I-rs,rel	bgtz,
$08 asm-op asm-I-rt,rs,imm		addi,
$09 asm-op asm-I-rt,rs,imm		addiu,
$0a asm-op asm-I-rt,rs,imm		slti,
$0b asm-op asm-I-rt,rs,imm		sltiu,
$0c asm-op asm-I-rt,rs,uimm		andi,
$0d asm-op asm-I-rt,rs,uimm		ori,
$0e asm-op asm-I-rt,rs,uimm		xori,
$0f asm-op asm-I-rt,uimm		lui,
$20 asm-op asm-I-rt,offset,rs		lb,
$21 asm-op asm-I-rt,offset,rs		lh,
$22 asm-op asm-I-rt,offset,rs		lwl,
$23 asm-op asm-I-rt,offset,rs		lw,
$24 asm-op asm-I-rt,offset,rs		lbu,
$25 asm-op asm-I-rt,offset,rs		lhu,
$26 asm-op asm-I-rt,offset,rs		lwr,
$28 asm-op asm-I-rt,offset,rs		sb,
$29 asm-op asm-I-rt,offset,rs		sh,
$2a asm-op asm-I-rt,offset,rs		swl,
$2b asm-op asm-I-rt,offset,rs		sw,
$2e asm-op asm-I-rt,offset,rs		swr,

\ MIPS2 instructions with immediate argument
$14 asm-op asm-I-rs,rt,imm		beql,
$15 asm-op asm-I-rs,rt,imm		bnel,
$00 $16 asm-op asm-rt asm-I-rs,rel	blezl,
$00 $17 asm-op asm-rt asm-I-rs,rel	bgtzl,
$08 asm-regimm-rs,imm			tgei,
$09 asm-regimm-rs,imm			tgeiu,
$0a asm-regimm-rs,imm			tlti,
$0b asm-regimm-rs,imm			tltiu,
$0c asm-regimm-rs,imm			teqi,
$0e asm-regimm-rs,imm			tnei,
$30 asm-op asm-I-rt,offset,rs		ll,
$38 asm-op asm-I-rt,offset,rs		sc,

\ MIPS3 instructions with immediate argument
$18 asm-op asm-I-rt,rs,imm		daddi,
$19 asm-op asm-I-rt,rs,imm		daddiu,
$1a asm-op asm-I-rt,offset,rs		ldl,
$1b asm-op asm-I-rt,offset,rs		ldr,
$34 asm-op asm-I-rt,offset,rs		lld,
$37 asm-op asm-I-rt,offset,rs		ld,
$3c asm-op asm-I-rt,offset,rs		scd,
$3f asm-op asm-I-rt,offset,rs		sd,

\ MIPS1 instructions without immediate args (and immediate shifts) 
$02 asm-op asm-J-target			j,
$03 asm-op asm-J-target			jal,

$00 asm-special-rd,rt,sa		sll,
$02 asm-special-rd,rt,sa		srl,
$03 asm-special-rd,rt,sa		sra,
$04 asm-special-rd,rt,rs		sllv,
$06 asm-special-rd,rt,rs		srlv,
$07 asm-special-rd,rt,rs		srav,
$08 asm-special-rs			jr,
$09 asm-special-rd,rs			jalr, 
$0c asm-special-nothing			syscall,
$0d asm-special-nothing			break,
$10 asm-special-rd			mfhi,
$11 asm-special-rs			mthi,
$12 asm-special-rd			mflo,
$13 asm-special-rs			mtlo,
$18 asm-special-rs,rt			mult,
$19 asm-special-rs,rt			multu,
$1a asm-special-rs,rt			div,
$1b asm-special-rs,rt			divu,
$20 asm-special-rd,rs,rt		add,
$21 asm-special-rd,rs,rt		addu,
$22 asm-special-rd,rs,rt		sub,
$23 asm-special-rd,rs,rt		subu,
$24 asm-special-rd,rs,rt		and,
$25 asm-special-rd,rs,rt		or,
$26 asm-special-rd,rs,rt		xor,
$27 asm-special-rd,rs,rt		nor,
$2a asm-special-rd,rs,rt		slt,
$2b asm-special-rd,rs,rt		sltu,

$00 asm-special2-rs,rt			madd,
$01 asm-special2-rs,rt			maddu,
$02 asm-special2-rd,rs,rt		mul,
$04 asm-special2-rs,rt			msub,
$05 asm-special2-rs,rt			msubu,

\ MIPS2 instructions without immediate arg
\ todo: these take an (optional) 10-bit argument CODE for which we need
\ to add support.
$30 asm-special-rs,rt			tge,
$31 asm-special-rs,rt			tgeu,
$32 asm-special-rs,rt			tlt,
$33 asm-special-rs,rt			tltu,
$34 asm-special-rs,rt			teq,
$36 asm-special-rs,rt			tne,

\ MIPS3 Instructions without immediate (and immediate shifts)
$2c asm-special-rd,rs,rt		dadd,
$2d asm-special-rd,rs,rt		daddu,
$2e asm-special-rd,rs,rt		dsub,
$2f asm-special-rd,rs,rt		dsubu,
$1c asm-special-rs,rt			dmult,
$1d asm-special-rs,rt			dmultu,
$1e asm-special-rs,rt			ddiv,
$1f asm-special-rs,rt			ddivu,

$14 asm-special-rd,rt,rs		dsllv,
$16 asm-special-rd,rt,rs		dsrlv,
$17 asm-special-rd,rt,rs		dsrav,
$38 asm-special-rd,rt,sa		dsll,
$3a asm-special-rd,rt,sa		dsrl,
$3b asm-special-rd,rt,sa		dsra,
$3c asm-special-rd,rt,sa		dsll32,
$3e asm-special-rd,rt,sa		dsrl32,
$3f asm-special-rd,rt,sa		dsra32,

\ MIPS4 instructions
$0a asm-special-rd,rs,rt		movz,
$0b asm-special-rd,rs,rt		movn,

\ (some) MIPS32 and newer instructions
$20 asm-special2-rd,rs			clz,
$21 asm-special2-rd,rs			clo,
$3b asm-special3-rt,rd			rdhwr,
$10 asm-bshfl-rd,rt			seb,
$02 asm-bshfl-rd,rt			wsbh,

\ MIPS1 coprocessor Instructions
$30 asm-copz-rt,offset,rs		lwcz,
$38 asm-copz-rt,offset,rs		swcz,
$31 asm-op asm-I-rt,offset,rs		lwc1,
$32 asm-op asm-I-rt,offset,rs		lwc2,
$39 asm-op asm-I-rt,offset,rs		swc1,
$3a asm-op asm-I-rt,offset,rs		swc2,
asm-copz-MF $00 asm-rs asm-copz-rt,rd	mfcz,
asm-copz-CF $00 asm-rs asm-copz-rt,rd	cfcz,
asm-copz-MT $00 asm-rs asm-copz-rt,rd	mtcz,
asm-copz-CT $00 asm-rs asm-copz-rt,rd	ctcz,
asm-copz-BC $00 asm-rs asm-copz-BCF swap asm-rt asm-copz-imm bczf,
asm-copz-BC $00 asm-rs asm-copz-BCT swap asm-rt asm-copz-imm bczt,
$01 asm-copz0				tlbr,
$02 asm-copz0				tlbwi,
$06 asm-copz0				tlbwr,
$08 asm-copz0				tlbl,
