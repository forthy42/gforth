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

$0000 c-noarg: illegal
$2000 c-fldd: fld
$4000 c-ldw: lw
$6000 c-ldd: ld
$A000 c-fldd: fsd
$C000 c-ldw: sw
$E000 c-ldd: sd

$0001 c-noarg: nop
$0001 c-addi: addi
$2001 c-addi: addiw
$4001 c-li: li
$6101 c-addi16: add16sp
$6001 c-lui: lui
$8001 c-sri: srli
$8401 c-sri: srai
$8801 c-andi: andi
$8C01 c-and: sub
$8C21 c-and: xor
$8C41 c-and: or
$8C61 c-and: and
$9C01 c-and: subw
$9C21 c-and: addw
$A001 c-j: j
$C001 c-beq: beqz
$E001 c-beq: bnez

$0002 c-sli: slli
$2002 c-fldsp: fldsp
$4002 c-lwsp: lwsp
$6002 c-ldsp: ldsp
$8002 c-jr: jr
$8002 c-mv: mv
$9002 c-noarg: ebreak
$9002 c-jr: jalr
$9002 c-add: add
$A002 c-fsdsp: fsdsp
$C002 c-swsp: swsp
$E002 c-sdsp: sdsp
