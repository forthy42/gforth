\ recognizer-based interpreter                       05oct2011py

\ Copyright (C) 2012,2013 Free Software Foundation, Inc.

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

: (r:fail)  no.extensions ;
' no.extensions dup >vtable
' (r:fail) AConstant r:fail
\G If a recognizer fails, it returns @code{r:fail}

: lit, ( n -- ) postpone Literal ;

: >postpone ( token table -- )
    dup name>comp drop  >namevt @ >vtpostpone perform ;

: rec:word ( addr u -- xt | r:fail )
    \G Searches a word in the wordlist stack
    find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ]
    dup 0= IF  drop r:fail  THEN ;

:noname ( n -- n ) ;
comp: ( n xt -- ) drop postpone Literal ;
post: ( n xt -- ) >r postpone Literal r> post, ;
AConstant r:num

:noname ( d -- d ) ;
comp: ( d xt -- ) drop postpone 2Literal ;
post: ( d xt -- ) >r postpone 2Literal r> post, ;
AConstant r:dnum

\ snumber? should be implemented as recognizer stack

: rec:num ( addr u -- n/d table | r:fail )
    \G converts a number to a single/double integer
    snumber?  dup
    IF
	0> IF  r:dnum   ELSE  r:num  THEN  EXIT
    THEN
    drop r:fail ;

\ generic stack get/set

$10 Constant max-stack#

: get-stack ( rec-addr -- xt1 .. xtn n )
    dup swap @ dup >r cells bounds swap ?DO
	I @
    cell -LOOP  r> ;

: set-stack ( xt1 .. xtn n rec-addr -- )
    over max-stack# u>= abort" Too many items"
    2dup ! cell+ swap cells bounds ?DO
	I !
    cell +LOOP ;

Variable forth-recognizer
\G The system recognizer

' rec:word A, ' rec:num A, max-stack# 2 - cells allot
2 forth-recognizer !
\ ' num-recognizer ' word-recognizer 2 forth-recognizer set-recognizers

: get-recognizers ( -- xt1 .. xtn n )
    \G push the content on the recognizer stack
    forth-recognizer get-stack ;
: set-recognizers ( xt1 .. xtn n )
    \G set the recognizer stack from content on the stack
    forth-recognizer set-stack ;

\ recognizer loop

: map-recognizer ( addr u rec-addr -- tokens table )
    \G apply a recognizer stack to a string, delivering a token
    dup cell+ swap @ cells bounds ?DO
	2dup I -rot 2>r
	perform dup r:fail <>  IF  2rdrop UNLOOP  EXIT  THEN  drop
	2r>
    cell +LOOP
    2drop r:fail ;

: do-recognizer ( addr u -- tokens xt )
    \G process the string @var{addr u} in the recognizer stack
    forth-recognizer map-recognizer ;

\ nested recognizer helper

\ : nest-recognizer ( addr u -- token table | r:fail )
\   xxx-recognizer do-recognizer ;

: interpreter-r ( addr u -- ... xt )
    do-recognizer name>int ;

' interpreter-r IS parser1

: compiler-r ( addr u -- ... xt )
    do-recognizer name>comp ;

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter-r  IS parser1 state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler-r     IS parser1 state on  ;

: postpone ( "name" -- ) \ core
    \g Compiles the compilation semantics of @i{name}.
    parse-name do-recognizer >postpone
; immediate restrict
