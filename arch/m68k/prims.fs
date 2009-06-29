\ m68k primitives

\ Copyright (C) 2009 Free Software Foundation, Inc.

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

\ INFO: Register using for this  68K-FORTH
\ 
\   Processor:            68xxx (here  MCF52223)
\ 
\                 Register allocation
\   A3 : FP : Base address of FORTH
\   A4 : IP : Instruction pointer  ( IP-Register )
\   A6 : SP : Data Stack pointer   ( SP-Register )
\   A5 : RP : Return Stack pointer ( RP-Register )
\ 
\   D6 : AUXiliary register 
\   D7 : WorkingAddress register WA ( Contains CFA of topical word )
\ 
\ ******************************************************************


start-macros
\ ??? ===>
\  \ register definition
\   ' R0L Alias tos.b
\ 
\  \ hfs wichtig, damit der erste Befehl richtig compiliert wird
\    reset  \ hfs \ ???
\ <=== ???

 \ system depending macros
  Code Next,
      .l IP )+ WA move,        \ [IP] -> WA  (ip+cell -> ip)
      WA reg) AUX move,        \ [WA] -> AUX (cfa)
      AUX reg) jmp,            \ jmp [cfa]
   End-Code
end-macros
\ 
\   
\ ??? ===>
\ 
\   unlock
\     $0000 $FFFF region address-space
\     $C000 $4000 region rom-dictionary
\     $0400 $0400 region ram-dictionary
\   .regions
\   setup-target
\   lock
\ 
\ \ ==============================================================
\ rom starts with jump to GFORTH-kernel (must be at $0000 !!!)
\ ==============================================================
\   Label into-forth
\     # $ffff , ip mov.w:g            \ ip will be patched
\     # $0780 , sp ldc                \ sp at $0600...$0700
\     # $0800 , rp mov.w:g            \ rp at $0780...$0800
\     # $C084 , intbl ldc
\     # $0F , $E3  mov.b:g
\     # $0F , $E1  mov.b:g
\   Label mem-init
\     $01 , $0A bset:g
\     $00 , $05 bset:g                \ open data RAM
\     $01 , $0A bclr:g
\   Label clock-init                  \ default is 125kHz/8
\     $00 , $0A  bset:g
\     # $2808 , $06  mov.w:g
\     AHEAD  THEN
\     2 , $0C bclr:g
\     # $00 , $08  mov.b:g            \ set to 20MHz
\     $00 , $0A  bclr:g
\   Label uart-init
\     # $27 , $B0  mov.b:g      \ hfs
\ \    # $8105 , $A8  mov.w:g    \ ser1: 9600 baud, 8N1  \ hfs
\ \    # $2005 , $A8  mov.w:g    \ ser1: 38k4 baud, 8N1  \ hfs
\     # $0500 , $AC  mov.w:g      \ hfs
\     I fset
\   next,
\   End-Label
\ 
\ <=== ???


\ ==============================================================
\ GFORTH minimal primitive set
\ ==============================================================
\ inner interpreter
\ -----------------
  
  align

  Code: :docol      ( -- ; R: -- addr ) ( assembler routine of : )
    .l FP IP suba,
    .l IP RP -) move,
    .l 2 WA FP DI) IP lea,
    .l Next,
   End-Code

   align

  Code: :docon       ( -- n )    ( assembler routine of CONSTANT )
    .l CELL WA FP DI) .l SP -) move,      \ value to stack
    Next,
   End-Code

  align

\   Code: :dovalue
\  \    '2 dout,                    \ only for debugging
\     tos push.w:g
\     4 [w] , w mov.w:g  [w] , tos mov.w:g
\     next,
\   End-Code
\
\   align
\
\   Code: :dofield
\       4 [w] , tos add.w:g
\       next,
\   end-code
  
  align

  Code: :dodefer     ( ??? -- ??? ) ( assembler routine of DEFER )
    .l CELL WA FP DI) .l WA move,                \ offset
    p_vdp FP D) WA add,                          \ add to VDP
    WA reg)  WA move,                            \ get CFA
    WA reg) AUX move,                            \ AUX with cfa@
    .l AUX reg) jmp, .l                          \ jump to cfa@
   End-Code

  align
  
   Code: :dodoes  ( -- pfa ) \ get pfa and execute DOES> part
\ ??? ===>
                  ( addr -- ; R: -- addr2 ) ( code part of DOES> )
     .l FP IP suba,
     .l IP RP -) move,
     A7 )+ IP lmove,
     CELL WA addq,
     WA SP -) move,
