\ Deferred interpretation

\ Authors: Bernd Paysan
\ Copyright (C) 2026 Free Software Foundation, Inc.

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

translate-method: deferring ( ... translation -- ... ) \ gforth-experimental
\G compiling words found in quasi interpretation state into a temporary buffer

obsolete-mask 2/ Constant non-deferring-mask \ gforth-experimental
\G a non-deferring word breaks deferred compilation

: non-deferring ( -- ) \ gforth-experimental
    \G Mark the last word as non-deferring.
    \G Note that only non-immediate non-deferring words need this marker
    non-deferring-mask lastflags or! ;

: non-deferring-words ( "name1" .. "namex" -- ) \ gforth-experimental
    \G mark existing words as non-deferring
    BEGIN  parse-name dup WHILE
	    find-name ?dup-IF  make-latest non-deferring  THEN
    REPEAT 2drop ; non-deferring

\ non-deferring words are words that either parse or change the state

[: >voc dup xt? IF  make-latest non-deferring  ELSE  drop  THEN ;] map-vocs

non-deferring-words set-order also only previous vocabulary
non-deferring-words char : :noname ' create variable constant value create-from noname-from [:
non-deferring-words 2variable 2value 2constant fvariable fvalue fconstant
non-deferring-words varue fvarue 2varue
non-deferring-words +field begin-structure cfield: wfield: lfield: xfield: field: 2field: ffield: sffield: dffield:
non-deferring-words value: cvalue: wvalue: lvalue: scvalue: swvalue: slvalue: 2value:
non-deferring-words fvalue: sfvalue: dfvalue: zvalue: $value: defer: value[]: $value[]:
non-deferring-words timer: see locate where synonym alias marker cold bye
non-deferring-words [IF] [ELSE] [defined] [undefined] [IFDEF] [IFUNDEF] ] parse parse-name
non-deferring-words binary decimal hex interpret evaluate

$1000 buffer: one-shot-dict
Variable one-shot-dp
one-shot-dict one-shot-dp !

: one-shot-interpret ( ... xt -- )
    dp @ >r  one-shot-dp @ dp !  catch
    dp @ one-shot-dp !  r> dp !  throw ;

: defer-start ( -- ) \ gforth-experimental
    \G start new deferred line
    get-state `interpreting = IF
	[: one-shot-dict one-shot-dp ! :noname ;] one-shot-interpret
	`deferring set-state
    THEN ;
: defer-finish ( -- ) \ gforth-experimental
    \G finish the current deferred execution buffer and execute it
    get-state `deferring = IF
	[: ]] exit ; [[ ;] one-shot-interpret execute
    THEN ;

: comp2defer ( translator -- )
    dup action-of compiling >r
    :noname r> lit, ]] one-shot-interpret ; [[
    swap is deferring ;

translate-cell        comp2defer
translate-dcell       comp2defer
translate-float       comp2defer
scan-translate-string comp2defer
translate-to          comp2defer
translate-complex     comp2defer
translate-env         comp2defer
:noname ( ... nt -- .. )
    dup >r ?obsolete  name>compile
    r> non-deferring-mask mask? IF
	drop `compile, one-shot-interpret after-line before-line
    ELSE
	one-shot-interpret
    THEN ;
translate-name is deferring

: deferring-forth ( -- ) \ gforth-experimental
    \G enable deferred interpretation
    ['] defer-start is before-line
    ['] defer-finish is after-line ;

: interpreting-forth ( -- ) \ gforth-experimental
    ['] noop  dup is before-line is after-line ; non-deferring immediate

deferring-forth \ default on
