\ asm.fs	assembler file for loongarch64
\
\ Authors: Bernd Paysan
\ Copyright (C) 2025 Free Software Foundation, Inc.

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

: Constants ( n -- )
    0 ?DO  parse-name 2dup "--" str=
	IF  2drop  ELSE  nextname i constant  THEN  LOOP ;
: #+mask ( addr u -- num mask )
    #0. 2swap bounds ?DO
	2* swap 2* swap
	case I c@
	    '0' of  1 or  endof
	    '1' of  swap 1 or swap 1 or  endof
	endcase
    LOOP ;

get-current also assembler definitions

$20 Constants caf saf clt slt ceq seq cle sle cun sun cult sult cueq sueq cule sule cne sne -- -- cor sor -- -- cune sune -- -- -- -- -- --
$20 Constants r0 r1 r2 r3 r4 r5 r6 r7 r8 r9 r10 r11 r12 r13 r14 r15 r16 r17 r18 r19 r20 r21 r22 r23 r24 r25 r26 r27 r28 r29 r30 r31
$20 Constants f0 f1 f2 f3 f4 f5 f6 f7 f8 f9 f10 f11 f12 f13 f14 f15 f16 f17 f18 f19 f20 f21 f22 f23 f24 f25 f26 f27 f28 f29 f30 f31

\ predefined register aliases for gforth-fast-ll-reg
r23 constant rip
r24 constant rsp
r25 constant rrp
r26 constant rlp
r27 constant rcfa
r28 constant rtos
r29 constant rop
f24 constant ftos

