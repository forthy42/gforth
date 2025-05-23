\         *** Assembler for the Athlon64 ***           17jul04py

\ Authors: David Kühling, Bernd Paysan, Anton Ertl
\ Copyright (C) 2000,2001,2003,2004,2007,2010,2017,2019,2020,2021,2024 Free Software Foundation, Inc.

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
\ The syntax is reverse polish. Source and destination are
\ reversed. Size prefixes are used instead of AX/EAX. Example:
\ Intel                           gives
\ mov  ax,bx                      .w bx ax mov
\ mov  eax,[ebx]                  .d bx ) ax mov
\ add  eax,4                      .d 4 # ax add
\ add  rax,8                      .q 8 # ax add
\ 
\ in .86 mode  .w is the default size, in .386 mode  .d is default
\ in .amd64 mode .q is default size.
\ .wa, .da, and .qa change address size. .b, .w(a) and .d(a) are not
\ switches like in my assem68k, they are prefixes.
\ [A-D][L|H] implicitely set the .b size. So
\ AH AL mov
\ generates a byte move. Sure you need .b for memory operations
\ like .b ax ) inc    which is  inc  BYTE PTR [eAX]

\ athlon64 Assembler Load Screen                       21apr00py

base @ get-current ALSO ASSEMBLER DEFINITIONS also

&8 base !

: [F]  Forth     ; immediate
: [A]  Assembler ; immediate

\ Assembler Forth words                                11mar00py

: user' ' >body @ ; immediate
: case? ( n1 n2 -- t / n1 f )
    over = IF  drop true  ELSE  false  THEN ;

\ Code generating primitives                           07mar93py

Variable >codes
: (+rel      ;
Create nrc  ' c, A, ' here A, ' allot A, ' c! A, ' (+rel A,

: nonrelocate   nrc >codes ! ;      nonrelocate

: >exec   Create  dup c,  cell+
            Does>  c@  >codes @  +  perform ;

0
>exec ,       >exec here    >exec allot   >exec c!
>exec +rel
drop

\ Stack-Buffer für Extra-Werte                         22dec93py

Variable ModR/M               Variable ModR/M#
Variable SIB                  Variable SIB#
Variable disp                 Variable disp#
Variable imm                  Variable imm#
Variable Aimm?                Variable Adisp?
Variable byte?                Variable seg
Variable .asize               Variable .anow
Variable .osize               Variable .onow
Variable .64size              Variable .64now
Variable .rex                 Variable .64bit
Variable .arel                Variable media
Variable dquad?
: pre-    seg off  .asize @ .anow !  .osize @ .onow !
  .rex off  .arel off  .64size @ .64now !  media off  dquad? off ;
: sclear  pre-  Aimm? off  Adisp? off
    ModR/M# off  SIB# off  disp# off  imm# off  byte? off ;

: .b  1 byte? !  imm# @ 1 min imm# ! ;

: .w   .onow off .64now off ;   : .wa  .anow off ;
: .d   .onow on .64now off ;    : .da  .anow on ;
: .q   .64now on ;              : .qa  .anow off ;
: -rex  .64now off ;            : -size  .osize @ .onow ! ;
: .dq  dquad? on ;
   
\ Extra-Werte compilieren                              01may95py
: bytes,  ( nr x n -- )
    0 ?DO  over 0< IF  +rel  1 under+  THEN  dup ,  $8 rshift
    LOOP   2drop ;
: rbytes, ( nr x n -- )
    >r here r@ + - r> bytes, ;
: opcode, ( opcode -- )
    .asize @ .anow @  <> IF  $67 ,  THEN
    media  @ IF media @ ,
    ELSE .osize @ .onow @  <> IF  $66 ,  THEN  THEN
    seg    @ IF  seg @ ,  THEN
    .64now @ IF  .rex @ $08 or .rex !  THEN
    .rex   @ IF  .rex @ $F and $40 or ,  THEN
    ,  pre- ;
: finish ( opcode -- )  opcode,
    ModR/M# @ IF  ModR/M @ ,  THEN
    SIB#    @ IF  SIB    @ ,  THEN
    Adisp?  @ disp @ disp# @ .arel @ IF  rbytes,  ELSE  bytes,  THEN
    Aimm?   @ imm  @ imm#  @ bytes,    sclear  ;
: finishb  ( opcode -- )       byte? @ xor  finish ;
: 0F,  $0F opcode, ;

\ Possible bug: '.64now off' does not have any effect _after_ 0F,
: finish0F ( opcode -- )       0F, .64now off finish ;
: finishxx0f  ( $xx0fyy -- )  \ xx 0f yy opcodes  (xx=66,f2,f3 or 00 if none)
   dup $10 rshift media !  -size  $ff and finish0F ;

\ Register                                             29mar94py

: (regs)  ( mod n xt -- )  -rot  FOR  2dup swap execute  11 +  NEXT  2drop ;
: Regs  ( mod n -- )  ['] constant (regs) ;
: breg  ( reg -- )  Create c,  DOES> c@  .b ;
: bregs ( mod n -- ) ['] breg (regs) ;
: wadr: ( reg -- )  Create c,  DOES> c@  .wa ;
: wadr  ( mod n -- ) ['] wadr: (regs) ;
: xmmreg  ( reg -- )  Create [F] , [A]  DOES> @  .dq ;
: xmmregs  ( reg -- )  ['] xmmreg (regs) ;
      0 7 wadr [BX+SI] [BX+DI] [BP+SI] [BP+DI] [SI] [DI] [BP] [BX]
    300 7 regs  AX CX DX BX SP BP SI DI
0200300 7 regs  R8 R9 R10 R11 R12 R13 R14 R15
    300 7 bregs AL CL DL BL AH CH DH BH
2000300 3 bregs SPL BPL SIL DIL
0200300 7 bregs  R8L R9L R10L R11L R12L R13L R14L R15L
   2300 5 regs ES CS SS DS FS GS
' SI alias RP   ' BP alias UP   ' DI Alias OP
: .386  .64size off .64bit off .asize on   .osize on  sclear ;
: .86   .64size off .64bit off .asize off  .osize off sclear ;
: .amd64  .386  .64size on  .asize off  .64bit on  sclear ;  .amd64
: asize@  2 .anow @ IF  2*  THEN  .64bit @ IF  drop 4  THEN ;
: osize@  2 .onow @ IF  2*  THEN  ;

\ Address modes                                        01may95py
: index  ( breg1 ireg2 -- ireg )
    dup $10000 and >r 370 and swap dup $10000 and >r 7 and or
    SIB ! 1 SIB# ! r> r> 2* or 44 or ;
: #) ( disp -- reg )
    .64bit @ IF  disp !  44 55 index 4 disp# !  EXIT  THEN
    disp ! .anow @ IF  55 4  ELSE  66 2  THEN  disp# ! ;
: R#) ( disp -- reg )
    .64bit @ 0= abort" RIP address only in 64 bit mode"
    disp ! 4 disp# ! .arel on  55 ;
: *2   100 xor ;    : *4   200 xor ;    : *8   300 xor ;
: I) ( reg1 reg2 -- ireg )  .anow @ .64bit @ or 0= abort" No Index!"
  *8  index ;
: I#) ( disp32 reg -- ireg ) BP swap I) swap #) drop ;
: seg)  ( seg disp -- -1 )
  disp !  asize@ disp# !  imm ! 2 imm# !  -1 ;
: )  ( reg -- reg )  dup SP = IF dup I) ELSE 200077 and THEN ;
: D) ( disp reg -- reg )  ) >r dup disp !  $80 -$80 within
  Adisp? @ or IF  200 asize@  ELSE  100 1  THEN disp# ! r> or ;
