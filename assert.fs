\ assertions

\ Copyright (C) 1995 Free Software Foundation, Inc.

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

require source.fs

variable assert-level ( -- a-addr ) \ gforth
\G all assertions above this level are turned off
1 assert-level !

: assertn ( n -- ) \ gforth assert-n
    \ this is internal (it is not immediate)
    assert-level @ >
    if
	POSTPONE (
    then ;

: assert0( ( -- ) \ gforth assert-zero
    \G important assertions that should always be turned on
    0 assertn ; immediate
: assert1( ( -- ) \ gforth assert-one
    \G normal assertions; turned on by default
    1 assertn ; immediate
: assert2( ( -- ) \ gforth assert-two
    \G debugging assertions
    2 assertn ; immediate
: assert3( ( -- ) \ gforth assert-three
    \G slow assertions that you may not want to turn on in normal debugging;
    \G you would turn them on mainly for thorough checking
    3 assertn ; immediate
: assert( ( -- ) \ gforth
    \G equivalent to assert1(
    POSTPONE assert1( ; immediate

: (endassert) ( flag -- ) \ gforth-internal
    \ inline argument sourcepos
    if
	r> sourcepos drop + >r EXIT
    else
	r> print-sourcepos ." : failed assertion"
	true abort" assertion failed" \ !! or use a new throw code?
    then ;

: ) ( -- ) \ gforth	close-paren
    \G end an assertion
    POSTPONE (endassert) sourcepos, ; immediate
