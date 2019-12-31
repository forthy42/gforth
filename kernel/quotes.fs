\ quote: S" and ." words

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke
\ Copyright (C) 1996,1998,1999,2002,2003,2007,2013,2014,2016,2018,2019 Free Software Foundation, Inc.

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

\ String literals

Defer next-section     \ put some data within a definition
Defer previous-section \ end that part

:noname  postpone ahead ; is next-section
:noname  postpone then ; is previous-section

: CLiteral ( Compilation c-addr1 u ; run-time -- c-addr )
    2>r next-section here 2r> s, >r  previous-section
    r> postpone literal ; immediate restrict

: SLiteral ( Compilation c-addr1 u ; run-time -- c-addr2 u ) \ string
\G Compilation: compile the string specified by @i{c-addr1},
\G @i{u} into the current definition. Run-time: return
\G @i{c-addr2 u} describing the address and length of the
    \G string.
    tuck 2>r next-section here 2r> chars mem, align >r previous-section
    r> postpone literal postpone literal ; immediate restrict

\ \ abort"							22feb93py

: abort" ( compilation 'ccc"' -- ; run-time f -- ) \ core,exception-ext	abort-quote
\G If any bit of @i{f} is non-zero, perform the function of @code{-2 throw},
\G displaying the string @i{ccc} if there is no exception frame on the
\G exception stack.
    postpone if '"' parse postpone cliteral postpone c(abort")
    dead-code on postpone then ; immediate restrict

: warning" ( compilation 'ccc"' -- ; run-time f -- ) \ gforth
    \G if @i{f} is non-zero, display the string @i{ccc} as warning message.
    postpone if '"' parse postpone cliteral postpone c(warning")
    postpone then ; immediate restrict

\ create s"-buffer /line chars allot
:noname
    '"' parse
[ has? OS [IF] ]
    save-mem
[ [THEN] ]
;
:noname '"' parse postpone SLiteral ;
interpret/compile: s" ( compilation 'ccc"' -- ; run-time -- c-addr u )	\ core,file	s-quote
  \G Compilation: Parse a string @i{ccc} delimited by a @code{"}
  \G (double quote). At run-time, return the length, @i{u}, and the
  \G start address, @i{c-addr} of the string. Interpretation: parse
  \G the string as before, and return @i{c-addr}, @i{u}. Gforth
  \G @code{allocate}s the string. The resulting memory leak is usually
  \G not a problem; the exception is if you create strings containing
  \G @code{S"} and @code{evaluate} them; then the leak is not bounded
  \G by the size of the interpreted files and you may want to
  \G @code{free} the strings.  Forth-2012 only guarantees two buffers of
  \G 80 characters each, so in standard programs you should assume that the
  \G string lives only until the next-but-one @code{s"}.

:noname '"' parse type ;
:noname '"' parse postpone SLiteral postpone type ;
interpret/compile: ."  ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote
  \G Compilation: Parse a string @i{ccc} delimited by a " (double
  \G quote). At run-time, display the string. Interpretation semantics
  \G for this word are undefined in ANS Forth. Gforth's interpretation
  \G semantics are to display the string. This is the simplest way to
  \G display a string from within a definition; see examples below.
\    '"' parse type ;
\ has? compiler [IF]
\     comp: drop '"' parse postpone sLiteral postpone type ;
\ [THEN]
