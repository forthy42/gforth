\ assertions

\ Copyright (C) 1995,1996,1997,1999,2002,2003,2007 Free Software Foundation, Inc.

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

require source.fs

variable assert-level ( -- a-addr ) \ gforth
\G All assertions above this level are turned off.
1 assert-level !

: assertn ( n -- ) \ gforth assert-n
    \ this is internal (it is not immediate)
    assert-level @ >
    if
	POSTPONE (
    then ;

: assert0( ( -- ) \ gforth assert-zero
    \G Important assertions that should always be turned on.
    0 assertn ; immediate
: assert1( ( -- ) \ gforth assert-one
    \G Normal assertions; turned on by default.
    1 assertn ; immediate
: assert2( ( -- ) \ gforth assert-two
    \G Debugging assertions.
    2 assertn ; immediate
: assert3( ( -- ) \ gforth assert-three
    \G Slow assertions that you may not want to turn on in normal debugging;
    \G you would turn them on mainly for thorough checking.
    3 assertn ; immediate
: assert( ( -- ) \ gforth
    \G Equivalent to @code{assert1(}
    POSTPONE assert1( ; immediate

: (end-assert) ( flag nfile nline -- ) \ gforth-internal
    rot if
	2drop
    else
	.sourcepos ." : failed assertion"
	true abort" assertion failed" \ !! or use a new throw code?
    then ;

: ) ( -- ) \ gforth	close-paren
\G End an assertion.
    compile-sourcepos POSTPONE (end-assert) ; immediate
