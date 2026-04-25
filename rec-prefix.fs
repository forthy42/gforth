\ table driven prefix recognizer dispatcher

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2026 Free Software Foundation, Inc.

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

Create prefix-table
$7F bl [DO] ' rec-none , [LOOP]

: rec-prefix ( addr u -- translate-something ) \ gforth-experimental
    dup IF
	over c@ dup bl $7F within IF
	    bl - cells prefix-table + perform  EXIT
	THEN  THEN
    rec-none ;

' rec-prefix action-of rec-forth >back

: add-prefix ( xt char -- ) \ gforth-experimental
    \G add a recognizer for the prefix @var{char}.
    dup $7F bl within abort" Only ASCII prefixes are valid"
    bl - cells prefix-table +
    dup @ ['] rec-none = IF  !  EXIT  THEN
    dup @ >does-code ['] recognize = IF  @ >back  EXIT  THEN
    dup >r @ 2 noname rec-sequence:  latestxt r> ! ;

:is rec-id. ( recognizer -- )
    dup id.
    ['] rec-prefix = IF
	." [ "
	$7F bl DO
	    prefix-table I bl - cells + @ dup ['] rec-none <> IF
		I emit ." :"
		dup >does-code ['] recognize = IF
		    ." ( " .recognizer-sequence ." ) "
		ELSE  id.  THEN
	    ELSE
		drop
	    THEN
	LOOP
	." ] "
    THEN ;
