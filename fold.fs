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
    \G check if can fold by one literals; if so, does so, otherwise
    \G compiles the primitive
    lits# 1 u>= IF  >r lits> r> execute >lits
    ELSE  peephole-compile,  THEN ;

: fold2 ( xt -- )
    \G check if can fold by two literals; if so, does so, otherwise
    \G compiles the primitive
    lits# 2 u>= IF  >r lits> lits> swap r> execute >lits
    ELSE  peephole-compile,  THEN ;

: fold2:2 ( xt -- )
    \G check if can fold by two literals; if so, does so, otherwise
    \G compiles the primitive
    lits# 2 u>= IF  >r lits> lits> swap r> execute swap >lits >lits
    ELSE  peephole-compile,  THEN ;

: fold1: ( "name" -- )
    ' dup (make-latest) ['] fold1 set-compiler vt, ;
: fold2: ( "name" -- )
    ' dup (make-latest) ['] fold2 set-compiler vt, ;
: fold2:2: ( "name" -- )
    ' dup (make-latest) ['] fold2:2 set-compiler vt, ;

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

fold2:2: m*
fold2:2: um*
fold2:2: /mod
