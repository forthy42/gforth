\ disasm.fs	disassembler file (for MIPS R3000)
\
\ Copyright (C) 1995-97 Martin Anton Ertl, Christian Pirker
\
\ This file is part of RAFTS.
\
\	RAFTS is free software; you can redistribute it and/or
\	modify it under the terms of the GNU General Public License
\	as published by the Free Software Foundation; either version 2
\	of the License, or (at your option) any later version.
\
\	This program is distributed in the hope that it will be useful,
\	but WITHOUT ANY WARRANTY; without even the implied warranty of
\	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\	GNU General Public License for more details.
\
\	You should have received a copy of the GNU General Public License
\	along with this program; if not, write to the Free Software
\	Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

: (disasm-op) ( code -- n )
    $1a rshift $6 asm-bitmask and ;

: disasm-op ( code -- )
    (disasm-op) 2 swap hexn. space ;

: (disasm-rs) ( code -- n )
    $15 rshift $5 asm-bitmask and ;

: disasm-rs ( code -- )
    (disasm-rs) 2 swap hexn. space ;

: (disasm-rt) ( code -- n )
    $10 rshift $5 asm-bitmask and ;

: disasm-rt ( code -- )
    (disasm-rt) 2 swap hexn. space ;

: (disasm-imm) ( code -- n )
    $10 asm-bitmask and ;

: disasm-imm ( code -- )
    (disasm-imm) 4 swap hexn. space ;

: disasm-addr ( addr code -- n )
    (disasm-imm) $2 lshift asm-expand dup 4 swap hexn. space ." ( " + cell+ hex. ." ) " ;

: (disasm-target) ( code -- n )
    $1a asm-bitmask and ;

: disasm-target ( code -- )
    (disasm-target) $2 lshift or hex. ;

: (disasm-rd) ( code -- n )
    $b rshift $5 asm-bitmask and ;

: disasm-rd ( code -- )
    (disasm-rd) 2 swap hexn. space ;

: (disasm-shamt) ( code -- n )
    $6 rshift $5 asm-bitmask and ;

: disasm-shamt ( code -- )
    (disasm-shamt) 2 swap hexn. space ;

' disasm-shamt alias disasm-sa

: disasm-funct ( code -- n )
    $6 asm-bitmask and ;

\ ***** I-types
: disasm-I-rt,imm ( addr -- )
    @ dup disasm-rt disasm-imm ;

: disasm-I-rs,imm ( addr -- )
    dup @ dup disasm-rs disasm-addr ;

: disasm-I-rt,rs,imm ( addr -- )
    @ dup disasm-rt dup disasm-rs disasm-imm ;

: disasm-I-rs,rt,imm ( addr -- )
    dup @ dup disasm-rs dup disasm-rt disasm-addr ;

: disasm-I-rt,offset,rs ( addr -- )
    @ dup disasm-rt dup disasm-imm disasm-rs ;

\ ***** regimm types
' disasm-I-rs,imm alias disasm-regimm-rs,imm

\ ***** copz types 1
: disasm-copz-imm ( addr -- )
    dup @ dup disasm-addr disasm-op ;

: disasm-copz-rt,offset,rs,z ( addr -- )
    @ dup disasm-rt dup disasm-imm
    dup disasm-rs disasm-op ;

\ ***** J-types
: disasm-J-target ( addr -- )
    dup $fc000000 and swap @ disasm-target ;

\ ***** R-types
: disasm-R-nothing ( addr -- )
    @ hex. ;

: disasm-R-rd ( addr -- )
    @ disasm-rd ;

: disasm-R-rs ( addr -- )
    @ disasm-rs ;

: disasm-R-rd,rs ( addr -- )
    @ dup disasm-rd disasm-rs ;

: disasm-R-rs,rt ( addr -- )
    @ dup disasm-rs disasm-rt ;

: disasm-R-rd,rs,rt ( addr -- )
    @ dup disasm-rd dup disasm-rs disasm-rt ;

: disasm-R-rd,rt,rs ( addr -- )
    @ dup disasm-rd dup disasm-rt disasm-rs ;

: disasm-R-rd,rt,sa ( addr -- )
    @ dup disasm-rd dup disasm-rt disasm-sa ;

