\ Tokenize Forth source code

\ Copyright (C) 2019 Free Software Foundation, Inc.

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

Variable tokens[]

: ?token { nt -- index t / f }
    tokens[] $@ bounds ?DO
	nt I @ = IF  I tokens[] $@ drop - cell/ true  UNLOOP  EXIT  THEN
    cell +LOOP  addr nt cell tokens[] $+!  false ;

\ token format:
\ 1 count string  -> nt rectype-name ( need to convert string to nt first)
\ 2 xchar         -> nt rectype-name
\ 3 cell          -> n rectype-num
\ 4 2*cell        -> d rectype-dnum
\ 5 float         -> f rectype-float
\ 6 string        -> addr u rectype-string
\ 7 count string  -> nt rectype-to ( need to convert string to nt first)
\ 8 xchar         -> nt rectype-to
\ 9 xcount string -> raw parse or parse-name input

Variable parsed-name$

: tokenize-it ( rectype rec-xt -- rectype )
    drop case dup
	rectype-name of
	    over ?token IF  2 emit xemit
	    ELSE  1 emit input-lexeme 2@ dup emit type  THEN
	endof
	rectype-num of
	    3 emit >r dup { w^ x } x cell type r>
	endof
	rectype-dnum of
	    4 emit >r 2dup { d^ x } x 2 cells type r>
	endof
	rectype-float of
	    5 emit >r fdup { f^ x } x 1 floats type r>
	endof
	rectype-string of
	    6 emit >r 2dup dup xemit type r>
	endof
	rectype-to of
	    ?token IF  8 emit xemit
	    ELSE  7 emit input-lexeme 2@ dup emit type  THEN
	endof
    endcase  parsed-name$ $free ;

stdout Value token-file

: t, ( ... xt -- )
    token-file outfile-execute ;

: tokenize ( rectype rec-xt -- rectype )
    ['] tokenize-it t, ;

: parse-name' ( -- addr u )
    parsed-name$ $@len IF
	parsed-name$ $@ [: 9 emit dup xemit type ;] t,
    THEN
    defers parse-name 2dup parsed-name$ $! ;

: parse' ( char -- addr u )
    parse [: 9 emit dup xemit 2dup type ;] t, ;

: >tokenize ( addr u -- )
    r/w create-file throw to token-file
    ['] parse-name' is parse-name
    ['] parse'      is parse
    ['] tokenize    is trace-recognizer ;
