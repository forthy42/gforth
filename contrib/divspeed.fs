\ divspeed.fs
\
\ Measure speed of division words in gforth.
\
\ Krishna Myneni, 2006-10-26;
\ Revisions:
\    2006-10-28  change DIVIDEND for 32-bit systems;
\                add tests for */ and M*/  ae, km

\ Mostly rewritten by Anton Ertl

: ms@ ( -- u | return time in ms)  cputime d+ 1 1000 m*/ d>s ;
: .elapsed ( starttime -- ) ms@ #tab emit swap - 5 .r ." ms" ;

1 31 lshift 1- constant  DIVIDEND
100000 constant DIVISOR
DIVIDEND negate s>d 2constant D_DIVIDEND
100000000     constant  N
create AB-operands DIVIDEND DIVISOR 2,
create CEF-operand1 D_DIVIDEND 2,
create E1-operand1 DIVIDEND 2 2,
create CDE-operand2 DIVISOR ,
create D-operand1 DIVIDEND s>d 2,
create D1-operand1 DIVIDEND 0 2,
create F-operand2 1 DIVISOR 2,

: '. ( "op" -- xt )
    parse-name 2dup ]] sliteral cr type [[ find-name name>interpret ;
    
: testA ( "op" -- )
    '. >r ]]
    ms@ DIVIDEND  N 1 DO dup I [[ r@ compile, ]] drop LOOP drop .elapsed
    ms@ AB-operands N 1 DO 2@ [[ r@ compile, AB-operands dup 2@ r@ execute - ]] literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testB ( "op" -- )
    '. >r ]]
    ms@ DIVIDEND  N 1 DO dup I [[ r@ compile, ]] 2drop LOOP drop .elapsed
    ms@ AB-operands N 1 DO 2@ [[ r@ compile, AB-operands dup 2@ r@ execute + - ]] + literal + LOOP drop .elapsed
    [[ rdrop ; immediate
 
: testC ( "op" -- )
    '. >r ]]
    ms@ D_DIVIDEND  N 1 DO 2dup I [[ r@ compile, ]] 2drop LOOP 2drop .elapsed
    ms@ CEF-operand1 N 1 DO 2@ CDE-operand2 @ [[ r@ compile, CEF-operand1 dup 2@ CDE-operand2 @ r@ execute + - ]] + literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testC1 ( "op" -- )
    '. >r ]]
    ms@ E1-operand1 2@ N 1 DO 2dup I [[ r@ compile, ]] 2drop LOOP 2drop .elapsed
    ms@ E1-operand1 N 1 DO 2@ CDE-operand2 @ [[ r@ compile, E1-operand1 dup 2@ CDE-operand2 @ r@ execute + - ]] + literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testD ( "op" -- )
    '. >r ]]
    ms@ DIVIDEND s>d N 1 DO 2dup I [[ r@ compile, ]] 2drop LOOP 2drop .elapsed
    ms@ D-operand1 N 1 DO 2@ CDE-operand2 @ [[ r@ compile, D-operand1 dup 2@ CDE-operand2 @ r@ execute + - ]] + literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testD2 ( "op" -- )
    '. >r ]]
    ms@ DIVIDEND s>d N 1 DO 2dup I [[ r@ compile, ]] 2drop drop LOOP 2drop .elapsed
    ms@ D-operand1 N 1 DO 2@ CDE-operand2 @ [[ r@ compile, D-operand1 dup 2@ CDE-operand2 @ r@ execute + + - ]] + + literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testE ( "op" -- )
    '. >r ]]
    ms@ D_DIVIDEND N 1 DO 2dup I [[ r@ compile, ]] drop LOOP 2drop .elapsed
    ms@ CEF-operand1 N 1 DO 2@ CDE-operand2 @ [[ r@ compile, CEF-operand1 dup 2@ CDE-operand2 @ r@ execute - ]] literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testE1 ( "op" -- )
    '. >r ]]
    ms@ E1-operand1 2@ N 1 DO 2dup I [[ r@ compile, ]] drop LOOP 2drop .elapsed
    ms@ E1-operand1 N 1 DO 2@ CDE-operand2 @ [[ r@ compile, E1-operand1 dup 2@ CDE-operand2 @ r@ execute - ]] literal + LOOP drop .elapsed
    [[ rdrop ; immediate

: testF ( "op" -- )
    '. >r ]]
    ms@ D_DIVIDEND  N 1 DO 2dup 1 I [[ r@ compile, ]] 2drop LOOP 2drop .elapsed
    ms@ CEF-operand1 N 1 DO 2@ F-operand2 2@ [[ r@ compile, CEF-operand1 dup 2@ F-operand2 2@ r@ execute + - ]] + literal + LOOP drop .elapsed
    [[ rdrop ; immediate

\ The tests return the start time in ms

: run-tests ( -- )
    cr ." warmup"
    testA  U/
    cr ."         thruput latency"
    \ testA  /
    testA  /S
    testA  /F
    testA  U/
    \ testA  MOD
    testA  MODS
    testA  MODF
    testA  UMOD
    \ testB  /MOD
    testB  /MODS
    testB  /MODF
    testB  U/MOD
    testC  FM/MOD
    testC  SM/REM
    testD  UM/MOD   ( testC causes overflow for UM/MOD )
    testc  DU/MOD
    \ testE  */
    testE  */S
    testE  */F
    testE1  U*/
    \ testC  */MOD
    testC  */MODS
    testC  */MODF
    testC1 U*/MOD
    testD2 UD/MOD
    testF  M*/
    cr ;

run-tests
