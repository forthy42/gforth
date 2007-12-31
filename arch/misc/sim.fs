\ MISC simulator

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

decimal

: .####  base @ >r hex
	0 <# # # # # #> type space r> base ! ;

variable ndp	: here ndp @ ;
variable src	variable dst	variable pc	$10 pc !
Variable pc-old

variable zf
variable sf
variable cf

variable accu

variable mem-size	128 1024 * mem-size !
mem-size @ allocate throw
constant mem 

0 ndp !

\ Jumping

: pc>		src @ 2* pc @ + ;
: >pc 		pc ! ;
: >pcsf		sf @ IF >pc ELSE drop THEN ;
: >pczf		zf @ IF >pc ELSE drop THEN ;
: >pccf		cf @ IF >pc ELSE drop THEN ;

\ Memory

: ram>		2* mem + dup c@ 8 lshift swap char+ c@ or ;

: >ram		\ dup $4000 u< ABORT" Memory below $4000 is read-only"
                2* mem + over 8 rshift over c! char+ c! ;

\ IO

variable nesting 0 nesting !
: .hs
	." RP: " $4000 ram> .####
	." SP: " $4001 ram> .####
	." UP: " $4002 ram> .#### ;

: .ip
	$4003 ram> ." IP: " .#### ;
: trace
		cr nesting @ spaces 
		dup CASE [char] : OF 1 nesting +! .ip ENDOF 
		[char] ; OF -1 nesting +! ENDOF ENDCASE ;

: >txd		
\                trace
		[IFDEF] curoff curoff [THEN]
		dup bl < IF
		    CASE
			#cr OF  ENDOF
			#lf OF  cr  ENDOF
			[IFDEF] del #bs OF  del  ENDOF [THEN]
			dup emit  ENDCASE
		    ELSE  emit  THEN
		[IFDEF] tflush tflush [ELSE] key? drop [THEN]
		[IFDEF] curon curon [THEN] [IFDEF] pause pause [THEN] ;
: tx?>		1 ;
: rxd>		key [IFDEF] curon curon [THEN] ;
: rx?>		key? 1 and [IFDEF] pause pause [THEN] ;

\ Arithmetic

: accu!	( u -- ) dup 0= zf ! dup $8000 and 0<> sf ! $FFFF and accu ! ;		

: >shr  cf @ >r dup 1 and 0<> cf !
    1 rshift r> IF $8000 or THEN accu! ;
: >xor  accu @ xor accu! ;
: >or   accu @ or accu! ;
: >and  accu @ and accu! ;

: (>sub) 2dup u< cf ! - accu! ;
: >sub9	 accu @ swap (>sub) ;
: >subA  accu @ (>sub) ;
 
: >add	 accu @ + $FFFF and dup accu @ u< cf ! accu! ;

: sf>	sf @ 1 and ;
: zf>	zf @ 1 and ;
: cf>	cf @ 1 and ;

: accu>		accu @ ;
: >accu		accu! ;

: aind> accu @ ram> ;
: >aind accu @ >ram ;

: crash  -$200 throw ;

create table>
	' crash ,	' tx?> ,	' rxd> ,	' rx?> ,
	' pc> ,		' pc> ,		' pc> ,		' pc> ,
	' crash ,	' crash ,	' crash ,	' aind> ,
	' accu> ,	' sf> ,		' zf> ,		' crash ,
	' cf> ,		' crash ,	' crash ,	' crash ,

create >table
	' >txd ,	' crash ,	' crash ,	' crash ,
	' >pc ,		' >pcsf , 	' >pczf ,	' crash ,
	' >pccf ,	' crash ,	' crash ,	' >aind ,
	' >accu ,	' >sub9 ,	' >suba ,	' >add ,
	' >xor ,	' >or ,		' >and ,	' >shr ,
	
: special? ( n -- ) $10 $FFFC within 0= ;

' special? ALIAS special>?	' special? ALIAS >special?

: dotable ( /trans table n -- trans/ )
    4 + $FFFF and cells + perform ;

: do>	( -- val )
	src @ >special?
	IF	table> src @ dotable
	ELSE	src @ ram> 
	THEN  ;

: >do	( val -- )
	dst @ >special?
	IF	>table dst @ dotable
	ELSE	dst @ >ram
	THEN ;

variable trans -1 trans !

: .stat
	." PC: " pc-old @ .#### 
	." : " src @ .####
	." -( " trans @ .####
	." )-> " dst @ .####
        ." ACCU: " accu @ .#### ;

variable steps 0 steps !

: step  1 steps +!
	pc @ pc-old !
	pc @ ram> src !
	pc @ 1+ ram> dst !
	do> 	pc @ 2 + pc !
		dup trans ! 
	>do ;

: s step .stat cr ;

: load	
	bl word count r/o bin open-file throw >r
	mem mem-size @ r@ read-file throw
	r> close-file throw 
	. cr ;

: n,	ndp @ >ram 1 ndp +! ;


\ DUMP                       2may93jaw - 9may93jaw    06jul93py

Variable /dump

: .4 ( addr -- addr' )
    3 FOR  -1 /dump +!  /dump @ 0<
        IF  ."    "  ELSE  dup c@ 0 <# # # #> type space  THEN
    char+ NEXT ;
: .chars ( addr -- )
    /dump @ bounds
    ?DO I c@ dup $7F bl within
	IF  drop [char] .  THEN  emit
    LOOP ;

: .line ( addr -- )
  dup .4 space .4 ." - " .4 space .4 drop  10 /dump +!  space .chars ;

: d  ( addr u -- )
    swap 2* mem + swap
    cr base @ >r hex        \ save base on return stack
    0 ?DO  I' I - 10 min /dump !
        dup mem - 2/ 8 u.r ." : " dup .line cr  10 +
        10 +LOOP
    drop r> base ! ;

defer end? ' noop IS end?

variable t1 variable t2

: token2 t1 @ src @ = t2 @ dst @ = and or ;

: jmp?   dst @ 5 < or ;
: surejmp? dst @ 0= or ;

: st
  dup ram> t1 ! 1+ ram> t2 ! 
  ['] token2 IS end? ;

: stepinto BEGIN step false end? UNTIL ;

: g
    [IFDEF] curon curon [THEN]
    BEGIN step AGAIN
    [IFDEF] curoff curoff [THEN] ;

: si stepinto ." Stopped" cr .stat cr ;

variable stepcnt

: sq s 
	BEGIN key steps @ stepcnt ! CASE 
		[char] q OF EXIT ENDOF
		[char] j OF ['] jmp? IS end? stepinto ENDOF
		[char] s OF ['] surejmp? IS end? stepinto ENDOF
		[char] g OF ['] g catch -$200 = IF ." crashed " THEN  ENDOF
		step
		ENDCASE
		." [" steps @ stepcnt @ - 0 <# #S #> type ." ]"
		.stat cr
	AGAIN ;

