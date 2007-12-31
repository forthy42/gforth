\ disassembler in forth for alpha

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

\ util

\ require asm.fs

\  : h@ ( addr -- n )  \ 32 bit fetch
\  dup dup aligned = if
\    @
\    $00000000ffffffff and
\  else
\    4 - @
\    $20 rshift
\  endif
\  ;

also assembler
vocabulary disassembler
get-current
also disassembler definitions

create string_table
1000 allot

\ makes an table entry with following data structure
\ 64 start address in string_table 48 strlen 32 format (cOpc, cBra, cF-P, cMem, cMfc, cMbr, cOpr, cPcd) 0

: mktbentry, { start format straddr strlen -- start }  \ make table entry
  straddr string_table start + strlen cmove
  start 48 lshift
  strlen 32 lshift or
  format or
  ,
  start strlen +
;

\ prints the string from stringtable
\ table_entry = 64 start address in string_table 48 strlen 32 unused 0

: print_string ( table_entry -- )  \ print string entry
  dup
  48 rshift string_table +
  swap
  32 rshift $000000000000ffff and
  type
;

\ Opr tab0 opcode 10.xxx
\ Opr tab1 opcode 11.xxx
\ Opr tab2 opcode 12.xxx
\ Opr tab3 opcode 13.xxx

\ F-P tab0 opcode 15.xxx
\ F-P tab1 opcode 16.xxx
\ F-P tab2 opcode 17.xxx

: tab0 2* 2* ;
: tab1 2* 2* 1 + ;
: tab2 2* 2* 2 + ;
: tab3 2* 2* 3 + ;

0 \ string_table offset

create Opr_list

$00 tab0 s" addl"          mktbentry,
$40 tab0 s" addlv"         mktbentry,
$20 tab0 s" addq"          mktbentry,
$60 tab0 s" addqv"         mktbentry,
$0f tab0 s" cmpbge"        mktbentry,
$2d tab0 s" cmpeq"         mktbentry,
$6d tab0 s" cmple"         mktbentry,
$4d tab0 s" cmplt"         mktbentry,
$3d tab0 s" cmpule"        mktbentry,
$1d tab0 s" cmpult"        mktbentry,
$02 tab0 s" s4addl"        mktbentry,
$22 tab0 s" s4addq"        mktbentry,
$0b tab0 s" s4subl"        mktbentry,
$2b tab0 s" s4subq"        mktbentry,
$12 tab0 s" s8addl"        mktbentry,
$32 tab0 s" s8addq"        mktbentry,
$1b tab0 s" s8ubl"         mktbentry,
$3b tab0 s" s8ubq"         mktbentry,
$09 tab0 s" subl"          mktbentry,
$49 tab0 s" sublv"         mktbentry,
$29 tab0 s" subq"          mktbentry,
$69 tab0 s" subqv"         mktbentry,

$00 tab1 s" and"           mktbentry,
$08 tab1 s" bic"           mktbentry,
$20 tab1 s" bis"           mktbentry,
$24 tab1 s" cmoveq"        mktbentry,
$46 tab1 s" cmovge"        mktbentry,
$66 tab1 s" cmovgt"        mktbentry,
$16 tab1 s" cmovlbc"       mktbentry,
$14 tab1 s" cmovlbs"       mktbentry,
$64 tab1 s" cmovle"        mktbentry,
$44 tab1 s" cmovlt"        mktbentry,
$26 tab1 s" cmovne"        mktbentry,
$48 tab1 s" eqv"           mktbentry,
$28 tab1 s" ornot"         mktbentry,
$40 tab1 s" xor"           mktbentry,

$06 tab2 s" extbl"         mktbentry,
$6a tab2 s" extlh"         mktbentry,
$26 tab2 s" extll"         mktbentry,
$7a tab2 s" extqh"         mktbentry,
$36 tab2 s" extql"         mktbentry,
$5a tab2 s" extwh"         mktbentry,
$16 tab2 s" extwl"         mktbentry,
$0b tab2 s" insbl"         mktbentry,
$67 tab2 s" inslh"         mktbentry,
$2b tab2 s" insll"         mktbentry,
$77 tab2 s" insqh"         mktbentry,
$3b tab2 s" insql"         mktbentry,
$57 tab2 s" inswh"         mktbentry,
$1b tab2 s" inswl"         mktbentry,
$02 tab2 s" mskbl"         mktbentry,
$62 tab2 s" msklh"         mktbentry,
$22 tab2 s" mskll"         mktbentry,
$72 tab2 s" mskqh"         mktbentry,
$32 tab2 s" mskql"         mktbentry,
$52 tab2 s" mskwh"         mktbentry,
$12 tab2 s" mskwl"         mktbentry,
$39 tab2 s" sll"           mktbentry,
$3c tab2 s" sra"           mktbentry,
$34 tab2 s" srl"           mktbentry,
$30 tab2 s" zap"           mktbentry,
$31 tab2 s" zapnot"        mktbentry,

