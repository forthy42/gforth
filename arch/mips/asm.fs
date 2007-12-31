\ asm.fs	assembler file (for MIPS R3000)
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

\ test this with
\ gforth arch/mips/asm.fs -e "also assembler here" arch/mips/testasm.fs -e "here over - here" arch/mips/testdisasm.fs -e "here over - compare throw bye"

require ./../../code.fs

get-current
also assembler definitions

$20 constant asm-registers

\ register names
0 constant $zero
1 constant $at
2 constant $v0
3 constant $v1
\ 4 constant $a0 \ commented out to avoid shadowing hex numbers
\ 5 constant $a1
\ 6 constant $a2
\ 7 constant $a3
8 constant $t0
9 constant $t1
10 constant $t2
11 constant $t3
12 constant $t4
13 constant $t5
14 constant $t6
15 constant $t7
16 constant $s0
17 constant $s1
18 constant $s2
19 constant $s3
20 constant $s4
21 constant $s5
22 constant $s6
23 constant $s7
24 constant $t8
25 constant $t9
26 constant $k0
27 constant $k1
28 constant $gp
29 constant $sp
30 constant $s8
31 constant $ra

$00 constant asm-init-code

$1F constant asm-bm05
$3F constant asm-bm06
$FFFF constant asm-bm10
$3FFFFFF constant asm-bm1A

: asm-op ( n -- code )
    asm-bm06 and $1a lshift ;

: check-range ( u1 u2 u3 -- )
    within 0= -24 and throw ;

: asm-rs ( u code -- code )
    over 0 $20 check-range
    swap $15 lshift or ;

: asm-rt ( n code -- code )
    over 0 $20 check-range
    swap $10 lshift or ;

: asm-imm ( n code -- code )
    over -$8000 $8000 check-range
    swap $ffff and or ;
' asm-imm alias asm-offset

: asm-uimm ( u code -- code )
    over 0 $10000 check-range
    or ;

: asm-rel ( n code -- code )
    over 3 and 0<> -24 and throw \ check lower 2 bits
    swap 2/ 2/ swap asm-imm ;

: asm-target ( n code -- code )
    over here cell+ xor $f0000003 and 0<> -24 and throw
    swap 2 rshift asm-bm1A and or ;

: asm-rd ( n code -- code )
    over 0 $20 check-range
    swap $b lshift or ;

: asm-shamt ( n code -- code )
    over 0 $20 check-range
    swap $6 lshift or ;
' asm-shamt alias asm-sa

: asm-funct ( n code -- code )
    swap asm-bm06 and or ;

: asm-special ( code1 -- code2 )
    asm-init-code asm-funct ;

\ ***** I-types
: asm-I-rt,imm ( code -- )
    create ,
does> ( rt imm -- )
    @ asm-imm asm-rt , ;

: asm-I-rt,uimm ( code -- )
    create ,
does> ( rt uimm -- )
    @ asm-uimm asm-rt , ;

: asm-I-rs,imm ( code -- )
    create ,
does> ( rs imm -- )
    @ asm-rel asm-rs , ;

: asm-I-rt,rs,imm ( code -- )
    create ,
does> ( rt rs imm -- )
    @ asm-imm asm-rs asm-rt , ;

: asm-I-rt,rs,uimm ( code -- )
    create ,
does> ( rt rs uimm -- )
    @ asm-uimm asm-rs asm-rt , ;

: asm-I-rs,rt,imm ( code -- )
    create ,
does> ( rs rt imm -- )
    @ asm-rel asm-rt asm-rs , ;

: asm-I-rt,offset,rs ( code -- )
    create ,
does> ( rt offset rs -- )
    @ asm-rs asm-offset asm-rt , ;

\ ***** regimm types
: asm-regimm-rs,imm ( funct -- )
    $01 asm-op asm-rt asm-I-rs,imm ;

\ ***** copz types 1

: asm-I-imm,z ( code -- )
    create ,
does> ( imm z -- )
    @ swap asm-op or asm-rel , ;

: asm-copz-imm ( code -- )
    $10 asm-op or asm-I-imm,z ;

: asm-I-rt,offset,rs,z ( code -- )
    create ,
does> ( rt offset rs z -- )
    @ swap asm-op or asm-rs asm-offset asm-rt , ;

: asm-copz-rt,offset,rs ( code -- )
    asm-op asm-I-rt,offset,rs,z ;

: asm-J-target ( code -- )
    create ,
does> ( target -- )
    @ asm-target , ;

\ ***** special types
: asm-special-nothing ( code -- )
    asm-special create ,
does> ( addr -- )
    @ , ;

: asm-special-rd ( code -- )
    asm-special create ,
does> ( rd addr -- )
    @ asm-rd , ;

: asm-special-rs ( code -- )
    asm-special create ,
does> ( rs addr -- )
    @ asm-rs , ;

: asm-special-rd,rs ( code -- )
    asm-special create ,
does> ( rd rs addr -- )
    @ asm-rs asm-rd , ;

: asm-special-rs,rt ( code -- )
    asm-special create ,
does> ( rs rt addr -- )
    @ asm-rt asm-rs , ;

: asm-special-rd,rs,rt ( code -- )
    asm-special create ,
does> ( rd rs rt addr -- )
    @ asm-rt asm-rs asm-rd , ;

: asm-special-rd,rt,rs ( code -- )
    asm-special create ,
