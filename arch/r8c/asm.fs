\ FORTH Assembler for R8C                    hfs 23:54 25.12.05
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
\
\ Autor:          Heinz Schnitter (hfs)
\
\ Information:
\
\ - R8C.ASM is a table driven assembler.
\ - R8C.ASM supports OPCodes of Renesas R8C Microcomputer in
\   postfix notation.

\ only forth definitions

require asm/basic.fs

 also ASSEMBLER definitions

require asm/target.fs

\ for tests only                hfs 07:47 04/24/92

\ : TC, base @ >r hex 0 <# # # #>       type r> base ! ;
\ : T,  base @ >r hex 0 <# # # # # #>   type r> base ! ;
\ : ta, base @ >r hex 0 <# # # # # # #> type r> base ! ;
: ta, true abort" This stupid 20 bit addressing mode doesn't work!" ;

 HERE                   ( Begin )

\ VARIABLEs and RESET                        hfs 10:03 04/23/92

      VARIABLE SSP                 \ save of SP
      VARIABLE <,>                 \ two addrs
      VARIABLE <M>                 \ mode searched for in table
      VARIABLE <OPC>               \ OPCode
      VARIABLE <.B>
      VARIABLE <S-OPND>
      VARIABLE <D-OPND>

 : ssave SP@ SSP ! ;

 : RESET       ( clears all variables )
   0 <M>   C!    $0FF <M>   1+ C!
   0 <OPC> C!    $0FF <OPC> 1+ C!
   <S-OPND> OFF  <D-OPND> OFF
   <,> OFF       <.B> OFF  ssave ;

 : opnd?  ( -- N ) SSP @ SP@ - 1 cells - 1 cells / ;
 : ,? <,> @ ;
 : S-OPND? <S-OPND> @ ;
 : D-OPND? <D-OPND> @ ;

 : WITHIN >R OVER > SWAP R> <= 0= OR 0= ;

 : 4B?  ( n -- n f ) ( within -8 .. 7 ? )
   DUP -$8   $7   WITHIN ;
 : >4B?  ( n -- n f ) 4B? 0= ;

 : 8B?  ( n -- n f ) ( within -128 .. 127 ? )
   DUP -$80   $7F   WITHIN ;
 : >8B?  ( n -- n f ) 8B? 0= ;

   $0ff   CONSTANT abs8
 : abs8?  ( n -- n f ) DUP abs8 u< ;
 : >abs8? ( n -- n f ) abs8? 0= ;

 : >opc <M> 1+ C@ <OPC> 1+ C! ;

 : M: CREATE  C, C,
       DOES> ,?
       IF    dup C@          <M>    c@ $F0 AND OR <M>    c!
              1+ C@          <M> 1+ c@ $F0 AND OR <M> 1+ c!
       ELSE  dup C@ 4 lshift <M>    c@ $0F AND OR <M>    c!
              1+ C@ 4 lshift <M> 1+ c@ $0F AND OR <M> 1+ c!
       THEN ;

\ address-modes hfs

 %0000 0 M: R0    ' R0 alias R0L  ' R0 alias R2R0  ' R0  alias C
 %0001 0 M: R1    ' R1 alias R0H  ' R1 alias R3R1  ' R1  alias D
 %0010 0 M: R2    ' R2 alias R1L                   ' R2  alias Z
 %0011 0 M: R3    ' R3 alias R1H                   ' R3  alias S
 %0100 0 M: A0                    ' A0 alias A1A0  ' A0  alias B
 %0101 0 M: A1                                     ' A1  alias O
 %0110 0 M: [A0]                                  ' [A0] alias I
 %0111 0 M: [A1]                                  ' [A1] alias U
 %1010 0 M: [SB]
 %1011 0 M: [FB]
 %1111 0 M: abs:16
 %0000 1 M: #
 %0001 1 M: INTBL
 %0010 1 M: INTBH
 %0011 1 M: FLG
 %0100 1 M: ISP
 %0101 1 M: SP
 %0110 1 M: SB
 %0111 1 M: FB
 
\ %0000 2 M: [SP]

\ two cells are used for each adress mode in the table:
\
\ cell 1 contains the src and dst MODE# searched for.
\
\ Cell 2 contains the xt of the action word for this address mode.
\
\ |     #src#dst|src  dst|                     |
\ |.....76543210 76543210 .....7654321076543210|
\ |          MODE        |            xt       |

 : TABLE:        ( -- addr ) \ generate new table
   CREATE HERE DUP 1 cells + , RESET ;

 : ;TABLE        ( addr -- ) \ change endpoint
   HERE SWAP ! ;

 : TAB,          ( xt opcode -- )
   <M> @ , , RESET ;

 : SEARCH.MODE.2 ( addr -- xt )
   TRUE SWAP
   DUP     C@ <OPC>     C!    ( opc from GROUP2: )
   DUP 1+  C@ <OPC> 1+  C!    ( opc from GROUP2: )
   2 + @ DUP @                ( tableaddr )
   SWAP 1 cells +             ( end+2 begin )
   ?DO [ forth ] I [ assembler ] @ <M> @ =
       IF [ forth ] I [ assembler ] 1 cells + @ swap 0= LEAVE THEN
   2 cells +LOOP ABORT" Addressmode failed" ;

 : SEARCH.MODE.4 ( addr -- xt )
   TRUE SWAP
   DUP     C@ <OPC>     C!    ( opc from GROUP4: )
   DUP 1+  C@ <OPC> 1+  C!    ( opc from GROUP4: )
   DUP 2 + C@ <OPC> 2 + C!    ( opc from GROUP4: )
   DUP 3 + C@ <OPC> 3 + C!    ( opc from GROUP4: )
   4 + @ DUP @                ( tableaddr )
   SWAP 1 cells +             ( end+2 begin )
   ?DO [ forth ] I [ assembler ] @ <M> @ =
       IF [ forth ] I [ assembler ] 1 cells + @ swap 0= LEAVE THEN
   2 cells +LOOP ABORT" Addressmode failed" ;

\ GROUPS                                     hfs 07:18 04/24/92

 : GROUP2:   CREATE C, C,  ,        ( xt opc opc  -- )
             DOES> SEARCH.MODE.2 EXECUTE RESET ;

 : GROUP4:   CREATE C, C,  C, C,  , ( xt opc opc  opc opc  -- )
             DOES> SEARCH.MODE.4 EXECUTE RESET ;

\ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 : , opnd? <s-opnd> ! ssave <,> ON ;
\ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

 : OPC, <OPC> C@ X C, <OPC> 1+ C@ X C, ;

 : [An],     <M> 1+ C@ $F0 AND
             CASE
                %01100000 of s-opnd?  \ [A0]
                             IF abs8? IF %10000000 ELSE %11000000 THEN
                             <M> 1+ C@ $0F AND OR <M> 1+ C! THEN
                          endof
                %01110000 of s-opnd?  \ [A1]
                             IF abs8? IF %10010000 ELSE %11010000 THEN
                             <M> 1+ C@ $0F AND OR <M> 1+ C! THEN
                          endof
             ENDCASE ;

 : ,[An]     <M> 1+ C@ $0F AND
             CASE
                %0110 of d-opnd?  \ [A0]
                         IF abs8? IF %1000 ELSE %1100 THEN
                         <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
                      endof
                %0111 of d-opnd?  \ [A1]
                         IF abs8? IF %1001 ELSE %1101 THEN
                         <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
                      endof
             ENDCASE ;

 : [SB],     s-opnd?
             IF >abs8? IF %11100000 <M> 1+ C@ $0F AND OR <M> 1+ C! THEN
             ELSE true abort" displacement expected" THEN ;

 : ,[SB]     d-opnd?
             IF >abs8? IF %1110 <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
             ELSE true abort" displacement expected" THEN ;

 : [FB],     s-opnd? 0= ABORT" displacement expected" ;

 : ,[FB]     d-opnd? 0= ABORT" displacement expected" ;

 : abs:16,   s-opnd? 0= ABORT" absolute Adr expected" ;

 : ,abs:16   d-opnd? 0= ABORT" absolute Adr expected" ;

 : #,        s-opnd? 0= ABORT" operand expected"
             <OPC> 2 + c@ %00000001 and 0= IF <.b> on THEN
             <OPC> 2 + c@ <OPC> c!
             <OPC> 3 + c@ <M> 1+ c@ $0F and or <M> 1+ c! ;

 : q#,       s-opnd? 0= ABORT" operand expected"
             s-opnd? d-opnd? and IF swap THEN
             >4B? ABORT" immediate > 4 Bit"
             4 lshift <M> 1+ c@ $0F and or <M> 1+ c! ;

 : ctrl,     <M> 1+ C@ %10000000 OR <M> 1+ C! ;

 : ,ctrl     <M> 1+ C@ $0F and 4 lshift
             <M> 1+ C@ $F0 and 4 rshift OR %10000000 or <M> 1+ C! ;
\ ----------------------------------------------------------------------------------------------
\ ----------------------------------------------------------------------------------------------
 : GROUP.1B: CREATE C,                 ( opc  -- )
             DOES> c@ X C,
             RESET ;
 %00000100 GROUP.1B: nop
 %11111011 GROUP.1B: reit
 %11110011 GROUP.1B: rts
\ ----------------------------------------------------------------------------------------------
 : GROUP.1B.l8: CREATE C,              ( opc  -- )
             DOES> c@ X C,
             opnd? 0= ABORT" label expected"
             >8B?  ABORT" label > 8 Bit" X C,
             RESET ;
 %01101000 GROUP.1B.l8: JGEU ' JGEU alias JC
 %01101001 GROUP.1B.l8: JGTU
 %01101010 GROUP.1B.l8: JEQ  ' JEQ  alias JZ
 %01101011 GROUP.1B.l8: JN
 %01101100 GROUP.1B.l8: JLTU ' JLTU alias JNC
 %01101101 GROUP.1B.l8: JLEU
 %01101110 GROUP.1B.l8: JNE  ' JNE  alias JNZ
 %01101111 GROUP.1B.l8: JPZ
 %11111110 GROUP.1B.l8: JMP.B
\ ----------------------------------------------------------------------------------------------
 : GROUP.1B.l16: CREATE C,              ( opc  -- )
             DOES> c@ X C,
             opnd? 0= ABORT" label expected"
             X ,
             RESET ;
 %11110100 GROUP.1B.l16: JMP.W
\ ----------------------------------------------------------------------------------------------
 : GROUP.2B: CREATE C, C,           ( opc opc  -- )
             DOES>  dup c@ X C, 1+ c@ X C,
             RESET ;
 %11101001 %01111100 GROUP.2B: smovb.b
 %11101000 %01111100 GROUP.2B: smovf.b
 %11101010 %01111100 GROUP.2B: sstr.b

 %11101001 %01111101 GROUP.2B: smovb.w
 %11101000 %01111101 GROUP.2B: smovf.w
 %11101010 %01111101 GROUP.2B: sstr.w
\ ----------------------------------------------------------------------------------------------
 : GROUP.2B.l8: CREATE C, C,         ( opc opc  -- )
             DOES>  dup c@ X C, 1+ c@ X C,
             opnd? 0= ABORT" label expected"
             >8B?  ABORT" label > 8 Bit" X C,
             RESET ;
 %11001000 %01111101 GROUP.2B.l8: JLE
 %11001001 %01111101 GROUP.2B.l8: JO
 %11001010 %01111101 GROUP.2B.l8: JGE
 %11001100 %01111101 GROUP.2B.l8: JGT
 %11001101 %01111101 GROUP.2B.l8: JNO
 %11001110 %01111101 GROUP.2B.l8: JLT
\ ----------------------------------------------------------------------------------------------
 : GROUP.2B.F: CREATE C, C,           ( opc opc  -- )
             DOES>   dup c@ X C, 1+ c@ <M> 1+ C@ $70 and or X C,
             RESET ;
 %00000101 %11101011 GROUP.2B.F: FCLR
 %00000100 %11101011 GROUP.2B.F: FSET
