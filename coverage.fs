\ Code coverage tool

\ Copyright (C) 2018 Free Software Foundation, Inc.

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

require sections.fs

section-size extra-section coverage

' Create coverage cover-start

: cover-end ( -- addr ) ['] here coverage ;
: cover, ( n -- ) ['] , coverage ;

0 Value coverage?
0 Value last-cover?

: (cov+) ( flag -- )
    current-sourceview cover,
    IF    postpone inc# cover-end , 0
    ELSE  1  THEN  cover,
    here to last-cover? ;

: cov+ ( -- )
    coverage?
    here last-cover? <>  and
    source nip >in @ u>  and  IF
	state @ (cov+)
    THEN ; immediate compile-only

$10 buffer: cover-hash

: hash-cover ( -- addr u )
    cover-hash $10 erase
    cover-end cover-start U+DO
	I cell false cover-hash hashkey2
    2 cells +LOOP
    cover-hash $10 ;

:noname defers :-hook coverage? IF  true (cov+)  THEN ; is :-hook
:noname defers basic-block-end    postpone cov+ ; is basic-block-end
:noname defers before-line        postpone cov+ ; is before-line

: .coverage ( -- )
    \G print all coverage data
    cover-end cover-start U+DO
	I @ .sourceview ." : " I cell+ ? cr
    2 cells +LOOP ;

\ coverage tests

true [IF]
true to coverage?
: test1 ( n -- )  0 ?DO  I .  LOOP ;
: test2 ( flag -- ) IF ." yes"  ELSE ." no"  THEN ;
[THEN]