: code, ( code inst -- inst' ) swap $7FFF and or ;
: rd, ( x inst -- inst' )  swap $1F and or ;
: cd, ( x inst -- inst' )  swap $7 and or ;
: rj, ( x inst -- inst' )  swap $1F and #5 lshift or ;
: cj, ( x inst -- inst' )  swap $7 and #5 lshift or ;
: rk, ( x inst -- inst' )  swap $1F and #10 lshift or ;
: ui6, ( x inst -- inst' )  swap $3F and #10 lshift or ;
: ra, ( x inst -- inst' )  swap $1F and #15 lshift or ;
: ca, ( x inst -- inst' )  swap $7 and #15 lshift or ;
: lsbw, ( x inst -- inst' )  swap $1F and #16 lshift or ;
: lsbd, ( x inst -- inst' )  swap $3F and #16 lshift or ;
: sa2, ( x inst -- inst' )  swap $3 and swap ra, ;
: sa3, ( x inst -- inst' )  swap $7 and swap ra, ;
: i8, ( x inst -- inst' )  swap $FF and #10 lshift or ;
: i12, ( x inst -- inst' )  swap $FFF and #10 lshift or ;
: i14, ( x inst -- inst' )  swap $3FFF and #10 lshift or ;
: i16, ( x inst -- inst' )  swap $FFFF and #10 lshift or ;
: i20, ( x inst -- inst' )  swap $FFFFF and #5 lshift or ;
: offs16, ( x inst -- inst' )  swap $FFFF and #10 lshift or ;
: offs21, ( x inst -- inst' )
    over $FFFF and #10 lshift or
    swap $10 rshift $1F and or ;
: offs26, ( x inst -- inst' )
    over $FFFF and #10 lshift or
    swap $10 rshift $3FF and or ;

: <code> ( code inst-addr -- ) @ code, l, ;
: <rd,rj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <rd,rj,rk> ( rd rj rk inst-addr -- ) @ rk, rj, rd, l, ;
: <rd,rk,rj> ( rd rk rj inst-addr -- ) @ rj, rk, rd, l, ;
: <fd,rk,rj> ( rd rk rj inst-addr -- ) @ rj, rk, rd, l, ;
: <rd,rj,rk,sa2> ( rd rj rk sa2 inst-addr -- ) @ sa2, rk, rj, rd, l, ;
: <rd,rj,rk,sa3> ( rd rj rk sa3 inst-addr -- ) @ sa3, rk, rj, rd, l, ;
: <rd,rj,ui5> ( rd rj ui5 inst-addr -- ) @ rk, rj, rd, l, ;
: <rd,rj,ui6> ( rd rj ui6 inst-addr -- ) @ ui6, rj, rd, l, ;
: <rd,rj,msbw,lsbw> ( rd rj msbw lsbw inst-addr -- ) @ lsbw, rk, rj, rd, l, ;
: <rd,rj,msbd,lsbd> ( rd rj msbw lsbw inst-addr -- ) @ lsbd, ui6, rj, rd, l, ;
: <rd,rj,si12> ( rd rj si12 inst-addr -- ) @ i12, rj, rd, l, ;
: <rd,rj,si14> ( rd rj si14 inst-addr -- ) @ i14, rj, rd, l, ;
: <rd,rj,si16> ( rd rj si16 inst-addr -- ) @ i16, rj, rd, l, ;
: <rd,si20> ( rd rj si16 inst-addr -- ) @ i20, rd, l, ;
: <rd,rj,ui12> ( rd rj si12 inst-addr -- ) @ i12, rj, rd, l, ;
: <rj,rk> ( rj rk inst-addr -- ) @ rk, rj, l, ;
: <fd,fj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <fd,rj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <fd,rj,si12> ( rd rj si12 inst-addr -- ) @ i12, rj, rd, l, ;
: <fd,rj,rk> ( rd rj rk inst-addr -- ) @ rk, rj, rd, l, ;
: <rd,fj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <fd,fj,fk> ( rd rj rk inst-addr -- ) @ rk, rj, rd, l, ;
: <fd,fj,fk,fa> ( rd rj rk ra inst-addr -- ) @ rk, rj, rd, ra, l, ;
: <fcsr,rj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <rd,fcsr> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <cd,fj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <cd,rj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <fd,cj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <rd,cj> ( rd rj inst-addr -- ) @ rj, rd, l, ;
: <--> ( inst-addr -- )  @ l, ;
: <offs> ( offs26 inst-addr -- )  @ offs26, l, ;
: <rd,rj,offs> ( rd rj offs16 inst-addr -- ) @ offs16, rj, rd, l, ;
: <rj,rd,offs> ( rd rj offs16 inst-addr -- ) @ offs16, rd, rj, l, ;
: <rj,offs> ( rd offs21 inst-addr -- ) @ offs21, rj, l, ;
: <cj,offs> ( rd offs21 inst-addr -- ) @ offs21, cj, l, ;
: <cd,fj,fk> ( cd fj fk cond inst-addr -- ) @ ra, rk, rj, cd, l, ;
: <fd,fj,fk,ca> ( fd fj fk ca inst-addr -- ) @ ca, rk, rj, rd, l, ;
: <hint,rj,rk> ( hint rj rk inst-addr -- ) @ rk, rj, i12, l, ;
: <hint,rj,si12> ( hint rj si12 inst-addr -- ) @ i12, rj, rd, l, ;
: <op,rj,rk> ( hint rj rk inst-addr -- ) @ rk, rj, rd, l, ;
: <rd,csr> ( rd csr inst-addr -- ) @ i14, rd, l, ;
: <rd,rj,csr> ( rd rj csr inst-addr -- ) @ i14, rj, rd, l, ;
: <rd,rj,level> ( rd rj csr inst-addr -- ) @ i8, rj, rd, l, ;
: <rj,seq> ( rj csr inst-addr -- ) @ i8, rj, l, ;

: read-asm ( -- )
    "~" "cond" replaces
    BEGIN  refill  WHILE
	    parse-name 2dup s" \" str= over 0= or IF  2drop
	    ELSE
		$substitute drop
		nextname ' Create parse-name #+mask drop , set-does>
	    THEN
    REPEAT ;

s" ./insts.fs" open-fpath-file throw ' read-asm execute-parsing-named-file

: next ( -- )
    \ assume dynamic code generation works, so NOOP's code can be copied
    \ Essentially assumes: code noop next end-code
    ['] noop >code-address ['] call >code-address over -
    here swap dup allot move ;

previous set-current definitions
