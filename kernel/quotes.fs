\ quote: S" and ." words

\ Copyright (C) 1996,1998,1999 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ this file comes last, because these words override cross' words.

require ./vars.fs

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
:noname    postpone (.") ,"  align ;
interpret/compile: ." ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote
  \G Compilation: Parse a string @i{ccc} delimited by a " (double
  \G quote). At run-time, display the string. Interpretation semantics
  \G for this word are undefined in ANS Forth. Gforth's interpretation
  \G semantics are to display the string. This is the simplest way to
  \G display a string from within a definition; see examples below.

