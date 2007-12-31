\ dis.fs Disassembler for ShBoom CPU

\ Copyright (C) 1997,2003,2004,2007 Free Software Foundation, Inc.

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

>CROSS

hex

[IFUNDEF] X
: X ; immediate [THEN]
[IFUNDEF] linked,
: linked, ( link ) here swap dup @ , ! ; [THEN]


Create   I-Latch 4 chars allot
Variable I-Nr
Variable T-IP
Variable I-IP
Variable Stop-IP
4 Value MaxOps

: getinit
  4 to MaxOps
  4 I-Nr ! ;

: getquad ( -- n )
  I-IP @ X @ X cell I-IP +! ;

: getops ( -- )
  4 0 DO I-IP @ I + X c@ I-Latch I + C! LOOP
  I-IP @ T-IP !
  X cell I-IP +! 
  0 I-Nr !
  4 to MaxOps ;

: getop ( -- token true | false )
\ gets next opcode from instruction latch
  I-Nr @ MaxOps = IF false EXIT THEN
  I-Latch I-Nr @ chars + c@
  1 I-Nr +! true ;

: getnextop ( --   token )
  getop 0= IF cr getops getop drop THEN ;

: getbyte ( -- c )
  I-Nr @ MaxOps u>=
  ABORT" No byte in opcode quad"
  I-Latch 3 chars + c@
  3 to MaxOps ;

: TotalCollect ( n -- n )
\ collect rest of instrution
  BEGIN GetOp WHILE
        swap 8 lshift or
  REPEAT ;

: RestBytesNr
  4 I-Nr @ - ;

: TotalBrBits
  RestBytesNr 8 * 3 + ;

: DisCall ( calltarget -- )
  gdiscover
  IF @name '< emit type '> emit space
  ELSE ." call " . THEN ;

: b? ( token -- token false | true )
\  BREAK:
  dup $e0 and
  0<> IF false EXIT THEN
  TotalBrBits >r
  dup $07 and TotalCollect

  \ check for highest bit
  \ if set, fill rest to the left with 1
  1 r@ 1- lshift over and
  IF    1 r> lshift 1- invert or
  ELSE  rdrop THEN
  4 * swap

  $18 and
  CASE  0   OF ." br"   ENDOF
        $8  OF \ ." call"
                T-IP @ + discall true EXIT ENDOF
        $10 OF ." bz"   ENDOF
        $18 OF ." dbr"  ENDOF
  ENDCASE
  space
  . true ;

: push.n? ( token -- token true | false )
  dup $f0 and $20 <> IF false EXIT THEN
  ." push.n #"
  $f and
  dup 9 u< IF . ELSE $10 - . THEN
  true ;

: push/pop? ( token -- token false | true )
  dup $50 $60 within
  IF ." pop g" $f and dec. true EXIT THEN
  dup $70 $80 within
  IF ." push g" $f and dec. true EXIT THEN
  dup $a0 $af within
  IF ." pop r" $f and dec. true EXIT THEN
  dup $80 $8f within
  IF ." push r" $f and dec. true EXIT THEN
  false ;

: op-simple ( adr -- )
  3 cells + count type space ;  

Variable op-link 0 op-link !
Variable op-xt

: op1
  op-link linked, name evaluate , , name string, align ;

: op
  op-xt @ op1 ;

' op-simple op-xt !

op 30 skip
op 31 skipc
op 32 skipn
op 33 skipz
op 34 step
op 35 skipnc
op 36 skipnn
op 37 skipnz
op 38 mloop
op 39 mloopc
op 3a mloopn
op 3a mloopnp
op 3b mloopz
op 3c bkpt
op 3d mloopnc
op 3e mloopnn
op 3f mloopnz
op 40 @                 \ ld[]
op 41 ld[x]
op 42 ld[r0]
op 44 ld[--r0]
op 45 scache
op 46 ld[r0++]
op 48 c@                \ ld.b[]
op 49 ld[x++]
op 4a ld[--x]
op 4b br[]
op 4d lcache
op 4e call[]
op 60 st[]
op 61 st[x]
op 62 st[r0]
op 64 st[--r0]
op 66 st[r0++]
op 68 st[--x]
op 69 st[x++]
op 6e ;                 \ ret
op 6f reti
op 80 r@                \ push_r0
op 91 push_mode
op 92 dup               \ push_s0
op 93 over              \ push_s1
op 94 push_ct
op 96 ldo[]
op 97 ldo.i[]
op 98 push_x
op 99 split
op 9a r>                \ push_lstack
op 9b ldepth
op 9c push_sa
op 9d push_la
op 9e push_s2
op 9f sdepth
op b0 sto[]
op b1 sto.i[]
op b2 swap
op b3 drop
op b4 pop_ct
op b5 replexp
op b6 ei
op b7 di
op b8 pop_x
op b9 pop_mode
op ba >r                \ pop_lstack
op bb add_pc
op bc pop_sa
op bd pop_la
op be lframe
op bf sframe
op c0 add               
op c1 dec_ct
op c2 addc
op c3 xor
op c4 expdif
op c5 denorm
op c6 normr
op c7 norml
op c8 -
op c9 negate            \ neg
op ca subb
op cb cmp
op cc inc#4
op cd dec#4
op ce 1+                \ inc#1
op cf 1-                \ dec#1
op d0 copyb
op d1 rnd
op d2 addexp
op d3 subexp
op d4 testexp
op d5 muls
op d6 mulfs
op d7 mulu
op d8 sexb
op d9 testb
op da replb
op db extexp
op dc extsig
op dd notc
op de divu
op df mxm
op e0 or
op e1 and
op e2 shl#1
op e3 shr#1
op e4 rot               \ rev
op e5 0=                \ eqz
op e6 shld#1
op e7 shlr#1
op e8 +
op e9 iand
op ea nop
op ec shl#8
op ed shr#8
op ee shift
op ef shiftd


:noname ( adr -- )
  drop ." push.l #" getquad u. ; op1 4f push.l

:noname ( adr .. )
  drop ." push.b #" getbyte u. ; op1 90 push.b

: op-simple? ( token -- token flase | true )
  >r op-link
  BEGIN @ dup WHILE
        dup cell+ @ r@ =
        IF rdrop dup 2 cells + @ EXECUTE true EXIT THEN

  REPEAT
  drop r> false ;

: one-op ( token -- )
  op-simple? ?EXIT
  b? ?EXIT
  push.n? ?EXIT
  push/pop? ?EXIT
  drop ." ??? " ;

: disloop ( adr len -- )
  over I-IP ! + Stop-IP !
  getinit
  BEGIN getnextop
        dup _not_reached =
        IF drop EXIT THEN
        one-op
        I-IP @ Stop-IP @ =
  UNTIL ;

: disxt ( adr -- )
  -1 disloop ;

: dis ( -- )
  T ' H disxt ;

