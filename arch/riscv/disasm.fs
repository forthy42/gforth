\ RISC-V disassembler

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

Vocabulary disassembler

disassembler also definitions

: .lformat   ( addr -- )  $C u.r ." :" ;
: tab #tab emit ;
: .,  ',' emit ;
: .(  '(' emit ;
: .)  ')' emit ;
: .$  '$' emit ;
: hex.4 ( inst -- )
    0 <# # # # # #> type ;
: hex.8 ( inst -- )
    0 <# # # # # # # # # #> type ;

\ register names

: ..of ( compilation  -- of-sys ; run-time x1 x2 x3 -- x1 ) \ core-ext
    \g if x1 is within x2 and x3, continue (dropping x2 and x3); otherwise,
    \g leave x1 on the stack and jump behind @code{endof} or @code{contof}.
    lits# 2 u>= IF  lits> lits> postpone dup >lits >lits
    ELSE  ]] third -rot [[  THEN  ]] within ?of [[ ; immediate

: .reg ( n -- ) $1F and
    case
	0 of  ." zero"  endof
	1 of  ." ra"    endof
	2 of  ." sp"    endof
	3 of  ." gp"    endof
	4 of  ." tp"    endof
	 5  8 ..of   5 - 't' emit 0 .r  endof
	 8 10 ..of   8 - 's' emit 0 .r  endof
	10 18 ..of  10 - 'a' emit 0 .r  endof
	18 28 ..of  16 - 's' emit 0 ['] .r #10 base-execute  endof
	dup 25 - 't' emit 0 .r
    endcase ;

: .freg ( n -- ) $1F and
    case
	 0  8 ..of  ." ft" 0 .r  endof
	 8 10 ..of  8 - ." fs" 0 .r  endof
	10 18 ..of  10 - ." fa" 0 .r  endof
	18 28 ..of  16 - ." fs" 0 .r  endof
	dup 20 - ." ft" 0 .r
    endcase ;

\ print registers from instructions, 16 bit ops

: .rs0 ( x -- )  dup 2 rshift .reg ;
: .rfs0 ( x -- )  dup 2 rshift .freg ;
: .rs1' ( x -- ) dup 7 rshift 7 and 8 + .reg ;
: .rd' ( x -- )  dup 2 rshift 7 and 8 + .reg ;
: .rfd' ( x -- )  dup 2 rshift 7 and 8 + .freg ;
: imm-1 ( x -- u ) dup 2 rshift $1F and swap 12 5 - rshift $20 and or ;
: imm-1s ( x -- n ) imm-1 dup $20 and negate or ;
: imm-2 ( x -- u ) dup 5 rshift 3 and swap 8 rshift $1C and or 2* ;
: imm-3 ( x -- u ) dup 7 rshift $3F and ;
: imm-size ( imm size -- )
    -1 swap lshift >r dup r@ invert and 6 lshift or r> and ;
: offset ( x -- )  2 rshift
    \ offset[11|4|9:8|10|6|7|3:1|5]
    dup 1 and 5 lshift >r 2/
    dup 7 and 1 lshift r> or >r 2/ 2/ 2/
    dup 1 and 7 lshift r> or >r 2/
    dup 1 and 6 lshift r> or >r 2/
    dup 1 and 10 lshift r> or >r 2/
    dup 3 and 8 lshift r> or >r 2/ 2/
    dup 1 and 4 lshift r> or >r 2/
    1 and 11 lshift r> or
    dup $800 and negate or ;
: offset' ( x -- )  2 rshift
    \  offset[8|4:3] src' offset[7:6|2:1|5] op
    dup 1 and 5 lshift >r 2/
    dup 3 and 1 lshift r> or >r 2/ 2/
    dup 3 and 6 lshift r> or >r 5 rshift
    dup 3 and 3 lshift r> or >r
    1 and 8 lshift r> or
    dup $100 and negate or ;

: c-ldw ( x -- ) .rd' ., .$ dup imm-2 2 imm-size 0 .r .( .rs1' .) drop ;
: c-ldd ( x -- ) .rd' ., .$ dup imm-2 3 imm-size 0 .r .( .rs1' .) drop ;
: c-fldd ( x -- ) .rfd' ., .$ dup imm-2 3 imm-size 0 .r .( .rs1' .) drop ;

\ print registers from instructions, 32 bit ops

