\ assembler in forth for alpha

\ Copyright (C) 1999,2000,2007 Free Software Foundation, Inc.

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

\ contributed by Bernd Thallner

require ./../../code.fs

get-current
also assembler definitions

\ register

 $0 constant v0
 $1 constant t0
 $2 constant t1
 $3 constant t2
 $4 constant t3
 $5 constant t4
 $6 constant t5
 $7 constant t6
 $8 constant t7
 $9 constant s0
 $a constant s1
 $b constant s2
 $c constant s3
 $d constant s4
 $e constant s5
 $f constant fp
\ commented out to avoid shadowing hex numbers
\  $10 constant a0
\  $11 constant a1
\  $12 constant a2
\  $13 constant a3
\  $14 constant a4
\  $15 constant a5
$16 constant t8
$17 constant t9
$18 constant t10
$19 constant t11
$1a constant ra
$1b constant t12
$1c constant at
$1d constant gp
$1e constant sp
$1f constant zero

\ util

: h@ ( addr -- n )		\ 32 bit fetch
dup dup aligned = if
  @
  $00000000ffffffff and
else
  4 - @
  $20 rshift
endif
;

: h! ( n addr -- )		\ 32 bit store
dup dup aligned = if
  dup @
  $ffffffff00000000 and
  rot or
  swap !
else
  4 - dup @
  $00000000ffffffff and
  rot $20 lshift or
  swap !
endif
;

: h, ( h -- )			\ 32 bit store + allot
here here aligned = if
  here !
else
  32 lshift
  here 4 - dup
  @ rot or
  swap !
endif
4 allot
;

\ operands

: check-range ( u1 u2 u3 -- )
    within 0= -24 and throw ;

: rega ( rega code -- code )
    \ ra field, named rega to avoid conflict with register ra
    swap dup 0 $20 check-range
    21 lshift or ;

: rb ( rb code -- code )
    swap dup 0 $20 check-range
    16 lshift or ;

: rc ( rc code -- code )
    swap dup 0 $20 check-range
    or ;

: hint ( addr code -- code )
    swap 2 rshift $3fff and or ;

: disp ( n code -- code )
    swap dup -$8000 $8000 check-range
    $ffff and or ;

: branch-rel ( n code -- code )
    swap dup 3 and 0<> -24 and throw
    2/ 2/
    dup -$100000 $100000 check-range
    $1fffff and or ;

: branch-disp ( addr code -- code )
    swap here 4 + - swap branch-rel ;

: imm ( u code -- code )
    swap dup 0 $100 check-range
    13 lshift or ;

: palcode ( u code -- code )
    swap dup 0 $4000000 check-range or ;

\ formats

: Bra ( opcode -- )			\ branch instruction format
    create 26 lshift ,
does> ( rega target-addr -- )
    @ branch-disp rega h, ;

: Mbr ( opcode hint -- )		\ memory branch instruction format
    create 14 lshift swap 26 lshift or ,
does> ( rega rb hint -- )
    @ hint rb rega h, ; 

: F-P ( opcode func -- )	\ floating-point operate instruction format
    create 5 lshift swap 26 lshift or ,
does> ( fa fb fc -- )
    @ rc rb rega h, ;

: Mem ( opcode -- )		\ memory instruction format
  create 26 lshift ,
does> ( rega memory_disp rb -- )
  @ rb disp rega h, ;

: Mfc ( opcode func -- )	\ memory instruction with function code format
  create swap 26 lshift or ,
does> ( rega rb -- )
  @ rb rega h, ;

: Opr ( opcode.ff )		\ operate instruction format
  create 5 lshift swap 26 lshift or ,
does> ( rega rb rc -- )
  @ rc rb rega h, ;

: Opr# ( opcode func -- )		\ operate instruction format
  create 5 lshift swap 26 lshift or 1 12 lshift or ,
does> ( rega imm rc -- )
  @ rc imm rega h, ;

: Pcd ( opcode -- )		\ palcode instruction format
  create 26 lshift ,
does> ( palcode addr -- )
  @ palcode h, ;

\ instructions

