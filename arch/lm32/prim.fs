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

: next,
   wa  fip 0  lw,
   r2  wa 0  lw,
   fip  fip 4  addi,
   r2  b, ;

end-macros

unlock
   $40000000   $20000 region dictionary
   setup-target
lock

label into-forth
   fip     $0000  mvhi,   \ patched later
   fip fip $0000  ori,    \ patched later   
   r0 r0 r0  xor,     \ r0 needs to be zeroed manually!
   IM  r0  wcsr,      \ disable IRQs for now
   fsp  $40021800  li,
   frp  $40022000  li,
   next,
end-label

\ Minimal Gforth primitive set

CODE: :docol
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
   fip  fip 0  lw,
   next,   
END-CODE

CODE lit
   fsp -4  tos  sw,
   tos  fip 0  lw,
   wa  fip 4  lw,		\ optimized instance of "next":
   fsp  fsp -4  addi,   
   r2  wa 0  lw,		\ fip update folded into next.
   fip  fip 8  addi,
   r2  b, 
END-CODE

CODE dup      ( n -- n n )
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
   tos  fsp 0  lw,
   fsp  fsp 4  addi,
   next,
END-CODE

CODE 2drop      ( n -- )
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

CODE r>       ( -- x   R: x -- )
   fsp -4  tos  sw,
   tos  frp 0  lw,
   fsp  fsp -4  addi,
   frp  frp 4  addi,   
   next,
END-CODE

CODE >r       ( x --   R: -- x )
   frp -4  tos sw,
   tos  fsp 0  lw,
   fsp  fsp 4  addi,
   frp  frp -4  addi,   
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

CODE *       ( n1 n2  -- n3 )  \ no easy way to implement (u)m* on lm32 :(
   r2  fsp 0  lw,
   tos  r2 tos  mul,
   fsp  fsp 4 addi,
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
   fip  frp 0  lw,
   frp  frp 4  addi,
   next,
END-CODE
 
CODE execute  ( xt -- )
   wa  tos  mv,		\ like "next", but with cfa fetched via tos
   r2  wa 0  lw,
   tos  frp 0  lw,
   frp  frp 4 addi,
   r2  b, 
END-CODE

CODE ?branch   ( flag -- ) \ jump on true
   fip  fip 4  addi,
   tos r0 ?<> if,
      fip  fip -4  lw,
   then,
   next,   
END-CODE

\ memory access
CODE @       ( a-addr -- x )
   tos  tos 0  lw,
   next,
END-CODE

CODE c@       ( a-addr -- c )
   tos  tos 0  lbu,
   next,
END-CODE

CODE !       ( x a-addr -- )
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
CODE sp@  ( -- sp )  \ hmm return sp at start or end of invocation? 
   fsp -4  tos  sw,
   tos  fsp -4  addi,   \ for now:  SP after sp@ call (as in 8086/prims.fs)
   fsp  fsp -4  addi,
   next,
END-CODE

CODE sp!      ( sp -- ) \ needs to conform to code above
   fsp  tos  mv,
   tos  fsp 0  lw,
   fsp fsp  4 addi,
   next,
END-CODE

CODE rp@  ( -- rp )
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

CODE (bye)     ( -- ) \ hmm, only works when ra not modified
   ret,
END-CODE

$e0000000 CONSTANT csr-uart-rxtx
$e0000008 CONSTANT csr-uart-stat

: (emit)  ( char -- )
   BEGIN csr-uart-stat @ 1 AND UNTIL   \ wait for transmit register empty
   csr-uart-rxtx ! ;
: (key?)  ( -- flag )
   csr-uart-stat @ 2 AND 0<> ;
: (key)  ( -- char )
   BEGIN (key?) UNTIL
   csr-uart-rxtx @ ;

: lm32boot
   'F (emit) boot ;

CODE: :doesjump
END-CODE

