\ r8c/m16c primitives

\ Copyright (C) 2006 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

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
      [w] , r1 mov.w:g  r1 jmpi.a ;
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
    # $07FE , sp ldc                \ sp at $0700...$07FE
    # $0700 , rp mov.w:g            \ rp at $0600...$0700
    # $0F , $E3  mov.b:g
    # $0F , $E1  mov.b:g
  Label clock-init                  \ default is 125kHz/8
    # $08 , $06  mov.b:g
    # $38 , $07  mov.b:g
    # $00 , $08  mov.b:g            \ set to 20MHz
  Label uart-init
    # $20 , $B0  mov.b:g      \ hfs
    # $8105 , $A8  mov.w:g    \ ser1: 9600 baud, 8N1  \ hfs
    # $05 , $AD  mov.b:g      \ hfs
  End-Label


\ ==============================================================
\ GFORTH minimal primitive set
\ ==============================================================
 \ inner interpreter
  Code: :docol
  \     ': dout,                    \ only for debugging
     # -2 , rp add.w:q
     w , r1 mov.w:g
     rp , w mov.w:g  ip , [w] mov.w:g
     # 4 , r1 add.w:q  r1 , ip mov.w:g
     next,
   End-Code

  Code: :dovar
\    '2 dout,                    \ only for debugging
    tos push.w:g
    # 4 , w add.w:q
    w , tos mov.w:g
    next,
  End-Code

  Code: :docon
\    '2 dout,                    \ only for debugging
    tos push.w:g
    # 4 , w add.w:q  [w] , tos mov.w:g
    next,
  End-Code

  Code: :dovalue
\    '2 dout,                    \ only for debugging
    tos push.w:g
    # 4 , w add.w:q   [w] , w mov.w:g  [w] , tos mov.w:g
    next,
  End-Code

  Code: :dodefer
      # $05 , $E1 mov.b:g
     # 4 , w add.w:q  [w] , w mov.w:g  [w] , w mov.w:g
     next1,
  End-Code

  Code: :dodoes  ( -- pfa ) \ get pfa and execute DOES> part
\    '6 dout,                    \ only for debugging
\      # $06 , $E1 mov.b:g
     tos push.w:g
     w , tos mov.w:g   # 4 , tos add.w:q
     # -2 , rp add.w:q
     # 2 , w add.w:q   [w] , r1 mov.w:g
     rp , w mov.w:g  ip , [w] mov.w:g
     # 4 , r1 add.w:q  r1 , ip mov.w:g
     next,                                       \ execute does> part
  End-Code
  
 \ program flow
  Code ;s       ( -- ) \ exit colon definition
\    '; dout,                    \ only for debugging
      rp , w mov.w:g  # 2 , rp add.w:q
      [w] , ip mov.w:g
      next,
  End-Code

  Code execute   ( xt -- ) \ execute colon definition
\    'E dout,                    \ only for debugging
    # $07 , $E1 mov.b:g
    tos , w mov.w:g                          \ copy tos to w
    tos pop.w:g                              \ get new tos
    next1,
  End-Code

  Code ?branch   ( f -- ) \ jump on f<>0
      # 2 , ip add.w:q
      tos , tos tst.w   0<> IF  -2 [ip] , ip mov.w:g   THEN
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

  Code c@        ( addr -- n ) \ read cell
      tos , w mov.w:g  tos , tos xor.w  [w] , tos.b mov.b:g
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
  
  Code -        ( n1 n2 -- n3 ) \ addition
      r1 pop.w:g
      tos , r1 sub.w:g
      r1 , tos mov.w:g
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

   Code >r       ( n -- ; R: -- n )
       # -2 , rp add.w:q  \ ? hfs
       rp , w mov.w:g
       tos , [w] mov.w:g
       tos pop.w:g
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
      r2 , r0 mulu.w:g
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
      r1 divu.w
      r2 push.w:g
      r3 , rp mov.w:g
      r3 , r3 xor.w
      next,
   End-Code

 \ shift
  Code 2/       ( n1 -- n2 ) \ arithmetic shift right
 \ hfs geht noch nicht !!!     # -1 , tos sha.w 
     # -1 , r1h mov.b:q
     r1h , tos sha.w
     next,
   End-Code

0 [IF]
  Code lshift   ( n1 n2 -- n3 ) \ shift n1 left n2 bits
 \     tos.b , r1h mov.w:g
     tos.b , r1h mov.b:g  \ ? hfs
     r1h , tos shl.w
     next,
   End-Code

  Code rshift   ( n1 n2 -- n3 ) \ shift n1 right n2 bits
\     tos.b , r1h mov.w:g
     tos.b , r1h mov.b:g  \ ? hfs
     r1h neg.b
     r1h , tos shl.w
     next,
   End-Code
[THEN]

 \ compare
  Code 0=       ( n -- f ) \ Test auf 0
    tos , tos tst.w
    0= IF  # -1 , tos mov.w:g   next,
    THEN   # 0  , tos mov.w:g   next,
    next,
   End-Code

  Code =        ( n1 n2 -- f ) \ Test auf Gleichheit
    r1 pop.w:g
    r1 , tos sub.w:g
    0= IF  # -1 , tos mov.w:g   next,
    THEN   # 0  , tos mov.w:g   next,
   End-Code

  Code (key)    ( -- char ) \ get character
    # $08 , $E1 mov.b:g
      tos push.w:g
\      BEGIN  # $08 , $AD abs:16 tst.b  0<> UNTIL
      BEGIN  # $08 , $AD  tst.b  0<> UNTIL
      tos , tos xor.w
\      $AE abs:16 , tos.b mov.b:g
      $AE  , tos.b mov.b:g
    next,
   End-Code

  Code (emit)     ( char -- ) \ output character
    # $09 , $E1 mov.b:g
\      BEGIN  # $02 , $AD abs:16 tst.b  0= UNTIL
\      tos.b , $AA abs:16 mov.b:g
      BEGIN  # $02 , $AD  tst.b  0= UNTIL
      tos.b , $AA  mov.b:g
      tos pop.w:g
    # $0A , $E1 mov.b:g
      next,
  End-Code

 \ additon io routines
  Code (key?)     ( -- f ) \ check for read sio character
      tos push.w:g
\      # $08 , $AD abs:16 tst.b
      # $08 , $AD  tst.b
      0<> IF  # -1 , tos mov.w:g   next,
      THEN   # 0  , tos mov.w:g   next,
   End-Code

  Code emit?    ( -- f ) \ check for write character to sio
      tos push.w:g
\      # $02 , $AD abs:16 tst.b
      # $02 , $AD  tst.b
      0= IF  # -1 , tos mov.w:g   next,
      THEN   # 0  , tos mov.w:g   next,
   End-Code

[then]
: (bye)     ( 0 -- ) \ back to DOS
    drop ;

: bye ( -- )  0 (bye) ;
    
: compile-prim1 ;
: finish-code ;
: x@+/string ( addr u -- addr' u' c )
    over c@ >r 1 /string r> ;
