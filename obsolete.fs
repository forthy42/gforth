\ some obsolete code that is not needed anywhere else

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

\ these transformations are used for legacy words like find

: (name>comp) ( nt -- xt +-1 ) \ gforth
    \G @i{w xt} is the compilation token for the word @i{nt}.
    name>comp ['] execute = flag-sign ;

: sfind ( c-addr u -- 0 / xt +-1  ) \ gforth-obsolete
    find-name dup
    if ( nt )
	state @
	if
	    (name>comp)
	else
	    (name>intn)
	then
   then ;

: find ( c-addr -- xt +-1 | c-addr 0 ) \ core,search
    \G Search all word lists in the current search order for the
    \G definition named by the counted string at @i{c-addr}.  If the
    \G definition is not found, return 0. If the definition is found
    \G return 1 (if the definition has non-default compilation
    \G semantics) or -1 (if the definition has default compilation
    \G semantics).  The @i{xt} returned in interpret state represents
    \G the interpretation semantics.  The @i{xt} returned in compile
    \G state represented either the compilation semantics (for
    \G non-default compilation semantics) or the run-time semantics
    \G that the compilation semantics would @code{compile,} (for
    \G default compilation semantics).  The ANS Forth standard does
    \G not specify clearly what the returned @i{xt} represents (and
    \G also talks about immediacy instead of non-default compilation
    \G semantics), so this word is questionable in portable programs.
    \G If non-portability is ok, @code{find-name} and friends are
    \G better (@pxref{Name token}).
    dup count sfind dup
    if
	rot drop
    then ;
