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
constant sd-size

0
field: anchor-parent \ a root references itself
sd-size chars +field anchor-offsets \ offsets from immediate parent
sd-size stacks * +field anchor-effects
constant anchor-size

: do-one-stack-effect {: sd1 sd2 -- :}
    \ given a one-stack effect sd1, change it to be the one-stack
    \ effect of sd1 followed by sd2.
    sd1 sd-out c@ sd2 sd-in c@ - dup 0< if
	negate dup sd1 sd-in c+!
	0 then
    sd2 sd-out c@ + sd1 sd-out c! ;

: do-stack-effect ( sds1 sds2 -- )
    \ given a stack effect sds1, change it to be the stack effect of
    \ sds1 followed by sds2.
    stacks 0 ?do
	2dup do-one-stack-effect
	sd-size + swap sd-size + swap loop
    2drop ;

wordlist constant prim-stack-effects

: current-execute ( ... wordlist xt -- ... )
    get-current >r swap set-current catch r> set-current ;

: stack-effect ( "name" -- )
    ' {: w^ xt :} xt cell next-name
    prim-stack-effects ['] create current-execute
    ['] do-stack-effect set-does> ;

: stack-effect-unknown ( "name" -- )
    stack-effect ;

: .stacks ( a -- )
    \ a is the address of a field of the first sd in a stack effect description
    stacks 0 ?do
	dup c@ 0 ?do
	    stack-letters i + c@ emit loop
	sd-size + loop
    drop ;

: .stack-effects ( se -- )
    dup sd-in .stacks '-' emit sd-out .stacks ;

: anchor-init ( a -- )
    dup anchor-size erase
    dup anchor-parent ! ;

: anchor-root ( a1 -- a2 )
    begin
	dup anchor-parent @ tuck =
    until ;

: .anchor ( a -- )
    


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