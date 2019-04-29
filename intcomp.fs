\ defining words for words with non-default and non-immediate compilation semantics

\ Copyright (C) 1996,1997,2000,2003,2007,2010,2017 Free Software Foundation, Inc.

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

"compiling word without compilation semantics" exception constant no-compsem

: default-c-i/c-int -14 throw ;
: default-c-i/c-comp no-compsem throw ;

variable last-c-i/c-comp

: c-i/c-comp-xt ( -- )
    "test" nextname create 0 `default-c-i/c-comp 2,
    [: 2@ execute ;] set-does>
    lastxt last-c-i/c-comp ! ;

c-i/c-comp-xt \ make one so that the vt already exists for the real use

: create-interpret/compile ( "name" -- )
    c-i/c-comp-xt
    create [: name>view 2 cells - body> `execute ;] set->comp ;

: interpretation> ( compilation. -- orig colon-sys ) \ gforth-obsolete
    postpone [: ; immediate restrict

: <interpretation ( compilation. orig colon-sys -- ) \ gforth-obsolete
    ]] ;] set-does> [[ ; immediate restrict

: compilation> ( compilation. -- orig colon-sys ) \ gforth-obsolete
    ]] [: [[ ; immediate restrict

: set-c-i/c-comp ( xt -- )
    lastxt >body swap last-c-i/c-comp @ >body 2! ;
    
: <compilation ( orig colon-sys -- ) \ gforth-obsolete
    ]] ;]  set-c-i/c-comp [[ ; immediate restrict

\\\ example
: constant ( n "name" -- )
    create-interpret/compile
    ,
interpretation>    @                    <interpretation
compilation>       @ postpone literal   <compilation ;

5 constant five

cr
five . cr
: fuenf five ;
see fuenf cr
