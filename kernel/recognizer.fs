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

: recognizer: ( xt1 xt2 xt3 -- ) Create rot , swap , , ;

(field) r>int      ( r-addr -- addr )  0 cells ,
(field) r>comp     ( r-addr -- addr )  1 cells ,
(field) r>lit      ( r-addr -- addr )  2 cells ,

' no.extensions dup dup Create r:fail A, A, A,

: lit, ( n -- ) postpone Literal ;
: nt, ( nt -- ) name>comp execute ;
: nt-ex ( nt -- )
    [ cell 1 floats - dup [IF] ] lp+!# [ dup , [THEN] drop ]
    r> >l name>int execute @local0 >r lp+ ;

' nt-ex
' nt,
' lit,
Create r:word rot A, swap A, A,

: word-recognizer ( addr u -- nt r:word | addr u r:fail )
    2dup find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ] dup
    IF  nip nip r:word  ELSE  drop r:fail  THEN ;

' noop
' lit,
dup
Create r:num rot A, swap A, A,

' noop
:noname ( n -- ) postpone 2Literal ;
dup
Create r:2num rot A, swap A, A,

\ snumber? should be implemented as recognizer stack

: num-recognizer ( addr u -- n/d table | addr u r:fail )
    2dup 2>r snumber?  dup
    IF
	2rdrop 0> IF  r:2num   ELSE  r:num  THEN  EXIT
    THEN
    drop 2r> r:fail ;

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

: do-recognizer ( addr u rec-addr -- token table )
    dup cell+ swap @ cells bounds ?DO
	I perform dup r:fail <>  IF  UNLOOP  EXIT  THEN  drop
    cell +LOOP
    r:fail ;

\ nested recognizer helper

\ : nest-recognizer ( addr u -- token table | addr u r:fail )
\   xxx-recognizer do-recognizer ;

: interpreter-r ( addr u -- ... xt )
    forth-recognizer do-recognizer r>int @ ;

' interpreter-r IS parser1

: compiler-r ( addr u -- ... xt )
    forth-recognizer do-recognizer r>comp @ ;

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter-r  IS parser1 state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler-r     IS parser1 state on  ;

: >int      ( token table -- )  r>int perform ;
: >comp     ( token table -- )  r>comp perform ;
: >postpone ( token table -- )
    >r r@ r>lit perform r> r>comp @ compile, ;

: postpone ( "name" -- ) \ core
    \g Compiles the compilation semantics of @i{name}.
    parse-name forth-recognizer do-recognizer >postpone
; immediate restrict

