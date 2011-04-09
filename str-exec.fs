\ >STRING-EXECUTE >BUFFER-EXECUTE
\
\ Copyright (C) 2011 Free Software Foundation, Inc.

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

: >string-execute ( ... xt -- ... addr u )
    \ execute xt while the standard output (TYPE, EMIT, and everything
    \ that uses them) is redirected to a string.  The resulting string
    \ is addr u, which is in ALLOCATEd memory; it is the
    \ responsibility of the caller of >STRING-EXECUTE to FREE this
    \ string.
    >string-buffer 2@ >string-len @
    action-of type action-of emit    { d: oldbuf oldlen oldtype oldemit }
    try
	>string-initial-buflen dup allocate throw swap >string-buffer 2!
	0 >string-len !
	['] >string-type is type
	['] >string-emit is emit
	execute
	>string-buffer 2@ drop >string-len @ tuck resize throw swap
	0 \ throw ball
    restore
	oldbuf >string-buffer 2!
	oldlen >string-len !
	oldtype is type
	oldemit is emit
    endtry
    throw ;

0 [if]
\ tests
5 ' . >string-execute dump
5 5 ' .r >string-execute dump

: test 0 do i . loop ;

cr  20 ' test >string-execute .s cr 2dup type drop free throw
cr 120 ' test >string-execute .s cr 2dup type drop free throw
cr
[endif]
