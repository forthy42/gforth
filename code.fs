\ ASSEMBLER, CODE etc.

\ Copyright (C) 1995,1996,1997,1999,2003,2007,2010,2013,2014,2015 Free Software Foundation, Inc.

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

\ does not include the actual assembler (which is machine-dependent),
\ only words like CODE that are implementation-dependent, but can be
\ defined for all machines.

vocabulary assembler ( -- ) \ tools-ext
\g A vocubulary: Replaces the wordlist at the top of the search order
\g with the assembler wordlist.

: init-asm ( -- ) \ gforth
\g Pushes the assembler wordlist on the search order.
    also assembler ;
    
: code ( "name" -- colon-sys )	\ tools-ext
    \G Start a native code definition that runs in the context of the
    \G Gforth virtual machine (engine).  Such a definition is not
    \G portable between Gforth installations, so we recommend using
    \G @code{abi-code} instead of @code{code}.  You have to end a
    \G @code{code} definition with a dispatch to the next virtual
    \G machine instruction.
    header ['] noop vtcopy,
    here latest !
    defstart init-asm ;

[ifdef] doabicode:
: abi-code ( "name" -- colon-sys )	\ gforth	abi-code
   \G Start a native code definition that is called using the platform's
   \G ABI conventions corresponding to the C-prototype:
   \G @example
   \G Cell *function(Cell *sp, Float **fpp);
   \G @end example
   \G The FP stack pointer is passed in by providing a reference to a
   \G memory location containing the FP stack pointer and is passed
   \G out by storing the changed FP stack pointer there (if necessary).
    header  ['] (abi-code-dummy) vtcopy,
    doabicode: latest !
    defstart init-asm ;
[endif]

: (;code) ( -- ) \ gforth
    \ execution semantics of @code{;code}
    r> latestxt code-address! ;

[ifundef] ?colon-sys
: ?colon-sys  ( ... xt tag -- )
    ?struc execute ;
[then]

:noname ( -- colon-sys )
    align here latestxt code-address!
    defstart init-asm ;
:noname ( colon-sys1 -- colon-sys2 )	\ tools-ext	semicolon-code
    ( create the [;code] part of a low level defining word )
    [ifdef] 0-adjust-locals-size 0-adjust-locals-size [then]
    ;-hook postpone (;code) basic-block-end finish-code ?colon-sys postpone [
    defstart init-asm ;
interpret/compile: ;code ( compilation. colon-sys1 -- colon-sys2 )	\ tools-ext	semicolon-code
\g The code after @code{;code} becomes the behaviour of the last
\g defined word (which must be a @code{create}d word).  The same
\g caveats apply as for @code{code}, so we recommend using
\g @code{;abi-code} instead.

[ifdef] do;abicode: 
: !;abi-code ( addr -- )
    latestxt do;abicode: any-code! ;

: ;abi-code ( -- ) \ gforth semicolon-abi-code
    ['] !;abi-code does>-like postpone [ init-asm ; immediate
[then]
    
: end-code ( colon-sys -- )	\ gforth	end_code
    \G End a code definition.  Note that you have to assemble the
    \G return from the ABI call (for @code{abi-code}) or the dispatch
    \G to the next VM instruction (for @code{code} and @code{;code})
    \G yourself.
    latestxt here over - flush-icache
    previous ?struc reveal ;
