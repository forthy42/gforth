\ defining words for words with non-default and non-immediate compilation semantics

\ Copyright (C) 1996 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ used like
\ : <name> create-interpret/compile ...
\     interpretation> ... <interpretation
\     compilation> ... <compilation ;


noname create
does> abort" interpreting word without interpretation semantics" ;
lastxt >does-code
does> abort" compiling word without compilation semantics" ;
lastxt >does-code
constant no-compilation-does-code
constant no-interpretation-does-code

: create-interpret/compile ( -- )
    0 0 interpret/compile:
    here lastxt interpret/compile-int !
    no-interpretation-does-code here does-code!
    0 >body allot
    here lastxt interpret/compile-comp !
    no-compilation-does-code here does-code!
    0 >body allot ; \ restrict?

: fix-does-code ( addr ret-addr -- )
    lastxt [ interpret/compile-struct drop ] literal + >r
    lastxt interpret/compile?
    lastxt interpret/compile-int @ r@ = and
    lastxt interpret/compile-comp @ r> >body = and
    0= abort" not created with create-interpret/compile"
    cell+ cell+ /does-handler + \ to does-code
    swap @ does-code! ;

: (interpretation>) ( -- )
    lastxt interpret/compile-int r@ fix-does-code ;

: interpretation> ( -- orig colon-sys )
    POSTPONE (interpretation>) POSTPONE ahead
    dodoes, defstart dead-code off 0 set-locals-size-list POSTPONE >body ; immediate restrict

: <interpretation ( orig colon-sys -- )
    ?struc POSTPONE exit
    POSTPONE then ; immediate restrict

: (compilation>) ( -- )
    lastxt interpret/compile-comp r@ fix-does-code ;

: compilation> ( -- orig colon-sys )
    POSTPONE (compilation>) POSTPONE ahead
    dodoes, defstart dead-code off 0 set-locals-size-list ; immediate restrict

comp' <interpretation drop Alias <compilation immediate restrict

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
