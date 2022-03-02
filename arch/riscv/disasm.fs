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
: .rs1' ( x -- ) dup 7 rshift 7 and 8 + .reg ;
: .rd' ( x -- )  dup 2 rshift 7 and 8 + .reg ;
: .rfd' ( x -- )  dup 2 rshift 7 and 8 + .freg ;
: imm-1 ( x -- u ) dup 2 rshift $1F and swap 12 5 - rshift $20 and or ;
: imm-1s ( x -- n ) imm-1 dup $20 and negate or ;
: imm-2 ( x -- u ) dup 5 rshift 3 and swap 8 rshift $1C and or 2* ;
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
: c-stw ( x -- ) .$ dup imm-2 2 imm-size 0 .r .( .rs1' .) ., .rd' drop ;
: c-std ( x -- ) .$ dup imm-2 3 imm-size 0 .r .( .rs1' .) ., .rd' drop ;
: c-fstd ( x -- ) .$ dup imm-2 3 imm-size 0 .r .( .rs1' .) ., .rfd' drop ;

\ print registers from instructions, 32 bit ops

: .rd ( x -- x )   dup  7 rshift .reg ;
: .rs1 ( x -- x )  dup 15 rshift .reg ;
: .rs2 ( x -- x )  dup 20 rshift .reg ;
: imm-i ( x -- x imm ) dup l>s 20 arshift ;
: imm-s ( x -- x imm ) dup l>s dup 20 arshift -$20 and swap 7 rshift $1F and or ;
: imm-u ( x -- x imm ) dup l>s -$1000 and ;
: imm-b ( x -- x imm )  imm-s
    dup $000FF000 and >r
    dup $00100000 and 9 rshift >r
    dup $7FE00000 and 20 rshift >r
    -$8000000     and
    r> r> r> or or or ;

: c-addi ( x -- ) .rd ., .rd ., imm-1s 0 .r ;
: c-li ( x -- ) .rd ., imm-1s 0 .r ;
: c-lui ( x -- ) .rd ., imm-1s 12 lshift 0 .r ;
: c-addi16 ( x -- ) .rd ., imm-1s $3F and
    dup 1 and 5 lshift >r 2/
    dup 3 and 8 lshift r> or >r 2/ 2/
    dup 1 and 6 lshift r> or >r 2/
    dup 1 and 4 lshift r> or >r 6 rshift
    1 and negate 9 lshift r> or 0 .r ;

\ different format outputs

: r-type ( x -- ) .rd ., .rs1 ., .rs2 drop ;
: i-type ( x -- ) ;
: l-type ( x -- ) .rd ., .$ imm-i 0 .r .( .rs1 .) drop ;
: s-type ( x -- ) ;
: b-type ( x -- ) ;
: u-type ( x -- ) ;
: j-type ( x -- ) ;

: inst, ( match mask "operation" "name" -- )
    , , ' , parse-name string, align ;

Create inst-table16
$0000 $FFFF inst, drop illegal
$2000 $E003 inst, c-fldd fld
$4000 $E003 inst, c-ldw lw
$6000 $E003 inst, c-ldd ld
$A000 $E003 inst, c-fstd fsd
$C000 $E003 inst, c-stw sw
$E000 $E003 inst, c-std sd
$0001 $EF83 inst, drop nop
$0001 $E003 inst, c-addi addi
$2001 $E003 inst, c-addi addiw
$4001 $E003 inst, c-li li
$6101 $EF83 inst, c-addi16 add16sp
$6001 $E003 inst, c-lui lui
$0000 $0000 inst, hex.4 -/-

Create inst-table32
$00000003 $0000703F inst, l-type lb
$00001003 $0000703F inst, l-type lh
$00002003 $0000703F inst, l-type lw
$00003003 $0000703F inst, l-type ld
$00004003 $0000703F inst, l-type lbu
$00005003 $0000703F inst, l-type lhu
$00006003 $0000703F inst, l-type lwu
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
    [: dup .lformat tab .code ;]
    $10 base-execute ;

: disasm ( addr u -- ) \ gforth
    [: bounds u+do  cr i disline i - +loop  cr ;] $10 base-execute ;

' disasm is discode

previous Forth
