\ stack depth checking

\ Authors: Anton Ertl
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

"xRr" save-mem \ x: data, R: return, r: fp
constant stacks
constant stack-letters

0
cfield: sd-in  \ number of stack items of a stack consumed by word or sequence
cfield: sd-out \ number of stack items of a stack produced by word or sequence
constant sd-size \ stack effect for one stack

sd-size
sd-size +field anchor-offset \ when following the link to the parent,
                             \ apply the offset (as if compiling a
                             \ word) to get the corresponding stack
                             \ descriptor for the parent.
field: anchor-parent \ address of parent anchor, or to itself (if no parent)
constant anchor-size

anchor-size stacks * constant ase-size
\ an anchored stack effect (ase) consists of STACKS anchors (one anchor for
\ each stack)

unused extra-section in-stack-check-section
\ for now don't do proper memory reclamation
ase-size ' small-allot in-stack-check-section constant dummy-ase

dummy-ase value current-ase
0 value colon-ase \ ase at the start of a colon definition

: one-stack-effect {: nin1 nout1 nin2 nout2 -- nin3 nout3 :}
    nin2 nout1 - ( n )
    nin1 over 0 max +
    nout2 rot 0 min - ;
    
: sd@ ( sd -- nin nout )
    dup sd-in c@ swap sd-out c@ ;



: do-one-stack-effect {: sd1 sd2 -- :}
    \ given a one-stack effect sd1, change it to be the one-stack
    \ effect of sd1 followed by sd2.
    sd1 sd@ sd2 sd@ one-stack-effect sd1 sd-out c! sd1 sd-in c! ;

: do-stack-effect ( as sds -- )
    \ given an anchored stack effect as, change it to be the stack effect of
    \ as followed by sds.
    stacks 0 ?do
	2dup do-one-stack-effect
	sd-size + swap anchor-size + swap loop
    2drop ;

table constant prim-stack-effects

: current-execute ( ... wordlist xt -- ... )
    get-current >r swap set-current catch r> set-current throw ;

