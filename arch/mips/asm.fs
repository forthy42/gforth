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

?test $0800 [IF]
cr ." Test for asm.fs" cr

: exec ( ... xt u -- ... w1 ... wu )
    >r execute r> 1+ cells cell ?do
	here i - @
    cell +loop ;

: same ( valn ... val0 u -- flag )
    true swap dup 4 + 4 ?do
	i 2dup pick
	rot rot + 1- pick
	= rot and swap
    loop
    swap >r 2* 0 ?do
	drop
    loop
    r> ;

variable asm-xt
variable asm-u
variable asm-z

: save ( xt u -- )
    asm-u !
    dup name. asm-xt ! ;

: check ( ... -- )
    asm-xt @ asm-u @ dup >r exec r> same if
	." OK "
    else
	." NOK "
    endif ;

: asm-test0 ( val xt u -- )
    save
    check cr ;

: asm-test2 ( val val xt u -- )
    save
    $ffffffff check
    $1 check cr ;

: asm-test2i ( val val xt u -- )
    save
    $fffffffc check
    $4 check cr ;

: asm-test2-copzi ( val val z xt u -- )
    save asm-z !
    $fffffffc asm-z @ check
    $4 asm-z @ check cr ;

: asm-test4 ( val val val val xt u -- )
    save
    $ffffffff $ffffffff check
    $0 $1 check
    $1 $0 check
    $1 $1 check cr ;

: asm-test4i ( val val val val xt u -- )
    save
    $ffffffff $fffffffc check
    $0 $4 check
    $1 $0 check
    $1 $4 check cr ;

: asm-test4-copz ( val val val val z xt u -- )
    save asm-z !
    $ffffffff $ffffffff asm-z @ check
    $0 $1 asm-z @ check
    $1 $0 asm-z @ check
    $1 $1 asm-z @ check cr ;

: asm-test5 ( val val val val val xt u -- )
    save
    $ffffffff $ffffffff $ffffffff check
    $0 $0 $1 check
    $0 $1 $0 check
    $1 $0 $0 check
    $1 $1 $1 check cr ;

: asm-test5i ( val val val val val xt u -- )
    save
    $ffffffff $ffffffff $fffffffc check
    $0 $0 $4 check
    $0 $1 $0 check
    $1 $0 $0 check
    $1 $1 $4 check cr ;

: asm-test5-copz ( val val val val val z xt u -- )
    save asm-z !
    $ffffffff $ffffffff $ffffffff asm-z @ check
    $0 $0 $1 asm-z @ check
    $0 $1 $0 asm-z @ check
    $1 $0 $0 asm-z @ check
    $1 $1 $1 asm-z @ check cr ;

