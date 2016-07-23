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

Variable ,space ,space on

: ., ( -- ) ',' emit ,space @ IF space THEN ;
: .[ ( -- ) '[' emit ,space off ;
: .] ( -- ) ']' emit ,space on ;
: .# ( -- ) '#' emit ;
: tab ( -- ) #tab emit ;

: .1" ( addr u opcode -- ) \ print substring by 1
    safe/string 1 min -trailing type ;
: .2" ( addr u opcode -- ) \ print substring by 2
    2* safe/string 2 min -trailing type ;
: .3" ( addr u opcode -- ) \ print substring by 3
    3 * safe/string 3 min -trailing type ;
: .4" ( addr u opcode -- ) \ print substring by 4
    4 * safe/string 4 min -trailing type ;
: .5" ( addr u opcode -- ) \ print substring by 5
    5 * safe/string 5 min -trailing type ;
: .6" ( addr u opcode -- ) \ print substring by 6
    6 * safe/string 6 min -trailing type ;
: .op4 ( opcode addr u -- ) \ select one of four opcodes
    rot #29 rshift 3 and .4" ;
: .op2 ( opcode addr u -- )
    rot #30 rshift 1 and IF  dup 2/ /string  ELSE  2/  THEN  -trailing type ;
: .ops ( opcode -- )  #29 rshift 1 and IF  ." s"  THEN ;
: s? ( opcode -- flag )  $80000000 and ;
: v? ( opcode -- flag )  $04000000 and ;
: .regsize ( opcode -- )
    s? 'x' 'w' rot select emit ;
