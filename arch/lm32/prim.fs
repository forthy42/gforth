\ prims.fs	LM32 primitives (for use with Milkymist SoC)
\
\ Copyright (C) 2012 Free Software Foundation, Inc.

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

\ Author: David Kühling
\ Created: Feb 2012


start-macros

' sp alias frp
' r1 alias fsp   \ = C-ABI argument0 : ABI-Code compatible!
' r24 alias tos  \ use last 3 callee-saved register for VM state
' r25 alias fip
' r27 alias wa    \ use 'fp', spare 'gp'

$e0000000 equ csr-uart-rxtx
$e0000008 equ csr-uart-stat

: next,
   wa  fip 0  lw,
   r2  wa 0  lw,
   fip  fip 4  addi,
   r2  b, ;

end-macros

\ unlock
\    $40000000   $40000 region dictionary
\    setup-target
\ lock

label into-forth
   r0 r0 r0  xor,         \ r0 needs to be zeroed manually for mvhi, to work!
   fip     $0000  mvhi,   \ patched later
   fip fip $0000  ori,    \ patched later   
   IM  r0  wcsr,      \ disable IRQs for now

   \ uncomment to get an immediate live signal
   \ r2  '~  mvi,
   \ r3  csr-uart-rxtx li,
   \ r3 0  r2 sw,
   \ begin,
   \ r4  r3 8 lw,
   \ r4  r4  1 andi,
   \ r4 r0 ?<> until,

   fsp  $40041800  li,
   frp  $40042000  li,
   
   next,
end-label

label dout   \ print character in r20.  clobber r20,r21,r22
   r21  csr-uart-rxtx  li,
   begin,
      r22  r21 8  lw,
      r22  r22 1  andi,
   r22 r0 ?<> until,
   r21 0  r20  sw, 
   ret,
end-label

start-macros

VARIABLE demit?  demit? off
: demit,  ( char -- )
   demit? @ IF
      r20  ROT ( char)  mvi,  dout  calli,
   ELSE
      DROP
   THEN ;


end-macros

\ Minimal Gforth primitive set

CODE: :docol
   ': demit,
   frp -4  fip sw,
   fip  wa 8  addi,
   frp  frp -4  addi,
   next,
END-CODE

CODE: :dovar
   fsp -4  tos  sw,
   tos  wa 8  addi,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE: :dodoes
   '> demit,
   frp -4  fip sw,	\ save IP
   fip  wa 4  lw,	\ load does> address
   fsp -4  tos  sw,	\ push pfa
   tos  wa 8  addi,
   frp  frp -4  addi,
   fsp  fsp -4  addi,      
   next,
END-CODE

\ could we do without these:
CODE: :docon
   fsp  -4 tos  sw,
   tos  wa 8  lw,
   fsp  fsp -4  addi,    
   next,
END-CODE


CODE: :dodefer
   wa  wa 8  lw,	\ like "next", but with cfa fetched from data field
   r2  wa 0  lw,
   r2  b, 
END-CODE


CODE branch
   'B demit,
   fip  fip 0  lw,
   next,   
END-CODE

CODE lit
   '# demit,
   fsp -4  tos  sw,
   tos  fip 0  lw,
   wa  fip 4  lw,		\ optimized instance of "next":
   fsp  fsp -4  addi,   
   r2  wa 0  lw,		\ fip update folded into next.
   fip  fip 8  addi,
   r2  b, 
END-CODE

CODE dup      ( n -- n n )
   'd demit,      
   fsp -4  tos  sw,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE 2dup      ( d -- d d )
   r2  fsp 0  lw,   \ r2=nos
   fsp -4  tos  sw,
   fsp -8  r2  sw,
   fsp  fsp -8  addi,
   next,
END-CODE

CODE drop      ( n -- )
   '. demit,   
   tos  fsp 0  lw,
   fsp  fsp 4  addi,
   next,
END-CODE

CODE nip      ( n -- )
   fsp  fsp 4  addi,
   next,
END-CODE

