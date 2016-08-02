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
: .]' ( -- ) ']' emit ;
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
: .regsize' ( opcode -- )
    #30 rshift 3 = 'x' 'w' rot select emit ;
: #.r ( n -- ) \ print decimal
    0 ['] .r #10 base-execute ;
: b>sign ( u m -- n ) over and negate or ;
: .spreg ( opcodeshift -- )
    $1F and dup $1F = IF  drop ." sp"  ELSE  #.r  THEN ;
: .zrreg ( opcodeshift -- )
    $1F and dup $1F = IF  drop ." zr"  ELSE  #.r  THEN ;
: .rd ( opcode -- )
    dup .regsize .spreg ;
: .rt ( opcode -- )
    dup .regsize' .zrreg ;
: .rt2 ( opcode -- )
    dup .regsize' #16 rshift .zrreg ;
: .rd' ( opcode -- )
    dup .regsize .zrreg ;
: .rn ( opcode -- )
    'x' emit #5 rshift .spreg ;
: .rn' ( opcode -- )
    dup .regsize #5 rshift .zrreg ;
: .rm ( opcode -- )
    dup .regsize #16 rshift .spreg ;
: .rm' ( opcode -- )
    dup .regsize #16 rshift .zrreg ;
: .ra ( opcode -- )
    dup .regsize #10 rshift .zrreg ;
: .ra' ( opcode -- )
    dup .regsize' #10 rshift .zrreg ;
: .imm5 ( opcode -- ) \ print 5 bit immediate
    #16 rshift $1F and .# 0 .r ;
: .imm6 ( opcode -- ) \ print 6 bit immediate
    #10 rshift $3F and .# 0 .r ;
: .imm6' ( opcode -- ) \ print 6 bit immediate
    #16 rshift $3F and .# 0 .r ;
: .imm7 ( opcode -- ) \ print 7 bit immediate
    dup #15 rshift $7F and $40 b>sign
    swap s? IF  dfloats  ELSE  sfloats  THEN
    .# 0 .r ;
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
    dup s? >r
    dup #22 rshift 1 and { N }
    dup #16 rshift $3F and { R }
    #10 rshift $3F and { S }
    N IF  64 -1
    ELSE
	case
	    S $00 $20 within ?of  #32  endof
	    S $20 $30 within ?of  #16  S $F and to S  endof
	    S $30 $38 within ?of  #08  S $7 and to S  endof
	    S $38 $3C within ?of  #04  S $3 and to S  endof
	    S $3C $3E within ?of  #02  S $1 and to S  endof
	    0 0
	endcase
	1 over lshift 1-
    THEN
    { simd_size mask }
    simd_size 1- R and to R
    S simd_size 1- = IF  0
    ELSE
	1 S 1+ lshift 1-
	R IF  dup simd_size R - lshift swap R rshift or  THEN
	simd_size #02 = IF  dup #02 lshift or  THEN
	simd_size #04 = IF  dup #04 lshift or  THEN
	simd_size #08 = IF  dup #08 lshift or  THEN
	simd_size #16 = IF  dup #16 lshift or  THEN
	simd_size #32 = IF  dup #32 lshift or  THEN
    THEN
    r> 0= IF  $FFFFFFFF and  THEN  .# 0 .r ;


: pcrel ( addr opcode -- )
    ." adr" dup $80000000 and IF  'p' emit #12  ELSE  0  THEN  >r
    tab dup $1F and .rd .,
    dup $FFFFE0 and #3 rshift swap #29 rshift 3 and or r> lshift
    over + . ;
: addsub# ( opcode -- )
    dup s" addsub" .op2 dup .ops tab dup .rd' ., dup .rn' ., .imm12 ;
: logic# ( opcode -- )
    dup s" and orr eor ands" .op4 tab
    dup .rd ., dup .rn' ., .immrs ;
: tst# ( opcode -- )
    ." tst" tab dup .rn' ., .immrs ;
: movw# ( opcode -- )
    dup s" movnmov?movzmovk" .op4 tab
    dup .rd ., dup .imm16 .lsl ;
: bitfield# ( opcode -- )
    dup #29 rshift $3 and s" sbfmbfm ubfmxxxx" rot .4" tab
    dup .rd' ., dup .rn' ., dup .imm6' ., .imm6 ;
: extract# ( opcode -- )
    ." extr" tab dup .rd' ., dup .rn' ., dup .rm' ., .imm6 ;

\ load store

: .sname ( opcode -- )
    #30 rshift s" sdq?" rot .1" ;
: .sname' ( opcode -- )
    #22 rshift 3 and s" sdq?" rot .1" ;
: .srd ( opcode -- )
    dup .sname $1F and #.r ;
: .sra ( opcode -- )
    dup .sname #10 rshift $1F and #.r ;

: .rd/smd ( opcode -- )
    dup v? IF
	dup .srd
    ELSE
	dup $1F and swap -$20 and 2* or .rt
    THEN ;

: .st/ld ( opcode -- opcode )
    s" stld" 2 pick #22 rshift $1 and .2" ;
: .st/ld3 ( opcode -- opcode )
    s" stldldld" 2 pick #22 rshift $3 and .2" ;    
: .bhw ( opcode -- opcode )
    s" bhw " 2 pick #30 rshift .1" ;
: ldstex  ( opcode -- )
    .st/ld
    dup #22 rshift $1 and 'a' 'l' rot select
    over #15 rshift $1 and IF  emit  ELSE  drop  THEN
    'r' emit .bhw tab
    dup #22 rshift $1 and 0= IF  dup .rt2 .,  THEN
    dup .rt ., .[ .rn .] ;
    
: ldr# ( opcode -- )
    dup #30 rshift s" ldr  ldr  ldrswprfm " rot .5" tab
    dup .rd/smd ., .imm19 ;
: ldstp ( opcode -- ) \ missing: vector encoding
    .st/ld
    dup #23 rshift $3 and s" npp p p " rot .2" tab
    dup v? IF \ simd/fp
	dup .srd ., dup .sra .,
    ELSE \ normal
	dup .rt ., dup .ra' .,
    THEN
    case dup #23 rshift $3 and
	0 of .[ dup .rn ., .imm7 .]  endof
	1 of .[ dup .rn .]' ., .imm7 ,space on  endof
	2 of .[ dup .rn ., .imm7 .]  endof
	3 of .[ dup .rn ., .imm7 .] '!' emit  endof
	nip endcase ;

: .smd-size ( opcode -- opcode )
    dup #30 rshift $3 and over #21 rshift 4 and or
    s" bhsdq   " rot .1" ;

: ldstr# ( opcode -- )
    dup v? IF
	.st/ld
	s" u t " 2 pick #10 rshift $3 and .1" 'r' emit tab
	.smd-size dup $1F and #.r
    ELSE
	.st/ld3
	s" u t " 2 pick #10 rshift $3 and .1" 'r' emit
	s"   ss" 2 pick #22 rshift $3 and .1"
	.bhw
	tab dup .rt
    THEN  .,
    case dup #10 rshift $3 and
	0 of .[ dup .rn ., .imm9 .]  endof
	1 of .[ dup .rn .]' ., .imm9 ,space on  endof
	2 of .[ dup .rn ., .imm9 .]  endof
	3 of .[ dup .rn ., .imm9 .] '!' emit  endof
    endcase ;
: ldustr# ( opcode -- )
    dup v? IF
	.st/ld tab
	.smd-size dup $1F and #.r
    ELSE
	.st/ld3
	s"   ss" 2 pick #22 rshift $3 and .1"
	.bhw tab dup .rt
    THEN  .,
    .[ dup .rn ., .imm12' .] ;

\ data processing

: mov ( opcode -- ) \ is a special orr variant
    ." mov" tab dup .rd ., .rm' ;
: 1source ( opcode -- ) \ other one source operations
    dup #10 rshift $3F and
    s" rbit rev16rev32rev  clz  cls  " rot .5" tab dup .rd ., .rn' ;
: 2source ( opcode -- ) \ other two source operations
    dup #10 rshift $7 and  over #13 rshift $7 and
    case
	0 of  s" xx  xx  udivsdiv" rot .4"  endof
	1 of  s" lsllsrasrror" rot .3"  endof
	2 of ." crc32" s" b h w x cbchcwcx" rot .4"  endof
	drop unallocated  EXIT
    endcase
    tab  dup .rd ., dup .rn' ., .rm' ;

: 3source ( opcode -- ) \ three source operations
    dup #20 rshift $E and over #15 rshift 1 and or
    s" madd  msub  smaddlsmsublumaddlsmulh                              umsubllumulh" rot .6" tab
    dup .rd ., dup .rn' ., dup .rm' ., .ra ;

: .shift ( opcode -- )
    dup #10 rshift $3F and ?dup-0=-IF  drop EXIT  THEN  >r
    ., #22 rshift $3 and s" lsllsrasrror" rot .3" space
    .# r> 0 .r ;

: ltst# ( opcode -- ) \ logical with shifted operand, tst case
    ." tst" tab dup .rn' ., dup .rm' .shift ;

: logshift# ( opcode -- ) \ logical with shifted operand
    dup #28 rshift $6 and over #21 rshift $1 and or
    s" and bic orr orn eor eon andsbics" rot .4" tab
    dup .rd' ., dup .rn' ., dup .rm' .shift ;

: tstshift# ( opcode -- ) \ logical with shifted operand
    ." tst" tab dup .rn' ., dup .rm' .shift ;

: addshift# ( opcode -- ) \ logical with shifted operand
    dup s" addsub" .op2 dup .ops tab
    dup .rd' ., dup .rn' ., dup .rm' .shift ;

: .ext ( opcode -- )
    #10 rshift dup $7 and >r .,
    #3 rshift $7 and s" uxtbuxthuxtwuxtxsxtbsxthsxtwxstx" rot .4" space
    .# r> 0 .r ;

: addext# ( opcode -- ) \ addsub with shifted operand
    dup s" addsub" .op2 dup .ops tab
    dup .rd ., dup .rn ., dup .rm .ext ;

: addc# ( opcode -- ) \ addsub with carry
    dup s" adcsbc" .op2 dup .ops tab
    dup .rd' ., dup .rn' ., .rm' ;

: .nzcv ( n -- ) .#
    dup $8 and 'n' '-' rot select emit
    dup $4 and 'z' '-' rot select emit
    dup $2 and 'c' '-' rot select emit
    $1     and 'v' '-' rot select emit ;

: ccmp ( opcode -- )
    ." ccm" dup #30 rshift 1 and 'p' 'n' rot select emit tab
    dup .rn' ., dup .rm' ., dup $F and .nzcv ., #12 rshift .cond ;

: ccmp# ( opcode -- )
    ." ccm" dup #30 rshift 1 and 'p' 'n' rot select emit tab
    dup .rn' ., dup .imm5 ., dup $F and .nzcv ., #12 rshift .cond ;

: csel ( opcode -- )
    ." cs" dup #29 rshift $2 and over #10 rshift 1 and or
    s" el incinvneg" rot .3" tab
    dup .rd' ., dup .rn' ., dup .rm' ., #12 rshift .cond ;

\ floating point

: .sd ( opcode -- )
    dup .sname' $1F and #.r ;
: .sn ( opcode -- )
    dup .sname' #5 rshift $1F and #.r ;
: .sm ( opcode -- )
    dup .sname' #16 rshift $1F and #.r ;
Fvariable fxx
: .f8 ( float8 -- )
    dup $80 and #8 lshift swap $7F and
    $40 b>sign $7FFF and $4000 xor or fxx 6 + w! fxx f@ .# f. ;

: .fimm8 ( opcode -- )
    #13 rshift $FF and .# .f8 ;

: fp1source ( opcode -- )
    'f' emit  dup #15 rshift $3F and
    s" mov  abs  neg  sqrt cvt  cvt  cvt  cvt  rintnrintprintmrintzrinta???  rintxrinti" rot .5" tab
    dup .sd ., .sn ;
: fp2source ( opcode -- )
    'f' emit  dup #12 rshift $F and
    s" mul  div  add  sub  max  min  maxnmminnmnmul " rot .5" tab
    dup .sd ., dup .sn ., .sm ;
: fpcmp ( opcode -- )
    ." fcmp" dup $10 and IF  'e' emit  THEN tab
    dup .sn ., dup $8 and IF  ." #0.0" drop  ELSE  .sm  THEN ;
: fp#  ( opcode -- )
    ." fmov" tab dup .sd ., .fimm8 ;
: fpccmp ." fccmp "   unallocated ;
: fpcsel  ( opcode -- )
    ." fcsel" tab dup .sd ., dup .sn ., dup .sm ., #12 rshift .cond ;
: fp3source  unallocated ;
: simdsc3  unallocated ;

\ instruction table

Create inst-table
\ data processing, immediate
$10000000 , $1F000000 , ' pcrel ,
$11000000 , $1F000000 , ' addsub# ,
$7200001F , $7F80001F , ' tst# ,
$12000000 , $1F800000 , ' logic# ,
$12800000 , $1F800000 , ' movw# ,
$13000000 , $1F800000 , ' bitfield# ,
$13800000 , $1F800000 , ' extract# ,

\ data processing, register
$2A0003E0 , $7FE0FFE0 , ' mov ,
$5AC00000 , $5FFF0000 , ' 1source ,
$1AC00000 , $5FC00000 , ' 2source ,
$1B000000 , $7F000000 , ' 3source ,
$6A00001F , $7F00001F , ' tstshift# ,
$0A000000 , $1F000000 , ' logshift# ,
$0B000000 , $1F200000 , ' addshift# ,
$0B200000 , $1F200000 , ' addext# ,
$1A000000 , $1FE0F800 , ' addc# ,
$3A400000 , $3FE00C10 , ' ccmp ,
$3A400800 , $3FE00C10 , ' ccmp# ,
$1A800000 , $3FE00800 , ' csel ,

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

\ simd+fp
$5E200400 , $DF200400 , ' simdsc3 ,
$1E204000 , $FF207C00 , ' fp1source ,
$1E202000 , $FF203C00 , ' fpcmp ,
$1E201000 , $FF201C00 , ' fp# ,
$1E200400 , $FF200C00 , ' fpccmp ,
$1E200800 , $FF200C00 , ' fp2source ,
$1E200C00 , $FF200C00 , ' fpcsel ,
$1F000000 , $FF000000 , ' fp3source ,

\ catch all
$00000000 , $00000000 , ' unallocated ,

: inst ( opcode -- )  inst-table
    BEGIN  2dup 2@ >r and r> <>  WHILE  3 cells +  REPEAT
    2 cells + perform ;

Forth definitions

: disasm ( addr u -- ) \ gforth
    [: over + >r
	begin
	    dup r@ u<
	while
		cr dup 14 .r ." : " dup l@ inst 4 +
	repeat
	cr rdrop drop ;] $10 base-execute ;

' disasm is discode

previous Forth