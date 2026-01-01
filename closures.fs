\ A powerful closure implementation

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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
false Value 1t-closure?

: alloch ( size -- addr )
    \ addr is the end of the allocated region
    dup allocate throw + ;
: allocd ( size -- addr )
    \ addr is the end of the allocated region
    dp +! dp @ ;

: >lp ( addr -- r:oldlp ) r> lp@ >r >r lp! ;
opt: drop ]] lp@ >r lp! [[ ;
: lp> ( r:oldlp -- ) r> r> lp! >r ;
opt: drop ]] r> lp! [[ ;

Variable extra-locals ( additional hidden locals size )

locals-types definitions

: :}* ( hmaddr u latest latestnt wid 0 a-addr1 u1 ... xt -- ) \ gforth-internal colon-close-brace-star
    0 lit, lits, here cell- >r
    compile, ]] >lp [[
    :}
    locals-size @ extra-locals @ + r> !
    ['] endref, end-d
    ['] execute is end-d  ['] noop is endref,
    extra-locals off activate-locals ;

: :}xt ( hmaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth colon-close-brace-x-t
    \G Ends a closure's locals definition.  The closure will be allocated by
    \G the xt on the stack, so the closure's run-time stack effect is @code{(
    \G ... xt-alloc -- xt-closure )}.
    \ run-time: ( xt size -- ... )
    [: swap execute ;] :}* ;

