\ Recognizer extensions

\ Authors: Bernd Paysan
\ Copyright (C) 2020,2021,2022 Free Software Foundation, Inc.

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
"No more translator slots free" exception constant translator-overflow

: >translator ( xt rectype translator -- )
    >body @ >body + ;
to-opt: ( xt -- ) >body @ lit, ]] >body + [[ ;

' >translator defer-table to-method: translator-to

: translator: ( "name" -- ) \ gforth-experimental
    \G create a new translator, extending the translator table.
    \G You can assign an xt to an existing rectype by using
    \G @var{xt rectype} @code{to} @var{translator}.
    translator-offset translator-max-offset# u>=
    translator-overflow and throw
    Create translator-offset ,  cell +to translator-offset
    [: @ + @ execute-;s ;] set-does>
    [:  lits# 0= IF  does,  EXIT  THEN
	lits> dup >does-code ['] do-rec = IF
	    swap @ + @ compile,
	ELSE  >lits does,  THEN ;] set-optimizer
    ['] translator-to set-to ;

translator: >interpret ( translator -- ) \ gforth-experimental
\G perform interpreter action of translator
translator: >compile ( translator -- ) \ gforth-experimental
\G perform compile action of translator
0 warnings !@ \ we already have this, but this version is better
translator: >postpone ( translator -- ) \ gforth-experimental
\G perform postpone action of translator
warnings !

: translate>state ( xt -- )
    >body @ cell/ negate state ! ;
