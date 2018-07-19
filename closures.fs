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

require glocals.fs

Create do-closure
DOES> ;
' noop set->int
' (noname->comp) set->comp

Defer end-d   ' execute is end-d

$10 stack: locals-sizes
$10 stack: locals-lists

: >addr ( xt -- addr )
    [ cell maxaligned ]L - ;

locals-types definitions

: :}d ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    0 lit, here cell- >r
    ]] allot laddr# [[ 0 , ]] >r here lp! [[
    :}
    locals-size @ [ 3 cells maxaligned ]L + r> !
    [: ]] r> lp! [[ ;] end-d ;

: alloch ( size -- addr ) \ addr is the end of the allocated region
    dup allocate throw + ;

: :}m ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    0 lit, here cell- >r
    ]] alloch laddr# [[ 0 , ]] >r lp! [[
    :}
    locals-size @ [ 3 cells maxaligned ]L + r> !
    [: ]] r> lp! [[ ;] end-d ;

: :}l ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    ]] lp+!# [[ here >r 0 ,
    :}
    locals-size @ [ 3 cells maxaligned ]L +
    dup locals-sizes stack> + locals-sizes >stack
    negate r> !
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
; immediate

false [IF]
    : test [{: a f: b d: c :}d a b c ;] ;
    5 3.3e #1234. test execute d. f. . cr

    : A {: k x1 x2 x3 x4 x5 :} recursive
	k 0<= IF  x4 x5 execute execute f+ ELSE
	    0 addr k x1 x2 x3 x4
	    [{: B w! k x1 x2 x3 x4 :}L -1 +to k
		k B x1 x2 x3 x4 A ;]
	    dup dup >body ! \ modify first local in quotation
	    execute THEN ;
    
    : man-or-boy? ( n -- ) [: 1e ;] [: -1e ;] 2dup swap [: 0e ;] A f. ;
    
    \ start with: gforth -l128M -r16M -d1M closures.fs
    14 set-precision
    20 0 [DO] [i] man-or-boy? [LOOP] cr
[THEN]