$00210820 $00000820 $00200020 $00010020 $03fff820 ' add, 1 asm-test5
$20210001 $20010000 $20200000 $20000001 $23ffffff ' addi, 1 asm-test5
$24210001 $24010000 $24200000 $24000001 $27ffffff ' addiu, 1 asm-test5
$00210821 $00000821 $00200021 $00010021 $03fff821 ' addu, 1 asm-test5
$00210824 $00000824 $00200024 $00010024 $03fff824 ' and, 1 asm-test5
$30210001 $30010000 $30200000 $30000001 $33ffffff ' andi, 1 asm-test5
$45000001 $4500ffff 1 ' bczf, 1 asm-test2-copzi
$45010001 $4501ffff 1 ' bczt, 1 asm-test2-copzi
$10210001 $10200000 $10010000 $10000001 $13ffffff ' beq, 1 asm-test5i
$04210001 $04210000 $04010001 $07e1ffff ' bgez, 1 asm-test4i
$04310001 $04310000 $04110001 $07f1ffff ' bgezal, 1 asm-test4i
$1c200001 $1c200000 $1c000001 $1fe0ffff ' bgtz, 1 asm-test4i
$18200001 $18200000 $18000001 $1be0ffff ' blez, 1 asm-test4i
$04200001 $04200000 $04000001 $07e0ffff ' bltz, 1 asm-test4i
$04300001 $04300000 $04100001 $07f0ffff ' bltzal, 1 asm-test4i
$14210001 $14200000 $14010000 $14000001 $17ffffff ' bne, 1 asm-test5i
$0000000d ' break, 1 asm-test0
$44410800 $44410000 $44400800 $445ff800 1 ' cfcz, 1 asm-test4-copz
$44c10800 $44c10000 $44c00800 $44dff800 1 ' ctcz, 1 asm-test4-copz
$0021001a $0020001a $0001001a $03ff001a ' div, 1 asm-test4
$0021001b $0020001b $0001001b $03ff001b ' divu, 1 asm-test4
$08000001 $0bffffff ' j, 1 asm-test2i
$0c000001 $0fffffff ' jal, 1 asm-test2i
$00200809 $00000809 $00200009 $03e0f809 ' jalr, 1 asm-test4
$00200008 $03e00008 ' jr, 1 asm-test2
$80210001 $80010000 $80000001 $80200000 $83ffffff ' lb, 1 asm-test5
$90210001 $90010000 $90000001 $90200000 $93ffffff ' lbu, 1 asm-test5
$84210001 $84010000 $84000001 $84200000 $87ffffff ' lh, 1 asm-test5
$94210001 $94010000 $94000001 $94200000 $97ffffff ' lhu, 1 asm-test5
$3c010001 $3c010000 $3c000001 $3c1fffff ' lui, 1 asm-test4
$8c210001 $8c010000 $8c000001 $8c200000 $8fffffff ' lw, 1 asm-test5
$c4210001 $c4010000 $c4000001 $c4200000 $c7ffffff 1 ' lwcz, 1 asm-test5-copz
$88210001 $88010000 $88000001 $88200000 $8bffffff ' lwl, 1 asm-test5
$98210001 $98010000 $98000001 $98200000 $9bffffff ' lwr, 1 asm-test5
$44010800 $44010000 $44000800 $441ff800 1 ' mfcz, 1 asm-test4-copz
$00000810 $0000f810 ' mfhi, 1 asm-test2
$00000812 $0000f812 ' mflo, 1 asm-test2
$44810800 $44810000 $44800800 $449ff800 1 ' mtcz, 1 asm-test4-copz
$00200011 $03e00011 ' mthi, 1 asm-test2
$00200013 $03e00013 ' mtlo, 1 asm-test2
$00210018 $00200018 $00010018 $03ff0018 ' mult, 1 asm-test4
$00210019 $00200019 $00010019 $03ff0019 ' multu, 1 asm-test4
$00210827 $00000827 $00200027 $00010027 $03fff827 ' nor, 1 asm-test5
$00210825 $00000825 $00200025 $00010025 $03fff825 ' or, 1 asm-test5
$34210001 $34010000 $34200000 $34000001 $37ffffff ' ori, 1 asm-test5
$a0210001 $a0010000 $a0000001 $a0200000 $a3ffffff ' sb, 1 asm-test5
$a4210001 $a4010000 $a4000001 $a4200000 $a7ffffff ' sh, 1 asm-test5
$0021082a $0000082a $0020002a $0001002a $03fff82a ' slt, 1 asm-test5
$28210001 $28010000 $28200000 $28000001 $2bffffff ' slti, 1 asm-test5
$2c210001 $2c010000 $2c200000 $2c000001 $2fffffff ' sltiu, 1 asm-test5
$0021082b $0000082b $0020002b $0001002b $03fff82b ' sltu, 1 asm-test5
$00210822 $00000822 $00200022 $00010022 $03fff822 ' sub, 1 asm-test5
$00210823 $00000823 $00200023 $00010023 $03fff823 ' subu, 1 asm-test5
$ac210001 $ac010000 $ac000001 $ac200000 $afffffff ' sw, 1 asm-test5
$e4210001 $e4010000 $e4000001 $e4200000 $e7ffffff 1 ' swcz, 1 asm-test5-copz
$a8210001 $a8010000 $a8000001 $a8200000 $abffffff ' swl, 1 asm-test5
$b8210001 $b8010000 $b8000001 $b8200000 $bbffffff ' swr, 1 asm-test5
$0000000c ' syscall, 1 asm-test0
$42000008 ' tlbl, 1 asm-test0
$42000001 ' tlbr, 1 asm-test0
$42000002 ' tlbwi, 1 asm-test0
$42000006 ' tlbwr, 1 asm-test0
$00210826 $00000826 $00200026 $00010026 $03fff826 ' xor, 1 asm-test5
$38210001 $38010000 $38200000 $38000001 $3bffffff ' xori, 1 asm-test5

$00200821 $00000821 $00200021 $03e0f821 ' move, 1 asm-test4
$00010823 $00200821 $04210002
$00000823 $00000821 $04010002
$00010023 $00200021 $04210002
$001ff823 $03e0f821 $07e10002 ' abs, 3 asm-test4
$00010823 $00000823 $00010023 $001ff823 ' neg, 1 asm-test4
$00010823 $00000823 $00010023 $001ff823 ' negu, 1 asm-test4
$00200827 $00000827 $00200027 $03e0f827 ' not, 1 asm-test4
$14200001 $0021082a $14200000 $0020082a $14200000
$0001082a $14200001 $0000082a $1420ffff $03ff082a ' blt, 2 asm-test5i
$10200001 $0021082a $10200000 $0001082a $10200000
$0020082a $10200001 $0000082a $1020ffff $03ff082a ' ble, 2 asm-test5i
$14200001 $0021082a $14200000 $0001082a $14200000
$0020082a $14200001 $0000082a $1420ffff $03ff082a ' bgt, 2 asm-test5i
$10200001 $0021082b $10200000 $0020082b $10200000
$0001082b $10200001 $0000082b $1020ffff $03ff082b ' bgeu, 2 asm-test5i
$14200001 $0021082b $14200000 $0020082b $14200000
$0001082b $14200001 $0000082b $1420ffff $03ff082b ' bltu, 2 asm-test5i
$10200001 $0021082b $10200000 $0001082b $10200000
$0020082b $10200001 $0000082b $1020ffff $03ff082b ' bleu, 2 asm-test5i
$14200001 $0021082b $14200000 $0001082b $14200000
$0020082b $14200001 $0000082b $1420ffff $03ff082b ' bgtu, 2 asm-test5i
$10200001 $0021082b $10200000 $0020082b $10200000
$0001082b $10200001 $0000082b $1020ffff $03ff082b ' bgeu, 2 asm-test5i

finish
[THEN]
