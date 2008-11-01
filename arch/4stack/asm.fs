\ four stack assembler                                 19jan94py

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

Vocabulary asm4stack
Vocabulary asmdefs

asm4stack also asmdefs also definitions Forth

' asm4stack Alias [A] immediate
' Forth     Alias [F] immediate
: :A asm4stack definitions Forth ;
: :D asmdefs   definitions Forth ;

\ assembly area setup                                  24apr94py

Defer '2@
Defer '2!
Defer 'c!
Defer '!
Defer 'SF!
Defer 'F!
Defer 4here
Defer 4allot

\ frame format:
\ { target addr, target length, host addr, framelink }

: 4there  4here ;

cell 8 = [IF]
: op!       >r drop $100000000 /mod r> '2! ;
: op@       '2@ $100000000 * + 0 ;
[ELSE]
: op!       '2! ;
: op@       '2@ ;
[THEN]
: op,       4there op!  8 4allot ;
: caddr  ;  immediate
: waddr  ;  immediate
: laddr  ;  immediate

\ instruction generation                               24apr94py

2Variable ibuf       0. ibuf 2!
Variable  instfield  0  instfield !
Variable  condfield  0  condfield !
Variable  lastmove   0  lastmove !

8 cells Constant bit/cell

Create instmasks  $003FFFFF.FFFFFFFF , ,
                  $FFC00FFF.FFFFFFFF , , 
                  $FFFFF003.FFFFFFFF , ,
                  $FFFFFFFC.00FFFFFF , ,
                  $FFFFFFFF.FF003FFF , ,
                  $FFFFFFFF.FFFFC00F , ,

: instshift ( 10bit -- 64bit )
  1 5 instfield @ - &10 * 4 + bit/cell /mod >r
  lshift um* r> IF  swap  THEN ;

: 2and  ( d1 d2 -- d )  rot and -rot and swap ;
: 2or   ( d1 d2 -- d )  rot or  -rot or  swap ;

: !inst ( 10bit -- )  instshift
  instfield @ 2* cells instmasks + 2@ ibuf 2@ 2and 2or ibuf 2!
  1 instfield +! ;

: finish ( -- )  ibuf 2@ op,
  0 0 ibuf 2!  instfield off  condfield off lastmove off ;
: finish?   instfield @ IF  finish  THEN ;
:A
: ;;  ( -- )  finish?  postpone \ ;
: .org ( n -- )  4here - 4allot ;
:D

\ checks for instruction slots                         19jan94py

: alu?  ( -- flag )  instfield @ 0 4 within ;
: move? ( -- flag )  instfield @ 4 6 within
  ibuf cell+ @ 3 and 1 <> and ;
: call? ( -- flag )  instfield @ 4 < ;
: br?   ( -- flag )  instfield @ 5 < ;

: ?finish ( -- )  instfield @ 6 = IF  finish  THEN ;

\ automatic feed of instructions                       19jan94py

Variable lastalu
Variable lastalufield

: !alu  ( 10bit -- )
  alu? 0= IF  finish  THEN
  dup lastalu !
  instfield @ lastalufield !
  !inst ;

: !data ( 10bit -- )  alu? IF  4 instfield !  THEN
  move? 0= IF  finish 4 instfield !  THEN
  instfield @ lastmove !  !inst ;

: !br ( 10bit likelyhood -- addr )
  br? 0= abort" No Data in Branch!"
  alu? IF  4 instfield !  THEN  >r !inst
  ibuf 2@  2 r> 3 and 2* 2* + 0 2or  ibuf 2!  4here ;
:A
: do   ( -- addr )     0 0 !br finish ;
: br   ( -- addr )  $200 1 !br ;

: br,0 ( -- addr )  $200 0 !br ;
: br,1 ( -- addr )  $200 1 !br ;

: call ( -- addr )  call? 0= IF  finish  THEN
  6 instfield !  ibuf 2@ $0.00000003 2or  ibuf 2!  4here ;
