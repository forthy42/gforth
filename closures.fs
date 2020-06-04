\ A powerful closure implementation

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2018,2019 Free Software Foundation, Inc.

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
    cell- ;
: alloch ( size -- addr ) \ addr is the end of the allocated region
    dup allocate throw + ;
: allocd ( size -- addr ) \ addr is the end of the allocated region
    dp +! dp @ ;

: >lp ( addr -- r:oldlp ) r> lp@ >r >r lp! ;
opt: drop ]] laddr# [[ 0 , ]] >r lp! [[ ;
: lp> ( r:oldlp -- ) r> r> lp! >r ;
opt: drop ]] r> lp! [[ ;

Variable extra-locals ( additional hidden locals size )

locals-types definitions

: :}* ( vtaddr u latest latestnt wid 0 a-addr1 u1 ... xt -- ) \ gforth close-brace-dictionary
    0 lit, lits, here cell- >r
    compile, ]] >lp [[
    :}
    locals-size @ extra-locals @ + r> !
    ['] endref, end-d
    ['] execute is end-d  ['] noop is endref,
    extra-locals off ;

: :}xt ( vtaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-xt
    \G end a closure's locals declaration.  The closure will be allocated by
    \G the xt on the stack, so the closure's run-time stack effect is @code{(
    \G xt-alloc -- xt-closure}.
    \ run-time: ( xt size -- ... )
    [: swap execute ;] :}* ;

