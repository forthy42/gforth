
\ bernd thallner 9725890 881
\ assembler in forth for alpha

\ requires code.fs

\ also assembler definitions

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
$10 constant a0
$11 constant a1
$12 constant a2
$13 constant a3
$14 constant a4
$15 constant a5
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

: right_shift ( a n -- a>>=n )
0
?do
  2/
loop
;

: left_shift ( a n -- a<<=n )
0
?do
  2*
loop
;

: h@ ( addr -- n )		\ 32 bit fetch
dup dup aligned = if
  @
  $00000000ffffffff and
else
  4 - @
  $20 right_shift
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
  rot $20 left_shift or
  swap !
endif
;

: h, ( h -- )			\ 32 bit store + allot
here here aligned = if
  here !
else
  32 left_shift
  here 4 - dup
  @ rot or
  swap !
endif
4 allot
;

\ format

: Bra ( oo )			\ branch instruction format
  create ,
does> ( ra, branch_disp, addr )
  @ 26 left_shift
  swap $1fffff and or
  swap $1f and 21 left_shift or h,
;

: Mbr ( oo.h )			\ memory branch instruction format
  create 2,
does> ( ra, rb, hint, addr )
  2@ 14 left_shift
  swap 26 left_shift or
  swap $3fff and or
  swap $1f and 16 left_shift or
  swap $1f and 21 left_shift or
  h,
; 

: F-P ( oo.fff )		\ floating-point operate instruction format
  create 2,
does> ( fa, fb, fc, addr )
  2@ 5 left_shift
  swap 26 left_shift or
  swap $1f and or
  swap $1f and 16 left_shift or
  swap $1f and 21 left_shift or
  h,
;

: Mem ( oo )			\ memory instruction format
  create ,
does> ( ra, memory_disp, rb, addr )
  @ 26 left_shift
  swap $1f and 16 left_shift or
  swap $ffff and or 
  swap $1f and 21 left_shift or
  h,
;

: Mfc ( oo.ffff )		\ memory instruction with function code format
  create 2,
does> ( ra, rb, addr )
  2@
  swap 26 left_shift or
  swap $1f and 16 left_shift or
  swap $1f and 21 left_shift or
  h,
;

: Opr ( oo.ff )			\ operate instruction format
  create 2,
does> ( ra, rb, rc, addr )
  2@
  5 left_shift
  swap 26 left_shift or
  swap $1f and or
  swap $1f and 16 left_shift or
  swap $1f and 21 left_shift or
  h, 
;

: Opr# ( oo.ff )		\ operate instruction format
  create 2,
does> ( ra, lit, rc, addr )
  2@
  5 left_shift
  swap 26 left_shift or
  1 12 left_shift or
  swap $1f and or
  swap $ff and 13 left_shift or
  swap $1f and 21 left_shift or
  h, 
;

: Pcd ( oo )			\ palcode instruction format
  create ,
does> ( palcode, addr )
  @ 26 left_shift
  swap $3ffffff and or
  h,
;

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

\ structures

\ <register_number> if, <if_code> [ else, <else_code> ] endif,

: if,
  0 beq, here 4 -
;

: else,
  dup here swap - 4 /
  $1fffff and
  over h@ or swap h!
  31 0 br,
  here 4 -
;

: endif,
  dup here swap - 4 - 4 /
  $1fffff and
  over h@ or swap h!
;

\ begin, <code> again,

: begin,
  here
;

: again,
  here - 4 - 4 /
  $1fffff and
  31 swap br,
;

\ begin, <code> <register_number> until,

: until,
  here rot swap - 4 - 4 /
  $1fffff and
  bne,
;

\ begin, <register_number> while, <code> repeat,

: while,
  0 beq, here 4 -
;

: repeat,
  swap here - 4 - 4 /
  $1fffff and
  31 swap br,
  dup here 4 - swap - 4 /
  $1fffff and
  over h@ or swap h!
;

\ labels

10 constant mark_numbers
10 constant mark_uses

create mark_table
mark_numbers mark_uses 1 + * cells allot

: set_mark ( mark_number -- )

;

: set_branch ( mark_number -- )

;

: calculate_marks ( -- )

;







