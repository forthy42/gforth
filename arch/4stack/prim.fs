\ 4stack primitives

\ Copyright (C) 2000,2003,2007,2008 Free Software Foundation, Inc.

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

Label start
        nop          ;; first opcode must be a nop!
	$80000000 ## ;;
	#,           ;;
	sr!          jmpa $818 >IP ;;

$800 .org
ip0:	.int 0
	.int 0
varpat:	ip@      nop       nop      jmpa                              ;;
colpat:	ip@      nop       nop      jmpa                              ;;
;;      ds       cfa       fs       rs
main:   ;;
	-$200 ## nop       nop      nop       -8 #        ld 1: ip    ;;
	#,       nop       nop      nop       set 0: R3               ;;
	nop      nop       nop      nop       0 #         set 1: R1   ;;
	nop      nop       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;

docol:  .endif ;;
;;	nop      ip@       nop      call docol                        ;;
;;      ds ca    cfa       fs       rs
dodoes:
;;      ip@      nop       nop      call doesjump
;;      ip@      nop       nop      call dodoes
;;      ds df ca cfa       fs       rs
        drop     pick 0s0  nop      nop       0 #         get 3: R1   ;;
	nop      nop       nop      -4 #      0 #         set 1: R1   ;;
        nop      drop      nop      add       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;

dovar:  .endif ;;
;;	ip@      nop       nop      call dovar                        ;;
;;      ds       cfa       fs       rs
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;

docon:  ;;
;;	ip@      nop       nop      call dovar                        ;;
;;      ds       cfa       fs       rs
	nop      ip!       nop      nop       ld 0: s0b   ld 1: R1 N+ ;;
	drop     nop       nop      nop                               ;;
end-code

-2 Doer: :docol
-3 Doer: :docon
-4 Doer: :dovar
-9 Doer: :dodoes
-10 Doer: :doesjump

Code execute ( xt -- )
	ip!      nop       nop      nop                               ;;
	nop      nop       nop      nop                               ;;
end-code

Code ?branch
	nop      nop       nop      nop       br 0 ?0<>
	nop      dup       nop      nop       0 #         set 1: R1   ;;
.endif
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code +
	add      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code and
	and      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code xor
	xor      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code sp@
	sp@      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code sp!
	sp!      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code rp@
	nop      ip!       nop      sp@       0 #         ld 1: R1 N+ ;;
	pick 3s0 nop       nop      drop                              ;;
end-code

Code rp!
	drop     ip!       nop      pick 0s0  0 #         ld 1: R1 N+ ;;
	nop      nop       nop      sp!                               ;;
end-code

Code ;s
	nop      drop      nop      nop       0 #         set 3: R1   ;;
	nop      nop       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code @
	nop      ip!       nop      nop       ld 0: s0b   ld 1: R1 N+ ;;
	drop     nop       nop      nop                               ;;
end-code

Code !
	drop     ip!       nop      nop       st 0: s0b   ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

\ obligatory IO

Code (key?)
	nop      nop       nop      nop       inb R3      3 #         ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	0<>      nop       nop      nop                               ;;
end-code

Code (key)
.begin					      inb R3	  3 #          ;;
	nop				      br 0 ?0= .until
					      inb R3	  2 #          ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code (emit)
.begin					      inb R3	  1 #         ;;
	nop				      br 0 ?0= .until
					      outb R3	  0 #         ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

\ this was obligatory, now some things to speed it up

Code 2/
	asr      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code branch
	nop      nop       nop      nop       0 #         set 1: R1   ;;
	nop      nop       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code (loop)
	pick 3s1 nop       nop      inc                               ;;
        sub 3s0  nop       nop      nop       br 0 ?0=
	nop      dup       nop      nop       0 #         set 1: R1   ;;
.endif
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code (+loop)
	pick 3s1 nop       nop      nop                               ;;
	subr 3s0 nop       nop      nop                               ;;
	xor #min nop       nop      nop                               ;;
	add s1   nop       nop      nop       br 0 ?ov
	nop      dup       nop      nop       0 #         set 1: R1   ;;
.endif
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	drop     nop       nop      add 0s0                           ;;
end-code

Code (do)
	nip      ip!       nop      pick 0s1  0 #         ld 1: R1 N+ ;;
	drop     nop       nop      pick 0s0                          ;;
end-code

Code unloop
	nop      ip!       nop      drop      0 #         ld 1: R1 N+ ;;
	nop      nop       nop      drop                              ;;
end-code

Code -
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code or
	or       ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code 1+
	inc      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code cell+
	4 #      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	add      nop       nop      nop                               ;;
end-code

Code cells
	asl      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	asl      nop       nop      nop                               ;;
end-code

Code c@
	nop      ip!       nop      nop       ldb 0: s0b  ld 1: R1 N+ ;;
	drop     nop       nop      nop                               ;;
end-code

Code c!
	drop     ip!       nop      nop       stb 0: s0b  ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code um*
	umul     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	mul@     nop       nop      nop                               ;;
end-code

Code m*
	mul      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	mul@     nop       nop      nop                               ;;
end-code

Code d+
	pass     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	mul@+    nop       nop      nop                               ;;
end-code

Code >r
	drop     ip!       nop      pick 0s0  0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code r>
	pick 3s0 ip!       nop      drop      0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code drop
	drop     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code swap
	swap     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code over
	over     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code 2dup
	over     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	over     nop       nop      nop                               ;;
