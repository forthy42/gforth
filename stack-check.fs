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
cfield: anchor-offset \ !!! document
field: anchor-parent \ address of parent anchor, or to itself (if no parent)
constant anchor-size

anchor-size stacks * constant ase-size
\ an anchored stack effect constists of STACKS anchors (one anchor for
\ each stack)

unused extra-section in-stack-check-section
  \ for now don't do proper memory reclamation
0 value colon-ase

: do-one-stack-effect {: sd1 sd2 -- :}
    \ given a one-stack effect sd1, change it to be the one-stack
    \ effect of sd1 followed by sd2.  
    sd2 sd-in c@ sd1 sd-out c@ - ( n )
    sd2 sd-out c@ over 0 min - sd1 sd-out assert( over 0>= ) c! ( +n )
    0 max sd1 sd-in c+! ;

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

: stack-effect ( "name" -- )
    parse-name find-name ?dup-if
	name>interpret {: w^ xt :}
	xt cell nextname prim-stack-effects ['] create current-execute
	['] do-stack-effect set-does>
    then ;

: stack-effect-unknown ( "name" -- )
    stack-effect ;

require prim_effects.fs

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
    colon-ase xt cell prim-stack-effects find-name-in name>int execute
    cr colon-ase .ase ;

: stack-check-:-hook ( -- )
    defers :-hook
    [: here dup to colon-ase ase-size allot ase-init ;] in-stack-check-section ;

: stack-check-;-hook ( -- )
    cr ." at ;: " colon-ase .ase defers ;-hook ;
    

true [if] \ test
    `prim-stack-check is prim-check
    `stack-check-:-hook is :-hook
    `stack-check-;-hook is ;-hook

    : foo r> >r f@ ;
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