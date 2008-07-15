\ Simple ARM RPN Assembler

\ Author: David Kühling <dvdkhlng AT gmx DOT de>
\ Created: 2007

\ Copyright (C) 2000,2007,2008 Free Software Foundation, Inc.

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

\ *** For details on usage, please have a look at section "ARM assembler" in
\ the Gforth documentaion. ***

require ./../../code.fs

ALSO ASSEMBLER DEFINITIONS

HEX  \ EVERYTHING BELOW IS IN HEXADECIMAL!

\ change these for cross compilation
: t,  , ;
: there  here ;
: t@  @ ;
: t!  ! ;


\ Enumerations
: enumerate:  ( N start "name1" ... "nameN" -- )
   DUP ROT + SWAP ?DO   I CONSTANT  LOOP ;

\ operand types
6 70000000 enumerate: register shifted #immediate psr cxsf-mask offset
2 70000006 enumerate: multimode register-list 

: nand ( x1 -- x2 )  invert and ;
: ?register   ( n -- )
   register <> ABORT" Inavlid operand, need a register R0..R15" ;
: ?psr   ( n -- )
   psr <> ABORT" Invalid operand, need special register SPSR or CSPR" ;

\ Registers
: regs:  10 0 DO  I register 2CONSTANT LOOP ;
regs: R0 R1 R2 R3 R4 R5 R6 R7 R8 R9 R10 R11 R12 R13 R14 R15

000000 psr 2CONSTANT CPSR	400000 psr 2CONSTANT SPSR

\ Bit masks
: bit:  ( n "name" -- )   1 SWAP LSHIFT CONSTANT ;
: bits ( 0 bit1 ... bitN -- x )
   0  BEGIN OVER OR  SWAP 0= UNTIL ;

19 bit: %I   18 bit: %P   17 bit: %U   16 bit: %B   15 bit: %W

\ Basic instruction creation, condition codes
VARIABLE instruction
VARIABLE had-cc
: encode  ( x1 -- )   instruction @  OR  instruction ! ;
: ?can-cc ( -- )
   had-cc @ ABORT" Attempt to specify condition code twice." ;
: cc:  ( x "name" -- )  CREATE ,
  DOES> @  ( x -- )
   ?can-cc  1C LSHIFT encode   TRUE had-cc ! ;

00 cc: EQ	01 cc: NE	02 cc: CS	03 cc: CC
04 cc: MI	05 cc: PL	06 cc: VS	07 cc: VC
08 cc: HI	09 cc: LS	0A cc: GE	0B cc: LT
0C cc: GT	0D cc: LE	0E cc: AL	0F cc: NV
02 CC: HS	03 cc: LO

: invert-cc  ( -- ) \ invert meaning of condition code (EQ -> NE etc.)
   had-cc @ 0= ABORT" No condition code specified for instruction"
   instruction @   1 1C LSHIFT XOR  instruction ! ;

: <instruction  ( x -- )
   DUP  0F0000000 AND IF
      had-cc @ ABORT" Condition code not allowed for instruction"
   ELSE  had-cc @ 0= IF AL THEN   THEN
   encode ;
: instruction>  ( -- x )
   instruction @   0 instruction !  FALSE had-cc ! ;

\ Simple register operands
: register-operand:  ( bit-offs "name" -- )
   CREATE ,
  DOES> @  ( n-reg 'register' n-bit -- mask )
   >R ?register R> LSHIFT   encode ;

10 register-operand: Rn,	0C register-operand: Rd,
10 register-operand: RdHi,	0C register-operand: RdLo,
8 register-operand: Rs,		 0 register-operand: Rm,

\ PSR register operands
: psr,  ?psr  encode ;

