\ recognizer-based interpreter                       05oct2011py

\ Copyright (C) 2012,2013,2014,2015,2016 Free Software Foundation, Inc.

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
\ ( addr u -- token table | addr u rectype-null )
\ where the token is the result of the parsing action (can be more than
\ one stack or live on other stacks, e.g. on the FP stack)
\ and the table contains three actions (as array of three xts):
\ interpret it, compile it, compile it as literal.

:noname  no.extensions ;
' no.extensions dup >vtable
AConstant rectype-null
\G If a recognizer fails, it returns @code{rectype-null}

: lit, ( n -- ) postpone Literal ;

' name?int alias rectype>int
' name>comp alias rectype>comp
: rectype>post ( r:table -- xt ) >namevt @ >vtlit, @ ;

: do-lit, ( .. xt -- .. ) rectype>post execute ;
: >postpone ( token table -- )
    dup >r do-lit, r> post, ;

: rec-word ( addr u -- xt | rectype-null )
    \G Searches a word in the wordlist stack
    find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ]
    dup 0= IF  drop rectype-null  THEN ;

:noname ( n -- n ) ;
' do-lit, set-optimizer
lit,: ( n -- ) postpone Literal ;
AConstant rectype-num

:noname ( d -- d ) ;
' do-lit, set-optimizer
lit,: ( d -- ) postpone 2Literal ;
AConstant rectype-dnum

\ snumber? should be implemented as recognizer stack

: rec-num ( addr u -- n/d table | rectype-null )
    \G converts a number to a single/double integer
    snumber?  dup
    IF
	0> IF  rectype-dnum   ELSE  rectype-num  THEN  EXIT
    THEN
    drop rectype-null ;

\ generic stack get/set

: get-stack ( stack -- x1 .. xn n )
    \G fetch everything from the generic stack to the data stack
    $@ dup cell/ >r bounds ?DO  I @  cell +LOOP  r> ;
: set-stack ( x1 .. xn n stack -- )
    \G set the generic stack with values from the data stack
    >r cells r@ $!len
    r> $@ bounds cell- swap cell- -DO  I !  cell -LOOP ;

: stack: ( n "name" -- )
    \G create a named stack with at least @var{n} cells space
    drop $Variable ;
: stack ( n -- addr )
    \G create an unnamed stack with at least @var{n} cells space
    drop align here 0 , ;

: >stack ( x stack -- )
    \G push to top of stack
    >r r@ $@len cell+ r@ $!len
    r> $@ + cell- ! ;
: stack> ( stack -- x )
    \G pop from top of stack
    >r r@ $@ ?dup IF  + cell- @ r@ $@len cell- r> $!len
    ELSE  drop rdrop  THEN ;

$Variable default-recognizer
\G The system recognizer

default-recognizer AValue forth-recognizer

: get-recognizers ( -- xt1 .. xtn n )
    \G push the content on the recognizer stack
    forth-recognizer get-stack ;
: set-recognizers ( xt1 .. xtn n )
    \G set the recognizer stack from content on the stack
    forth-recognizer set-stack ;

\ recognizer loop

Defer trace-recognizer  ' drop is trace-recognizer

: recognize ( addr u rec-addr -- tokens table )
    \G apply a recognizer stack to a string, delivering a token
    $@ bounds cell- swap cell- U-DO
	2dup I -rot 2>r
	perform dup rectype-null <>  IF
	    2rdrop I @ trace-recognizer  UNLOOP  EXIT  THEN  drop
	2r>
    cell -LOOP
    2drop rectype-null ;

\ nested recognizer helper

\ : nest-recognizer ( addr u -- token table | rectype-null )
\   xxx-recognizer recognize ;

: interpreter-r ( addr u -- ... xt )
    forth-recognizer recognize name?int ;

' interpreter-r IS parser1

: compiler-r ( addr u -- ... xt )
    forth-recognizer recognize name>comp ;

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter-r  IS parser1 state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler-r     IS parser1 state on  ;

: postpone ( "name" -- ) \ core
    \g Compiles the compilation semantics of @i{name}.
    parse-name forth-recognizer recognize >postpone
; immediate restrict
