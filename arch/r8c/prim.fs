\ r8c/m16c primitives

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

\ * Register using for r8c
\  Renesas  Forth    used for
\   R0      TOS   oberstes Stackelement
\   R3      RP    Returnstack Pointer
\   SP      SP    Stack Pointer
\   A1      IP    Instruction Pointer
\   A0      W     Arbeitsregister
\
\ * Memory ( use only one 64K-Page ): TBD
\ **************************************************************


start-macros
 \ register definition
  ' R0L Alias tos.b

 \ hfs wichtig, damit der erste Befehl richtig compiliert wird
   reset  \ hfs

 \ system depending macros
  : next1,
      [w] , r1 mov.w:g  r3r1 jmpi.a ;
  : next,
      [ip] , w mov.w:g
      # 2 , ip add.w:q  next1, ;
\ note that this is really for 8086 and 286, and _not_ intented to run
\ fast on a Pentium (Pro). These old chips load their code from real RAM
\ and do it slow, anyway.
\ If you really want to have a fast 16 bit Forth on modern processors,
\ redefine it as
\ : next,  [ip] w mov,  2 # ip add,  [w] jmp, ;

end-macros

  unlock
    $0000 $FFFF region address-space
    $C000 $4000 region rom-dictionary
    $0400 $0400 region ram-dictionary
  .regions
  setup-target
  lock

\ ==============================================================
\ rom starts with jump to GFORTH-kernel (must be at $0000 !!!)
\ ==============================================================
  Label into-forth
    # $ffff , ip mov.w:g            \ ip will be patched
    # $0780 , sp ldc                \ sp at $0600...$0700
    # $0800 , rp mov.w:g            \ rp at $0780...$0800
    # $C084 , intbl ldc
    # $0F , $E3  mov.b:g
    # $0F , $E1  mov.b:g
  Label mem-init
    $01 , $0A bset:g
    $00 , $05 bset:g                \ open data RAM
    $01 , $0A bclr:g
  Label clock-init                  \ default is 125kHz/8
    $00 , $0A  bset:g
    # $2808 , $06  mov.w:g
    AHEAD  THEN
    2 , $0C bclr:g
    # $00 , $08  mov.b:g            \ set to 20MHz
    $00 , $0A  bclr:g
  Label uart-init
    # $27 , $B0  mov.b:g      \ hfs
\    # $8105 , $A8  mov.w:g    \ ser1: 9600 baud, 8N1  \ hfs
\    # $2005 , $A8  mov.w:g    \ ser1: 38k4 baud, 8N1  \ hfs
    # $0500 , $AC  mov.w:g      \ hfs
    I fset
  next,
  End-Label


\ ==============================================================
\ GFORTH minimal primitive set
\ ==============================================================
 \ inner interpreter
  align

  Code: :docol
  \     ': dout,                    \ only for debugging
     # -2 , rp add.w:q
     w , r1 mov.w:g
     rp , w mov.w:g  ip , [w] mov.w:g
     # 4 , r1 add.w:q  r1 , ip mov.w:g
     next,
   End-Code

   align

  Code: :docon
\    '2 dout,                    \ only for debugging
    tos push.w:g
    4 [w] , tos mov.w:g
    next,
  End-Code

  align

  Code: :dovalue
\    '2 dout,                    \ only for debugging
    tos push.w:g
    4 [w] , w mov.w:g  [w] , tos mov.w:g
    next,
  End-Code

  align

  Code: :dofield
      4 [w] , tos add.w:g
      next,
  end-code
  
  align

  Code: :dodefer
\      # $05 , $E1 mov.b:g
     4 [w] , w mov.w:g  [w] , w mov.w:g
     next1,
  End-Code

  align
  
  Code: :dodoes  ( -- pfa ) \ get pfa and execute DOES> part
\    '6 dout,                    \ only for debugging
\      # $06 , $E1 mov.b:g
     tos push.w:g
     w , tos mov.w:g   # 4 , tos add.w:q
     # -2 , rp add.w:q  2 [w] , r1 mov.w:g
     rp , w mov.w:g  ip , [w] mov.w:g
     r1 , ip mov.w:g
     next,                                       \ execute does> part
  End-Code

  $FF $C0FE here - tcallot
  
  Code: :dovar
