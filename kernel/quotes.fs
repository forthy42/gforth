\ quote: S" and ." words

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke
\ Copyright (C) 1996,1998,1999,2002,2003,2007,2013,2014,2016,2018,2019,2021,2023,2024 Free Software Foundation, Inc.

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

Defer next-section ( -- ) \ gforth
\g Switch to the next section in the section stack.  If there is no
\g such section yet, create it (with the size being a quarter of the
\g size of the current section).

Defer previous-section ( -- ) \ gforth
\g Switch to the previous section in the section stack; the now-next
\g section continues to exist with everything that was put there.
\g Throw an exception if there is no previous section.

:noname  latestnt  postpone ahead ; is next-section
:noname  postpone then  lastnt !  ; is previous-section

: CLiteral ( Compilation c-addr1 u ; run-time -- c-addr )
    2>r next-section here 2r> string, align >r  previous-section
    r> postpone literal ; immediate restrict

: SLiteral ( Compilation c-addr1 u -- ; run-time -- c-addr2 u ) \ string
    \G Compilation semantics: ( @i{c-addr1 u --} )
    \G Copy the string described by @i{c-addr1 u} to @i{c-addr2 u} and
    \g compile the run-time semantics.@*
    \G Run-time Semantics: ( @i{ -- c-addr2 u} ).@*
    \G Interpretation semantics: not defined in the standard.
    tuck 2>r next-section here 2r> chars mem, align >r previous-section
    r> postpone literal postpone literal ; immediate restrict

\ \ abort"							22feb93py

: abort" ( compilation 'ccc"' -- ; run-time ... f -- ) \ core,exception-ext	abort-quote
\G If any bit of @i{f} is non-zero, perform the function of @code{-2 throw},
\G displaying the string @i{ccc} if there is no exception frame on the
\G exception stack.
    postpone if '"' parse postpone cliteral postpone c(abort")
    dead-code on postpone then ; immediate restrict

: warning" ( compilation 'ccc"' -- ; run-time f -- ) \ gforth warning-quote
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
interpret/compile: s" ( Interpretation 'ccc"' -- c-addr u )	\ core,file	s-quote
\G Interpretation: Parse the string @i{ccc} delimited by a @code{"}
\G (double quote).  Store the resulting string in newly allocated heap
\G memory, and push its descriptor @i{c-addr u}.
\G @*
\G Compilation @code{( '@i{ccc}"' -- )}: Parse the string @i{ccc}
\G delimited by a @code{"} (double quote).  Append the run-time
\G semantics below to the current definition.
\G @*
\G Run-time @code{( -- c-addr u )}: Push a descriptor for the
\G parsed string.

:noname '"' parse type ;
:noname '"' parse postpone SLiteral postpone type ;
interpret/compile: ."  ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote
  \G Compilation: Parse a string @i{ccc} delimited by a " (double
  \G quote). At run-time, display the string. Interpretation semantics
  \G for this word are undefined in standard Forth. Gforth's interpretation
  \G semantics are to display the string.
