\ asm386.fth
\ Andrew McKewan
\ mckewan@netcom.com

\ 80386 "subset" assembler.
\ Greatly inspired by:
\    1. 32-BIT MINI ASSEMBLER BASED ON riFORTH by Richard Astle
\    2. F83 8086 Assembler by Mike Perry

\ This assembler will run under Win32Forth.  It was written to support a 
\ metacompiler so it does not implement the full range of opcodes and 
\ operands.  In particular, it does not support direct memory access
\ (i.e.  mov [memory],eax ).  This is because the Forth system, like 
\ Win32Forth, uses only relative addresses (index or base+index).
\ The syntax is postfix and is similar to F83.  Here are some examples:
\
\       EAX EBX MOV             \ move ebx,eax
\       3 # EAX MOV             \ mov eax,3
\       100 [EDI] EAX MOV       \ mov eax,100[edi]
\       4 [EBX] [ECX] EAX MOV   \ mov eax,4[ebx][ecx]
\       16: EAX EBX MOV         \ mov bx,ax


ONLY FORTH ALSO DEFINITIONS

VOCABULARY ASSEMBLER   ASSEMBLER ALSO DEFINITIONS   HEX


\ ---------------------------------------------------------------------
\ Defer memory-access words for the metacompiler

DEFER HERE     FORTH ' HERE  ASSEMBLER IS HERE
DEFER ,        FORTH ' ,     ASSEMBLER IS ,
DEFER C,       FORTH ' C,    ASSEMBLER IS C,
DEFER TC@      FORTH ' C@    ASSEMBLER IS TC@
DEFER TC!      FORTH ' C!    ASSEMBLER IS TC!


\ ---------------------------------------------------------------------
\ Register Fields:  8000 <flags> <sib>

\ Flag bits:    0 = size field   1 = DWORD, 0 = BYTE
\               1 = index field  1 = INDEXED MODE
\               2 = sib flag     1 = SIB byte required

: REG     ( mask off register field )   7 AND ;
: REG?    ( reg -- f )     FFFF0200 AND 80000000 = ;
: R32?    ( reg -- f )     FFFF0300 AND 80000100 = ;
: INDEX?  ( reg -- f )     FFFF0200 AND 80000200 = ;
: SIZE?   ( reg -- 0/1 )   8 RSHIFT 1 AND ;
: SIB?    ( reg -- f )     400 AND ;

: INDEX  ( n -- )  \ create an index register
    CREATE ,  DOES> @
    OVER INDEX?
    IF    REG 3 LSHIFT         ( move reg to index field in sib )
          400 OR                ( set sib flag )
          SWAP FFFFFFC7 AND OR  ( put into sib byte of previous register )
    THEN ;

80000100 CONSTANT EAX       80000000 CONSTANT AL
80000101 CONSTANT ECX       80000001 CONSTANT CL
80000102 CONSTANT EDX       80000002 CONSTANT DL
80000103 CONSTANT EBX       80000003 CONSTANT BL
80000104 CONSTANT ESP       80000004 CONSTANT AH
80000105 CONSTANT EBP       80000005 CONSTANT CH
80000106 CONSTANT ESI       80000006 CONSTANT DH
80000107 CONSTANT EDI       80000007 CONSTANT BH

80000300 INDEX [EAX]
80000301 INDEX [ECX]
80000302 INDEX [EDX]
80000303 INDEX [EBX]
80000724 INDEX [ESP]
80000305 INDEX [EBP]
80000306 INDEX [ESI]
80000307 INDEX [EDI]

80010000 CONSTANT #   ( just different from any register )

\ Scaled index mode must have a base register, i.e.
\ 	0 [EDI] [EAX] *4 ECX MOV
: *2  40 OR ;
: *4  80 OR ;
: *8  C0 OR ;

\ ---------------------------------------------------------------------
\ Assembler addressing mode bytes

VARIABLE SIZE  1 SIZE !
: BYTE   0 SIZE ! ;
: OP,    ( n op -- )    OR C, ;
: SIZE,  ( op reg -- )  SIZE? OP, ;
: SHORT? ( n -- f )    -80 80 WITHIN ;
: DISP?  ( n reg -- n reg f )   2DUP REG 5 = ( [EBP] ) OR ;

: RR,   ( reg reg/op -- )  3 LSHIFT OR C0 OP, ;

: MEM,	( operand [reg] reg -- )
    3 LSHIFT >R  ( move to reg/opcode field )
    DUP SIB?
    IF    DISP?
          IF  OVER SHORT?
              IF      R> 44 OP, C, C,
              ELSE    R> 84 OP, C, ,
              THEN
          ELSE        R> 4 OP, C, DROP   ( no displacement )
          THEN
    ELSE  DISP?
          IF  OVER SHORT?
              IF    R> OR 40 OP, C,
              ELSE  R> OR 80 OP, ,
              THEN
          ELSE	    R> OP, DROP  ( no displacement )
          THEN
    THEN ;

