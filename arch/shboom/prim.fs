\ Prims for ShBoom

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

hex

\ Used Global Registers:
\
\ G0 used by DIV and MUL
\ G1 UP


6e ALIAS ;s
: add,    M c0 ;
: add_pc, _inline       M bb ;
: +       M e8 ;
: addc,   M c2 ;
: addexp, M d2 ;
: and     M e1 ;
: bkpt,   M 3c ;
: lcache, M 4d ;
: scache, M 45 ;
: execute M 4e ; 	\ call[]
: cmp,    M cb ;
: copyb,  M d0 ;
: 1-      M cf ;
: 4-      M cd ;
: cell-   M cd ;
: dec_ct, M c1 ;
: denorm, M c5 ;
: ldepth, M 9b ;
: sdepth, M 9f ;
: di,     M b7 ;
: divu,   M de ;
: ei,     M b6 ;
: 0=      M e5 ;        \ eqz
: expdif, M c4 ;
: extexp, M db ; 
: extsig, M dc ;
: lframe, M be ;
: sframe, M bf ;
: iand,   M e9 ;
: 1+      M ce ;
: 4+      M cc ;
: cell+   M cc ;
: char+   M ce ; 
: ld[--r0], M 44 ;
: ld[--x],  M 4a ;
: ld[r0++], M 46 ;
: ld[r0],   M 42 ;
: ld[x++],  M 49 ;
: ld[x],  M 41 ;
: @       M 40 ;
: c@      M 48 ;
: ldo[],  M 96 ;
: ldo.o[], M 97 ;
: mloop,  M 38 ;
: mloopc, M 39 ;
: mloopn, M 3a ;
: mloopnc, M 3d ; 
: mloopp, M 3e ;
: mloopnz, M 3f ;
: mloopz, M 3b ;
: mulfs,  M d6 ;
: muls,   M d5 ;
: mulu,   M d7 ;
: mxm,    M df ;
: negate  M c9 ;
: noop    M ea ;
: norml,  M c7 ;
: normr,  M c6 ;
: notc,   M dd ;
: or      M e0 ;
: drop    M b3 ;           \ pop
: pop_ct, M b4 ;
\ ,pop_gi !!
: pop_la, _inline M bd ;
: >r _inline M ba ;		\ pop_lstack
: pop_mode, M b9 ;
\ ,pop_ri !!
: pop_sa, M bc ;
: pop_x,  M b8 ;
: dup     M 92 ;            \ push
: push_ct, M 94 ;
\ , push gi
: push_la, _inline M 9d ;
: r> _inline M 9a ;         \ push_lstack
: push_mode, M 91 ;
\ ,push ri !!
: r@ _inline M 80 ;         \ push r0
: over    M 93 ;           \ push s1
: 2pick   M 94 ;          \ push s2
: push_sa, M 9c ;
: push_x, M 98 ;
: replb,  M da ;
: replexp, M b5 ;
\ : ;s M 6e ;		\ ret
: reti,   M 6f ;
: rot     M e4 ;            \ rev
: rnd,    M d1 ;
: shift,  M ee ;
: shiftd, M ef ;
: 2*      M e2 ;
: 8<<     M ec ;        \ shl #8
: shld#1, M e6 ;
: shr#1,  M e3 ;
: shr#8,  M ed ;
: 8>>     M ed ;
: 2/      shr#1, ;
: shrd#1, M e7 ;
: skip,   M 30 ;
: skipc,  M 31 ;
: skipn,  M 32 ;
: skipnc, M 35 ;
: skipp,  M 36 ;
: skipnz, M 37 ;
: skipz,  M 33 ;
: split,  M 99 ;
: st[--r0], M 64 ;
: st[--x],  M 68 ;
: st[r0++], M 66 ;
: st[r0],   M 62 ;
: st[x++],  M 69 ;
: st[x],  M 61 ;
: st[],   M 60 ;
: step,   M 34 ;
: sto[],  M b0 ;
: sto.i[],  M b1 ;
: -       M c8 ;              \ sub
: subb,   M ca ;
: subexp, M d3 ;
: testb,  M d9 ;
: testexp, M d4 ;
: swap    M b2 ;           \ xcg
: xor     M c3 ;

: nip     swap drop ;
: !       st[], drop ;

: up@     M 71 ;
: up!     M 51 ;

: sp!    \ ( 's (emit) dup .x ) 
        pop_sa, drop drop ;
: sp@ 	-2 .quad4 
        scache, drop sdepth, push_sa,
	swap 2* 2* -
\	'S (emit) dup .x 
 	;

\ nochmal testen!
\ : pick  >r
\	-&14 .quad2
\ 	,scache	\ wirte all to memory
\	,push_sa drop
\	r> cells + @ ;

: pick  dup
	BEGIN dup WHILE rot >r 1- REPEAT
	drop over swap
	BEGIN dup WHILE r> rot rot 1- REPEAT
	drop ;

: rp@   _noinline -&14 .quad2 lcache, push_la,
 	\ ,ldepth 2* 2* -
\	'R (emit) dup .x 
	;

\ : rp! 'r (emit) dup .x pop_la, ;
: rp! _inline pop_la, ;


: um*
  M 50 0 mulu, ;

: um/mod
  M 50 divu, swap ;

: < cmp, drop shr#8, shr#8, shr#8, shr#8, ;