\    '2 dout,                    \ only for debugging
    tos push.w:g
    # 4 , w add.w:q
    w , tos mov.w:g
    next,
  End-Code

\ program flow
  Code ;s       ( -- ) \ exit colon definition
\    '; dout,                    \ only for debugging
      rp , w mov.w:g  # 2 , rp add.w:q
      [w] , ip mov.w:g
      next,
  End-Code

  Code execute   ( xt -- ) \ execute colon definition
    tos , w mov.w:g                          \ copy tos to w
    tos pop.w:g                              \ get new tos
    next1,
  End-Code

  Code perform   ( xt -- ) \ execute colon definition
    tos , w mov.w:g                          \ copy tos to w
    tos pop.w:g                              \ get new tos
    [w] , w mov.w:g
    next1,
  End-Code

  Code ?branch   ( f -- ) \ jump on f=0
      # 2 , ip add.w:q
      tos , tos tst.w   0= IF  -2 [ip] , ip mov.w:g   THEN
      tos pop.w:g
      next,
  End-Code

  Code (for) ( n -- r:0 r:n )
      # -4 , rp add.w:q  rp , w mov.w:g
      r3 , 2 [w] mov.w:g
      tos , [w] mov.w:g
      tos pop.w:g
      next,
  End-Code
  
  Code (?do) ( n -- r:0 r:n )
      # 2 , ip add.w:q
      # -4 , rp add.w:q  rp , w mov.w:g
      tos , [w] mov.w:g
      r1 pop.w:g
      r1 , 2 [w] mov.w:g
      tos pop.w:g
      [w] , r1 sub.w:g
      0= IF  -2 [ip] , ip mov.w:g   THEN
      next,
  End-Code
  
  Code (do) ( n -- r:0 r:n )
      # -4 , rp add.w:q  rp , w mov.w:g
      tos , [w] mov.w:g
      tos pop.w:g
      tos , 2 [w] mov.w:g
      tos pop.w:g
      next,
  End-Code
  
  Code (next) ( -- )
      # 2 , ip add.w:q
      rp , w mov.w:g  [w] , r1 mov.w:g
      # -1 , r1 add.w:q  r1 , [w] mov.w:g
      u>= IF  -2 [ip] , ip mov.w:g  THEN
      next,
  End-Code

  Code (loop) ( -- )
      # 2 , ip add.w:q
      rp , w mov.w:g  [w] , r1 mov.w:g
      # 1 , r1 add.w:q  r1 , [w] mov.w:g
      2 [w] , r1 sub.w:g
      0<> IF  -2 [ip] , ip mov.w:g  THEN
      next,
  End-Code

  Code (+loop) ( n -- )
      # 2 , ip add.w:q
      rp , w mov.w:g  [w] , r1 mov.w:g
      2 [w] , r1 sub.w:g  # $8000 , r1 xor.w
      tos , r1 add.w:g
      no IF  -2 [ip] , ip mov.w:g  THEN
      tos , [w] add.w:g
      tos pop.w:g
      next,
  End-Code

 \ memory access
  Code @        ( addr -- n ) \ read cell
      tos , w mov.w:g  [w] , tos mov.w:g
      next,
   End-Code

  Code !        ( n addr -- ) \ write cell
      tos , w mov.w:g  tos pop.w:g  tos , [w] mov.w:g
      tos pop.w:g
      next,
   End-Code

  Code +!        ( n addr -- ) \ write cell
      tos , w mov.w:g  tos pop.w:g  tos , [w] add.w:g
      tos pop.w:g
      next,
   End-Code

  Code c@        ( addr -- uc ) \ read cell
      tos , w mov.w:g  tos , tos xor.w  [w] , tos.b mov.b:g
      next,
   End-Code

  Code count     ( addr -- addr+1 uc ) \ read cell
      tos , w mov.w:g  tos , tos xor.w  [w] , tos.b mov.b:g
      # 1 , w add.w:q  w push.w:g
      next,
   End-Code

  Code c!        ( n addr -- ) \ write cell
      tos , w mov.w:g  tos pop.w:g  tos.b , [w] mov.b:g
      tos pop.w:g
      next,
   End-Code

 \ arithmetic and logic
  Code +        ( n1 n2 -- n3 ) \ addition
      r1 pop.w:g
      r1 , tos add.w:g
      next,
  End-Code
  
  Code 2*        ( n1 n2 -- n3 ) \ addition
      tos , tos add.w:g
      next,
  End-Code
  
  Code -        ( n1 n2 -- n3 ) \ addition
      r1 pop.w:g
      tos , r1 sub.w:g
      r1 , tos mov.w:g
      next,
  End-Code

  Code negate ( n1 -- n2 )
      tos neg.w
      next,
  End-Code
  
  Code invert ( n1 -- n2 )
      tos not.w:g
      next,
  End-Code
  
  Code 1+        ( n1 n2 -- n3 ) \ addition
      # 1 , tos add.w:g
      next,
  End-Code
  
  Code 1-        ( n1 n2 -- n3 ) \ addition
      # -1 , tos add.w:g
      next,
  End-Code
  
  Code cell+        ( n1 n2 -- n3 ) \ addition
      # 2 , tos add.w:g
      next,
  End-Code
  
  Code and        ( n1 n2 -- n3 ) \ addition
      r1 pop.w:g
      r1 , tos and.w:g
      next,
  End-Code
  
  Code or       ( n1 n2 -- n3 ) \ addition
      r1 pop.w:g
      r1 , tos or.w:g
      next,
  End-Code
  
  Code xor      ( n1 n2 -- n3 ) \ addition
      r1 pop.w:g
      r1 , tos xor.w
      next,
   End-Code

 \ moving datas between stacks
  Code r>       ( -- n ; R: n -- )
      tos push.w:g
      rp , w mov.w:g
      [w] , tos mov.w:g
      # 2 , rp add.w:q  \ ? hfs
      next,
  End-Code
  
  Code i       ( -- n ; R: n -- )
      tos push.w:g
      rp , w mov.w:g
      [w] , tos mov.w:g
      next,
   End-Code

  Code i'       ( -- n ; R: n -- )
      tos push.w:g
      rp , w mov.w:g
      2 [w] , tos mov.w:g
      next,
   End-Code

  Code j       ( -- n ; R: n -- )
      tos push.w:g
      rp , w mov.w:g
      4 [w] , tos mov.w:g
      next,
   End-Code

  Code k       ( -- n ; R: n -- )
      tos push.w:g
      rp , w mov.w:g
      8 [w] , tos mov.w:g
      next,
   End-Code

   Code >r       ( n -- ; R: -- n )
       # -2 , rp add.w:q  \ ? hfs
       rp , w mov.w:g
       tos , [w] mov.w:g
       tos pop.w:g
       next,
   End-Code

   Code rdrop       ( R:n -- )
      # 2 , rp add.w:q  \ ? hfs
      next,
   End-Code

   Code unloop       ( R:n -- )
      # 4 , rp add.w:q  \ ? hfs
      next,
   End-Code

 \ datastack and returnstack address
  Code sp@      ( -- sp ) \ get stack address
      tos push.w:g
      sp , tos stc
      next,
  End-Code

  Code sp!      ( sp -- ) \ set stack address
      tos , sp ldc
      tos pop.w:g
      next,
  End-Code

  Code rp@      ( -- rp ) \ get returnstack address
    tos push.w:g
    rp , tos mov.w:g
    next,
  End-Code

  Code rp!      ( rp -- ) \ set returnstack address
      tos , rp mov.w:g
      tos pop.w:g
      next,
  End-Code

  Code branch   ( -- ) \ unconditional branch
      [ip] , ip mov.w:g
      next,
   End-Code

  Code lit     ( -- n ) \ inline literal
      tos push.w:g
      [ip] , tos mov.w:g
      # 2 , ip add.w:q
      next,
   End-Code