\ ***** special types
' disasm-R-nothing	alias disasm-special-nothing
' disasm-R-rd		alias disasm-special-rd
' disasm-R-rs		alias disasm-special-rs
' disasm-R-rd,rs	alias disasm-special-rd,rs
' disasm-R-rs,rt	alias disasm-special-rs,rt
' disasm-R-rd,rs,rt	alias disasm-special-rd,rs,rt
' disasm-R-rd,rt,rs	alias disasm-special-rd,rt,rs
' disasm-R-rd,rt,sa	alias disasm-special-rd,rt,sa

\ ***** copz types 2
: disasm-cop0 ( addr -- )
    @ disasm-rs ;

: disasm-copz-rt,rd ( addr -- )
    @ dup disasm-rt dup disasm-rd disasm-op ;

$40 2 matrix disasm-opc
$40 2 matrix disasm-opc-special
$20 2 matrix disasm-opc-regimm
$20 2 matrix disasm-opc-copzrs
$20 2 matrix disasm-opc-copzrt
$40 2 matrix disasm-opc-cop0

: (disasm-print) ( addr n addr -- )
    >r dup 1 r@ [ 1 -2 wword-regs-adjust ] execute @ rot swap [ 0 -1 wword-regs-adjust ] execute
    0 r> [ 1 -2 wword-regs-adjust ] execute @ name. ;