: xt-stack-effect {: w^ xt -- :}
    xt cell nextname prim-stack-effects ['] create current-execute
    ['] do-stack-effect set-does> ;

: stack-effect ( "name" -- )
    parse-name find-name ?dup-if
	name>interpret xt-stack-effect
    then ;

: stack-effect-unknown ( "name" -- )
    stack-effect ;

require prim_effects.fs

\ redefine some prim-effects for control-flow primitives
stack-effect call 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
stack-effect ;s 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
stack-effect-unknown does-xt 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,

: .se-side {: a stride -- :}
    \ a is the address of a field of the first sd in a stack effect description
    a stacks 0 ?do
	dup c@ 0 ?do
	    stack-letters j + c@ emit loop
	stride + loop
    drop ;

: .stack-effects ( se stride -- )
    over sd-in over .se-side '-' emit swap sd-out swap .se-side ;

: anchor-init ( a -- )
    dup anchor-size erase
    dup anchor-parent ! ;

: anchor-root ( a1 -- a2 )
    begin
	dup anchor-parent @ tuck =
    until ;

: ase-init ( ase -- )
    stacks 0 ?do
	dup anchor-init
	anchor-size + loop
    drop ;

: .ase ( ase -- )
    \ print an anchored stack effect:
    \ !! deal with parent and offset
    anchor-size .stack-effects ;

: prim-stack-check ( xt -- xt )
    dup {: w^ xt :}
    current-ase xt cell prim-stack-effects find-name-in dup if
	name>int execute
    else
	2drop cr ." unknown" then
    cr current-ase .ase ;

: stack-check-:-hook ( -- )
    defers :-hook
    [:  ase-size small-allot dup ase-init
	dup to current-ase to colon-ase ;] in-stack-check-section ;

: current>stack-effect ( xt -- )
    xt-stack-effect
    current-ase stacks 0 ?do
	dup sd-in c@ c, dup sd-out c@ c, anchor-size + loop
    drop ;

: call-stack-check ( xt -- xt )
    prim-stack-check ;

: does-stack-check ( xt -- xt )
    ['] lit prim-stack-check drop
    dup >namevt @ >vtextra @ prim-stack-check drop ;

: :stack-effect ( -- )
    \ create stack effect header for the current colon definition
    latestnt @
    lastxt ['] current>stack-effect in-stack-check-section
    make-latest ;

: stack-check-;-hook ( -- )
    \ !! is current-ase connected with start-ase?
    current-ase anchor-size + dup sd-in c@ swap sd-out c@ or 0<>
    [: ." return stack error in " lastnt @ .name  current-ase .ase ;] ?warning
    :stack-effect
    cr ." at ;: " current-ase .ase
    dummy-ase to current-ase defers ;-hook ;

: copy-ase ( -- ase )
    \ ase is a copy of current-ase; used in cs-item-pushing words
    ase-size ['] small-allot in-stack-check-section
    current-ase over ase-size move
    dup stacks 0 ?do \ the copy has 0 offsets from the original
	dup anchor-offset 0 over sd-in c! 0 swap sd-out c!
	anchor-size + loop
    drop ;

: anchor-effect1 {: a -- nin nout a2 :}
    a sd@ a begin ( nin1 nout1 a1 )
        dup anchor-parent @ tuck <> while
            dup >r anchor-offset sd@ one-stack-effect r> repeat
    assert( dup anchor-offset sd@ #0. d= ) ;

: anchor-effect ( a -- nin nout )
    anchor-effect1 drop ;

: compare-anchors {: a1 a2 -- :}
    a1 anchor-effect {: a1-in a1-out :}
    a2 anchor-effect {: a2-in a2-out :}
    a1-in a2-in - 0 max dup a2 sd-in c+! a2 sd-out c+!
    a2-in a1-in - 0 max dup a1 sd-in c+! a1 sd-out c+!
    a1 anchor-effect a2 anchor-effect assert( fourth third = ) d<>
    [: ." stack depth mismatch in "lastnt @ .name current-ase .ase ;] ?warning
    \ !! also print the ase compared to
    \ !! adjust the anchors for common maximum depth
;

: synchronize-anchors {: a1 a2 -- :}
    \ make the root of a1 the common root of a1 and a2
    a1 anchor-effect1 {: a1-in a1-out root1 :}
    a2 anchor-effect1 {: a2-in a2-out root2 :}
    a2-in a1-in - 0 max
    \ !!
    abort ; \ not yet implemented

: match-anchors {: a1 a2 -- :}
    \ match the two anchors; if they don't already have a common root,
    \ they have it afterwards; if they have a common root, compare the
    \ stack effects (taking offsets into account), and report if they
    \ do not match
    a1 anchor-root a2 anchor-root = if
	a1 a2 compare-anchors	
    else
	a1 a2 synchronize-anchors
    then ;

: match-ase ( ase -- )
    \ make ase match with current-ase; if they mismatch, produce a warning.
    \ used in cs-item-consuming words
    current-ase stacks 0 ?do
	2dup match-anchors
	anchor-size + swap anchor-size + swap loop
    2drop ;

`prim-stack-check is prim-check
`call-stack-check is call-check
`does-stack-check is does-check
`stack-check-:-hook is :-hook
`stack-check-;-hook is ;-hook

`copy-ase is push-stack-state
`match-ase is pop-stack-state


false [if] \ test
    : myconst create , `@ set-does> ;


    5 myconst five
    : foo r> >r f@ ;
    : bar >r foo r> ;
    : bla five ;


    : if0 if swap then ;
    : begin0 begin dup until ;
    : begin1 begin dup + again ;
    : do0 do i @ + loop ;
    : doerr do i loop ;
    \ : if1 if nip else drop then ;
    
    \ create ase1 ase-size allot
    \ ase1 ase-init
    \ ase1 .ase cr
    \ ase1 `r> pad ! pad cell prim-stack-effects find-name-in name>int execute
    \ ase1 .ase cr
    \ ase1 `>r pad ! pad cell prim-stack-effects find-name-in name>int execute
    \ ase1 .ase cr
    \ ase1 `f@ pad ! pad cell prim-stack-effects find-name-in name>int execute
    \ ase1 .ase cr
    
    
[then]



0 [if]
: compare-anchors {: a1 a2 -- :}
    ... ;

: synchronize-anchors {: a1 a2 -- a :}
    ... ;
    
: anchors-join {: a1 a2 -- a :}
    a1 anchor-root a2 anchor-root = if
	a1 a2 compare-anchors a1
    else
	a1 a2 anchors-synchronize
    then ;


' noop is prim-check ( xt -- xt )
[then]