: R/M,  ( operand [reg] reg | reg reg -- )
    OVER REG? IF  RR,  ELSE  MEM,  THEN ;

: WR/SM,  ( r/m reg op -- )  2 PICK REG?
    IF  2 PICK SIZE, RR,  ELSE  SIZE @ OP,  MEM,  THEN  1 SIZE ! ;


\ ---------------------------------------------------------------------
\ Opcode Defining Words

: CPU  ( op -- )  CREATE C,  DOES> C@ C, ;

66 CPU 16:	\ 16-bit opcode prefix (cannot use with immedate ops)
C3 CPU RET
F2 CPU REP   F2 CPU REPNZ   F3 CPU REPZ
FC CPU CLD   FD CPU STD     99 CPU CDQ


: SHORT  ( op opex regop -- )
    CREATE C, C, C,
    DOES>  ( reg | offset [reg] -- )  OVER R32?
    IF  C@ OP,  ELSE  1+ COUNT SWAP C@ WR/SM,  THEN ;

FF 6 50 SHORT PUSH   8F 0 58 SHORT POP
FE 0 40 SHORT INC    FE 1 48 SHORT DEC


: UNARY  ( opex -- )
    CREATE C,  DOES>  ( reg | offset [reg] )  C@ F6 WR/SM, ;

