\ defining words for words with non-default and non-immediate compilation semantics

\ Copyright (C) 1996,1997,2000,2003,2007 Free Software Foundation, Inc.

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

\ used like
\ : <name> create-interpret/compile ...
\     interpretation> ... <interpretation
\     compilation> ... <compilation ;


noname create
does> abort" interpreting word without interpretation semantics" ;
latestxt >does-code
does> abort" compiling word without compilation semantics" ;
latestxt >does-code
constant no-compilation-does-code
constant no-interpretation-does-code

: create-interpret/compile ( "name" -- ) \ gforth
    0 0 interpret/compile:
    here latestxt interpret/compile-comp !
    no-compilation-does-code here does-code!
    [ 0 >body ] literal allot
    here latestxt interpret/compile-int !
    no-interpretation-does-code here does-code!
    [ 0 >body ] literal allot ; \ restrict?

: fix-does-code ( addr ret-addr -- )
    latestxt [ interpret/compile-struct %size ] literal + >r
    latestxt interpret/compile?
    latestxt interpret/compile-int @ r@ >body = and
    latestxt interpret/compile-comp @ r> = and
    0= abort" not created with create-interpret/compile"
    cell+ cell+ maxaligned /does-handler + \ to does-code
    swap @ does-code! ;

: (interpretation>1) ( addr R:retaddr -- )
    latestxt interpret/compile-int swap fix-does-code ;

: interpretation> ( compilation. -- orig colon-sys ) \ gforth
    here 4 cells +  POSTPONE literal POSTPONE (interpretation>1) POSTPONE ahead
    dodoes, defstart dead-code off 0 set-locals-size-list ; immediate restrict

: <interpretation ( compilation. orig colon-sys -- ) \ gforth
    ?struc POSTPONE exit
    POSTPONE then ; immediate restrict

: (compilation>1) ( addr R:retaddr -- )
    latestxt interpret/compile-comp swap fix-does-code ;

: compilation> ( compilation. -- orig colon-sys ) \ gforth
    here 4 cells + POSTPONE literal POSTPONE (compilation>1) POSTPONE ahead
    dodoes, defstart dead-code off 0 set-locals-size-list POSTPONE >body ; immediate restrict

comp' <interpretation drop
Alias <compilation ( compilation. orig colon-sys -- ) \ gforth
immediate restrict

\ example
\ : constant ( n "name" -- )
\     create-interpret/compile
\     ,
\ interpretation>
\     @
\ <interpretation
\ compilation>
\     @ postpone literal
\ <compilation ;

\ 5 constant five

\ cr
\ five . cr
\ : fuenf five ;
\ see fuenf cr
