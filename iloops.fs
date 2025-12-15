\ multi-line loops in the interpeter

\ Author: Bernd Paysan
\ Copyright (C) 2018,2019,2023 Free Software Foundation, Inc.

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
\ Structs for interpreter                              28nov92py

require string.fs

[IFUNDEF] (i)
    user (i)
[THEN]

$10 stack: input-stack
$20 stack: loop-indices

: >input ( -- )
    { | w^ in } save-input in set-stack in @ input-stack >stack ;
: input-drop ( -- )
    input-stack stack> ?dup-IF  { w^ in } in $free  THEN ;
: input< ( -- )
    input-stack $[]# ?dup-IF
	1- input-stack $[] get-stack restore-input -12 and throw
    THEN ;

: [DO]  ( n-limit n-index -- ) \ gforth bracket-do
    (i) @ loop-indices >stack (i) ! loop-indices >stack >input
; immediate

: [?DO] ( n-limit n-index -- ) \ gforth bracket-question-do
    2dup = IF 2drop postpone [ELSE] ELSE postpone [DO] THEN ;
                                                      immediate

: [+LOOP] ( n -- ) \ gforth bracket-question-plus-loop
    loop-indices stack> >r (i) @ >r
    AHEAD  DO
	input<  r> (i) ! r> loop-indices >stack  EXIT
    BUT  THEN  +LOOP
loop-indices stack> (i) !  input-drop
;                                immediate

: [LOOP] ( -- ) \ gforth bracket-loop
  1 postpone [+LOOP] ;                                 immediate

: [FOR] ( n -- ) \ gforth bracket-for
  0 swap postpone [DO] ;                              immediate

: [NEXT] ( n -- ) \ gforth bracket-next
  -1 postpone [+LOOP] ;                               immediate

: INT-[I] ( -- n ) \ gforth int-bracket-i
    \G Push the loop index of the @code{[do]} iteration at
    \G text-interpretation time.
    (i) @ ; immediate

: comp-[i] ( run-time -- n )
    postpone int-[i] postpone literal ;

' int-[i] ' comp-[i] interpret/compile: [I] ( run-time -- n ) \ gforth bracket-i
\G At run-time, @code{[I]} pushes the loop index of the
\G text-interpretation-time @code{[do]} iteration.  If you want to
\G process the index at text-interpretation time, use @code{INT-[I]}
\G or perform the interpretation semantics of @code{[I]}.

: [BEGIN] ( -- ) \ gforth bracket-begin
    >input ;                                          immediate

: [UNTIL] ( flag -- ) \ gforth bracket-until
    IF  input-drop  ELSE  input<  THEN ;
                                                      immediate

: [REPEAT]  ( -- ) \ gforth bracket-repeat
    input< ;                                          immediate

' [REPEAT] Alias [AGAIN] ( -- ) \ gforth bracket-again
                                                      immediate

: [WHILE]   ( flag -- ) \ gforth bracket-while
    0= IF   postpone [ELSE] 1 countif +!  THEN ;      immediate
