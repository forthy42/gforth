\ >STRING-EXECUTE >BUFFER-EXECUTE
\
\ Copyright (C) 2011,2016 Free Software Foundation, Inc.

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

32 constant >string-initial-buflen

2variable >string-buffer \ buffer
variable  >string-len    \ actual string length

: >string-type { c-addr u -- }
    >string-len @ { str-len }
    str-len u + { new-str-len }
    >string-buffer 2@
    begin { buf-addr buf-size }
	new-str-len buf-size > while
	    buf-size 2* buf-addr over resize throw swap
	    2dup >string-buffer 2!
    repeat 
    c-addr buf-addr str-len + u move
    new-str-len >string-len ! ;

: >string-emit { c^ c -- }
    c 1 >string-type ;

: >string-cr ( -- )  newline >string-type ;
1 -1 1 rshift 2Constant >string-form

' >string-type ' >string-emit ' >string-cr ' >string-form output: >string-out

: >string-execute ( ... xt -- ... addr u )
    \G execute xt while the standard output (TYPE, EMIT, and everything
    \G that uses them) is redirected to a string.  The resulting string
    \G is addr u, which is in ALLOCATEd memory; it is the
    \G responsibility of the caller of >STRING-EXECUTE to FREE this
    \G string.
    >string-buffer 2@ >string-len @ op-vector @
    { d: oldbuf oldlen oldvector }
    try
	>string-initial-buflen dup allocate throw swap >string-buffer 2!
	0 >string-len !
	>string-out
	execute
	>string-buffer 2@ drop >string-len @ tuck resize throw swap
	0 \ throw ball
    restore
	oldbuf >string-buffer 2!
	oldlen >string-len !
	oldvector op-vector !
    endtry
    throw ;

\ altenative interface (for systems without memory allocation wordset):

\ >buffer-execute ( ... c-addr u1 xt -- ... u2 ) execute xt while the
\ standard output (TYPE, EMIT, and everything that uses them) is
\ redirected to the buffer c-addr u.  u2 is the number of characters
\ that were output with TYPE or EMIT.  If u2<=u1, then the string
\ c-addr u2 contains the output, otherwise c-addr u1 contains the
\ first u1 characters of the output, and the other characters are not
\ stored.

\ You can emulate >STRING-EXECUTE with >BUFFER-EXECUTE like this:
\ Instead of

\ ... ['] foo >string-execute ( c-addr u ) ...

\ where FOO has the stack effect ( x1 x2 -- x3 ), write

\ ... 2dup 2>r (or whatever is necessary to save FOO's input operands)
\ pad 0 ['] foo >buffer-execute >r drop ( throw away result of FOO )
\ 2r> ( restore FOO's input operands )
\ r@ allocate throw r> 2dup 2>r ['] foo >buffer-execute drop 2r>


0 [if]
\ tests
5 ' . >string-execute dump
5 5 ' .r >string-execute dump

: test 0 swap 0 do i . i + loop ;

cr  20 ' test >string-execute .s cr 2dup type drop free throw .
cr 120 ' test >string-execute .s cr 2dup type drop free throw .
cr
[endif]