does> ( rd rt rs addr -- )
    @ asm-rs asm-rt asm-rd , ;

: asm-special-rd,rt,sa ( code -- )
    asm-special create ,
does> ( rd rt sa addr -- )
    @ asm-sa asm-rt asm-rd , ;

\ ***** copz types 2
: asm-copz0 ( funct -- )
    $10 $10 asm-op asm-rs asm-funct create ,
does> ( addr -- )
    @ , ;

: asm-copz-rt,rd ( funct -- )
    $10 asm-op or create ,
does> ( rt rd z addr -- )
    @ swap asm-op or asm-rd asm-rt , ;

: nop, ( -- )
    0 , ;

include ./insts.fs

: move, ( rd rs -- )
    $zero addu, ;

\ commented out to reduce delay slot exceptions
\  : abs, ( rd rs -- )
\      dup $0008 bgez,
\      2dup move,
\      $zero swap subu, ;

: neg, ( rd rs -- )
    $zero swap subu, ;

: negu, ( rd rs -- )
    $zero swap subu, ;

: not, ( rd rs -- )
    $zero nor, ;

: li, ( rd imm -- )
    dup 0= if
	drop dup $zero = if
	    drop nop, assert( false )
	else
	    $zero move,
	endif
    else
	dup $8000 u< if
	    $zero swap addiu,
	else
	    dup $10000 u< if
		$zero swap ori,
	    else
		dup $ffff and 0= if
		    $10 rshift lui,
		else
		    dup $ffff8000 and $ffff8000 = if
			$zero swap addiu,
		    else
			2dup $10 rshift lui,
			over swap ori,
		    endif
		endif
	    endif
	endif
    endif ;

: blt, ( rs rt imm -- )		\ <
    >r $at rot rot slt,
    $at $zero r> bne, ;

: ble, ( rs rt imm -- )		\ <=
    >r $at rot rot swap slt,
    $at $zero r> beq, ;

: bgt, ( rs rt imm -- )		\ >
    >r $at rot rot swap slt,
    $at $zero r> bne, ;

: bge, ( rs rt imm -- )		\ >=
    >r $at rot rot slt,
    $at $zero r> beq, ;

: bltu, ( rs rt imm -- )	\ < unsigned
    >r $at rot rot sltu,
    $at $zero r> bne, ;

: bleu, ( rs rt imm -- )	\ <= unsigned
    >r $at rot rot swap sltu,
    $at $zero r> beq, ;

: bgtu, ( rs rt imm -- )	\ > unsigned
    >r $at rot rot swap sltu,
    $at $zero r> bne, ;

: bgeu, ( rs rt imm -- )	\ >= unsigned
    >r $at rot rot sltu,
    $at $zero r> beq, ;

\ control structures

\ conditions; they are reversed because of the if and until logic (the
\ stuff enclosed by if is performed if the branch around has the
\ inverse condition, cf. 0branch).

' beq,  constant ne
' bne,  constant eq
' blez, constant gtz
' bgtz, constant lez
' bltz, constant gez
' bgez, constant ltz
\ bczf, bczt, \ these don't take the relative address as last argument
' blt,  constant ge
' ble,  constant gt
' bgt,  constant le
' bge,  constant lt
' bltu, constant geu
' bleu, constant gtu
' bgtu, constant leu
' bgeu, constant ltu

\ an asm-cs-item consists of ( addr magic1 magic2 ).  addr is the
\ address behind the branch or the destination. magic2 is LIVE-ORIG or
\ DEST xored with asm-magic to make it harder to confuse with a
\ register number or immediate value. magic1 is LIVE-orig or DEST.
\ It's there to make CS-ROLL etc. work.

: magic-asm ( u1 u2 -- u3 u4 )
    \ turns a magic number into an asm-magic number or back
    $87654321 xor ;

: patch-branch ( branch-delay-addr target-addr -- )
    \ there is a branch just before branch-delay-addr; PATCH-BRANCH
    \ patches this branch to branch to target-addr
    over - ( branch-delay-addr rel )
    swap cell - dup >r ( rel branch-addr R:branch-addr )
    @ asm-rel r> ! ; \ !! relies on the imm field being 0 before

: if, ( ... xt -- asm-orig )
    \ xt is for a branch word ( ... addr -- )
    0 swap execute
    here live-orig magic-asm live-orig ;

: ahead, ( -- asm-orig )
    $zero $zero ne if, ;

: then, ( asm-orig -- )
    orig? magic-asm orig?
    here patch-branch ;

: begin, ( -- asm-dest )
    here dest magic-asm dest ;

: until, ( asm-dest ... xt -- )
    \ xt is a condition and ... are its arguments
    0 swap execute
    dest? magic-asm dest?
    here swap patch-branch ;

: again, ( asm-dest -- )
    $zero $zero ne until, ;

: while, ( asm-dest -- asm-orig asm-dest )
    if, 1 cs-roll ;

: delayed-then, ( asm-orig -- )
    \ set the target of asm-orig to one instruction after the current one
    0 , then, -1 cells allot ;

: else, ( asm-orig1 -- asm-orig2 )
    ahead, 1 cs-roll delayed-then, ;

: repeat, ( asm-orig asm-dest -- )
    again, delayed-then, ;

: endif, ( asm-orig -- )
    then, ;

previous
set-current