end-code

Code rot
	rot      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code -rot
	rot      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	rot      nop       nop      nop                               ;;
end-code

Code i
	pick 3s0 ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code i'
	pick 3s1 ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code j
	pick 3s2 ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code lit
	pick 1s0 drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code 0=
	0=       ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code 0<>
	0<>      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop                               ;;
end-code

Code u<
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	u<       nop       nop      nop                               ;;
end-code

Code u>
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	u>       nop       nop      nop                               ;;
end-code

Code u<=
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	u<=      nop       nop      nop                               ;;
end-code

Code u>=
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	u>=      nop       nop      nop                               ;;
end-code

Code <=
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	<=       nop       nop      nop                               ;;
end-code

Code >=
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	>=       nop       nop      nop                               ;;
end-code

Code =
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	0=       nop       nop      nop                               ;;
end-code

Code <>
	subr     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	0<>      nop       nop      nop                               ;;
end-code

\ : (findl-samelen) ( u name1 -- u name2/0 )
\     BEGIN  2dup cell+ @ $1FFFFFFF and <> WHILE  @  dup 0= UNTIL  THEN ;
Code (findl-samelen)
        nop      0 #       0 #      $20 #                             ;;
        nop      nop       pick 0s0 hib                               ;;
        nop      nop       nop      dec                               ;;
.begin
	drop     drop      nop      nop       ld 0: s0b   1 #         ;;
        nop      pick 3s0  nip      nop       ld 2: s0b   0 #         ;;
	drop     and 0s0   nop      nop                               ;;
	pick 2s0 sub 0s0   nop      nop       br 1&2 :0<> .until      ;;
	nop      nop       nop      nop       br 1 ?0=                ;;
	nop      ip!       drop     drop      0 #         ld 1: R1 N+ ;;
	nop      nop       drop     nop                               ;;
.endif
	pick 2s1 ip!       drop     drop      0 #         ld 1: R1 N+ ;;
	nip      nop       drop     nop                               ;;
end-code

\ necessary high-level code

: (type)
    bounds ?DO  I c@ (emit)  LOOP ;
\    BEGIN  dup  WHILE
\	>r dup c@ (emit) 1+ r> 1-  REPEAT  2drop ;

\ obligatory code address manipulations

: >code-address ( xt -- addr )  cell+ @ -8 and ;
: >does-code    ( xt -- addr )
    cell+ @ -8 and \ dup 3 and 3 <> IF  drop 0  EXIT  THEN
    8 + dup cell - @ 3 and 0<> and ;
: code-address! ( addr xt -- )  >r 3 or $808 @ r> 2! ;
: does-code!    ( a_addr xt -- )  >r 5 - $808 @ r> 2! ;
: does-handler! ( a_addr -- )  >r $810 2@ r> 2! ;
2 cells constant /does-handler

: bye  0 execute ;
: (bye) 0 execute ;
: float+ 8 + ;

: capscomp ( c_addr1 u c_addr2 -- n )
 swap bounds
 ?DO  dup c@ I c@ <>
     IF  dup c@ toupper I c@ toupper =
     ELSE  true  THEN  WHILE  1+  LOOP  drop 0
 ELSE  c@ toupper I c@ toupper - unloop  THEN  sgn ;

\ division a/b
\ x:=a, y:=b, r:=est; iterate(x:=x*r, y:=y*r, r:=2-y*r);
\ result: x=a/b; y=1; r=1

Code newu/mod ( u1 u2 -- q r )
    drop     nop       pick 0s0  call idiv ;;
    pick 1s0 drop      nop       nop                 ;;
    swap     ip!       nop       nop       0 #         ld 1: R1 N+ ;;
    nop      nop       nop       nop                               ;;
.macro .idiv-table [F]
	$100 $80 DO  $100.00000000 I 2* 1+ um/mod  long, drop  LOOP
.end-macro
approx:
   .idiv-table
idiv:
;; a         --        b         --
   nop       pick 2s0  ff1       1 #       br 1 :0=              ;;
   ip@       pick 2s0  bfu       cm!       set 0: R2             ;;
;; a         n         b'        --
   nop       -$1D #    lob       pick 2s0  0 #            -$104 ## ;;
   nop       add       pick 3s0  drop      ld 2: R2 +s0   #, ;;
   nop       cm!       nip       nop       ;;
;; a         n         b' r      --
   umul 2s0  pick 0s0  umul      nop       ;;
   mulr@     0 #       mulr@     -mulr@    ;; first iteration
   umul 3s0  pick s2   umul 3s0  drop      ;;
   mulr@     nop       nop       -mulr<@   ;; second iteration
   umul 3s0  nop       nop       drop      ;;
   nop       mulr<@    nop       nop       ;; final iteration+shift
   pick 1s0  umul      nop       nop       ;;
   nop       -mul@+    nop       ret       br 1 ?0< ;;
   nop       nip       nop       nop       ;;
.endif
   dec       add       nop       nop       ;;
;; q         r

.endif
   nop       drop      drop      drop      ;;
   dec       0 #       drop      ret       ;;
   nop                                     ;;
end-code

: new/mod  ( d1 n1 -- n2 n3 )
 dup >r dup 0< IF  negate >r negate r>  THEN
 over       0< IF  tuck + swap  THEN
 newu/mod
 r> 0< IF  swap negate swap  THEN ;
