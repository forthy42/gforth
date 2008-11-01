\ 4stack primitives

\ Copyright (C) 2000,2007,2008 Free Software Foundation, Inc.

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

Label start  ;;
nop          ;; first opcode must be a nop!
$80000000 ## ;;
#,           ;;
sr!          jmpa $828 >IP ;;

$800 .org
ip0:	.int 0
	.int 0
conpat:	nop      nop       ip@      jmpa                              ;;
varpat:	nop      nop       ip@      jmpa                              ;;
jmppat:	nop      ip@       nop      jmpa                              ;;
colpat:	nop      nop       ip@      jmpa                              ;;
;;      ds       cfa       fs       rs
main:   ;;
	-$200 ## nop       nop      nop       -12 #       ld 1: ip    ;;
	#,       nop       nop      nop       set 0: R3               ;;
	nop      nop       nop      nop       0 #         set 1: R1   ;;
	nop      nop       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;

docol:  .endif ;;
;;	nop      nop       ip@      jmp docol                         ;;
;;      ds ca    cfa       fs       rs
        nop      8 #       drop     -12 #     get 0: R1   get 3: R1   ;;
	drop     add 0s0   nop      add       0 #         set 1: R1   ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
dodoes: .endif ;;
;;      nop      nop       ip@      jmp doesjump
;;      nop      ip@       nop      jmp dodoes
;;      ds df ca cfa       fs       rs
	8 #      nop       drop     -12 #     get 0: R1   get 3: R1   ;;
	add      nop       nop      add       0 #         set 1: R1   ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
dovar:  .endif ;;
;;	nop      nop       ip@      jmp dovar                         ;;
;;      ds       cfa       fs       rs
	8 #      swap      ip!      nop       get 0: R1               ;;
	add      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;

docon:  .endif ;;
;;	nop      nop       ip@      jmp dovar                         ;;
;;      ds       cfa       fs       rs
	nop      swap      ip!      nop       ld 0: R1    2 #         ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

-2 Doer: :docol
-3 Doer: :docon
-4 Doer: :dovar
-8 Doer: :dodoes
-9 Doer: :doesjump

Code execute ( xt -- )
	nop      nop       nop      ip@       br .endif
	ip!      drop      pick 0s0 nop       set 2: R1               ;;
	nop      nop       nop      ip!       -1 #        ld 1: R1    ;;
end-code

Code ?branch
	nop      nop       nop      ip@       br .endif
	nop      swap      nop      nop       br 0 ?0<>
	nop      nop       nop      nop       -12 #       R1= R1 3: +s0 ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
.endif
	nop      ip!       nop      drop      0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code +
	add      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code and
	and      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code xor
	xor      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code sp@
	sp@      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code sp!
	sp!      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code rp@
	nop      nop       ip@      sp@       br .endif
	pick 3s0 swap      ip!      drop                              ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code rp!
	drop     nop       ip@      pick 0s0  br .endif
	nop      swap      ip!      sp!                               ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code ;s
	nop      nop       nop      nop       br .endif
	nop      drop      nop      nop       0 #         set 3: R1   ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      nop       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code @
	nop      nop       ip@      nop       br .endif
	drop     swap      ip!      nop       ld 0: s0b   0 #         ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code !
	nop      nop       ip@      nop       br .endif
	drop     swap      ip!      nop       st 0: s0b   0 #         ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

\ obligatory IO

Code key?
	nop      nop       ip@      nop       br .endif
	nop      swap      nop      nop       inb R3      3 #         ;;
	nop      nop       ip!      nop                               ;;
	0<>      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code (key)
	nop      nop       ip@      nop       br .endif
.begin					      inb R3	  3 #         ;;
	nop				      br 0 ?0= .until
					      inb R3	  2 #         ;;
	nop      swap      ip!      nop                               ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code (emit)
	nop      nop       ip@      nop       br .endif
;; .begin					      inb R3	  1 #         ;;
;;	nop				      br 0 ?0= .until
					      outb 0: R3  0 #         ;;
	nop      swap      ip!      nop                               ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code 2/
	asr      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code branch
	nop      nop       nop      ip@       br .endif
	nop      nop       nop      nop       -12 #       R1= R1 3: +s0 ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       nop      drop      0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code (loop)
	pick 3s1 nop       nop      ip@       br .endif
	dec      nop       nop      nop                               ;;
	sub 3s1  swap      nop      nop       br 0 ?0=
	nop      nop       nop      nop       -12 #       R1= R1 3: +s0 ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
.endif
	nop      ip!       nop      drop      0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      inc       set 2: R1   ld 1: R1 N+ ;;
end-code

Code (+loop)
	pick 3s1 nop       nop      ip@       br .endif
	subr 3s1 nop       nop      nop                               ;;
	xor #min nop       nop      nop                               ;;
	add s1   swap      nop      nop       br 0 ?ov
	nop      nop       nop      nop       -12 #       R1= R1 3: +s0 ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
	nop      drop      nop      nop       0 #         ld 1: R1 N+ ;;
.endif
	nop      ip!       nop      drop      0 #         ld 1: R1 N+ ;;
	drop     ip!       ip@      add 0s0   set 2: R1   ld 1: R1 N+ ;;
end-code

Code (do)
	nop      nop       ip@      nop       br .endif
	nip      swap      ip!      pick 0s1                          ;;
	drop     ip!       ip@      pick 0s0  set 2: R1   ld 1: R1 N+ ;;
end-code