\ ----------------------------------------------------------------------------------------------
 : ctrl,R    opnd? <d-opnd> !
             ctrl,
             >opc OPC, ;

 : ctrl,[An] opnd? <d-opnd> !
             ctrl, ,[An]
             >opc OPC,
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : ctrl,[SB] opnd? <d-opnd> !
             ctrl, ,[SB]
             >opc OPC,
             abs8? IF X C, ELSE X , THEN ;

 : ctrl,[FB] opnd? <d-opnd> !
             ctrl, ,[FB]
             >opc OPC,
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : ctrl,abs:16 opnd? <d-opnd> !
             ctrl, ,abs:16
             >opc OPC,
             X , ;

 Table: st-control-reg
        INTBL , R0     ' ctrl,R TAB,        INTBL , R1     ' ctrl,R TAB,
        INTBL , R2     ' ctrl,R TAB,        INTBL , R3     ' ctrl,R TAB,
        INTBL , A0     ' ctrl,R TAB,        INTBL , A1     ' ctrl,R TAB,
        INTBL , [A0]   ' ctrl,[An] TAB,     INTBL , [A1]   ' ctrl,[An] TAB,
        INTBL , [SB]   ' ctrl,[SB] TAB,     INTBL , [FB]   ' ctrl,[FB] TAB,
        INTBL , abs:16 ' ctrl,abs:16 TAB,
        INTBH , R0     ' ctrl,R TAB,        INTBH , R1     ' ctrl,R TAB,
        INTBH , R2     ' ctrl,R TAB,        INTBH , R3     ' ctrl,R TAB,
        INTBH , A0     ' ctrl,R TAB,        INTBH , A1     ' ctrl,R TAB,
        INTBH , [A0]   ' ctrl,[An] TAB,     INTBH , [A1]   ' ctrl,[An] TAB,
        INTBH , [SB]   ' ctrl,[SB] TAB,     INTBH , [FB]   ' ctrl,[FB] TAB,
        INTBL , abs:16 ' ctrl,abs:16 TAB,
        FLG   , R0     ' ctrl,R TAB,        FLG   , R1     ' ctrl,R TAB,
        FLG   , R2     ' ctrl,R TAB,        FLG   , R3     ' ctrl,R TAB,
        FLG   , A0     ' ctrl,R TAB,        FLG   , A1     ' ctrl,R TAB,
        FLG   , [A0]   ' ctrl,[An] TAB,     FLG   , [A1]   ' ctrl,[An] TAB,
        FLG   , [SB]   ' ctrl,[SB] TAB,     FLG   , [FB]   ' ctrl,[FB] TAB,
        FLG   , abs:16 ' ctrl,abs:16 TAB,
        ISP   , R0     ' ctrl,R TAB,        ISP   , R1     ' ctrl,R TAB,
        ISP   , R2     ' ctrl,R TAB,        ISP   , R3     ' ctrl,R TAB,
        ISP   , A0     ' ctrl,R TAB,        ISP   , A1     ' ctrl,R TAB,
        ISP   , [A0]   ' ctrl,[An] TAB,     ISP   , [A1]   ' ctrl,[An] TAB,
        ISP   , [SB]   ' ctrl,[SB] TAB,     ISP   , [FB]   ' ctrl,[FB] TAB,
        ISP   , abs:16 ' ctrl,abs:16 TAB,
        SP    , R0     ' ctrl,R TAB,        SP    , R1     ' ctrl,R TAB,
        SP    , R2     ' ctrl,R TAB,        SP    , R3     ' ctrl,R TAB,
        SP    , A0     ' ctrl,R TAB,        SP    , A1     ' ctrl,R TAB,
        SP    , [A0]   ' ctrl,[An] TAB,     SP    , [A1]   ' ctrl,[An] TAB,
        SP    , [SB]   ' ctrl,[SB] TAB,     SP    , [FB]   ' ctrl,[FB] TAB,
        SP    , abs:16 ' ctrl,abs:16 TAB,
        SB    , R0     ' ctrl,R TAB,        SB    , R1     ' ctrl,R TAB,
        SB    , R2     ' ctrl,R TAB,        SB    , R3     ' ctrl,R TAB,
        SB    , A0     ' ctrl,R TAB,        SB    , A1     ' ctrl,R TAB,
        SB    , [A0]   ' ctrl,[An] TAB,     SB    , [A1]   ' ctrl,[An] TAB,
        SB    , [SB]   ' ctrl,[SB] TAB,     SB    , [FB]   ' ctrl,[FB] TAB,
        SB    , abs:16 ' ctrl,abs:16 TAB,
        FB    , R0     ' ctrl,R TAB,        FB    , R1     ' ctrl,R TAB,
        FB    , R2     ' ctrl,R TAB,        FB    , R3     ' ctrl,R TAB,
        FB    , A0     ' ctrl,R TAB,        FB    , A1     ' ctrl,R TAB,
        FB    , [A0]   ' ctrl,[An] TAB,     FB    , [A1]   ' ctrl,[An] TAB,
        FB    , [SB]   ' ctrl,[SB] TAB,     FB    , [FB]   ' ctrl,[FB] TAB,
        FB    , abs:16 ' ctrl,abs:16 TAB,

 ;TABLE

 st-control-reg %11111111 %01111011 GROUP2: stc