: .rd ( x -- x )   dup  7 rshift .reg ;
: .rs1 ( x -- x )  dup 15 rshift .reg ;
: .rs2 ( x -- x )  dup 20 rshift .reg ;
: .rfd ( x -- x )   dup  7 rshift .freg ;
: .rfs1 ( x -- x )  dup 15 rshift .freg ;
: .rfs2 ( x -- x )  dup 20 rshift .freg ;
: .rfs3 ( x -- x )  dup 27 rshift .freg ;
: imm-i ( x -- x imm ) dup l>s 20 arshift ;
: imm-s ( x -- x imm ) dup l>s dup 20 arshift -$20 and
    swap 7 rshift $1F and or ;
: imm-b ( x -- x imm ) imm-s
    dup 1 and 11 lshift >r -2 and $800 invert and r> or ;
: imm-u ( x -- x imm ) dup l>s -$1000 and ;
: imm-j ( x -- x imm )  imm-u
    dup $000FF000 and >r
    dup $00100000 and 9 rshift >r
    dup $7FE00000 and 20 rshift >r
    12 arshift -$80000 and
    r> r> r> or or or ;

: c-addi ( x -- ) .rd ., .rd ., imm-1s 0 .r ;
: c-sli ( x -- ) .rd ., .rd ., imm-1 0 .r ;
: c-andi ( x -- ) .rd' ., .rd' ., imm-1s 0 .r ;
: c-sri ( x -- ) .rd' ., .rd' ., imm-1 0 .r ;
: c-and ( x -- ) .rd' ., .rs1' ., .rd' drop ;
: c-li ( x -- ) .rd ., imm-1s 0 .r ;
: c-lui ( x -- ) .rd ., imm-1s 12 lshift 0 .r ;
: c-addi16 ( x -- ) .rd ., imm-1s $3F and
    dup 1 and 5 lshift >r 2/
    dup 3 and 8 lshift r> or >r 2/ 2/
    dup 1 and 6 lshift r> or >r 2/
    dup 1 and 4 lshift r> or >r 6 rshift
    1 and negate 9 lshift r> or 0 .r ;
: c-j ( addr x -- addr ) offset over + 0 .r ;
: c-beq ( addr x -- addr )
    .rd' ., offset' over + 0 .r ;
: c-ldsp ( x -- )
    .rd ., imm-1 3 imm-size 0 .r .( 2 .reg .) ;
: c-lwsp ( x -- )
    .rd ., imm-1 2 imm-size 0 .r .( 2 .reg .) ;
: c-fldsp ( x -- )
    .rfd ., imm-1 3 imm-size 0 .r .( 2 .reg .) ;
: c-flwsp ( x -- )
    .rfd ., imm-1 2 imm-size 0 .r .( 2 .reg .) ;

: c-sdsp ( x -- )
    .rs0 ., .$ imm-1 3 imm-size 0 .r .( 2 .reg .) ;
: c-swsp ( x -- )
    .rs0 ., .$ imm-1 2 imm-size 0 .r .( 2 .reg .) ;
: c-fsdsp ( x -- )
    .rfs0 ., .$ imm-1 3 imm-size 0 .r .( 2 .reg .) ;
: c-fswsp ( x -- )
    .rfs0 ., .$ imm-1 2 imm-size 0 .r .( 2 .reg .) ;

: c-jr ( x -- ) .rd drop ;
: c-mv ( x -- ) .rd ., .rs0 drop ;
: c-add ( x -- ) .rd ., .rd ., .rs0 drop ;

\ different format outputs

