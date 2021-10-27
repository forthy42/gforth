\ Unicode bidi algorthm

\ Authors: Bernd Paysan
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

require unicode/bidi-db.fs
require unicode/brackets.fs
require set-compsem.fs

$Variable $bidi-buffer
$Variable $flag-buffer
$Variable $level-buffer

: >bidi ( addr u -- )
    [: bounds ?DO
	    I xc@+
	    1 bidi@ IF  c@ emit  ELSE  drop 0 emit  THEN
	I - +LOOP ;] $bidi-buffer $exec ;

: flag-sep ( -- )
    $bidi-buffer $@ $flag-buffer $!
    $bidi-buffer $@ bounds U+DO
	I c@ $1F and I c!
    LOOP
    $flag-buffer $@ bounds U+DO
	I c@ $E0 and I c!
    LOOP
    $bidi-buffer $@len $level-buffer $!len
    $level-buffer $@ erase ;

Vocabulary bidi

get-current >r also bidi definitions

#125 Constant max-depth#
$80 buffer: stack
Variable stack#
: stack-top ( -- addr )  stack stack# @ + ;
Variable overflow-isolate#
Variable isolate#
Variable overflow-embedded#

$4 Constant dis
$0 Constant emtpy
$1 Constant neutral
$2 Constant rtl
$3 Constant ltr