\ <=== ???
     Next,                                       \ execute does> part
  End-Code

  $FF $C0FE here - tcallot
  
  Code: :dovar       ( -- addr ) ( assembler routine of VARIABLE )
    .l CELL WA FP di) .l WA move,                \ offset
    p_vdp FP d) WA add,                          \ add to VDP
    Wa SP -) move,
    Next,
  End-Code

  
\ program flow
\ ------------
  Code ;s       ( -- ) \ exit colon definition
\      (exit    ( -- ; R: addr -- )
      RP )+ WA move,
      .l WA IP move, .l
      .l FP IP adda,
      .l   Next,
  End-Code

  Code execute   ( xt -- ) \ execute colon definition
\                ( cfa -- )    ( get CFA and execute )
      SP )+ WA move,                  \ copy TOS to WA
      WA reg) AUX move,
      .l AUX reg) jmp, .l
  End-Code

  Code perform   ( xt -- ) \ execute colon definition
\                ( addr -- )  ( get ADDR and execute )
      SP )+ WA move,                  \ copy TOS to WA
      WA reg)  WA move,
      WA reg) AUX move,
      .l AUX reg) jmp, .l
   End-Code

  Code  (branch
      IP ) IP adda,
      Next,
   End-Code

  Code ?branch   ( f -- ) \ jump on f=0
      SP )+ tst,
      (branch beq,
      CELL IP addq,
      Next,
   End-Code

\   Code (for) ( n -- r:0 r:n )
\       # -4 , rp add.w:q  rp , w mov.w:g
\       r3 , 2 [w] mov.w:g
\       tos , [w] mov.w:g
\       tos pop.w:g
\       next,
\    End-Code

  Code ((do
      CELL IP addq,                        \ inner loop
      .l IP D1 move,
      FP D1 sub,
      .l D1 RP -) move,                    \ return jump
      SP )+ RP -) move,
      D0 RP -) move,
      Next,
   End-Code

  Code (?do) ( n -- r:0 r:n )
       ( limit start -- ) ( DO runtime routine )
      SP )+ D0 move,
      SP ) D0 sub,
      ((do bne,           \ difference
      CELL SP addq,
      IP ) IP adda,
      Next,
   End-Code
  
  Code (do) ( n -- r:0 r:n )
       ( limit start -- ) ( DO runtime routine )
      SP )+ D0 move,
      SP ) D0 sub,        \ difference
   End-Code

  
  Code (next) ( -- )
\ ??? ===>
      $4000        bsr,              ( !T!: Trace-Routinen )
      .l IP )+ WA move,
      Wa reg) AUX move,
      AUX reg)     jmp,
\ <=== ???
\       # 2 , ip add.w:q
\       rp , w mov.w:g  [w] , r1 mov.w:g
\       # -1 , r1 add.w:q  r1 , [w] mov.w:g
\       u>= IF  -2 [ip] , ip mov.w:g  THEN
\       next,
  End-Code

  Code (loop) ( -- )
      1 RP ) addq,
      cc IF,
        4 RP d) AUX move,
        AUX reg) IP lea,
        Next,
      THEN,
      6 RP addq,
      Next,
  End-Code

  Code (+loop) ( n -- )
      SP )+ D0 move,
      D0 D1 move,
      D0 RP ) add,
      1 # d1 roxr,
      D0 D1 eor,
      0>= IF,
        4 RP d) AUX move,
        AUX reg) IP lea,
        Next,
      THEN,
      6 RP addq,
      Next,
  End-Code


  
\ memory access
\ -------------
  
  Code @        ( addr -- n ) \ "store" read cell
      SP )+ AUX move,   AUX reg) A0 lea,
      .l A0 ) SP -) move,
      Next,
   End-Code

  Code !        ( n addr -- ) \ "fetch" write cell
      SP )+ AUX move,   AUX reg) A0 lea,
      .l SP )+ A0 )+ move,
      Next,
   End-Code

  Code +!        ( n addr -- ) \ "plus-store" write cell
\ ??? ===>
      SP )+ AUX move,
      AUX reg) A0 lea,
      CELL A0 addq,
      CELL SP addq,
      4 # move>ccr,
      .b SP -) A0 -) addx,  \ ???
      SP -) A0 -) addx,
      .l CELL SP addq,
      Next,
\ <=== ???
   End-Code

  Code count     ( addr -- addr+1 len )
\ ??? ===>
    SP ) AUX move,   AUX reg) A0 lea,
    D0 clr,  .b A0 )+ D0 move,  .l 1 SP ) addq,  D0 SP -) move,
    Next,
