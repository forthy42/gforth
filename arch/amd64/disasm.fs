\ *** Disassembler for amd64 ***

\ Copyright (C) 1992-2000 by Bernd Paysan (486 disassemlber)

\ Copyright (C) 2016 Free Software Foundation, Inc.

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
\
\ amd disassembler loadscreen                         19may97py

Vocabulary disassembler

disassembler also definitions

base @ $8 base !

Variable cp?

\ long words and presigns                              31dec92py

: .#    '# emit ;
: .$    '$ emit ;
: .,    ', emit ;
: .+    '+ emit ;
: .-    '- emit ;
: ..    '. emit ;
: .:    ': emit ;
: .[    '[ emit ;
: .]    '] emit ;

\ signed / unsigned byte, word and long output         07aug10py

: .lformat   ( addr -- )  $C u.r ." :" ;

: .du   ( n -- )       0  <<# #s #> type #>> ;
: .$du  ( n -- )       .$ .du ;
: .$ds  ( n -- )       dup 0< IF  .- negate  THEN  .$du ;

: .by   ( 8b -- )      0  <<#  # #  #>  type #>> ;
: .$bu  ( 8b -- )      .$ .by ;
: .$bs  ( 8b -- )      $FF and dup $7F >
                           IF .- $100 swap - THEN .$bu  ;

: .dump ( addr len -- )   bounds DO  i c@ .by  LOOP ;


\ Variables and tabs                                   16nov97py

Variable opcode
Variable mode
Variable length
Variable alength
Variable .length
Variable .alength
Variable .amd64mode  .amd64mode on
Variable seg: seg: on
Variable rex

  &36 constant  bytfld
  &10 constant  mnefld
  &18 constant  addrfld
: tab #tab emit ;

: len!  .length @ length !  .alength @ alength !  seg: on ;

: t,   swap align c, c, align ' ,   '" parse here over 1+ allot place align ;

\ Strings                                              07feb93py
Create "regs  ," AXCXDXBXSPBPSIDI8 9 101112131415"
Create "breg  ," AL  CL  DL  BL  AH  CH  DH  BH  R8L R9L R10LR11LR12LR13LR14LR15L"
Create "breg2 ," AL  CL  DL  BL  SPL BPL SIL DIL R8L R9L R10LR11LR12LR13LR14LR15L"
Create "16ri ," BX+SIBX+DIBP+SIBP+DISI   DI   BP   BX   "
Create "ptrs ," DWORDWORD BYTE "
Create "idx  ,"   *2*4*8"
Create "seg  ," ESCSSSDSFSGS"   Create "seg1 ," escsssdsfsgs"
Create "jmp ," o b z bes p l le"
Create grp1 ," addor adcsbbandsubxorcmprolrorrclrcrshlshrsalsar"
Create grp3 ," testtestnot neg mul imuldiv idiv"
Create grp4 ," inc  dec  call callfjmp  jmpf push g4?? "
Create grp6  ," sldtstr lldtltr verrverwg6??g6??sgdtsidtlgdtlidtsmswg7??lmswg7??"
Create grp8 ," ???? src"
2 "regs c!      5 "16ri c!      5 "ptrs c!      2 "idx  c!
2 "seg  c!      2 "seg1 c!      2 "jmp  c!      3 grp1  c!
4 grp3  c!      5 grp4  c!      4 grp6  c!      1 grp8  c!
4 "breg c!      4 "breg2 c!

\ rex handling