CODE 2drop      ( n -- )
   '| demit,
   tos  fsp 4  lw,
   fsp  fsp 8  addi,
   next,
END-CODE

CODE swap      ( x1 x2 -- x2 x1 )
   r2  fsp 0  lw,	\ load NOS
   fsp 0  tos  sw,
   tos  r2  mv,
   next,
END-CODE

CODE over      ( x1 x2 -- x1 x2 x1 )
   fsp -4  tos  sw,
   tos  fsp 0  lw,      \ load NOS into new TOS
   fsp  fsp -4  addi,
   next,
END-CODE

CODE rot      ( x1 x2 x3 -- x2 x3 x1 )
   r2  fsp 0  lw,	\ load NOS
   r3  fsp 4  lw,	\ load NNOS
   fsp 0  tos  sw,
   fsp 4  r2   sw,
   tos  r3  mv,
   next,
END-CODE

CODE -rot      ( x1 x2 x3 -- x3 x1 x2  )
   r2  fsp 0  lw,	\ load NOS
   r3  fsp 4  lw,	\ load NNOS
   fsp 0  r3  sw,
   fsp 4  tos  sw,
   tos  r2  mv,
   next,
END-CODE

 \ return stack
CODE i       ( -- x   R: x -- x )  ( aliased to r@ in basics.fs )
   fsp -4  tos  sw,
   tos  frp 0  lw,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE i'       ( -- x1   R: x1 x2 -- x1 x2 )
   fsp -4  tos  sw,
   tos  frp 4  lw,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE j       ( -- x1   R: x1 x2 x3 -- x1 x2 x3 )
   fsp -4  tos  sw,
   tos  frp 8  lw,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE rdrop       ( R: x -- )
   frp  frp 4  addi,   
   next,
END-CODE

CODE 2rdrop       ( R: x -- )
   frp  frp 8  addi,   
   next,
END-CODE

CODE r>       ( -- x   R: x -- )
   's demit,      
   fsp -4  tos  sw,
   tos  frp 0  lw,
   fsp  fsp -4  addi,
   frp  frp 4  addi,   
   next,
END-CODE

CODE >r       ( x --   R: -- x )
   'r demit,   
   frp -4  tos sw,
   tos  fsp 0  lw,
   fsp  fsp 4  addi,
   frp  frp -4  addi,   
   next,
END-CODE

CODE 2r>       ( -- x1 x2   R: x1 x2 -- )
   fsp -4  tos  sw,
   r2  frp 4  lw,   
   tos  frp 0  lw,
   fsp -8  r2  sw,
   fsp  fsp -8  addi,
   frp  frp 8  addi,   
   next,
END-CODE

CODE 2>r       ( x1 x2 --   R: -- x1 x2 )
   r2  fsp 0  lw,
   frp -8  tos sw,
   r2  fsp 0  lw,   
   tos  fsp 4  lw,
   fsp  fsp 8  addi,
   frp -4  r2  sw,
   frp  frp -8  addi,   
   next,
END-CODE


\ arithmetic
CODE -       ( n1 n2 -- n3 )
   r2  fsp 0  lw,
   tos  r2 tos  sub,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE negate       ( n1 -- n2 )
   tos  r0 tos  sub,
   next,
END-CODE

CODE +       ( n1 n2 -- n3 )
   r2  fsp 0  lw,
   tos  r2 tos  add,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE 1+       ( n1 -- n2 )
   tos  tos 1  addi,
   next,
END-CODE

CODE 1-       ( n1 -- n2 )
   tos  tos -1  addi,
   next,
END-CODE

CODE cell+       ( n1 -- n2 )
   tos  tos 4  addi,
   next,
END-CODE

CODE cells       ( n1 -- n2 )
   tos  tos 2  sli,
   next,
END-CODE

