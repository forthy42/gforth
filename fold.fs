\ Constant folding for some primitives

\ Copyright (C) 2019 Free Software Foundation, Inc.

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

: fold1 ( xt -- )
    \G check if can fold one literal;
    \G if so, does so, otherwise compiles the
    \G primitive
    lits# 1 u>= IF >r lits> r> execute >lits
    ELSE peephole-compile, THEN ;

: 2lits> ( -- d )  lits> lits> swap ;
: >2lits ( d -- )  swap >lits >lits ;
: 3lits> ( -- t )  2lits> lits> -rot ;
: >3lits ( -- t )  rot >lits >2lits ;

: fold2 ( xt -- )
    \G check if can fold two literals;
    \G if so, does so, otherwise
    \G compiles the primitive
    lits# 2 u>= IF
	>r 2lits> r> execute >lits
    ELSE  peephole-compile,  THEN ;

: fold1:2 ( xt -- )
    \G check if can fold one literal;
    \G if so, does so (returning 2), otherwise
    \G compiles the primitive
    lits# 1 u>= IF
	>r lits> r> execute >2lits
    ELSE peephole-compile, THEN ;

: fold2:2 ( xt -- )
    \G check if can fold two literals;
    \G if so, does so (returning 2),
    \G otherwise compiles the primitive
    lits# 2 u>= IF
	>r 2lits> r> execute >2lits
    ELSE peephole-compile, THEN ;

: fold2:3 ( xt -- )
    \G check if can fold two literals;
    \G if so, does so (returning 3), otherwise
    \G compiles the primitive
    lits# 2 u>= IF
	>r 2lits> r> execute >3lits
    ELSE peephole-compile, THEN ;

: fold3:3 ( xt -- )
    \G check if can fold three literals;
    \G if so, does so (returning 3),
    \G otherwise compiles the primitive
    lits# 3 u>= IF
	>r 3lits> r> execute >3lits
    ELSE peephole-compile, THEN ;

: fold3:2 ( xt -- )
    \G check if can fold three literals;
    \G if so, does so (returning 2),
    \G otherwise compiles the primitive
    lits# 3 u>= IF
	>r 3lits> r> execute >2lits
    ELSE peephole-compile, THEN ;

: fold3:1 ( xt -- )
    \G check if can fold three literals;
    \G if so, does so (returning 1),
    \G otherwise compiles the primitive
    lits# 3 u>= IF
	>r 3lits> r> execute >lits
    ELSE peephole-compile, THEN ;

: folder ( xt "name" -- )
    create , does> vt,
    ' dup (make-latest) @ set-optimizer ;

' fold1 folder fold1:
' fold2 folder fold2:
' fold1:2 folder fold1:2:
' fold2:2 folder fold2:2:
' fold2:3 folder fold2:3:
' fold3:3 folder fold3:3:
' fold3:2 folder fold3:2:
' fold3:1 folder fold3:1:

fold1: invert
fold1: abs
fold1: negate
fold1: >pow2
fold1: w><
fold1: l><
fold1: x><
fold1: 1+
fold1: 1-
fold1: 2*
fold1: 2/
fold1: cells
fold1: cell/
fold1: wcwidth
fold1: floats
fold1: sfloats
fold1: dfloats
fold1: float/
fold1: sfloat/
fold1: dfloat/

fold2: +
fold2: -
fold2: *
fold2: /
fold2: mod
fold2: u/
fold2: umod
fold2: and
fold2: or
fold2: xor
fold2: min
fold2: max
fold2: umin
fold2: umax
fold2: rshift
fold2: lshift
fold2: arshift
fold2: rol
fold2: ror
fold2: =
fold2: >
fold2: >=
fold2: <
fold2: <=
fold2: u>
fold2: u>=
fold2: u<
fold2: u<=
fold2: drop
fold2: nip

fold1:2: dup

fold2:2: m*
fold2:2: um*
fold2:2: /mod
fold2:2: swap

fold2:3: over
fold2:3: tuck

fold3:3: rot
fold3:3: -rot

fold3:2: um/mod
fold3:2: fm/mod
fold3:2: sm/rem
fold3:2: */mod

fold3:1: */