Code: :doesjump
end-code

\ ==============================================================
\ usefull lowlevel words
\ ==============================================================
 \ word definitions


 \ branch and literal

 \ data stack words
  Code dup      ( n -- n n )
    tos push.w:g
    next,
   End-Code

  Code 2dup     ( d -- d d )
    r1 pop.w:g
    r1 push.w:g
    tos push.w:g
    r1 push.w:g
    next,
   End-Code

  Code drop     ( n -- )
    tos pop.w:g
    next,
   End-Code

  Code 2drop    ( d -- )
    tos pop.w:g
    tos pop.w:g
    next,
   End-Code

  Code swap     ( n1 n2 -- n2 n1 )
    r1 pop.w:g
    tos push.w:g
    r1 , tos mov.w:g
    next,
   End-Code

  Code over     ( n1 n2 -- n1 n2 n1 )
    tos , r1 mov.w:g
    tos pop.w:g
    tos push.w:g
    r1 push.w:g
    next,
   End-Code

  Code rot      ( n1 n2 n3 -- n2 n3 n1 )
    tos , r1 mov.w:g
    r3 pop.w:g
    tos pop.w:g
    r3 push.w:g
    r1 push.w:g
    r3 , r3 xor.w
    next,
   End-Code

  Code -rot     ( n1 n2 n3 -- n3 n1 n2 )
    tos , r1 mov.w:g
    tos pop.w:g
    r3 pop.w:g
    r1 push.w:g
    r3 push.w:g
    r3 , r3 xor.w
    next,
   End-Code


 \ return stack
  Code r@       ( -- n ; R: n -- n )
    tos push.w:g
    rp , w mov.w:g
    [w] , tos mov.w:g
    next,
  End-Code


 \ arithmetic

  Code um*      ( u1 u2 -- ud ) \ unsigned multiply
      rp , r3 mov.w:g
      r2 pop.w:g
      r2 , r2r0 mulu.w:g
      r0 push.w:g
      r2 , tos mov.w:g
      r3 , rp mov.w:g
      r3 , r3 xor.w
      next,
   End-Code

  Code m*      ( u1 u2 -- ud ) \ unsigned multiply
      rp , r3 mov.w:g
      r2 pop.w:g
      r2 , r2r0 mul.w:g
      r0 push.w:g
      r2 , tos mov.w:g
      r3 , rp mov.w:g
      r3 , r3 xor.w
      next,
   End-Code

  Code um/mod   ( ud u -- r q ) \ unsiged divide
      rp , r3 mov.w:g
      tos , r1 mov.w:g
      r2 pop.w:g
      tos pop.w:g
      r3r1 divu.w
      r2 push.w:g
      r3 , rp mov.w:g
      r3 , r3 xor.w
      next,
   End-Code

 \ shift
  Code 2/       ( n1 -- n2 ) \ arithmetic shift right
     # -1 , tos sha.w 
 \    # -1 , r1h mov.b:q
 \    r1h , tos sha.w
     next,
   End-Code

  Code lshift   ( n1 n2 -- n3 ) \ shift n1 left n2 bits
 \     tos.b , r1h mov.w:g
      tos.b , r1h mov.b:g  \ ? hfs
      tos pop.w:g
      r1h , tos shl.w
      next,
   End-Code

  Code rshift   ( n1 n2 -- n3 ) \ shift n1 right n2 bits
