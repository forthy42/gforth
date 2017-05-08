\ meta-recognizer for disambiguation

\ Copyright (C) 2017 Free Software Foundation, Inc.

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

: rec:meta ( addr u -- xt r:to | r:fail )
    \G words prefixed with @var{recognizer}@code{?} are processed by
    \G @code{rec:}@var{recognizer} to disambiguate recognizers.
    '?' $split dup 0= IF  2drop
    ELSE
	2swap [: ." rec:" type ;] $tmp find-name ?dup-IF
	    name?int execute  EXIT
	THEN
    THEN
    2drop r:fail ;

get-recognizers ' rec:meta swap 1+ set-recognizers