\ Field mask (for MSR)
: cxsf-mask:  ( #bit "name" -- )  1 SWAP LSHIFT  cxsf-mask 2CONSTANT ;
: cxsf,  BEGIN  DUP cxsf-mask = WHILE  DROP encode  REPEAT ;
10 cxsf-mask: C   11 cxsf-mask: X   12 cxsf-mask: S   13 cxsf-mask: F

\ Right-hand side operands
: lrotate32  ( x1 n -- x2 )
   2DUP LSHIFT >R    20 SWAP - RSHIFT R> OR ;
: #  ( n -- x )
   16 0 ?DO
      DUP 0 100 WITHIN IF
	 I 8 LSHIFT OR  %I OR  #immediate UNLOOP EXIT
      THEN
      2 LROTATE32
   LOOP
   ABORT" Immediate operand cannot be expressed as shifted 8 bit value" ;

: ?shift ( x1 -- x1 )
   DUP 1 20 WITHIN 0= ABORT" Invalid shift value" ;
: #shift: ( mask "name" -- )  CREATE ,
  DOES> @  ( n-reg 'register' shift mask --  operand 'shifted' )
   >R   ?shift 7 LSHIFT >R  ?register  R> OR  R> OR   shifted  ;
: rshift:  ( mask "name" -- )  CREATE ,
   DOES> @  ( n-reg 'register' mask --  operand 'shifted' )
    >R   ?register 8 LSHIFT >R  ?register   R> OR  R> OR   010 OR shifted ;
: RRX  ( n-reg 'register' -- operand 'shifted' )
   ?register  060 OR  shifted ;
   
000 DUP #shift: #LSL  rshift: LSL	020 DUP #shift: #LSR  rshift: LSR
040 DUP #shift: #ASR  rshift: ASR	060 DUP #shift: #ROR  rshift: ROR

: ?rhs  ( 'shifted'|'register'|'#immediate' -- )
   >R R@ shifted <>  R@ #immediate <> AND  R> register <> AND
   ABORT" Need a (shifted) register or immediate value as operand" ;
: ?#shifted-register ( x 'shifted'|'register' -- x )
   >R R@ shifted <>  R> register <> AND  ABORT" Need a (shifted) register here"
   DUP 010 AND  ABORT" Shift by register not allowed here" ;
: rhs,  ( x 'r-shifted'|`#-shifted' -- )
   ?rhs  encode ;
: rhs',  ( x 'r-shifted'|`#-shifted' -- )
   DUP shifted = ABORT" Shifted registers not allowed here."  rhs, ;

\ Addressing modes
: offset:  ( 0 bit1 ... bitN "name" -- )  bits   offset 2CONSTANT ;

0 %P %I %U	offset: +]
0 %P %I		offset: -]
0 %P %I %U %W	offset: +]!
0 %P %I    %W	offset: -]!
0    %I %U	offset: ]+
0    %I		offset: ]-
0 %P 		offset: #]
0 %P       %W	offset: #]!
0    		offset: ]#

