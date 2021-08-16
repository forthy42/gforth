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

3 constant stacks \ 0: data, 1: return, 2: fp

0
cfield: sd-in  \ number of stack items of a stack consumed by word or sequence
cfield: sd-out \ number of stack items of a stack produced by word or sequence
constant sd-size

0
field: anchor-parent \ a root references itself
sd-size chars +field anchor-offsets \ offsets from immediate parent
sd-size stacks * +field anchor-stacks
constant anchor-size

: anchor-init ( a -- )
    dup anchor-size erase
    dup anchor-parent ! ;

: anchor-root ( a1 -- a2 )
    begin
	dup anchor-parent @ tuck =
    until ;

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