\ <=== ???
   End-Code

  Code c@        ( addr -- uc ) \ "char-fetch" read byte
      SP )+ AUX move,   AUX reg) A0 lea,
      0 D0 moveq,     .b A0 ) D0 move,
      .l D0 SP -) move,
      Next,
   End-Code

  Code c!        ( n addr -- ) \ "char-store" write byte
      SP )+ AUX move,   AUX reg) A0 lea,
      SP )+ D0 move,   .b D0 A0 ) move,
      Next,
   End-Code

  Code w@        ( addr -- uw ) \ "word-fetch" read word
      SP )+ AUX move,   AUX reg) A0 lea,
      0 D0 moveq,     .w A0 ) D0 move,
      .l D0 SP -) move,
      Next,
   End-Code

  Code wc!        ( n addr -- ) \ "word-store" write word
      SP )+ AUX move,   AUX reg) A0 lea,
      .w SP )+ A0 )+ move,
      Next,
   End-Code


   
\ arithmetic and logic
\ --------------------
  Code +        ( n1 n2 -- n3 ) \ addition
      SP )+ D0 move,   D0 SP ) add,
      Next,
  End-Code
  
  Code CELLS     ( n1 n2 -- n3 )
      SP ) asl,
      Next,
  End-Code
  
  Code 2*        ( n1 n2 -- n3 )
      SP ) asl,
      Next,
  End-Code
  
  Code -        ( n1 n2 -- n3 ) \ subtraction
      SP )+ D0 move,   D0 SP ) sub,
      Next,
  End-Code

  Code negate ( n1 -- n2 )
      SP ) neg,
      Next,
  End-Code
  
  Code invert ( n1 -- n2 ) \ logical complement
      SP ) not,
      Next,
  End-Code
  
  Code 1+        ( n1 n2 -- n3 )
      1 SP ) addq,
      Next,
  End-Code
  
  Code 1-        ( n1 n2 -- n3 )
      1 SP ) subq,
      Next,
  End-Code
  
  Code cell+        ( n1 n2 -- n3 ) \ addition
      CELL SP ) addq,
      Next,
  End-Code
  
  Code and        ( n1 n2 -- n3 )
      SP )+ D0 move,   D0 SP ) and,    Next,
  End-Code
  
  Code or       ( n1 n2 -- n3 )
    SP )+ D0 move,   D0 SP ) or,    Next,
  End-Code
  
  Code xor      ( n1 n2 -- n3 )
    SP )+ D0 move,   D0 SP ) eor,   Next,
   End-Code



\ moving datas between stacks
\ ---------------------------
  Code r>       ( -- n ; R: n -- )
      RP )+ SP -) move,
      Next,
  End-Code
  
  Code >r       ( n -- ; R: -- n )
       SP )+ RP -) move,
       Next,
   End-Code

  Code rdrop       ( R:n -- )
       CELL RP addq,
       Next,
   End-Code


  
  Code i       ( -- n ; R: n -- )
      RP ) D0 move,   2 RP d) D0 add,   D0 SP -) move,
      Next,
   End-Code

  Code i'       ( -- n ; R: n -- )
      2 RP d) SP -) move,
      Next,
   End-Code

  Code j       ( -- n ; R: n -- )
      6 RP d) D0 move,   8 RP d) D0 add,   D0 SP -) move,
      Next,
   End-Code

\  Code k       ( -- n ; R: n -- )
\   End-Code

  Code unloop   ( -- ) ( R: addr x y -- )
      6 RP addq,
      Next,
   End-Code


   
\ datastack and returnstack address
\ ---------------------------------   
   Code sp@      ( -- sp ) \ get stack address
       .l SP AUX move,
       FP AUX sub,
       .l AUX SP -) move,
       Next,
  End-Code

  Code sp!      ( sp -- ) \ set stack address
      SP )+ AUX move,
      $fffe AUX andi,  \ ???
      AUX reg) SP lea,
      Next,
  End-Code

  Code rp@      ( -- rp ) \ get returnstack address
      RP ) SP -) move,
      Next,
  End-Code

  Code rp!      ( rp -- ) \ set returnstack address
      SP )+ AUX move,
      $fffe AUX andi,  \ ???
      AUX reg) RP lea,
      Next,
  End-Code


  Code branch   ( -- ) \ unconditional branch
      IP ) IP adda,
      Next,
   End-Code

\  Code lit     ( -- n ) \ inline literal
\   End-Code


   
Code: :doesjump
end-code