\     tos.b , r1h mov.w:g
      tos.b , r1h mov.b:g  \ ? hfs
      r1h neg.b
      tos pop.w:g
      r1h , tos shl.w
     next,
   End-Code

 \ compare
  Code 0=       ( n -- f ) \ Test auf 0
    tos , tos tst.w
    0= IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
    next,
   End-Code

   Code 0<       ( n -- f ) \ Test auf 0
    tos , tos tst.w
    0< IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
    next,
   End-Code

  Code =        ( n1 n2 -- f ) \ Test auf Gleichheit
    r1 pop.w:g
    r1 , tos sub.w:g
    0= IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
   End-Code

   ' = alias u=
   
  Code u<        ( n1 n2 -- f ) \ Test auf Gleichheit
    r1 pop.w:g
    r1 , tos sub.w:g
    u> IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
   End-Code

  Code u>        ( n1 n2 -- f ) \ Test auf Gleichheit
    r1 pop.w:g
    r1 , tos sub.w:g
    u< IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
   End-Code

  Code <        ( n1 n2 -- f ) \ Test auf Gleichheit
    r1 pop.w:g
    r1 , tos sub.w:g
    > IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
   End-Code

  Code >        ( n1 n2 -- f ) \ Test auf Gleichheit
    r1 pop.w:g
    r1 , tos sub.w:g
    < IF  # -1 , tos mov.w:q   next,
    THEN   #  0 , tos mov.w:q   next,
   End-Code

  Code (key)    ( -- char ) \ get character
      tos push.w:g
      BEGIN  3 , $AD  btst:g  0<> UNTIL
      $AE  , tos mov.w:g  r0h , r0h xor.b
    next,
   End-Code

  Code (emit)     ( char -- ) \ output character
      BEGIN  1 , $AD  btst:g  0<> UNTIL
      tos.b , $AA  mov.b:g
      tos pop.w:g
      next,
  End-Code

 \ additon io routines
  Code (key?)     ( -- f ) \ check for read sio character
      tos push.w:g
      3 , $AD  btst:g
      0<> IF  # -1 , tos mov.w:q   next,
      THEN    #  0 , tos mov.w:q   next,
   End-Code

  Code emit?    ( -- f ) \ check for write character to sio
      tos push.w:g
      1 , $AD  btst:g
      0<> IF  # -1 , tos mov.w:q   next,
      THEN    #  0 , tos mov.w:q   next,
   End-Code

   \ String operations

   Code fill ( addr u char -- )
       R3 pop.w:g  ip , r1 mov.w:g  A1 pop.w:g
       sstr.b  tos pop.w:g
       R3 , R3 xor.w  r1 , ip mov.w:g  next,
   End-Code

   Code cmove ( from to count -- )
       tos , R3 mov.w:g  ip , r1 mov.w:g
       a1 pop.w:g  a0 pop.w:g  r1 push.w:g  r1 , r1 xor.w
       smovf.b
       R3 , R3 xor.w  ip pop.w:g  tos pop.w:g next,
   End-Code
   
   Code cmove> ( from to count -- )
       tos , R3 mov.w:g  ip , r1 mov.w:g
       a1 pop.w:g  a0 pop.w:g  r1 push.w:g  r1 , r1 xor.w
       r3 , a0 add.w:g  # -1 , a0 add.w:q
       r3 , a1 add.w:g  # -1 , a1 add.w:q
       smovb.b
       R3 , R3 xor.w  ip pop.w:g  tos pop.w:g next,
   End-Code
   
   Code (find-samelen) ( u f83name1 -- u f83name2/0 )
       tos , w mov.w:g  r0 pop.w:g
       BEGIN  2 [w] , r0h mov.b:g  # $1F , r0h and.b:g
	   r0l , r0h cmp.b:g  0<> WHILE  [w] , w mov.w:g
	   0= UNTIL  THEN
       r0h , r0h xor.b  r0 push.w:g  w , tos mov.w:g
       next,
   End-Code

