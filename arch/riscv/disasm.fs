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
: atom-type ( x -- ) .rd ., .rs2 ., .( .rs1 .) drop ;

: .fence ( n -- )
    $F and s" iorw" bounds DO
	dup 8 and IF  I c@ emit  THEN  2*
    LOOP  drop ;
: fence-type ( x -- )
    dup 24 rshift .fence ., dup 20 rshift .fence drop ; 

: inst, ( match mask operation-xt "name" -- )
    >r , , r> , parse-name string, align ;

: inst: ( mask "operation" name -- )
    ' Create , , DOES> 2@ inst, ;

\ 16 bit instruction types
$FFFF inst: drop c-noarg:
$E003 inst: c-fldd c-fldd:
$E003 inst: c-ldw c-ldw:
$E003 inst: c-ldd c-ldd:
$E003 inst: c-addi c-addi:
$E003 inst: c-li c-li:
$EF83 inst: c-addi16 c-addi16:
$E003 inst: c-lui c-lui:
$EC03 inst: c-andi c-andi:
$FC63 inst: c-and c-and:
$F003 inst: c-add c-add:
$E003 inst: c-j c-j:
$E003 inst: c-beq c-beq:
$E003 inst: c-sli c-sli:
$EC03 inst: c-sri c-sri:
$E003 inst: c-fldsp c-fldsp:
$E003 inst: c-lwsp c-lwsp:
$E003 inst: c-ldsp c-ldsp:
$F07F inst: c-jr c-jr:
$F003 inst: c-mv c-mv:
$E003 inst: c-fsdsp c-fsdsp:
$E003 inst: c-swsp c-swsp:
$E003 inst: c-sdsp c-sdsp:
$0000 inst: hex.4 c-catchall:

\ 32 bit instruction types
$0000007F inst: u-type u-type:
$0000007F inst: u-type-pc u-type-pc:
$0000007F inst: j-type j-type:
$0000707F inst: l-type l-type:
$0000707F inst: b-type b-type:
$0000707F inst: s-type s-type:
$0000707F inst: i-type i-type:
$FC00707F inst: sh-type sh-type:
$FE00707F inst: r-type r-type:
$0000707F inst: fence-type fence-type:
$FFFFFFFF inst: drop noarg-type:
$0000707F inst: csr-type csr-type:
$0000707F inst: csri-type csri-type:
$F800707F inst: atom-type atom-type:
$0000707F inst: fl-type fl-type:
$0600007F inst: fr4-type fr4-type:
$FE00007F inst: fr-type fr-type:
$FFF0007F inst: fr2-type fr2-type:
$FFF0007F inst: fri-type fri-type:
$FFF0007F inst: fir-type fir-type:
$00000000 inst: hex.8 catchall-type:

Create inst-table16
include ./inst16.fs
$0000 c-catchall: -/-

Create inst-table32
include ./inst32.fs
$00000000 catchall-type: -/-

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
