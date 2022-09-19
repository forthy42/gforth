\ recognizer-based interpreter, sequence

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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
    1 rec-level +!
    $@ bounds cell- swap cell- U-DO
	2dup I -rot 2>r  perform
	dup ['] notfound <>  IF
	    -1 rec-level +!
	    2rdrop I @ trace-recognizer  UNLOOP  EXIT  THEN  drop
	2r>
	cell [ 2 cells ] Literal I cell- 2@ <> select \ skip double entries
	\ note that we search first and then skip, because the first search
	\ has a very likely hit.  So doubles will be skipped, tripples not
    -loop
    -1 rec-level +!
    2drop ['] notfound ;

: recognizer-sequence: ( x1 .. xn n "name" -- ) \ gforth-experimental
    ['] recognize do-stack: ;

' rec-num ' rec-nt 2 recognizer-sequence: default-recognize
' default-recognize is forth-recognize
