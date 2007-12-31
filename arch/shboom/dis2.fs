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

\ written in august 1997 (C) Jens A. Wilke

hex

[IFUNDEF] X		: X ; immediate [THEN]

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


[IFDEF] gdiscover
: DisCall ( calltarget -- )
    gdiscover
    IF @name '{ emit type '} emit space
    ELSE ." call " . THEN ;
[ELSE]

include look.fs hex
    
: DisCall ( calltarget -- )
    look
    IF '{ emit head>string type '} emit space
    ELSE ." call " . THEN ;
[THEN]
	

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

create simple-ops-tab
30 , ," skip"
31 , ," skipc"
32 , ," skipn"
33 , ," skipz"
34 , ," step"
35 , ," skipnc"
36 , ," skipnn"
37 , ," skipnz"
38 , ," mloop"
39 , ," mloopc"
3a , ," mloopn"
3a , ," mloopnp"
3b , ," mloopz"
3c , ," bkpt"
3d , ," mloopnc"
3e , ," mloopnn"
3f , ," mloopnz"
40 , ," @"                 \ ld[]
41 , ," ld[x]"
42 , ," ld[r0]"
44 , ," ld[--r0]"
45 , ," scache"
46 , ," ld[r0++]"
48 , ," c@"                \ ld.b[]
49 , ," ld[x++]"
4a , ," ld[--x]"
4b , ," br[]"
4d , ," lcache"
4e , ," call[]"
60 , ," st[]"
61 , ," st[x]"
62 , ," st[r0]"
64 , ," st[--r0]"
66 , ," st[r0++]"
68 , ," st[--x]"
69 , ," st[x++]"
6e , ," ;"                 \ ret
6f , ," reti"
80 , ," r@"                \ push_r0
91 , ," push_mode"
92 , ," dup"               \ push_s0
93 , ," over"              \ push_s1
94 , ," push_ct"
96 , ," ldo[]"
97 , ," ldo.i[]"
98 , ," push_x"
99 , ," split"
9a , ," r>"                \ push_lstack
9b , ," ldepth"
9c , ," push_sa"
9d , ," push_la"
9e , ," push_s2"
9f , ," sdepth"
b0 , ," sto[]"
b1 , ," sto.i[]"
b2 , ," swap"
b3 , ," drop"
b4 , ," pop_ct"
b5 , ," replexp"
b6 , ," ei"
b7 , ," di"
b8 , ," pop_x"
b9 , ," pop_mode"
ba , ," >r"                \ pop_lstack
bb , ," add_pc"
bc , ," pop_sa"
bd , ," pop_la"
be , ," lframe"
bf , ," sframe"
c0 , ," add"
c1 , ," dec_ct"
c2 , ," addc"
c3 , ," xor"
c4 , ," expdif"
c5 , ," denorm"
c6 , ," normr"
c7 , ," norml"
c8 , ," -"
c9 , ," negate"            \ neg
ca , ," subb"
cb , ," cmp"
cc , ," inc#4"
cd , ," dec#4"
ce , ," 1+"                \ inc#1
cf , ," 1-"                \ dec#1
d0 , ," copyb"
d1 , ," rnd"
d2 , ," addexp"
d3 , ," subexp"
d4 , ," testexp"
d5 , ," muls"
d6 , ," mulfs"
d7 , ," mulu"
d8 , ," sexb"
d9 , ," testb"
da , ," replb"
db , ," extexp"
dc , ," extsig"
dd , ," notc"
de , ," divu"
df , ," mxm"
e0 , ," or"
e1 , ," and"
e2 , ," shl#1"
e3 , ," shr#1"
e4 , ," rot"               \ rev
e5 , ," 0="                \ eqz
e6 , ," shld#1"
e7 , ," shlr#1"
e8 , ," +"
e9 , ," iand"
ea , ," nop"
ec , ," shl#8"
ed , ," shr#8"
ee , ," shift"
ef , ," shiftd"
-1 ,

: op-simple? ( token -- token false | true )
    >r simple-ops-tab
    BEGIN
	dup @ r@ =
	IF cell+ count type space r> drop true EXIT THEN
	cell+ count + aligned
	dup @ -1 =
    UNTIL
    drop r> false ;

: push.b?
    dup 90 =
    IF
	drop ." push.b #" getbyte u. true
    ELSE
	false
    THEN ;

: push.l?
    dup 4f =
    IF
	drop ." push.l #" getquad u. true
    ELSE
	false
    THEN ;
	
: one-op ( token -- )
    op-simple? IF EXIT THEN
    push.l? IF EXIT THEN
    push.b? IF EXIT THEN
    b? IF EXIT THEN
    push.n? IF EXIT THEN
    push/pop? IF EXIT THEN
    drop ." ??? " ;

: disloop ( adr len -- )
    over I-IP ! + Stop-IP !
    getinit
    BEGIN
	getnextop
	dup ( _not_reached ) 4c =
	IF drop EXIT THEN
	one-op
	I-IP @ Stop-IP @ =
    UNTIL ;

: disxt ( adr -- )
    -1 disloop ;

: dis ( -- )
    X ' disxt ;