\ ----------------------------------------------------------------------------------------------
 : R,ctrl    ,ctrl
             >opc OPC, ;

 : [An],ctrl opnd? <d-opnd> !
             [An], ,ctrl
             >opc OPC,
             s-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : [SB],ctrl opnd? <d-opnd> !
             [SB], ,ctrl
             >opc OPC,
             abs8? IF X C, ELSE X , THEN ;

 : [FB],ctrl opnd? <d-opnd> !
             [FB], ,ctrl
             >opc OPC,
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : #,ctrl    #, ,ctrl
             <M> 1+ C@ %01110000 and <M> 1+ C!
             >opc OPC,
             X , ;

 Table: ld-control-reg
        R0   , INTBL ' R,ctrl TAB,        R0   , INTBH  ' R,ctrl TAB,
        R0   , FLG   ' R,ctrl TAB,        R0   , ISP    ' R,ctrl TAB,
        R0   , SP    ' R,ctrl TAB,        R0   , SB     ' R,ctrl TAB,
        R0   , FB    ' R,ctrl TAB,
        R1   , INTBL ' R,ctrl TAB,        R1   , INTBH  ' R,ctrl TAB,
        R1   , FLG   ' R,ctrl TAB,        R1   , ISP    ' R,ctrl TAB,
        R1   , SP    ' R,ctrl TAB,        R1   , SB     ' R,ctrl TAB,
        R1   , FB    ' R,ctrl TAB,
        R2   , INTBL ' R,ctrl TAB,        R2   , INTBH  ' R,ctrl TAB,
        R2   , FLG   ' R,ctrl TAB,        R2   , ISP    ' R,ctrl TAB,
        R2   , SP    ' R,ctrl TAB,        R2   , SB     ' R,ctrl TAB,
        R2   , FB    ' R,ctrl TAB,
        R3   , INTBL ' R,ctrl TAB,        R3   , INTBH  ' R,ctrl TAB,
        R3   , FLG   ' R,ctrl TAB,        R3   , ISP    ' R,ctrl TAB,
        R3   , SP    ' R,ctrl TAB,        R3   , SB     ' R,ctrl TAB,
        R3   , FB    ' R,ctrl TAB,
        A0   , INTBL ' R,ctrl TAB,        A0   , INTBH  ' R,ctrl TAB,
        A0   , FLG   ' R,ctrl TAB,        A0   , ISP    ' R,ctrl TAB,
        A0   , SP    ' R,ctrl TAB,        A0   , SB     ' R,ctrl TAB,
        A0   , FB    ' R,ctrl TAB,
        A1   , INTBL ' R,ctrl TAB,        A1   , INTBH  ' R,ctrl TAB,
        A1   , FLG   ' R,ctrl TAB,        A1   , ISP    ' R,ctrl TAB,
        A1   , SP    ' R,ctrl TAB,        A1   , SB     ' R,ctrl TAB,
        A1   , FB    ' R,ctrl TAB,
        [A0] , INTBL ' [An],ctrl TAB,   [A0]   , INTBH  ' [An],ctrl TAB,
        [A0] , FLG   ' [An],ctrl TAB,   [A0]   , ISP    ' [An],ctrl TAB,
        [A0] , SP    ' [An],ctrl TAB,   [A0]   , SB     ' [An],ctrl TAB,
        [A0] , FB    ' [An],ctrl TAB,
        [A1] , INTBL ' [An],ctrl TAB,   [A1]   , INTBH  ' [An],ctrl TAB,
        [A1] , FLG   ' [An],ctrl TAB,   [A1]   , ISP    ' [An],ctrl TAB,
        [A1] , SP    ' [An],ctrl TAB,   [A1]   , SB     ' [An],ctrl TAB,
        [A1] , FB    ' [An],ctrl TAB,
        [SB] , INTBL ' [SB],ctrl TAB,   [SB]   , INTBH  ' [SB],ctrl TAB,
        [SB] , FLG   ' [SB],ctrl TAB,   [SB]   , ISP    ' [SB],ctrl TAB,
        [SB] , SP    ' [SB],ctrl TAB,   [SB]   , SB     ' [SB],ctrl TAB,
        [SB] , FB    ' [FB],ctrl TAB,
        [FB] , INTBL ' [FB],ctrl TAB,   [FB]   , INTBH  ' [FB],ctrl TAB,
        [FB] , FLG   ' [FB],ctrl TAB,   [FB]   , ISP    ' [FB],ctrl TAB,
        [FB] , SP    ' [FB],ctrl TAB,   [FB]   , SB     ' [FB],ctrl TAB,
        [FB] , FB    ' [FB],ctrl TAB,
         #   , INTBL ' #,ctrl TAB,        #    , INTBH  ' #,ctrl TAB,
         #   , FLG   ' #,ctrl TAB,        #    , ISP    ' #,ctrl TAB,
         #   , SP    ' #,ctrl TAB,        #    , SB     ' #,ctrl TAB,
         #   , FB    ' #,ctrl TAB,
 ;TABLE

 ld-control-reg %00000000 %11101011  %11111111 %01111010 GROUP4: ldc

