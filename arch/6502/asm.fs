\ asm.fs
\
\ postfix assembler/disassembler for 6502 and variants.

\ Generic routines for labels (forwards/backward references). The
\ assembler-specific part is supplied as an xt in "std-resolver" below.
HEX
require asm/numref.fs
HEX

also assembler definitions

\ Select CPU variant
VARIABLE CPU&

: 65N02    10 CPU& C! ; \ 0001xxxx
: R65C02   80 CPU& C! ; \ 0010xxxx
: 65SC02   40 CPU& C! ; \ 0100xxxx
: 6511Q    20 CPU& C! ; \ 1000xxxx

65N02 \ Set a default CPU type

\ Support for std-resolver
: ABS/REL@                    \ ( Addr --- A/R )
                              \ Addr is the Addr of the Opcode
                              \ A/R is a flag: A=1 R=0
\ RELative opcodes are 01, 03, 05, 07, 08, 09, 0B, 0D, 0F all other are ABS
  X c@                        \ Get Opcode
  dup 0F and 0=               \ 0000xxxx = 0
  over 10 and 0<>             \ xxxxxxx1 = 0
  and                         \ 0 0 => 1
  swap 80 =		      \ !!JAW bugfix for bra
  or 0= ;                     \ Flag: 1 = ABS 0 = Rel

: ABS/REL!                    \ ( $-Addr $:-Addr A/R --- )
  IF
      swap 1+ X !             \ ABS
  ELSE
      over 2 + -              \ REL
      dup 0 80 - < ABORT" branch out of range"
      dup 7F > ABORT" branch out of range"
      swap 1+ X C!
  THEN ;

: doresolve ( instruction-address -- )
  ref-adr @ over abs/rel@ abs/rel! ;
  
' doresolve to std-resolver

\ Tokens used in ASMTAB:
\ ASMTAB consists of 256 entrys that are 16Bit wide
\
\ Opcode implemented on:
\ 1111xxxx Fx   65C02 & 65SC02 & 6511Q & 65N02   basic
\ 1011xxxx Bx   65C02 &          6511Q & 65N02   basic + 6511Q
\ 1101xxxx Dx   65C02 & 65SC02 &         65N02   basic + 65SC02
\
\ Addressing:
\ x0 = None      x1 = A.      x2 = C #.    x3 = C X)
\ x4 = C )Y      x5 = C ,X    x6 = C ,Y    x7 = C
\ x8 = N ,X      x9 = N ,Y    xA = N       xB = N ).
\ xC = N X)      xD = C rel   xE = C ).    xF = C bit
\
\ Opcodes:
\ 00 ADC   01 and   02 ASL   03 BBR   04 BBS   05 BCC
\ 06 BCS   07 BEQ   08 BIT   09 BMI   0A BNE   0B BPL
\ 0C BRA   0D BRK   0E BVC   0F BVS   10 CLC   11 CLD
\ 12 CLI   13 CLV   14 CMP   15 CPX   16 CPY   17 DEC
\ 18 DEX   19 DEY   1A Eor   1B INC   1C INX   1D INY
\ 1E JMP   1F JSR   20 LDA   21 LDX   22 LDY   23 LSR
\ 24 NOP   25 orA   26 PHA   27 PHP   28 PHX   29 PHY
\ 2A PLA   2B PLP   2C PLX   2D PLY   2E RMB   2F ROL
\ 30 Ror   31 RTI   32 RTS   33 SBC   34 SEC   35 SED
\ 36 SEI   37 SMB   38 STA   39 STX   3A STY   3B STZ
\ 3C TAX   3D TAY   3E TRB   3F TSB   40 TSX   41 TXA
\ 42 TXS   43 TYA   44 WAI   45 STP
\ 80 no Opcode
\
\ Position:   AABC
\              ||| Addressing
\              || CPU-Type
\              | Opcode
\