: r-type ( x -- ) .rd ., .rs1 ., .rs2 drop ;
: fr-type ( x -- ) .rfd ., .rfs1 ., .rfs2 drop ;
: fr2-type ( x -- ) .rfd ., .rfs1 drop ;
: fri-type ( x -- ) .rd ., .rfs1 drop ;
: fir-type ( x -- ) .rfd ., .rs1 drop ;
: fr4-type ( x -- ) .rfd ., .rfs1 ., .rfs2 ., .rfs3 drop ;
: sh-type ( x -- ) .rd ., .rs1 ., .$ 20 rshift $3F and 0 .r ;
: i-type ( x -- ) .rd ., .rs1 ., .$ imm-i 0 .r drop ;
: l-type ( x -- ) .rd ., .$ imm-i 0 .r .( .rs1 .) drop ;
: fl-type ( x -- ) .rfd ., .$ imm-i 0 .r .( .rs1 .) drop ;
: s-type ( x -- ) .rs2 ., .$ imm-s 0 .r .( .rs1 .) drop ;
: fs-type ( x -- ) .rfs2 ., .$ imm-s 0 .r .( .rs1 .) drop ;
: b-type ( x -- ) .rs1 ., .rs2 ., .$ imm-b nip over + 0 .r ;
: u-type ( x -- ) .rd ., .$ imm-u 0 .r drop ;
: u-type-pc ( addr x -- addr ) .rd ., .$ imm-u nip over + 0 .r ;
: j-type ( addr x -- addr ) .rd ., .$ imm-j nip over + 0 .r ;
: csr-type ( x -- ) .rd ., .rs1 ., .$ imm-i 0 .r drop ;
: csri-type ( x -- ) .rd ., .$ dup 15 rshift $1F and 0 .r ., .$ imm-i 0 .r drop ;
: cond-type ( x -- ) .rd ., .rs2 ., .( .rs1 .) drop ;

: .fence ( n -- )
    $F and s" iorw" bounds DO
	dup 8 and IF  I c@ emit  THEN  2*
    LOOP  drop ;
: fence-type ( x -- )
    dup 24 rshift .fence ., dup 20 rshift .fence drop ; 

: inst, ( match mask "operation" "name" -- )
    , , ' , parse-name string, align ;
Create inst-table16
$0000 $FFFF inst, drop illegal
$2000 $E003 inst, c-fldd fld
$4000 $E003 inst, c-ldw lw
$6000 $E003 inst, c-ldd ld
$A000 $E003 inst, c-fldd fsd
$C000 $E003 inst, c-ldw sw
$E000 $E003 inst, c-ldd sd

$0001 $EF83 inst, drop nop
$0001 $E003 inst, c-addi addi
$2001 $E003 inst, c-addi addiw
$4001 $E003 inst, c-li li
$6101 $EF83 inst, c-addi16 add16sp
$6001 $E003 inst, c-lui lui
$8001 $EC03 inst, c-sri srli
$8401 $EC03 inst, c-sri srai
$8801 $EC03 inst, c-andi andi
$8C01 $FC63 inst, c-and sub
$8C21 $FC63 inst, c-and xor
$8C41 $FC63 inst, c-and or
$8C61 $FC63 inst, c-and and
$9C01 $FC63 inst, c-and subw
$9C21 $FC63 inst, c-and addw
$A001 $E003 inst, c-j j
$C001 $E003 inst, c-beq beqz
$E001 $E003 inst, c-beq bnez

$0002 $E003 inst, c-sli slli
$2002 $E003 inst, c-fldsp fldsp
$4002 $E003 inst, c-lwsp lwsp
$6002 $E003 inst, c-ldsp ldsp
$8002 $F07F inst, c-jr jr
$8002 $F003 inst, c-mv mv
$9002 $FFFF inst, drop ebreak
$9002 $F07F inst, c-jr jalr
$9002 $F003 inst, c-add add
$A002 $E003 inst, c-fsdsp fsdsp
$C002 $E003 inst, c-swsp swsp
$E002 $E003 inst, c-sdsp sdsp
$0000 $0000 inst, hex.4 -/-

