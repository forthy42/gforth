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

get-current also bidi definitions

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

\ rules according to https://unicode.org/reports/tr9/#X1

: x1 ( -- )
    stack# off  stack $80 erase
    isolate# off  overflow-isolate# off  overflow-embedded# off ;

Create x-match
$20 0 [DO] ' noop , [LOOP]
: bind ( xt "name" -- )
    parse-name ['] bidis >wordlist find-name-in dup IF
	@ cells x-match + !
    ELSE  2drop  THEN ;

0 Value current-char

: x2 ( -- ) \ match on RLE
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  stack# !  neutral stack-top c!  THEN ;
' x2 bind ..RLE
: x3 ( -- ) \ match on LRE
    stack# @ next-even dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  stack# !  neutral stack-top c!  THEN ;
' x3 bind ..LRE
: x4 ( -- ) \ match on RLO
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  stack# !  rtl stack-top c!  THEN ;
' x4 bind ..RLO
: x5 ( -- ) \ match on LRO
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	drop overflow-isolate# @ 0= negate  overflow-embedded# +!
    ELSE  stack# !  ltr stack-top c!  THEN ;
' x5 bind ..LRO

: change-current-char ( -- )
    case stack-top c@ 3 and
	ltr of  [ also bidis ' ..L previous @ ]L current-char c!  endof
	rtl of  [ also bidis ' ..R previous @ ]L current-char c!  endof
    endcase ;

: x5a ( -- ) \ match on RLI
    change-current-char
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	1 overflow-isolate# +! drop
    ELSE  stack# !  rtl dis or stack-top c!  1 isolate# +!  THEN ;
' x5a bind ..RLI
: x5b ( -- ) \ match on LRI
    change-current-char
    stack# @ next-odd dup max-depth# u>
    overflow-isolate# @ overflow-embedded# @ or or IF
	1 overflow-isolate# +! drop
    ELSE  stack# !  ltr dis or stack-top c!  1 isolate# +!  THEN ;
' x5b bind ..LRI

\ X5c. With each FSI, apply rules P2 and P3 to the sequence of characters
\ between the FSI and its matching PDI, or if there is no matching PDI, the
\ end of the paragraph, as if this sequence of characters were a paragraph. If
\ these rules decide on paragraph embedding level 1, treat the FSI as an RLI
\ in rule X5a. Otherwise, treat it as an LRI in rule X5b.

: x5c ( -- ) !!FIXME!!
    ( ... ) IF  x5a  ELSE  x5b  THEN
;
' x5c bind ..FSI

: >level ( n -- )
    $level-buffer $@
    current-char $bidi-buffer $@ drop - /string
    IF  c!  ELSE  2drop  THEN ;

: x6 ( -- )
    stack# @ >level  change-current-char ;
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
	stack-top c@ 0= stack# +!  THEN
;
' x7 bind ..PDF
synonym x8 x1 \ paragraph ending are similar to paragraph start
' x8 bind ..B

: x[2..8] ( -- )
    x2
    $bidi-buffer $@ bounds U+DO
	I to current-char
	I c@ cells x-match + perform
    LOOP ;

previous set-current