: ]   0 #] ;
: [#]  ( addr -- R15 offs 'offset' )  \ generate PC-relative address
   >R R15  R> there 8 +  -   #] ;

: multimode:  ( 0 bit1 ... bitN "name" -- )  bits  multimode 2CONSTANT ;
 
0 		multimode: DA
0    %U		multimode: IA
0 %P		multimode: DB
0 %P %U		multimode: IB
0 	%W	multimode: DA!
0    %U	%W	multimode: IA!
0 %P	%W	multimode: DB!
0 %P %U	%W	multimode: IB!

: ?offset  ( 'offset' -- )
   offset <> ABORT" Invalid operand, need an address offset e.g ' Rn ] ' " ;
: ?multimode  ( 'offset' -- )
   multimode <> ABORT" Need an address mode for load/store multiple: DA etc." ;
: ?upwards  ( n1 -- n2 )
   DUP 0< IF  NEGATE ELSE %U encode THEN ;
: ?post-offset  ( x 'offset' -- x )
   ?offset  DUP %P AND 0=
   ABORT" Only post-indexed addressing, ]#, ]+ or ]- , allowed here" ;
: ?0#]  ( 0 'offset' -- )
   ?offset    0 #] DROP D<>
   ABORT" Only addresses without offset, e.g R0 ] allowed here" ;
: #offset12,  ( n -- )
   ?upwards  DUP 000 1000 WITHIN 0= ABORT" Offset out of range"  encode ;
: #offset8,  ( n -- )
   ?upwards  DUP 000 100 WITHIN 0= ABORT" Offset out of range"
   %B encode	\ %B replaces (inverted) %I-bit for  8-bit offsets!
   DUP 0F AND ( low nibble) encode  0F0 AND 4 LSHIFT  ( high nibble) encode ;
: R#shifted-offset,  ( n  'register'|'shifted-reg' -- )
   ?#shifted-register encode  ;
: R-offset,  ( n  'register'|'shifted-reg' -- )
   ?register encode  ;
: offs12,  ( x1..xn 'offset' -- )
   ?offset DUP encode
   %I AND 0= IF  #offset12, ELSE R#shifted-offset, THEN ;
: offsP,  ( x1..xn 'offset' -- )
   2DUP ?post-offset DROP  offs12, ;
: offs8,  ( x1..xn 'offset' -- ) \ limited addressing for halword load etc.
   ?offset DUP %I nand encode
   %I AND 0= IF  #offset8, ELSE R-offset, THEN ;
: mmode,  ( x 'multimode' -- )
   ?multimode encode ;

\ Branch offsets
2 80000000
enumerate: forward backward
: ?branch-offset  ( offset -- offset )
   DUP -2000000 2000000 WITHIN 0= ABORT" Branch destination out of range"
   DUP 3 AND 0<> ABORT" Branch destination not 4 byte-aligned" ;
: branch-addr>offset  ( src dest -- offset )   SWAP 8 +  -   ?branch-offset ;
: branch-offset>bits  ( offset -- x )  2 RSHIFT 0FFFFFF AND ;
: branch-addr,  ( addr -- x )
   there SWAP branch-addr>offset  branch-offset>bits  encode ;
: a<mark  ( -- addr 'backward' )  there backward ;
: a<resolve  ( addr 'backward' -- addr )
   backward <> ABORT" Expect assembler backward reference on stack" ;
: a>mark  ( -- addr 'forward' addr )  there forward   OVER ;
: a>resolve  ( addr 'forward' -- )
   forward <> ABORT" Expect assembler forward reference on stack"
   DUP  there branch-addr>offset  branch-offset>bits
   OVER t@ 0FF000000 AND  OR   SWAP t! ;

\ "Comment" fields (SVC/SWI)
: ?comment  ( x -- x )
   DUP 0 01000000 WITHIN 0= ABORT" Comment field is limited to 24 bit values" ;
: comment,  ( x -- )
   ?comment encode ;

\ Register lists (for LDM and STM)
: {  ( -- mark )  77777777 ;
: }  ( mark reg1 .. regN -- reglist )
   0 BEGIN OVER 77777777 <> WHILE
	 SWAP ?register   1 ROT LSHIFT OR
   REPEAT  NIP register-list ;
: R-R  ( reg1 regN -- reg1 reg2... regN )
   ?register  SWAP ?register  1+ ?DO  I register LOOP ;
: ?register-list  ( 'register-list' -- )
   register-list <> ABORT" Need a register list { .. } as operand" ;
: reg-list,  ( x 'register-list' -- )
   ?register-list encode ;
   
\ Mnemonics
: instruction-class:  ( xt "name" -- )  CREATE ,
  DOES> @  ( mask xt "name" -- )  CREATE 2,
  DOES> 2@   ( mask xt -- )  >R   <instruction R> EXECUTE instruction> t, ;

:NONAME  Rd,	rhs,	Rn, ;		instruction-class: data-op:
:NONAME  rhs,	Rn, ;			instruction-class: cmp-op:
:NONAME  Rd,	rhs, ;			instruction-class: mov-op:
:NONAME  Rd,	psr, ;			instruction-class: mrs-op:
:NONAME  cxsf, psr, rhs', ;	instruction-class: msr-op:
:NONAME  Rd,	offs12, Rn, ;		instruction-class: mem-op:
:NONAME  Rd,	offsP,  Rn, ;		instruction-class: memT-op:
:NONAME  Rd,	offs8,  Rn, ;		instruction-class: memH-op:
:NONAME  Rd,	Rm,	?0#] Rn, ;	instruction-class: memS-op:
:NONAME  reg-list, mmode, Rn, ;		instruction-class: mmem-op:
:NONAME  branch-addr, ;			instruction-class: branch-op:
:NONAME  Rn,	Rs,	Rm, ;		instruction-class: RRR-op:
:NONAME  Rd,	Rn,	Rs,	Rm, ;	instruction-class: RRRR-op:
:NONAME  RdHi, RdLo,	Rs,	Rm, ;	instruction-class: RRQ-op:
:NONAME  comment, ;			instruction-class: comment-op:
:NONAME  rm, ;				instruction-class: branchR-op:
: mmem-op2x:  ( x "name1" "name2" -- )  DUP mmem-op: mmem-op: ;

00000000 data-op: AND,             00100000 data-op: ANDS,
00200000 data-op: EOR,		   00300000 data-op: EORS,
00400000 data-op: SUB,		   00500000 data-op: SUBS,
00600000 data-op: RSB,		   00700000 data-op: RSBS,
00800000 data-op: ADD,		   00900000 data-op: ADDS,
00A00000 data-op: ADC,		   00B00000 data-op: ADCS,
00C00000 data-op: SBC,		   00D00000 data-op: SBCS,
00E00000 data-op: RSC,		   00F00000 data-op: RSCS,
01100000 cmp-op:  TST,		   0110F000 cmp-op:  TSTP,
01300000 cmp-op:  TEQ,		   0130F000 cmp-op:  TEQP,
01500000 cmp-op:  CMP,		   0150F000 cmp-op:  CMPP,
01700000 cmp-op:  CMN,		   0170F000 cmp-op:  CMNP,
01800000 data-op: ORR,		   01900000 data-op: ORRS,
01A00000 mov-op:  MOV,		   01B00000 mov-op:  MOVS,
01C00000 data-op: BIC,		   01D00000 data-op: BICS,
01E00000 mov-op:  MVN,		   01F00000 mov-op:  MVNS,

04000000 mem-op:  STR,		   04100000 mem-op:  LDR,             
04400000 mem-op:  STRB,            04500000 mem-op:  LDRB,
04200000 memT-op: STRT,		   04300000 memT-op: LDRT, 
04600000 memT-op: STRBT,	   04700000 memT-op: LDRBT,
000000B0 memH-op: STRH,		   001000B0 memH-op: LDRH, 
001000F0 memH-op: LDRSH,	   000000D0 memH-op: LDRSB,
01000090 memS-op: SWP,		   01400090 memS-op: SWPB,

08000000 mmem-op:  STM,		   08100000 mmem-op: LDM,
08400000 mmem-op:  ^STM,	   08500000 mmem-op: ^LDM,

010F0000 mrs-op:  MRS,		   


0A000000 branch-op:  B,		   0B000000 branch-op:  BL,
0A120010 branchR-op: BX,
\ FA000000 branchx-op: BLX,	   
0F000000 comment-op: SWI,	   0F000000 comment-op: SVC,

00000090 RRR-op:  MUL,		   00100090 RRR-op:  MULS,
00200090 RRRR-op: MLA,		   00300090 RRRR-op: MLAS,
00800090 RRQ-op:  UMULL,	   00900090 RRQ-op:  UMULLS,
00A00090 RRQ-op:  UMLAL,	   00B00090 RRQ-op:  UMLALS,
00C00090 RRQ-op:  SMULL,	   00D00090 RRQ-op:  SMULLS,
000E0090 RRQ-op:  SMLAL,	   00F00090 RRQ-op:  SMLALS,

\
\ Labels and branch resolving
\
: LABEL  there CONSTANT ;
: IF-NOT,  a>mark B, ;
: IF,  invert-cc IF-NOT, ;
: AHEAD,  AL IF-NOT, ;
: THEN,  a>resolve ;
: ELSE,  a>mark AL B,  2SWAP THEN, ;
: BEGIN,  a<mark ;
: UNTIL-NOT,  a<resolve B, ;
: UNTIL,  invert-cc UNTIL-NOT, ;
: AGAIN,  AL UNTIL-NOT, ;
: WHILE-NOT,  IF-NOT, ;
: WHILE,  invert-cc WHILE-NOT, ;
: REPEAT,  2SWAP  AGAIN,  THEN, ;
: REPEAT-UNTIL-NOT,  2SWAP  UNTIL-NOT,  THEN, ;
: REPEAT-UNTIL,  invert-cc REPEAT-UNTIL-NOT, ;

\ Register aliases (see also machine.h)
R15 2CONSTANT PC
R14 2CONSTANT LR
R13 2CONSTANT SP
R12 2CONSTANT IP	\ "intra procedure call scratch register" *not* PC
R11 2CONSTANT FP	\ frame pointer
R7  2CONSTANT RP	\ only if compiled with --enable-force regs

\ Minimal Gforth interpreter support
: NEXT,		\ Do 32-bit branch to NOOP
   PC  -4 #]   PC LDR,	\ due to pipeline PC is always 8 bytes ahead
   ['] NOOP >code-address t, ;

PREVIOUS DEFINITIONS DECIMAL
\ : ]ASM   ALSO ASSEMBLER ; 
\ : ASM[   PREVIOUS ;

\ : [ASM]   ]ASM ; IMMEDIATE
\ : [END-ASM]   ASM[ ; IMMEDIATE