2 UNARY INV  ( INV = Intel's NOT )
3 UNARY NEG   4 UNARY MUL
5 UNARY IMUL  6 UNARY DIV   7 UNARY IDIV


\ The following forms are accepted for binary operands.
\ Note that immediate to memory is not supported.
\	reg reg <op>
\	n # reg <op>
\	ofs [reg] reg <op>
\	reg ofs [reg] <op>

: BINARY  ( op -- )
    CREATE C,
    DOES> C@ 2 PICK # =
    IF	OVER SIZE?
        IF    81 C,  RR,  DROP ,
        ELSE  80 C,  RR,  DROP C,
        THEN
    ELSE  3 LSHIFT
          OVER INDEX? IF  >R ROT R>  ELSE  2 OR  THEN
          OVER SIZE, R/M,
    THEN ;

: MOV   ( operands... -- )
    OVER # =
    IF    DUP SIZE? IF  B8 OP, DROP ,  ELSE  B0 OP, DROP C,  THEN
    ELSE  DUP INDEX? IF  ROT 88  ELSE  8A  THEN  OVER SIZE, R/M,
    THEN ;

: LEA   ( reg/mem reg -- )   8D C,  MEM, ;

: XCHG  ( mr1 reg -- )
    OVER REG? OVER EAX = AND
    IF    DROP REG 90 OP,
    ELSE  86 OVER SIZE,  R/M,  THEN ;
    
( TEST ... )


\ Shift/Rotate syntax:
\	eax shl		0 [ecx] [edi] shl
\	eax 4 shl	0 [ecx] [edi] 4 shl
\	eax cl shl	0 [ecx] [edi] cl shl

: SHIFT  ( op -- )
    CREATE C,
    DOES> C@ OVER CL =
    IF    NIP D2 WR/SM,
    ELSE  OVER 0< ( reg/index)
        IF    D0 WR/SM,
        ELSE  OVER 1 =
            IF    NIP D0 WR/SM,
            ELSE  SWAP >R C0 WR/SM, R> C,
            THEN
        THEN
    THEN ;

0 SHIFT ROL   1 SHIFT ROR   2 SHIFT RCL   3 SHIFT RCR
4 SHIFT SHL   5 SHIFT SHR   7 SHIFT SAR

\ String instructions. Precede with BYTE for byte version
: STR  ( op -- )
    CREATE C,  DOES> C@ SIZE @ OP,  1 SIZE ! ;

A4 STR MOVS   A6 STR CMPS
AA STR STOS   AC STR LODS   AE STR SCAS


\ ---------------------------------------------------------------------
\ Relative jumps and calls

: OFFSET  ( dest source -- offset )
   1+ -  DUP SHORT? 0= ABORT" branch target out of range"  ;

: REL8,   ( addr -- )  HERE OFFSET C, ;
: REL32,  ( addr -- )  HERE CELL+ - , ;

: REL  ( op -- )   CREATE C,  DOES> C@ C,  REL8, ;

70 REL JO    71 REL JNO   72 REL JB    73 REL JAE
74 REL JE    75 REL JNE   76 REL JBE   77 REL JA
78 REL JS    79 REL JNS   7A REL JPE   7B REL JNP
7C REL JL    7D REL JGE   7E REL JLE   7F REL JG
E3 REL JECXZ

: JMP  ( addr | r/m -- )
   DUP 0< ( reg/index ) IF  FF C,  4 R/M,
   ELSE  DUP HERE 2 + - SHORT? IF  EB C,  REL8,
   ELSE  E9 C,  REL32,  THEN THEN ;

: CALL  ( addr | r/m -- )
   DUP 0< ( reg/index ) IF  FF C,  2 R/M,  ELSE  E8 C,  REL32,  THEN ;


\ ---------------------------------------------------------------------
\ Local labels

10 CONSTANT MAX-LABELS  ( adjust as required )

: ARRAY  CREATE CELLS ALLOT  DOES> SWAP CELLS + ;

MAX-LABELS ARRAY LABEL-VALUE    ( value of label or zero if not resolved )
MAX-LABELS ARRAY LABEL-LINK     ( linked list of unresolved references   )

: CLEAR-LABELS   ( initialize label arrays )
   0 LABEL-VALUE MAX-LABELS CELLS ERASE
   0 LABEL-LINK  MAX-LABELS CELLS ERASE  ;   CLEAR-LABELS

: CHECK-LABELS  ( make sure all labels have been resolved )
   MAX-LABELS 0
   DO  I LABEL-LINK @ IF CR ." Label " I . ." not resolved" THEN  LOOP ;

: $:  ( n -- )   ( define a label )
   DUP LABEL-VALUE @ ABORT" Duplicate label"
   HERE OVER LABEL-LINK @ ?DUP      ( any unresolved references? )
   IF  ( n address link )
       BEGIN  DUP TC@ >R            ( save offset to next reference )
              2DUP OFFSET OVER TC!  ( resolve this reference )
              R@ 100 - +            ( go to next reference )
              R> 0=                 ( more references? )
       UNTIL
       DROP OVER LABEL-LINK OFF     ( clear unresolved list )
   THEN
   SWAP LABEL-VALUE !  ;            ( resolve label address )

: $  ( n -- addr )    ( reference a label )
   DUP LABEL-VALUE @  ( already resolved? )
   IF    LABEL-VALUE @  
   ELSE  DUP LABEL-LINK @ ?DUP 0=   ( first reference? )
         IF   HERE 1+  THEN  1+     ( link to previous label )
         HERE 1+ ROT LABEL-LINK !   ( save current label at head of list )
   THEN ;


\ ---------------------------------------------------------------------
\ Structured Conditionals

75 CONSTANT 0=   79 CONSTANT 0<   73 CONSTANT U<   76 CONSTANT U>
7D CONSTANT <    7E CONSTANT >    71 CONSTANT OV   E3 CONSTANT ECX0<>

: NOT   1 XOR ;  ( reverse logic of conditional )

: IF        C, HERE 0 C, ;
: THEN      HERE OVER OFFSET  SWAP TC! ;
: ELSE      EB IF  SWAP THEN ;
: BEGIN     HERE ;
: UNTIL     C, REL8, ;
: LOOP      E2 UNTIL ;
: AGAIN     EB UNTIL ;
: WHILE     IF SWAP ;
: REPEAT    AGAIN THEN ;

0 BINARY ADD   1 BINARY OR    2 BINARY ADC   3 BINARY SBB
4 BINARY AND   5 BINARY SUB   6 BINARY XOR   7 BINARY CMP

: RET#   ( n -- )  C2 C, W, ;
: PUSH#  ( n -- )  68 C, ,  ;

: NEXT   ( -- )
   LODS   0 [EAX] [EDI] ECX MOV   EDI ECX ADD   ECX JMP ;

: XCALL  ( n -- )
    EDX EBX MOV
    6 CELLS [EDI] EAX MOV
    ( n ) CELLS [EAX] CALL
    EBX EDX MOV  ;

VARIABLE AVOC
: ASM-INIT   CONTEXT @ AVOC !  ASSEMBLER  CLEAR-LABELS  !CSP ;

: END-CODE  ?CSP  CHECK-LABELS  AVOC @ CONTEXT !  REVEAL ;
: C;   END-CODE ;

FORTH DEFINITIONS

: LABEL   CREATE HIDE  ASM-INIT ;
: CODE    LABEL  HERE DUP CELL - !  ;

: ;CODE   ( -- )
   ?COMP ?CSP  COMPILE (;CODE)  [COMPILE] [  ASM-INIT ; IMMEDIATE


ONLY FORTH ALSO DEFINITIONS DECIMAL