: disasm-print ( addr -- )
    dup @ if
	dup @ (disasm-op)
	dup 0 disasm-opc @ NIL <> if
	    ['] disasm-opc (disasm-print)
	else
	    1 disasm-opc @ [ 0 -1 wword-regs-adjust ] execute
	endif
    else
	drop ['] nop, name.
    endif ;

: disasm-dump ( addr count -- )
    cr
    over + swap ?do
	i ." ( " dup hex. ." , " dup @ hex. ." ) " disasm-print cr
    4 +loop ;

: (disasm-gen) ( name func n addr -- )
    >r tuck 1 r@ [ 1 -2 wword-regs-adjust ] execute !
    0 r> [ 1 -2 wword-regs-adjust ] execute ! ;

: disasm-gen ( name func n -- )
    ['] disasm-opc (disasm-gen) ;

: disasm-print-special ( addr -- )
    dup @ disasm-funct ['] disasm-opc-special (disasm-print) ;

: disasm-gen-special ( name func n -- )
    ['] disasm-opc-special (disasm-gen) ;

: disasm-print-regimm ( addr -- )
    dup @ (disasm-rt) ['] disasm-opc-regimm (disasm-print) ;

: disasm-gen-regimm ( name func n -- )
    ['] disasm-opc-regimm (disasm-gen) ;

: disasm-print-copzrs ( addr -- )
    dup @ (disasm-rs)
    dup 0 disasm-opc-copzrs @ NIL <> if
	['] disasm-opc-copzrs (disasm-print)
    else
	1 disasm-opc-copzrs @
	[ 0 -1 wword-regs-adjust ]
	execute
    endif ;

: disasm-gen-copzrs ( name func n -- )
    ['] disasm-opc-copzrs (disasm-gen) ;

: disasm-print-copzrt ( addr -- )
    dup @ (disasm-rt) ['] disasm-opc-copzrt (disasm-print) ;

: disasm-gen-copzrt ( name func n -- )
    ['] disasm-opc-copzrt (disasm-gen) ;

: disasm-print-copzi ( addr -- )
    dup @ (disasm-rs) ['] disasm-opc-copzrs (disasm-print) ;

: disasm-gen-copzi ( name func n -- )
    >r 2dup r@ 1+ disasm-gen
    2dup r@ 2 + disasm-gen
    r> 3 + disasm-gen ;

: disasm-print-cop0 ( addr -- )
    dup @ disasm-funct ['] disasm-opc-cop0 (disasm-print) ;

: disasm-gen-cop0 ( name func n -- )
    ['] disasm-opc-cop0 (disasm-gen) ;

: illegal-code ( -- ) ;

: disasm-nop ( code -- )
    @ 8 swap ." ( " hexn. space ." ) " ;

: disasm-init ( xt n -- )
    0 ?do
	['] illegal-code ['] disasm-nop i 3 pick
	[ 0 -3 wword-regs-adjust ] execute
    loop
    drop ;
' disasm-gen $40 disasm-init
' disasm-gen-special $40 disasm-init
' disasm-gen-regimm $20 disasm-init
' disasm-gen-copzrs $20 disasm-init
' disasm-gen-copzrt $20 disasm-init
' disasm-gen-cop0 $40 disasm-init
NIL ' disasm-print-special $00 disasm-gen
NIL ' disasm-print-regimm $01 disasm-gen
NIL ' disasm-print-cop0 $10 disasm-gen
NIL ' disasm-print-copzrs $11 disasm-gen
NIL ' disasm-print-copzrs $12 disasm-gen
NIL ' disasm-print-copzrs $13 disasm-gen
NIL ' disasm-print-copzrt asm-copz-BC disasm-gen-copzrs

' beq,		' disasm-I-rs,rt,imm $04 disasm-gen
' bne,		' disasm-I-rs,rt,imm $05 disasm-gen
' blez,		' disasm-I-rs,imm $06 disasm-gen
' bgtz,		' disasm-I-rs,imm $07 disasm-gen
' addi,		' disasm-I-rt,rs,imm $08 disasm-gen
' addiu,	' disasm-I-rt,rs,imm $09 disasm-gen
' slti,		' disasm-I-rt,rs,imm $0a disasm-gen
' sltiu,	' disasm-I-rt,rs,imm $0b disasm-gen
' andi,		' disasm-I-rt,rs,imm $0c disasm-gen
' ori,		' disasm-I-rt,rs,imm $0d disasm-gen
' xori,		' disasm-I-rt,rs,imm $0e disasm-gen
' lui,		' disasm-I-rt,imm $0f disasm-gen
' lb,		' disasm-I-rt,offset,rs $20 disasm-gen
' lh,		' disasm-I-rt,offset,rs $21 disasm-gen
' lwl,		' disasm-I-rt,offset,rs $22 disasm-gen
' lw,		' disasm-I-rt,offset,rs $23 disasm-gen
' lbu,		' disasm-I-rt,offset,rs $24 disasm-gen
' lhu,		' disasm-I-rt,offset,rs $25 disasm-gen
' lwr,		' disasm-I-rt,offset,rs $26 disasm-gen
' sb,		' disasm-I-rt,offset,rs $28 disasm-gen
' sh,		' disasm-I-rt,offset,rs $29 disasm-gen
' swl,		' disasm-I-rt,offset,rs $2a disasm-gen
' sw,		' disasm-I-rt,offset,rs $2b disasm-gen
' swr,		' disasm-I-rt,offset,rs $2e disasm-gen

' j,		' disasm-J-target $02 disasm-gen
' jal,		' disasm-J-target $03 disasm-gen

' sll,		' disasm-special-rd,rt,sa $00 disasm-gen-special
' srl,		' disasm-special-rd,rt,sa $02 disasm-gen-special
' sra,		' disasm-special-rd,rt,sa $03 disasm-gen-special
' sllv,		' disasm-special-rd,rt,rs $04 disasm-gen-special
' srlv,		' disasm-special-rd,rt,rs $06 disasm-gen-special
' srav,		' disasm-special-rd,rt,rs $07 disasm-gen-special
' jr,		' disasm-special-rs $08 disasm-gen-special
' jalr,		' disasm-special-rd,rs $09 disasm-gen-special
' syscall,	' disasm-special-nothing $0c disasm-gen-special
' break,	' disasm-special-nothing $0d disasm-gen-special
' mfhi,		' disasm-special-rd $10 disasm-gen-special
' mthi,		' disasm-special-rs $11 disasm-gen-special
' mflo,		' disasm-special-rd $12 disasm-gen-special
' mtlo,		' disasm-special-rs $13 disasm-gen-special
' mult,		' disasm-special-rs,rt $18 disasm-gen-special
' multu,	' disasm-special-rs,rt $19 disasm-gen-special
' div,		' disasm-special-rs,rt $1a disasm-gen-special
' divu,		' disasm-special-rs,rt $1b disasm-gen-special
' add,		' disasm-special-rd,rs,rt $20 disasm-gen-special
' addu,		' disasm-special-rd,rs,rt $21 disasm-gen-special
' sub,		' disasm-special-rd,rs,rt $22 disasm-gen-special
' subu,		' disasm-special-rd,rs,rt $23 disasm-gen-special
' and,		' disasm-special-rd,rs,rt $24 disasm-gen-special
' or,		' disasm-special-rd,rs,rt $25 disasm-gen-special
' xor,		' disasm-special-rd,rs,rt $26 disasm-gen-special
' nor,		' disasm-special-rd,rs,rt $27 disasm-gen-special
' slt,		' disasm-special-rd,rs,rt $2a disasm-gen-special
' sltu,		' disasm-special-rd,rs,rt $2b disasm-gen-special

' bltz,		' disasm-regimm-rs,imm $00 disasm-gen-regimm
' bgez,		' disasm-regimm-rs,imm $01 disasm-gen-regimm
' bltzal,	' disasm-regimm-rs,imm $10 disasm-gen-regimm
' bgezal,	' disasm-regimm-rs,imm $11 disasm-gen-regimm

' lwcz,		' disasm-copz-rt,offset,rs,z $30 disasm-gen-copzi
' swcz,		' disasm-copz-rt,offset,rs,z $38 disasm-gen-copzi
' mfcz,		' disasm-copz-rt,rd asm-copz-MF disasm-gen-copzrs
' cfcz,		' disasm-copz-rt,rd asm-copz-CF disasm-gen-copzrs
' mtcz,		' disasm-copz-rt,rd asm-copz-MT disasm-gen-copzrs
' ctcz,		' disasm-copz-rt,rd asm-copz-CT disasm-gen-copzrs
' bczf,		' disasm-copz-imm asm-copz-BCF disasm-gen-copzrt
' bczt,		' disasm-copz-imm asm-copz-BCT disasm-gen-copzrt
' tlbr,		' disasm-cop0 $01 disasm-gen-cop0
' tlbwi,	' disasm-cop0 $02 disasm-gen-cop0
' tlbwr,	' disasm-cop0 $06 disasm-gen-cop0
' tlbl,		' disasm-cop0 $08 disasm-gen-cop0

?test $0800 [IF]
cr ." Test for disasm..fs" cr

: gen ( coden ... code0 n -- )
    0 ?do
	a,
    loop ;

here
$00210820 $00000820 $00200020 $00010020 $03fff820 5 gen
$20210001 $20010000 $20200000 $20000001 $23ffffff 5 gen
$24210001 $24010000 $24200000 $24000001 $27ffffff 5 gen
$00210821 $00000821 $00200021 $00010021 $03fff821 5 gen
$00210824 $00000824 $00200024 $00010024 $03fff824 5 gen
$30210001 $30010000 $30200000 $30000001 $33ffffff 5 gen
$45000001 $4500ffff 2 gen
$45010001 $4501ffff 2 gen
$10210001 $10200000 $10010000 $10000001 $13ffffff 5 gen
$04210001 $04210000 $04010001 $07e1ffff 4 gen
$04310001 $04310000 $04110001 $07f1ffff 4 gen
$1c200001 $1c200000 $1c000001 $1fe0ffff 4 gen
$18200001 $18200000 $18000001 $1be0ffff 4 gen
$04200001 $04200000 $04000001 $07e0ffff 4 gen
$04300001 $04300000 $04100001 $07f0ffff 4 gen
$14210001 $14200000 $14010000 $14000001 $17ffffff 5 gen
$0000000d 1 gen
$44410800 $44410000 $44400800 $445ff800 4 gen
$44c10800 $44c10000 $44c00800 $44dff800 4 gen
$0021001a $0020001a $0001001a $03ff001a 4 gen
$0021001b $0020001b $0001001b $03ff001b 4 gen
$08000001 $0bffffff 2 gen
$0c000001 $0fffffff 2 gen
$00200809 $00000809 $00200009 $03e0f809 4 gen
$00200008 $03e00008 2 gen
$80210001 $80010000 $80000001 $80200000 $83ffffff 5 gen
$90210001 $90010000 $90000001 $90200000 $93ffffff 5 gen
$84210001 $84010000 $84000001 $84200000 $87ffffff 5 gen
$94210001 $94010000 $94000001 $94200000 $97ffffff 5 gen
$3c010001 $3c010000 $3c000001 $3c1fffff 4 gen
$8c210001 $8c010000 $8c000001 $8c200000 $8fffffff 5 gen
$c4210001 $c4010000 $c4000001 $c4200000 $c7ffffff 5 gen
$88210001 $88010000 $88000001 $88200000 $8bffffff 5 gen
$98210001 $98010000 $98000001 $98200000 $9bffffff 5 gen
$44010800 $44010000 $44000800 $441ff800 4 gen
$00000810 $0000f810 2 gen
$00000812 $0000f812 2 gen
$44810800 $44810000 $44800800 $449ff800 4 gen
$00200011 $03e00011 2 gen
$00200013 $03e00013 2 gen
$00210018 $00200018 $00010018 $03ff0018 4 gen
$00210019 $00200019 $00010019 $03ff0019 4 gen
$00210827 $00000827 $00200027 $00010027 $03fff827 5 gen
$00210825 $00000825 $00200025 $00010025 $03fff825 5 gen
$34210001 $34010000 $34200000 $34000001 $37ffffff 5 gen
$a0210001 $a0010000 $a0000001 $a0200000 $a3ffffff 5 gen
$a4210001 $a4010000 $a4000001 $a4200000 $a7ffffff 5 gen
$0021082a $0000082a $0020002a $0001002a $03fff82a 5 gen
$28210001 $28010000 $28200000 $28000001 $2bffffff 5 gen
$2c210001 $2c010000 $2c200000 $2c000001 $2fffffff 5 gen
$0021082b $0000082b $0020002b $0001002b $03fff82b 5 gen
$00210822 $00000822 $00200022 $00010022 $03fff822 5 gen
$00210823 $00000823 $00200023 $00010023 $03fff823 5 gen
$ac210001 $ac010000 $ac000001 $ac200000 $afffffff 5 gen
$e4210001 $e4010000 $e4000001 $e4200000 $e7ffffff 5 gen
$a8210001 $a8010000 $a8000001 $a8200000 $abffffff 5 gen
$b8210001 $b8010000 $b8000001 $b8200000 $bbffffff 5 gen
$0000000c 1 gen
$42000008 1 gen
$42000001 1 gen
$42000002 1 gen
$42000006 1 gen
$00210826 $00000826 $00200026 $00010026 $03fff826 5 gen
$38210001 $38010000 $38200000 $38000001 $3bffffff 5 gen

$00200821 $00000821 $00200021 $03e0f821 4 gen
$00010822 $00200821 $04210002 $00000822 $00000821 $04010002
$00010022 $00200021 $04210002 $001ff822 $03e0f821 $07e10002 12 gen
$00010822 $00000822 $00010022 $001ff822 4 gen
$00010823 $00000823 $00010023 $001ff823 4 gen
$00200827 $00000827 $00200027 $03e0f827 4 gen
$14200001 $0021082a $14200000 $0020082a $14200000 $0001082a
$14200001 $0000082a $1420ffff $03ff082a 10 gen
$10200001 $0021082a $10200000 $0001082a $10200000 $0020082a
$10200001 $0000082a $1020ffff $03ff082a 10 gen
$14200001 $0021082a $14200000 $0001082a $14200000 $0020082a
$14200001 $0000082a $1420ffff $03ff082a 10 gen
$10200001 $0021082b $10200000 $0020082b $10200000 $0001082b
$10200001 $0000082b $1020ffff $03ff082b 10 gen
$14200001 $0021082b $14200000 $0020082b $14200000 $0001082b
$14200001 $0000082b $1420ffff $03ff082b 10 gen
$10200001 $0021082b $10200000 $0001082b $10200000 $0020082b
$10200001 $0000082b $1020ffff $03ff082b 10 gen
$14200001 $0021082b $14200000 $0001082b $14200000 $0020082b
$14200001 $0000082b $1420ffff $03ff082b 10 gen
$10200001 $0021082b $10200000 $0020082b $10200000 $0001082b
$10200001 $0000082b $1020ffff $03ff082b 10 gen
here over - disasm-dump

finish
[THEN]