\ ----------------------------------------------------------------------------------------------
 : (R)    <OPC> 1+ c@ <M> 1+ c@ 4 rshift or <M> 1+ c!
          >opc OPC, ;


 : ([An]) <OPC> 1+ c@ <M> 1+ c@ 4 rshift or <M> 1+ c!
          <M> 1+ C@ $0F AND
          CASE
              %0110 of opnd?  \ [A0]
                       IF abs8? IF %1000 ELSE %1100 THEN
                       <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
                    endof
              %0111 of opnd?  \ [A1]
                       IF abs8? IF %1001 ELSE %1101 THEN
                       <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
                    endof
           ENDCASE
           >opc OPC,
           <opc> c@ %01111101 = <opc> 1+ c@ %00101100 = and   \ $12345 [ao] jmpi.w
           <opc> c@ %01111101 = <opc> 1+ c@ %00101101 = and   \ $12345 [a1] jmpi.w
           or
           IF   opnd? IF 8B? IF X C, ELSE ta, THEN THEN
           ELSE opnd? IF 8B? IF X C, ELSE X , THEN THEN THEN ;

 : ([SB])  <OPC> 1+ c@ <M> 1+ c@ 4 rshift or <M> 1+ c!
           opnd? <d-opnd> !
           ,[SB]
           >opc OPC,
           8B? IF X C, ELSE X , THEN ;

 : ([FB])  <OPC> 1+ c@ <M> 1+ c@ 4 rshift or <M> 1+ c!
           opnd? <d-opnd> !
           ,[FB]
           >opc OPC,
           X C, ;

 : (abs:16) <OPC> 1+ c@ <M> 1+ c@ 4 rshift or <M> 1+ c!
           opnd? <d-opnd> !
           ,abs:16
           >opc OPC,
           X , ;

 : (#)     opnd? 0= ABORT" operand expected"
           <OPC> 1+ c@ %01000000 =
           IF   <OPC> c@
                CASE
                    %01110100 OF %01111100 <OPC> c! ENDOF
                    %01110101 OF %01111101 <OPC> c! ENDOF
                ENDCASE
           ELSE
                true abort" push.?:g only"
           THEN %11100010 <M> 1+ c!
           >opc OPC,
           8B? IF X C, ELSE X , THEN ;

 Table: 2ByteOPC(dsp8/dsp16)          \ 2ByteOPC(dsp8/dsp16)
        R0          ' (R) TAB,        R1     ' (R) TAB,
        R2          ' (R) TAB,        R3     ' (R) TAB,
        A0          ' (R) TAB,        A1     ' (R) TAB,
        [A0]        ' ([An]) TAB,     [A1]   ' ([An]) TAB,
        [SB]        ' ([SB]) TAB,     [FB]   ' ([FB]) TAB,
        abs:16      ' (abs:16) TAB,    #     ' (#) TAB,
 ;TABLE

 2ByteOPC(dsp8/dsp16) %11110000 %01110110 GROUP2: abs.b
 2ByteOPC(dsp8/dsp16) %11100000 %01110110 GROUP2: adcf.b
 2ByteOPC(dsp8/dsp16) %11010000 %01110110 GROUP2: div.b
 2ByteOPC(dsp8/dsp16) %11000000 %01110110 GROUP2: divu.b
 2ByteOPC(dsp8/dsp16) %10010000 %01110110 GROUP2: divx.b
 2ByteOPC(dsp8/dsp16) %01010000 %01110100 GROUP2: neg.b
 2ByteOPC(dsp8/dsp16) %01110000 %01110100 GROUP2: not.b:g
 2ByteOPC(dsp8/dsp16) %11010000 %01110100 GROUP2: pop.b:g
 2ByteOPC(dsp8/dsp16) %01000000 %01110100 GROUP2: push.b:g
 2ByteOPC(dsp8/dsp16) %10100000 %01110110 GROUP2: rolc.b
 2ByteOPC(dsp8/dsp16) %10110000 %01110110 GROUP2: rorc.b

 2ByteOPC(dsp8/dsp16) %11110000 %01110111 GROUP2: abs.w
 2ByteOPC(dsp8/dsp16) %11100000 %01110111 GROUP2: adcf.w
 2ByteOPC(dsp8/dsp16) %11010000 %01110111 GROUP2: div.w
 2ByteOPC(dsp8/dsp16) %11000000 %01110111 GROUP2: divu.w
 2ByteOPC(dsp8/dsp16) %10010000 %01110111 GROUP2: divx.w
 2ByteOPC(dsp8/dsp16) %01010000 %01110101 GROUP2: neg.w
 2ByteOPC(dsp8/dsp16) %01110000 %01110101 GROUP2: not.w:g
 2ByteOPC(dsp8/dsp16) %11010000 %01110101 GROUP2: pop.w:g
 2ByteOPC(dsp8/dsp16) %01000000 %01110101 GROUP2: push.w:g
 2ByteOPC(dsp8/dsp16) %10100000 %01110111 GROUP2: rolc.w
 2ByteOPC(dsp8/dsp16) %10110000 %01110111 GROUP2: rorc.w

 2ByteOPC(dsp8/dsp16) %00100000 %01111101 GROUP2: jmpi.w
 2ByteOPC(dsp8/dsp16) %00000000 %01111101 GROUP2: jmpi.a

\ ----------------------------------------------------------------------------------------------
 : q#,R      opnd? <d-opnd> !
             q#,
             >opc OPC, ;

 : q#,[an]   opnd? <d-opnd> !
             q#, ,[an]
             >opc OPC,
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : q#,[sb]   opnd? <d-opnd> !
             q#, ,[sb]
             >opc OPC,
             abs8? IF X C, ELSE X , THEN ;

 : q#,[fb]   opnd? <d-opnd> !
             q#, ,[fb]
             >opc OPC,
             X C, ;

 : q#,abs:16 opnd? <d-opnd> !
             q#, ,abs:16
             >opc OPC,
             X , ;
 Table: quick
        #   , R0     ' q#,R TAB,        #   , R1     ' q#,R TAB,
        #   , R2     ' q#,R TAB,        #   , R3     ' q#,R TAB,
        #   , A0     ' q#,R TAB,        #   , A1     ' q#,R TAB,
        #   , [A0]   ' q#,[An] TAB,     #   , [A1]   ' q#,[An] TAB,
        #   , [SB]   ' q#,[SB] TAB,     #   , [FB]   ' q#,[FB] TAB,
        #   , abs:16 ' q#,abs:16 TAB,
 ;TABLE

 quick %00000000 %11001000 GROUP2: add.b:q
 quick %00000000 %11010000 GROUP2: cmp.b:q
 quick %00000000 %11011000 GROUP2: mov.b:q

 quick %00000000 %11001001 GROUP2: add.w:q
 quick %00000000 %11010001 GROUP2: cmp.w:q
 quick %00000000 %11011001 GROUP2: mov.w:q

\ ----------------------------------------------------------------------------------------------
 : R,R      >opc OPC, ;

 : R,[An]    opnd? <d-opnd> !
             ,[An]
             >opc OPC,
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : R,[SB]    opnd? <d-opnd> !
             ,[SB]
             >opc OPC,
             abs8? IF X C, ELSE X , THEN ;

 : R,[FB]    opnd? <d-opnd> !
             ,[FB]
             >opc OPC,
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : R,abs:16  opnd? <d-opnd> !
             ,abs:16
             >opc OPC,
             X , ;
\ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 : [An],R    opnd? <d-opnd> !
             [An],
             >opc OPC,
             s-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : [An],[An] opnd? <d-opnd> !
             [An], ,[An]
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             s-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : [An],[SB] opnd? <d-opnd> !
             [An], ,[SB]
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             s-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN
             abs8? IF X C, ELSE X , THEN ;

 : [An],[FB] opnd? <d-opnd> !
             [An], ,[FB]
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             s-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : [An],abs:16 opnd? <d-opnd> !
             [An], ,abs:16
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             s-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN
             X , ;
\ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 : [SB],R    opnd? <d-opnd> !
             [SB],
             >opc OPC,
             abs8? IF X C, ELSE X , THEN ;

 : [SB],[An] opnd? <d-opnd> !
             [SB], ,[An]
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             abs8? IF X C, ELSE X , THEN
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : [SB],[SB] opnd? <d-opnd> !
             [SB], ,[SB]
             >opc OPC,
             swap
             abs8? IF X C, ELSE X , THEN
             abs8? IF X C, ELSE X , THEN ;

 : [SB],[FB] opnd? <d-opnd> !
             [SB], ,[FB]
             >opc OPC,
             swap
             abs8? IF X C, ELSE X , THEN
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : [SB],abs:16 opnd? <d-opnd> !
             [SB], ,abs:16
             >opc OPC,
             swap
             abs8? IF X C, ELSE X , THEN
             X , ;
\ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 : [FB],R    opnd? <d-opnd> !
             [FB],
             >opc OPC,
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : [FB],[An] opnd? <d-opnd> !
             [FB], ,[An]
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             >8B? ABORT" displacement > 8 Bit"
             X C, 
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : [FB],[SB] opnd? <d-opnd> !
             [FB], ,[SB]
             >opc OPC,
             swap
             >8B? ABORT" displacement > 8 Bit"
             X C, 
             abs8? IF X C, ELSE X , THEN ;

 : [FB],[FB] opnd? <d-opnd> !
             [FB], ,[FB]
             >opc OPC,
             swap
             >8B? ABORT" displacement > 8 Bit"
             X C, 
             d-opnd?
             IF 8B?   IF X C, ELSE X , THEN THEN ;

 : [FB],abs:16 opnd? <d-opnd> !
             [FB], ,abs:16
             >opc OPC,
             swap
             >8B? ABORT" displacement > 8 Bit"
             X C,
             X , ;
\ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 : abs:16,R  opnd? <d-opnd> !
             abs:16,
             >opc OPC,
             X , ;

 : abs:16,[An] opnd? <d-opnd> !
             abs:16, ,[An]
             >opc OPC,
             s-opnd? d-opnd? and IF swap THEN
             X ,
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN ;

 : abs:16,[SB] opnd? <d-opnd> !
             abs:16, ,[SB]
             >opc OPC,
             swap
             X ,
             abs8? IF X C, ELSE X , THEN ;

 : abs:16,[FB] opnd? <d-opnd> !
             abs:16, ,[FB]
             >opc OPC,
             swap
             X ,
             >8B? ABORT" displacement > 8 Bit"
             X C, ;

 : abs:16,abs:16 opnd? <d-opnd> !
             abs:16, ,abs:16
             >opc OPC,
             swap
             X ,
             X , ;
\ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 : #,R       opnd? <d-opnd> !
             #,
             >opc OPC,
             <.b> @ IF X C, ELSE X , THEN ;

 : #,[An]    opnd? <d-opnd> !
             #, ,[An]
             >opc OPC,
             d-opnd?
             IF abs8? IF X C, ELSE X , THEN THEN
             <.b> @ IF X C, ELSE X , THEN ;

 : #,[SB]    opnd? <d-opnd> !
             #, ,[SB]
             >opc OPC,
             abs8? IF X C, ELSE X , THEN
             <.b> @ IF X C, ELSE X , THEN ;

 : #,[FB]    opnd? <d-opnd> !
             #, ,[FB]
             >opc OPC,
             >8B? ABORT" displacement > 8 Bit"
             X C, 
             <.b> @ IF X C, ELSE X , THEN ;

 : #,abs:16  opnd? <d-opnd> !
             #, ,abs:16
             >opc OPC,
             X ,
             <.b> @ IF X C, ELSE X , THEN ;
\ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 Table: generic
        R0   , R0     ' R,R TAB,        R0   , R1     ' R,R TAB,
        R0   , R2     ' R,R TAB,        R0   , R3     ' R,R TAB,
        R0   , A0     ' R,R TAB,        R0   , A1     ' R,R TAB,
        R0   , [A0]   ' R,[An] TAB,     R0   , [A1]   ' R,[An] TAB,
        R0   , [SB]   ' R,[SB] TAB,     R0   , [FB]   ' R,[FB] TAB,
        R0   , abs:16 ' R,abs:16 TAB,

        R1   , R0     ' R,R TAB,        R1   , R1     ' R,R TAB,
        R1   , R2     ' R,R TAB,        R1   , R3     ' R,R TAB,
        R1   , A0     ' R,R TAB,        R1   , A1     ' R,R TAB,
        R1   , [A0]   ' R,[An] TAB,     R1   , [A1]   ' R,[An] TAB,
        R1   , [SB]   ' R,[SB] TAB,     R1   , [FB]   ' R,[FB] TAB,
        R1   , abs:16 ' R,abs:16 TAB,

        R2   , R0     ' R,R TAB,        R2   , R1     ' R,R TAB,
        R2   , R2     ' R,R TAB,        R2   , R3     ' R,R TAB,
        R2   , A0     ' R,R TAB,        R2   , A1     ' R,R TAB,
        R2   , [A0]   ' R,[An] TAB,     R2   , [A1]   ' R,[An] TAB,
        R2   , [SB]   ' R,[SB] TAB,     R2   , [FB]   ' R,[FB] TAB,
        R2   , abs:16 ' R,abs:16 TAB,

        R3   , R0     ' R,R TAB,        R3   , R1     ' R,R TAB,
        R3   , R2     ' R,R TAB,        R3   , R3     ' R,R TAB,
        R3   , A0     ' R,R TAB,        R3   , A1     ' R,R TAB,
        R3   , [A0]   ' R,[An] TAB,     R3   , [A1]   ' R,[An] TAB,
        R3   , [SB]   ' R,[SB] TAB,     R3   , [FB]   ' R,[FB] TAB,
        R3   , abs:16 ' R,abs:16 TAB,

        A0   , R0     ' R,R TAB,        A0   , R1     ' R,R TAB,
        A0   , R2     ' R,R TAB,        A0   , R3     ' R,R TAB,
        A0   , A0     ' R,R TAB,        A0   , A1     ' R,R TAB,
        A0   , [A0]   ' R,[An] TAB,     A0   , [A1]   ' R,[An] TAB,
        A0   , [SB]   ' R,[SB] TAB,     A0   , [FB]   ' R,[FB] TAB,
        A0   , abs:16 ' R,abs:16 TAB,

        A1   , R0     ' R,R TAB,        A1   , R1     ' R,R TAB,
        A1   , R2     ' R,R TAB,        A1   , R3     ' R,R TAB,
        A1   , A0     ' R,R TAB,        A1   , A1     ' R,R TAB,
        A1   , [A0]   ' R,[An] TAB,     A1   , [A1]   ' R,[An] TAB,
        A1   , [SB]   ' R,[SB] TAB,     A1   , [FB]   ' R,[FB] TAB,
        A1   , abs:16 ' R,abs:16 TAB,

        [A0] , R0     ' [An],R  TAB,      [A0] , R1     ' [An],R  TAB,
        [A0] , R2     ' [An],R  TAB,      [A0] , R3     ' [An],R  TAB,
        [A0] , A0     ' [An],R  TAB,      [A0] , A1     ' [An],R  TAB,
        [A0] , [A0]   ' [An],[An] TAB,    [A0] , [A1]   ' [An],[An] TAB,
        [A0] , [SB]   ' [An],[SB] TAB,    [A0] , [FB]   ' [An],[FB] TAB,
        [A0] , abs:16 ' [An],abs:16 TAB,

        [A1] , R0     ' [An],R  TAB,      [A1] , R1     ' [An],R  TAB,
        [A1] , R2     ' [An],R  TAB,      [A1] , R3     ' [An],R  TAB,
        [A1] , A0     ' [An],R  TAB,      [A1] , A1     ' [An],R  TAB,
        [A1] , [A0]   ' [An],[An] TAB,    [A1] , [A1]   ' [An],[An] TAB,
        [A1] , [SB]   ' [An],[SB] TAB,    [A1] , [FB]   ' [An],[FB] TAB,
        [A1] , abs:16 ' [An],abs:16 TAB,

        [SB] , R0     ' [SB],R TAB,       [SB] , R1     ' [SB],R TAB,
        [SB] , R2     ' [SB],R TAB,       [SB] , R3     ' [SB],R TAB,
        [SB] , A0     ' [SB],R TAB,       [SB] , A1     ' [SB],R TAB,
        [SB] , [A0]   ' [SB],[An] TAB,    [SB] , [A1]   ' [SB],[An] TAB,
        [SB] , [SB]   ' [SB],[SB] TAB,    [SB] , [FB]   ' [SB],[FB] TAB,
        [SB] , abs:16 ' [SB],abs:16 TAB,

        [FB] , R0     ' [FB],R TAB,       [FB] , R1     ' [FB],R TAB,
        [FB] , R2     ' [FB],R TAB,       [FB] , R3     ' [FB],R TAB,
        [FB] , A0     ' [FB],R TAB,       [FB] , A1     ' [FB],R TAB,
        [FB] , [A0]   ' [FB],[An] TAB,    [FB] , [A1]   ' [FB],[An] TAB,
        [FB] , [SB]   ' [FB],[SB] TAB,    [FB] , [FB]   ' [FB],[FB] TAB,
        [FB] , abs:16 ' [FB],abs:16 TAB,

         #   , R0     '  #,R TAB,          #   , R1     ' #,R TAB,
         #   , R2     '  #,R TAB,          #   , R3     ' #,R TAB,
         #   , A0     '  #,R TAB,          #   , A1     ' #,R TAB,
         #   , [A0]   '  #,[An] TAB,       #   , [A1]   ' #,[An] TAB,
         #   , [SB]   '  #,[SB] TAB,       #   , [FB]   ' #,[FB] TAB,
         #   , abs:16 '  #,abs:16 TAB,

      abs:16 , R0     ' abs:16,R TAB,      abs:16 , R1   ' abs:16,R TAB,
      abs:16 , R2     ' abs:16,R TAB,      abs:16 , R3   ' abs:16,R TAB,
      abs:16 , A0     ' abs:16,R TAB,      abs:16 , A1   ' abs:16,R TAB,
      abs:16 , [A0]   ' abs:16,[An] TAB,   abs:16 , [A1] ' abs:16,[An] TAB,
      abs:16 , [SB]   ' abs:16,[SB] TAB,   abs:16 , [FB] ' abs:16,[FB] TAB,
      abs:16 , abs:16 ' abs:16,abs:16 TAB,
 ;TABLE

 generic %01100000 %01110110  %11111111 %10110000 GROUP4: adc.b
 generic %01000000 %01110110  %11111111 %10100000 GROUP4: add.b:g
 generic %00100000 %01110110  %11111111 %10010000 GROUP4: and.b:g
 generic %10000000 %01110110  %11111111 %11000000 GROUP4: cmp.b:g
 generic %11000000 %01110100  %11111111 %01110010 GROUP4: mov.b:g
 generic %01010000 %01111100  %11111111 %01111000 GROUP4: mul.b:g
 generic %01000000 %01111100  %11111111 %01110000 GROUP4: mulu.b:g
 generic %00110000 %01110110  %11111111 %10011000 GROUP4: or.b:g
 generic %01110000 %01110110  %11111111 %10111000 GROUP4: sbb.b
 generic %01010000 %01110110  %11111111 %10101000 GROUP4: sub.b:g
 generic %00000000 %01110110  %11111111 %10000000 GROUP4: tst.b
 generic %00010000 %01110110  %11111111 %10001000 GROUP4: xor.b

 generic %01100000 %01110111  %11111111 %10110001 GROUP4: adc.w
 generic %01000000 %01110111  %11111111 %10100001 GROUP4: add.w:g
 generic %00100000 %01110111  %11111111 %10010001 GROUP4: and.w:g
 generic %10000000 %01110111  %11111111 %11000001 GROUP4: cmp.w:g
 generic %11000000 %01110101  %11111111 %01110011 GROUP4: mov.w:g
 generic %01010000 %01111101  %11111111 %01111001 GROUP4: mul.w:g
 generic %01000000 %01111101  %11111111 %01110001 GROUP4: mulu.w:g
 generic %00110000 %01110111  %11111111 %10011001 GROUP4: or.w:g
 generic %01110000 %01110111  %11111111 %10111001 GROUP4: sbb.w
 generic %01010000 %01110111  %11111111 %10101001 GROUP4: sub.w:g
 generic %00000000 %01110111  %11111111 %10000001 GROUP4: tst.w
 generic %00010000 %01110111  %11111111 %10001001 GROUP4: xor.w

\ ----------------------------------------------------------------------------------------------
 : r1h,      <OPC> 1 + c@ $F0 and <M> 1+ c@ $0F and or <M> 1+ c! ;

 : R1H,R      r1h, r,r ;

 : R1H,[An]   r1h, r,[An] ;

 : R1H,[SB]   r1h, r,[SB] ;

 : R1H,[FB]   r1h, r,[FB] ;

 : R1H,abs:16 r1h, r,abs:16 ;

\ ----------------------------------------------------------------------------------------------
 : rot#        CASE
                 1 of $00 endof
                 2 of $10 endof
                 3 of $20 endof
                 4 of $30 endof
                 5 of $40 endof
                 6 of $50 endof
                 7 of $60 endof
                 8 of $70 endof
                -1 of $80 endof
                -2 of $90 endof
                -3 of $A0 endof
                -4 of $B0 endof
                -5 of $C0 endof
                -6 of $D0 endof
                -7 of $E0 endof
                -8 of $F0 endof
                abort
               ENDCASE ;

 : rot#,       s-opnd? 0= ABORT" operand expected"
               <OPC> 2 + c@ <OPC> c!
               s-opnd? d-opnd? and IF swap THEN
               rot# <M> 1+ c@ $0F and or <M> 1+ c! ;

 : rot#,R      rot#,
               >opc OPC, ;

 : rot#,[An]   opnd? <d-opnd> !
               rot#, ,[An]
               >opc OPC,
               d-opnd?
               IF abs8? IF X C, ELSE X , THEN THEN ;

 : rot#,[SB]   opnd? <d-opnd> !
               rot#, ,[SB]
               >opc OPC,
               abs8? IF X C, ELSE X , THEN ;

 : rot#,[FB]   opnd? <d-opnd> !
               rot#, ,[FB]
               >opc OPC,
               >8B? ABORT" displacement > 8 Bit"
               X C, ;

 : rot#,abs:16 opnd? <d-opnd> !
               rot#, ,abs:16
               >opc OPC,
               X , ;

 Table: rot-group
         R1H , R0L    '  R1H,R TAB,          R1H , R0H    ' R1H,R TAB,
         R1H , R1L    '  R1H,R TAB,          R1H , R3     ' R1H,R TAB,
         R1H , A0     '  R1H,R TAB,          R1H , A1     ' R1H,R TAB,
         R1H , [A0]   '  R1H,[An] TAB,       R1H , [A1]   ' R1H,[An] TAB,
         R1H , [SB]   '  R1H,[SB] TAB,       R1H , [FB]   ' R1H,[FB] TAB,
         R1H , abs:16 '  R1H,abs:16 TAB,
           # , R0L    ' rot#,R TAB,            # , R0H    ' rot#,R TAB,
           # , R1L    ' rot#,R TAB,            # , R3     ' rot#,R TAB,
           # , A0     ' rot#,R TAB,            # , A1     ' rot#,R TAB,
           # , [A0]   ' rot#,[An] TAB,         # , [A1]   ' rot#,[An] TAB,
           # , [SB]   ' rot#,[SB] TAB,         # , [FB]   ' rot#,[FB] TAB,
           # , abs:16 ' rot#,abs:16 TAB,
 ;TABLE

 rot-group %00001111 %11100001 %01101111 %01110101 GROUP4: rot.w
 rot-group %00001111 %11101001 %11101111 %01110101 GROUP4: shl.w
 rot-group %00001111 %11110001 %11111111 %01110101 GROUP4: sha.w

 rot-group %00001111 %11100000 %01101111 %01110100 GROUP4: rot.b
 rot-group %00001111 %11101000 %11101111 %01110100 GROUP4: shl.b
 rot-group %00001111 %11110000 %11111111 %01110100 GROUP4: sha.b

\ ----------------------------------------------------------------------------------------------
 ' noop alias bit

 : bit,      s-opnd? 0= ABORT" operand expected"
             <OPC> 1+ C@ $F0 AND <M> 1+ c@ $0F AND OR <M> 1+ c! ;

 : bit,R      bit,
              >opc OPC,
              %00000111 and
              X c, ;

 : bit[An]    <OPC> 1+ c@ $F0 AND <M> 1+ c@ 4 rshift or <M> 1+ c!
              <M> 1+ C@ $0F AND
              CASE
              %0110 of opnd?  \ [A0]
                       IF abs8? IF %1000 ELSE %1100 THEN
                       <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
                    endof
              %0111 of opnd?  \ [A1]
                       IF abs8? IF %1001 ELSE %1101 THEN
                       <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
                    endof
              ENDCASE
              >opc OPC,
              opnd?
              IF abs8? IF X C, ELSE X , THEN THEN ;

 : bit,[SB]   opnd? <d-opnd> !
              bit, ,[SB]
              3 lshift swap %00000111 and or
              >abs8? IF %1110 <M> 1+ C@ $F0 AND OR <M> 1+ C! THEN
              >opc OPC,
              abs8? IF X C, ELSE X , THEN ;

 : bit,[FB]   opnd? <d-opnd> !
              bit, ,[FB]
              >opc OPC,
              3 lshift swap %00000111 and or
              >8B? ABORT" displacement > 8 Bit"
              X C, ;

 : bit,abs:16 opnd? <d-opnd> !
              bit, ,abs:16
              >opc OPC,
              3 lshift swap %00000111 and or
              X , ;

 Table: bit-group
         bit , R0     '  bit,R TAB,          bit , R1     ' bit,R TAB,
         bit , R2     '  bit,R TAB,          bit , R3     ' bit,R TAB,
         bit , A0     '  bit,R TAB,          bit , A1     ' bit,R TAB,
               [A0]   '   bit[An] TAB,             [A1]   '  bit[An] TAB,
         bit , [SB]   '  bit,[SB] TAB,       bit , [FB]   ' bit,[FB] TAB,
         bit , abs:16 '  bit,abs:16 TAB,
 ;TABLE

 bit-group %01001111 %01111110 GROUP2: band
 bit-group %10001111 %01111110 GROUP2: bclr:g
 bit-group %01011111 %01111110 GROUP2: bnand
 bit-group %01111111 %01111110 GROUP2: bnor
 bit-group %10101111 %01111110 GROUP2: bnot:g
 bit-group %00111111 %01111110 GROUP2: bntst
 bit-group %11011111 %01111110 GROUP2: bnxor
 bit-group %01101111 %01111110 GROUP2: bor
 bit-group %10011111 %01111110 GROUP2: bset:g
 bit-group %10111111 %01111110 GROUP2: btst:g
 bit-group %00001111 %01111110 GROUP2: btstc
 bit-group %00011111 %01111110 GROUP2: btsts
 bit-group %11001111 %01111110 GROUP2: bxor
\ ----------------------------------------------------------------------------------------------
\ register definition
  ' R2 Alias rp
  ' R0 Alias tos
  ' A1 Alias ip
  ' A0 Alias w
  ' [A1] Alias [ip]
  ' [A0] Alias [w]
\ ----------------------------------------------------------------------------------------------

: IF,     dup $FF u> IF  dup 8 rshift X c,  THEN  X c, ;
	  
	  $68 Constant u<
	  $69 Constant u<=
	  $6A Constant 0<>
	  $6B Constant 0>=
	  $6C Constant u>=
	  $6D Constant u>
	  $6E Constant 0=
	  $6F Constant 0<
	  $7DC8 Constant >
	  $7DC9 Constant no
	  $7DCA Constant <
	  $7DCC Constant <=
	  $7DCD Constant o
	  $7DCE Constant >=

: IF          IF, X here  0 X c, reset ;
: THEN        X here  over - swap X c! reset  ;
: AHEAD       $FE IF ;
: ELSE        AHEAD swap THEN reset ;
: WHILE       IF swap reset ;
: BEGIN       X here reset ;
: UNTIL       IF, X here -  X c, reset ;
: AGAIN       $FE UNTIL reset ;
: REPEAT      AGAIN THEN reset ;

	 
\ include asm-test.fs

 HERE  SWAP -
 CR .( Length of Assembler: ) . .( Bytes ) CR