$00 tab3 s" mull"          mktbentry,
$20 tab3 s" mullq"         mktbentry,
$30 tab3 s" umulh"         mktbentry,
$40 tab3 s" mullv"         mktbentry,
$60 tab3 s" mullqv"        mktbentry,

create Mfc_list

$0000 s" trapb"            mktbentry,
$0400 s" excb"             mktbentry,
$4000 s" mb"               mktbentry,
$4400 s" wmb"              mktbentry,
$8000 s" fetch"            mktbentry,
$a000 s" fetch_m"          mktbentry,
$c000 s" rpcc"             mktbentry,
$e000 s" rc"               mktbentry,
$f000 s" rs"               mktbentry,

create Mbr_table

( 00 ) 0 s" jmp"           mktbentry, 
( 01 ) 0 s" jsr"           mktbentry,
( 02 ) 0 s" ret"           mktbentry,
( 03 ) 0 s" jsr_coroutine" mktbentry,

create F-P_list

$080 tab0 s" addf"         mktbentry,
$081 tab0 s" subf"         mktbentry,
$082 tab0 s" mulf"         mktbentry,
$083 tab0 s" divf"         mktbentry,
$09e tab0 s" cvtdg"        mktbentry,
$0a0 tab0 s" addg"         mktbentry,
$0a1 tab0 s" subg"         mktbentry,
$0a2 tab0 s" mulg"         mktbentry,
$0a3 tab0 s" divg"         mktbentry,
$0a5 tab0 s" cmpgeq"       mktbentry,
$0a6 tab0 s" cmpglt"       mktbentry,
$0a7 tab0 s" cmpgle"       mktbentry,
$0ac tab0 s" cvtgf"        mktbentry,
$0ad tab0 s" cvtgd"        mktbentry,
$0af tab0 s" cvtgq"        mktbentry,
$0bc tab0 s" cvtqf"        mktbentry,
$0be tab0 s" cvtqg"        mktbentry,

$080 tab1 s" adds"         mktbentry,
$081 tab1 s" subs"         mktbentry,
$082 tab1 s" mulls"        mktbentry,
$083 tab1 s" divs"         mktbentry,
$0a0 tab1 s" addt"         mktbentry,
$0a1 tab1 s" subt"         mktbentry,
$0a2 tab1 s" mullt"        mktbentry,
$0a3 tab1 s" divt"         mktbentry,
$0a4 tab1 s" cmptun"       mktbentry,
$0a5 tab1 s" cmpteq"       mktbentry,
$0a6 tab1 s" cmptlt"       mktbentry,
$0a7 tab1 s" cmptle"       mktbentry,
$0ac tab1 s" cvtts"        mktbentry,
$0af tab1 s" cvttq"        mktbentry,
$0bc tab1 s" cvtqs"        mktbentry,
$0be tab1 s" cvtqt"        mktbentry,
$2ac tab1 s" cvtst"        mktbentry,

$010 tab2 s" cvtlq"        mktbentry,
$020 tab2 s" cpys"         mktbentry,
$021 tab2 s" cpysn"        mktbentry,
$022 tab2 s" cpyse"        mktbentry,
$024 tab2 s" mt_fpcr"      mktbentry,
$025 tab2 s" mf_fpcr"      mktbentry,
$02a tab2 s" fcmoveq"      mktbentry,
$02b tab2 s" fcmovne"      mktbentry,
$02c tab2 s" fcmovlt"      mktbentry,
$02d tab2 s" fcmovge"      mktbentry,
$02e tab2 s" fcmovle"      mktbentry,
$02f tab2 s" fcmovgt"      mktbentry,
$030 tab2 s" cvtql"        mktbentry,
$130 tab2 s" cvtqlv"       mktbentry,
$530 tab2 s" cvtqlsv"      mktbentry,

create register_table

