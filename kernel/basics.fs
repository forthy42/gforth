\ kernel.fs    GForth kernel                        17dec92py

\ Copyright (C) 1995,1998 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ Idea and implementation: Bernd Paysan (py)

\ Needs:

require ./vars.fs

hex

\ labels for some code addresses

\- NIL NIL AConstant NIL \ gforth

\ Aliases

[IFUNDEF] r@
' i Alias r@ ( -- w ; R: w -- w ) \ core r-fetch
\G Copy @var{w} from the return stack to the data stack.
[THEN]

\ !! this is machine-dependent, but works on all but the strangest machines

: maxaligned ( addr -- f-addr ) \ gforth
    [ /maxalign 1 - ] Literal + [ 0 /maxalign - ] Literal and ;
\ !! machine-dependent and won't work if "0 >body" <> "0 >body maxaligned"
' maxaligned Alias cfaligned ( addr1 -- addr2 ) \ gforth

: chars ( n1 -- n2 ) \ core
\G @i{n2} is the number of address units corresponding to @i{n1} chars.""
; immediate


\ : A!    ( addr1 addr2 -- ) \ gforth
\    dup relon ! ;
\ : A,    ( addr -- ) \ gforth
\    here cell allot A! ;
' ! alias A! ( addr1 addr2 -- ) \ gforth

\ UNUSED                                                17may93jaw

has? ec 
[IF]
unlock ram-dictionary area nip lock
Constant dictionary-end
[ELSE]
: dictionary-end ( -- addr )
    forthstart [ 3 cells ] Aliteral @ + ;
[THEN]

: usable-dictionary-end ( -- addr )
    dictionary-end [ word-pno-size pad-minsize + ] Literal - ;

: unused ( -- u ) \ core-ext
    \G Return the amount of free space remaining (in address units) in
    \G the region addressed by @code{here}.
    usable-dictionary-end here - ;

\ here is used for pad calculation!

: dp    ( -- addr ) \ gforth
    dpp @ ;
: here  ( -- addr ) \ core
    \G Return the address of the next free location in data space.
    dp @ ;

\ on off                                               23feb93py

\ on is used by docol:
: on  ( a-addr -- ) \ gforth
    \G Set the (value of the) variable  at @i{a-addr} to @code{true}.
    true  swap ! ;
: off ( a-addr -- ) \ gforth
    \G Set the (value of the) variable at @i{a-addr} to @code{false}.
    false swap ! ;

\ dabs roll                                           17may93jaw

: dabs ( d1 -- d2 ) \ double d-abs
    dup 0< IF dnegate THEN ;

: roll  ( x0 x1 .. xn n -- x1 .. xn x0 ) \ core-ext
  dup 1+ pick >r
  cells sp@ cell+ dup cell+ rot move drop r> ;

\ place bounds                                         13feb93py

: place  ( addr len to -- ) \ gforth
    over >r  rot over 1+  r> move c! ;
: bounds ( beg count -- end beg ) \ gforth
    over + swap ;

\ (word)                                               22feb93py

: scan   ( addr1 n1 char -- addr2 n2 ) \ gforth
    \ skip all characters not equal to char
    >r
    BEGIN
	dup
    WHILE
	over c@ r@ <>
    WHILE
	1 /string
    REPEAT  THEN
    rdrop ;
: skip   ( addr1 n1 char -- addr2 n2 ) \ gforth
    \ skip all characters equal to char
    >r
    BEGIN
	dup
    WHILE
	over c@ r@  =
    WHILE
	1 /string
    REPEAT  THEN
    rdrop ;

\ digit?                                               17dec92py

: digit?   ( char -- digit true/ false ) \ gforth
  base @ $100 =
  IF
    true EXIT
  THEN
  toupper [char] 0 - dup 9 u> IF
    [ char A char 9 1 + -  ] literal -
    dup 9 u<= IF
      drop false EXIT
    THEN
  THEN
  dup base @ u>= IF
    drop false EXIT
  THEN
  true ;

