\ asm.fs	assembler file (for MIPS R3000)
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

$20 constant asm-registers

: asm-register ( n n "name" ... "name" -- )
    ?do
	i constant
    loop ;

$08 $00 asm-register @zero @at @v0 @v1 @a0 @a1 @a2 @a3
$10 $08 asm-register @t0 @t1 @t2 @t3 @t4 @t5 @t6 @t7
$18 $10 asm-register @s0 @s1 @s2 @s3 @s4 @s5 @s6 @s7
$20 $18 asm-register @t8 @t9 @k0 @k1 @gp @sp @s8 @ra

$00 constant asm-init-code

: asm-bitmask ( n -- code )
    $1 swap lshift 1- ;

: asm-bmask ( n n "name" ... "name" -- )
    ?do
	i asm-bitmask constant
    loop ;

$09 $01 asm-bmask asm-bm01 asm-bm02 asm-bm03 asm-bm04 asm-bm05 asm-bm06 asm-bm07 asm-bm08
$11 $09 asm-bmask asm-bm09 asm-bm0A asm-bm0B asm-bm0C asm-bm0D asm-bm0E asm-bm0F asm-bm10
$19 $11 asm-bmask asm-bm11 asm-bm12 asm-bm13 asm-bm14 asm-bm15 asm-bm16 asm-bm17 asm-bm18
$20 $19 asm-bmask asm-bm19 asm-bm1A asm-bm1B asm-bm1C asm-bm1D asm-bm1E asm-bm1F
$FFFFFFFF constant asm-bm20

: asm-expand ( x -- x )
    dup $0000ffff > if
	$ffff0000 or
    endif ;

: asm-op ( n -- code )
    asm-bm06 and $1a lshift ;

: asm-rs ( n code -- code )
    swap asm-bm05 and $15 lshift or ;

: asm-rt ( n code -- code )
    swap asm-bm05 and $10 lshift or ;

: asm-imm ( n code -- code )
    swap asm-bm10 and or ;
' asm-imm alias asm-offset

: asm-target ( n code -- code )
    swap 2 rshift asm-bm1A and or ;

: asm-rd ( n code -- code )
    swap asm-bm05 and $b lshift or ;

: asm-shamt ( n code -- code )
    swap asm-bm05 and $6 lshift or ;
' asm-shamt alias asm-sa

: asm-funct ( n code -- code )
    swap asm-bm06 and or ;

\ ***** I-types
: asm-I-type ( code -- )
    a, ;

: (asm-I-rt,imm) ( rt imm addr -- )
    @ asm-imm asm-rt a, ;

: asm-I-rt,imm
    create asm-I-type
does>
    (asm-I-rt,imm) ;

: (asm-I-rs,imm) ( rs imm addr -- )
    @ swap 2 rshift swap asm-imm asm-rs a, ;

: asm-I-rs,imm
    create asm-I-type
does>
    (asm-I-rs,imm) ;

: (asm-I-rt,rs,imm) ( rt rs imm addr -- )
    @ asm-imm asm-rs asm-rt a, ;

: asm-I-rt,rs,imm
    create asm-I-type
does>
    (asm-I-rt,rs,imm) ;

: (asm-I-rs,rt,imm) ( rs rt imm addr -- )
    @ swap 2 rshift swap asm-imm asm-rt asm-rs a, ;

: asm-I-rs,rt,imm
    create asm-I-type
does>
    (asm-I-rs,rt,imm) ;

: (asm-I-rt,offset,rs) ( rt offset rs addr -- )
    @ asm-rs asm-offset asm-rt a, ;

: asm-I-rt,offset,rs
    create asm-I-type
does>
    (asm-I-rt,offset,rs) ;

\ ***** regimm types
: asm-regimm-rs,imm ( funct -- )
    $01 asm-op asm-rt asm-I-rs,imm ;

\ ***** copz types 1
: (asm-I-imm,z) ( imm z addr -- )
    @ swap asm-op or swap 2 rshift swap asm-imm a, ;

: asm-I-imm,z
    create asm-I-type
does>
    (asm-I-imm,z) ;

: asm-copz-imm ( code -- )
    $10 asm-op or asm-I-imm,z ;

: (asm-I-rt,offset,rs,z) ( rt offset rs z addr -- )
    @ swap asm-op or asm-rs asm-offset asm-rt a, ;

: asm-I-rt,offset,rs,z
    create asm-I-type
does>
    (asm-I-rt,offset,rs,z) ;

: asm-copz-rt,offset,rs ( code -- )
    asm-op asm-I-rt,offset,rs,z ;

$00 constant asm-copz-MF
$02 constant asm-copz-CF
$04 constant asm-copz-MT
$06 constant asm-copz-CT
$08 constant asm-copz-BC
$10 constant asm-copz-C0

$00 constant asm-copz-BCF
$01 constant asm-copz-BCT

\ ***** J-types
: asm-J-type ( code -- )
    a, ;

: (asm-J-target) ( target addr -- )
    @ asm-target a, ;

: asm-J-target
    create asm-J-type
does>
    (asm-J-target) ;

\ ***** R-types
: asm-R-type ( code -- )
    a, ;

: (asm-R-nothing) ( addr -- )
    @ a, ;

: asm-R-nothing
    create asm-R-type
does>
    (asm-R-nothing) ;

: (asm-R-rd) ( rd addr -- )
    @ asm-rd a, ;

: asm-R-rd
    create asm-R-type
does>
    (asm-R-rd) ;

: (asm-R-rs) ( rs addr -- )
    @ asm-rs a, ;

: asm-R-rs
    create asm-R-type
does>
    (asm-R-rs) ;