: rex? ( n -- flag )  rex @ and 0<> ;
: p? ( -- flag ) 100 rex? ; 
: w? ( -- flag )  10 rex? ;
Defer >reg
: >r? ( reg -- reg' )   4 rex? 10 and or ; 
: >x? ( reg -- reg' )   2 rex? 10 and or ; 
: >b? ( reg -- reg' )   1 rex? 10 and or ;
' >r? is >reg

\ Register display                                     05dec92py

: *."  ( n addr -- )  count >r swap r@ * + r> -trailing type ;
: .regsize ( -- ) 'R' 'E' w? select emit ;
: .(reg ( n l -- )
    dup 0= IF  drop .regsize "regs ( " )  ELSE  2 = IF
	    "breg2 "breg p? select  ELSE  "regs ( " ) THEN  THEN
    >r >reg r> *." ;
: .reg  ( n -- )  length @ .(reg ;
: .r/reg  ( n -- )    ['] >r? is >reg length @ .(reg ;
: .m/reg  ( n -- )    ['] >x? is >reg length @ .(reg ;
: .ereg ( n -- )  .regsize >reg "regs *." ;
: .mi/reg  ( n -- )   ['] >x? is >reg .ereg ;
: .sib/reg  ( n -- )  ['] >b? is >reg .ereg ;
: .seg  ( n -- )  "seg *." ;

: mod@ ( addr -- addr' r/m reg )
  count dup 70 and 3 rshift swap 307 and swap ;
: .8b  ( addr -- addr' )  count .$bs ;
: .32b ( addr -- addr' )  dup l@  .$ds 4 + ;
: .32u ( addr -- addr' )  dup l@  .$du 4 + ;
: .64b ( addr -- addr' )  dup @   .$ds $8 + ;
: .64u ( addr -- addr' )  dup @   .$du $8 + ;

\ Register display                                     05dec92py

Create .disp ' noop ,  ' .8b ,   ' .32b ,

: .sib  ( addr mod -- addr' ) >r count  dup 7 and 5 = r@ 0= and
  IF    rdrop >r .32b r>
  ELSE  swap r> cells .disp + perform swap dup 7 and .[ .sib/reg .]
  THEN  3 rshift dup 7 and 4 = 0=
  IF    .[ dup 7 and .mi/reg 3 rshift "idx *." .]
  ELSE  drop  THEN ;

: .32a  ( addr r/m -- addr' ) dup 7 and >r 6 rshift
  dup 3 =            IF  drop r>       .m/reg    exit  THEN
  dup 0= r@ 5 = and  IF  drop rdrop .[ .32u .] exit  THEN
  r@  4 =            IF       rdrop    .sib    exit  THEN
  cells .disp + perform  r> .[ .sib/reg .] ;
\ Register display                                     29may10py

: wcount ( addr -- addr' w ) dup uw@ >r 2 + r> ;
: wxcount ( addr -- addr' w ) dup sw@ >r 2 + r> ;
: +8b  ( addr -- addr' )  count  .$bs ;
: +16b ( addr -- addr' )  wcount .$ds ;

Create .16disp  ' noop , ' +8b , ' +16b ,

: .16r  ( reg -- ) .[ "16ri *." .] ;
: .16a  ( addr r/m -- addr' ) 307 and
  dup 006 =  IF  drop wcount .[ .$du .] exit  THEN
  dup 7 and >r 6 rshift  dup 3 =  IF  drop r> .m/reg exit  THEN
  cells .16disp + perform r> .16r  ;


\ Register display                                     01jan93py

: .addr ( addr r/m -- addr' )
  seg: @ 0< 0= IF  seg: @ .seg ': emit  THEN
  alength @  IF  .16a  ELSE  .32a  THEN ;

: .ptr  ( addr r/m -- addr' )
  dup 300 < IF  length @ "ptrs *." ."  PTR "  THEN  .addr ;

: .mod  ( addr -- addr' )  mod@ .r/reg ., .addr ;
: .rmod ( addr -- addr' )  mod@ >r .addr r> ., .r/reg ;

: .imm  ( addr -- addr' )  length @
  dup 0= IF  drop  dup l@  .$ds 4 + exit  THEN
  1 =    IF  wcount .$ds exit  THEN  count .$bs ;

\ .ari                                                 07feb93py

Defer .code

: .b? ( -- ) opcode @ 1 and 0= IF  2 length !  THEN ;
: .ari   .b? tab
  opcode @ dup 4 and  IF  drop 0 .r/reg ., .imm exit  THEN
  2 and  IF  .mod  ELSE  .rmod  THEN ;
: .modt  tab .mod ;
: .gr    tab  opcode @ 7 and .r/reg ;
: .rexinc  .amd64mode @ IF  opcode @ rex !  .code  rex off  EXIT  THEN
    ." inc" .gr ;
: .rexdec  .amd64mode @ IF  opcode @ rex !  .code  rex off  EXIT  THEN
    ." dec" .gr ;

: .igrv  .gr ., .imm ;
: .igrb  2 length ! .igrv ;
: .igr   .b? .igrv ;
: .modb  .b? tab .rmod ;

: .xcha  .gr ., 0 .m/reg ;
\ .conds modifier                                      29may10py

: .cond ( -- ) opcode @
  17 and  dup 1 and  IF  'n' emit  THEN  2/ "jmp *." ;
: .jb   tab count dup $80 and IF -$80 or THEN over + .$du ;
: .jv   tab  alength @  IF  wxcount over
  ELSE  dup sl@  swap 4 + tuck  THEN + .$du ;
: .js   .cond .jb ;
: .jl   .cond .jv ;
: .set  .cond tab mod@ drop 2 length ! .ptr ;

: asize   alength @ invert alength ! .code ;
: osize   length @ 1 xor   length ! .code ;
: .seg:   opcode @ 3 rshift 3 and seg: ! .code ;
: .segx   opcode @ 1 and 4 +   seg: ! .code ;
: .pseg   tab opcode @ 3 rshift 7 and .seg ;
\ .grp1 .grp4 .adj .arpl                               05dec92py
: .grp1   .b? mod@ grp1 *." tab .ptr .,
  opcode @ 3 and 3 = IF  2 length !  THEN  .imm ;
: .grp2   .b? mod@ $8 + grp1 *." tab .ptr .,
  opcode @ 2 and IF ." CL" ELSE ." 1" THEN ;
: .grp3   .b? mod@ dup >r grp3 *." tab
  r@ 3 > IF  0 .r/reg .,  THEN
  r@ 2 4 within  IF  .ptr  ELSE  .addr  THEN
  r> 2 < IF  ., .imm  THEN ;
: .grp4   .b? mod@ dup grp4 *." tab
  2 + 7 and 4 < IF  .ptr  ELSE  .addr  THEN ;
: .adj    opcode @ dup $10 and
  IF  'a'  ELSE  'd'  THEN  emit  'a' emit  $8 and
  IF  's'  ELSE  'a'  THEN  emit ;
: .seg#   .[ dup alength @ 2* 4 + + wcount .$du
  .: swap alength @ IF  wcount .$du  ELSE  .32u  THEN .] drop ;
\ .movo .movx .str                                     23jan93py
: .movo   tab .b?
  opcode @ 2 and 0= IF  0 .r/reg .,  THEN  $05 alength @ - .addr
  opcode @ 2 and    IF  ., 0 .r/reg  THEN ;
: .movx   tab mod@ .r/reg ., 1 length ! .b? .ptr ;
: .movi   .b? tab mod@ drop .ptr ., .imm ;
: .movs   tab mod@  opcode @ 2 and
  IF  .seg ., .addr  ELSE  >r .addr ., r> .seg  THEN ;
: .str    .b? " dwb" 1+ length @ + c@ emit ;
: .far    tab .seg# ;
: .modiv   .modt ., .imm ;
: .modib   .modt ., 2 length ! .imm ;
: .iv     tab .imm ;
: .ib     2 length ! .iv ;
: .ev     tab mod@ drop .ptr ;
: .arpl   tab  1 length !  .rmod ;
\ .mne                                                 16nov97py

: .io   tab .b? 0 .r/reg ., 1 length ! 2 .m/reg ;
: .io#  tab .b? 0 .r/reg ., count .$bu ;
: .ret  opcode @ 1 and 0= IF tab wcount .$du THEN ;
: .enter  tab wcount .$du ., count .$bu ;
: .stcl opcode @ 1 and IF ." st" ELSE ." cl" THEN
  " cid " 1+ opcode @ 2/ 3 and + c@ emit ;

: .mne ( addr field -- addr' )  >r count dup opcode ! r>
    BEGIN  2dup c@  and  over 1+ c@ = 0= WHILE
	    cell+ cell+ count + aligned  REPEAT
  nip dup cell+ cell+  count type  cell+ perform len! ;

0 Value mntbl
:noname  mntbl .mne ; is .code

\ .grp6 .grp7                                          07aug10py

: .grp6  1 length ! mod@ opcode @ 3 lshift + grp6 *." tab .addr ;
: .grp2i  .b? mod@ $8 + grp1 *." tab .ptr ., 2 length ! .imm ;
: .grp8   mod@ grp8 *." tab .addr ., 2 length ! .imm ;
: .bt     opcode @ 3 rshift 7 and grp8 *." tab .rmod ;
Create  lbswap  0 c, 3 c, 3 c, 0 c,
: .movrx  tab  opcode @ dup 3 and lbswap + c@ xor 7 and >r
  mod@ r@ 1 and  IF  swap 7 and .r/reg .,  THEN
  r@ 2/ " CDT?" + 1+ c@ swap 0 <<# # 'R hold rot hold #> type
  #>> r> 1 and  0= IF  ., 7 and .m/reg  THEN ;
: .lxs  opcode @ 7 and "seg1 *." .modt ;
: .shd  tab .rmod ., 2 length ! opcode @ 1 and
  IF  1 .r/reg  ELSE  .imm  THEN ;


\ .esc                                                 22may93py
: flt,  c, bl parse here over 1+ allot place ;
Create fop1table hex
80 flt, chs     81 flt, abs     84 flt, tst     85 flt, xam
08 flt, ld1     09 flt, ldl2t   0A flt, ldl2e   0B flt, ldpi
0C flt, ldlg2   0D flt, ldln2   0E flt, ldz
90 flt, 2xm1    D1 flt, yl2x    92 flt, ptan    D3 flt, patan
94 flt, xtract  D5 flt, prem1   16 flt, decstp  17 flt, incstp
D8 flt, prem    D9 flt, yl2xp1  9A flt, sqrt    9B flt, sincos
9C flt, rndint  DD flt, scale   9E flt, sin     9F flt, cos
: .st   ." ST"  ?dup IF  ." (" 1 .r ." )"  THEN ;
: .st?  dup 40 and IF 1 .st ., THEN  80 and IF 0 .st THEN ;
: .fop1 ( IP opcode -- IP )  1F and >r fop1table
  BEGIN  count 1F and r@ <  WHILE  count +  REPEAT
  dup 1- c@ dup 1F and r> =
  IF  swap count type tab  .st?  ELSE  ." ??" 2drop  THEN ;
\ .esc                                                 18dec93py
Create fopbtable
00 flt, add     01 flt, mul     02 flt, com     03 flt, comp
04 flt, sub     05 flt, subr    06 flt, div     07 flt, divr
08 flt, ld      09 flt, xch     0A flt, st      0B flt, stp
Create "fptrs ," SFLOATDWORD DFLOATWORD  "      6 "fptrs c!
: .modst  count type dup 200 and IF  ." p"  THEN  tab
  dup 400 and IF  dup 7 and .st .,  THEN  0 .st
  dup 400 and 0= IF  dup 7 and ., .st  THEN  drop ;
: .fmodm  over 9 rshift dup >r 1 and IF ." i" THEN  count type tab
  r> "fptrs *." ."  PTR " FF and .addr ;
: .modfb ( IP opcode -- IP' )  dup 1D0 = IF drop ." nop" exit THEN
  dup 7F8 and 5C0 = IF ." free" tab 7 and .st exit  THEN
  dup dup 38 and 3 rshift swap 100 and 5 rshift or >r fopbtable
  BEGIN  count 1F and r@ <  WHILE  count +  REPEAT  rdrop
  over C0 and C0 =  IF  .modst  ELSE  .fmodm  THEN  ;
\ .esc                                                 22may93py
Create fopatable
00 flt, ldenv   01 flt, ldcw    02 flt, stenv   03 flt, stcw
                05 flt, ld                      07 flt, stp
08 flt, rstor                   0A flt, save    0B flt, stsw
0C flt, bld     0D flt, ild     0E flt, bstp    0F flt, istp

: .modfa  ( IP opcode -- IP' )
    dup 7E0 = IF  drop ." stsw" tab ." AX" exit  THEN
    dup 600 and 7 rshift over 18 and 3 rshift or >r fopatable
    BEGIN  count 1F and r@ <  WHILE  count +  REPEAT
    dup 1- c@ r> = 0= IF  drop  " ??"  THEN
    count type tab FF and .addr ;



\ .esc                                                 02mar97py

: .fop2   1F and  dup 2 = IF  drop ." clex" exit  THEN
  dup 3 = IF  drop ." init" exit THEN ." ??" .  ;
: .esc  ( ip -- ip' )  count opcode @ 7 and 8 lshift or
  dup 7E0 and 1E0 = IF  .fop1  exit  THEN
  dup 7E0 and 3E0 = IF  .fop2  exit  THEN
  dup 120 and 120 = IF  .modfa exit  THEN  .modfb ;








\ .mmi                                                 02mar97py

: .mmr ( reg -- )  ." MM" 7 and 0 .r ;
: .mma ( r/m -- )  dup $C0 <
  IF ." QUAD PTR " .addr  ELSE  .mmr  THEN ;
: .mmq ( ip -- ip' )  tab mod@ .mmr ., .mma ;
: .mms ( -- )  opcode @ 3 and s" bwdq" drop + c@ emit ;
: .mmx ( ip -- ip' )  .mms  .mmq ;
: .mmi ( ip -- ip' )  mod@ 2/ 3 and
  s" ??rlrall" drop swap 2* + 2 type .mms tab .mmr ., .8b ;






\ 0Ffld                                                16nov97py
Create 0Ftbl
FE 00 t, .grp6 "
FF 02 t, .modt lar"             FF 03 t, .modt lsl"
FF 06 t, noop clts"             F8 20 t, .movrx mov"
FF 08 t, noop invd"             FF 09 t, noop wbinvd"
F0 80 t, .jl j"                 F0 90 t, .set set"
F7 A0 t, .pseg push"            F7 A1 t, .pseg pop"
FE A4 t, .shd shld"             FE AC t, .shd shrd"
E7 A3 t, .bt bt"                FE A6 t, .modb cmpxchg"
FE B6 t, .movx movzx"           FF BA t, .grp8 bt"
F8 B0 t, .lxs l"                FE BE t, .movx movsx"
FE C0 t, .modb xadd"            F8 C8 t, .gr bswap"
FF AF t, .modt imul"            FF BC t, .modt bsf"
FF BD t, .modt bsr"             FF C7 t, .ev cmpxchg8b"

\ 0Ffld                                                12apr98py

FC 70 t, .mmi ps"
FF 30 t, noop wrmsr"            FF 32 t, noop rdmsr"


FF D5 t, .mmq pmullw"           FF E5 t, .mmq pmulhw"
FF F5 t, .mmq pmaddwd"
FF DB t, .mmq pand"             FF $DF t, .mmq pandn"
FF EB t, .mmq por"              FF EF t, .mmq pxor"
FC D0 t, .mmx psrl"             FC D8 t, .mmx psubu"
FC E0 t, .mmx psra"             FC E8 t, .mmx psubs"
FC F0 t, .mmx psll"             FC F8 t, .mmx psub"
FC DC t, .mmx paddu"            FC EC t, .mmx padds"
FC FC t, .mmx padd"             00 00 t, noop 0F???"
: .0f     0Ftbl  .mne ;
\ disassembler table                                   22may93py
align here to mntbl
FF 0F t, .0f "
E7 06 t, .pseg push"            E7 07 t, .pseg pop"
F8 00 t, .ari add"              F8 08 t, .ari or"
F8 10 t, .ari adc"              F8 18 t, .ari sbb"
E7 26 t, .seg: "                E7 27 t, .adj "
F8 20 t, .ari and"              F8 28 t, .ari sub"
F8 30 t, .ari xor"              F8 38 t, .ari cmp"
F8 40 t, .rexinc "              F8 48 t, .rexdec "
F8 50 t, .gr push"              F8 58 t, .gr pop"
FF 60 t, noop pusha"            FF 61 t, noop popa"
FF 62 t, .modt bound"           FF 63 t, .arpl arpl"
FE 64 t, .segx "
FF 66 t, osize "                FF 67 t, asize "

\ disassembler table                                   21may94py

FF 68 t, .iv push"              FF 69 t, .modiv imul"
FF 6A t, .ib push"              FF 6B t, .modib imul"
FE 6C t, .str ins"              FE 6E t, .str outs"
F0 70 t, .js j"                 FF 82 t, noop ???"
FC 80 t, .grp1 "                FE 84 t, .modb test"
FE 86 t, .modb xchg"            FC 88 t, .ari mov"
FD 8C t, .movs mov"             FF 8D t, .modt lea"
FF 8F t, .ev pop"
FF 90 t, noop nop"              F8 90 t, .xcha xchg"
FF 98 t, noop cbw"              FF 99 t, noop cwd"
FF 9A t, .far callf"            FF 9B t, noop wait"
FF 9C t, noop pushf"            FF 9D t, noop popf"
FF 9E t, noop sahf"             FF 9F t, noop lahf"

\ disassembler table                                   22may93py

FC A0 t, .movo mov"             FE A4 t, .str movs"
FE A6 t, .str cmps"             FE A8 t, .igr test"
FE AA t, .str stos"             FE AC t, .str lods"
FE AE t, .str scas"
F8 B0 t, .igrb mov"             F8 B8 t, .igrv mov"
FE C0 t, .grp2i "               FE C2 t, .ret ret"
FF C4 t, .modt les"             FF C5 t, .modt lds"
FE C6 t, .movi mov"
FF C8 t, .enter enter"          FF C9 t, noop leave"
FE CA t, .ret retf"
FF CC t, noop int3"            FF 0CD t, .ib int"
FF CE t, noop into"             FF CF t, noop iret"


\ disassembler table                                   12aug00py
FC D0 t, .grp2 "
FF D4 t, noop aam"              FF D5 t, noop aad"
FF D6 t, noop salc"
FF D7 t, noop xlat"             F8 D8 t, .esc f"
FF E0 t, .jb loopne"            FF E1 t, .jb loope"
FF E2 t, .jb loop"              FF E3 t, .jb jcxz"
FE E4 t, .io# in"               FE E6 t, .io# out"
FF E8 t, .jv call"              FF E9 t, .jv jmp"
FF EA t, .far jmpf"             FF EB t, .jb jmp"
FE EC t, .io in"                FE EE t, .io out"
FF F0 t, .code lock "           FF F2 t, .code rep "
FF F3 t, .code repe "           FF F4 t, noop hlt"
FF F5 t, noop cmc"              FE F6 t, .grp3 "
FE FE t, .grp4 "                F8 F8 t, .stcl "
00 00 t, noop ???"
\ addr! dis disw disline                               13may95py

: .86    1 .length !  .alength on  len! ;
: .386   .length off  .alength off len! ;
: .amd64 .386 .amd64mode on ;

base !

Forth definitions

: disline ( addr -- addr' )
    [: dup .lformat tab .code ;]
    $10 base-execute ;

: disasm ( addr u -- ) \ gforth
    [: over + >r
	begin  dup r@ u<  while  cr disline  repeat
	cr rdrop drop ;] $10 base-execute ;

' disasm is discode

previous Forth