: accumulate ( +d0 addr digit - +d1 addr )
  swap >r swap  base @  um* drop rot  base @  um* d+ r> ;

: >number ( ud1 c-addr1 u1 -- ud2 c-addr2 u2 ) \ core to-number
    \G Attempt to convert the character string @var{c-addr1, u1} to an
    \G unsigned number in the current number base. The double
    \G @var{ud1} accumulates the result of the conversion to form
    \G @var{ud2}. Conversion continues, left-to-right, until the whole
    \G string is converted or a character that is not convertable in
    \G the current number base is encountered (including + or -). For
    \G each convertable character, @var{ud1} is first multiplied by
    \G the value in @code{BASE} and then incremented by the value
    \G represented by the character. @var{c-addr2} is the location of
    \G the first unconverted character (past the end of the string if
    \G the whole string was converted). @var{u2} is the number of
    \G unconverted characters in the string. Overflow is not detected.
    0
    ?DO
	count digit?
    WHILE
	accumulate
    LOOP
        0
    ELSE
	1- I' I -
	UNLOOP
    THEN ;

\ s>d um/mod						21mar93py

: s>d ( n -- d ) \ core		s-to-d
    dup 0< ;

: ud/mod ( ud1 u2 -- urem udquot ) \ gforth
    >r 0 r@ um/mod r> swap >r
    um/mod r> ;

\ catch throw                                          23feb93py
\ bounce                                                08jun93jaw

\ !! allow the user to add rollback actions    anton
\ !! use a separate exception stack?           anton

has? glocals [IF]
: lp@ ( -- addr ) \ gforth	lp-fetch
 laddr# [ 0 , ] ;
[THEN]

defer catch ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
:noname ( ... xt -- ... 0 )
    execute 0 ;
is catch

defer throw ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
:noname ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error )
    ?dup if
	[ here forthstart 9 cells + ! ]
	cr ." error " decimal . cr 1 (bye)
    then ;
is throw

\ (abort")

: (abort")
    "lit >r
    IF
	r> "error ! -2 throw
    THEN
    rdrop ;

: abort ( ?? -- ?? ) \ core,exception-ext
    \G Empty the data stack and perform the functions of @code{quit}.
    \G Since the exception word set is present, this is performed by
    \G @code{-1 throw}.
    -1 throw ;

\ ?stack                                               23feb93py

: ?stack ( ?? -- ?? ) \ gforth
    sp@ sp0 @ u> IF    -4 throw  THEN
[ has? floating [IF] ]
    fp@ fp0 @ u> IF  -&45 throw  THEN
[ [THEN] ]
;
\ ?stack should be code -- it touches an empty stack!

\ DEPTH                                                 9may93jaw

: depth ( -- +n ) \ core depth
    \G @var{+n} is the number of values that were on the data stack before
    \G @var{+n} itself was placed on the stack.
    sp@ sp0 @ swap - cell / ;

: clearstack ( ... -- ) \ gforth clear-stack
    \G remove and discard all/any items from the data stack.
    sp0 @ sp! ;

\ Strings						 22feb93py

: "lit ( -- addr )
  r> r> dup count + aligned >r swap >r ;

\ */MOD */                                              17may93jaw

\ !! I think */mod should have the same rounding behaviour as / - anton
: */mod ( n1 n2 n3 -- n4 n5 ) \ core	star-slash-mod
    >r m* r> sm/rem ;

: */ ( n1 n2 n3 -- n4 ) \ core	star-slash
    */mod nip ;

\ HEX DECIMAL                                           2may93jaw

: decimal ( -- ) \ core
    \G Set the numeric conversion radix (the value of @code{BASE}) to 10
    \G (decimal).
    a base ! ;
: hex ( -- ) \ core-ext
    \G Set the numeric conversion radix (the value of @code{BASE}) to 16
    \G (hexadecimal).
    10 base ! ;