: #.r ( n -- ) \ print decimal
    0 ['] .r #10 base-execute ;
: b>sign ( u m -- n ) over and negate or ;
: .rd ( opcode -- )
    dup .regsize $1F and dup $1F = IF  ." SP"  ELSE  #.r  THEN ;
: .rn ( opcode -- )
    dup .regsize #5 rshift $1F and dup $1F = IF  drop ." SP"  ELSE  #.r  THEN ;
: .rm ( opcode -- )
    dup .regsize #14 rshift $1F and dup $1F = IF  drop ." ZR"  ELSE  #.r  THEN ;
: .rm' ( opcode -- )
    dup .regsize #16 rshift $1F and dup $1F = IF  drop ." ZR"  ELSE  #.r  THEN ;
: .ra ( opcode -- )
    dup .regsize #10 rshift $1F and dup $1F = IF  drop ." ZR"  ELSE  #.r  THEN ;
: .imm9 ( opcode -- ) \ print 9 bit immediate, sign extended
    #12 rshift $1FF and $100 b>sign .# 0 .r ;
: .imm12 ( opcode -- ) \ print 12 bit immediate with 2 bit shift
    #10 rshift dup $FFF and swap #12 rshift 3 and #12 * lshift .# 0 .r ;
: .imm12' ( opcode -- ) \ print 12 bit immediate with 2 bit shift
    #10 rshift dup $FFF and swap #20 rshift 3 and lshift .# 0 .r ;
: .imm14 ( addr opcode -- addr ) \ print 19 bit branch target
    #5 rshift $3FFF and 2* 2* over + 0 .r ;
: .imm16 ( opcode -- ) \ print 16 bit immediate
    #5 rshift $FFFF and .# . ;
: .lsl ( opcode -- ) \ print shift
    #21 rshift $3 and #4 lshift ?dup-IF  ." , lsl #$" 0 .r  THEN ;
: .imm19 ( addr opcode -- addr ) \ print 19 bit branch target
    #5 rshift $7FFFF and $40000 b>sign 2* 2* over + 0 .r ;
: .imm26 ( addr opcode -- addr ) \ print 19 bit branch target
    $3FFFFFF and $2000000 b>sign 2* 2* over + 0 .r ;
: .cond ( n -- ) $F and
    s" eqnecsccmiplvsvchilsgeltgtlealnv" rot .2" ;

: unallocated ( opcode -- )
    ." <" 0 .r ." >" ;

\ branches

: .?nz ( opcode -- )
    $01000000 and IF  'n' emit  THEN  'z' emit ;
: .b40 ( opcode -- )  .#
    dup #18 rshift $1F and dup #24 rshift $20 and or #.r ',' emit ;

: c&branch# ( opcode -- )
    ." cb" dup .?nz tab dup .rd ., .imm19 ;
: condbranch# ( opcode -- )
    ." cb" dup .cond tab .imm19 ;
: ucbranch# ( opcode -- )
    ." b" dup $80000000 and IF 'l' emit  THEN tab .imm26 ;
: t&branch# ( opcode -- )
    ." tb" dup .?nz tab dup .rd ., dup .b40 .imm14 ;
: >opc ( opcode -- opc ) #21 rshift $7 and ;
: exceptions ( opcode -- )
    case  dup >opc
	0 of
	    dup $1F and dup 1 4 within IF
		s" svchvcsmc" rot 1- .3"
		tab  .imm16
	    ELSE  unallocated  THEN  endof
	1 of  dup $1F and 0= IF  ." brk " .imm16  ELSE  unallocated  THEN  endof
	2 of  dup $1F and 0= IF  ." hlt " .imm16  ELSE  unallocated  THEN  endof
	5 of  dup $1F and 1 4 within IF  ." dcps" dup $1F and . .imm16
	    ELSE  unallocated  THEN  endof
	swap unallocated
    endcase ;
: ucbranch ( opcode -- )
    dup >opc dup #5 u> IF  drop unallocated
    ELSE  s" br  blr ret eretdrps" rot .4" tab .rn  THEN ;

\ data processing, immediate

: .immrs ( opcode -- )
    .# dup #22 rshift 1 and 0 .r .,
    dup #16 rshift $3F and 0 .r .,
    dup #10 rshift $3F and 0 .r ;

: pcrel ( addr opcode -- )
    ." adr" dup $80000000 and IF  'p' emit #12  ELSE  0  THEN  >r
    tab dup $1F and .rd .,
    dup $FFFFE0 and #3 rshift swap #29 rshift 3 and or r> lshift
    over + . ;
: addsub# ( opcode -- )
    dup s" addsub" .op2 dup .ops tab dup .rd ., dup .rn ., .imm12 ;
: logic# ( opcode -- )
    dup s" and orr eor ands" .op4 tab
    dup .rd ., dup .rn ., .immrs ;
: movw# ( opcode -- )
    dup s" movnmov?movzmovk" .op4 tab
    dup .rd ., dup .imm16 .lsl ;
: bitfield# unallocated ;
: extract# unallocated ;

\ load store

: .rd/smd ( opcode -- )
    dup v? IF
	dup #30 rshift s" sdq?" rot .1" $1F and #.r
    ELSE
	dup $1F and swap -$20 and 2* or .rd
    THEN ;

: ldstex  unallocated ;
: ldr# ( opcode -- )
    dup #30 rshift s" ldr  ldr  ldrswprfm " rot .5" tab
    dup .rd/smd ., .imm19 ;
: ldstp unallocated ;
: ldstr# ( opcode -- )
    dup v? IF  unallocated
    ELSE
	s" stldldld" 2 pick #22 rshift $3 and .2"
	s" u t " 2 pick #10 rshift $3 and .1" 'r' emit
	s"   ss" 2 pick #22 rshift $3 and .1"
	s" bhw " 2 pick #30 rshift .1" tab dup .rd .,
	case dup #10 rshift $3 and
	    0 of .[ dup .rn ., .imm9 .]  endof
	    1 of .[ dup .rn .] ., .imm9  endof
	    2 of .[ dup .rn ., .imm9 .]  endof
	    3 of .[ dup .rn ., .imm9 .] '!' emit  endof
	endcase
    THEN ;
: ldustr# ( opcode -- )
    dup v? IF  unallocated
    ELSE
	s" stldldld" 2 pick #22 rshift $3 and .2"
	s"   ss" 2 pick #22 rshift $3 and .1"
	s" bhw " 2 pick #30 rshift .1" tab dup .rd .,
	.[ dup .rn ., .imm12' .]
    THEN  ;

\ data processing

: mov ( opcode -- ) \ is a special orr variant
    ." mov" tab dup .rd ., .rm ;
: 1source ( opcode -- ) \ other one source operations
    dup #10 rshift $3F and
    s" rbit rev16rev32rev  clz  cls  " rot .5" tab dup .rd ., .rn ;
: 2source ( opcode -- ) \ other two source operations
    dup #10 rshift $7 and  over #13 rshift $7 and
    case
	0 of  s" xx  xx  udivsdiv" rot .4"  endof
	1 of  s" lslvlsrvasrvrorv" rot .4"  endof
	2 of ." crc32" s" b h w x cbchcwcx" rot .4"  endof
	drop unallocated  EXIT
    endcase
    tab  dup .rd ., dup .rn ., .rm' ;

: 3source ( opcode -- ) \ three source operations
    dup #20 rshift $E and over #15 rshift 1 and or
    s" madd  msub  smaddlsmsublumaddlsmulh                              umsubllumulh" rot .6" tab
    dup .rd ., dup .rn ., dup .rm' ., .ra ;

\ instruction table

Create inst-table
\ data processing, immediate
$10000000 , $1F000000 , ' pcrel ,
$11000000 , $1F000000 , ' addsub# ,
$12000000 , $1F800000 , ' logic# ,
$12800000 , $1F800000 , ' movw# ,
$13000000 , $1F800000 , ' bitfield# ,
$13800000 , $1F800000 , ' extract# ,

\ data processing, register
$2A0003E0 , $7FE0FFE0 , ' mov ,
$5AC00000 , $5FFF0000 , ' 1source ,
$1AC00000 , $5FC00000 , ' 2source ,
$1B000000 , $7F000000 , ' 3source ,

\ branches
$54000000 , $FE000000 , ' condbranch# ,
$D4000000 , $FF000000 , ' exceptions ,
$14000000 , $7C000000 , ' ucbranch# ,
$34000000 , $7E000000 , ' c&branch# ,
$35000000 , $7E000000 , ' t&branch# ,
\ $D5000000 , $FF000000 , ' system ,
$D61F0000 , $FE1FFC1F , ' ucbranch ,

\ load store
$08000000 , $3F000000 , ' ldstex ,
$18000000 , $3A000000 , ' ldr# ,
$28000000 , $3A000000 , ' ldstp ,
$38000000 , $3B000000 , ' ldstr# ,
$39000000 , $3B000000 , ' ldustr# ,

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
		cr dup 14 .r ." : " dup l@ inst 4 +
	repeat
	cr rdrop drop ;] $10 base-execute ;

previous

' disasm is discode