: :}d ( vtaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-dictionary
    \G end a closure's locals declaration.  The closure will be allocated in
    \G the dictionary.
    ['] allocd :}* ;

: :}h ( vtaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-heap
    \G end a closure's locals declaration.  The closure will be allocated on
    \G the heap.
    ['] alloch :}* ;

forth definitions

: push-locals ( list size -- )
    locals-size @ locals-sizes >stack  locals-size !
    locals-list @ locals-lists >stack  locals-list ! ;

: pop-locals ( -- )
    locals-lists stack> locals-list !
    locals-sizes stack> locals-size ! ;

: dummy-local, ( n -- )
    locals-size +!
    get-current >r  0 warnings !@ >r  [ ' locals >body ]l set-current
    s" " nextname create-local locals-size @ locals,
    r> warnings !  r> set-current ;

locals-types definitions

: :}l ( vtaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-locals
    \G end a closure's locals declaration.  The closure will be allocated on
    \G the local's stack.
    :}
    locals-size @ locals-list @ over 2>r  pop-locals
    [ 2 cells maxaligned ]L + dummy-local,
    2r> push-locals
    ['] noop end-d ;

forth definitions

: wrap-closure ( xt -- )
    dup >namevt @ >vtextra !  ['] does, set-optimizer
    finish-code  vt,  previous-section  wrap!  dead-code off ;

: (closure-;]) ( closure-sys lastxt -- )
    >r r@ wrap-closure
    orig? r> >namevt @ swap ! drop
    pop-locals ;

: closure-:-hook ( sys -- sys addr xt n )
    \ addr is the nfa of the defined word, xt its xt
    latest latestnt
    clear-leave-stack
    dead-code off
    defstart ;

: closure> ( body -- addr ) \ gforth-experimental closure-end
    \G create trampoline head
    dodoes: >l >l lp@ cell+ ;
: end-dclosure ( unravel-xt -- closure-sys )
    >r
    postpone lit >mark
    ]] closure> [[ r> execute
    wrap@ next-section finish-code|
    action-of :-hook >r  ['] closure-:-hook is :-hook
    :noname
    r> is :-hook
    case locals-size @ \ special optimizations for few locals
	cell    of ]] @ >l   [[ endof
	2 cells of ]] 2@ 2>l [[ endof
	]] lp+!# [[ dup negate , ]] laddr# [[ 0 , dup ]] literal move [[
    endcase
    ['] (closure-;]) colon-sys-xt-offset stick ;

: [{: ( -- vtaddr u latest latestnt wid 0 ) \ gforth-experimental start-closure
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
    #0. push-locals
    ['] end-dclosure is end-d  [: ]] lp> [[ ;] is endref,
    [ 2 cells maxaligned ]L extra-locals !
    postpone {:
; immediate compile-only

: <{: ( -- vtaddr u latest latestnt wid 0 ) \ gforth-experimental start-homelocation
    \G starts a home location
    #0. push-locals postpone {:
; immediate compile-only

: ;> ( -- ) \ gforth-experimental end-homelocation
    \G end using a home location
    pop-locals ]] laddr# [[ 0 , ]] lp> [[
; immediate compile-only

\ stack-based closures without name

: (;*]) ( xt -- vt )
    >r ] postpone endscope third locals-list ! postpone endscope
    r@ wrap-closure  r> >namevt @ ;

: (;]l) ( xt1 n xt2 -- ) (;*]) >r dummy-local,
    compile, r> lit, ]] closure> [[ ;
: (;]*) ( xt0 xt1 n xt2 -- )  (;*]) >r lit, swap compile,
    ]] >lp [[ compile, r> lit, ]] closure> lp> [[ ;

: :l ( -- xt )                  ['] (;]l) ; immediate restrict
: :h ( -- xt1 xt2 )  ['] alloch ['] (;]*) ; immediate restrict
: :d ( -- xt1 xt2 )  ['] allocd ['] (;]*) ; immediate restrict

: [*:: [{: xt@ xt>l size :}d
	>r xt>l size [ 2 cells ]L + maxaligned postpone [: xt@ compile,
	r> [ colon-sys-xt-offset 2 + ]L stick ;]
    alias immediate restrict ;

cell 4 = [IF]  :noname ( n -- xt )  false >l >l ;  [ELSE]  ' >l  [THEN]
' @  swap  1 cells  [*:: [n: ( xt -- colon-sys )
' 2@ ' 2>l 2 cells  [*:: [d: ( xt -- colon-sys )
' f@ ' f>l 1 floats [*:: [f: ( xt -- colon-sys )

\ combined names (used in existing code)

: [n:l ( -- colon-sys ) ]] :l [n: [[ ; immediate restrict
: [n:h ( -- colon-sys ) ]] :h [n: [[ ; immediate restrict
: [n:d ( -- colon-sys ) ]] :d [n: [[ ; immediate restrict

: [d:l ( -- colon-sys ) ]] :l [d: [[ ; immediate restrict
: [d:h ( -- colon-sys ) ]] :h [d: [[ ; immediate restrict
: [d:d ( -- colon-sys ) ]] :d [d: [[ ; immediate restrict

: [f:l ( -- colon-sys ) ]] :l [f: [[ ; immediate restrict
: [f:h ( -- colon-sys ) ]] :h [f: [[ ; immediate restrict
: [f:d ( -- colon-sys ) ]] :d [f: [[ ; immediate restrict

[IFDEF] test-it
    : foo [{: a f: b d: c xt: d :}d a . b f. c d. d ;] ;
    5 3.3e #1234. ' cr foo execute
    : homeloc <{: w^ a w^ b w^ c :}h a b c ;> ;
    1 2 3 homeloc >r ? ? ? r> free throw cr

    : A {: w^ k x1 x2 x3 xt: x4 xt: x5 | w^ B :} recursive
	k @ 0<= IF  x4 x5 +  ELSE
	    B k x1 x2 x3 action-of x4 [{: B k x1 x2 x3 x4 :}L
		-1 k +!
		k @ B @ x1 x2 x3 x4 A ;] dup B !
	    execute  THEN ;
    : man-or-boy? ( n -- n' ) [: 1 ;] [: -1 ;] 2dup swap [: 0 ;] A ;
    
    \ start with: gforth -l64M -r8M closures.fs
    \ start with: gforth-fast -l6G -r768M closures.fs if you want to go up to 26
    20 0 [DO] [i] dup . !time man-or-boy? . .time cr [LOOP]
[THEN]