CODE *       ( n1 n2  -- n3 )  \ no easy way to implement (u)m* on lm32 :(
   r2  fsp 0  lw,
   tos  r2 tos  mul,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE um*       ( u1 u2  -- ud3 )  
   r2  fsp 0  lw,		\ tos=u2, r2=u1
   r3  r2 $ffff  andi,		\ r3=u2.l, r2=u2.h
   r2  r2 16  srui,		
   r4  tos $ffff  andi,		\ r4=u1.l, r5=u4.h
   r5  tos 16  srui,
   
   r6  r3 r4  mul,		\ r6=l*l  (ud3.l)
   r7  r2 r5  mul,		\ r7=h*l
   r8  r3 r5  mul,		\ r8,r9=l*h,h*l
   r9  r4 r2  mul,
   
   r8  r9 r8  add,		\ r8=l*h+h*l
   r9  r9 r8  cmpgu,		\ r9=carry
   r9  r9 16  sli,		\ add carry to h*h in ud3.h=tos
   tos  r9 r7  add,

   r9  r8 16  srui,		\ add (high part of) l*h+h*l to ud3.h
   tos  tos r9 add,

   r9  r8 16  sli,		\ add low part of) l*h+h*l to ud3.l
   r6  r9 r6  add,
   r9  r9 r6  cmpgu,		\ add carry to ud3.h
   tos  tos r9  add,		

   fsp 0  r6  sw,		\ store r6=ud3.l=nos
   next,
END-CODE

CODE D+   ( d1 d2 -- d3 )
   r2  fsp 0  lw,		\ tos = d2.h, r2 = d2.l
   r3  fsp 4  lw,		\ r3  = d1.h, r4 = d1.l
   r4  fsp 8  lw,
   fsp  fsp 8  addi,
   r2  r4 r2  add,		\ add low, r2 = d3.l
   r4  r4 r2  cmpgu,		\ r4 = carry
   tos  r3 tos  add,		\ add high, tos = d3.h
   tos  tos r4  add,		\ add carry
   fsp 0  r2  sw,		\ store r2=nos
   next,
END-CODE

CODE D-   ( d1 d2 -- d3 )
   r2  fsp 0  lw,		\ tos = d2.h, r2 = d2.l
   r3  fsp 4  lw,		\ r3  = d1.h, r4 = d1.l
   r4  fsp 8  lw,
   fsp  fsp 8  addi,
   r5  r4 r2  sub,		\ subtract low, r5 = d3.l
   r4  r2 r4  cmpgu,		\ r4 = borrow
   tos  r3 tos  sub,		\ subtract high, tos = d3.h
   tos  tos r4  sub,		\ subtract borrow
   fsp 0  r5  sw,		\ store r2=nos   
   next,
END-CODE

CODE 2*       ( u1 -- u2 )
   tos  tos 1  sli,
   next,
END-CODE

CODE 2/       ( n1 -- n2 )
   tos  tos 1  sri,
   next,
END-CODE

\ logic
CODE invert       ( x1 -- x2 )
   tos  tos r0  xnor,
   next,
END-CODE

CODE and       ( x1 x2 -- x3 )
   'a demit,      
   r2  fsp 0  lw,
   tos  r2 tos  and,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE or       ( x1 x2 -- x3 )
   r2  fsp 0  lw,
   tos  r2 tos  or,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE xor       ( x1 x2 -- x3 )
   r2  fsp 0  lw,
   tos  r2 tos  xor,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE lshift       ( u1 u2  -- u3 )
   r2  fsp 0  lw,
   tos  r2 tos  sl,
   fsp  fsp 4 addi,
   next,
END-CODE

CODE rshift       ( u1 u2  -- u3 )
   r2  fsp 0  lw,
   tos  r2 tos  sru,
   fsp  fsp 4 addi,
   next,
END-CODE


\ comparison
CODE 0=       ( x1  -- flag )
   tos  tos r0  cmpne,
   tos  tos -1 addi,
   next,
END-CODE

CODE 0<>       ( x1  -- flag )
   tos  tos r0  cmpe,
   tos  tos -1 addi,
   next,
END-CODE

CODE 0<       ( x1  -- flag )
   tos  tos 31 sri,
   next,
END-CODE

