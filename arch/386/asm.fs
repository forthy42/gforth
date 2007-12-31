\         *** Assembler for the Intel i486 ***         07nov92py

\ Copyright (C) 1992-2000 by Bernd Paysan

\ Copyright (C) 2000,2001,2003,2007 Free Software Foundation, Inc.

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
\ 
\ in .86 mode  .w is the default size, in .386 mode  .d is default
\ .wa and .da change address size. .b, .w(a) and .d(a) are not
\ switches like in my assem68k, they are prefixes.
\ [A-D][L|H] implicitely set the .b size. So
\ AH AL mov
\ generates a byte move. Sure you need .b for memory operations
\ like .b ax ) inc    which is  inc  BYTE PTR [eAX]

\ 80486 Assembler Load Screen                          21apr00py

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

\ Stack-Buffer fÅr Extra-Werte                         22dec93py

Variable ModR/M               Variable ModR/M#
Variable SIB                  Variable SIB#
Variable disp                 Variable disp#
Variable imm                  Variable imm#
Variable Aimm?                Variable Adisp?
Variable byte?                Variable seg
Variable .asize               Variable .anow
Variable .osize               Variable .onow
: pre-    seg off  .asize @ .anow !  .osize @ .onow !  ;
: sclear  pre-  Aimm? off  Adisp? off
    ModR/M# off  SIB# off  disp# off  imm# off  byte? off ;

: .b  1 byte? !  imm# @ 1 min imm# ! ;

: .w   .onow off ;              : .wa  .anow off ;
: .d   .onow on  ;              : .da  .anow on  ;

\ Extra-Werte compilieren                              01may95py
: bytes,  ( nr x n -- )
    0 ?DO  over 0< IF  +rel  swap 1+ swap  THEN  dup ,  $8 rshift
    LOOP   2drop ;
: opcode, ( opcode -- )
    .asize @ .anow @  <> IF  $67 ,  THEN
    .osize @ .onow @  <> IF  $66 ,  THEN
    seg     @ IF  seg @ ,  THEN  ,  pre- ;
: finish ( opcode -- )  opcode,
    ModR/M# @ IF  ModR/M @ ,  THEN
    SIB#    @ IF  SIB    @ ,  THEN
    Adisp?  @ disp @ disp# @ bytes,
    Aimm?   @ imm  @ imm#  @ bytes,    sclear  ;
: finishb  ( opcode -- )       byte? @ xor  finish ;
: 0F,  $0F opcode, ;
: finish0F ( opcode -- )       0F,  finish ;

\ Register                                             29mar94py

: Regs  ( mod n -- ) FOR  dup Constant 11 +  NEXT  drop ;
: breg  ( reg -- )  Create c,  DOES> c@  .b ;
: bregs ( mod n -- ) FOR  dup breg     11 +  NEXT  drop ;
: wadr: ( reg -- )  Create c,  DOES> c@  .wa ;
: wadr  ( mod n -- ) FOR  dup wadr:    11 +  NEXT  drop ;
   0 7 wadr [BX+SI] [BX+DI] [BP+SI] [BP+DI] [SI] [DI] [BP] [BX]
 300 7 regs  AX CX DX BX SP BP SI DI
 300 7 bregs AL CL DL BL AH CH DH BH
2300 5 regs ES CS SS DS FS GS
' SI alias RP   ' BP alias UP   ' DI Alias OP
: .386  .asize on   .osize on  sclear ;  .386
: .86   .asize off  .osize off sclear ;
: asize@  2 .anow @ IF  2*  THEN ;
: osize@  2 .onow @ IF  2*  THEN ;

\ Address modes                                        01may95py
: #) ( disp -- reg )
  disp ! .anow @ IF  55 4  ELSE  66 2  THEN  disp# ! ;
: *2   100 xor ;    : *4   200 xor ;    : *8   300 xor ;
: index  ( reg1 reg2 -- modr/m )  370 and swap 7 and or ;
: I) ( reg1 reg2 -- ireg )  .anow @ 0= abort" No Index!"
  *8  index  SIB ! 1 SIB# ! 44 ;
: I#) ( disp32 reg -- ireg ) BP swap I) swap #) drop ;
: seg)  ( seg disp -- -1 )
  disp !  asize@ disp# !  imm ! 2 imm# !  -1 ;
: )  ( reg -- reg )  dup SP = IF dup I) ELSE 77 and THEN ;
: D) ( disp reg -- reg )  ) >r dup disp !  $80 -$80 within
  Adisp? @ or IF  200 asize@  ELSE  100 1  THEN disp# ! r> or ;