: :}d ( hmaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth colon-close-brace-d
    \G Ends a closure's locals definition.  The closure will be allocated in
    \G the dictionary.
    ['] allocd :}* ;

: :}h ( hmaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth colon-close-brace-h
    \G Ends a closure's locals definition.  At the run-time of the
    \G surrounding definition this allocates the closure on the heap;
    \G you are then responsible for deallocating it with
    \G @code{free-closure}.
    ['] alloch :}* ;

: :}h1 ( hmaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth colon-close-brace-h-one
    \G Ends a closure's locals definition.  The closure is deallocated
    \G after the first execution, so this is a one-shot closure,
    \G particularly useful in combination with @code{send-event}
    \G (@pxref{Message queues}).
    true to 1t-closure? ['] alloch :}* ;

forth definitions

: push-locals ( list size -- )
    locals-size @ locals-sizes >stack  locals-size !
    locals-list @ locals-lists >stack  locals-list ! ;

: pop-locals ( -- )
    locals-lists stack> locals-list !
    locals-sizes stack> locals-size ! ;

: dummy-local, ( n -- )
    locals-size +!
    get-current >r  0 warnings !@ >r  [ ' locals >wordlist ]l set-current
    s" " nextname ['] n/a dup create-local locals-size @ locals,
    r> warnings !  r> set-current ;

locals-types definitions

: :}l ( hmaddr u latest latestnt wid 0 a-addr1 u1 ... -- ) \ gforth close-brace-locals
    \G Ends a closure's locals definition.  The closure will be allocated on
    \G the locals stack.
    :}
    locals-size @ locals-list @ over 2>r  pop-locals
    [ 2 cells maxaligned ]L + dummy-local,
    2r> push-locals
    ['] noop end-d
    activate-locals ;

forth definitions

: wrap-closure ( xt -- )
    dup >extra !  ['] does, set-optimizer
    flush-code  hm,  wrap!  hmtemplate off \ dead hmtemplate link
    previous-section  dead-code off ;

: (closure-;]) ( closure-sys lastxt -- )
    dup >r wrap-closure
    r> >namehm @ swap !
    pop-locals ;

[IFUNDEF] in-colon-def?
    0 Value in-colon-def? ( -- flag ) \ gforth-experimental
    \G @i{flag} is true if and only if there is an active colon
    \G definition to which @word{compile,} and friends would append
    \G code.
[THEN]

: closure-:-hook ( sys -- sys addr xt n )
    \ addr is the nfa of the defined word, xt its xt
    :-hook1
    ['] here locals-headers
    clear-leave-stack
    dead-code off
    defstart
    true to in-colon-def? ;

: free-closure ( xt -- ) \ gforth
    \G Free the heap-allocated closure @i{xt}.
    >cfa free throw ;
: closure> ( hmaddr -- addr ) \ gforth-internal closure-end
    \G create trampoline head
    [ 0 >body ] [IF] dodoes: >l >l lp@ cell+
    [ELSE] >l dodoes: >l lp@ cell+ cell+ [THEN] ;
: end-dclosure ( unravel-xt -- closure-sys )
    >r
    postpone lit here 0 ,
    ]] closure> [[ r> execute
    wrap@ next-section
    action-of :-hook >r  ['] closure-:-hook is :-hook
    :noname
    r> is :-hook
    1t-closure? IF  ]] dup [[ THEN
    case locals-size @ \ special optimizations for few locals
	cell    of ]] @ >l   [[ endof
	2 cells of ]] 2@ 2>l [[ endof
	dup negate ]] literal lp+! lp@ [[ dup ]] literal move [[
    endcase
    1t-closure? IF  ]] free-closure [[ THEN
    false to 1t-closure?
    ['] (closure-;]) colon-sys-xt-offset stick ;

: [{: ( compilation -- hmaddr u latest wid 0 ; instantiation ... -- xt ) \ gforth start-closure
    \G Starts a closure.  Closures started with @code{[@{:} define
    \G locals for use inside the closure.  The locals-definition part
    \G ends with @code{:@}l}, @code{:@}h}, @code{:@}h1}, @code{:@}d}
    \G or @code{:@}xt}.  The rest of the closure definition is Forth
    \G code.  The closure ends with @code{;]}.  When the closure
    \G definition is encountered during execution (closure creation
    \G time), the values going into the locals are consumed, and an
    \G execution token (xt) is pushed on the stack; when that
    \G execution token is executed (with @code{execute}, through
    \G @code{compile,} or a deferred word), the code in the closure is
    \G executed (closure execution time).  If the xt of a closure is
    \G executed multiple times, the values of the locals at the start
    \G of code execution are those from closure-creation time,
    \G unaffected by any locals-changes in earlier executions of the
    \G closure.
    [: ] drop ;] defstart
    #0. push-locals
    ['] end-dclosure is end-d [: ]] lp> [[ ;] is endref,
    [ 2 cells maxaligned ]L extra-locals !
    postpone {:
; immediate compile-only

: <{: ( compilation -- colon-sys ; run-time -- ) \ gforth-obsolete start-homelocation
    \G Starts defining a home location block.
    #0. push-locals postpone {:
; immediate compile-only

: ;> ( compilation colon-sys -- ; run-time -- addr ) \ gforth-obsolete end-homelocation
    \G Ends defining a home location; @i{addr} is the start address of
    \G the home-location block (used for deallocation).
    pop-locals ]] lp@ lp> [[
; immediate compile-only

\ stack-based closures without name

: (;]*) ( xt -- hm )
    >r ] ]] UNREACHABLE ENDSCOPE [[
    r@ wrap-closure  r> >namehm @ ;

: (;]l) ( xt1 n xt2 -- )
    (;]*) >r dummy-local,
    compile, r> lit, ]] closure> [[ ;

: alloc-by-xt, ( xt n -- )
    lit, swap compile, ]] >lp [[ ;
: (;]xt) ( xt0 xt1 n xt2 -- )
    (;]*) >r alloc-by-xt,
    compile, r> lit, ]] closure> lp> [[ ;

: :l ( -- xt )                  ['] (;]l) ; immediate restrict
: :h ( -- xt1 xt2 )  ['] alloch ['] (;]xt) ; immediate restrict
: :h1 ( -- xt1 xt2 ) true to 1t-closure? ]] :h [[ ; immediate restrict
: :d ( -- xt1 xt2 )  ['] allocd ['] (;]xt) ; immediate restrict

: [*:: [{: xt@ xt>l size :}d
	>r xt>l size [ 2 cells ]L + maxaligned
	postpone [:
	1t-closure? IF ]] dup >r [[ THEN
	xt@ compile,
	1t-closure? IF ]] r> free-closure [[ THEN
	false to 1t-closure?
	r> [ colon-sys-xt-offset 2 + ]L stick ;]
    alias immediate restrict ;

cell 4 = [IF]  :noname ( n -- xt )  false >l >l ;  [ELSE]  ' >l  [THEN]
' @  swap  1 cells  [*:: [n: ( xt -- colon-sys )
' 2@ ' 2>l 2 cells  [*:: [d: ( xt -- colon-sys )
' f@ ' f>l 1 floats [*:: [f: ( xt -- colon-sys )

\ combined names (used in existing code)

: [n:l ( compilation -- colon-sys; run-time: n -- xt ; xt execution: -- n ) \ gforth open-bracket-n-colon-l
    ]] :l [n: [[ ; immediate restrict
: [d:l ( compilation -- colon-sys; run-time: d -- xt ; xt execution: -- d ) \ gforth open-bracket-d-colon-l
    ]] :l [d: [[ ; immediate restrict
: [f:l ( compilation -- colon-sys; run-time: r -- xt ; xt execution: -- r ) \ gforth open-bracket-r-colon-l
    ]] :l [f: [[ ; immediate restrict

: [n:d ( compilation -- colon-sys; run-time: n -- xt ; xt execution: -- n ) \ gforth open-bracket-n-colon-d
    ]] :d [n: [[ ; immediate restrict
: [d:d ( compilation -- colon-sys; run-time: d -- xt ; xt execution: -- d ) \ gforth open-bracket-d-colon-d
    ]] :d [d: [[ ; immediate restrict
: [f:d ( compilation -- colon-sys; run-time: r -- xt ; xt execution: -- r ) \ gforth open-bracket-r-colon-d
    ]] :d [f: [[ ; immediate restrict

: [n:h ( compilation -- colon-sys; run-time: n -- xt ; xt execution: -- n ) \ gforth open-bracket-n-colon-h
    ]] :h [n: [[ ; immediate restrict
: [d:h ( compilation -- colon-sys; run-time: d -- xt ; xt execution: -- d ) \ gforth open-bracket-d-colon-h
    ]] :h [d: [[ ; immediate restrict
: [f:h ( compilation -- colon-sys; run-time: r -- xt ; xt execution: -- r ) \ gforth open-bracket-r-colon-h
    ]] :h [f: [[ ; immediate restrict

: [n:h1 ( compilation -- colon-sys; run-time: n -- xt ; xt execution: -- n ) \ gforth open-bracket-n-colon-h1
    ]] :h1 [n: [[ ; immediate restrict
: [d:h1 ( compilation -- colon-sys; run-time: d -- xt ; xt execution: -- d ) \ gforth open-bracket-d-colon-h1
    ]] :h1 [d: [[ ; immediate restrict
: [f:h1 ( compilation -- colon-sys; run-time: r -- xt ; xt execution: -- r ) \ gforth open-bracket-r-colon-h1
    ]] :h1 [f: [[ ; immediate restrict

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
