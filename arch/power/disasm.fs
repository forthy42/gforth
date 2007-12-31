\ disasm.fs	disassembler file (for PPC32/64)
\
\ Copyright (C) 2006,2007 Free Software Foundation, Inc.

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

get-current 
vocabulary disassembler
also disassembler definitions

\ convention for disasm words, please refer to ibm man pages 655ff
\ since for example BO refers to bits 6 till 10 of instruction bcx und D refers
\ to the same bits, i won't define two words disasm-bo and disasm-d but only one
\ disasm-6,10 if only one number is given it refers to a single bit

: disasm-0,5 ( w -- u )
  26 rshift ;

: disasm-31 ( w -- u )
  $1 and ;

: disasm-30 ( w -- u )
  1 rshift $1 and ;

: disasm-6,29 ( w -- u )
  2 rshift $FFFFFF and ;

: disasm-6,10 ( w -- u )
  21 rshift $1F and ;

: disasm-11,15 ( w -- u )
  16 rshift $1F and ;

: disasm-16,29 ( w -- u )
  2 rshift $3FFF and ;

: disasm-16,31 ( w -- u )
  $FFFF and ;

: disasm-6,8 ( w -- u )
  23 rshift $7 and ;

: disasm-10 ( w -- u )
  21 rshift $1 and ;

: disasm-30,31 ( w -- u )
  $3 and ;

: disasm-16,20 ( w -- u )
  11 rshift $1F and ;

: disasm-21,30 ( w -- u )
  1 rshift $3FF and ;

: disasm-11,13 ( w -- u )
  18 rshift $7 and ;

: disasm-12,15 ( w -- u )
  16 rshift $F and ;

: disasm-16,19 ( w -- u )
  12 rshift $F and ;

: disasm-11,20 ( w -- u )
  11 rshift $3FF and ;

: disasm-12,19 ( w -- u )
  12 rshift $FF and ;

: disasm-7,14 ( w -- u )
  17 rshift $FF and ;

: disasm-21,29 ( w -- u )
  2 rshift $1FF and ;

: disasm-21 ( w -- u )
  10 rshift $1 and ;

: disasm-22,30 ( w -- u )
  1 rshift $1ff and ;

: disasm-26,30 ( w -- u )
  1 rshift $1F and ;

: disasm-21,25 ( w -- u )
  6 rshift $1F and ;

: disasm-21,26 ( w -- u )
  5 rshift $3F and ;

: disasm-27,29 ( w -- u )
  2 rshift $7 and ;

: disasm-27,30 ( w -- u )
  1 rshift $F and ;

: disasm-illegal ( addr w -- )
  hex. ." , ( illegal inst ) " drop ;