: (asm-R-rd,rs) ( rd rs addr -- )
    @ asm-rs asm-rd a, ;

: asm-R-rd,rs
    create asm-R-type
does>
    (asm-R-rd,rs) ;

: (asm-R-rs,rt) ( rs rt addr -- )
    @ asm-rt asm-rs a, ;

: asm-R-rs,rt
    create asm-R-type
does>
    (asm-R-rs,rt) ;

: (asm-R-rd,rs,rt) ( rd rs rt addr -- )
    @ asm-rt asm-rs asm-rd a, ;

: asm-R-rd,rs,rt
    create asm-R-type
does>
    (asm-R-rd,rs,rt) ;

: (asm-R-rd,rt,rs) ( rd rt rs addr -- )
    @ asm-rs asm-rt asm-rd a, ;

: asm-R-rd,rt,rs
    create asm-R-type
does>
    (asm-R-rd,rt,rs) ;

: (asm-R-rd,rt,sa) ( rd rt sa addr -- )
    @ asm-sa asm-rt asm-rd a, ;

: asm-R-rd,rt,sa
    create asm-R-type
does>
    (asm-R-rd,rt,sa) ;

\ ***** special types
: asm-special-nothing ( funct -- )
    asm-init-code asm-funct asm-R-nothing ;

: asm-special-rd ( funct -- )
    asm-init-code asm-funct asm-R-rd ;

: asm-special-rs ( funct -- )
    asm-init-code asm-funct asm-R-rs ;

: asm-special-rd,rs ( funct -- )
    asm-init-code asm-funct asm-R-rd,rs ;

: asm-special-rs,rt ( funct -- )
    asm-init-code asm-funct asm-R-rs,rt ;

: asm-special-rd,rs,rt ( funct -- )
    asm-init-code asm-funct asm-R-rd,rs,rt ;

: asm-special-rd,rt,rs ( funct -- )
    asm-init-code asm-funct asm-R-rd,rt,rs ;

: asm-special-rd,rt,sa ( funct -- )
    asm-init-code asm-funct asm-R-rd,rt,sa ;

\ ***** copz types 2
: asm-copz0 ( funct -- )
    $10 $10 asm-op asm-rs asm-funct asm-R-nothing ;

: (asm-R-rt,rd,z) ( rt rd z addr -- )
    @ swap asm-op or asm-rd asm-rt a, ;

: asm-R-rt,rd,z
    create asm-R-type
does>
    (asm-R-rt,rd,z) ;

: asm-copz-rt,rd ( funct -- )
    $10 asm-op or asm-R-rt,rd,z ;

: nop, ( -- )
    0 a, ;

$04 asm-op asm-I-rs,rt,imm		beq,
$05 asm-op asm-I-rs,rt,imm		bne,
$00 $06 asm-op asm-rt asm-I-rs,imm	blez,
$00 $07 asm-op asm-rt asm-I-rs,imm	bgtz,
$08 asm-op asm-I-rt,rs,imm		addi,
$09 asm-op asm-I-rt,rs,imm		addiu,
$0a asm-op asm-I-rt,rs,imm		slti,
$0b asm-op asm-I-rt,rs,imm		sltiu,
$0c asm-op asm-I-rt,rs,imm		andi,
$0d asm-op asm-I-rt,rs,imm		ori,
$0e asm-op asm-I-rt,rs,imm		xori,
$0f asm-op asm-I-rt,imm			lui,
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

$00 asm-regimm-rs,imm			bltz,
$01 asm-regimm-rs,imm			bgez,
$10 asm-regimm-rs,imm			bltzal,
$11 asm-regimm-rs,imm			bgezal,

$30 asm-copz-rt,offset,rs		lwcz,
$38 asm-copz-rt,offset,rs		swcz,
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

: move, ( rd rs -- )
    @zero addu, ;

: abs, ( rd rs -- )
    dup $0008 bgez,
    2dup move,
    @zero swap subu, ;

: neg, ( rd rs -- )
    @zero swap subu, ;

: negu, ( rd rs -- )
    @zero swap subu, ;

: not, ( rd rs -- )
    @zero nor, ;

: li, ( rd imm -- )
    dup 0= if
	drop dup @zero = if
	    drop nop, assert( false )
	else
	    @zero move,
	endif
    else
	dup $8000 u< if
	    @zero swap addiu,
	else
	    dup $10000 u< if
		@zero swap ori,
	    else
		dup $ffff and 0= if
		    $10 rshift lui,
		else
		    dup $ffff8000 and $ffff8000 = if
			@zero swap addiu,
		    else
			2dup $10 rshift lui,
			over swap ori,
		    endif
		endif
	    endif
	endif
    endif ;

: blt, ( rs rt imm -- )		\ <
    >r @at rot rot slt,
    @at @zero r> bne, ;

: ble, ( rs rt imm -- )		\ <=
    >r @at rot rot swap slt,
    @at @zero r> beq, ;

: bgt, ( rs rt imm -- )		\ >
    >r @at rot rot swap slt,
    @at @zero r> bne, ;

: bge, ( rs rt imm -- )		\ >=
    >r @at rot rot slt,
    @at @zero r> beq, ;

: bltu, ( rs rt imm -- )	\ < unsigned
    >r @at rot rot sltu,
    @at @zero r> bne, ;

: bleu, ( rs rt imm -- )	\ <= unsigned
    >r @at rot rot swap sltu,
    @at @zero r> beq, ;

: bgtu, ( rs rt imm -- )	\ > unsigned
    >r @at rot rot swap sltu,
    @at @zero r> bne, ;

: bgeu, ( rs rt imm -- )	\ >= unsigned
    >r @at rot rot sltu,
    @at @zero r> beq, ;

