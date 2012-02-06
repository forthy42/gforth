\ testasm.fs	tests for LatticeMico32 assembler
\
\ Copyright (C) 2012 Free Software Foundation, Inc.

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

\ Author: David Kühling
\ Created: Feb 2012

require ./asm.fs

ALIGN
ALSO ASSEMBLER

0 VALUE first  0 VALUE last

HERE TO first

\ todo: pseudo instructions for these:

ra b,
r1 r2 r0 or,
r1 r0 $7fff orhi,
r1 r2 r0 xnor,
r2 r0 $7fff addi,
r4 r9 r10 add,
r4 r9 123 addi,
r4 r9 r10 and,
r4 r9 123 andhi,
r4 r9 123 andi,
r9 b,
r9 r10 $123 be,
r9 r10 $123 bg,
r9 r10 $123 bge,
r9 r10 $123 bgeu,
r9 r10 $123 bgu,
$1234567 bi,
r9 r10 $123 bne,
break,
ba b, \ bret,
r9 call,
$1234567 calli,
r4 r9 r10 cmpe,
r4 r9 123 cmpei,
r4 r9 r10 cmpg,
r4 r9 123 cmpgi,
r4 r9 r10 cmpge,
r4 r9 123 cmpgei,
r4 r9 r10 cmpgeu,
r4 r9 123 cmpgeui,
r4 r9 r10 cmpgu,
r4 r9 123 cmpgui,
r4 r9 r10 cmpne,
r4 r9 123 cmpnei,
r4 r9 r10 divu,
ea b, \ eret,
r4 r18 123 lb,
r4 r18 123 lbu,
r4 r18 123 lh,
r4 r18 123 lhu,
r4 r18 123 lw,
r4 r9 r10 modu,
r4 r9 r10 mul,
r4 r9 $123 muli,
r4 r9 r10 nor,
r4 r9 $123 nori,
r4 r9 r10 or,
r4 r9 123 ori,
r4 r9 123 orhi,
r4 IM rcsr,
ra b, \ ret
r18 123 r9 sb,
scall,
r4 r9 sextb,
r4 r9 sexth,
r18 123 r9 sh,
r4 r9 r10 sl,
r4 r9 123 sli,
r4 r9 r10 sr,
r4 r9 123 sri,
r4 r9 r10 sru,
r4 r9 123 srui,
r4 r9 r10 sub,
r18 123 r9 sw,
IM r4 wcsr,
r4 r9 r10 xnor,
r4 r9 $123 xnori,
r4 r9 r10 xor,
r4 r9 $123 xori,

HERE TO last

\ write out assembled data to file
s" testasm.bin" w/o create-file throw CONSTANT fid
first  last over - fid write-file throw
fid close-file throw