Code -
	subr     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code or
	or       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code 1+
	inc      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code 2*
	asl      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code cell+
	add c2   ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code cells
	nop      nop       ip@      nop       br .endif
	asl      swap      ip!      nop                               ;;
	asl      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code c@
	nop      nop       ip@      nop       br .endif
	drop     swap      ip!      nop       ldb 0: s0b   0 #         ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code c!
	nop      nop       ip@      nop       br .endif
	drop     swap      ip!      nop       stb 0: s0b   0 #         ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code um*
	nop      nop       ip@      nop       br .endif
	umul     swap      ip!      nop                               ;;
	mul@     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code m*
	nop      nop       ip@      nop       br .endif
	mul      swap      ip!      nop                               ;;
	mul@     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code d+
	nop      nop       ip@      nop       br .endif
	pass     swap      ip!      nop                               ;;
	mul@+    ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code >r
	drop     ip!       ip@      pick 0s0  set 2: R1   ld 1: R1 N+ ;;
end-code

Code r>
	pick 3s0 ip!       ip@      drop      set 2: R1   ld 1: R1 N+ ;;
end-code

Code drop
	drop     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code swap
	swap     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code over
	over     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code 2dup
	nop      nop       ip@      nop       br .endif
	over     swap      ip!      nop                               ;;
	over     ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code rot
	rot      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code -rot
	nop      nop       ip@      nop       br .endif
	rot      swap      ip!      nop                               ;;
	rot      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code i
	pick 3s0 ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code i'
	pick 3s1 ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code j
	pick 3s2 ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code lit
	ip@      nop       pick 1s0 nop       br .endif                ;;
	nop      nip       ip!      nop       0 #         ld 1: R1 N+ ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code 0=
	0=       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code 0<>
	0<>      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code u<
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	u<       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code u>
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	u>       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code u>=
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	u>=      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code u<=
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	u<=      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code >=
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	>=       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code <=
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	<=       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code =
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	0=       ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

Code <>
	nop      nop       ip@      nop       br .endif
	subr     swap      ip!      nop                               ;;
	0<>      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

\ : (find-samelen) ( u f83name1 -- u f83name2/0 )
\     BEGIN  2dup cell+ c@ $1F and <> WHILE  @  dup 0= UNTIL  THEN ;
Code (find-samelen)
	nop      nop       ip@      nop       br .endif
        nop      0 #       0 #      nop                               ;;
	nop      nop       pick 0s0 nop                               ;;
.begin
	drop     drop      nop      nop       ldb 0: s0b  4 #         ;;
        nop      $1F #     nip      nop       ld 2: s0b   0 #         ;;
	drop     and 0s0   nop      nop                               ;;
	pick 2s0 sub 0s0   nop      nop       br 1&2 :0<> .until      ;;
	pick 2s1 nop       pass     nop       br 1 ?0=                ;;
	drop     swap      ip!      nop                               ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
.endif
	nip      swap      ip!      nop                               ;;
	nop      ip!       ip@      nop       set 2: R1   ld 1: R1 N+ ;;
end-code

\ obligatory code address manipulations

: >code-address ( xt -- addr )  cell+ @ -8 and ;
: >does-code    ( xt -- addr )
    cell+ @ -8 and \ dup 3 and 3 <> IF  drop 0  EXIT  THEN
    8 + dup cell - @ 3 and 0<> and ;
: code-address! ( addr xt -- )  >r 3 or $808 @ r> 2! ;
: does-code!    ( a_addr xt -- )  >r 5 - $808 @ r> 2! ;
: does-handler! ( a_addr -- )  $818 2@ rot 2! ;

\ this was obligatory, now some things to speed it up

: (type)
    bounds ?DO  I c@ (emit)  LOOP ;
\    BEGIN  dup  WHILE
\	>r dup c@ (emit) 1+ r> 1-  REPEAT  2drop ;

\ division a/b
\ x:=a, y:=b, r:=est; iterate(x:=x*r, y:=y*r, r:=2-y*r);
\ result: x=a/b; y=1; r=1

\ Label idiv-table
\ idiv-tab:
\ .macro .idiv-table [F]
\ 	$100 $80 DO  0 $100 I 2* 1+ um/mod  long, drop  LOOP
\ .end-macro
\ 	.idiv-table
\ end-code
\ 
\ Code um/mod1 ( u -- 1/u )
\ ;;	b        --        --       --        --          --          ;;
\ 	ff1      -$1F #    nop      nop       br 0 :0= div0
\ 	bfu      add 0s0   ip@      nop       set 2: R2               ;;
\ ;;	b'       --        --       --        --          --          ;;
\ 	lob      $0FF ##   pick 0s0 pick 0s0  0 #         -$108 ## ;;
\ 	1 #      #,        sub #min 1 #       ld 0: R2 +s0 #,         ;;
\ 	cm!      and       nop      cm!       br 2 ?0= by2
\ ;;      est      --        --       b'        --          --          ;;
\ 	umul 3s0 pick 0s0  nop      umul 0s0  0 #         0 #         ;;
\ 	mulr<@   nop       nop      -mulr@                            ;;
\ 	drop     umul 3s0  nop      umul 0s0                          ;;
\ 	mulr<@   cm!       nop      -mulr@                            ;;
\ 	umul 3s0 drop      pick 1s0 drop                              ;;
\ 	drop     mulr<@    ip!      nop       0 #         ld 1: R1 N+ ;;
\ 	pick 1s0 drop      nop      nop                               ;;
\ by2:
\ div0:
\ 	-1 #     ip!       nop      nop       0 #         ld 1: R1 N+ ;;
\ 	nop      nop       nop      nop                               ;;
\ end-code
