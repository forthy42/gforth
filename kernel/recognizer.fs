\ recognizer-based interpreter                       05oct2011py

\ Copyright (C) 2012 Free Software Foundation, Inc.

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

\ Recognizer are words that take a string and try to figure out
\ what to do with it.  I want to separate the parse action from
\ the interpret/compile/postpone action, so that recognizers
\ are more general than just be used for the interpreter.

\ The "design pattern" used here is the *factory*, even though
\ the recognizer does not return a full-blown object.
\ A recognizer has the stack effect
\ ( addr u -- token table | addr u r:fail )
\ where the token is the result of the parsing action (can be more than
\ one stack or live on other stacks, e.g. on the FP stack)
\ and the table contains three actions (as array of three xts):
\ interpret it, compile it, compile it as literal.

: r:fail  no.extensions ;
' no.extensions dup >vtable

: lit, ( n -- ) postpone Literal ;

: >int      ( token table -- )  name>int execute ;
: >postpone ( token table -- )
    dup >namevt @ >vtpostpone perform ;

: word-recognizer ( addr u -- xt | r:fail )
    find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ]
    dup 0= IF  drop ['] r:fail  THEN ;

:noname ( n xt -- ) drop postpone Literal ;
:noname ( n xt -- ) >r postpone Literal r> post, ;
: r:num ;
>vtable

:noname ( d xt -- ) drop postpone 2Literal ;
:noname ( d xt -- ) >r postpone 2Literal r> post, ;
: r:2num ;
>vtable

\ snumber? should be implemented as recognizer stack

: num-recognizer ( addr u -- n/d table | r:fail )
    snumber?  dup
    IF
	0> IF  ['] r:2num   ELSE  ['] r:num  THEN  EXIT
    THEN
    drop ['] r:fail ;

\ recognizer stack

$10 Constant max-rec#

: get-recognizers ( rec-addr -- xt1 .. xtn n )
    dup swap @ dup >r cells bounds swap ?DO
	I @
    cell -LOOP  r> ;

: set-recognizers ( xt1 .. xtn n rec-addr -- )
    over max-rec# u>= abort" Too many recognizers"
    2dup ! cell+ swap cells bounds ?DO
	I !
    cell +LOOP ;

Variable forth-recognizer

' word-recognizer A, ' num-recognizer A, max-rec# 2 - cells allot
2 forth-recognizer !
\ ' num-recognizer ' word-recognizer 2 forth-recognizer set-recognizers

\ recognizer loop

: do-recognizer ( addr u rec-addr -- tokens xt )
    dup cell+ swap @ cells bounds ?DO
	2dup I -rot 2>r
	perform dup ['] r:fail <>  IF  2rdrop UNLOOP  EXIT  THEN  drop
	2r>
    cell +LOOP
    2drop ['] r:fail ;

\ nested recognizer helper

\ : nest-recognizer ( addr u -- token table | r:fail )
\   xxx-recognizer do-recognizer ;

: interpreter-r ( addr u -- ... xt )
    forth-recognizer do-recognizer name>int ;

' interpreter-r IS parser1

: compiler-r ( addr u -- ... xt )
    forth-recognizer do-recognizer name>comp ;

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter-r  IS parser1 state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler-r     IS parser1 state on  ;

: postpone ( "name" -- ) \ core
    \g Compiles the compilation semantics of @i{name}.
    parse-name forth-recognizer do-recognizer >postpone
; immediate restrict