: next-odd ( n -- n' ) 1+ 1 or ;
: next-even ( n -- n' ) 2 + -2 and ;

: (b') ( "name" -- n )
    parse-name ['] bidis >wordlist find-name-in >body @ ;
: bm' ( "name" -- mask )
    1 (b') lshift ;
compsem: 1 (b') lshift postpone Literal ;
: b' ( "name" -- n ) (b') ;
compsem: (b') postpone Literal ;

\ rules according to https://unicode.org/reports/tr9/#P1

: (p2) ( -- level )
    0 -rot U+DO
	1 I c@ lshift { mask }
	bm' ..B mask and ?LEAVE \ end of paragraph
	bm' ..LRI bm' ..RLI or bm' ..FSI or mask and 0<> -
	bm' ..PDI mask and 0<> +
	dup 0< ?LEAVE \ end of embedded level
	dup 0= IF
	    bm' ..L bm' ..R bm' ..AL or or mask and IF
		drop \ p3
		bm' ..R bm' ..AL or mask and 0<> negate
		unloop  EXIT
	    THEN
	THEN
    LOOP  nip 0 ;
: p2 ( -- level )
    $bidi-buffer $@ bounds (p2) ;

\ rules according to https://unicode.org/reports/tr9/#X1

: x1-rest  stack# !  stack $80 erase  neutral stack-top c!
    isolate# off  overflow-isolate# off  overflow-embedded# off ;
: x1 ( -- )
    p2 x1-rest ;

Create x-match
$20 0 [DO] ' noop , [LOOP]
: bind ( xt "name" -- )
    (b') cells x-match + ! ;

0 Value current-char

: change-current-char ( -- )
    case stack-top c@ 3 and
	ltr of  b' ..L current-char c!  endof
	rtl of  b' ..R current-char c!  endof
    endcase ;
: >level ( n -- )
    $level-buffer $@
    current-char $bidi-buffer $@ drop - /string
    IF  c!  ELSE  2drop  THEN ;

: x2 ( -- ) \ match on RLE
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  neutral stack-top c!  THEN ;
' x2 bind ..RLE
: x3 ( -- ) \ match on LRE
    stack# @ next-even dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  neutral stack-top c!  THEN ;
' x3 bind ..LRE
: x4 ( -- ) \ match on RLO
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  rtl stack-top c!  THEN ;
' x4 bind ..RLO
: x5 ( -- ) \ match on LRO
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  ltr stack-top c!  THEN ;
' x5 bind ..LRO

: x6 ( -- )
    stack# @ >level  change-current-char ;
: x5a ( -- ) \ match on RLI
    x6 stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	1 overflow-isolate# +! drop
    ELSE  stack# !  rtl dis or stack-top c!  1 isolate# +!  THEN ;
' x5a bind ..RLI
: x5b ( -- ) \ match on LRI
    x6 stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	1 overflow-isolate# +! drop
    ELSE  stack# !  ltr dis or stack-top c!  1 isolate# +!  THEN ;
' x5b bind ..LRI

\ X5c. With each FSI, apply rules P2 and P3 to the sequence of characters
\ between the FSI and its matching PDI, or if there is no matching PDI, the
\ end of the paragraph, as if this sequence of characters were a paragraph. If
\ these rules decide on paragraph embedding level 1, treat the FSI as an RLI
\ in rule X5a. Otherwise, treat it as an LRI in rule X5b.

: x5c ( -- )
    $bidi-buffer $@ bounds drop current-char 1+
    (p2)  IF  x5a  ELSE  x5b  THEN
;
' x5c bind ..FSI

' x6 bind ..L
' x6 bind ..R
' x6 bind ..AL
' x6 bind ..AN
' x6 bind ..CS
' x6 bind ..EN
' x6 bind ..ES
' x6 bind ..ET
' x6 bind ..NSM
' x6 bind ..S
' x6 bind ..WS
' x6 bind ..ON
' x6 bind ..BN

: x6a ( -- )
    overflow-isolate# @ IF
	-1 overflow-isolate# +!  EXIT  THEN
    isolate# @ 0= ?EXIT
    overflow-embedded# off
    BEGIN  stack# @  WHILE
	    stack-top c@  0 stack-top c!  -1 stack# +!
	dis and UNTIL  THEN
    x6 ;
' x6a bind ..PDI
: x7 ( -- )
    overflow-isolate# @ ?EXIT
    overflow-embedded# @ IF
	-1 overflow-embedded# +!  EXIT  THEN
    stack# @ 2 u>= stack-top c@ dis and 0= and IF
	0 stack-top c!  -1 stack# +!
	stack-top c@ 0= stack# +!  THEN ;
' x7 bind ..PDF
: x8 ( -- )
    $bidi-buffer $@ bounds drop current-char 1+
    (p2) x1-rest ;
' x8 bind ..B
: x9 ( -- ) ; \ we don't remove anything

: x1-9 ( -- )
    x1
    $bidi-buffer $@ bounds U+DO
	I to current-char
	I c@ cells x-match + perform
    LOOP
    x9 ;

128 stack: isolated-runs

: >isolated-run ( start len -- )
    { d^ ir } ir 2 cells $make isolated-runs >stack ;

: x10 ( -- ) \ TBD: level runs
    0 $bidi-buffer $@len >isolated-run ;

\ isolating weak types

0 Value sos
0 Value eos
0 Value seg-start

: run-isolated { xt: rule -- }
    isolated-runs $@ bounds U+DO
	sos  I $@ bounds dup to seg-start  U+DO
	    $bidi-buffer $@ I 2@ >r safe/string r> umin bounds U+DO
		I rule
	    LOOP
	2 cells +LOOP  drop
    cell +LOOP ;

: w1 ( -- )
    [: { p c -- p' } c c@ b' ..NSM = IF
	    b' ..ON  p
	    1 p lshift bm' ..LRI bm' ..RLI or bm' ..PDI or and
	    select c c!
	THEN c c@ ;] run-isolated ;

: w2 ( -- )
    [: { p c -- p' } c c@ b' ..EN = IF
	    p b' ..AL = IF  b' ..AN c c!  THEN  p
	ELSE
	    c c@  p
	    1 c c@ lshift bm' ..R bm' ..L or bm' ..AL or and select
	THEN
    ;] run-isolated ;

: w3 ( -- )
    [: { c -- }
	c c@ b' ..AL = IF  b' ..R c c!  THEN
    ;] run-isolated ;

: w4 ( -- )
    [: { p c -- p' }
	p 8 lshift c c@ or dup
	case
	    b' ..EN #16 lshift b' ..ES 8 lshift or b' ..EN or
	    of  b' ..EN  c 1- c!  endof
	    b' ..EN #16 lshift b' ..CS 8 lshift or b' ..EN or
	    of  b' ..EN  c 1- c!  endof
	    b' ..AN #16 lshift b' ..CS 8 lshift or b' ..AN or
	    of  b' ..AN  c 1- c!  endof
	endcase
	$FFFF and ;] run-isolated ;

: w5 ( -- )
    [: { p c -- p' }
	p 8 lshift c c@ or dup
	case
	    b' ..ET #16 lshift b' ..ET 8 lshift or b' ..EN or
	    of  b' ..EN dup 8 lshift or  c 2 - w!  endof
	    b' ..EN #16 lshift b' ..ET 8 lshift or b' ..ET or
	    of  b' ..EN dup 8 lshift or  c 1 - w!  endof
	    b' ..AN #16 lshift b' ..ET 8 lshift or b' ..EN or
	    of  b' ..EN  c 1- c!  endof
	endcase
	$FFFF and ;] run-isolated ;

: w6 ( -- )
    [: { p c -- p' }
	p 8 lshift c c@ or dup
	case
	    b' ..L #16 lshift b' ..ES 8 lshift or b' ..EN or
	    of  b' ..ON  c 1- c!  endof
	    b' ..EN #16 lshift b' ..CS 8 lshift or b' ..AN or
	    of  b' ..ON  c 1- c!  endof
	    $FFFF and
	    b' ..AN #8 lshift  b' ..ET or
	    of  b' ..ON  c c!  endof
	    b' ..ET #8 lshift  b' ..AN or
	    of  b' ..ON  c 1- c!  endof
	endcase
	$FFFF and ;] run-isolated ;

: w7 ( -- )
    [: { p c -- p' }
	p b' ..L =  c c@ b' ..EN = and  IF  b' ..L c c!  THEN
	c c@ p
	1 c c@ lshift  bm' ..L bm' ..R or and  select
    ;] run-isolated ;

: ws ( -- )
    w1 w2 w3 w4 w5 w6 w7 ;

\ identify brackets

previous r> set-current