CODE 0>       ( x1  -- flag )
   tos  r0 tos  cmpge,
   tos  tos -1  addi,
   next,
END-CODE

CODE =       ( x1 x2 -- flag )
   r2  fsp 0  lw,
   tos  r2 tos  cmpne,
   fsp  fsp 4 addi,
   tos  tos -1  addi,
   next,
END-CODE

CODE <>       ( x1 x2 -- flag )
   r2  fsp 0  lw,
   tos  r2 tos  cmpe,
   fsp  fsp 4 addi,
   tos  tos -1  addi,
   next,
END-CODE

CODE <       ( n1 n2 -- flag )
   r2  fsp 0  lw,
   tos  r2 tos  cmpge,
   fsp  fsp 4 addi,
   tos  tos -1  addi,
   next,
END-CODE

CODE u<       ( u1 u2 -- flag )
   r2  fsp 0  lw,
   tos  r2 tos  cmpgeu,
   fsp  fsp 4 addi,
   tos  tos -1  addi,
   next,
END-CODE

CODE >       ( n1 n2 -- flag )
   r2  fsp 0  lw,
   tos  tos r2  cmpge,
   fsp  fsp 4 addi,
   tos  tos -1  addi,
   next,
END-CODE

CODE u>       ( u1 u2 -- flag )
   r2  fsp 0  lw,
   tos  tos r2  cmpgeu,
   fsp  fsp 4 addi,
   tos  tos -1  addi,
   next,
END-CODE

\ flow control
CODE ;s  ( R: a-addr -- )
   '; demit,         
   fip  frp 0  lw,
   frp  frp 4  addi,
   next,
END-CODE
 
CODE execute  ( xt -- )
   wa  tos  mv,		\ like "next", but with cfa fetched via tos
   r2  wa 0  lw,
   tos  fsp 0  lw,
   fsp  fsp 4 addi,
   r2  b, 
END-CODE

CODE ?branch   ( flag -- ) \ jump on false
   '? demit,   
   fip  fip 4  addi,
   tos r0 ?= if,
      fip  fip -4  lw,
   then,
   tos  fsp 0  lw,
   fsp  fsp 4  addi,
   next,   
END-CODE

CODE unloop   ( r: n1 n2 -- )
   frp  frp 8  addi,
   next,
END-CODE

CODE (loop)   ( r: n1 n2 -- n1 n3 )
   r2  frp 0  lw,		\ r2=i
   r3  frp 4  lw,		\ r3=limit
   r2  r2  1  addi,
   fip  fip 4  addi,
   r2 r3 ?<> if,
      fip  fip -4  lw,
   then,
   frp 0  r2  sw,
   next,
END-CODE

\ memory access
CODE @       ( a-addr -- x )
   '@ demit,   
   tos  tos 0  lw,
   next,
END-CODE

CODE c@       ( a-addr -- c )
   'c demit,      
   tos  tos 0  lbu,
   next,
END-CODE

CODE count       ( a-addr -- a-addr+1 c )
   r2   tos 1  addi,
   fsp -4  r2  sw,
   tos  tos 0  lbu,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE !       ( x a-addr -- )
   '! demit,      
   r2  fsp 0  lw,
   tos 0  r2  sw,
   tos  fsp 4  lw,
   fsp  fsp 8  addi,
   next,
END-CODE

CODE c!       ( c a-addr -- )
   r2  fsp 0  lw,
   tos 0  r2  sb,
   tos  fsp 4  lw,
   fsp  fsp 8  addi,
   next,
END-CODE


\ stack addresses (vm registers)
CODE sp@  ( -- sp ) 
   \ according to kernel/prim.fs 'sp@ cell+ @' must equal 'over'
   '$ demit,
   fsp -4  tos  sw,
   tos  fsp -4  addi,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE sp!      ( sp -- ) \ needs to conform to code above
   '& demit,   
   fsp  tos 4  addi,
   tos  fsp -4  lw,
   next,
END-CODE

CODE rp@  ( -- rp )
   '% demit,   
   fsp -4  tos  sw,
   tos  frp  mv,
   fsp  fsp -4  addi,
   next,
