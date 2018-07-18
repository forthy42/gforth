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

0 Value last-closure-size

Create do-closure
DOES> ;
' noop set->int
' (noname->comp) set->comp

: end-dlocals ( -- )
    r> r> locals-size ! >r  ]] r> [[ ;

Defer end-d   ' end-dlocals   is end-d

locals-types definitions

: :}d ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    0 lit, here cell- to last-closure-size
    ]] allot laddr# [[ 0 , ]] >r here lp! [[
    :}
    locals-size @ 3 cells maxaligned + last-closure-size !
    end-d ;

forth definitions

: (closure-;]) ( some-sys lastxt -- )
    ]
    postpone THEN
    wrap! locals-list ! locals-size !
    ['] end-dlocals is end-d ;

: closure> ( body -- addr )
    >l dodoes: >l lp@
    [ ' do-closure cell- @ ]L >l
    [ cell maxaligned cell <> ] [IF] 0 >l [THEN] ;
: end-dclosure ( -- closure-sys )
    wrap@
    postpone lit >mark
    ]] closure> r> lp! AHEAD BUT THEN lp+!# [[ locals-size @ negate ,
    locals-size @ ]] laddr# [[ 0 , ]] literal move [[
    ['] (closure-;]) defstart  last @ lastcfa @ defstart ;

: [{: ( -- vtaddr u latest latestxt wid 0 )
    [: ] drop ;] defstart
    locals-size @ locals-size off
    locals-list @ locals-list off
    ['] end-dclosure is end-d
    postpone {:
; immediate

0 [IF]
    : test [{: a f: b d: c :}d a b c ;] ;
    5 3.3e #1234. test execute d. f. .
[THEN]
