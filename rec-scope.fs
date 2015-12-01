\ scope recognizer

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

: rec:scope ( addr u -- xt | r:fail )
    ':' $split 2swap find-name dup IF
	dup >does-code [ ' forth >does-code ]L = IF
	    >body find-name-in dup 0= IF  drop  r:fail  THEN
	    EXIT
	THEN
    THEN  drop 2drop r:fail ;

get-recognizers 1+ ' rec:scope -rot set-recognizers
