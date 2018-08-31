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

$10 stack: locals-sizes
$10 stack: locals-lists

Create do-closure \G vtable prototype for closures
DOES> ;           \G the does-code part is patched for each closure
' noop set->int            \ closures don't have a full header, so the default
' (noname->comp) set->comp \ actions (that check flags) don't work


Defer end-d ( ... xt -- ... )
\ is either EXECUTE (for {: ... :}*) or END-DCLOSURE (for [{: ... :}*).
\ xt is either ' NOOP or [: ]] r> lp! [[ ;], which restores LP.
' execute is end-d
Defer endref, ( -- )
\ pushes a reference to the location
' noop is endref,

: >addr ( xt -- addr ) \ gforth-experimental to-addr
    \G convert the xt of a closure on the heap to the @var{addr} with can be
    \G passed to @code{free} to get rid of the closure
    [ cell maxaligned ]L - ;
: alloch ( size -- addr ) \ addr is the end of the allocated region
    dup allocate throw + ;
: allocd ( size -- addr ) \ addr is the end of the allocated region
    dp +! dp @ ;

: >lp r> lp@ >r >r lp! ;
opt: drop ]] laddr# [[ 0 , ]] >r lp! [[ ;
: lp> r> r> lp! >r ;
opt: drop ]] r> lp! [[ ;

Variable extra-locals ( additional hidden locals size )

locals-types definitions

: :}* ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... xt -- ) \ gforth close-brace-dictionary
    0 lit, here cell- >r
    compile, ]] >lp [[
    :}
    locals-size @ extra-locals @ + r> !
    ['] endref, end-d
    ['] execute is end-d  ['] noop is endref,
    extra-locals off ;

: :}xt ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-xt
    \G end a closure's locals declaration.  The closure will be allocated by
    \G the xt on the stack, so the closure's run-time stack effect is @code{(
    \G xt-alloc -- xt-closure}.
    \ run-time: ( xt size -- ... )
    [: swap execute ;] :}* ;

: :}d ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    \G end a closure's locals declaration.  The closure will be allocated in
    \G the dictionary.
    ['] allocd :}* ;

: :}h ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-heap
    \G end a closure's locals declaration.  The closure will be allocated on
    \G the heap.
    ['] alloch :}* ;

forth definitions

: push-locals ( -- )
    locals-size @ locals-sizes >stack  locals-size off
    locals-list @ locals-lists >stack  locals-list off ;

: pop-locals ( -- )
    locals-lists stack> locals-list !
    locals-sizes stack> locals-size ! ;

locals-types definitions

: :}l ( vtaddr u latest latestxt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-locals
    \G end a closure's locals declaration.  The closure will be allocated on
    \G the local's stack.
    :}
    locals-size @ [ 3 cells maxaligned ]L +
    locals-size @ locals-list @ 2>r  pop-locals  locals-size +!
    get-current >r  [ ' locals >body ]l set-current
    0 new-local-mem
    s" " nextname ['] create-local1 dict-execute locals-size @ locals,
    r> set-current
    push-locals  2r> locals-list ! locals-size !
    ['] noop end-d ;

forth definitions

: (closure-;]) ( closure-sys lastxt -- )
    ]
    postpone THEN
    wrap! pop-locals ;

: closure> ( body -- addr ) \ gforth-experimental closure-end
    \G create trampoline head
    >l dodoes: >l lp@
    [ ' do-closure cell- @ ]L >l
    [ cell maxaligned cell <> ] [IF] 0 >l [THEN] ;
: end-dclosure ( unravel-xt -- closure-sys )
    >r wrap@
    postpone lit >mark
    ]] closure> [[ r> execute ]] AHEAD BUT THEN [[
    case locals-size @ \ special optimizations for few locals
	cell    of ]] @ >l   [[ endof
	2 cells of ]] 2@ 2>l [[ endof
	]] lp+!# [[ dup negate , ]] laddr# [[ 0 , dup ]] literal move [[
    endcase
    ['] (closure-;]) defstart  last @ lastcfa @ defstart ;

: [{: ( -- vtaddr u latest latestxt wid 0 ) \ gforth-experimental start-closure
    \G starts a closure.  Closures first declare the locals frame they are
    \G going to use, and then the code that is executed with those locals.
    \G Closures end like quotations with a @code{;]}.  The locals declaration
    \G ends depending where the closure's locals are created.  At run-time, the
    \G closure is created as trampolin xt, and fills the values of its local
    \G frame from the stack.  At execution time of the xt, the local frame is
    \G copied to the locals stack, and used inside the closure's code.  After
    \G return, those values are removed from the locals stack, and not updated
    \G in the closure itself.
    [: ] drop ;] defstart
    push-locals
    ['] end-dclosure is end-d  [: ]] lp> [[ ;] is endref,
    [ 3 cells maxaligned ]L extra-locals !
    postpone {:
; immediate compile-only

: <{: ( -- vtaddr u latest latestxt wid 0 ) \ gforth-experimental start-homelocation
    \G starts a home location
    push-locals postpone {:
; immediate compile-only

: ;> ( -- ) \ gforth-experimental end-homelocation
    \G end using a home location
    pop-locals ]] laddr# [[ 0 , ]] lp> [[
; immediate compile-only

false [IF]
    : foo [{: a f: b d: c xt: d :}d a . b f. c d. d ;] ;
    5 3.3e #1234. ' cr foo execute
    : homeloc <{: w^ a w^ b w^ c :}h a b c ;> ;
    1 2 3 homeloc >r ? ? ? r> free throw cr

    : A {: w^ k x1 x2 x3 xt: x4 xt: x5 | w^ B :} recursive
	k @ 0<= IF  x4 x5 f+  ELSE
	    B k x1 x2 x3 action-of x4 [{: B k x1 x2 x3 x4 :}L
		-1 k +!
		k @ B @ x1 x2 x3 x4 A ;] dup B !
	    execute  THEN ;
    : man-or-boy? ( n -- ) [: 1e ;] [: -1e ;] 2dup swap [: 0e ;] A f. ;
    
    \ start with: gforth -l64M -r8M closures.fs
    \ start with: gforth-fast -l4G -r512M closures.fs if you want to go up to 25
    14 set-precision
    20 0 [DO] [i] dup . !time man-or-boy? .time cr [LOOP]
[THEN]
