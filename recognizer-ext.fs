\ Recognizer extensions

\ Authors: Bernd Paysan
\ Copyright (C) 2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

0 Value translator-offset
#10 cells constant translator-max-offset#
s" No more translator slots free" exception constant translator-overflow

: >translate-method ( xt rectype translate-method -- )
    >body @ >body + ;
fold1: ( xt -- ) >body @ lit, postpone >body postpone + ;

' >translate-method defer-table to-class: translate-method-to

' postponing make-latest
' translate-method-to set-to

Create translate-methods
translator-max-offset# 0 [DO] ' noop , [LOOP]

: translate-method: ( "name" -- ) \ gforth-experimental
    \G create a new translate method, extending the translator table.
    \G You can assign an xt to an existing rectype by using
    \G @var{xt rectype} @code{to} @var{translator}.
    translator-offset translator-max-offset# u>=
    translator-overflow and throw
    ['] postponing create-from reveal
    latestxt translate-methods translator-offset + !
    translator-offset ,  cell +to translator-offset ;

translate-method: interpreting ( ... translator -- ... ) \ gforth-experimental
\G Perform the interpreting action of @i{translator}.  For a
\G system-defined translator, first consume the translator and
\G translator-specific additional stack items and possibly perform
\G additional scanning specified for the translator, then perform the
\G @word{interpreting} run-time specified for the translator.  For a
\G user-defined translator, remove @i{translator} from the stack and
\G execute its @i{int-xt}.

translate-method: compiling ( ... translator -- ... ) \ gforth-experimental
\G Perform the compiling action of @i{translator}.  For a
\G system-defined translator, first consume the translator and
\G translator-specific additional stack items and possibly perform
\G additional scanning specified for the translator, then perform the
\G @word{compiling} run-time specified for the translator, or, if none
\G is specified, compile the @word{interpreting} run-time.  For a
\G user-defined translator, remove @i{translator} from the stack and
\G execute its @i{comp-xt}.

\ we already have defined this in the kernel
\ translate-method: postponing ( ... translator -- ) \ gforth-experimental
' postponing translate-methods translator-offset + !
cell +to translator-offset

: set-state ( xt -- ) \ gforth-experimental
    \G change the current state of the system so that executing
    \G a translator matches the translate-method passed as @var{xt}
    dup >does-code [ ' postponing >does-code ] Literal <> #-12 and throw
    >body @ cell/ negate state ! ;
opt: lits# 1 u>= IF
	lits> dup >does-code [ ' postponing >does-code ] Literal = IF
	    >body @ cell/ negate lit, postpone state postpone !  drop  EXIT
	ELSE  #-12 throw  THEN
    THEN  :, ;

: get-state ( -- xt ) \ gforth-experimental
    \G return the currently used translate-method @var{xt}
    state @ abs cells translate-methods + @ ;