: DI) ( disp reg1 reg2 -- ireg )  I) D) ;
: A: ( -- )  Adisp? on ;        : A::  ( -- )  -2 Adisp? ! ;
: A#) ( imm -- )  A: #) ;       : Aseg) ( * -- ) A: seg) ;

\ # A# rel) CR DR TR ST <ST STP                        01jan98py
: # ( imm -- ) dup imm !  -$80 $80 within  byte? @ or
  IF  1  ELSE  osize@  THEN  imm# ! ;
: L#  ( imm -- )  imm !  osize@ imm# ! ;
: A#  ( imm -- )  Aimm? on  L# ;
: rel)  ( addr -- -2 )  disp ! asize@ disp# ! -2 ;
: L) ( disp reg -- reg ) ) >r disp ! 200 asize@ disp# ! r> or ;
: LI) ( disp reg1 reg2 -- reg ) I) L) ;
: >>mod ( reg1 reg2 -- mod )  70 and swap 307 and or ;
: >mod ( reg1 reg2 -- )  >>mod modR/M !  1 modR/M# ! ;
: CR  ( n -- )  7 and 11 *  $1C0 or ;    0 CR constant CR0
: DR  ( n -- )  7 and 11 *  $2C0 or ;
: TR  ( n -- )  7 and 11 *  $3C0 or ;
: ST  ( n -- )  7 and       $5C0 or ;
: <ST ( n -- )  7 and       $7C0 or ;
: STP ( n -- )  7 and       $8C0 or ;

\ reg?                                                 10apr93py
: reg= ( reg flag mask -- flag ) 2 pick and = ;
: reg? ( reg -- reg flag )  $C0 -$40 reg= ;
: ?reg ( reg -- reg )  reg? 0= abort" reg expected!" ;
: ?mem ( mem -- mem )  dup $C0 < 0= abort" mem expected!" ;
: ?ax  ( reg -- reg )  dup AX <> abort" ax/al expected!" ;
: cr?  ( reg -- reg flag ) $100 -$100 reg= ;
: dr?  ( reg -- reg flag ) $200 -$100 reg= ;
: tr?  ( reg -- reg flag ) $300 -$100 reg= ;
: sr?  ( reg -- reg flag ) $400 -$100 reg= ;
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
    2dup or sr? nip
    IF    xr>mod  $8C
    ELSE  2dup or $8 rshift 1+ -3 and >r  xr>mod  0F,  r> $20 or
    THEN  or  finish ;

\ mov                                                  23jan93py

: assign#  byte? @ 0= IF  osize@ imm# !  ELSE 1 imm# ! THEN ;

: ?ofax ( reg ax -- flag ) .anow @ IF 55 ELSE 66 THEN AX d= ;
: mov ( r/m reg / reg r/m / reg -- )  2dup or 0> imm# @ and
  IF    assign#  reg?
        IF    7 and  $B8 or byte? @ 3 lshift xor  byte? off
        ELSE  0 >mod  $C7  THEN
  ELSE  2dup or $FF > IF  movxr exit  THEN
        2dup ?ofax
        IF  2drop $A1  ELSE  2dup swap  ?ofax
            IF  2drop $A3  ELSE  reg>mod $88 or  THEN
        THEN
  THEN  finishb ;

