\ recognizer-based interpreter, sequence

\ Authors: Bernd Paysan
\ Copyright (C) 2022,2023,2024 Free Software Foundation, Inc.

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

\ recognizer loop

Defer trace-recognizer  ' drop is trace-recognizer

: recognize ( addr u rec-addr -- ... rectype ) \ gforth-experimental
    \G apply a recognizer stack to a string, delivering a token
    1 rec-level +!  -rot >l >l
    $@ bounds cell- swap cell- U-DO
	@local0 @local1 I perform
	dup  IF
	    -1 rec-level +!
	    I @ trace-recognizer  UNLOOP  [ cell 8 = ] [IF] lp+2 [ELSE] lp+ [THEN] EXIT  THEN  drop
	cell [ 2 cells ] Literal I cell- 2@ <> select \ skip double entries
	\ note that we search first and then skip, because the first search
	\ has a very likely hit.  So doubles will be skipped, tripples not
    -loop
    -1 rec-level +!
    0 [ cell 8 = ] [IF] lp+2 [ELSE] lp+ [THEN] ;

: recognizer-sequence: ( xt1 .. xtn n "name" -- ) \ gforth-experimental
    \G Define @i{name}, a recognizer sequence that first searches
    \G @i{xtn} and last searches @i{xt1}.  @i{name} is a recognizer
    \G itself, which makes recognizer sequences nestable.  The order
    \G of operands is inspired by @word{get-order} and
    \G @word{set-order}.
    ['] recognize do-stack: ;

\ : rec-sequence ( xt1 .. xtn n "name" -- ) \ gforth
\     n>r : nr> ]] 2>r [[ 0 ?DO
\ 	]] 2r@ [[ compile,
\ 	]] dup IF 2rdrop EXIT THEN drop [[
\     LOOP ]] 2rdrop ; [[ ;

' rec-num ' rec-nt 2 recognizer-sequence: default-recognize
' default-recognize is forth-recognize

: rec-nothing ( c-addr u -- 0 ) \ gforth-experimental
    \G This recognizer recognizes nothing.  It can be useful as a
    \G placeholder.
    2drop 0 ;