: DI) ( disp reg1 reg2 -- ireg )  I) D) ;
: A: ( -- )  Adisp? on ;        : A::  ( -- )  -2 Adisp? ! ;
: A#) ( imm -- )  A: #) ;       : Aseg) ( * -- ) A: seg) ;
: ?fix-i ( r/m reg -- r/m reg )
    2dup 44 300 d= disp# @ 0= and  IF  >r 0 swap d) r>  THEN ;

\ # A# rel) CR DR TR ST <ST STP                        01jan98py
: # ( imm -- ) dup imm !  -$80 $80 within  byte? @ or
  IF  1  ELSE  osize@  THEN  imm# ! ;
: L#  ( imm -- )  imm !  osize@ imm# ! ;
: A#  ( imm -- )  Aimm? on  L# ;
: rel)  ( addr -- -2 )  disp ! asize@ disp# ! -2 ;
: L) ( disp reg -- reg ) ) >r disp ! 200 asize@ disp# ! r> or ;
: LI) ( disp reg1 reg2 -- reg ) I) L) ;
: >>mod ( reg1 reg2 -- mod )
    2dup or $80000 and $0F rshift .rex +!  
    dup $10000 and $0E rshift .rex +! 70 and swap
    dup $30000 and $10 rshift .rex +! 307 and or ;