\ not neg mul (imul div idiv                           29mar94py

: modf  ( r/m reg opcode -- )  -rot >mod finish   ;
: modfb ( r/m reg opcode -- )  -rot >mod finishb  ;
: mod0F ( r/m reg opcode -- )  -rot >mod finish0F ;
: modf:  Create  c,  DOES>  c@ modf ;
: not: ( mode -- )  Create c, DOES> ( r/m -- ) c@ $F7 modfb ;

00 not: test#                 22 not: NOT     33 not: NEG
44 not: MUL     55 not: (IMUL 66 not: DIV     77 not: IDIV

: inc: ( mode -- )  Create c,
    DOES>  ( r/m -- ) c@ >r reg?  byte? @ 0=  and
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
  imm# @ 1 = IF  $6A finish exit  THEN
  imm# @     IF  $68 finish exit  THEN
  reg?       IF  7 and $50 or finish exit  THEN
  sr?        IF  0 pushs  exit  THEN
  66 $FF modf ;
: pop   ( reg -- )
  reg?       IF  7 and $58 or finish exit  THEN
  sr?        IF  1 pushs  exit  THEN
  06 $8F modf ;

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
    disp !  1 disp# !  r> c@ finish ;
$E0 sb: LOOPNE  $E1 sb: LOOPE   $E2 sb: LOOP    $E3 sb: JCXZ
: (ret ( op -- )  imm# @  IF  2 imm# !  1-  THEN  finish ;
: ret  ( -- )  $C3  (ret ;
: retf ( -- )  $CB  (ret ;

\ call jmp                                             22dec93py

: call  ( reg / disp -- ) rel?
  IF  drop $E8 disp @ [A] here [F] 1+ asize@ + - disp ! finish
      exit  THEN  22 $FF modf ;
: callf ( reg / seg -- )
  seg? IF  drop $9A  finish exit  THEN  33 $FF modf ;

: jmp   ( reg / disp -- )
  rel? IF  drop disp @ [A] here [F] 2 + - dup -$80 $80 within
           IF    disp ! 1 disp# !  $EB
           ELSE  3 - disp ! $E9  THEN  finish exit  THEN
  44 $FF modf ;
: jmpf  ( reg / seg -- )
  seg? IF  drop $EA  finish exit  THEN  55 $FF modf ;

: next ['] noop >code-address rel) jmp ;

\ jump if                                              22dec93py

: cond: 0 DO  i Constant  LOOP ;

$10 cond: vs vc   u< u>=  0= 0<>  u<= u>   0< 0>=  ps pc   <  >=   <=  >
$10 cond: o  no   b  nb   z  nz   be  nbe  s  ns   pe po   l  nl   le  nle
: jmpIF  ( addr cond -- )
  swap [A] here [F] 2 + - dup -$80 $80 within
  IF            disp ! $70 1
  ELSE  0F,  4 - disp ! $80 4  THEN  disp# ! or finish ;
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
  IF reg? IF 7 and $90 or finish exit THEN  AX  THEN THEN
  $87 modfb ;

: movx ( r/m reg opcode -- ) 0F, modfb ;
: movsx ( r/m reg -- )  $BF movx ;
: movzx ( r/m reg -- )  $B7 movx ;

\ misc                                                 16nov97py

: ENTER ( imm8 -- ) 2 imm# ! $C8 finish [A] , [F] ;
: ARPL ( reg r/m -- )  swap $63 modf ;
$62 modf: BOUND ( mem reg -- )

: mod0F:  Create c,  DOES> c@ mod0F ;
$BC mod0F: BSF ( r/m reg -- )   $BD mod0F: BSR ( r/m reg -- )

$06 bc0F: CLTS
$08 bc0F: INVD  $09 bc0F: WBINVD

: CMPXCHG ( reg r/m -- ) swap $A7 movx ;
: CMPXCHG8B ( r/m -- )   $8 $C7 movx ;
: BSWAP ( reg -- )       7 and $C8 or finish0F ;
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
: 0F.0: ( r/m -- ) Create c, DOES> c@ $00 mod0F ;
00 0F.0: SLDT   11 0F.0: STR    22 0F.0: LLDT   33 0F.0: LTR
44 0F.0: VERR   55 0F.0: VERW
: 0F.1: ( r/m -- ) Create c, DOES> c@ $01 mod0F ;
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
: f@!: Create c,  DOES>  ( fm -- ) c@ $D9 modf ;

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
: FBLD   ( fm -- ) 44 $D8 modf ;
: FBSTP  ( fm -- ) 66 $DF modf ;
: FFREE  ( st -- ) 00 $DD modf ;
: FSAVE  ( fm -- ) 66 $DD modf ;
: FRSTOR ( fm -- ) 44 $DD modf ;
: FINIT  ( -- )  [A] DB, $E3 , [F] ;
: FXCH   ( st -- ) 11 $D9 modf ;

44 f@!: FLDENV  55 f@!: FLDCW   66 f@!: FSTENV  77 f@!: FSTCW

\ fild fst fstsw fucom                                 22may93py
: FUCOM ( st -- )  ?st st? IF 77 ELSE 66 THEN $DD modf ;
: FUCOMPP ( -- )  [A] DA, $E9 , [F] ;
: FNCLEX  ( -- )  [A] DB, $E2 , [F] ;
: FCLEX   ( -- )  [A] fwait fnclex [F] ;
: FSTSW ( r/m -- )
  dup AX = IF  44  ELSE  ?mem 77  THEN  $DF modf ;
: f@!,  fsize @ 1 and IF  drop  ELSE  nip  THEN
    fsize @ $D9 or modf ;
: fx@!, ( mem/st l x -- )  rot  st? 0=
    IF  swap $DD modf drop exit  THEN  ?mem -rot
    fsize @ 3 = IF drop $DB modf exit THEN  f@!, ;
: FST  ( st/m -- ) st?  0=
  IF  22 $DD modf exit  THEN  ?mem 77 22 f@!, ;
: FLD  ( st/m -- )  st? 0= IF 0 $D9 modf exit THEN 55 0 fx@!, ;
: FSTP ( st/m -- )  77 33 fx@!, ;

\ PPro instructions                                    28feb97py


: cmovIF ( r/m r flag -- )  $40 or mod0F ;
: cmov:  Create c, DOES> c@ cmovIF ;
: cmovs:  0 DO  I cmov:  LOOP ;
$10 cmovs: cmovo  cmovno   cmovb   cmovnb   cmovz  cmovnz   cmovbe  cmovnbe   cmovs  cmovns   cmovpe  cmovpo   cmovl  cmovnl   cmovle  cmovnle

\ MMX opcodes                                          02mar97py

300 7 regs MM0 MM1 MM2 MM3 MM4 MM5 MM6 MM7

: mmxs ?DO  I mod0F:  LOOP ;
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

\ MMX opcodes                                          02mar97py

$D5 mod0F: pmullw               $E5 mod0F: pmulhw
$F5 mod0F: pmaddwd
$DB mod0F: pand                 $DF mod0F: pandn
$EB mod0F: por                  $EF mod0F: pxor
: pshift ( mmx imm/m mod op -- )
  imm# @ IF  1 imm# !  ELSE  + $50 +  THEN  mod0F ;
: PSRLW ( mmx imm/m -- )  020 $71 pshift ;
: PSRLD ( mmx imm/m -- )  020 $72 pshift ;
: PSRLQ ( mmx imm/m -- )  020 $73 pshift ;
: PSRAW ( mmx imm/m -- )  040 $71 pshift ;
: PSRAD ( mmx imm/m -- )  040 $72 pshift ;
: PSLLW ( mmx imm/m -- )  060 $71 pshift ;
: PSLLD ( mmx imm/m -- )  060 $72 pshift ;
: PSLLQ ( mmx imm/m -- )  060 $73 pshift ;

\ MMX opcodes                                         27jun99beu

\ mmxreg --> mmxreg move
$6F mod0F: MOVQ

\ memory/reg32 --> mmxreg load
$6F mod0F: PLDQ  \ Intel: MOVQ mm,m64
$6E mod0F: PLDD  \ Intel: MOVD mm,m32/r

\ mmxreg --> memory/reg32
: PSTQ ( mm m64   -- ) SWAP  $7F mod0F ; \ Intel: MOVQ m64,mm
: PSTD ( mm m32/r -- ) SWAP  $7E mod0F ; \ Intel: MOVD m32/r,mm

\ 3Dnow! opcodes (K6)                                  21apr00py
: mod0F# ( code imm -- )  # 1 imm ! mod0F ;
: 3Dnow: ( imm -- )  Create c,  DOES> c@ mod0F# ;
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

: FEMMS  $0E finish0F ;
: PREFETCH  000 $0D mod0F ;    : PREFETCHW  010 $0D mod0F ;

\ 3Dnow!+MMX opcodes (Athlon)                          21apr00py

$F7 mod0F: MASKMOVQ             $E7 mod0F: MOVNTQ
$E0 mod0F: PAVGB                $E3 mod0F: PAVGW
$C5 mod0F: PEXTRW               $C4 mod0F: PINSRW
$EE mod0F: PMAXSW               $DE mod0F: PMAXUB
$EA mod0F: PMINSW               $DA mod0F: PMINUB
$D7 mod0F: PMOVMSKB             $E4 mod0F: PMULHUW
$F6 mod0F: PSADBW               $70 mod0F: PSHUFW

$0C 3Dnow: PI2FW                $1C 3Dnow: PF2IW
$8A 3Dnow: PFNACC               $8E 3Dnow: PFPNACC
$BB 3Dnow: PSWABD               : SFENCE   $AE $07 mod0F# ;
: PREFETCHNTA  000 $18 mod0F ;  : PREFETCHT0  010 $18 mod0F ;
: PREFETCHT1   020 $18 mod0F ;  : PREFETCHT2  030 $18 mod0F ;

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

