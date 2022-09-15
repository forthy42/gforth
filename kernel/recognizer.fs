\ recognizer-based interpreter                       05oct2011py

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019,2020,2021 Free Software Foundation, Inc.

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
\ ( addr u -- token table | addr u notfound )
\ where the token is the result of the parsing action (can be more than
\ one stack or live on other stacks, e.g. on the FP stack)
\ and the table contains three actions (as array of three xts):
\ interpret it, compile it, compile it as literal.

: lit, ( n -- ) postpone Literal ;
: 2lit, ( n -- ) postpone 2literal ;

: do-rec ( rectype -- ) state @ abs cells + @ execute-;s ;
: translate: ( int-xt comp-xt post-xt "name" -- )
    \G create a new recognizer table.  Items are in order of
    \G @var{STATE} value, which are 0 or negative.  Up to 7 slots
    \G are available for extensions.
    Create swap rot , , , 7 0 DO  ['] no.extensions ,  LOOP
    ['] do-rec set-does> ;

: >postpone ( ... rectype -- )
    2 cells + @ execute-;s ;

: name-compsem ( ... nt -- ... )
    \ perform compilation semantics of nt
    name>comp execute-;s ;

forth-wordlist is rec-nt
:noname ['] rec-nt >body ; is context

:noname name?int  execute-;s ;
' name-compsem
:noname  lit, postpone name-compsem ;
translate: translate-nt ( takes nt, i.e. result of find-name and find-name-in )

' noop
' lit,
:noname lit, postpone lit, ;
translate: translate-num

' noop
' 2lit,
:noname 2lit, postpone 2lit, ;
translate: translate-dnum

: translate-nt? ( token -- flag )
    \G check if name token; postpone action may differ
    >body 2@ ['] translate-nt >body 2@ d= ;
: nt>rec ( nt / 0 -- nt translate-nt / notfound )
    dup IF  dup where, ['] translate-nt  ELSE  drop ['] notfound  THEN ;

\ snumber? should be implemented as recognizer stack

: rec-num ( addr u -- n/d table | notfound ) \ gforth-experimental
    \G converts a number to a single/double integer
    snumber?  dup
    IF
	0> IF  ['] translate-dnum  ELSE  ['] translate-num  THEN  EXIT
    THEN
    drop ['] notfound ;

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
    dup >r $@len cell+ r@ $!len
    r> $@ + cell- ! ;
: stack> ( stack -- x )
    \G pop from top of stack
    dup >r $@ ?dup-IF  + cell- @ r@ $@len cell- r> $!len
    ELSE  drop rdrop  THEN ;
: stack# ( stack -- elements )
    $@len cell/ ;

\ recognizer loop

Defer trace-recognizer  ' drop is trace-recognizer

Variable rec-level

: recognize ( addr u rec-addr -- ... rectype ) \ gforth-experimental
    \G apply a recognizer stack to a string, delivering a token
    1 rec-level +!
    $@ bounds cell- swap cell- U-DO
	2dup I -rot 2>r  perform
	dup ['] notfound <>  IF
	    -1 rec-level +!
	    2rdrop I @ trace-recognizer  UNLOOP  EXIT  THEN  drop
	2r>
	cell [ 2 cells ] Literal I cell- 2@ <> select \ skip double entries
	\ note that we search first and then skip, because the first search
	\ has a very likely hit.  So doubles will be skipped, tripples not
    -loop
    -1 rec-level +!
    2drop ['] notfound ;

: recognizer-sequence: ( x1 .. xn n "name" -- ) \ gforth-experimental
    ['] recognize do-stack: ;

$Variable default-recognize
DOES> recognize ;

( ' rec-num ' rec-nt 2 combined-recognizer: default-recognize ) \ see pass.fs
\G The system recognizer
Defer forth-recognize ( c-addr u -- ... translate-xt ) \ recognizer
\G The system recognizer
' default-recognize is forth-recognize
: set-forth-recognize ( xt -- ) \ recognizer
    \G Change the system recognizer
    is forth-recognize ;
:noname drop is forth-recognize ;
: forth-recognizer ( -- xt ) \ gforth-experimental
    \G backward compatible to Matthias Trute recognizer API
    ['] forth-recognize defer@ ;
unlock set-to lock

: forth-parser ( addr u -- ... )
    forth-recognize execute-;s ;

' forth-parser IS parser

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    state on  ;

: postpone ( "name" -- ) \ core
    \g Compiles the compilation semantics of @i{name}.
    parse-name forth-recognize >postpone
; immediate restrict