: capscomp ( c_addr1 u c_addr2 -- n )
 swap bounds
 ?DO  dup c@ I c@ <>
     IF  dup c@ toupper I c@ toupper =
     ELSE  true  THEN  WHILE  1+  LOOP  drop 0
 ELSE  c@ toupper I c@ toupper - unloop  THEN  sgn ;
: sgn ( n -- -1/0/1 )
 dup 0= IF EXIT THEN  0< 2* 1+ ;
       
   Code btst ( b# addr -- f ) \ check for bit set in addr
      tos , w mov.w:g  # 3 , w shl.w
      r1 pop.w:g       r1 , w add.w:g      [w] btst:g
      0<> IF    # -1 , tos mov.w:q   next,
          THEN  #  0 , tos mov.w:q   next,
   End-Code

   Code bset ( b# addr -- ) \ set bit in addr
      tos , w mov.w:g  # 3 , w shl.w
      r1 pop.w:g       r1 , w add.w:g      [w] bset:g
      tos pop.w:g      next,
   End-Code

   Code bclr ( b# addr -- ) \ clr bit in addr
      tos , w mov.w:g  # 3 , w shl.w
      r1 pop.w:g       r1 , w add.w:g      [w] bclr:g
      tos pop.w:g      next,
   End-Code

   Code us ( n -- ) \ n microseconds delay
       BEGIN  AHEAD  THEN  AHEAD  THEN
           r1 , r1 mov.w:g
           # -1 , tos  add.w:q  0= UNTIL
       tos pop.w:g
       next,
   end-code

   Variable timer
   
   Code ms-irq ( -- )
       # 1 , timer add.w:g
       reit
   end-code

   ' ms-irq >body $C084 $40 + ! 0 $C084 $42 + c!

   : timer-init ( -- )
       &19999 $9E !
       $0401 $9A !
       1 $50 c! ;

   : noop ;
   defer pause ' noop is pause
   
   : ms ( n -- )  timer @ +
       BEGIN  pause dup timer @ - 0<  UNTIL  drop ;
   
   $400 constant ram-start
   $2FFC Constant ram-shadow
   0 Constant ram-mirror
   0 Constant ram-size
   $E0 Constant port0
   $E1 Constant port1
   
   : led!  port1 c! ;
   : >lcd ( 4bit -- )
       1+ dup port0 c! dup 8 + port0 c!  1 us  port0 c!
       &40 us ;
   : lcdctrl!  ( n -- )
       dup $F0 and >lcd
       4 lshift >lcd
       &100 us ;
   : lcdemit ( n -- )  &100 us
       dup $F0 and 4 + >lcd
       4 lshift 4 + >lcd
       &250 us ;
   : lcdtype  bounds ?DO  I c@ lcdemit  LOOP ;
   : lcdpage  $01 lcdctrl! &15 ms ;
   : lcdcr    $C0 lcdctrl! ;
   : lcdinit ( -- )
       $02 $0A bset $FD $E2 c!
       &20 ms $30 >lcd  5 ms  $33 lcdctrl! 5 ms $20 >lcd
       &5 ms  $28 lcdctrl!
       &1 ms  $0C lcdctrl!
       &1 ms  lcdpage ;
   \ default channel is channel 6
   : adc@ ( chan -- value )  $80 + $D6 c!  $28 $D7 c!
       6 $D6 bset  BEGIN  6 $D6 btst 0=  UNTIL  $C0 @ ;
   : ?flash  BEGIN  $1B7 c@ 1 and 1 =  UNTIL ;
   : flashc! ( c addr -- )  $40 over c! c! ?flash ;
   : flash! ( x addr -- )  2dup flashc! >r 8 rshift r> 1+ flashc! ;
   : flash-off ( addr -- )  $20 over c! $D0 swap c! ?flash ;
   : flash-enable ( -- )   1 $1b7 c! 3 $1b7 c! 0 $1b5 c! 2 $1b5 c! ;
   : 9k6   $8105 $A8 ! ; \ baud setting
   : 38k4  $2005 $A8 ! ; \ fast terminal
   : r8cboot ( -- ) ['] noop IS pause
       timer-init flash-enable lcdinit 38k4
       s" Gforth EC R8C" lcdtype boot ;
   ' r8cboot >body $C002 !
   : savesystem ( -- )
       dpp @ >r rom here normal-dp @ ram-start tuck - tuck
       here over allot r> dpp ! -rot
       bounds ?DO  I c@ over flashc! 1+  LOOP  drop
       ram-shadow tuck flash! cell+ flash! ;
   : refill-loop ( -- )
       BEGIN  3 emit refill  WHILE  interpret  REPEAT ;   
   : included ( addr u -- )
       2 emit dup $20 + emit type
       echo @ IF
	   echo off ['] refill-loop catch
	   dup IF  4 emit  THEN  echo on  throw
       THEN ;
   : include ( "file" -- )  parse-name included ;
   : empty ( -- )  $2800 flash-off $2000 flash-off
       forth-wordlist ram-mirror + ram-start - @ forth-wordlist !
       normal-dp ram-mirror + ram-start - @ normal-dp ! $2000 flash-dp ! ;

