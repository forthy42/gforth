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

: .1" ( addr u opcode -- ) \ print substring by 1
    safe/string 1 min type ;
: .2" ( addr u opcode -- ) \ print substring by 2
    2* safe/string 2 min type ;
: .3" ( addr u opcode -- ) \ print substring by 3
    3 * safe/string 3 min type ;
: .4" ( addr u opcode -- ) \ print substring by 4
    4 * safe/string 4 min type ;
: .5" ( addr u opcode -- ) \ print substring by 5
    5 * safe/string 5 min type ;
: .op4 ( opcode addr u -- ) \ select one of four opcodes
    rot #29 rshift 3 and .4" ;
: .op2 ( opcode addr u -- )
    rot #30 rshift 1 and IF  dup 2/ /string  ELSE  2/  THEN  type ;
: .ops ( opcode -- )  #29 rshift 1 and IF  ." s"  THEN ;
: .regsize ( opcode -- )
    $80000000 and 'X' 'W' rot select emit ;
: #.r ( n -- ) \ print decimal
    0 ['] .r #10 base-execute ;
: ., ( -- ) ',' emit ;
: .rd ( opcode -- )
    dup .regsize $1F and dup $1F = IF  ." SP"  ELSE  #.r  THEN ;
: .rn ( opcode -- )
    dup .regsize #5 rshift $1F and dup $1F = IF  ." SP"  ELSE  #.r  THEN ;
: .rm ( opcode -- )
    dup .regsize #14 rshift $1F and dup $1F = IF  ." ZR"  ELSE  #.r  THEN ;
: .imm12 ( opcode -- ) \ print 12 bit immediate with 2 bit shift
    #10 rshift dup $FFF and swap #22 rshift 3 and #12 * lshift '#' emit . ;
: .imm16 ( opcode -- ) \ print 16 bit immediate
    #5 rshift $FFFF and '#' emit . ;
: .lsl ( opcode -- ) \ print shift
    #21 rshift $3 and #4 lshift ?dup-IF  ." , lsl #$" .  THEN ;

: .imm14 ( addr opcode -- addr ) \ print 19 bit branch target
    #5 rshift $3FFF and 2* 2* over + . ;
: .imm19 ( addr opcode -- addr ) \ print 19 bit branch target
    #5 rshift $7FFFF and 2* 2* over + . ;
: .imm26 ( addr opcode -- addr ) \ print 19 bit branch target
    $3FFFFFF and 2* 2* over + . ;
: .cond ( n -- ) $F and
    s" eqnecsccmiplvsvchilsgeltgtlealnv" rot .2" ;

: unallocated ( opcode -- )
    ." <" 0 .r ." >" ;

\ branches

: .?nz ( opcode -- )
    $01000000 and IF  'n' emit  THEN  'z' emit ;
: .b40 ( opcode -- )  '#' emit
    dup #18 rshift $1F and dup #24 rshift $20 and or #.r ',' emit ;

: condbranch# ( opcode -- )
    ." cb" dup .?nz space dup .rd ., .imm19 ;
: c&branch# ( opcode -- )
    ." cb" dup .cond space .imm19 ;
: ucbranch# ( opcode -- )
    ." b" dup $80000000 and IF 'l' emit  THEN space .imm26 ;
: t&branch# ( opcode -- )
    ." tb" dup .?nz space dup .rd ., dup .b40 .imm14 ;
: >opc ( opcode -- opc ) #21 rshift $7 and ;
: exceptions ( opcode -- )
    case  dup >opc
	0 of
	    dup $1F and dup 1 4 within IF
		s" svchvcsmc" rot 1- .3"
		space  .imm16
	    ELSE  unallocated  THEN  endof
	1 of  dup $1F and 0= IF  ." brk " .imm16  ELSE  unallocated  THEN  endof
	2 of  dup $1F and 0= IF  ." hlt " .imm16  ELSE  unallocated  THEN  endof
	5 of  dup $1F and 1 4 within IF  ." dcps" dup $1F and . .imm16
	    ELSE  unallocated  THEN  endof
	swap unallocated
    endcase ;
: ucbranch ( opcode -- )
    dup >opc dup #5 u> IF  drop unallocated
    ELSE  s" br  blr ret eretdrps" rot .4" space .rn  THEN ;

\ data processing, immediate

: .immrs ( opcode -- )
    '#' emit dup #22 rshift 1 and 0 .r .,
    dup #16 rshift $3F and 0 .r .,
    dup #10 rshift $3F and 0 .r ;

: pcrel ( addr opcode -- )
    ." adr" dup $80000000 and IF  'p' emit #12  ELSE  0  THEN  >r
    space dup $1F and .rd .,
    dup $FFFFE0 and #3 rshift swap #29 rshift 3 and or r> lshift
    over + . ;
: addsub# ( opcode -- )
    dup s" addsub" .op2 dup .ops space dup .rd ., dup .rn ., .imm12 ;
: logic# ( opcode -- )
    dup s" and orr eor ands" .op4 space
    dup .rd ., dup .rn ., .immrs ;
: movw# ( opcode -- )
    dup s" movnmov?movzmovk" .op4 space
    dup .rd ., dup .imm16 .lsl ;
: bitfield# unallocated ;
: extract# unallocated ;

\ load store

: .rd/smd ( opcode -- )
    dup $04000000 and IF
	dup #30 rshift s" sdq?" rot .1" $1F and #.r
    ELSE
	dup $1F and swap -$20 and 2* or .rd
    THEN ;

: ldstex  unallocated ;
: ldr# ( opcode -- )
    dup #30 rshift s" ldr  ldr  ldrswprfm " rot .5" space
    dup .rd/smd ., .imm19 ;
: ldstp unallocated ;
: ldstr# unallocated ;

\ instruction table

Create inst-table
\ data processing, immediate
$10000000 , $1F000000 , ' pcrel ,
$11000000 , $1F000000 , ' addsub# ,
$12000000 , $1F800000 , ' logic# ,
$12800000 , $1F800000 , ' movw# ,
$13000000 , $1F800000 , ' bitfield# ,
$13800000 , $1F800000 , ' extract# ,

\ branches
$14000000 , $7C000000 , ' ucbranch# ,
$34000000 , $7E000000 , ' c&branch# ,
$35000000 , $7E000000 , ' t&branch# ,
$54000000 , $FE000000 , ' condbranch# ,
$D4000000 , $FF000000 , ' exceptions ,
\ $D5000000 , $FF000000 , ' system ,
$D61F0000 , $FE1FFC1F , ' ucbranch ,

\ load store
$08000000 , $3F000000 , ' ldstex ,
$18000000 , $3B000000 , ' ldr# ,
$28000000 , $3A000000 , ' ldstp ,
$38000000 , $3B200000 , ' ldstr# ,

\ catch all
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

