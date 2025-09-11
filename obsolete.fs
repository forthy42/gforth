\ some obsolete code that is not needed anywhere else

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2017,2019,2022,2023,2024 Free Software Foundation, Inc.

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

\ list obsolete words

-1 warnings !@ \ supress obsolete warnings

[IFDEF] obsolete-mask
    : obsoletes ( -- ) \ gforth-internal
	\G show all obsolete words
	cr 0 [: dup >f+c @ obsolete-mask and
	    IF  .word  ELSE  drop  THEN  true ;]
	context @ traverse-wordlist  drop ;
[ELSE]
    : obsolete ;
[THEN]
\ from kernel

: header, ( c-addr u -- ) \ gforth-obsolete
    \G create a header for a named word
    hm, name, hmtemplate namehm, named-hm ;

: longstring, ( c-addr u -- ) \ gforth-obsolete
    \G puts down string as longcstring
    dup , mem, ;

' latestxt alias lastxt ( -- xt ) \ gforth-obsolete

\G old name for @code{latestxt}.

: [(')]  ( compilation "name" -- ; run-time -- nt ) \ gforth-obsolete bracket-paren-tick
    (') postpone Literal ; immediate restrict

: code-address! ( c_addr xt -- ) \ gforth-obsolete
    \G Change a code field with code address @i{c-addr} at @i{xt}.
    next-section latestnt >r dup xt>name make-latest
    over case
        docon:     of ['] constant, endof
        docol:     of ['] :,        endof
        dovar:     of ['] variable, endof
        douser:    of ['] user,     endof
        dodefer:   of ['] defer,    endof
        dofield:   of ['] field+,   endof
        doabicode: of ['] abi-code, endof
        drop ['] general-compile,
    endcase
    set-optimizer
    r> make-latest previous-section
    only-code-address! ;

: definer! ( definer xt -- ) \ gforth-obsolete
    \G The word represented by @var{xt} changes its behaviour to the
    \G behaviour associated with @var{definer}.
    over 3 and case
        0 of code-address! endof
        1 of swap 3 invert and swap does-code! endof
        2 of swap 3 invert and swap
            do;abicode: any-code! ['] ;abi-code, set-optimizer endof
        -12 throw
    endcase ;

' opt: alias comp: ( compilation -- colon-sys2 ; run-time -- nest-sys ) \ gforth-obsolete
\G Use @code{opt:} instead.

' parse-name alias parse-word ( -- c-addr u ) \ gforth-obsolete
\G old name for @code{parse-name}; this word has a conflicting
\G behaviour in some other systems.

' parse-name alias name ( -- c-addr u ) \ gforth-obsolete
\G old name for @code{parse-name}
    
: .strings ( addr u -- ) \ gforth-obsolete
    \G list the strings from an array of string descriptors at addr
    \G with u entries, one per line.
    2 cells MEM+DO
        cr I 2@ type
    LOOP ;

: +place {: c-addr1 u1 c-addr2 -- :} \ gforth-obsolete plus-place
    \G append the string @var{c-addr1 u} to counted string at
    \G @var{c-addr2} and increase it's length by @var{u}.  Only write
    \G up to the maximum string length (255 bytes, plus the count
    \G byte).
    c-addr2 count {: c-addr u2 :}
    u2 u1 + $ff min {: u :}
    c-addr1 c-addr u u2 /string move
    u c-addr2 c! ;

\ memory words with u or s prefix

' w@  alias uw@  ( c-addr -- u  ) obsolete
' l@  alias ul@  ( c-addr -- u  ) obsolete
[IFDEF] x@
' x@  alias ux@  ( c-addr -- u  ) obsolete
[THEN]
' xd@ alias uxd@ ( c-addr -- ud ) obsolete
inline: sxd@ ( c-addr -- d ) ]] xd@ xd>s [[ ;inline obsolete

\ various byte-order dependent memory words
\ replacement: compose sequences like "uw@ wbe w>s"

inline: be-w!  (  x c-addr -- )  ]] >r wbe  r>  w! [[ ;inline obsolete
inline: be-l!  (  x c-addr -- )  ]] >r lbe  r>  l! [[ ;inline obsolete
[IFDEF] x!
inline: be-x!  (  x c-addr -- )  ]] >r xbe  r>  x! [[ ;inline obsolete
[THEN]
inline: be-xd! ( xd c-addr -- )  ]] >r xdbe r> xd! [[ ;inline obsolete
inline: le-w!  (  x c-addr -- )  ]] >r wle  r>  w! [[ ;inline obsolete
inline: le-l!  (  x c-addr -- )  ]] >r lle  r>  l! [[ ;inline obsolete
[IFDEF] x!
inline: le-x!  (  x c-addr -- )  ]] >r xle  r>  x! [[ ;inline obsolete
[THEN]
inline: le-xd! ( xd c-addr -- )  ]] >r  xdle  r>   xd! [[ ;inline obsolete
inline:  be-uw@ ( c-addr -- u )  ]]  w@  wbe [[ ;inline obsolete
inline:  be-ul@ ( c-addr -- u )  ]]  l@  lbe [[ ;inline obsolete
[IFDEF] x@
inline:  be-ux@ ( c-addr -- u )  ]]  x@  xbe [[ ;inline obsolete
[THEN]
inline: be-uxd@ ( c-addr -- ud ) ]] xd@ xdbe [[ ;inline obsolete
inline:  le-uw@ ( c-addr -- u )  ]]  w@  wle [[ ;inline obsolete
inline:  le-ul@ ( c-addr -- u )  ]]  l@  lle [[ ;inline obsolete
[IFDEF] x@
inline:  le-ux@ ( c-addr -- u )  ]]  x@  xle [[ ;inline obsolete
[THEN]
inline: le-uxd@ ( c-addr -- ud ) ]] xd@ xdle [[ ;inline obsolete

\ legacy rectype stuff

: rectype>int  ( rectype -- xt ) \ gforth-obsolete
    >body @ ;
: rectype>comp ( rectype -- xt ) \ gforth-obsolete
    cell >body + @ ;
: rectype>post ( rectype -- xt ) \ gforth-obsolete
    2 cells >body + @ ;

: rectype ( int-xt comp-xt post-xt -- rectype ) \ gforth-obsolete
    \G create a new unnamed recognizer token
    noname (translate:) latestxt ; 

: rectype: ( int-xt comp-xt post-xt "name" -- ) \ gforth-obsolete
    \G create a new recognizer table
    rectype Constant ;

0 Constant rectype-null \ gforth-obsolete
translate-nt AConstant rectype-nt \ gforth-obsolete
translate-num AConstant rectype-num \ gforth-obsolete
translate-dnum AConstant rectype-dnum \ gforth-obsolete
translate-to Constant rectype-to \ gforth-obsolete
[ifdef] translate-eval
    translate-eval Constant rectype-eval \ gforth-obsolete
[then]
translate-env Constant rectype-env \ gforth-obsolete
[IFDEF] translate-string
    translate-string Constant rectype-string \ gforth-obsolete
[THEN]

: get-recognizers ( -- xt1 .. xtn n ) \ gforth-obsolete
    \G push the content on the recognizer stack
    ['] forth-recognize get-recognizer-sequence ;
: set-recognizers ( xt1 .. xtn n -- ) \ gforth-obsolete
    \G set the recognizer stack from content on the stack
    ['] forth-recognize set-recognizer-sequence ;

\ from ekey.fs
' k-f1  alias k1  ( -- u ) \ gforth-obsolete
' k-f2  alias k2  ( -- u ) \ gforth-obsolete
' k-f3  alias k3  ( -- u ) \ gforth-obsolete
' k-f4  alias k4  ( -- u ) \ gforth-obsolete
' k-f5  alias k5  ( -- u ) \ gforth-obsolete
' k-f6  alias k6  ( -- u ) \ gforth-obsolete
' k-f7  alias k7  ( -- u ) \ gforth-obsolete
' k-f8  alias k8  ( -- u ) \ gforth-obsolete
' k-f9  alias k9  ( -- u ) \ gforth-obsolete
' k-f10 alias k10 ( -- u ) \ gforth-obsolete
' k-f11 alias k11 ( -- u ) \ gforth-obsolete
' k-f12 alias k12 ( -- u ) \ gforth-obsolete
\ shifted fuinction keys (don't work in xterm (same as unshifted, but
\ s-k1..s-k8 work in the Linux console)
k-f1  k-shift-mask or constant s-k1  ( -- u ) \ gforth-obsolete 
k-f2  k-shift-mask or constant s-k2  ( -- u ) \ gforth-obsolete 
k-f3  k-shift-mask or constant s-k3  ( -- u ) \ gforth-obsolete 
k-f4  k-shift-mask or constant s-k4  ( -- u ) \ gforth-obsolete 
k-f5  k-shift-mask or constant s-k5  ( -- u ) \ gforth-obsolete 
k-f6  k-shift-mask or constant s-k6  ( -- u ) \ gforth-obsolete 
k-f7  k-shift-mask or constant s-k7  ( -- u ) \ gforth-obsolete 
k-f8  k-shift-mask or constant s-k8  ( -- u ) \ gforth-obsolete 
k-f9  k-shift-mask or constant s-k9  ( -- u ) \ gforth-obsolete 
k-f10 k-shift-mask or constant s-k10 ( -- u ) \ gforth-obsolete 
k-f11 k-shift-mask or constant s-k11 ( -- u ) \ gforth-obsolete
k-f12 k-shift-mask or constant s-k12 ( -- u ) \ gforth-obsolete

\ from intcomp.fs

\ used like
\ : <name> create-interpret/compile ...
\     interpretation> ... <interpretation
\     compilation> ... <compilation ;

require rec-tick.fs

synonym create-interpret/compile create ( "name" -- ) \ gforth-obsolete

: interpretation> ( compilation. -- orig colon-sys ) \ gforth-obsolete
    postpone [: ; immediate restrict

: <interpretation ( compilation. orig colon-sys -- ) \ gforth-obsolete
    ]] ;] set-does> [[ ; immediate restrict

: compilation> ( compilation. -- orig colon-sys ) \ gforth-obsolete
    \G use a anonymous closure on the heap, acceptable leakage
    ]] [: >body [n:h [[ ; immediate restrict

: <compilation ( orig colon-sys -- ) \ gforth-obsolete
    ]] ;] `execute ;] set->comp [[ ; immediate restrict

\ example
\ : constant ( n "name" -- )
\     create-interpret/compile
\     ,
\ interpretation>    @                    <interpretation
\ compilation>       @ postpone literal   <compilation ;
\ 
\ 5 constant five
\ 
\ cr
\ five . cr
\ : fuenf five ;
\ see fuenf cr


\ from stuff.fs

\ const-does>

: compile-literals ( w*u u -- ; run-time: -- w*u ) recursive
    \ compile u literals, starting with the bottommost one
    ?dup-if
	swap >r 1- compile-literals
	r> POSTPONE literal
    endif ;

: compile-fliterals ( r*u u -- ; run-time: -- w*u ) recursive
    \ compile u fliterals, starting with the bottommost one
    ?dup-if
	{ F: r } 1- compile-fliterals
	r POSTPONE fliteral
    endif ;

[IFUNDEF] in-colon-def?
    0 Value in-colon-def? ( -- flag ) \ gforth-experimental
    \G allows to check if there currently is an active colon
    \G definition where you can append code to.
[THEN]

: (const-does>) ( w*uw r*ur uw ur target "name" -- )
    \ define a colon definition "name" containing w*uw r*ur as
    \ literals and a call to target.
    { uw ur target }
    ['] on create-from \ start colon def without stack junk
    true to in-colon-def?
    ur compile-fliterals uw compile-literals
    target compile, POSTPONE exit flush-code reveal
    false to in-colon-def? ;

: const-does> ( run-time: w*uw r*ur uw ur "name" -- ) \ gforth-obsolete const-does
    \G Defines @var{name} and returns.
    \G  
    \G @var{name} execution: pushes @var{w*uw r*ur}, then performs the
    \G code following the @code{const-does>}.
    basic-block-end here >r 0 POSTPONE literal
    POSTPONE (const-does>)
    POSTPONE ;
    noname : POSTPONE rdrop
    latestxt r> cell+ ! \ patch the literal
; immediate


synonym what's action-of ( interpretation "name" -- xt; compilation "name" -- ; run-time -- xt ) \ gforth-obsolete
\G Old name of @code{action-of}


' name>interpret alias name>int ( nt -- xt|0 ) \ gforth-obsolete name-to-int
    \G @i{xt} represents the interpretation semantics @i{nt}; returns
    \G 0 if @i{nt} has no interpretation semantics

' name>compile alias name>comp ( nt -- w xt ) \ gforth-obsolete name-to-comp
\G @i{w xt} is the compilation token for the word @i{nt}.

\ from kernel/cond.fs

: CONTINUE ( dest-sys j*sys -- dest-sys j*sys ) \ gforth-obsolete
    \g jump to the next outer BEGIN
    depth 0 ?DO  I pick dest = IF
	    I cs-item-size / cs-pick postpone AGAIN
	    UNLOOP  EXIT  THEN
    cs-item-size +LOOP
    true abort" no BEGIN found" ; immediate restrict

\ A symmetric version of "+LOOP". I.e., "-high -low ?DO -inc S+LOOP"
\ will iterate as often as "high low ?DO inc S+LOOP". For positive
\ increments it behaves like "+LOOP". Use S+LOOP instead of +LOOP for
\ negative increments.
: S+LOOP ( compilation do-sys -- ; run-time loop-sys1 n -- | loop-sys2 )	\ gforth-obsolete	s-plus-loop
    \G @xref{Counted Loops}.
 ['] (s+loop) ['] (s+loop)-lp+!# loop-like ; immediate restrict

\ from search.fs
' id. alias .name ( nt -- ) \ gforth-obsolete  dot-name
\G Gforth <=0.5.0 name for @code{id.}.

warnings !