Create inst-table32
$00000037 $0000007F inst, u-type lui
$00000017 $0000007F inst, u-type-pc auipc
$0000006F $0000007F inst, j-type jal
$00000067 $0000707F inst, l-type jalr
$00000063 $0000707F inst, b-type beq
$00001063 $0000707F inst, b-type bne
$00004063 $0000707F inst, b-type blt
$00005063 $0000707F inst, b-type bge
$00006063 $0000707F inst, b-type bltu
$00007063 $0000707F inst, b-type bgeu
$00000003 $0000707F inst, l-type lb
$00001003 $0000707F inst, l-type lh
$00002003 $0000707F inst, l-type lw
$00003003 $0000707F inst, l-type ld
$00004003 $0000707F inst, l-type lbu
$00005003 $0000707F inst, l-type lhu
$00006003 $0000707F inst, l-type lwu
$00000023 $0000707F inst, s-type sb
$00001023 $0000707F inst, s-type sh
$00002023 $0000707F inst, s-type sw
$00003023 $0000707F inst, s-type sd
$00000013 $0000707F inst, i-type addi
$00002013 $0000707F inst, i-type slti
$00003013 $0000707F inst, i-type sltiu
$00004013 $0000707F inst, i-type xori
$00006013 $0000707F inst, i-type ori
$00007013 $0000707F inst, i-type andi
$00001013 $FC00707F inst, sh-type slli
$00005013 $FC00707F inst, sh-type srli
$40005013 $FC00707F inst, sh-type srai
$0000101B $FC00707F inst, sh-type slliw
$0000501B $FC00707F inst, sh-type srliw
$4000501B $FC00707F inst, sh-type sraiw
$00000033 $FE00707F inst, r-type add
$40000033 $FE00707F inst, r-type sub
$00001033 $FE00707F inst, r-type sll
$00002033 $FE00707F inst, r-type slt
$00003033 $FE00707F inst, r-type sltu
$00004033 $FE00707F inst, r-type xor
$00005033 $FE00707F inst, r-type srl
$40005033 $FE00707F inst, r-type sra
$00006033 $FE00707F inst, r-type or
$00007033 $FE00707F inst, r-type and
$0000003B $FE00707F inst, r-type addw
$4000003B $FE00707F inst, r-type subw
$0000103B $FE00707F inst, r-type sllw
$0000503B $FE00707F inst, r-type srlw
$4000503B $FE00707F inst, r-type sraw
$0000000F $0000707F inst, fence-type fence
$0000100F $0000707F inst, fence-type fence.i \ Zifencei
$00000073 $FFFFFFFF inst, drop ecall
$00100073 $FFFFFFFF inst, drop ebreak
$00001073 $0000707F inst, csr-type csrrw \ Zicsr
$00002073 $0000707F inst, csr-type csrrs
$00003073 $0000707F inst, csr-type csrrc
$00005073 $0000707F inst, csri-type csrrwi
$00006073 $0000707F inst, csri-type csrrsi
$00007073 $0000707F inst, csri-type csrrci
$02000033 $FE00707F inst, r-type mul \ multiplication&division
$02001033 $FE00707F inst, r-type mulh
$02002033 $FE00707F inst, r-type mulhsu
$02003033 $FE00707F inst, r-type mulhu
$02004033 $FE00707F inst, r-type div
$02005033 $FE00707F inst, r-type divu
$02006033 $FE00707F inst, r-type rem
$02007033 $FE00707F inst, r-type remu
$0200003B $FE00707F inst, r-type mulw
$0200403B $FE00707F inst, r-type divw
$0200503B $FE00707F inst, r-type divuw
$0200603B $FE00707F inst, r-type remw
$0200703B $FE00707F inst, r-type remuw

$0000202F $F800707F inst, cond-type amoadd.w
$0800202F $F800707F inst, cond-type amoswap.w
$1000202F $F800707F inst, cond-type lr.w
$1800202F $F800707F inst, cond-type sc.w
$2000202F $F800707F inst, cond-type amoxor.w
$4000202F $F800707F inst, cond-type amoor.w
$6000202F $F800707F inst, cond-type amoand.w
$8000202F $F800707F inst, cond-type amomin.w
$A000202F $F800707F inst, cond-type amomax.w
$C000202F $F800707F inst, cond-type amominu.w
$E000202F $F800707F inst, cond-type amomaxu.w

$0000302F $F800707F inst, cond-type amoadd.d
$0800302F $F800707F inst, cond-type amoswap.d
$1000302F $F800707F inst, cond-type lr.d
$1800302F $F800707F inst, cond-type sc.d
$2000302F $F800707F inst, cond-type amoxor.d
$4000302F $F800707F inst, cond-type amoor.d
$6000302F $F800707F inst, cond-type amoand.d
$8000302F $F800707F inst, cond-type amomin.d
$A000302F $F800707F inst, cond-type amomax.d
$C000302F $F800707F inst, cond-type amominu.d
$E000302F $F800707F inst, cond-type amomaxu.d

