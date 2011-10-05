\ recognizer-based interpreter                       05oct2011py

\ Recognizer are words that take a string and try to figure out
\ what to do with it.  I want to separate the parse action from
\ the interpret/compile/postpone action, so that recognizers
\ are more general than just be used for the interpreter.

\ The "design pattern" used here is the *factory*, even though
\ the recognizer does not return a full-blown object.
\ A recognizer has the stack effect
\ ( addr u -- token table true | addr u false )
\ where the token is the result of the parsing action (can be more than
\ one stack or live on other stacks, e.g. on the FP stack)
\ and the table contains for actions (as array of four xts:
\ interpret it, compile interpretation semantics
\ compile it, compile it as literal.

: recognizer: ( xt1 xt2 xt3 xt4 -- ) Create 2swap swap 2, swap 2, ;

(field) r>int     ( r-addr -- addr )  0 cells ,
(field) r>compint ( r-addr -- )       1 cells ,
(field) r>comp    ( r-addr -- )       2 cells ,
(field) r>lit     ( r-addr -- )       3 cells ,

:noname ( ... nt -- ) name>int execute ;
:noname ( ... nt -- ) name>int compile, ;
:noname ( ... nt -- ) name>comp execute ;
:noname ( ... nt -- ) postpone Literal ;
recognizer: r:int-table

:noname ( addr u -- nt int-table true | addr u false )
    2dup find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ] dup
    IF
	nip nip r:int-table true  EXIT
    THEN ; Constant int-recognizer

' noop
:noname  postpone Literal ;
dup
dup
recognizer: r:number

' noop
:noname  postpone 2Literal ;
dup
dup
recognizer: r:2number

:noname ( addr u -- nt int-table true | addr u false )
    2dup 2>r snumber?  dup
    IF
	2rdrop 0> IF  r:2number   ELSE  r:number  THEN  true  EXIT
    THEN
    drop 2r> false ; Constant num-recognizer

' no.extensions dup 2dup recognizer: r:fail

\ recognizer stack

$10 Constant max-rec#
Variable forth-recognizer max-rec# cells allot

: get-recognizers ( rec-addr -- xt1 .. xtn n )
    dup cell+ swap @ dup >r cells bounds ?DO
	I @
    cell +LOOP  r> ;

: set-recognizers ( xt1 .. xtn n rec-addr -- )
    over max-rec# u>= abort" Too many recognizers"
    2dup ! swap cells bounds swap ?DO
	I !
    cell -LOOP ;

num-recognizer int-recognizer 2 forth-recognizer set-recognizers

\ recognizer loop

: do-recognizer ( addr u rec-addr -- token table )
    dup cell+ swap @ cells bounds ?DO
	I perform IF  UNLOOP  EXIT  THEN
    cell +LOOP
    r:fail ;

: interpreter-r ( addr u -- ... xt )
    forth-recognizer do-recognizer r>int @ ;

: compiler-r ( addr u -- ... xt )
    forth-recognizer do-recognizer r>comp @ ;

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter-r  IS parser1 state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler-r     IS parser1 state on  ;