$15 $80   F-P  addf,
$15 $a0   F-P  addg,
$10 $00   Opr  addl,
$10 $00   Opr# addl#,
$10 $40   Opr  addlv,
$10 $40   Opr# addlv#,
$10 $20   Opr  addq,
$10 $20   Opr# addq#,
$10 $60   Opr  addqv,
$10 $60   Opr# addqv#,
$16 $80   F-P  adds,
$16 $a0   F-P  addt,
$11 $00   Opr  and,
$11 $00   Opr# and#,
$39       Bra  beq,
$3e       Bra  bge,
$3f       Bra  bgt,
$11 $08   Opr  bic,
$11 $08   Opr# bic#,
$11 $20   Opr  bis,
$11 $20   Opr# bis#,
$38       Bra  blbc,
$3c       Bra  blbs,
$3b       Bra  ble,
$3a       Bra  blt,
$3d       Bra  bne, 
$30       Bra  br,
$34       Bra  bsr,
$00       Pcd  call_pal,
$11 $24   Opr  cmoveq,
$11 $24   Opr# cmoveq#,
$11 $46   Opr  cmovge,
$11 $46   Opr# cmovge#,
$11 $66   Opr  cmovgt,
$11 $66   Opr# cmovgt#,
$11 $16   Opr  cmovlbc,
$11 $16   Opr# cmovlbc#,
$11 $14   Opr  cmovlbs,
$11 $14   Opr# cmovlbs#,
$11 $64   Opr  cmovle,
$11 $64   Opr# cmovle#,
$11 $44   Opr  cmovlt,
$11 $44   Opr# cmovlt#,
$11 $26   Opr  cmovne,
$11 $26   Opr# cmovne#,
$10 $0f   Opr  cmpbge,
$10 $0f   Opr# cmpbge#,
$10 $2d   Opr  cmpeq,
$10 $2d   Opr# cmpeq#,
$15 $a5   F-P  cmpgeq,
$15 $a7   F-P  cmpgle,
$15 $a6   F-P  cmpglt,
$10 $6d   Opr  cmple,
$10 $6d   Opr# cmple#,
$10 $4d   Opr  cmplt,
$10 $4d   Opr# cmplt#,
$16 $a5   F-P  cmpteq,
$16 $a7   F-P  cmptle,
$16 $a6   F-P  cmptlt,
$16 $a4   F-P  cmptun,
$10 $3d   Opr  cmpule,
$10 $3d   Opr# cmpule#,
$10 $1d   Opr  cmpult,
$10 $1d   Opr# cmpult#,
$17 $20   F-P  cpys,
$17 $22   F-P  cpyse,
$17 $21   F-P  cpysn,
$15 $9e   F-P  cvtdg,
$15 $ad   F-P  cvtgd,
$15 $ac   F-P  cvtgf,
$15 $af   F-P  cvtgq,
$17 $10   F-P  cvtlq,
$15 $bc   F-P  cvtqf,
$15 $be   F-P  cvtqg,
$17 $30   F-P  cvtql,
$17 $530  F-P  cvtqlsv,
$17 $130  F-P  cvtqlv,
$16 $bc   F-P  cvtqs,
$16 $be   F-P  cvtqt,
$16 $2ac  F-P  cvtst,
$16 $af   F-P  cvttq,
$16 $ac   F-P  cvtts,
$15 $83   F-P  divf,
$15 $a3   F-P  divg,
$16 $83   F-P  divs,
$16 $a3   F-P  divt,
$11 $48   Opr  eqv,
$11 $48   Opr# eqv#,
$18 $400  Mfc  excb,
$12 $06   Opr  extbl,
$12 $06   Opr# extbl#,
$12 $6a   Opr  extlh,
$12 $6a   Opr# extlh#,
$12 $26   Opr  extll,
$12 $26   Opr# extll#,
$12 $7a   Opr  extqh,
$12 $7a   Opr# extqh#,
$12 $36   Opr  extql,
$12 $36   Opr# extql#,
$12 $5a   Opr  extwh,
$12 $5a   Opr# extwh#,
$12 $16   Opr  extwl,
$12 $16   Opr# extwl#,
$31       Bra  fbeq,
$36       Bra  fbge,
$37       Bra  fbgt,
$33       Bra  fble,
$32       Bra  fblt,
$35       Bra  fbne,
$17 $2a   F-P  fcmoveq,
$17 $2d   F-P  fcmovge,
$17 $2f   F-P  fcmovgt,
$17 $2e   F-P  fcmovle,
$17 $2c   F-P  fcmovlt,
$17 $2b   F-P  fcmovne,
$18 $8000 Mfc  fetch,
$18 $a000 Mfc  fetch_m,
$12 $0b   Opr  insbl,
$12 $0b   Opr# insbl#,
$12 $67   Opr  inslh,
$12 $67   Opr# inslh#,
$12 $2b   Opr  insll,
$12 $2b   Opr# insll#,
$12 $77   Opr  insqh,
$12 $77   Opr# insqh#,
$12 $3b   Opr  insql,
$12 $3b   Opr# insql#,
$12 $57   Opr  inswh,
$12 $57   Opr# inswh#,
$12 $1b   Opr  inswl,
$12 $1b   Opr# inswl#,
$1a $00   Mbr  jmp,
$1a $01   Mbr  jsr,
$1a $03   Mbr  jsr_coroutine,
$08       Mem  lda,
$09       Mem  ldah,
$20       Mem  ldf,
$21       Mem  ldg,
$28       Mem  ldl,
$2a       Mem  ldl_l,
$29       Mem  ldq,
$2b       Mem  ldq_l,
$0b       Mem  ldq_u,
$22       Mem  lds,
$23       Mem  ldt,
$18 $4000 Mfc  mb,
$17 $25   F-P  mf_fpcr,
$12 $02   Opr  mskbl,
$12 $02   Opr# mskbl#,
$12 $62   Opr  msklh,
$12 $62   Opr# msklh#,
$12 $22   Opr  mskll,
$12 $22   Opr# mskll#,
$12 $72   Opr  mskqh,
$12 $72   Opr# mskqh#,
$12 $32   Opr  mskql,
$12 $32   Opr# mskql#,
$12 $52   Opr  mskwh,
$12 $52   Opr# mskwh#,
$12 $12   Opr  mskwl,
$12 $12   Opr# mskwl#,
$17 $24   F-P  mt_fpcr,
$15 $82   F-P  mulf,
$15 $a2   F-P  mulg,
$13 $00   Opr  mull,
$13 $00   Opr# mull#,
$13 $40   Opr  mullv,
$13 $40   Opr# mullv#,
$13 $20   Opr  mullq,
$13 $20   Opr# mullq#,
$13 $60   Opr  mullqv,
$13 $60   Opr# mullqv#,
$16 $82   F-P  mulls,
$16 $a2   F-P  mullt,
$11 $28   Opr  ornot,
$11 $28   Opr# ornot#,
$18 $e000 Mfc  rc,
$1a $02   Mbr  ret,
$18 $c000 Mfc  rpcc,
$18 $f000 Mfc  rs,
$10 $02   Opr  s4addl,
$10 $02   Opr# s4addl#,
$10 $22   Opr  s4addq,
$10 $22   Opr# s4addq#,
$10 $0b   Opr  s4subl,
$10 $0b   Opr# s4subl#,
$10 $2b   Opr  s4subq,
$10 $2b   Opr# s4subq#,
$10 $12   Opr  s8addl,
$10 $12   Opr# s8addl#,
$10 $32   Opr  s8addq,
$10 $32   Opr# s8addq#,
$10 $1b   Opr  s8ubl,
$10 $1b   Opr# s8ubl#,
$10 $3b   Opr  s8ubq,
$10 $3b   Opr# s8ubq#,
$12 $39   Opr  sll,
$12 $39   Opr# sll#,
$12 $3c   Opr  sra,
$12 $3c   Opr# sra#,
$12 $34   Opr  srl,
$12 $34   Opr# srl#,
$24       Mem  stf,
$25       Mem  stg,
$26       Mem  sts,
$2c       Mem  stl,
$2e       Mem  stl_c,
$2d       Mem  stq,
$2f       Mem  stq_c,
$0f       Mem  stq_u,
$27       Mem  stt,
$15 $81   F-P  subf,
$15 $a1   F-P  subg,
$10 $09   Opr  subl,
$10 $09   Opr# subl#,
$10 $49   Opr  sublv,
$10 $49   Opr# sublv#,
$10 $29   Opr  subq,
$10 $29   Opr# subq#,
$10 $69   Opr  subqv,
$10 $69   Opr# subqv#,
$16 $81   F-P  subs,
$16 $a1   F-P  subt,
$18 $00   Mfc  trapb,
$13 $30   Opr  umulh,
$13 $30   Opr# umulh#,
$18 $4400 Mfc  wmb,
$11 $40   Opr  xor,
$11 $40   Opr# xor#,
$12 $30   Opr  zap,
$12 $30   Opr# zap#,
$12 $31   Opr  zapnot,
$12 $31   Opr# zapnot#,