$00002007 $0000707F inst, fl-type flw
$00002027 $0000707F inst, fl-type fsw
$00000043 $0600007F inst, fr4-type fmadd.s
$00000047 $0600007F inst, fr4-type fmsub.s
$0000004B $0600007F inst, fr4-type fnmsub.s
$0000004F $0600007F inst, fr4-type fnmadd.s
$00000053 $FE00007F inst, fr-type fadd.s
$08000053 $FE00007F inst, fr-type fsub.s
$10000053 $FE00007F inst, fr-type fmul.s
$18000053 $FE00007F inst, fr-type fdiv.s
$58000053 $FFF0007F inst, fr2-type fsrqt.s
$20000053 $FE00707F inst, fr-type fsgnj.s
$20001053 $FE00707F inst, fr-type fsgnjn.s
$20002053 $FE00707F inst, fr-type fsgnjx.s
$28000053 $FE00707F inst, fr-type fmin.s
$28001053 $FE00707F inst, fr-type fmax.s
$C0000053 $FFF0007F inst, fri-type fcvt.w.s
$C0100053 $FFF0007F inst, fri-type fcvt.wu.s
$C0200053 $FFF0007F inst, fri-type fcvt.l.s
$C0300053 $FFF0007F inst, fri-type fcvt.lu.s
$E0000053 $FFF0707F inst, fir-type fmv.x.w
$A0000053 $FE00707F inst, fr-type fle.s
$A0001053 $FE00707F inst, fr-type flt.s
$A0002053 $FE00707F inst, fr-type feq.s
$E0001053 $FFF0707F inst, fr-type fclass.s
$D0000053 $FFF0007F inst, fir-type fcvt.s.w
$D0100053 $FFF0007F inst, fir-type fcvt.s.wu
$D0200053 $FFF0007F inst, fir-type fcvt.s.l
$D0300053 $FFF0007F inst, fir-type fcvt.s.lu
$F0000053 $FFF0707F inst, fri-type fmv.w.x

$00003007 $0000707F inst, fl-type fld
$00003027 $0000707F inst, fl-type fsd
$02000043 $0600007F inst, fr4-type fmadd.d
$02000047 $0600007F inst, fr4-type fmsub.d
$0200004B $0600007F inst, fr4-type fnmsub.d
$0200004F $0600007F inst, fr4-type fnmadd.d
$02000053 $FE00007F inst, fr-type fadd.d
$0A000053 $FE00007F inst, fr-type fsub.d
$12000053 $FE00007F inst, fr-type fmul.d
$1A000053 $FE00007F inst, fr-type fdiv.d
$5A000053 $FFF0007F inst, fr2-type fsrqt.d
$22000053 $FE00707F inst, fr-type fsgnj.d
$22001053 $FE00707F inst, fr-type fsgnjn.d
$22002053 $FE00707F inst, fr-type fsgnjx.d
$2A000053 $FE00707F inst, fr-type fmin.d
$2A001053 $FE00707F inst, fr-type fmax.d
$C2000053 $FFF0007F inst, fri-type fcvt.w.d
$C2100053 $FFF0007F inst, fri-type fcvt.wu.d
$C2200053 $FFF0007F inst, fri-type fcvt.l.d
$C2300053 $FFF0007F inst, fri-type fcvt.lu.d
$E2000053 $FFF0707F inst, fir-type fmv.x.d
$A2000053 $FE00707F inst, fr-type fle.d
$A2001053 $FE00707F inst, fr-type flt.d
$A2002053 $FE00707F inst, fr-type feq.d
$E2001053 $FFF0707F inst, fr-type fclass.d
$D2000053 $FFF0007F inst, fir-type fcvt.d.w
$D2100053 $FFF0007F inst, fir-type fcvt.d.wu
$D2200053 $FFF0007F inst, fir-type fcvt.d.l
$D2300053 $FFF0007F inst, fir-type fcvt.d.lu
$F2000053 $FFF0707F inst, fri-type fmv.d.x

$00000000 $00000000 inst, hex.8 -/-

: .inst ( inst table -- ) swap >r
    BEGIN  dup 2@ r@ and <>  WHILE
	    3 cells + count + aligned  REPEAT
    dup 3 cells + count type tab
    r> swap 2 cells + perform ;

: .code ( addr -- addr' )
    dup c@ $3 and 3 = IF
	dup l@ inst-table32 .inst sfloat+
    ELSE
	dup w@ inst-table16 .inst 2 +
    THEN ;

Forth definitions

: disline ( ip -- ip' )
    [: dup .lformat tab .code ;] $10 base-execute ;

: disasm ( addr u -- ) \ gforth
    bounds u+do  cr i disline i - +loop  cr ;

' disasm is discode

previous Forth