END-CODE

CODE rp!      ( sp -- ) \ needs to conform to code above
   frp  tos  mv,
   tos  fsp 0  lw,
   fsp fsp  4 addi,
   next,
END-CODE

\ some words to access CSR registers
UNLOCK >TARGET
: csr@:  ( n "name" -- )  \ generate csr-fetching word for CSR #n
   >R
   X CODE  [ ALSO ASSEMBLER ]
   fsp -4  tos  sw,   tos  R> #csr  rcsr,   fsp  fsp -4  addi,
   next,
   X END-CODE [ PREVIOUS ] ;
: csr!:  ( n "name" -- )  \ generate csr-writing word for CSR #n
   >R
   X CODE  [ ALSO ASSEMBLER ]
   R> #csr  tos  wcsr,   tos  fsp 0  lw,   fsp fsp  4 addi,
   next,
   X END-CODE [ PREVIOUS ] ;
: csr@s:  DO  I csr@: LOOP ;
: csr!s:  DO  I csr!: LOOP ;
LOCK

8 0 csr@s: IE@ IM@ IP@ ICC@ DCC@ CC@ CFG@ EBA@
8 0 csr!s: IE! IM! IP! ICC! DCC! CC! CFG! EBA!
10 8 csr@s: DC@ DEBA@
10 8 csr!s: DC! DEBA!
20 14 csr@s: JTX@ JRX@ BP0@ BP1@ BP2@ BP3@
20 14 csr!s: JTX! JRX! BP0! BP1! BP2! BP3!
28 24 csr@s: WP0@ WP1@ WP2@ WP3@
28 24 csr!s: WP0! WP1! WP2! WP3!
31 28 csr@s: TLBCTRL@ TLBVADDR@ TLBPADDR@
31 28 csr!s: TLBCTRL! TLBVADDR! TLBPADDR!

CODE (bye)     ( -- ) \ hmm, only works when ra not modified
   ret,
END-CODE

$e0000000 CONSTANT csr-uart-rxtx
$e0000008 CONSTANT csr-uart-stat

\ threaded-code definition of (emit) hangs when debug-out 'dout' is used
\ : (emit)  ( char -- )
\    csr-uart-rxtx @ DROP    \ clear out receive buffer (ugly but required)
\    csr-uart-rxtx !
\    BEGIN csr-uart-stat @ 1 AND UNTIL ;  \ wait for transmit register empty
CODE (emit)  ( char -- )  \ defining (emit) via dout helps.
   r20  tos  mv,
   dout  calli,
   tos  fsp 0  lw,
   fsp  fsp 4  addi,
   next,
END-CODE


\ : (key?)  ( -- flag )
\    csr-uart-stat @ 2 AND 0<> ;
\ : (key)  ( -- char )
\    BEGIN (key?) UNTIL
\    csr-uart-rxtx @
\    2 csr-uart-stat ! ;

\ As logn as this in not IRQ-driven we use asm code for higher performance
\ Else chars swallowed, (especially when pasting to the console)
CODE (key?)  ( -- flag )
   fsp -4  tos  sw,
   tos  csr-uart-stat  li,
   tos  tos 0  lw,
   tos  tos 2  andi,
   tos  tos r0  cmpe,
   tos  tos -1 addi,
   fsp  fsp -4  addi,   
   next,
END-CODE  

CODE (key)  ( -- char )
   fsp -4  tos  sw,
   r2  csr-uart-stat  li,
   begin,
      tos  r2 0  lw,
      tos  tos 2  andi,
   tos  r0 ?<> until,
   tos  r2 -8  lw,   
   r3  2  mvi,
   r2 0  r3  sw,
   fsp  fsp -4  addi,
   next,
END-CODE  

: lm32boot
   'F (emit) boot ;

CODE: :doesjump
END-CODE


\ Customize Emacs
0 [IF]
   Local Variables:
   compile-command: "cd ../.. && ./build-ec lm32"
   End:
[THEN]