\ conditions; they are reversed because of the if and until logic (the
\ stuff enclosed by if is performed if the branch around has the
\ inverse condition).

' beq,  constant ne
' bge, 	constant lt
' bgt, 	constant le
' blbc,	constant lbs
' blbs,	constant lbc
' ble, 	constant gt
' blt,  constant ge
' bne,  constant eq
' fbeq, constant fne
' fbge, constant flt
' fbgt, constant fle
' fble, constant fgt
' fblt, constant fge
' fbne, constant feq

\ control structures

: magic-asm ( u1 u2 -- u3 u4 )
    \ turns a magic number into an asm-magic number or back
    $fedcba0987654321 xor ;

: patch-branch ( behind-branch-addr target-addr -- )
    \ there is a branch just before behind-branch-addr; PATCH-BRANCH
    \ patches this branch to branch to target-addr
    over - ( behind-branch-addr rel )
    swap 4 - dup >r ( rel branch-addr R:branch-addr )
    h@ branch-rel r> h! ; \ !! relies on the imm field being 0 before

: if, ( reg xt -- asm-orig )
    \ xt is for a branch word ( reg addr -- )
    here 4 + swap execute \ put 0 into the disp field
    here live-orig magic-asm live-orig ;

: ahead, ( -- asm-orig )
    zero ['] br, if, ;

: then, ( asm-orig -- )
    orig? magic-asm orig?
    here patch-branch ;

: begin, ( -- asm-dest )
    here dest magic-asm dest ;

: until, ( asm-dest reg xt -- )
    \ xt is a condition ( reg addr -- )
    here 4 + swap execute
    dest? magic-asm dest?
    here swap patch-branch ;

: again, ( asm-dest -- )
    zero ['] br, until, ;

: while, ( asm-dest -- asm-orig asm-dest )
    if, 1 cs-roll ;

: else, ( asm-orig1 -- asm-orig2 )
    ahead, 1 cs-roll then, ;

: repeat, ( asm-orig asm-dest -- )
    again, then, ;

: endif, ( asm-orig -- )
    then, ;

\  \ jump marks

\  \ example:

\  \ init_marktbl		\ initializes mark table
\  \ 31 0 br,
\  \ 0 store_branch	\ store jump address for mark 0
\  \ 1 2 3 addf,
\  \ 0 set_mark		\ store mark 0
\  \ 2 3 4 addf,
\  \ 2 0 beq,
\  \ 0 store_branch	\ store jump address for mark 0
\  \ calculate_marks       \ calculate all jumps

\  \ with <mark_address> <jump_address> calculate_branch you can calculate the
\  \ displacement field without the mark_table for one branch

\  \ example:
\  \ here 31 0 br,
\  \ here 1 2 3 addf,
\  \ calculate_branch

\  5 constant mark_numbers
\  5 constant mark_uses

\  create mark_table
\  mark_numbers mark_uses 1+ * cells allot

\  : init_marktbl ( -- )			\ initializes mark table
\    mark_table mark_numbers mark_uses 1+ * cells +
\    mark_table
\    begin
\      over over >
\    while
\      dup 0 swap !
\      1 cells +
\    repeat
\    drop drop
\  ;

\  : set_mark ( mark_number -- )		\ sets mark, store address in mark table
\    dup mark_numbers >= abort" error, illegal mark number"
\    mark_uses 1+ * cells
\    mark_table + here 8 - swap !
\  ;

\  : store_branch ( mark_number -- )	\ stores address of branch in mark table
\    dup mark_numbers >= abort" error, illegal mark number"
\    mark_uses 1+ * cells
\    mark_table + 1 cells +
\    dup mark_uses cells + swap
\    begin
\      over over > over @ and 
\    while
\      1 cells +
\    repeat
\    swap over = abort" error, not enough space in mark_table, increase mark_uses"
\    here 4 - swap !
\  ;

\  : calculate_branch ( mark_addr branch_addr -- ) \ calculate branch displacement field for one branch
\    swap over - 4 + 4 /
\    $1fffff and
\    over h@ or swap h!
\  ;

\  : calculate_mark ( tb mark_address -- tb )	\ calculates branch displacement field for one mark
\    over 1 cells +
\    dup mark_uses cells + swap
\    begin
\      over over >
\    while
\      2over swap drop ( ei i markaddr ej j markaddr )
\      over @
\      dup if
\        calculate_branch
\      else
\        drop drop
\      endif
\      1 cells +
\    repeat drop drop drop
\  ;

\  : calculate_marks ( -- )		\ calculates branch displacement field for all marks
\    mark_table mark_numbers 1- mark_uses 1+ * cells +
\    mark_table
\    begin
\      over over >=
\    while
\      dup @
\        dup if \ used mark
\          calculate_mark
\        else
\          drop
\        endif
\      mark_uses 1+ cells +
\    repeat
\    drop drop
\  ;

previous set-current



