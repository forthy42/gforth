\ disasm.fs	disassembler file (for ARM64 64-bit mode)
\
\ Copyright (C) 2014 Free Software Foundation, Inc.

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

vocabulary disassembler

disassembler also definitions

: .op4 ( opcode addr u -- ) \ select one of four opcodes
    rot #29 rshift 3 and 4 * /string 4 min type ;
: .op2 ( opcode addr u -- )
    rot #30 rshift 1 and over 2/ * dup >r /string r> min type ;
: .ops ( opcode -- )  #29 rshift 1 and IF  ." s"  THEN ;
: .regsize ( opcode -- )
    $80000000 and 'X' 'W' rot select emit ;
: #.r ( n -- ) \ print decimal
    0 ['] .r #10 base-execute ;
: .rd ( opcode -- )
    dup .regsize $1F and dup $1F = IF  ." SP"  ELSE  #.r  THEN ;
: .rt ( opcode -- ) .rd ',' emit ;
: .rn ( opcode -- )
    dup .regsize #5 rshift $1F and dup $1F = IF  ." SP"  ELSE  #.r  THEN
    ',' emit ;
: .rm ( opcode -- )
    dup .regsize #14 rshift $1F and dup $1F = IF  ." ZR"  ELSE  #.r  THEN
    ',' emit ;
: .imm12 ( opcode -- ) \ print 12 bit immediate with 2 bit shift
    #10 rshift dup $FFF and swap #22 rshift 3 and #12 * lshift '#' emit . ;

: .imm14 ( addr opcode -- addr ) \ print 19 bit branch target
    #5 rshift $3FFF and 2* 2* over + . ;
: .imm19 ( addr opcode -- addr ) \ print 19 bit branch target
    #5 rshift $7FFFF and 2* 2* over + . ;
: .imm26 ( addr opcode -- addr ) \ print 19 bit branch target
    $3FFFFFF and 2* 2* over + . ;
: .cond ( n -- ) $F and
    s" eqnecsccmiplvsvchilsgeltgtlealnv" rot 2* /string 2 min type ;

: .addsub ( opcode -- )
    dup s" addsub" .op2 .ops ;

: unallocated ( opcode -- )
    ." <" 0 .r ." >" ;

\ branches

: .?nz ( opcode -- )
    $01000000 and IF  'n' emit  THEN  'z' emit ;
: .b40 ( opcode -- )  '#' emit
    dup #18 rshift $1F and dup #24 rshift $20 and or #.r ',' emit ;

: condbranch# ( opcode -- )
    ." cb" dup .?nz space dup .rt .imm19 ;
: c&branch# ( opcode -- )
    ." b." dup .cond space .imm19 ;
: ucbranch# ( opcode -- )
    ." b" dup $80000000 and IF 'l' emit  THEN space .imm26 ;
: t&branch# ( opcode -- )
    ." tb" dup .?nz space dup .rt dup .b40 .imm14 ;
    

Create inst-table
\ branches
$14000000 , $7C000000 , ' ucbranch# ,
$34000000 , $7E000000 , ' c&branch# ,
$35000000 , $7E000000 , ' t&branch# ,
$54000000 , $FE000000 , ' condbranch# ,
\ $D4000000 , $FF000000 , ' exceptions ,
\ $D5000000 , $FF000000 , ' system ,
\ $D6000000 , $FE000000 , ' ucbranch ,

$00000000 , $00000000 , ' unallocated ,

: inst ( opcode -- )  inst-table
    BEGIN  2dup 2@ >r and r> <>  WHILE  3 cells +  REPEAT
    2 cells + perform ;

forth definitions

: disasm ( addr u -- ) \ gforth
    [: over + >r
	begin
	    dup r@ u<
	while
		cr dup 10 .r ." : " dup l@ inst 4 +
	repeat
	cr rdrop drop ;] $10 base-execute ;

previous

' disasm is discode

