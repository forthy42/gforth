\ quote: S" and ." words

\ Copyright (C) 1996,1998,1999,2002,2003,2007 Free Software Foundation, Inc.

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

\ this file comes last, because these words override cross' words.

require ./vars.fs

: CLiteral ( Compilation c-addr1 u ; run-time -- c-addr )
    2>r postpone ahead here 2r> s, >r postpone then
    r> postpone literal ; immediate restrict

: SLiteral ( Compilation c-addr1 u ; run-time -- c-addr2 u ) \ string
\G Compilation: compile the string specified by @i{c-addr1},
\G @i{u} into the current definition. Run-time: return
\G @i{c-addr2 u} describing the address and length of the
\G string.
    tuck 2>r postpone ahead here 2r> chars mem, align >r postpone then
    r> postpone literal postpone literal ; immediate restrict

\ \ abort"							22feb93py

: abort" ( compilation 'ccc"' -- ; run-time f -- ) \ core,exception-ext	abort-quote
\G If any bit of @i{f} is non-zero, perform the function of @code{-2 throw},
\G displaying the string @i{ccc} if there is no exception frame on the
\G exception stack.
    postpone if [char] " parse postpone cliteral postpone c(abort")
    postpone then ; immediate restrict

\ create s"-buffer /line chars allot
has? compiler 0= 
[IF] : s" [ELSE] :noname [THEN]
	[char] " parse
[ has? OS [IF] ]
    save-mem
[ [THEN] ]
;
\    	/line min >r s"-buffer r@ cmove
\    	s"-buffer r> ;
has? compiler [IF]
:noname [char] " parse postpone SLiteral ;
interpret/compile: S" ( compilation 'ccc"' -- ; run-time -- c-addr u )	\ core,file	s-quote
  \G Compilation: Parse a string @i{ccc} delimited by a @code{"}
  \G (double quote). At run-time, return the length, @i{u}, and the
  \G start address, @i{c-addr} of the string. Interpretation: parse
  \G the string as before, and return @i{c-addr}, @i{u}. Gforth
  \G @code{allocate}s the string. The resulting memory leak is usually
  \G not a problem; the exception is if you create strings containing
  \G @code{S"} and @code{evaluate} them; then the leak is not bounded
  \G by the size of the interpreted files and you may want to
  \G @code{free} the strings.  ANS Forth only guarantees one buffer of
  \G 80 characters, so in standard programs you should assume that the
  \G string lives only until the next @code{s"}.
[THEN]

:noname    [char] " parse type ;
:noname    [char] " parse postpone sLiteral postpone type ;
interpret/compile: ." ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote
  \G Compilation: Parse a string @i{ccc} delimited by a " (double
  \G quote). At run-time, display the string. Interpretation semantics
  \G for this word are undefined in ANS Forth. Gforth's interpretation
  \G semantics are to display the string. This is the simplest way to
  \G display a string from within a definition; see examples below.

