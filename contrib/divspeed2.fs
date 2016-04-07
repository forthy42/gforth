\ divspeed2.fs
\
\ Measure speed of division words in gforth.
\
\ Krishna Myneni, 2006-10-26;
\ Revisions:
\    2006-10-28  change DIVIDEND for 32-bit systems;
\                add tests for */ and M*/  ae, km
\    2016-04-01  version specific to gforth(-fast) --dynamic that avoids
\		 differences from code alignment.
\		 also introduces dependencies between results
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

: ms@ ( -- u | return time in ms)  cputime 2drop d>s 1000 / ;

1 31 lshift 1- constant  DIVIDEND
DIVIDEND negate s>d 2constant D_DIVIDEND
100000000     constant  N
64 constant aligns
n aligns / constant NA

: align-code { u -- }
    ]] cputime [[ -1 cells allot \ a non-relocatable primitive to force
                                 \ a dispatch after the previous code
    finish-code
    here ]] noop [[ finish-code @ ( addr ) \ get the current native-code-here
    64 naligned u + forget-dyncode 0= abort" forget-dyncode failed"
    -1 cells allot ;
    
\ Some helpful macros
variable xt
s" :noname [ xt @ >name name>string ] 2literal type ms@" 2constant s1

: bench { d: prelude d: kernel d: postlude -- }
    :noname
    ]] noop [[ prelude evaluate
    aligns 0 do
	]] na 1 do [[ i align-code kernel evaluate ]] loop [[
    loop
    postlude evaluate ]] ; [[
    \ [ also see-voc ] dup >body here see-code-range [ previous ]
    ms@ over execute ms@ swap - 4 .r ."  ms  " kernel type cr
    >body @ forget-dyncode ;

." classic divspeed reloaded" cr
s" dividend" s" dup i / drop" s" drop" bench
s" dividend" s" dup i mod drop" s" drop" bench
s" dividend" s" dup i /mod 2drop" s" drop" bench
s" d_dividend" s" 2dup i */ drop" s" 2drop" bench
s" d_dividend" s" 2dup i */mod 2drop" s" 2drop" bench
s" d_dividend" s" 2dup i fm/mod 2drop" s" 2drop" bench
s" d_dividend" s" 2dup i sm/rem 2drop" s" 2drop" bench
s" dividend s>d" s" 2dup i um/mod 2drop" s" 2drop" bench

." influence of dividend" cr
s" " s" 1 i / drop" s" " bench
s" " s" $7fffffffffffffff i / drop" s" " bench
s" " s" -1 i / drop" s" " bench
s" " s" $ffffffffffffffff. i 1+ um/mod 2drop" s" " bench
s" " s" $10000000000000000. i 1+ um/mod 2drop" s" " bench

." dependent divides for latency" cr
s" 10001" s" 100000000 swap /" s" drop" bench
s" 10001" s" -100000000 swap / negate" s" drop" bench
s" 3" s" 10 mod" s" drop" bench
s" -3" s" 10 mod -10 +" s" drop" bench
s" 3" s" 10 /mod +" s" drop" bench
s" -3" s" 10 /mod + -9 +" s" drop" bench
s" 10001" s" 20000 5000 rot */" s" drop" bench
s" 10001" s" -20000 5000 rot */ negate" s" drop" bench
s" 3"  s" 1 10 */mod +" s" drop" bench
s" -3" s" 1 10 */mod + -9 +" s" drop" bench
s"  3." s" 10 fm/mod" s" 2drop" bench
s" -3." s" 10 fm/mod -10 0 d+" s" 2drop" bench
s"  3." s" 10 sm/rem" s" 2drop" bench
s" -3." s" 10 sm/rem -1 +" s" 2drop" bench
s" 3." s" 10 um/mod" s" 2drop" bench
