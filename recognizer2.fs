\ FIND-based recognizers

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

\ !! This is just a rough sketch for the moment

\ !! no memory management yet
\ !! add good way to deal with several recognizers
\ maybe as wordlists

\ do the wrapper once for all the recognizers

require sections.fs

: rec2-wrapper {: c-addr u xt -- nt|0 :}
    \ xt ( c-addr u -- nt|0 )
    wrap@ next-section vtsave
    c-addr u xt catch vt,
    2>r vtrestore previous-section wrap! 2r> throw ;

: single-rec2 ( c-addr u -- nt|0 )
    \ !! compilation does not work for some reason
    0. 2swap >number 0= if \ it is a number
	2drop noname constant lastxt exit then
    2drop drop r:fail ;

\ or (to get nicer decompilation for ['] and POSTPONE):

\ : single-rec2 {: c-addr u -- nt|0 :}
\     0. c-addr u >number 0= if \ it is a number
\ 	2drop c-addr u nextname constant latest exit then
\     -13 throw \ don't fallback to the other recognizers in this demo
\     2drop drop 0 ;

: single-recognizer2 ( c-addr u -- nt|0 )
    ['] single-rec2 rec2-wrapper ;

get-recognizers ' single-recognizer2 -rot 1+ set-recognizers
