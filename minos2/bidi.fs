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

\ Description: https://unicode.org/reports/tr9/

require unicode/bidi-db.fs
require unicode/brackets.fs
require set-compsem.fs

: (b') ( "name" -- n )
    parse-name [: ." .." type ;] $tmp
    ['] bidis >wordlist find-name-in >body @ ;
: bm' ( "name" -- mask )
    1 (b') lshift ;
compsem: 1 (b') lshift postpone Literal ;
: b' ( "name" -- n ) (b') ;
compsem: (b') postpone Literal ;
: (b'-') ( t1 t3 -- n )
    (b') $10 lshift (b') or ;
: b'-' ( t1 t3 -- n ) (b'-') ;
compsem: (b'-') postpone Literal ;
: (b'') ( t1 t2 -- n )
    (b') 8 lshift (b') or ;
: b'' ( t1 t2 -- n ) (b'') ;
compsem: (b'') postpone Literal ;
: (b''') ( t1 t2 t3 -- n )
    (b') $10 lshift (b') 8 lshift or (b') or ;
: b''' ( t1 t2 t3 -- n ) (b''') ;
compsem: (b''') postpone Literal ;

0 stack: bracket-queue
0 stack: bracket-stack
0 stack: bracket-pairs

$Variable $bidi-buffer
$Variable $flag-buffer
$Variable $level-buffer
0 Value current-char
0 Value embedded-level

0 stack: iso-stack<>
$[]Variable iso-list[]
$Variable sos$
$Variable eos$

: start-bracket ( -- )
    bracket-stack $free
    bracket-pairs $free ;
: start-bidi ( -- )
    $bidi-buffer $free
    $flag-buffer $free
    $level-buffer $free
    bracket-queue $free
    iso-stack<> $free
    iso-list[] $[]free
    sos$ $free
    eos$ $free
    0 to embedded-level ;

: $bidi-pos ( -- pos )
    current-char $bidi-buffer $@ drop - ;

: bracket-enqueue ( xchar -- xchar )
    dup bracket<> IF
	dup bracket-queue >stack
	$bidi-buffer $@len bracket-queue >stack
    THEN ;

: bracket-check ( xchar type -- xchar type )
    dup $1F and b' ON = IF \ all brackets are ONs
	    swap bracket-enqueue swap
    THEN ;

: >bidi ( addr u -- )
    [: bounds ?DO
	    I xc@+ dup 1 bidi@ IF
		c@ bracket-check emit drop
	    ELSE  2drop 0 emit  THEN
	I - +LOOP ;] $bidi-buffer $exec ;

: flag-sep ( -- )
    $bidi-buffer $@ $flag-buffer $!
    $bidi-buffer $@ bounds U+DO
	I c@ $1F and I c!
    LOOP
    $flag-buffer $@ bounds U+DO
	I c@ $E0 and I c!
    LOOP
    $bidi-buffer $@len $level-buffer $room ;

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

\ rules according to https://unicode.org/reports/tr9/#P1

: (p2) ( -- level )
    0 -rot U+DO
	1 I c@ lshift { mask }
	bm' B mask and ?LEAVE \ end of paragraph
	bm' LRI bm' RLI or bm' FSI or mask and 0<> -
	bm' PDI mask and 0<> +
	dup 0< ?LEAVE \ end of embedded level
	dup 0= IF
	    bm' L bm' R bm' AL or or mask and IF
		drop \ p3
		bm' R bm' AL or mask and 0<> negate
		unloop  EXIT
	    THEN
	THEN
    LOOP  drop 0 ;
: p2 ( -- level )
    $bidi-buffer $@ bounds (p2) ;

\ isolated regions

begin-structure iso-region-element
    lvalue: <<reg
    lvalue: reg>>
end-structure

iso-region-element buffer: iso-current

: iso-start ( -- )
    $bidi-pos iso-current to <<reg ;
: iso-update ( -- ) \ intermediate update
    $bidi-pos iso-current to reg>>
    iso-current <<reg iso-current reg>> u< IF
	iso-current iso-region-element stack# @ iso-stack<> $[]+!
    THEN ;
: iso-stack>list ( n -- )
    0 swap iso-stack<> $[] !@ ?dup-IF  iso-list[] >stack  THEN ;
: iso-push ( -- ) \ final update&put on list
    iso-update stack# @ iso-stack>list ;

\ rules according to https://unicode.org/reports/tr9/#X1

: x1-rest ( level -- )
    stack# !  stack $80 erase  neutral stack-top c!
    isolate# off  overflow-isolate# off  overflow-embedded# off ;
: x1-start ( -- )
    $bidi-buffer $@ drop to current-char  iso-start ;
: x1 ( -- )
    p2 dup to embedded-level x1-rest x1-start ;

Create x-match
$20 0 [DO] ' noop , [LOOP]
: bind ( xt "name" -- )
    (b') cells x-match + ! ;

: change-current-char ( -- )
    case stack-top c@ 3 and
	ltr of  b' L current-char c!  endof
	rtl of  b' R current-char c!  endof
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
' x2 bind RLE
: x3 ( -- ) \ match on LRE
    stack# @ next-even dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  neutral stack-top c!  THEN ;
' x3 bind LRE
: x4 ( -- ) \ match on RLO
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  rtl stack-top c!  THEN ;
' x4 bind RLO
: x5 ( -- ) \ match on LRO
    stack# @ next-even dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  dup >level  stack# !  ltr stack-top c!  THEN ;
' x5 bind LRO

: x6 ( -- )
    stack# @ >level  change-current-char ;
: x5a ( -- ) \ match on RLI
    stack# @ >level
    x6 stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	1 overflow-isolate# +! drop
    ELSE  stack# !  rtl dis or stack-top c!  1 isolate# +!  THEN ;
' x5a bind RLI
: x5b ( -- ) \ match on LRI
    stack# @ >level
    x6 stack# @ next-even dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	1 overflow-isolate# +! drop
    ELSE  stack# !  ltr dis or stack-top c!  1 isolate# +!  THEN ;
' x5b bind LRI

\ X5c. With each FSI, apply rules P2 and P3 to the sequence of characters
\ between the FSI and its matching PDI, or if there is no matching PDI, the
\ end of the paragraph, as if this sequence of characters were a paragraph. If
\ these rules decide on paragraph embedding level 1, treat the FSI as an RLI
\ in rule X5a. Otherwise, treat it as an LRI in rule X5b.

: x5c ( -- )
    stack# @ >level
    $bidi-buffer $@ bounds drop current-char 1+
    (p2)  IF  x5a  ELSE  x5b  THEN
;
' x5c bind FSI

' x6 bind L
' x6 bind R
' x6 bind AL
' x6 bind AN
' x6 bind CS
' x6 bind EN
' x6 bind ES
' x6 bind ET
' x6 bind NSM
' x6 bind S
' x6 bind WS
' x6 bind ON
' x6 bind BN

: x6a ( -- )
    stack# @ >level
    overflow-isolate# @ IF
	-1 overflow-isolate# +!  EXIT  THEN
    isolate# @ 0= ?EXIT
    overflow-embedded# off
    BEGIN  stack# @  WHILE
	    stack-top c@  0 stack-top c!  -1 stack# +!
	    stack-top c@ 0= stack# +!
	dis and UNTIL  THEN
    iso-push iso-start x6 ;
' x6a bind PDI
: x7 ( -- )
    overflow-isolate# @ ?EXIT
    overflow-embedded# @ IF
	-1 overflow-embedded# +!  EXIT  THEN
    stack# @ 0> stack-top c@ dis and 0= and IF
	stack# @ >level
	0 stack-top c!  -1 stack# +!
	stack# @ 0> IF  stack-top c@ 0= stack# +!  THEN
    THEN
    iso-push 1 +to current-char iso-start ;
' x7 bind PDF
: x8 ( -- )
    1 +to current-char  iso-push
    $bidi-buffer $@ bounds drop current-char (p2) x1-rest
    iso-stack<> $[]# 0 ?DO  I iso-stack>list  LOOP  iso-start ;
' x8 bind B
: x9 ( -- ) ; \ we don't remove anything

\ isolating weak types

0 Value sos
0 Value eos
0 Value seg-start
0 Value sos#

: >sos/eos ( -- )
    sos$ $@ sos# safe/string IF  c@  ELSE  drop b' L  THEN  to sos
    eos$ $@ sos# safe/string IF  c@  ELSE  drop b' L  THEN  to eos ;
: run-isolated { xt: rule -- }
    0 to sos#
    iso-list[] $@ bounds U+DO
	>sos/eos sos
	I $@ bounds dup to seg-start  U+DO
	    $bidi-buffer $@ I reg>> umin I <<reg safe/string bounds U+DO
		I rule
	    LOOP
	iso-region-element +LOOP  drop
	1 +to sos#
    cell +LOOP ;

: w1 ( -- )
    [: { p c -- p' } c c@ b' NSM = IF
	    b' ON  p
	    1 p lshift bm' LRI bm' RLI or bm' PDI or and
	    select c c!
	THEN c c@ ;] run-isolated ;

: w2 ( -- )
    [: { p c -- p' } c c@ b' EN = IF
	    p b' AL = IF  b' AN c c!  THEN  p
	ELSE
	    c c@  p
	    1 c c@ lshift bm' R bm' L or bm' AL or and select
	THEN
    ;] run-isolated ;

: w3 ( -- )
    [: { c -- }
	c c@ b' AL = IF  b' R c c!  THEN
    ;] run-isolated ;

: w4 ( -- )
    [: { p c -- p' }
	p 8 lshift c c@ or dup
	case
	    b''' EN ES EN  of  b' EN  c 1- c!  endof
	    b''' EN CS EN  of  b' EN  c 1- c!  endof
	    b''' AN CS AN  of  b' AN  c 1- c!  endof
	endcase
	$FFFF and ;] run-isolated ;

: w5 ( -- )
    [: { p c -- p' }
	p 8 lshift c c@ or dup
	case \ endianess here is don't care     ↓
	    b''' ET ET EN  of  b'' EN EN  c 2 - w!  endof
	    b''' EN ET ET  of  b'' EN EN  c 1-  w!  endof
	    b''' AN ET EN  of   b' EN     c 1-  c!  endof
	endcase
	$FFFF and ;] run-isolated ;

: w6 ( -- )
    [: { p c -- p' }
	p 8 lshift c c@ or dup
	case
	    b''' L  ES EN  of  b' ON  c 1- c!  endof
	    b''' EN CS AN  of  b' ON  c 1- c!  endof
	    $FFFF and
	    b'' AN ET      of  b' ON  c    c!  endof
	    b'' ET AN      of  b' ON  c 1- c!  endof
	endcase
	$FFFF and ;] run-isolated ;

: w7 ( -- )
    [: { p c -- p' }
	p b' L =  c c@ b' EN = and  IF  b' L c c!  THEN
	c c@ p
	1 c c@ lshift  bm' L bm' R or and  select
    ;] run-isolated ;

\ identify brackets

: bracket-end ( pos xchar -- pos xchar )
    dup bracket> IF
	bracket-stack $@ over + U-DO
	    dup I cell- @ = IF
		I bracket-stack $@ drop -
		bracket-stack $!len
		bracket-stack stack> drop
		bracket-stack stack> bracket-pairs >stack
		over bracket-pairs >stack
		LEAVE  THEN
	2 cells -LOOP
    THEN ;
: bracket-check ( pos xchar -- pos xchar )
    dup bracket< ?dup-IF
	third bracket-stack >stack
	bracket-stack >stack
    ELSE
	bracket-end
    THEN ;
: bracket-scan 1+ { d: range -- }
    \ range swap hex. hex. space
    start-bracket
    bracket-queue $@ bounds U+DO
	I 2@ over range within IF  bracket-check  THEN  2drop
    2 cells +LOOP ;
: run-isolated' { xt: rule -- }
    iso-list[] $@ bounds U+DO
	I $@ bounds dup to seg-start  U+DO
	    I <<reg I reg>> rule
	iso-region-element +LOOP
    cell +LOOP ;

: gen-sos/eos { start end -- }
    start 0<= IF  embedded-level
    ELSE  $level-buffer $@ start 1- safe/string
	IF  c@  ELSE  drop  embedded-level  THEN  THEN
    $level-buffer $@ start safe/string drop c@ umax
    1 and b' R b' L rot select
    sos$ $@ sos# safe/string drop c!
    $level-buffer $@ end safe/string
    IF  c@  ELSE  drop  embedded-level  THEN
    $level-buffer $@ end 1- safe/string drop c@ umax
    1 and b' R b' L rot select
    eos$ $@ sos# safe/string drop c!
    1 +to sos# ;

: bd16 ( -- )  ['] bracket-scan run-isolated' ;
: sos/eos ( -- )  0 to sos#
    iso-list[] $[]# dup sos$ $room eos$ $room
    ['] gen-sos/eos run-isolated' ;

: check-strong-type ( a b start end -- a a / b b / a/b on ) { start end }
    $level-buffer $@ start safe/string drop c@ 1 and select
    b' ON
    $bidi-buffer $@ end umin start 1+ safe/string bounds U+DO
	over I c@ 1 over lshift bm' EN bm' AN or and IF  drop b' R  THEN
	= IF  drop dup LEAVE  THEN
    LOOP ;
: set-bracket-type ( type start end -- type ) 2>r
    dup $bidi-buffer $@ r> safe/string IF  c!  ELSE  2drop  THEN
    dup $bidi-buffer $@ r> safe/string IF  c!  ELSE  2drop  THEN ;

: check-brackets-within { end start -- }
    b' R b' L start end check-strong-type
    2dup = IF  start end set-bracket-type  2drop  EXIT  THEN  2drop
    b' L b' R start end check-strong-type
    2dup = IF  2drop b' L b' R -1 start check-strong-type
	2dup = IF  start end set-bracket-type  2drop  EXIT  THEN
	drop b' R xor start end set-bracket-type  drop  EXIT
    THEN  2drop ;

: n0 ( -- ) bd16
    bracket-pairs $@ bounds U+DO
	I 2@ check-brackets-within
    2 cells +LOOP ;

bm' B bm' S or bm' WS or bm' ON or bm' FSI or bm' LRI or bm' RLI or bm' PDI or
Constant NI-mask

: n1-replaces ( pattern start index -- pattern )
    over - { d: fill-addr } $FFFF and
    case  dup
	b'' L    L  of  fill-addr b' L fill  endof
	1 over $FF and lshift
	bm' R bm' AN bm' EN or or and 0<>
	1 rot  $08 rshift lshift
	bm' R bm' AN bm' EN or or and 0<> and
	?of  fill-addr b' R fill  endof
	0
    endcase ;

: n1 ( -- ) { | first-NI }
    sos $bidi-buffer $@ IF  c@ swap 8 lshift or  ELSE  drop  THEN
    1 over $FF and lshift NI-mask and
    IF  8 rshift  $bidi-buffer $@ drop to first-NI  THEN
    $bidi-buffer $@ 1 safe/string bounds U+DO
	1 I c@ lshift NI-mask and IF
	    first-NI 0= IF  I to first-NI  THEN
	ELSE
	    8 lshift I c@ or
	    first-NI ?dup-IF  I n1-replaces  THEN
	    0 to first-NI
	THEN
    LOOP 8 lshift eos or
    first-NI ?dup-IF  $bidi-buffer $@ + n1-replaces  THEN  drop ;

: n2 ( -- )
    $level-buffer $@ drop
    $bidi-buffer $@ bounds U+DO
	1 I c@ lshift NI-mask and IF
	    dup c@ 1 and b' R b' L rot select I c!
	THEN  1+
    LOOP  drop ;

$40 buffer: <i1+2>

1 b' R  2*    <i1+2> + c!
2 b' AN 2*    <i1+2> + c!
2 b' EN 2*    <i1+2> + c!

1 b' L  2* 1+ <i1+2> + c!
1 b' AN 2* 1+ <i1+2> + c!
1 b' EN 2* 1+ <i1+2> + c!

: i1+2 ( -- )
    $level-buffer $@ drop
    $bidi-buffer $@ bounds U+DO
	dup c@ 1 and I c@ 2* or <i1+2> + c@  over c+!  1+
    LOOP  drop ;

: x10 ( -- )
    sos/eos
    w1 w2 w3 w4 w5 w6 w7
    n0 n1 n2
    i1+2 ;

\ strong right or left indicate direction break
bm' R bm' RLE bm' RLO bm' RLI bm' AL or or or or Constant R-mask
bm' L bm' LRE bm' LRO bm' LRI or or or Constant L-Mask

: (skip-bidi?) { mask -- flag }
    $bidi-buffer $@ bounds U+DO
	1 I c@ lshift mask and IF  false  UNLOOP  EXIT  THEN
    LOOP  true ;

r> set-current

Defer skip-bidi? ' (skip-bidi?) is skip-bidi?

: bidi-rest ( -- )
    $bidi-buffer $@ bounds U+DO
	I to current-char
	I c@ cells x-match + perform
    LOOP
    x8 x9 x10 ;
: bidi-algorithm ( -- )
    \G auto-detect paragraph direction and do bidi algorithm
    flag-sep R-mask skip-bidi? ?EXIT  x1 bidi-rest ;
: bidi-algorithm# ( level -- )
    \G use @ivar{level} as main direction and do bidi algorithm
    flag-sep dup to embedded-level
    L-mask R-mask third select skip-bidi?  IF  drop  EXIT  THEN
    x1-rest x1-start bidi-rest ;

previous
