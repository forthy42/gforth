\ disasm.fs	disassembler file for loongarch64
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

[IFUNDEF] #+mask
    : #+mask ( addr u -- num mask )
	#0. 2swap bounds ?DO
	    2* swap 2* swap
	    case I c@
		'0' of  1 or  endof
		'1' of  swap 1 or swap 1 or  endof
	    endcase
	LOOP ;
[THEN]

vocabulary disassembler

disassembler also definitions

: ., ( -- )  ." , " ;
: .r# ( n -- )  $1F and ." $r" 0 .r ;
: .rd ( n -- )  .r# ;
: .rj ( n -- )  #5 rshift .r# ;
: .rk ( n -- )  #10 rshift .r# ;
: .f# ( n -- )  $1F and ." $f" 0 .r ;
: .fd ( n -- )  .f# ;
: .fj ( n -- )  #5 rshift .f# ;
: .fk ( n -- )  #10 rshift .f# ;
: .fa ( n -- )  #15 rshift .f# ;
: .fcc# ( n -- )  $7 and ." $fcc" 0 .r ;
: .fcsr# ( n -- )  $1F and ." $fcsr" 0 .r ;
: .2# ( n -- )  $3 and 0 .r ;
: .3# ( n -- )  $7 and 0 .r ;
: .5# ( n -- )  $1F and 0 .r ;
: .6# ( n -- )  $3F and 0 .r ;
: .u8# ( n -- )  $FF and 0 .r ;
: .u12# ( n -- )  $FFF and 0 .r ;
: .u14# ( n -- )  $3FFF and 0 .r ;
: .s12# ( n -- )  #10 rshift $FFF and dup $800 and negate or 0 .r ;
: .s14# ( n -- )  #10 rshift $3FFF and dup $2000 and negate or 0 .r ;
: .s16# ( n -- )  #10 rshift $FFFF and dup $8000 and negate or 0 .r ;
: .s20# ( n -- )  $FFFFF and dup $80000 and negate or 0 .r ;
: <code> ( n -- ) $7FFF and h. ;
: <rd,rj> ( inst -- )
    dup .rd ., .rj ;
: <rd,rj,rk> ( inst -- )
    dup <rd,rj> ., .rk ;
: <rd,rk,rj> ( inst -- )
    dup .rd ., dup .rk ., .rj ;
: <fd,rk,rj> ( inst -- )
    dup .fd ., dup .rk ., .rj ;
: <rd,rj,rk,sa2> ( inst -- )
    dup <rd,rj,rk> ., #15 rshift .2# ;
: <rd,rj,rk,sa3> ( inst -- )
    dup <rd,rj,rk> ., #15 rshift .3# ;
: <rd,rj,ui5> ( inst -- )
    dup <rd,rj> ., #10 rshift .5# ;
: <rd,rj,ui6> ( inst -- )
    dup <rd,rj> ., #10 rshift .6# ;
: <rd,rj,msbw,lsbw> ( inst -- )
    dup <rd,rj> ., dup #10 rshift .5# ., #16 rshift .5# ;
: <rd,rj,msbd,lsbd> ( inst -- )
    dup <rd,rj> ., dup #10 rshift .6# ., #16 rshift .6# ;
: <rd,rj,si12> ( inst -- )
    dup <rd,rj> ., .s12# ;
: <rd,rj,si14> ( inst -- )
    dup <rd,rj> ., .s14# ;
: <rd,rj,si16> ( inst -- )
    dup <rd,rj> ., .s16# ;
: <rd,si20> ( inst -- )
    dup .rd ., #5 rshift .s20# ;
: <rd,rj,ui12> ( inst -- )
    dup <rd,rj> ., #10 rshift .u12# ;
: <rj,rk> ( inst -- )
    dup .rj ., .rk ;
: <fd,fj> ( inst -- )
    dup .fd ., .fj ;
: <fd,rj> ( inst -- )
    dup .fd ., .rj ;
: <fd,rj,si12> ( inst -- )
    dup <fd,rj> ., .s12# ;
: <fd,rj,rk> ( inst -- )
    dup <fd,rj> ., .rk ;
: <rd,fj> ( inst -- )
    dup .rd ., .fj ;
