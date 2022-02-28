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

\ register names

: .reg ( n -- )
    case
	0 of  ." zero"  endof
	1 of  ." ra"    endof
	2 of  ." sp"    endof
	3 of  ." gp"    endof
	4 of  ." tp"    endof
	dup  5  8 within ?of   5 - 't' emit 0 .r  endof
	dup  8 10 within ?of   8 - 's' emit 0 .r  endof
	dup 10 18 within ?of  10 - 'a' emit 0 .r  endof
	dup 18 28 within ?of  16 - 's' emit 0 .r  endof
	dup 25 - 't' emit 0 .r
    endcase ;

\ print registers from instructions

: .rd ( x -- )    7 rshift $1F and .reg ;
: .rs1 ( x -- )  15 rshift $1F and .reg ;
: .rs2 ( x -- )  20 rshift $1F and .reg ;
: .imm-i ( x -- ) l>s 20 arshift . ;
: .imm-s ( x -- ) l>s dup 20 arshift -$20 and swap 7 rshift $1F and or . ;

\ different format outputs

: r-type ( x -- ) .rd ., .rs1 ., .rs2 ;
: i-type ( x -- ) ;
: s-type ( x -- ) ;
: b-type ( x -- ) ;
: u-type ( x -- ) ;
: j-type ( x -- ) ;

: .code ( addr -- addr' )
    dup l@ $8 u.r sfloat+ ;

Forth definitions

: disline ( ip -- ip' )
    [: dup .lformat tab .code ;]
    $10 base-execute ;

: disasm ( addr u -- ) \ gforth
    [: bounds u+do  cr i disline i - +loop  cr ;] $10 base-execute ;

' disasm is discode

previous Forth
