\ PIE MISC assembler

\ Copyright (C) 1998,2000,2003,2004,2007 Free Software Foundation, Inc.

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

Vocabulary assembler
also assembler also definitions forth
\ [IFUNDEF] cross
\ : X ;
\ [THEN]

\ sources

$0 Constant PC		$1 Constant PC+2
$2 Constant PC+4	$3 Constant PC+6

$8 Constant ACCU	$9 Constant SF
$A Constant ZF		$C Constant CF

\ destinations

$0 Constant JMP		$1 Constant JS
$2 Constant JZ		$4 Constant JC

$7 Constant *ACCU
( $8 Constant ACCU )	$9 Constant SUB
( $A Constant SUBR )	$B Constant ADD
$C Constant XOR		$D Constant OR
$E Constant AND		$F Constant SHR

$FFFC Constant txd
$FFFF Constant rx?
$FFFE Constant rxd
\ $FFF0 Constant tx

: end-label previous ;

Create marks $10 cells allot

: ahere X here 2/ ;

: m ( n -- ) cells marks + ahere 2* swap ! 0 ;
: r ( n -- ) cells marks + @ ahere swap s" !" evaluate 0 ;

\ intel hex dump

: 0.r ( n1 n2 -- ) 0 swap <# 0 ?DO # LOOP #> type ;

: tohex ( dest addr u -- )  base @ >r hex
  ." :" swap >r >r
  r@ dup 2 0.r  over 4 0.r  ." 00"
  over 8 rshift + +
  r> r> swap bounds ?DO  I ( 1 xor ) c@ dup 2 0.r +  LOOP
  negate $FF and 2 0.r  r> base ! ;

: 2hex ( dest addr u -- )
  BEGIN  dup WHILE
         >r 2dup r@ $10 min tohex cr
         r> $10 /string 0 max rot $10 + -rot
  REPEAT  drop 2drop ;

\ : sym 
\    base @ >r hex
\    cr ." sym:s/PC=" ahere 4 0.r ." /" bl word count type ." /g" cr
\    r> base ! ;
: sym bl word drop ;

: label 
  >in @ bl word count X here symentry >in !
  ahere Constant ;

: code
  -1 ABORT" Need end-code or end-label before a new code definition" ;

also forth definitions

: label also assembler label ;

: (code) also assembler ;
: (end-code) previous ;

previous previous previous

