\ divspeed.fs
\
\ Measure speed of division words in gforth.
\
\ Krishna Myneni, 2006-10-26;
\ Revisions:
\    2006-10-28  change DIVIDEND for 32-bit systems;
\                add tests for */ and M*/  ae, km
(
 In the development version of gforth, code has been added to test for division
 by zero in the division words. We want to measure the performance penalty
 introduced by these tests. Words to be checked are:

   /
   MOD
   /MOD
   */
   */MOD
   FM/MOD
   SM/REM
   UM/MOD
   M*/
)

: ms@ ( -- u | return time in ms)  cputime d+ 1 1000 m*/ d>s ;
: ?allot ( u -- a ) here swap allot ;
: table ( v1 v2 ... vn n <name> -- | create a table of singles ) 
        create dup cells ?allot over 1- cells + swap
        0 ?do dup >r ! r> 1 cells - loop drop ;

1 31 lshift 1- constant  DIVIDEND
DIVIDEND negate s>d 2constant D_DIVIDEND
10000000     constant  N

\ Some helpful macros
variable xt
s" :noname [ xt @ >name name>string ] 2literal type ms@" 2constant s1
    
: get-xt ( "op" -- ) bl word find 0= ABORT" Unknown word!" xt ! ;

: testA ( "op" -- )
    get-xt s1 evaluate
    s" DIVIDEND  N 1 DO dup I" evaluate xt @ compile,
    s" drop LOOP drop ;" evaluate ; immediate

: testB ( "op" -- )
    get-xt s1 evaluate
    s" DIVIDEND  N 1 DO dup I" evaluate xt @ compile,
    s" 2drop LOOP drop ;" evaluate ; immediate
 
: testC ( "op" -- )
    get-xt s1 evaluate
    s" D_DIVIDEND N 1 DO  2dup I" evaluate xt @ compile,
    s" 2drop LOOP 2drop ;" evaluate ; immediate

: testD ( "op" -- )
    get-xt s1 evaluate
    s" DIVIDEND s>d N 1 DO  2dup I" evaluate xt @ compile,
    s" 2drop LOOP 2drop ;" evaluate ; immediate

: testE ( "op" -- )
    get-xt s1 evaluate
    s" D_DIVIDEND N 1 DO  2dup I" evaluate xt @ compile,
    s" drop LOOP 2drop ;" evaluate ; immediate

: testF ( "op" -- )
    get-xt s1 evaluate
    s" D_DIVIDEND N 1 DO  2dup 1 I" evaluate xt @ compile,
    s" 2drop LOOP 2drop ;" evaluate ; immediate

\ The tests return the start time in ms

testA  /
testA  MOD
testB  /MOD
testE  */
testC  */MOD
testC  FM/MOD
testC  SM/REM
testD  UM/MOD   ( testC causes overflow for UM/MOD )
testF  M*/
9 table tests

: .elapsed ( starttime -- ) ms@ 9 emit swap - 4 .r ."  ms" ;

: run-tests
    cr ." Speed Tests:"
    9 0 DO  cr I cells tests + @ execute .elapsed LOOP cr ;


run-tests
