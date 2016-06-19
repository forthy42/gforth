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

: scope-split ( addr u -- addr' u' wid/0 )
    ':' $split dup 0= IF  nip  EXIT  THEN
    2swap find-name dup IF
	dup >does-code [ ' forth >does-code ]L = IF
	    >body  EXIT  THEN  drop 0  THEN ;

: rec:scope ( addr u -- xt | r:fail )
    scope-split dup IF find-name-in dup 0= IF  drop  r:fail  THEN
    ELSE  drop 2drop r:fail  THEN ;

get-recognizers 1+ ' rec:scope -rot set-recognizers

: :wlscope ( addr u -- addr' u' wid )
    2dup scope-split dup IF  >r 2nip r>  ELSE  drop 2drop defers wlscope  THEN ;

\ activate with ' :wlscope is wlscope