: jmp  ( -- addr )  call? 0= IF  finish  THEN
  6 instfield !  ibuf 2@ $1.00000003 2or  ibuf 2!  4here ;

: calla ( -- addr )  call? 0= IF  finish  THEN
  6 instfield !  ibuf 2@ $2.00000003 2or  ibuf 2!  4here ;
: jmpa  ( -- addr )  call? 0= IF  finish  THEN
  6 instfield !  ibuf 2@ $3.00000003 2or  ibuf 2!  4here ;
:D

\ branch conditions                                    20mar94py

Create and/or-tab
       $08 c, $04 c, $02 c, $01 c,
       $1C c, $1A c, $16 c, $19 c, $15 c, $13 c,
       $1E c, $1D c, $1B c, $17 c,
       $1F c,
       $0C c, $0A c, $06 c, $09 c, $05 c, $03 c,
       $0E c, $0D c, $0B c, $07 c,
       $0F c,

: >and/or ( n -- stacks )  and/or-tab + c@ ;

: constants  0 ?DO  constant  LOOP ;

:A
hex
9 8 7 6 5 4       6 constants  0&1 0&2 0&3 1&2 1&3 2&3
D C B A           4 constants  0&1&2 0&1&3 0&2&3 1&2&3
E                   constant   0&1&2&3

14 13 12 11 10 F  6 constants  0|1 0|2 0|3 1|2 1|3 2|3
18 17 16 15       4 constants  0|1|2 0|1|3 0|2|3 1|2|3
19                  constant   0|1|2|3
decimal
:D

\ branch conditions                                    20mar94py

Create condmasks  $FFFFFFFFFF07FFFF ,
                  $FFFFFFFFFFF83FFF ,
                  $FFFFFFFFFFFFC1FF ,
                  $FFFFFFFFFFFFFE0F ,

: !cond  ( n -- )  condfield @ 3 > abort" too much conds!"
  $1F and 3 condfield @ - 5 * 4 + lshift
  ibuf cell+ @  condmasks condfield @ cells + @ and or
  ibuf cell+ !  1 condfield +!
  condfield @ 2/ 4 + instfield ! ;

\ branch conditions                                    20mar94py

: brcond ( n flag -- )  swap >and/or !cond !cond ;

: cond:  ( n -- )  Create ,
  DOES> ( n/ -- )  @  ibuf cell+ @ 3 and
  dup 2 = IF    drop condfield @ dup 0=
                IF  drop  brcond  EXIT  THEN
          ELSE  dup 0=
                IF    1 ibuf cell+ +!
                ELSE  1 <>  THEN  THEN
  abort" Misplaced condition"  !cond ;

: conds: ( end start -- )  DO  I cond:  LOOP ;

:A
$08 $00 conds:  :t   :0=  :0<  :ov  :u<  :u>  :<  :>
$10 $08 conds:  :f   :0<> :0>= :no  :u>= :u<= :>= :<=
$18 $10 conds:  ?t   ?0=  ?0<  ?ov  ?u<  ?u>  ?<  ?>
$20 $18 conds:  ?f   ?0<> ?0>= ?no  ?u>= ?u<= ?>= ?<=
:D

\ loop/branch resolve                                  19mar94py

: resolve! ( dist addr -- )
  >r  r@ op@ drop 3 and
  dup 2 =  IF    drop $3FF8 and 0  ELSE
      dup 3 =  IF    drop -8 and 0
	  r@ op@ [ cell 8 = ] [IF]
	      drop $200000000
	  [ELSE]
	      nip 2
	  [THEN]
	  and IF  swap r@ 8 + + swap  THEN
           ELSE  true abort" No Jump!"  THEN THEN
  r@ op@ 2or r> op! ;

:A
: .loop  ( addr -- )  finish?  dup >r 4here swap - 8 -
  dup $2000 u>= abort" LOOP out of range!" r> resolve! ;
: .endif ( addr -- )  finish?  dup >r 4here swap - 8 -
  dup $1000 -$1000 within abort" BR out of range!"
  r> resolve! ;

