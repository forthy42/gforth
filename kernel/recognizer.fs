\ recognizer-based interpreter                       05oct2011py

\ Copyright (C) 2012,2013,2014,2015 Free Software Foundation, Inc.

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

' name?int alias r>int
' name>comp alias r>comp
: r>post ( r:table -- xt ) >namevt @ >vtlit, @ ;

: do-lit, ( .. xt -- .. ) r>post execute ;
: >postpone ( token table -- )
    dup >r name>comp drop do-lit, r> post, ;

: rec:word ( addr u -- xt | r:fail )
    \G Searches a word in the wordlist stack
    find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ]
    dup 0= IF  drop r:fail  THEN ;

:noname ( n -- n ) ;
' do-lit, set-compiler
lit,: ( n -- ) postpone Literal ;
AConstant r:num

:noname ( d -- d ) ;
' do-lit, set-compiler
lit,: ( d -- ) postpone 2Literal ;
AConstant r:dnum

\ snumber? should be implemented as recognizer stack

: rec:num ( addr u -- n/d table | r:fail )
    \G converts a number to a single/double integer
    snumber?  dup
    IF
	0> IF  r:dnum   ELSE  r:num  THEN  EXIT
    THEN
    drop r:fail ;

\ generic deque get/set

: deque@ ( deque -- x1 .. xn n )
    \G fetch everything from the generic deque to the data stack
    $@ dup cell/ >r bounds ?DO  I @  cell +LOOP  r> ;
: deque! ( x1 .. xn n deque -- )
    \G set the generic deque with values from the data stack
    >r cells r@ $!len
    r> $@ bounds cell- swap cell- -DO  I !  cell -LOOP ;

: deque: ( n "name" -- )
    \G create a named deque with at least @var{n} cells space
    drop Variable ;

: >deque ( x deque -- )
    \G push to top of deque
    >r r@ $@len cell+ r@ $!len
    r> $@ + cell- ! ;
: deque> ( deque -- x )
    \G pop from top of deque
    >r r@ $@ ?dup IF  + cell- @ r@ $@len cell- r> $!len
    ELSE  drop rdrop  THEN ;

AVariable default-recognizer
\G The system recognizer

here default-recognizer !
2 cells , ' rec:num A, ' rec:word A,

default-recognizer AValue forth-recognizer

: get-recognizers ( -- xt1 .. xtn n )
    \G push the content on the recognizer stack
    forth-recognizer deque@ ;
: set-recognizers ( xt1 .. xtn n )
    \G set the recognizer stack from content on the stack
    forth-recognizer deque! ;

\ recognizer loop

: map-recognizer ( addr u rec-addr -- tokens table )
    \G apply a recognizer stack to a string, delivering a token
    $@ bounds cell- swap cell- -DO
	2dup I -rot 2>r
	perform dup r:fail <>  IF  2rdrop UNLOOP  EXIT  THEN  drop
	2r>
    cell -LOOP
    2drop r:fail ;

: do-recognizer ( addr u -- tokens xt )
    \G process the string @var{addr u} in the recognizer stack
    forth-recognizer map-recognizer ;

\ nested recognizer helper

\ : nest-recognizer ( addr u -- token table | r:fail )
\   xxx-recognizer do-recognizer ;

: interpreter-r ( addr u -- ... xt )
    do-recognizer name?int ;

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