( 00 ) 0 s" v0"           mktbentry,
( 01 ) 0 s" t0"           mktbentry,
( 02 ) 0 s" t1"           mktbentry,
( 03 ) 0 s" t2"           mktbentry,
( 04 ) 0 s" t3"           mktbentry,
( 05 ) 0 s" t4"           mktbentry,
( 06 ) 0 s" t5"           mktbentry,
( 07 ) 0 s" t6"           mktbentry,
( 08 ) 0 s" t7"           mktbentry,
( 09 ) 0 s" s0"           mktbentry,
( 0a ) 0 s" s1"           mktbentry,
( 0b ) 0 s" s2"           mktbentry,
( 0c ) 0 s" s3"           mktbentry,
( 0d ) 0 s" s4"           mktbentry,
( 0e ) 0 s" s5"           mktbentry,
( 0f ) 0 s" fp"           mktbentry,
( 10 ) 0 s" a0"           mktbentry,
( 11 ) 0 s" a1"           mktbentry,
( 12 ) 0 s" a2"           mktbentry,
( 13 ) 0 s" a3"           mktbentry,
( 14 ) 0 s" a4"           mktbentry,
( 15 ) 0 s" a5"           mktbentry,
( 16 ) 0 s" t8"           mktbentry,
( 17 ) 0 s" t9"           mktbentry,
( 18 ) 0 s" t10"          mktbentry,
( 19 ) 0 s" t11"          mktbentry,
( 1a ) 0 s" ra"           mktbentry,
( 1b ) 0 s" t12"          mktbentry,
( 1c ) 0 s" at"           mktbentry,
( 1d ) 0 s" gp"           mktbentry,
( 1e ) 0 s" sp"           mktbentry,
( 1f ) 0 s" zero"         mktbentry,

defer decode_register

: decode_register_symb ( register -- )
  cells register_table +
  @ print_string $20 emit
;

: decode_register_number ( register -- )
  .
;

' decode_register_number is decode_register
\ ' decode_register_symb is decode_register

create decode_code

: decode_Opc ( instruction tbentry -- )
  print_string drop
;

: decode_Bra ( addr instruction tbentry -- addr )
  swap
  dup $03e00000 and 21 rshift decode_register
  $001fffff and 2* 2* 2 pick + 4 + hex.
  print_string
;

: decode_F-P ( instruction tbentry -- )
  drop
  dup $03e00000 and 21 rshift decode_register
  dup $001f0000 and 16 rshift decode_register
  dup $0000001f and decode_register
  dup 26 rshift $15 -
  swap $0000fff0 and 3 rshift or F-P_list
  begin
    dup @ rot swap over over $00000000ffffffff and
    = if print_string swap drop register_table swap else drop endif
    swap 1 cells + dup register_table >
  until
  drop drop
;

: decode_Mem ( instruction tbentry -- )
  swap
  dup $03e00000 and 21 rshift decode_register
  dup $0000ffff and dup 15 rshift negate 15 lshift or .
  $001f0000 and 16 rshift decode_register
  print_string
;

: decode_Mfc ( instruction tbentry -- )
  drop
  dup $03e00000 and 21 rshift decode_register
  dup $001f0000 and 16 rshift decode_register
  $0000ffff and Mfc_list
  begin
    dup @ rot swap over over $00000000ffffffff and
    = if print_string drop drop register_table 1 else drop endif
    swap 1 cells + dup F-P_list >
  until
  drop drop
;

: decode_Mbr ( instruction tbentry -- )
  drop
  dup $03e00000 and 21 rshift decode_register
  dup $001f0000 and 16 rshift decode_register
  dup $00003fff and decode_register
  $0000c000 and 14 rshift cells Mbr_table +
  @ print_string
;

: decode_Opr ( instruction tbentry -- )
  drop
  dup $03e00000 and 21 rshift decode_register
  dup dup $00001000 and $00001000
  = if
    $001fe000 and 13 rshift . -1
  else
    $001f0000 and 16 rshift decode_register 0
  endif
  swap dup $0000001f and decode_register
  dup 26 rshift $10 -
  swap $00000fe0 and 3 rshift or Opr_list
  begin
    dup @ rot swap over over $00000000ffffffff and
    = if print_string swap drop register_table swap else drop endif
    swap 1 cells + dup Mfc_list >
  until
  drop drop if $23 emit endif
;

: decode_Pcd ( instruction tbentry -- )
  swap
  $0000000003ffffff and .
  print_string
;

\ format