: .begin ( -- addr )  finish? 4here ;
: .until ( addr1 addr2 -- )  finish?  dup >r - 8 -
  dup $1000 -$1000 within abort" BR out of range! "
  r> resolve! ;

: +IP ( addr1 rel -- )  finish? 8 * swap resolve! ;
: >IP ( addr1 addr -- )  finish? over 8 + - swap resolve! ;
:D

\ labels                                               23may94py

Vocabulary symbols
: symbols[  symbols definitions ;
: symbols]  forth   definitions ;

: sym-lookup? ( addr len -- xt/0 )
  [ ' symbols >body ] ALiteral search-wordlist
  0= IF  0  THEN ;
: sym, ( addr len -- addr ) 2drop here 0 , ;
\  symframe cell+ 2@ + swap ( --> addr target len )
\  2dup aligned dup cell+ symframe cell+ +!
\  2dup + >r cell+ erase move r> ( --> addr ) ;
: label:  ( addr -- xt )
  symbols[ Create symbols] 0 A, , lastxt
  DOES>  ( addr -- ) dup cell+ @ @ dup
         IF  nip >IP  EXIT  THEN
         drop dup @ here rot ! A, , ;
: reveal: ( addr xt -- )  >body 2dup cell+ @ !
  BEGIN  @ dup  WHILE
         2dup cell+ @ swap >IP  REPEAT  2drop ;
: symbol,  ( addr len -- xt )
  2dup nextname  sym,
  also asmdefs  label:  previous ;
:A
: .globl  ( -- )  0 bl word count symbol, ;
:D

: is-label?  ( addr u -- flag )  drop c@ '@ >= ;
: do-label ( addr u -- )
    2dup 1- + c@ ': = dup >r +
    2dup sym-lookup? dup 0=
    IF  drop symbol,  ELSE  nip nip  THEN
    r@ IF  finish? 4here over reveal:  THEN
    r> 0= IF  execute  ELSE  drop  THEN ;
: ?label ( addr u -- )
  2dup is-label?  IF  ['] do-label EXIT  THEN
  defers interpreter-notfound1 ;

\ >call                                                09sep94py

: >call  call? 0= IF  finish  THEN  3 instfield ! ;

\ simple instructions                                  19jan94py

: alu: ( 10bit -- )  Create ,  DOES>  @ !alu ;

: readword ( -- )
  BEGIN  >in @  bl word count dup 0=
         WHILE  refill 2drop 2drop  REPEAT   2drop >in ! ;

: alus: ( start end step -- )  -rot swap
  ?DO  readword  I alu:
\       s" --" compare
\       IF  >in !  I alu:  ELSE  drop  THEN
       dup +LOOP  drop ;

:A
%0000001001 %0110001001 %100000
alus: or    add   addc   mul
      and   sub   subc   umul
      xor   subr  subcr  pass

\ s1p is default

\ mul@                                                 19jan94py

%0110100000 %0110110000 1
alus:  mul@    mul<@    mulr@    mulr<@
      -mul@   -mul<@   -mulr@   -mulr<@
       mul@+   mul<@+   mulr@+   mulr<@+
      -mul@+  -mul<@+  -mulr@+  -mulr<@+

\ flag generation                                      19jan94py

%0110110000 %0111000000 1
alus:  t   0=  0<  ov  u<  u>  <  >
       f   0<> 0>= no  u>= u<= >= <=

\ T4                                                   19jan94py

%0111000000 %0111100000 1
alus: asr    lsr    ror    rorc    asl    lsl    rol    rolc
      ff1    popc   lob    loh     extb   exth   hib    hih
      sp@    loops@ loope@ ip@     sr@    cm@    index@ flatch@
      sp!    loops! loope! ip!     sr!    cm!    index! flatch!

\ T5, floating point:                                  19jan94py

%0111100000 %0111110000 1
alus:  fadd     fsub     fmul     fnmul
       faddadd  faddsub  fmuladd  fmulsub
       fi2f     fni2f    fadd@    fmul@
       fs2d     fd2s     fxtract  fiscale

\ %0111110000 %0111110100 1
\ alus:  ext    extu   mak    clr

%0111110000 %0111110010 1  alus:  bfu  bfs
%0111110100 %0111110110 1  alus:  cc@  cc!

%0111111000 %1000000000 1
alus:  px1 px2 px4 px8
       pp1 pp2 pp4 pp8
:D

\ Stack effects                                        19jan94py

: >curstack ( 5bit -- 5bit )  lastalufield @ 2* 2* xor ;

: >stack ( alu -- )  lastalufield @
  dup 1+ instfield @ <> ABORT" Spurious stack address!"
  instfield ! !alu ;

\ pick and pin                                         21jan94py

: pin,  ( 5bit -- )  dup %10000 and
  IF    >curstack  dup %11 and swap  %01100 and
  ELSE  dup %11 and %100 + swap  %10000 %01100 within
  THEN  ABORT" Only current stack!"
  %0110000000 or >stack ;

: pick,  ( 5bit -- )
  dup %00000 %00100 within ABORT" No constant"
  %0110000000 or >stack ;

:A
%0110000000 alu: pin

: pick  ( -- )
  alu? 0= IF  finish  THEN
  instfield @ lastalufield !  %0110010000  >curstack !alu ;
:D

\ Stack addresses                                      21jan94py

: !stack ( 5bit -- )
  lastalu @ %0110000000 =  IF  pin,  EXIT  THEN
  lastalu @ %0110010000 >curstack  =  IF  pick,  EXIT  THEN
  lastalu @ %11111 and %01001 <> ABORT" Only one address!"
  lastalu @ %1111100000 and or
  dup %0110000000 u>= ABORT" no ALU instruction!" >stack ;

: stack: ( 5bit -- )  Create ,  DOES>  @ !stack ;

: stacks: ( n -- )
  0 ?DO  readword  I stack:  LOOP ;

:A
$20 stacks:  #0         #-1         #$7FFFFFFF  #$80000000
             c0         c1          c2          c3
             s0p        s1p         s2p         s3p
             s4         s5          s6          s7
             0s0        0s1         0s2         0s3
             1s0        1s1         1s2         1s3
             2s0        2s1         2s2         2s3
             3s0        3s1         3s2         3s3
:D

\ relativ to current stack                             21jan94py

: curstack: ( 5bit -- )
  Create ,  DOES>  @ >curstack !stack ;

:A
%10000 curstack: s0
%10001 curstack: s1
%10010 curstack: s2
%10011 curstack: s3

\ Abbrevations                                         21jan94py

' #$7FFFFFFF Alias #max
' #$80000000 Alias #min

\ FP abbrevations                                      21jan94py

[A]
: fabs  and #max ;
: fneg  xor #min ;
: f2*   add c3 ;
: f2/   sub c3 ;

\ ALU abbrevations                                     21jan94py

: nop   or   #0 ;
: not   xor #-1 ;
: neg   subr #0 ;
: inc   sub #-1 ;
: dec   add #-1 ;

\ Stack abbrevations                                   21jan94py

: dup   pick s0 ;
: over  pick s1 ;
: swap  pick s1p ;
: rot   pick s2p ;
: drop  pin  s0 ;
: nip   pin  s1 ;

\ ret                                                  19mar94py

: ret   ( -- ) >call ip! ;

[F]
:D

\ Literals                                             21mar94py

: !a/d  ( 10bit -- ) ?finish
    alu?  IF  $200 or !alu  ELSE  !data  THEN ;
Create lits  0. 2, 0. 2, 0. 2, 0. 2,  0. 2, 0. 2,

: bytesplit ( n -- n1 n2 )
    0 $1000000 um/mod swap 8 lshift swap ;

:A
: #  ( 8bit -- )  dup $80 -$80 within abort" out of range"
  $FF and !a/d ;
: #< ( 8bit -- )  dup $100 0  within abort" out of range"
  $100 or !a/d ;

: ## ( 32bit -- )  ?finish  3
  BEGIN  over $FF800000 and dup $FF800000 = swap 0= or  WHILE
         1- swap 8 lshift swap  dup 0= UNTIL  THEN
  swap bytesplit  dup $80 and negate or >r
  swap lits instfield @ 2* cells + 2!  r> [A] # [F] ;

: #, ( -- )  ?finish  lits instfield @ 2* cells + dup 2@ dup 0>
  IF    over 0= alu? and
        IF  dup 3 =  IF  hib  2drop  0 0 rot 2!  EXIT THEN
            dup 2 =  IF  hih  2drop  0 0 rot 2!  EXIT THEN THEN
        1- >r bytesplit #< r> rot 2!
  ELSE  2drop drop  alu? IF  nop  ELSE  0 #  THEN  THEN ;

: >sym ( "symbol" -- addr )
    bl word count sym-lookup? dup 0= abort" No symbol!"
    >body cell+ @ @ ;
:D
: >ip.b  ( -- )
    >sym 4here 8 + - ;
:A
: .ip.b#  ( -- )    >ip.b                [A] # [F] ;
: .ip.h#  ( -- )    >ip.b 2/             [A] # [F] ;
: .ip.w#  ( -- )    >ip.b 2/ 2/          [A] # [F] ;
: .ip.2#  ( -- )    >ip.b 2/ 2/ 2/       [A] # [F] ;
: .ip.4#  ( -- )    >ip.b 2/ 2/ 2/ 1+ 2/ [A] # [F] ;
' .ip.2# alias .ip.d#
' .ip.2# alias .ip.f#
' .ip.4# alias .ip.q#
' .ip.4# alias .ip.2f#
:D
Variable procstart
: >p.b  ( -- )
    >sym procstart @ - ;
:A
: .proc  finish?  4here procstart ! ;
: .p#    ( -- n )  >p.b                       ;
: .p.b#  ( -- )    >p.b             [A] # [F] ;
: .p.h#  ( -- )    >p.b 2/          [A] # [F] ;
: .p.w#  ( -- )    >p.b 2/ 2/       [A] # [F] ;
: .p.2#  ( -- )    >p.b 2/ 2/ 2/    [A] # [F] ;
: .p.4#  ( -- )    >p.b 2/ 2/ 2/ 2/ [A] # [F] ;
' .p.2# alias .p.d#
' .p.2# alias .p.f#
' .p.4# alias .p.q#
' .p.4# alias .p.2f#
: .p.b## ( -- )    >p.b             [A] ## [F] ;
: .p.h## ( -- )    >p.b 2/          [A] ## [F] ;
: .p.w## ( -- )    >p.b 2/ 2/       [A] ## [F] ;
: .p.2## ( -- )    >p.b 2/ 2/ 2/    [A] ## [F] ;
: .p.4## ( -- )    >p.b 2/ 2/ 2/ 2/ [A] ## [F] ;
' .p.2## alias .p.d##
' .p.2## alias .p.f##
' .p.4## alias .p.q##
' .p.4## alias .p.2f##
:D

\ data instructions                                    20mar94py

: cu ( -- n )  instfield @ 1- 1 and  IF  4  ELSE  8  THEN ;
: move:  ( n -- )  Create ,
  DOES> @  !data  cu  ibuf cell+ tuck @ or swap ! ;
: moves:  -rot ?DO  I move:  dup +LOOP  drop ;

:A
%0010000000 %0000000000 %100000 moves: ldb ldh ld ld2
%1010000000 %1000000000 %100000 moves: stb sth st st2

' ld2 Alias ldf
' ld2 Alias ldq
' st2 Alias stf
' st2 Alias stq
:D

\ data instructions                                    22mar94py

: ua:  ( n -- )  Create ,  DOES>  @ !data ;
: uas: ( e s i -- )  -rot ?DO  i ua:  dup +LOOP  drop ;

:A
%1000010000 %1000000000 %100 uas: R0= R1= R2= R3=
%1001000000 ua: get
%1001010000 ua: set
%1001100000 ua: getd
%1001110000 ua: setd

%1010010000 %1010000000 %100 uas: ccheck cclr cstore cflush
%1010100000 %1010010100 %100 uas: cload calloc cxlock

%1010011000 %1010010000 %100 uas: mccheck mdcheck
%1010011100 %1010011000 %001 uas: mcget mcset mchif mclof
%1010100000 %1010011100 %001 uas: mdget mdset mdhif mdlof

%1011100000 %1011000000 %100 uas: inb inh in ind outb outh out outd
%1011000011 %1011000001 %1   uas: inq ins

%1011100100 %1011100000 %1   uas: =c0  =c1  =c2  =c3

%1011101000 ua: geta
%1011111000 ua: seta
%1011101100 ua: getdrn
%1011111100 ua: setdrn
%1111101100 ua: getdmf
%1111111100 ua: setdmf

%1011100100 ua: getc
%1011110100 ua: setc
%1011100101 ua: stop
%1011110101 ua: restart
%1011100110 ua: stop1
%1011110110 ua: restart1
%1011100111 ua: halt

:D

\ data instructions                                    20mar94py

: |inst ( 10bit n -- )
  dup 0= abort" Only after moves!"
  instfield @ >r  instfield !
  instshift  ibuf 2@ 2or ibuf 2!  r> instfield ! ;
: mode:  Create ,  DOES>  @ lastmove @ |inst ;

: modes:  DO  I mode:  4 +LOOP ;
: regs:   DO  I mode:  LOOP ;

:A
$10 $04 modes: +N  N+  +N+
$20 $14 modes: +s0 s0+ +s0+

$10 $00 regs: R0 R1 R2 R3  N0 N1 N2 N3  L0 L1 L2 L3  F0 F1 F2 F3
$14 $10 regs: ip s0b ip+s0 s0l
:D

\ data instructions                                    22mar94py

: ua-only  true abort" Only for update!" ;
: umode:  >in @ >r  name sfind  r> >in !  Create
  0=  IF  ['] ua-only  THEN  swap , ,
  DOES>  dup @ lastmove @ 1 and IF  4  ELSE  8  THEN
  ibuf cell+ @ and  IF  drop cell+ @ execute  EXIT  THEN  
  lastmove @ |inst drop ;

:A
%0100000000 umode: +N
%0000010000 umode: +s0
%0000100000 umode: -N
%0000110000 umode: -s0
:D

\ data instructions                                    20mar94py

: stevnop: ( n -- )  Create ,
  DOES>  @ lastmove @ 4 <> abort" Only even stacks!"  4 |inst ;
: stoddop: ( n -- )  Create ,
  DOES>  @ lastmove @ 5 <> abort" Only odd stacks!" 5 |inst ;

: stevnops: ( end start disp -- )  -rot
  DO  I stevnop:  dup +LOOP  drop ;
: stoddops: ( end start disp -- )  -rot
  DO  I stoddop:  dup +LOOP  drop ;

:A
%1000000000 %0000000000 %0010000000  stevnops: 0: 0&2: 2: 2&0:
%1000000000 %0000000000 %0010000000  stoddops: 1: 1&3: 3: 3&1:
:D

\ data definition instructions                         24apr94py

Defer normal-mode
Defer char-mode

: number-mode ( n dest char -- n' dest' )
\ ." Number: " dup emit cr
  dup toupper digit?
  IF  nip rot base @ * + dup $10000 >=
      IF  normal-mode $100  THEN  swap EXIT  THEN
  >r tuck caddr 'c! 1+ $100 swap r> normal-mode ;

: esc-mode ( dest char -- dest' )
\ ." Escape: " dup emit cr
  dup 'n = IF  drop #lf  normal-mode  EXIT  THEN
  dup 't = IF  drop #tab normal-mode  EXIT  THEN
  dup 'x = IF  drop hex ['] number-mode IS char-mode   EXIT THEN
  dup '0 '8 within
  IF  8 base ! ['] number-mode IS char-mode char-mode  EXIT THEN
  $100 + normal-mode ;

: (normal-mode) ( dest char -- dest' )
\ ." Char  : " dup emit cr
  dup '\ = IF  drop ['] esc-mode IS char-mode  EXIT  THEN
  over caddr 'c! 1+ ['] normal-mode IS char-mode ;
' (normal-mode) IS normal-mode

: \move  ( addr len dest -- dest+n )
  base @ >r  ['] normal-mode IS char-mode
  $100 swap 2swap bounds  ?DO  I c@ char-mode  LOOP
  over $FF and 0> IF  tuck caddr 'c! 1+  ELSE  nip  THEN
  r> base ! ;

: byte,   4there caddr  'c!         1 4allot ;
: short,  $100 /mod 4there waddr  'c!
              4there  waddr 1+ 'c!  2 4allot ;
: int,    4there laddr   '!         4 4allot ;
: long,   4there laddr   '!         4 4allot ;
: quad,   op, ;
\ : float,  4there laddr 'SF!  1 cells  4allot ;
\ : double, 4there        'F!  1 floats 4allot ;

: ascii,  4there \move 4there - 4allot ;

:A
: .align ( "n[,m]" -- )   0 0 name >number
  dup IF  over c@ ', =
          IF  1 /string parser  0 0  THEN  THEN
  2drop  1 rot lshift  4here over 1- >r - r> and
  0 ?DO  dup 4there caddr 'c!  1 4allot  LOOP  drop ;

: .(  ') parse also Forth evaluate previous ;

: .byte   parse-name parser byte,   ;
: .short  parse-name parser short,  ;
: .int    parse-name parser int,    ;
: .long   parse-name parser long,   ;
: .quad   parse-name s>number dpl @ 0= abort" Not a number" quad, ;
\ : .float  parse-name >float 0= abort" Not a FP number" float,  ;
\ : .double parse-name >float 0= abort" Not a FP number" double, ;

: .ascii  '" parse 2drop
  source  >in @ /string  over  swap
  BEGIN  '"  scan   over 1- c@ '\ = over 0<> and  WHILE
         1 /string  REPEAT  >r
  over - dup r> IF 1+ THEN  >in +! ascii, ;

: .macro      finish?  also asmdefs also asm4stack definitions
              : ;
: .end-macro  postpone ; previous previous ; immediate restrict

: .include    include ;

: .times{  ( n -- input n )
  dup >r 1 > IF  save-input  THEN  r> ;
: .}times  ( input n -- input n-1 / 1 / )
  1- dup 0>
  IF  >r restore-input throw r@ 1 >
      IF  save-input  THEN  r>
  THEN ;
:D

\ save assembler output                                25apr94py

: (fdump ( handle link -- )  2dup >r swap
  3 cells + @  dup  IF  recurse  ELSE  2drop  THEN
  r@ cell+ @ 0=  IF  rdrop drop  EXIT  THEN
\ cr ." Writing " r@ @ . ." len " r@ cell+ @ .
  r@ cell+ @ 7 + -8 and r@ cell+ !
  r@ 4 2 pick write-file throw
  r@ cell+ 4 2 pick write-file throw
  r@ cell+ cell+ @  dup 7 and 2 =  IF  2drop rdrop  EXIT  THEN
  r> cell+ @  rot write-file throw ;

Create 4magic  ," 4stack00"

\ end of assembler

Variable old-notfound

:A
: F' ' ;

also Forth definitions

: (code)
    also asm4stack
    s" F' 2@ F' 2! F' c! F' ! F' here F' allot" evaluate
    IS 4allot  IS 4here  IS  '! IS  'c!  IS '2!  IS '2@
    What's interpreter-notfound1 old-notfound !
    ['] ?label IS interpreter-notfound1 ;
: label (code) 4here label: drop asm4stack depth ;
: (end-code) previous old-notfound @ IS interpreter-notfound1 ;

previous previous previous Forth

