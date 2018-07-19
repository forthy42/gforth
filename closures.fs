\ A powerful closure implementation

\ Copyright (C) 2018 Free Software Foundation, Inc.

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

\ more information in http://www.complang.tuwien.ac.at/anton/euroforth/ef18/drafts/ertl.pdf

Create do-closure
DOES> ;
' noop set->int
' (noname->comp) set->comp

Defer end-d   ' execute is end-d

$10 stack: locals-sizes
$10 stack: locals-lists

: >addr ( xt -- addr )
    [ cell maxaligned ]L - ;
: alloch ( size -- addr ) \ addr is the end of the allocated region
    dup allocate throw + ;
: allocd ( size -- addr ) \ addr is the end of the allocated region
    allot here ;

locals-types definitions

: :}* ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... xt -- ) \ gforth close-brace-dictionary
    0 lit, here cell- >r
    compile, ]] laddr# [[ 0 , ]] >r lp! [[
    :}
    locals-size @ [ 3 cells maxaligned ]L + r> !
    [: ]] r> lp! [[ ;] end-d ;

: :}d ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    ['] allocd :}* ;

: :}m ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-heap
    ['] alloch :}* ;

: :}l ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-locals
    :}
    locals-size @ [ 3 cells maxaligned ]L +
    locals-sizes stack> + locals-sizes >stack
    ['] noop end-d ;

forth definitions

: (closure-;]) ( some-sys lastxt -- )
    ]
    postpone THEN
    wrap!
    locals-lists stack> locals-list !
    locals-sizes stack> locals-size !
    ['] execute is end-d ;

: closure> ( body -- addr )
    >l dodoes: >l lp@
    [ ' do-closure cell- @ ]L >l
    [ cell maxaligned cell <> ] [IF] 0 >l [THEN] ;
: end-dclosure ( unravel-xt -- closure-sys )
    >r wrap@
    postpone lit >mark
    ]] closure> [[ r> execute ]] AHEAD BUT THEN lp+!# [[ locals-size @ negate ,
    locals-size @ ]] laddr# [[ 0 , ]] literal move [[
    ['] (closure-;]) defstart  last @ lastcfa @ defstart ;

: [{: ( -- vtaddr u latest latestxt wid 0 )
    [: ] drop ;] defstart
    locals-size @ locals-sizes >stack  locals-size off
    locals-list @ locals-lists >stack  locals-list off
    ['] end-dclosure is end-d
    postpone {:
; immediate compile-only

false [IF]
    : test [{: a f: b d: c :}d a b c ;] ;
    5 3.3e #1234. test execute d. f. . cr

    : A {: k x1 x2 x3 x4 x5 | B :} recursive
	k 0<= IF  x4 execute x5 execute f+ ELSE
	    addr B addr k x1 x2 x3 x4
	    [{: w! B w! k x1 x2 x3 x4 :}L -1 +to k
		k B x1 x2 x3 x4 A ;] dup to B
	    execute THEN ;
    : man-or-boy? ( n -- ) [: 1e ;] [: -1e ;] 2dup swap [: 0e ;] A f. ;
    
    \ start with: gforth -l64M -r8M closures.fs
    14 set-precision
    20 0 [DO] [i] man-or-boy? [LOOP] cr
[THEN]
