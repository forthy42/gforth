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
	dup translate-none <> IF
	    -1 rec-level +!
	    I @ trace-recognizer  UNLOOP  [ cell 8 = ] [IF] lp+2 [ELSE] lp+ [THEN] EXIT  THEN  drop
	cell [ 2 cells ] Literal I cell- 2@ <> select \ skip double entries
	\ note that we search first and then skip, because the first search
	\ has a very likely hit.  So doubles will be skipped, tripples not
    -loop
    -1 rec-level +!
    0 [ cell 8 = ] [IF] lp+2 [ELSE] lp+ [THEN] ;

: rec-sequence: ( xtu .. xt1 u "name" -- ) \ gforth-experimental
    \G Define a recognizer sequence @i{name}.  @i{xtu}..@i{xt1} are
    \G xts of recognizers, and are the initial contents of the
    \G recognizer sequence, with @i{xt1} searched first.  The order of
    \G operands is inspired by @word{get-order} and @word{set-order}.@*
    \G @i{name} execution: ( c-addr u -- translation )@*
    \G Execute the first xt in the recognizer sequence @i{name}.  If
    \G the resulting translation has a translation token other than
    \G @word{translate-none}, this is the result of @i{name} and no
    \G further recognizers are tried.  Otherwise, the stacks are
    \G restored to the initial state (@i{c-addr u}), and the next xt
    \G is tried.  If all xts produce @word{translate-none},
    \G @i{translation} is @code{translate-none}.  @i{name} is a
    \G recognizer itself, which makes recognizer sequences nestable.
    ['] recognize do-stack: ;

\ : rec-sequence ( xt1 .. xtn n "name" -- ) \ gforth
\     n>r : nr> ]] 2>r [[ 0 ?DO
\ 	]] 2r@ [[ compile,
\ 	]] dup translate-none <> IF 2rdrop EXIT THEN drop [[
\     LOOP ]] 2rdrop ; [[ ;

' rec-number-kernel ' rec-name 2 rec-sequence: default-recognize
' default-recognize is rec-forth

: rec-none ( c-addr u -- translate-none ) \ gforth-experimental
    \G This recognizer recognizes nothing.  It can be useful as a
    \G placeholder.
    2drop translate-none ;