: disasm-table ( n "name" -- )
  create 0 ?do
    ['] disasm-illegal ,
  loop
does> ( u -- addr )
  swap cells + ;

$400 disasm-table xl-tab-entry
$40  disasm-table opc-tab-entry
$4   disasm-table ds-58-tab-entry
$4   disasm-table ds-62-tab-entry
$20  disasm-table a-dis-tab-entry
$2   disasm-table i-tab-entry
$2   disasm-table b-tab-entry

: disasm-unknown ( addr w -- addr w flag )
  \ used to init tables for instructions with opcode 31, since there are forms 
  \ the XO starts from 22,30 and also 21,30 , this word will put a flag on the
  \ stack which signiliasies wheter the word for disasm-22,30, disasm-21,30 or
  \ disasm-21,29 (i.e. opcode 31) should be invoked.
  true ;

: disasm-unknown-table ( n "name" -- )
  create 0 ?do
    ['] disasm-unknown ,
  loop
does> ( u -- addr )
  swap cells + ;
  
\ XXX resize

$200 disasm-unknown-table xs-tab-entry
$400 disasm-unknown-table x-31-tab-entry
$400 disasm-unknown-table x-63-tab-entry
$200 disasm-unknown-table xo-tab-entry
$8   disasm-unknown-table md-tab-entry
$10  disasm-unknown-table mds-tab-entry
$20  disasm-unknown-table a-tab-entry

dup set-current 

: disasm-inst ( addr w -- )
  dup disasm-0,5 opc-tab-entry @ execute ;

\ for ppc32
1 cells 4 =
[if] 
  : disasm ( addr u -- )
    bounds u+do
      cr ." ( " i hex. ." ) " i i @ disasm-inst
      1 cells +loop
    cr ;
[endif]

\ for ppc64
1 cells 8 =
[if]
  : get-inst ( u -- o )
    32 rshift $FFFFFFFF and ;

  : disasm ( addr u -- )
    bounds u+do
      cr ." ( " i hex. ." ) " i i @ get-inst disasm-inst
      \ next inst plus 4
      4 +loop
    cr ;
[endif]

' disasm IS discode

definitions

: ext-16 
  \ word makes a 16 bit long int signed
  dup $8000 and if -$8000 or then ;

: get-xo,x,m,a-flag ( addr o -- addr )
  case
    0 of ." " endof
    1 of ." ." endof
    2 of ." o" endof
    3 of ." o." endof
  endcase ;

: disasm-md-11,15-6,10-16,20-30-21,26 ( addr w -- addr w flag )
  dup disasm-11,15 .
  dup disasm-6,10 .
  dup dup disasm-16,20 swap disasm-30 5 lshift or .
  dup dup disasm-21,26 1 rshift swap  disasm-21,26 1 and 5 lshift or . false ;

: disasm-mds-11,15-6,10-16,20-21,26 ( addr w -- addr w flag )
  dup disasm-11,15 .
  dup disasm-6,10 .
  dup disasm-16,20 .
  dup dup disasm-21,26 1 rshift swap  disasm-21,26 1 and 5 lshift or . false ;

: disasm-30-similar ( addr w -- )
  dup disasm-27,29 md-tab-entry @ execute
  invert if disasm-31 get-xo,x,m,a-flag drop
  else 
    dup disasm-27,30 mds-tab-entry @ execute
    invert if disasm-31 get-xo,x,m,a-flag drop 
      else disasm-illegal endif
  endif ;
' disasm-30-similar 30 opc-tab-entry !

: disasm-m ( addr w -- )
  dup disasm-11,15 .
  dup disasm-6,10 .
  dup disasm-16,20 .
  dup disasm-21,25 .
  dup disasm-26,30 . disasm-31 get-xo,x,m,a-flag drop ;

: get-xl-flag ( addr o -- addr )
  case
    0 of ." " endof
    1 of ." l" endof
  endcase ;
  
: disasm-xl ( addr w -- )
 dup disasm-21,30 xl-tab-entry @ execute 
 disasm-31 get-xl-flag drop ;
' disasm-xl 19 opc-tab-entry !

: disasm-xl-6,10-11,15 ( addr w -- addr w )
  dup disasm-6,10 .
  dup disasm-11,15 . ;

: disasm-xl-6,10-11,15-16,20 ( addr w -- addr w )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-16,20 . ;

: disasm-nothing ( addr w -- addr w ) ;

: disasm-xl-6,8-11,13 ( addr w -- addr w )
  dup disasm-6,8 .
  dup disasm-11,13 . ;

: disasm-d-oper-1 ( addr w -- ) 
  dup disasm-6,10 .
  dup disasm-11,15 .
  disasm-16,31 ext-16 .
  drop ;

: disasm-d-oper-2 ( addr w -- ) 
  dup disasm-11,15 .
  dup disasm-6,10 .
  disasm-16,31 .
  drop ;

: disasm-d-load-store ( addr w -- )
  dup disasm-6,10 .
  dup disasm-16,31 ext-16 . 
  disasm-11,15 .
  drop ;

: disasm-d-compare-1 ( addr w -- )
  dup disasm-6,8 .
  dup disasm-10 .
  dup disasm-11,15 .
  disasm-16,31 ext-16 .
  drop ;

: disasm-d-compare-2 ( addr w -- )
  dup disasm-6,8 .
  dup disasm-10 .
  dup disasm-11,15 .
  disasm-16,31 .
  drop ;

: disasm-xo-flags ( w -- u )
  dup disasm-21 1 lshift swap disasm-31 or ;

\ abstraction for opc 63 similar words
: disasm-63-similar ( add w -- )
  dup disasm-26,30 a-tab-entry @ execute 
  invert if disasm-31 get-xo,x,m,a-flag drop
    else dup disasm-21,30 x-63-tab-entry @ execute
      invert if disasm-31 get-xo,x,m,a-flag drop 
        else disasm-illegal endif
  endif ;
' disasm-63-similar 63 opc-tab-entry !

\ word which should be an abstraction for opcode 31 similar forms
: disasm-31-similar ( addr w -- )
 dup disasm-21,29 xs-tab-entry @ execute 
 invert if disasm-31 get-xo,x,m,a-flag drop
   else dup disasm-21,30 x-31-tab-entry @ execute 
     invert if disasm-31 get-xo,x,m,a-flag drop
       else dup disasm-22,30  xo-tab-entry @ execute 
       invert if disasm-xo-flags get-xo,x,m,a-flag drop 
        else disasm-illegal endif
     endif
 endif ;
' disasm-31-similar 31 opc-tab-entry !

: disasm-xfl-7,14-16,20 ( addr w -- addr w flag )
  dup disasm-7,14 .
  dup disasm-16,20 . false ;

: disasm-x-6,8-16,19 ( addr w -- addr w flag )
  dup disasm-6,8 .
  dup disasm-16,19 . false ;

: disasm-x-6,8-11,13 ( addr w -- addr w flag )
  dup disasm-6,8 .
  dup disasm-11,13 . false ;

: disasm-x-6,8-11,15-16,20 ( addr w -- addr w flag )
  dup disasm-6,8 .
  dup disasm-11,15 .
  dup disasm-16,20 . false ;

: disasm-x-11,15-6,10-16,20 ( addr w -- addr w flag )
  dup disasm-11,15 .
  dup disasm-6,10 .
  dup disasm-16,20 .  false ;

: disasm-x-6,8-10-11,15-16-20 ( addr w -- addr w flag )
  dup disasm-6,8 .
  dup disasm-10 .
  dup disasm-11,15 .
  dup disasm-16,20 . false ;

: disasm-x-no-args ( addr w -- addr w flag )
  false ;

: disasm-x-11,15-6,10 ( addr w -- addr w flag )
  dup disasm-11,15 .
  dup disasm-6,10 . false ;

: disasm-x-11,15-16,20 ( addr w -- addr w flag )
  dup disasm-11,15 .
  dup disasm-16,20 . false ;

: disasm-x-6,8 ( addr w -- addr w flag )
  dup disasm-6,8 . false ;

: disasm-x-6,10 ( addr w -- addr w flag )
  dup disasm-6,10 . false ;

: disasm-x-6,10-12,15 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-12,15 . false ;

: disasm-x-6,10-16,20 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-16,20 . false ;

: disasm-x-12,15-6,10 ( addr w -- addr w flag )
  dup disasm-12,15 .
  dup disasm-6,10 . false ;

: disasm-x-16,20 ( addr w -- addr w flag )
  dup disasm-16,20 . false ;

: calc-spr ( o -- spr )
  dup $1F and 5 lshift swap 5 rshift or ;

: disasm-xfx-6,10-11,20 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-11,20 calc-spr . false ;

: disasm-xfx-12,19-6,10 ( addr w -- addr w flag )
  dup disasm-12,19 .
  dup disasm-6,10 . false ;

: disasm-xfx-11,20-6,10 ( addr w -- addr w flag )
  dup disasm-11,20 calc-spr .
  dup disasm-6,10 . false ;

: disasm-xs-11,15-6,10-16,20-30 ( addr w -- addr w flag )
  dup disasm-11,15 .
  dup disasm-6,10 .
  dup dup disasm-16,20 swap disasm-30 5 lshift or . false ;

: disasm-xo-6,10-11,15 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-11,15 . false ;

: disasm-x-a-xo-6,10-11,15-16,20 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-16,20 . false ;

: disasm-x-string-6,10-11,15-16,20 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup dup disasm-16,20 0 = if 32 . drop else disasm-16,20 . endif false ;

: disasm-ds-6,10-16,29-11,15 ( addr w -- )
  dup disasm-6,10 .
  dup disasm-16,29 2 lshift ext-16 .
  disasm-11,15 .
  drop ;

: disasm-ds-58 ( addr w -- )
  dup disasm-30,31 ds-58-tab-entry @ execute ;
' disasm-ds-58 58 opc-tab-entry !

: disasm-ds-62 ( addr w -- )
  dup disasm-30,31 ds-62-tab-entry @ execute ;
' disasm-ds-62 62 opc-tab-entry !

: disasm-a-6,10-11,15-16,20 ( addr w -- addr w )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-16,20 .  ;

: disasm-a-6,10-11,15-21,25-16,20 ( addr w -- addr w )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-21,25 .
  dup disasm-16,20 . ;

: disasm-a-f-6,10-11,15-21,25-16,20 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-21,25 .
  dup disasm-16,20 . false ;

: disasm-a-6,10-11,15-21,25 ( addr w -- addr w )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-21,25 . ;

: disasm-a-f-6,10-11,15-21,25 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-21,25 . false ;

: disasm-a-6,10-16,20 ( addr w -- addr w )
  dup disasm-6,10 .
  dup disasm-16,20 . ;

: disasm-a-f-6,10-16,20 ( addr w -- addr w flag )
  dup disasm-6,10 .
  dup disasm-16,20 . false ;

: disasm-a ( addr w -- )
  dup disasm-26,30 a-dis-tab-entry @ execute
  disasm-31 get-xo,x,m,a-flag drop ;
' disasm-a 59 opc-tab-entry !

: get-i,b-flag ( o -- )
  case 
    0 of ." " endof
    1 of ." l" endof
    2 of ." a" endof
    3 of ." la" endof
  endcase ;

: calc-b-offset ( addr w o' -- addr w offset )
  2 lshift $8000 xor $8000 - ;

: disasm-b-6,10-11,15-absaddr ( addr w -- w )
  \ for bca opc 16
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-16,29 calc-b-offset hex. swap drop ;

: disasm-b-6,10-11,15-reladdr ( addr w -- w )
  \ for bc opc 16
  dup disasm-6,10 .
  dup disasm-11,15 .
  dup disasm-16,29 calc-b-offset rot + hex. ;

: calc-i-offset ( addr w o' -- addr w offset )
  2 lshift $2000000 xor $2000000 - ;

: disasm-i-absaddr ( addr w -- w )
  \ for ba opc 18
  dup disasm-6,29 calc-i-offset hex. swap drop ;

: disasm-i-reladdr ( addr w -- w )
  \ for b opc 18
  dup disasm-6,29 calc-i-offset rot + hex. ;

: disasm-b ( addr w -- )
  dup disasm-30 b-tab-entry @ execute 
  disasm-30,31 get-i,b-flag ;
' disasm-b 16 opc-tab-entry !

: disasm-i ( addr w -- )
  dup disasm-30 i-tab-entry @ execute 
  disasm-30,31 get-i,b-flag ;
' disasm-i 18 opc-tab-entry !

: define-format ( disasm-xt table-xt -- )
  create 2,
does> ( u "inst" -- )
  2@ swap here name string, 
  noname create 2,
  execute lastxt swap !
does> ( addr w -- )
  2@ >r
  execute
  r> count type ;

' disasm-x-a-xo-6,10-11,15-16,20 ' xo-tab-entry define-format asm-xo-1
' disasm-xo-6,10-11,15 ' xo-tab-entry define-format asm-xo-2
\ asm-d-oper-1 mnemonic D, A, SIMM (infix) i.e addi rD,rA,SIMM
' disasm-d-oper-1 ' opc-tab-entry define-format asm-d-oper-1
\ asm-d-oper-2 mnemonic A, S, UIMM (infix) i.e. andi. rA,rS,UIMM
' disasm-d-oper-2 ' opc-tab-entry define-format asm-d-oper-2
' disasm-d-load-store ' opc-tab-entry define-format asm-d-load-store
\ asm-d-compare-1 mnemonic crfD,L,rA,SIMM (infix) i.e. cmpi crfD,L,rA,SIMM
' disasm-d-compare-1 ' opc-tab-entry define-format asm-d-compare-1
\ asm-d-compare-2 mnemonic crfd,L,rA,UIMM (infix) i.e. cmpli crfD,L,rA,UIMM
' disasm-d-compare-2 ' opc-tab-entry define-format asm-d-compare-2
' disasm-ds-6,10-16,29-11,15 ' ds-58-tab-entry define-format asm-ds-1
' disasm-ds-6,10-16,29-11,15 ' ds-62-tab-entry define-format asm-ds-2
\ asm-x-1 : mnemonic A, S, B (infix) i.e and rA,rS,rB
' disasm-x-11,15-6,10-16,20 ' x-31-tab-entry define-format asm-x-1
\ asm-x-2 : mnemomic S, A, B (infix) i.e stdx rS,rA,rB
' disasm-x-a-xo-6,10-11,15-16,20 ' x-31-tab-entry define-format asm-x-2
' asm-x-2 alias asm-x-2-1
\ asm-x-3 : mnemomic crfD, L, A, B (infix) i.e. cmp crfD,L,rA,rB
' disasm-x-6,8-10-11,15-16-20 ' x-31-tab-entry define-format asm-x-3
\ asm-x-4 : mnemonic <no args> i.e. eieio
' disasm-x-no-args ' x-31-tab-entry define-format asm-x-4
\ asm-x-5 : mnemonic A, S (infix) i.e. cntlzd rA,rS
' disasm-x-11,15-6,10 ' x-31-tab-entry define-format asm-x-5
\ asm-x-6 : mnemonic A, B (infix) i.e. dcba rA,rB
' disasm-x-11,15-16,20 ' x-31-tab-entry define-format asm-x-6
\ asm-x-7 : mnemonic crfD (infix) i.e. mcrxr crfD
' disasm-x-6,8 ' x-31-tab-entry define-format asm-x-7
\ asm-x-8-{31,63} : mnemonic D (infix) i.e. mfcr rD
' disasm-x-6,10 ' x-31-tab-entry define-format asm-x-8-31
' disasm-x-6,10 ' x-63-tab-entry define-format asm-x-8-63
\ asm-x-9 : mnemonic D SR (infix) i.e. mfsr rD,SR
' disasm-x-6,10-12,15 ' x-31-tab-entry define-format asm-x-9
\ asm-x-10-{31,63} : mnemonic  D B (infix) i.e. mfsrin rD,rB
' disasm-x-6,10-16,20 ' x-31-tab-entry define-format asm-x-10-31
' disasm-x-6,10-16,20 ' x-63-tab-entry define-format asm-x-10-63
\ asm-x-11 : mnemonic S SR (infix) i.e. mtsr SR,rS
' disasm-x-12,15-6,10 ' x-31-tab-entry define-format asm-x-11
\ asm-x-12 : mnemonic B (infix) i.e. slbie rB
' disasm-x-16,20 ' x-31-tab-entry define-format asm-x-12
\ asm-x-13 : mnemonic crfD A B (infix) i.e. fcmpo crfD,frA,frB
' disasm-x-6,8-11,15-16,20 ' x-63-tab-entry define-format asm-x-13
\ asm-x-14 : mnemonic crfD crfS  (infix) i.e mcrfs crfD,crfS
' disasm-x-6,8-11,13 ' x-63-tab-entry define-format asm-x-14
\ asm-x-15 : mnemonic crbD IMM (infix) i.e. mtfsfi crbD,IMM
' disasm-x-6,8-16,19 ' x-63-tab-entry define-format asm-x-15
\ asm-x-16 : mnemomic S, A, B (infix) i.e lswi rS,rA,rB
' disasm-x-string-6,10-11,15-16,20 ' x-31-tab-entry define-format asm-x-16
\ asm-xfx-1 : mnemonic D SPR (infix) i.e. mfspr rD,SPR
' disasm-xfx-6,10-11,20 ' x-31-tab-entry define-format asm-xfx-1
\ asm-xfx-2 : mnemonic S CRM (infix) i.e. mtcrf CRM,rS
' disasm-xfx-12,19-6,10 ' x-31-tab-entry define-format asm-xfx-2
\ asm-xfx-3 : mnemonic S SPR (infix) i.e. mtspr SPR,rS
' disasm-xfx-11,20-6,10 ' x-31-tab-entry define-format asm-xfx-3
' disasm-xs-11,15-6,10-16,20-30 ' xs-tab-entry define-format asm-xs
\ asm-xl-1 : mnemonic BO BI (infix) i.e. bcctrx BO,BI
' disasm-xl-6,10-11,15 ' xl-tab-entry define-format asm-xl-1
\ asm-xl-2 : mnemonic crbD crbA crbB (infix) i.e. crand crbD,crbA,crbB
' disasm-xl-6,10-11,15-16,20 ' xl-tab-entry define-format asm-xl-2
\ asm-xl-3 : mnemonic nothing (infix) i.e. isync
' disasm-nothing ' xl-tab-entry define-format asm-xl-3
\ asm-xl-4 : mnemonic crfD crfS (infix) i.e. mcrf crfD,crfS
' disasm-xl-6,8-11,13 ' xl-tab-entry define-format asm-xl-4
' disasm-m ' opc-tab-entry define-format asm-m
' disasm-md-11,15-6,10-16,20-30-21,26 ' md-tab-entry define-format asm-md
' disasm-mds-11,15-6,10-16,20-21,26 ' mds-tab-entry define-format asm-mds
\ asm-a-1-63 : mnemonic D A B (infix) i.e. fadd frD,frA,frB
' disasm-x-a-xo-6,10-11,15-16,20 ' a-tab-entry define-format asm-a-1-63
\ asm-a-1-59 : mnemonic D A B (infix) i.e. fadds frD,frA,frB
' disasm-a-6,10-11,15-16,20 ' a-dis-tab-entry define-format asm-a-1-59
\ asm-a-2-[63,59] : mnemonic D A B C (infix) i.e. fmadd frD,frA,frC,frB
' disasm-a-f-6,10-11,15-21,25-16,20 ' a-tab-entry define-format asm-a-2-63
' disasm-a-6,10-11,15-21,25-16,20 ' a-dis-tab-entry define-format asm-a-2-59
\ asm-a-3-[63,59] : mnemonic D A C (infix) i.e. fmul frD,frA,frC
' disasm-a-f-6,10-11,15-21,25 ' a-tab-entry define-format asm-a-3-63
' disasm-a-6,10-11,15-21,25 ' a-dis-tab-entry define-format asm-a-3-59
\ asm-a-4-[63,59] : mnemonic D B (infix) i.e. fres frD,frB
' disasm-a-f-6,10-16,20 ' a-tab-entry define-format asm-a-4-63
' disasm-a-6,10-16,20 ' a-dis-tab-entry define-format asm-a-4-59
' disasm-i-reladdr ' i-tab-entry define-format asm-i-reladdr  
' disasm-i-absaddr ' i-tab-entry define-format asm-i-absaddr
' disasm-b-6,10-11,15-reladdr ' b-tab-entry define-format asm-b-reladdr
' disasm-b-6,10-11,15-absaddr ' b-tab-entry define-format asm-b-absaddr
' disasm-xfl-7,14-16,20 ' x-63-tab-entry define-format asm-xfl
' 2drop ' opc-tab-entry define-format asm-sc

include ./inst.fs

previous set-current