: >reg ( reg -- reg' )  dup $10000 and $10 rshift .rex +! 7 and ;
: >mod ( reg1 reg2 -- )  >>mod modR/M !  1 modR/M# ! ;
: CR  ( n -- )  7 and 11 *  $1C0 or ;    0 CR constant CR0
: DR  ( n -- )  7 and 11 *  $2C0 or ;
: TR  ( n -- )  7 and 11 *  $3C0 or ;
: ST  ( n -- )  7 and       $5C0 or ;
: <ST ( n -- )  7 and       $7C0 or ;
: STP ( n -- )  7 and       $8C0 or ;

\ reg?                                                 10apr93py
: reg= ( reg flag mask -- flag ) third and = ;
: reg? ( reg -- reg flag )  $C0 $FFC0 reg= ;
: ?reg ( reg -- reg )  reg? 0= abort" reg expected!" ;
: ?mem ( mem -- mem )  dup $C0 < 0= abort" mem expected!" ;
: ?ax  ( reg -- reg )  dup AX <> abort" ax/al expected!" ;
: cr?  ( reg -- reg flag ) $100 $FF00 reg= ;
: dr?  ( reg -- reg flag ) $200 $FF00 reg= ;
: tr?  ( reg -- reg flag ) $300 $FF00 reg= ;
: sr?  ( reg -- reg flag ) $400 $FF00 reg= ;
: st?  ( reg -- reg flag ) dup $8 rshift 5 - ;
: ?st  ( reg -- reg ) st? 0< abort" st expected!" ;
: xr?  ( reg -- reg flag ) dup $FF > ;
: ?xr  ( reg -- reg )  xr? 0= abort" xr expected!" ;
: rel? ( reg -- reg flag ) dup -2 = ;
: seg? ( reg -- reg flag ) dup -1 = ;

\ Single Byte instruction                              27mar94py

: bc:   ( opcode -- )  Create c, DOES> c@ ,        ;
: bc.b: ( opcode -- )  Create c, DOES> c@ finishb  ;
: bc0F: ( opcode -- )  Create c, DOES> c@ finish0F ;

: seg:  ( opcode -- )  Create c, DOES> c@ seg ! ;

$26 seg: ES:    $2E seg: CS:    $36 seg: SS:    $3E seg: DS:
$64 seg: FS:    $65 seg: GS:

Forth

\ arithmetics                                          07nov92py

: reg>mod ( reg1 reg2 -- 1 / 3 )
    reg? IF  >mod 3  ELSE  swap ?reg >mod 1  THEN  ;
: ari: ( n -- ) Create c,
    DOES> ( reg1 reg2 / reg -- )  c@ >r imm# @
    IF    imm# @ byte? @ + 1 > over AX = and
          IF    drop $05 r> 70 and or
          ELSE  r> >mod $81 imm# @ 1 byte? @ + = IF 2 + THEN
          THEN
    ELSE  reg>mod  r> 70 and or
    THEN  finishb  ;

00 ari: add     11 ari: or      22 ari: adc     33 ari: sbb
44 ari: and     55 ari: sub     66 ari: xor     77 ari: cmp

\ bit shifts    strings                                07nov92py

: shift: ( n -- )  Create c,
    DOES> ( r/m -- )  c@ >mod  imm# @
    IF    imm @ 1 =
          IF  $D1 0  ELSE  $C1 1  THEN   imm# !
    ELSE  $D3
    THEN  finishb ;

00 shift: rol   11 shift: ror   22 shift: rcl   33 shift: rcr
44 shift: shl   55 shift: shr   66 shift: sal   77 shift: sar

$6D bc.b: ins   $6F bc.b: outs
$A5 bc.b: movs  $A7 bc.b: cmps
$AB bc.b: stos  $AD bc.b: lods  $AF bc.b: scas

\ movxr                                                07feb93py

: xr>mod  ( reg1 reg2 -- 0 / 2 )
    xr?  IF  >mod  2  ELSE  swap ?xr >mod  0  THEN  ;

: movxr  ( reg1 reg2 -- )
    -rex   2dup or sr? nip 
    IF    xr>mod  $8C or finish
    ELSE  2dup or $8 rshift 1+ -3 and >r  xr>mod  r> $20 or or finish0F
    THEN  ;

\ mov                                                  23jan93py

: assign#  byte? @ 0= IF  osize@ imm# !  ELSE 1 imm# ! THEN ;
: ?64off  .64bit @ .anow @ 0= and IF  10 disp# ! THEN  0 sib# ! ;
: ?ofax ( reg ax -- flag )
    .64bit @ IF  44  ELSE  .anow @ IF 55 ELSE 66 THEN  THEN AX d= ;
: mov ( r/m reg / reg r/m / reg -- )
  ?fix-i imm# @
  IF    assign#  reg?
        IF    >reg  $B8 or byte? @ 3 lshift xor  byte? off
	      .64now @ IF  10 imm# !  THEN
        ELSE  0 >mod  $C7  THEN
  ELSE  2dup or $FFFF and $FF > IF  movxr exit  THEN
        2dup ?ofax
        IF  2drop $A1  ?64off  ELSE  2dup swap  ?ofax
            IF  2drop $A3  ?64off  ELSE  reg>mod $88 or  THEN
        THEN
  THEN  finishb ;

\ not neg mul (imul div idiv                           29mar94py

: modf, ( r/m reg opcode -- )  -rot >mod finish   ;
: modfb ( r/m reg opcode -- )  -rot >mod finishb  ;
: mod0F ( r/m reg opcode -- )  -rot >mod finish0F ;
: modxx0f  ( r/m reg opcode -- )  -rot >mod finishxx0f ;
: modf:  Create  c,  DOES>  c@ modf, ;
: not: ( mode -- )  Create c, DOES> ( r/m -- ) c@ $F7 modfb ;

00 not: test#                 22 not: NOT     33 not: NEG
44 not: MUL     55 not: (IMUL 66 not: DIV     77 not: IDIV

: inc: ( mode -- )  Create c,
  DOES>  ( r/m -- )
    c@ >r reg?  byte? @ 0=  and .64bit @ 0= and
    IF    107 and r> 70 and or finish
    ELSE  r> $FF modfb   THEN ;
00 inc: INC     11 inc: DEC

\ test shld shrd                                       07feb93py

: test  ( reg1 reg2 / reg -- )  imm# @
  IF    assign#  AX case?
        IF  $A9  ELSE  test#  exit  THEN
  ELSE  ?reg >mod  $85  THEN  finishb ;

: shd ( r/m reg opcode -- )
    imm# @ IF  1 imm# ! 1-  THEN  mod0F ;
: shld  swap 245 shd ;          : shrd  swap 255 shd ;

: btx: ( r/m reg/# code -- )  Create c,
    DOES> c@ >r imm# @
    IF    1 imm# !  r> $BA
    ELSE  swap 203 r> >>mod  THEN  mod0F ;
44 btx: bt      55 btx: bts     66 btx: btr     77 btx: btc

\ push pop                                             05jun92py

: pushs   swap  FS case?  IF  $A0 or finish0F exit  THEN
                  GS case?  IF  $A8 or finish0F exit  THEN
    30 and 6 or or finish ;

: push  ( reg -- )
  -rex
  imm# @ 1 = IF  $6A finish exit  THEN
  imm# @     IF  $68 finish exit  THEN
  reg?       IF  >reg $50 or finish exit  THEN
  sr?        IF  0 pushs  exit  THEN
  66 $FF modf, ;
: pop   ( reg -- )
  -rex
  reg?       IF  >reg $58 or finish exit  THEN
  sr?        IF  1 pushs  exit  THEN
  06 $8F modf, ;

\ Ascii Arithmetics                                    22may93py

$27 bc: DAA     $2F bc: DAS     $37 bc: AAA     $3F bc: AAS

: aa:  Create c,
    DOES> ( -- ) c@
    imm# @ 0= IF  &10 imm !  THEN  1 imm# ! finish ;
$D4 aa: AAM     $D5 aa: AAD     $D6 bc: SALC    $D7 bc: XLAT

$60 bc: PUSHA   $61 bc: POPA
$90 bc: NOP
$98 bc: CBW     $99 bc: CWD                     $9B bc: FWAIT
$9C bc: PUSHF   $9D bc: POPF    $9E bc: SAHF    $9F bc: LAHF
                $C9 bc: LEAVE
$CC bc: INT3                    $CE bc: INTO    $CF bc: IRET
' fwait Alias wait

\ one byte opcodes                                     25dec92py

$F0 bc: LOCK                    $F2 bc: REP     $F3 bc: REPE
$F4 bc: HLT     $F5 bc: CMC
$F8 bc: CLC     $F9 bc: STC     $FA bc: CLI     $FB bc: STI
$FC bc: CLD     $FD bc: STD

: ?brange ( offword --- offbyte )  dup $80 -$80 within
    IF ." branch offset out of 1-byte range" THEN ;
: sb: ( opcode -- )  Create c,
    DOES> ( addr -- ) >r  [A] here [F] 2 + - ?brange
    disp !  1 disp# !  r> c@ -rex finish ;
$E0 sb: LOOPNE  $E1 sb: LOOPE   $E2 sb: LOOP    $E3 sb: JCXZ
: (ret ( op -- )  imm# @  IF  2 imm# !  1-  THEN  -rex finish ;
: ret  ( -- )  $C3  (ret ;
: retf ( -- )  $CB  (ret ;

\ call jmp                                             22dec93py

: call  ( reg / disp -- ) rel?
  IF  drop $E8 disp @ [A] here [F] 1+ asize@ + - disp ! finish
      exit  THEN  22 $FF -rex modf, ;
: callf ( reg / seg -- )
  seg? IF  drop $9A  finish exit  THEN  33 $FF -rex modf, ;

: jmp   ( reg / disp -- )
  -rex
  rel? IF  drop disp @ [A] here [F] 2 + - dup -$80 $80 within
           IF    disp ! 1 disp# !  $EB
           ELSE  3 - disp ! $E9  THEN  finish exit  THEN
  44 $FF modf, ;
: jmpf  ( reg / seg -- )
  seg? IF  drop $EA  finish exit  THEN  55 $FF -rex modf, ;

: next ( -- )
    \ assume dynamic code generation works, so NOOP's code can be copied
    \ Essentially assumes: code noop next end-code
    ['] noop >code-address ['] call >code-address over -
    here swap dup allot move ;

\ jump if                                              22dec93py

: cond: 0 DO  i Constant  LOOP ;

$10 cond: vs vc   u< u>=  0= 0<>  u<= u>   0< 0>=  ps pc   <  >=   <=  >
$10 cond: o  no   b  nb   z  nz   be  nbe  s  ns   pe po   l  nl   le  nle
: jmpIF  ( addr cond -- )
  swap [A] here [F] 2 + - dup -$80 $80 within
  IF            disp ! $70 1
  ELSE  0F,  4 - disp ! $80 4  THEN  disp# ! or  -rex finish ;
: jmp:  Create c,  DOES> c@ jmpIF ;
: jmps  0 DO  i jmp:  LOOP ;
$10 jmps jo  jno   jb  jnb   jz  jnz   jbe  jnbe  js  jns   jpe jpo   jl  jnl   jle  jnle

\ xchg                                                 22dec93py

: setIF ( r/m cond -- ) 0 swap $90 or mod0F ;
: set: ( cond -- )  Create c,  DOES>  c@ setIF ;
: sets: ( n -- )  0 DO  I set:  LOOP ;
$10 sets: seto setno  setb  setnb  sete setne  setna seta  sets setns  setpe setpo  setl setge  setle setg
: xchg ( r/m reg / reg r/m -- )
  over AX = IF  swap  THEN  reg?  0= IF  swap  THEN  ?reg
  byte? @ 0=  IF AX case?
  IF reg? IF >reg $90 or finish exit THEN  AX  THEN THEN
  $87 modfb ;

: movx ( r/m reg opcode -- ) 0F, modfb ;
: movsx ( r/m reg -- )  $BF movx ;
: movzx ( r/m reg -- )  $B7 movx ;

\ misc                                                 16nov97py

: ENTER ( imm8 -- ) 2 imm# ! $C8 -rex finish [A] , [F] ;
: ARPL ( reg r/m -- )  swap $63 modf, ;
$62 modf: BOUND ( mem reg -- )

: mod0F:  Create c,  DOES> c@ mod0F ;
$BC mod0F: BSF ( r/m reg -- )   $BD mod0F: BSR ( r/m reg -- )

$06 bc0F: CLTS
$08 bc0F: INVD  $09 bc0F: WBINVD

: CMPXCHG ( reg r/m -- ) swap $A7 movx ;
: CMPXCHG8B ( r/m -- )   $8 $C7 movx ;
: BSWAP ( reg -- )       >reg $C8 or finish0F ;
: XADD ( r/m reg -- )    $C1 movx ;

\ misc                                                 20may93py

: IMUL ( r/m reg -- )  imm# @ 0=
  IF  dup AX =  IF  drop (IMUL exit  THEN
      $AF mod0F exit  THEN
  >mod imm# @ 1 = IF  $6B  ELSE  $69  THEN  finish ;
: io ( oc -- )  imm# @ IF  1 imm# !  ELSE  $8 +  THEN finishb ;
: IN  ( -- ) $E5 io ;
: OUT ( -- ) $E7 io ;
: INT ( -- ) 1 imm# ! $CD finish ;
: 0F.0: ( r/m -- ) Create c, DOES> c@ $00 -rex mod0F ;
00 0F.0: SLDT   11 0F.0: STR    22 0F.0: LLDT   33 0F.0: LTR
44 0F.0: VERR   55 0F.0: VERW
: 0F.1: ( r/m -- ) Create c, DOES> c@ $01 -rex mod0F ;
00 0F.1: SGDT   11 0F.1: SIDT   22 0F.1: LGDT   33 0F.1: LIDT
44 0F.1: SMSW                   66 0F.1: LMSW   77 0F.1: INVLPG

\ misc                                                 29mar94py

$02 mod0F: LAR ( r/m reg -- )
$8D modf:  LEA ( m reg -- )
$C4 modf:  LES ( m reg -- )
$C5 modf:  LDS ( m reg -- )
$B2 mod0F: LSS ( m reg -- )
$B4 mod0F: LFS ( m reg -- )
$B5 mod0F: LGS ( m reg -- )
\ Pentium/AMD K5 codes
: cpuid ( -- )  0F, $A2 [A] , [F] ;
: cmpchx8b ( m -- ) 0 $C7 mod0F ;
: rdtsc ( -- )  0F, $31 [A] , [F] ;
: rdmsr ( -- )  0F, $32 [A] , [F] ;
: wrmsr ( -- )  0F, $30 [A] , [F] ;
: rsm ( -- )  0F, $AA [A] , [F] ;
: ud0 ( -- )  0F, $FF [A] , [F] ;
: ud2 ( -- )  0F, $0B [A] , [F] ;

\ Floating point instructions                          22dec93py

$D8 bc: D8,   $D9 bc: D9,   $DA bc: DA,   $DB bc: DB,
$DC bc: DC,   $DD bc: DD,   $DE bc: DE,   $DF bc: DF,

: D9: Create c, DOES> D9, c@ finish ;

Variable fsize
: .fs   0 fsize ! ;  : .fl   4 fsize ! ;  : .fx   3 fsize ! ;
: .fw   6 fsize ! ;  : .fd   2 fsize ! ;  : .fq   7 fsize ! ;
.fx
: fop:  Create c,  DOES>  ( fr/m -- ) c@ >r
    st? dup 0< 0= IF  swap r> >mod 2* $D8 + finish exit  THEN
    drop ?mem r> >mod $D8 fsize @ dup 1 and dup 2* + - +
    finish ;
: f@!: Create c,  DOES>  ( fm -- ) c@ $D9 modf, ;

\ Floating point instructions                          08jun92py

$D0 D9: FNOP

$E0 D9: FCHS    $E1 D9: FABS
$E4 D9: FTST    $E5 D9: FXAM
$E8 D9: FLD1    $E9 D9: FLDL2T  $EA D9: FLDL2E  $EB D9: FLDPI
$EC D9: FLDLG2  $ED D9: FLDLN2  $EE D9: FLDZ
$F0 D9: F2XM1   $F1 D9: FYL2X   $F2 D9: FPTAN   $F3 D9: FPATAN
$F4 D9: FXTRACT $F5 D9: FPREM1  $F6 D9: FDECSTP $F7 D9: FINCSTP
$F8 D9: FPREM   $F9 D9: FYL2XP1 $FA D9: FSQRT   $FB D9: FSINCOS
$FC D9: FRNDINT $FD D9: FSCALE  $FE D9: FSIN    $FF D9: FCOS

\ Floating point instructions                          23jan94py

00 fop: FADD    11 fop: FMUL    22 fop: FCOM    33 fop: FCOMP
44 fop: FSUB    55 fop: FSUBR   66 fop: FDIV    77 fop: FDIVR

: FCOMPP ( -- )  [A] 1 stp fcomp [F] ;
: FBLD   ( fm -- ) 44 $D8 modf, ;
: FBSTP  ( fm -- ) 66 $DF modf, ;
: FFREE  ( st -- ) 00 $DD modf, ;
: FSAVE  ( fm -- ) 66 $DD modf, ;
: FRSTOR ( fm -- ) 44 $DD modf, ;
: FINIT  ( -- )  [A] DB, $E3 , [F] ;
: FXCH   ( st -- ) 11 $D9 modf, ;

44 f@!: FLDENV  55 f@!: FLDCW   66 f@!: FSTENV  77 f@!: FSTCW

\ fild fst fstsw fucom                                 22may93py
: FUCOM ( st -- )  ?st st? IF 77 ELSE 66 THEN $DD modf, ;
: FUCOMPP ( -- )  [A] DA, $E9 , [F] ;
: FNCLEX  ( -- )  [A] DB, $E2 , [F] ;
: FCLEX   ( -- )  [A] fwait fnclex [F] ;
: FSTSW ( r/m -- )
  dup AX = IF  44  ELSE  ?mem 77  THEN  $DF modf, ;
: f@!,  fsize @ 1 and IF  drop  ELSE  nip  THEN
    fsize @ $D9 or modf, ;
: fx@!, ( mem/st l x -- )  rot  st? 0=
    IF  swap $DD modf, drop exit  THEN  ?mem -rot
    fsize @ 3 = IF drop $DB modf, exit THEN  f@!, ;
: FST  ( st/m -- ) st?  0=
  IF  22 $DD modf, exit  THEN  ?mem 77 22 f@!, ;
: FLD  ( st/m -- )  st? 0= IF 0 $D9 modf, exit THEN 55 0 fx@!, ;
: FSTP ( st/m -- )  77 33 fx@!, ;

\ PPro instructions                                    28feb97py


: cmovIF ( r/m r flag -- )  $40 or mod0F ;
: cmov:  Create c, DOES> c@ cmovIF ;
: cmovs:  0 DO  I cmov:  LOOP ;
$10 cmovs: cmovo  cmovno   cmovb   cmovnb   cmovz  cmovnz   cmovbe  cmovnbe   cmovs  cmovns   cmovpe  cmovpo   cmovl  cmovnl   cmovle  cmovnle

\ MMX/SSE2 opcodes                            02mar97py/14aug10dk

    300 7 regs    MM0   MM1   MM2   MM3   MM4   MM5   MM6   MM7
    300 7 xmmregs XMM0  XMM1  XMM2  XMM3  XMM4  XMM5  XMM6  XMM7
0200300 7 xmmregs XMM8  XMM9  XMM10 XMM11 XMM12 XMM13 XMM14 XMM15
: mmx  -rex  dquad? @ $660000 and or   modxx0f ;
: mmx: ( code -- )  Create c,  DOES> c@  mmx ;
: mmxs ?DO  I mmx:  LOOP ;

$64 $60 mmxs PUNPCKLBW PUNPCKLWD PUNOCKLDQ PACKUSDW
$68 $64 mmxs PCMPGTB   PCMPGTW   PCMPGTD   PACKSSWB
$6C $68 mmxs PUNPCKHBW PUNPCKHWD PUNPCKHDQ PACKSSDW
$78 $74 mmxs PCMPEQB   PCMPEQW   PCMPEQD   EMMS
$DA $D8 mmxs PSUBUSB   PSUBUSW
$EA $E8 mmxs PSUBSB    PSUBSW
$FB $F8 mmxs PSUBB     PSUBW     PSUBD
$DE $DC mmxs PADDUSB   PADDUSW
$EE $EC mmxs PADDSB    PADDSW
$FF $FC mmxs PADDB     PADDW     PADDD

\ MMX/SSE2 opcodes                            02mar97py/14aug10dk

$D5 mmx: pmullw               $E5 mmx: pmulhw
$F5 mmx: pmaddwd
$DB mmx: pand                 $DF mmx: pandn
$EB mmx: por                  $EF mmx: pxor
: pshift ( mmx imm/m mod op -- )
  imm# @ IF  1 imm# !  ELSE  + $50 +  THEN  mmx ;
: dqshift  ( xmm imm mod op -- )
   dquad? @ imm# @ AND 0= ABORT" Usage  xmm<NN> imm # <opcode>"
   pshift ;
: PSRLW ( mmx imm/m -- )  020 $71 pshift ;
: PSRLD ( mmx imm/m -- )  020 $72 pshift ;
: PSRLQ ( mmx imm/m -- )  020 $73 pshift ;
: PSRAW ( mmx imm/m -- )  040 $71 pshift ;
: PSRAD ( mmx imm/m -- )  040 $72 pshift ;
: PSLLW ( mmx imm/m -- )  060 $71 pshift ;
: PSLLD ( mmx imm/m -- )  060 $72 pshift ;
: PSLLQ ( mmx imm/m -- )  060 $73 pshift ;
: PSRLDQ ( xmm imm -- )   030 $73 dqshift ;  \ shifts by bytes, not bits!
: PSLLDQ ( xmm imm -- )   070 $73 dqshift ;

\ MMX opcodes                                         27jun99beu

\ mmxreg --> mmxreg move
\ (dk)this misses the reg->mem move, redefined in the SSE part below
\ $6F mod0F: MOVQ   

\ memory/reg32 --> mmxreg load
$6F mmx: PLDQ  \ Intel: MOVQ mm,m64
$6E mmx: PLDD  \ Intel: MOVD mm,m32/r

\ mmxreg --> memory/reg32
: PSTQ ( mm m64   -- ) SWAP  $7F mmx ; \ Intel: MOVQ m64,mm
: PSTD ( mm m32/r -- ) SWAP  $7E mmx ; \ Intel: MOVD m32/r,mm

\ 3Dnow! opcodes (K6)                                  21apr00py
: ib!  ( code -- )  [A] # [F]  1 imm# ! ;
: mod0F# ( code imm -- )  ib! mod0F ;
: 3Dnow: ( imm -- )  Create c,  DOES>  -rex $0f swap c@ mod0F# ;
$0D 3Dnow: PI2FD                $1D 3Dnow: PF2ID
$90 3Dnow: PFCMPGE              $A0 3Dnow: PFCMPGT
$94 3Dnow: PFMIN                $A4 3Dnow: PFMAX
$96 3Dnow: PFRCP                $A6 3Dnow: PFRCPIT1
$97 3Dnow: PFRSQRT              $A7 3Dnow: PFRSQIT1
$9A 3Dnow: PFSUB                $AA 3Dnow: PFSUBR
$9E 3Dnow: PFADD                $AE 3Dnow: PFACC
$B0 3Dnow: PFCMPEQ              $B4 3Dnow: PFMUL
$B6 3Dnow: PFRCPIT2             $B7 3Dnow: PMULHRW
$BF 3Dnow: PAVGUSB

: FEMMS  -rex $0E finish0F ;
: PREFETCH  -rex 000 $0D mod0F ;    : PREFETCHW  -rex 010 $0D mod0F ;

\ 3Dnow!+MMX/SSE2 opcodes (Athlon/Athlon64)   21apr00py/14aug10dk

$F7 mmx: MASKMOVQ             $E7 mmx: MOVNTQ
$E0 mmx: PAVGB                $E3 mmx: PAVGW
$C5 mmx: PEXTRW               $C4 mmx: PINSRW
$EE mmx: PMAXSW               $DE mmx: PMAXUB
$EA mmx: PMINSW               $DA mmx: PMINUB
$D7 mmx: PMOVMSKB             $E4 mmx: PMULHUW
$F6 mmx: PSADBW               $70 mmx: PSHUFW

$0C 3Dnow: PI2FW                $1C 3Dnow: PF2IW
$8A 3Dnow: PFNACC               $8E 3Dnow: PFPNACC
$BB 3Dnow: PSWABD                  : SFENCE  .d $ae $f8 ib! finish0f ;     
: PREFETCHNTA  .d 000 $18 mod0F ;  : PREFETCHT0 .d 010 $18 mod0F ;
: PREFETCHT1   .d 020 $18 mod0F ;  : PREFETCHT2 .d  030 $18 mod0F ;

\ MMX/SSE/SSE2 moves                                     12aug10dk

: movxx  ( mod opcode1-regdst opcode2-memdst rex&mmx? -- )
   >r 2>r
   reg>mod 3 = if 2r> drop else  2r> nip then
   r@ 1 and if dquad? @ $660000 and or then
   r> 2 and 0= if -rex then
   finishxx0f ;
   
: movxx:  ( opcode1-regdst opcode2-memdst rex&mmx? -- )  create , 2,
  does>  ( xmm1 xmm2/mem | xmm1/mem xmm2   a-addr -- )
   dup @  swap cell+ 2@  rot  movxx ;

$0f10   $0f11   0 movxx: movups   $660f10 $660f11 0 movxx: movupd
$0f12   $0f13   0 movxx: movlps   $660f12 $660f13 0 movxx: movlpd
$0f16   $0f17   0 movxx: movhps   $660f16 $660f17 0 movxx: movhpd
$0f28   $0f29   0 movxx: movaps   $660f28 $660f29 0 movxx: movapd
$f30f10 $f30f11 0 movxx: movss    $f20f10 $f20f11 0 movxx: movsd   
$660f6f $660f7f 0 movxx: movdqa   $f30f6f $f30f7f 0 movxx: movdqu
$0f6e   $0f7e   3 movxx: movd   \ use .d/.q prefix to select operand size!

: maskmovdqu  .dq maskmovq ;
: movq  ( xmm/mmx mmx/xmm/mem64 | mmx/xmm/mem64 xmm/mmx )
   dquad? @ if  $f30f7e $660fd6
   else           $0f6f   $0f7f then  0 movxx ;

\ SSE floating point arithmetic                          12aug10dk
: sse:  ( opc1 rex? "name" -- ) create swap , c,
   does> ( xmm1 xmm2/mem a-addr -- )
   dup @  swap cell+ c@ 0= if -rex then   modxx0f ;
: sses  ( prefixN..prefix1 opc n "name1" ... "nameN"  -- )
   0 do   swap $10 lshift over or  0 sse:  loop  drop ;
: sse2xa  ( opc "name1" "name2-66"  -- )   $66 $00 rot 2 sses ;
: sse2xb  ( opc "name1-f3" "name2-f2"  -- )   $f2 $f3 rot 2 sses ;
: sse2xc  ( opc "name1-f3" "name2-f2"  -- )  $66 $f2 rot 2 sses ;
: sse4x  ( opc "name1" "name4"  -- )   dup sse2xa sse2xb ;

$0f58 sse4x addps addpd addss addsd
$0f5c sse4x subps subpd subss subsd  
$0f5f sse4x maxps maxpd maxss maxsd
$0f5d sse4x minps minpd minss minsd
$0f59 sse4x mulps mulpd mulss mulsd
$0f5e sse4x divps divpd divss divsd
$0f54 sse2xa andps andpd
$0f55 sse2xa andnps andnpd
$0f56 sse2xa orps orpd
$0f57 sse2xa xorps xorpd
$0f2e sse2xa ucomiss ucomisd
$0f2f sse2xa comiss comisd
$0f5b sse2xa cvtdq2ps cvtps2dq   $f30f5b 0 sse: cvttps2dq 
$0fe6 sse2xb cvtdq2pd cvtpd2dq   $660f5b 0 sse: cvttpd2dq 
$0f2d sse2xa cvtps2pi cvtpd2pi   
$0f2a sse2xa cvtpi2ps cvtpi2pd   
( these take .d/.q size prefix:)
$f30f2d 1 sse: cvtss2si
$f20f2d 1 sse: cvtsd2si
$f20f2a 1 sse: cvtsi2sd
$f30f2a 1 sse: cvtsi2ss

\ $0f5e sse2xa divps divpd \ already defined above
$0f7c sse2xc haddps haddpd
$0f7d sse2xc hsubps hsubpd
$0fd0 sse2xc addsubps addsubpd

: cmp: ( opc #cmp "name" -- )
   create swap , c,
   does>  ( xmm1/mem xmm2 a-addr -- )
   dup @ swap cell+ c@ ib! -rex modxx0f ;
: cmps:  ( opc "name1" ... "name8" -- )  $8 0 do  dup i cmp: loop  drop ;
$0fc2 cmps: cmpeqps cmpltps cmpleps cmpunordps cmpneqps cmpnltps cmpnleps cmpordps
$660fc2 cmps: cmpeqpd cmpltpd cmplepd cmpunordpd cmpneqpd cmpnltpd cmpnlepd cmpordpd
$f30fc2 cmps: cmpeqss cmpltss cmpless cmpunordss cmpneqss cmpnltss cmpnless cmpordss
$f20fc2 cmps: cmpeqsd cmpltsd cmplesd cmpunordsd cmpneqsd cmpnltsd cmpnlesd cmpordsd 

\ Assembler Conditionals                               22dec93py
: ~cond ( cond -- ~cond )  1 xor ;
: >offset ( start dest --- offbyte )  swap  2 + -  ?brange ;
: IF ( cond -- here )  [A] here [F] dup 2 + rot  ~cond  jmpIF ;
: THEN       dup [A] here >offset swap 1+ c! [F] ;
: AHEAD      [A] here [F] dup 2 + rel) jmp ;
: ELSE       [A] AHEAD swap THEN [F] ;
: BEGIN      [A] here ;         ' BEGIN Alias DO  [F]
: WHILE      [A] IF [F] swap ;
: UNTIL      ~cond  jmpIF ;
: AGAIN      rel) jmp ;
: REPEAT     [A] AGAIN  THEN [F] ;
: ?DO        [A] here [F] dup 2 + dup jcxz ;
: BUT        swap ;
: YET        dup ;
: makeflag   [A] ~cond AL swap setIF  1 # AX and  AX dec [F] ;


previous previous set-current decimal base !

