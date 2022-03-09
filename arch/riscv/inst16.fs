\ RISC-V instruction table for 16 bit instructions

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

$0000 c-noarg: c.illegal
$0000 c-addi4spn: c.addi4spn
$2000 c-fldd: c.fld
$4000 c-ldw: c.lw
$6000 c-ldd: c.ld
$A000 c-fldd: c.fsd
$C000 c-ldw: c.sw
$E000 c-ldd: c.sd

$0001 c-noarg: c.nop
$0001 c-addi: c.addi
$2001 c-addi: c.addiw
$4001 c-li: c.li
$6101 c-addi16: c.add16sp
$6001 c-lui: c.lui
$8001 c-sri: c.srli
$8401 c-sri: c.srai
$8801 c-andi: c.andi
$8C01 c-and: c.sub
$8C21 c-and: c.xor
$8C41 c-and: c.or
$8C61 c-and: c.and
$9C01 c-and: c.subw
$9C21 c-and: c.addw
$A001 c-j: c.j
$C001 c-beq: c.beqz
$E001 c-beq: c.bnez

$0002 c-sli: c.slli
$2002 c-fldsp: c.fldsp
$4002 c-lwsp: c.lwsp
$6002 c-ldsp: c.ldsp
$8002 c-jr: c.jr
$8002 c-mv: c.mv
$9002 c-noarg: c.ebreak
$9002 c-jr: c.jalr
$9002 c-add: c.add
$A002 c-fsdsp: c.fsdsp
$C002 c-swsp: c.swsp
$E002 c-sdsp: c.sdsp