CREATE ASMTAB
0DF0 , ( 00       BRK ) 25F3 , ( 01 C X>  orA )
80F0 , ( 02           ) 08F0 , ( 03           )
3FD7 , ( 04 C     TSB ) 25F7 , ( 05 C     orA )
02F7 , ( 06 C     ASL ) 2EBF , ( 07 C C   RMB )
27F0 , ( 08       PHP ) 25F2 , ( 09 C #.  orA )
02F1 , ( 0A A.    ASL ) 80F0 , ( 0B           )
3FDA , ( 0C N     TSB ) 25FA , ( 0D N     orA )
02FA , ( 0E N     ASL ) 03BF , ( 0F C C   BBR )

0BFD , ( 10 C rel BPL ) 25F4 , ( 11 C >Y  orA )
25DE , ( 12 C >   orA ) 80F0 , ( 13           )
3ED7 , ( 14 C     TRB ) 25F5 , ( 15 C ,X  orA )
02F5 , ( 16 C ,X  orA ) 2EBF , ( 17 C C   RMB )
10F0 , ( 18       CLC ) 25F9 , ( 19 N ,Y  orA )
1BD1 , ( 1A A.    INC ) 80F0 , ( 1B           )
3EDA , ( 1C N     TRB ) 25F8 , ( 1D N ,X  orA )
02F8 , ( 1E N ,X  ASL ) 03BF , ( 1F C C   BBR )

1FFA , ( 20 N     JSR ) 01F3 , ( 21 C X>  and )
80F0 , ( 22           ) 80F0 , ( 23           )
08F7 , ( 24 C     BIT ) 01F7 , ( 25 C     and )
2FF7 , ( 26 C     ROL ) 2EBF , ( 27 C C   RMB )
2BF0 , ( 28       PLP ) 01F2 , ( 29 C #.  and )
2FF1 , ( 2A A.    ROL ) 80F0 , ( 2B           )
08FA , ( 2C N     BIT ) 01FA , ( 2D N     and )
2FFA , ( 2E N     ROL ) 03BF , ( 2F C C   BBR )

09FD , ( 30 C rel BMI ) 01F4 , ( 31 C >Y  and )
01DE , ( 32 C >   and ) 80F0 , ( 33           )
08D5 , ( 34 C ,X  BIT ) 01F5 , ( 35 C ,X  BIT )
2FF5 , ( 36 C ,X  ROL ) 2EBF , ( 37 C C   RMB )
34F0 , ( 38       SEC ) 01F9 , ( 39 N ,Y  and )
17D1 , ( 3A A.    DEC ) 80F0 , ( 3B           )
08D8 , ( 3C N ,X  BIT ) 01F8 , ( 3D N ,X  and )
2FF8 , ( 3E N ,X  ROL ) 03BF , ( 3F C C   BBR )

31F0 , ( 40       RTI ) 1AF3 , ( 41 C X>  Eor )
80F0 , ( 42           ) 80F0 , ( 43           )
80F0 , ( 44           ) 1AF7 , ( 45 C     Eor )
23F7 , ( 46 C     LSR ) 2EBF , ( 47 C C   RMB )
26F0 , ( 48       PHA ) 1AF2 , ( 49 C #.  Eor )
23F1 , ( 4A A.    LSR ) 80F0 , ( 4B           )
1EFA , ( 4C N     JMP ) 1AFA , ( 4D N     Eor )
23FA , ( 4E N     LSR ) 03BF , ( 4F C C   BBR )

0EFD , ( 50 C rel BVC ) 1AF4 , ( 51 C >Y  Eor )
1ADE , ( 52 C >   Eor ) 80F0 , ( 53           )
80F0 , ( 54           ) 1AF5 , ( 55 C ,X  Eor )
23F5 , ( 56 C ,X  Eor ) 2EBF , ( 57 C C   RMB )
12F0 , ( 58       CLI ) 1AF9 , ( 59 N ,Y  Eor )
29D0 , ( 5A       PHY ) 80F0 , ( 5B           )
80F0 , ( 5C           ) 1AF8 , ( 5D N ,X  Eor )
23F8 , ( 5E N ,X  LSR ) 03BF , ( 5F C C   BBR )

32F0 , ( 60       RTS ) 00F3 , ( 61 C X>  ADC )
80F0 , ( 62           ) 80F0 , ( 63           )
3BD7 , ( 64 C     STZ ) 00F7 , ( 65 C     ADC )
30F7 , ( 66 C     Ror ) 2EBF , ( 67 C C   RMB )
2AF0 , ( 68       PLA ) 00F2 , ( 69 C #.  ADC )
30F1 , ( 6A A.    Ror ) 80F0 , ( 6B           )
1EFB , ( 6C N >   JMP ) 00FA , ( 6D N     ADC )
30FA , ( 6E N     Ror ) 03BF , ( 6F C C   BBR )

0FFD , ( 70 C rel BVS ) 00F4 , ( 71 C >Y  ADC )
00DE , ( 72 C >   ADC ) 80F0 , ( 73           )
3BD5 , ( 74 C ,X  STZ ) 00F5 , ( 75 C ,X  ADC )
30F5 , ( 76 C ,X  Ror ) 2EBF , ( 77 C C   RMB )
36F0 , ( 78       SEI ) 00F9 , ( 79 N ,Y  ADC )
2DD0 , ( 7A       PLY ) 80F0 , ( 7B           )
1EDC , ( 7C N X>  JMP ) 00F8 , ( 7D N ,X  ADC )
30F8 , ( 7E N ,X  Ror ) 03BF , ( 7F C C   BBR )

0CDD , ( 80 C rel BRA ) 38F3 , ( 81 C X>  STA )
80F0 , ( 82           ) 80F0 , ( 83           )
3AF7 , ( 84 C     STY ) 38F7 , ( 85 C     STA )
39F7 , ( 86 C     STX ) 37BF , ( 87 C C   SMB )
19F0 , ( 88       DEY ) 08D2 , ( 89 C #.  BIT )
41F0 , ( 8A       TXA ) 80F0 , ( 8B           )
3AFA , ( 8C N     STY ) 38FA , ( 8D N     STA )
39FA , ( 8E N     STX ) 04BF , ( 8F C C   BBS )

05FD , ( 90 C rel BCC ) 38F4 , ( 91 C >Y  STA )
38DE , ( 92 C >   STA ) 80F0 , ( 93           )
3AF5 , ( 94 C ,X  STY ) 38F5 , ( 95 C ,X  STA )
39F6 , ( 96 C ,Y  STX ) 37BF , ( 97 C C   SMB )
43F0 , ( 98       TYA ) 38F9 , ( 99 N ,Y  STA )
42F0 , ( 9A       TXS ) 80F0 , ( 9B           )
3BDA , ( 9C N     STZ ) 38F8 , ( 9D N ,X  STA )
3BD8 , ( 9E N ,X  STZ ) 04BF , ( 9F C C   BBS )

22F2 , ( A0 C #   LDY ) 20F3 , ( A1 C X>  LDA )
21F2 , ( A2 C #   LDX ) 80F0 , ( A3           )
22F7 , ( A4 C     LDY ) 20F7 , ( A5 C     LDA )
21F7 , ( A6 C     LDX ) 37BF , ( A7 C C   SMB )
3DF0 , ( A8       TAY ) 20F2 , ( A9 C #.  LDA )
3CF0 , ( AA       TAX ) 80F0 , ( AB           )
22FA , ( AC N     LDY ) 20FA , ( AD N     LDA )
21FA , ( AE N     LDX ) 04BF , ( AF C C   BBS )

06FD , ( B0 C rel BCS ) 20F4 , ( B1 C >Y  LDA )
20DE , ( B2 C >   LDA ) 80F0 , ( B3           )
22F5 , ( B4 C ,X  LDY ) 20F5 , ( B5 C ,X  LDA )
21F6 , ( B6 C ,X  LDX ) 37BF , ( B7 C C   SMB )
13F0 , ( B8       CLV ) 20F9 , ( B9 N ,Y  LDA )
42F0 , ( BA       TSX ) 80F0 , ( BB           )
22F8 , ( BC N ,X  LDY ) 20F8 , ( BD N ,X  LDA )
21F9 , ( BE N ,Y  LDX ) 04BF , ( BF C C   BBS )

16F2 , ( C0 C #.  CPY ) 14F3 , ( C1 X>    CMP )
80F0 , ( C2           ) 80F0 , ( C3           )
16F7 , ( C4 C     CPY ) 14F7 , ( C5 C     CMP )
17F7 , ( C6 C     DEC ) 37BF , ( C7 C C   SMB )
1DF0 , ( C8       INY ) 14F2 , ( C9 C #.  CMP )
18F0 , ( CA       DEX ) 4440 , ( CB       WAI )
16FA , ( CC N     CPY ) 14FA , ( CD N     CMP )
17FA , ( CE N     DEC ) 04BF , ( CF C C   BBS )

0AFD , ( D0 C rel BNE ) 14F4 , ( D1 C >Y  CMP )
14DE , ( D2 C >   CMP ) 80F0 , ( D3           )
80F0 , ( D4           ) 14F5 , ( D5 C ,X  CMP )
17F5 , ( D6 C ,X  DEC ) 37BF , ( D7 C C   SMB )
11F0 , ( D8       CLD ) 14F9 , ( D9 N ,Y  CMP )
28D0 , ( DA       PHX ) 4540 , ( DB       STP )
80F0 , ( DC           ) 14F8 , ( DD N ,X  CMP )
17F8 , ( DE N ,X  DEC ) 04BF , ( DF C C   BBS )

15F2 , ( E0 C #.  CPX ) 33F3 , ( E1 C X>  SBC )
80F0 , ( E2           ) 80F0 , ( E3           )
15F7 , ( E4 C     CPX ) 33F7 , ( E5 N     SBC )
1BF7 , ( E6 C     INC ) 37BF , ( E7 C C   SMB )
1CF0 , ( E8       INX ) 33F2 , ( E9 C #.  SBC )
24F0 , ( EA       NOP ) 80F0 , ( EB           )
15FA , ( EC N     CPX ) 33FA , ( ED N     SBC )
1BFA , ( EE N     INC ) 04BF , ( EF C C   BBS )

07FD , ( F0 C rel BEQ ) 33F4 , ( F1 C >Y  SBC )
33DE , ( F2 C >   SBC ) 80F0 , ( F3           )
80F0 , ( F4           ) 33F5 , ( F5 C ,X  SBC )
1BF5 , ( F6 C ,X  INC ) 37BF , ( F7 C C   SMB )
35F0 , ( F8       SED ) 33F9 , ( F9 N ,Y  SBC )
2CD0 , ( FA       PLX ) 80F0 , ( FB           )
80F0 , ( FC           ) 33F8 , ( FD N ,X  SBC )
1BF8 , ( FE N ,X  INC ) 04BF , ( FF C C   BBS )

\ ***********************************************************************
\
\ Assembler

\ single Byte Instructions

: BRK, 00 X c, ;     : CLC, 18 X c, ;     : CLD, D8 X c, ;     : CLI, 58 X c, ;
: CLV, B8 X c, ;     : DEX, CA X c, ;     : DEY, 88 X c, ;     : INX, E8 X c, ;
: INY, C8 X c, ;     : NOP, EA X c, ;     : PHA, 48 X c, ;     : PHP, 08 X c, ;
: PLA, 68 X c, ;     : PLP, 28 X c, ;     : RTI, 40 X c, ;     : RTS, 60 X c, ;
: SEC, 38 X c, ;     : SED, F8 X c, ;     : SEI, 78 X c, ;     : TAX, AA X c, ;
: TAY, A8 X c, ;     : TSX, BA X c, ;     : TXA, 8A X c, ;     : TXS, 9A X c, ;
: TYA, 98 X c, ;

: SOP, CPU& C@ C0 and IF X c, ELSE -1 ABORT" sop not supported"  THEN ;  ( C1 ---   )
: PLY, 7A SOP, ;  : PLX, FA SOP, ;  : PHY, 5A SOP, ;  : PHX, DA SOP, ;
: WAI, CB SOP, ;  : STP, DB SOP, ;

\ Addressingtokens
\ COPAD 00 : no Addressing valid

VARIABLE COPAD

: #. 02 COPAD C! ;   : ). 07 COPAD C! ;   : ,X 05 COPAD C! ;
: ,Y 06 COPAD C! ;   : X) 03 COPAD C! ;   : )Y 04 COPAD C! ;
: A. 01 COPAD C! ;

\ ->ASM2.F65
2D EMIT 3E EMIT
41 EMIT 53 EMIT 4D EMIT 32 EMIT
2E EMIT 46 EMIT 36 EMIT 35 EMIT CR

\ Branch Instructions

: RB,                                \ ( Addr1 ---    ) ( COPAD = 00 )
                                     \ (    C1 ---    ) COPAD = 02
  X c, COPAD C@ 02 =
  IF   00 COPAD C!                   \ C1 #.-Addressing
  ELSE 
      X here 1+ -
      dup 0 80 - < ABORT" branch out of range"
      dup 7F > ABORT" branch out of range"
  THEN  X c,  ;                      \ Put Addr in Memory

: BCC, 090 RB, ; : BCS, 0B0 RB, ; : BEQ, 0F0 RB, ; : BMI, 030 RB, ;
: BNE, 0D0 RB, ; : BPL, 010 RB, ; : BVC, 050 RB, ; : BVS, 070 RB, ;
: BRA, CPU& C@ C0 and IF 80 RB, ELSE -1 ABORT" bra not supported" THEN ;

\ These 4 Opcodes are only available in 65C02 and 6511Q.
\ Error 07 "Opcode-Bitaddressing is wrong"

: BOP,                              \    ( Addr1 C2  C3 ---  )
                                    \ or ( C1    C2  C3 ---  )
  CPU& C@ A0 and IF                 \ only 65C02 and 6511Q
      swap dup FFF8 and -1 ABORT" bop error jrd?!"         \ C2 is only 0 - 7
      4 lshift or                   \ make opcode
      dup 08 and IF
          X c, X ,                  \ BBS, BBR,
      ELSE X c, X c, THEN           \ RMB, SMB,
  ELSE -1 ABORT" bop not supported" THEN ;

: BBR, 0F BOP, ;                    \ ( Addr1 C2 ---  )
: BBS, 8F BOP, ;                    \ ( Addr1 C2 ---  )
: RMB, 07 BOP, ;                    \ ( C1    C2 ---  )
: SMB, 87 BOP, ;                    \ ( C1    C2 ---  )

: JMP,                              \ ( Addr1 ---    )
  COPAD C@ dup 07 = IF
      drop 6C                       \ Addr1 ) JMP
  ELSE 03 =                         \ Addr1 JMP
      IF CPU& C@ C0 and 0= ABORT" jmp (),x not supported" 7C
      ELSE 4C
      THEN
  THEN X c, X ,
  00 COPAD C! ;

: JSR,                              \ ( Addr1 ---    )
  20 X c, X , ;

\ Instructions with multiple Addressing
\
\ Offset: COPAD x 2 + 1 wenn N
\         COPAD x 2     wenn C
\ Content = AddrTokens of ASMTAB
\           or 80 = Error

CREATE FOPCADR
0A07 , 0101 , 8002 , 0C03 , 8004 , 0805 , 0906 , 0B0E ,

\
\    0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
\
\ C  07      01      02      03      04      05      06      0E
\ N      0A      01      80      0C      80      08      09      0B
\    ------  - A. -  - #. -  - X) -  - )Y -  - ,X -  - ,Y -  - ). -
\

: FOPC                                          \    (   C1 ---  )
                                                \ or ( N C1 ---  )
                                                \ or ( C C1 ---  )
  COPAD C@ 1 = IF 
      1 swap 1 swap 1 swap   \      ( 1 1 1 C1 --- )
  ELSE 1 ROT ROT swap        \      ( 1 N 1 C1 --- )
      dup FF00 and ROT       \ oder ( 1 C 0 C1 --- )
  THEN

  \ TOS C1 is a Token representing the Opcode corresponding to ASMTAB
  \ SOS Then comes a Flag indicating C or N -Data
  \ 3OS Then comes the Data or a Dummy in case of A.-Addr
  \ 4OS Last comes the Errorflag
  ASMTAB 100 cells + ASMTAB
  DO
      dup I @ 8 rshift =                       \ Opcode ? C1 = ?
      IF                                       \ Now test Addr
          COPAD C@ 06 = IF                     \ ,Y-Patch
              dup dup 39 =                     \ if STX
              swap 21 = or 0=                  \ or LDX not, otherwise
              IF swap 1 or swap THEN           \ make C a N
          THEN
          over COPAD C@
          cells FOPCADR + @                    \ look in FOPCADR
          swap IF 8 rshift ELSE 0ff and THEN   \ N => 1+ , C => 0+
          I @ 00f and =
          I @ 0f0 and CPU& C@ and 0<>          \ CPU-Test
          and                                  \ 1-Flag if ok
                                               \ ( 1 x x C1 F --- )
          IF I ASMTAB - 1 cells /              \ Recalculate Opcode from Offset
              X c,                             \ Compile Opcode
              drop                             \ C1 goes
              COPAD C@ 01 = IF                 \ No Operand if A.-Addr
                  2drop
              ELSE
                  IF X , ELSE X c, THEN        \ Compile Operand
              THEN
              drop                             \ drop Errorflag
              0 0 0 0 LEAVE                    \ Successflag and Dummys
          THEN
      THEN
      1 cells
  +LOOP drop 2drop ABORT" not supported" 0 COPAD C! ;

: ADC, 00 FOPC ; : and, 01 FOPC ; : ASL, 02 FOPC ; : CMP, 14 FOPC ;
: CPX, 15 FOPC ; : CPY, 16 FOPC ; : DEC, 17 FOPC ; : EOR, 1A FOPC ;
: INC, 1B FOPC ; : LDA, 20 FOPC ; : LDX, 21 FOPC ; : LDY, 22 FOPC ;
: LSR, 23 FOPC ; : ORA, 25 FOPC ; : ROL, 2F FOPC ; : ROR, 30 FOPC ;
: SBC, 33 FOPC ; : STA, 38 FOPC ; : STX, 39 FOPC ; : STY, 3A FOPC ;
: BIT, 08 FOPC ; : STZ, 3B FOPC ; : TSB, 3F FOPC ; : TRB, 3E FOPC ;

\ **********************************************************************
\
\ Disassembler
\
\ Format of Disassembler:
\
\ AAAA    BBB C DD EEEE
\
\ AAAA:  Addr in hex
\ BBB:   Opcode
\ C:     Operand embedded in opcode
\ DD:    Addressing
\ EEEE or
\ EE:    Operand in hex
\
\ Offset corresponds to ASMTAB-Offset

\ 1. Letter of Opcode
CREATE OPTAB1
41 C, 41 C, 41 C, 42 C, 42 C, 42 C,
42 C, 42 C, 42 C, 42 C, 42 C, 42 C,
42 C, 42 C, 42 C, 42 C, 43 C, 43 C,
43 C, 43 C, 43 C, 43 C, 43 C, 44 C,
44 C, 44 C, 45 C, 49 C, 49 C, 49 C,
4A C, 4A C, 4C C, 4C C, 4C C, 4C C,
4E C, 4F C, 50 C, 50 C, 50 C, 50 C,
50 C, 50 C, 50 C, 50 C, 52 C, 52 C,
52 C, 52 C, 52 C, 53 C, 53 C, 53 C,
53 C, 53 C, 53 C, 53 C, 53 C, 53 C,
54 C, 54 C, 54 C, 54 C, 54 C, 54 C,
54 C, 54 C, 57 C, 53 C,

\ 2.Letter of Opcode
CREATE OPTAB2
44 C, 4E C, 53 C, 42 C, 42 C, 43 C,
43 C, 45 C, 49 C, 4D C, 4E C, 50 C,
52 C, 52 C, 56 C, 56 C, 4C C, 4C C,
4C C, 4C C, 4D C, 50 C, 50 C, 45 C,
45 C, 45 C, 4F C, 4E C, 4E C, 4E C,
4D C, 53 C, 44 C, 44 C, 44 C, 53 C,
4F C, 52 C, 48 C, 48 C, 48 C, 48 C,
4C C, 4C C, 4C C, 4C C, 4D C, 4F C,
4F C, 54 C, 54 C, 42 C, 45 C, 45 C,
45 C, 4D C, 54 C, 54 C, 54 C, 54 C,
41 C, 41 C, 52 C, 53 C, 53 C, 58 C,
58 C, 59 C, 41 C, 54 C,

\ 3.Letter of Opcode
CREATE OPTAB3
43 C, 44 C, 4C C, 52 C, 53 C, 43 C,
53 C, 51 C, 54 C, 49 C, 45 C, 4C C,
41 C, 4B C, 43 C, 53 C, 43 C, 44 C,
49 C, 56 C, 50 C, 58 C, 59 C, 43 C,
58 C, 59 C, 52 C, 43 C, 58 C, 59 C,
50 C, 52 C, 41 C, 58 C, 59 C, 52 C,
50 C, 41 C, 41 C, 50 C, 58 C, 59 C,
41 C, 50 C, 58 C, 59 C, 42 C, 4C C,
52 C, 49 C, 53 C, 43 C, 43 C, 44 C,
49 C, 42 C, 41 C, 58 C, 59 C, 5A C,
58 C, 59 C, 42 C, 42 C, 58 C, 41 C,
53 C, 41 C, 49 C, 50 C,

CREATE ADTAB1

20  C, 41  C, 23  C, 58  C,
29  C, 2C  C, 2C  C, 20  C,
2C  C, 2C  C, 20  C, 29  C,
58  C, 20  C, 29  C, 20  C,

CREATE ADTAB2

20  C, 2E  C, 2E  C, 29  C,
59  C, 58  C, 59  C, 20  C,
58  C, 59  C, 20  C, 2E  C,
29  C, 20  C, 2E  C, 20  C,

CREATE ADTAB3

   0 C,    0 C,    3 C,    3 C,
   3 C,    3 C,    3 C,    3 C,
   2 C,    2 C,    2 C,    2 C,
   2 C,    1 C,    3 C,    3 C,

\ ->COLD.F65
2D EMIT 3E EMIT
43 EMIT 4F EMIT 4C EMIT 44 EMIT
2E EMIT 46 EMIT 36 EMIT 35 EMIT CR

\ end of definitions for assembler wordlist
previous definitions
