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

: .reg ( n -- )
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

: .freg ( n -- )
    case
	 0  8 ..of  ." ft" 0 .r  endof
	 8 10 ..of  8 - ." fs" 0 .r  endof
	10 18 ..of  10 - ." fa" 0 .r  endof
	18 28 ..of  16 - ." fs" 0 .r  endof
	dup 20 - ." ft" 0 .r
    endcase ;

\ print registers from instructions

: .rd ( x -- x )   dup  7 rshift $1F and .reg ;
: .rs1 ( x -- x )  dup 15 rshift $1F and .reg ;
: .rs2 ( x -- x )  dup 20 rshift $1F and .reg ;
: imm-i ( x -- x imm ) dup l>s 20 arshift ;
: imm-s ( x -- x imm ) dup l>s dup 20 arshift -$20 and swap 7 rshift $1F and or ;
: imm-u ( x -- x imm ) dup l>s -$1000 and ;
: imm-b ( x -- x imm )  imm-s
    dup $000FF000 and >r
    dup $00100000 and 9 rshift >r
    dup $7FE00000 and 20 rshift >r
    -$8000000     and
    r> r> r> or or or ;

\ different format outputs

: r-type ( x -- ) .rd ., .rs1 ., .rs2 drop ;
: i-type ( x -- ) ;
: l-type ( x -- ) .rd ., imm-i 0 .r .( .rs1 .) drop ;
: s-type ( x -- ) ;
: b-type ( x -- ) ;
: u-type ( x -- ) ;
: j-type ( x -- ) ;

: inst, ( match mask "operation" "name" -- )
    , , ' , parse-name string, align ;

Create inst-table16
$0000 $0000 inst, hex.4 reserved

Create inst-table32
$00000003 $0000703F inst, l-type lb
$00001003 $0000703F inst, l-type lh
$00002003 $0000703F inst, l-type lw
$00003003 $0000703F inst, l-type ld
$00004003 $0000703F inst, l-type lbu
$00005003 $0000703F inst, l-type lhu
$00006003 $0000703F inst, l-type lwu
$00000000 $00000000 inst, hex.8 reserved

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