: <fd,fj,fk> ( inst -- )
    dup <fd,fj> ., .fk ;
: <fd,fj,fk,fa> ( inst -- )
    dup <fd,fj,fk> ., .fa ;
: <fcsr,rj> ( inst -- )
    dup .fcsr# ., .rj ;
: <rd,fcsr> ( inst -- )
    dup .rd ., #5 rshift .fcsr# ;
: <cd,fj> ( inst -- )
    dup .fcc# ., .fj ;
: <fd,cj> ( inst -- )
    dup .fd ., #5 rshift .fcc# ;
: <cd,rj> ( inst -- )
    dup .fcc# ., .rj ;
: <rd,cj> ( inst -- )
    dup .rd ., #5 rshift .fcc# ;
: <--> ( inst -- ) drop ;
: .offs16 ( inst -- )
    #10 rshift $FFFF and
    dup $0008000 and negate or 0 .r ;
: .offs21 ( inst -- )
    dup #10 rshift $FFFF and
    swap $01F and $10 lshift or
    dup $0100000 and negate or 0 .r ;
: <offs> ( inst -- )
    dup #10 rshift $FFFF and
    swap $3FF and $10 lshift or
    dup $2000000 and negate or 0 .r ;
: <rd,rj,offs> ( inst -- )
    dup <rd,rj> ., .offs16 ;
: <rj,rd,offs> ( inst -- )
    dup .rj ., dup .rd ., .offs16 ;
: <rj,offs> ( inst -- )
    dup .rj ., .offs21 ;
: <cj,offs> ( inst -- )
    dup #5 rshift .fcc# ., .offs21 ;
: .cond ( inst -- )
    #15 rshift $1F and 2* 2*
    s" caf saf clt slt ceq seq cle sle cun sun cultsultcueqsueqculesulecne sne 0x120x13cor sor 0x160x17cunesune0x1A0x1B0x1C0x1D0x1E0x1F"
    drop + 4 -trailing "cond" replaces ;
: <cd,fj,fk> ( inst -- )
    dup .fcc# ., dup .fj ., .fk ;
: <fd,fj,fk,ca> ( inst -- )
    dup .fd ., dup .fj ., dup .fk ., .3# ;
: <hint,rj,rk> ( inst -- )
    dup .5# ., dup .rj ., .rk ;
: <hint,rj,si12> ( inst -- )
    dup .5# ., dup .rj ., .s12# ;
: <op,rj,rk> ( inst -- )
    dup .5# ., dup .rj ., .rk ;
: <rd,csr> ( inst -- )
    dup .rd ., #10 rshift .u14# ;
: <rd,rj,csr> ( inst -- )
    dup <rd,rj> ., #10 rshift .u14# ;
: <rd,rj,level> ( inst -- )
    dup <rd,rj> ., #10 rshift .u8# ;
: <rj,seq> ( inst -- )
    dup .rj ., #10 rshift .u8# ;

: str16, ( addr u -- )
    $10 umin here $10 allot dup $10 bl fill swap move ;

: read-dis ( -- )
    BEGIN  refill  WHILE
	    parse-name 2dup s" \" str= over 0= or IF  2drop
	    ELSE
		' parse-name #+mask 2, , str16,
	    THEN
    REPEAT ;

Create inst-table
s" ./insts.fs" open-fpath-file throw ' read-dis execute-parsing-named-file
#0. 2,
DOES> ( inst addr -- addr' inst )
  swap >r 5 cells -
  BEGIN  5 cells + dup 2@ 2dup d0<> WHILE
	  r@ and = UNTIL  ELSE  drop nip  THEN r> ;

Forth definitions

: disline ( ip -- ip' )
    [:  dup l@ inst-table over 0= IF
	    '<' emit 0 hex.r '>' emit drop
	ELSE
	    dup .cond
	    over 3 cells + $10 -trailing $substitute drop
	    tuck type $10 swap - 0 max spaces
	    swap 2 cells + perform
	THEN
	4 +
    ;] #10 base-execute ;

: disasm ( addr u -- ) \ gforth
    bounds u+do  cr i disline i - +loop  cr ;

' disasm is discode

previous Forth