' decode_Opc decode_code - constant cOpc
' decode_Bra decode_code - constant cBra
' decode_F-P decode_code - constant cF-P
' decode_Mem decode_code - constant cMem
' decode_Mfc decode_code - constant cMfc
' decode_Mbr decode_code - constant cMbr
' decode_Opr decode_code - constant cOpr
' decode_Pcd decode_code - constant cPcd

create opcode_table

( 00 ) cPcd s" call_pal" mktbentry,
( 01 ) cOpc s" opc01"       mktbentry,
( 02 ) cOpc s" opc02"       mktbentry,
( 03 ) cOpc s" opc03"       mktbentry,
( 04 ) cOpc s" opc04"       mktbentry,
( 05 ) cOpc s" opc05"       mktbentry,
( 06 ) cOpc s" opc06"       mktbentry,
( 07 ) cOpc s" opc07"       mktbentry,
( 08 ) cMem s" lda"         mktbentry,
( 09 ) cMem s" ldah"        mktbentry,
( 0a ) cOpc s" opc0a"       mktbentry,
( 0b ) cMem s" ldq_u"       mktbentry,
( 0c ) cOpc s" opc0c"       mktbentry,
( 0d ) cOpc s" opc0d"       mktbentry,
( 0e ) cOpc s" opc0e"       mktbentry,
( 0f ) cMem s" stq_u"       mktbentry,
( 10 ) cOpr s" "            mktbentry,
( 11 ) cOpr s" "            mktbentry,
( 12 ) cOpr s" "            mktbentry,
( 13 ) cOpr s" "            mktbentry,
( 14 ) cOpc s" opc14"       mktbentry,
( 15 ) cF-P s" "            mktbentry,
( 16 ) cF-P s" "            mktbentry,
( 17 ) cF-P s" "            mktbentry,
( 18 ) cMfc s" "            mktbentry,
( 19 ) cOpc s" pal19"       mktbentry,
( 1a ) cMbr s" "            mktbentry,
( 1b ) cOpc s" pal1b"       mktbentry,
( 1c ) cOpc s" opc1c"       mktbentry,
( 1d ) cOpc s" pal1d"       mktbentry,
( 1e ) cOpc s" pal1e"       mktbentry,
( 1f ) cOpc s" pal1f"       mktbentry,
( 20 ) cMem s" ldf"         mktbentry,
( 21 ) cMem s" ldg"         mktbentry,
( 22 ) cMem s" lds"         mktbentry,
( 23 ) cMem s" ldt"         mktbentry,
( 24 ) cMem s" stf"         mktbentry,
( 25 ) cMem s" stg"         mktbentry,
( 26 ) cMem s" sts"         mktbentry,
( 27 ) cMem s" stt"         mktbentry,
( 28 ) cMem s" ldl"         mktbentry,
( 29 ) cMem s" ldq"         mktbentry,
( 2a ) cMem s" ldl_l"       mktbentry,
( 2b ) cMem s" ldq_l"       mktbentry,
( 2c ) cMem s" stl"         mktbentry,
( 2d ) cMem s" stq"         mktbentry,
( 2e ) cMem s" stl_c"       mktbentry,
( 2f ) cMem s" stq_c"       mktbentry,
( 30 ) cBra s" br"          mktbentry,
( 31 ) cBra s" fbeq"        mktbentry,
( 32 ) cBra s" fblt"        mktbentry,
( 33 ) cBra s" fble"        mktbentry,
( 34 ) cBra s" bsr"         mktbentry,
( 35 ) cBra s" fbne"        mktbentry,
( 36 ) cBra s" fbge"        mktbentry,
( 37 ) cBra s" fbgt"        mktbentry,
( 38 ) cBra s" blbc"        mktbentry,
( 39 ) cBra s" beq"         mktbentry,
( 3a ) cBra s" blt"         mktbentry,
( 3b ) cBra s" ble"         mktbentry,
( 3c ) cBra s" blbs"        mktbentry,
( 3d ) cBra s" bne"         mktbentry,
( 3e ) cBra s" bge"         mktbentry,
( 3f ) cBra s" bgt"         mktbentry,

drop \ string_table end

set-current

: disasm-inst ( addr n -- addr )  \ instruction decoder
  dup $fc000000 and
  26 rshift cells
  opcode_table +
  @ dup $00000000ffffffff and
  decode_code + execute
  $2c emit cr
;

: disasm ( addr u -- )  \ gforth
    \G disassemble u aus starting at addr
    cr bounds
    u+do
	." ( " i hex. ." ) "
	i i h@ disasm-inst drop
	4
    +loop ;

' disasm is discode

previous previous
