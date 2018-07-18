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

\ backward allocating locals

: <,  ( x -- )  -1 cells  allot here  ! ;
: <f, ( x -- )  -1 floats allot here f! ;
: <2, ( x -- )  -2 cells  allot here 2! ;
: <c, ( x -- )  -1        allot here c! ;

: compile-dictlocal-w ( a-addr -- ) ( run-time: w -- )
\ compiles a push of a local variable, and adjusts locals-size
\ stores the offset of the local variable to a-addr
    locals-size @ alignlp-w cell+ locals,
    val-part @ IF  postpone false  THEN  postpone <, ;

: compile-dictlocal-f ( a-addr -- ) ( run-time: f -- )
    locals-size @ alignlp-f float+ locals,
    val-part @ IF  postpone 0e  THEN  postpone <f, ;

: compile-dictlocal-d ( a-addr -- ) ( run-time: w1 w2 -- )
    locals-size @ alignlp-w cell+ cell+ locals,
    val-part @ IF  postpone #0.  THEN  postpone <2, ;

: compile-dictlocal-c ( a-addr -- ) ( run-time: w -- )
    locals-size @ 1+ locals,
    val-part @ IF  postpone false  THEN  postpone <c, ;

Create dictlocals
' compile-dictlocal-w ,
' compile-dictlocal-f ,
' compile-dictlocal-d ,
' compile-dictlocal-c ,

0 Value last-closure
0 Value last-closure-size

: <closure ( -- )
    maxalign [ cell maxaligned cell <> ] [IF] 0 , [THEN]
    [ ' spaces cell- @ ]L , \ does-vtable !!FIXME!! use derived vtable
    here to last-closure 2 cells allot ;

: start-dlocals ( -- )
    r> locals-size @ >r >r  locals-size off
    ]] maxalign here >r [[ ;
: end-dlocals ( -- )
    r> r> locals-size ! >r  ]] r> [[ ;

Defer start-d ' start-dlocals is start-d
Defer end-d   ' end-dlocals   is end-d

locals-types definitions

: :}d ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    0 lit, here cell- to last-closure-size
    start-d ]] allot here >r [[
    dictlocals to compile-pushlocals } ]] r> dp ! [[
    pushlocals to compile-pushlocals
    locals-size @ last-closure-size !
    end-d ;

forth definitions

: (closure-;]) ( some-sys lastxt -- )
    ]
    postpone THEN
    locals-size ! wrap! locals-list !
    ['] start-dlocals is start-d
    ['] end-dlocals is end-d ;

: closure> ( body -- addr )
    last-closure tuck does-code! ;
: start-dclosure ( -- )
    r> locals-size @ >r >r  locals-size off
    ]] <closure [[ ;
: end-dclosure ( -- closure-sys )
    wrap@ r> r> swap >r
    here 6 cells + lit, ]] closure> AHEAD lp+!# [[ locals-size @ negate ,
    locals-size @ ]] laddr# [[ 0 , ]] literal move [[
    ['] (closure-;]) defstart  last @ lastcfa @ defstart ;

: [{: ( -- vtaddr u latest latestxt wid 0 )
    [: ] drop ;] defstart  locals-list @ locals-list off
    ['] start-dclosure is start-d
    ['] end-dclosure is end-d
    postpone {:
; immediate

0 [IF]
    : test [{: a f: b d: c :}d a b c ;] ;
    5 3.3e #1234. test execute d. f. .
[THEN]
