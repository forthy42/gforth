\ recognizer-based interpreter                       05oct2011py

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

' no.extensions dup dup rectype: rectype-null
\G If a recognizer fails, it returns @code{rectype-null}

: lit, ( n -- ) postpone Literal ;
: 2lit, ( n -- ) postpone 2literal ;

: rectype>int  ( rectype -- xt ) @ ;
: rectype>comp ( rectype -- xt ) cell+ @ ;
: rectype>post ( rectype -- xt ) cell+ cell+ @ ;

defer >postpone-replacer ( ... rectype1 -- ... rectype2 )
\ may replace recognizer result for postponing (used for postponing locals)
' noop is >postpone-replacer

: >postpone ( ... rectype -- )
    >postpone-replacer dup >r rectype>post execute r> rectype>comp compile, ;

: name-compsem ( ... nt -- ... )
    \ perform compilation semantics of nt
    name>comp execute-;s ;

:noname name?int  execute-;s ;
' name-compsem
' lit,
rectype: rectype-nt ( takes nt, i.e. result of find-name and find-name-in )

: rec-nt ( addr u -- nt rectype-name | rectype-null )
    \G Searches a word in the wordlist stack
    find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ]
    dup IF  rectype-nt  ELSE  drop rectype-null  THEN ;

' noop
' lit,
dup
rectype: rectype-num

' noop
' 2lit,
dup
rectype: rectype-dnum

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
: do-stack: ( x1 .. xn n xt "name" -- )
    >r dup stack: r> set-does> latest >body set-stack ;
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
: stack# ( stack -- elements )
    $@len cell/ ;

\ recognizer loop

Defer trace-recognizer  ' drop is trace-recognizer

: recognize ( addr u rec-addr -- ... rectype )
    \G apply a recognizer stack to a string, delivering a token
    $@ bounds cell- swap cell- U-DO
	2dup I -rot 2>r
	['] trace-recognizer defer@ >r  ['] drop is trace-recognizer
	perform  r> is trace-recognizer \ no tracing on recursive invocation
	dup rectype-null <>  IF
	    2rdrop I @ trace-recognizer  UNLOOP  EXIT  THEN  drop
	2r>
    cell -LOOP
    2drop rectype-null ;

: rec-sequence: ( x1 .. xn n "name" -- )
    ['] recognize do-stack: ;

$Variable default-recognizer
\G The system recognizer

default-recognizer AValue forth-recognizer

: get-recognizers ( -- xt1 .. xtn n )
    \G push the content on the recognizer stack
    forth-recognizer get-stack ;
: set-recognizers ( xt1 .. xtn n )
    \G set the recognizer stack from content on the stack
    forth-recognizer set-stack ;

\ nested recognizer helper

\ : nest-recognizer ( addr u -- token table | rectype-null )
\   xxx-recognizer recognize ;

: interpreter-r ( addr u -- ... xt )
    forth-recognizer recognize rectype>int ;

' interpreter-r IS parser1

: compiler-r ( addr u -- ... xt )
    forth-recognizer recognize rectype>comp ;

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
