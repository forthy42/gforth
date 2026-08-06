\ print FP numbers with "e" and variable width: e.p e.exact e. 

\ Author: M. Anton Ertl
\ Copyright (C) 2026 Free Software Foundation, Inc.

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

800 constant b-exact-len \ enough for binary64;
\ Intel's 80-bit extended FP numbers need up to 11452 digits in the mantissa

: rep.exp {: b mlen exp -- :}
    \ print postive number in exponential format
    b 1 type mlen 1 u> if
        ." ." b mlen 1 /string type then
    ." e" exp 1- 0 .r ;

: rep.>=1 {: b mlen exp -- :}
    \ print positive number >=1 with exponent 0
    exp mlen u< if
        b exp type ." ." b mlen exp /string type
    else
        b mlen type exp mlen - zeros then
    ." e" ;

: rep.<1 {: b mlen exp -- :}
    \ print positive number <1 with exponent 0
    ." ." exp negate zeros b mlen type ." e" ;

: rep. {: b mlen1 exp fnegative -- :}
    \ print finite number, remove trailing zeros
    fnegative if
        ." -" then
    b mlen1 -zeros {: mlen :}
    case ( b[ )
        exp mlen1 u<= ?of mlen exp rep.>=1 endof
        exp -5 u>        ?of mlen exp rep.<1  endof
        mlen exp rep.exp
    0 endcase ;

: e.p ( r u -- ) \ gforth-experimental e-dot-p
    \G Print @i{r} with up to @i{u} mantissa digits, leaving away
    \G trailing mantissa digits.  One additional mantissa digit is
    \G displayed if this can avoid showing an exponent.
    {: | b[ b-exact-len ] :}
    fdup fabs 10e dup s>f f** fover fover f>= 10e f* f< and if
        1+ then \ additional mantissa digit
    fdup b[ swap 2dup represent if ( r b[ u exp fnegative )
        fdrop rep.
    else
        2nip nip ( r fnegative )
        fdup f<> if
            drop ." NaN"
        else
            ( fnegative ) if
                ." -" then
            ." inf"
        then
    then ;

: e.exact ( r -- ) \ gforth-experimental e-dot-exact
    \g Print @i{r} at full length, with possibly more than 700 mantissa
    \g digits.  Caveat: @word{e.exact} only provides the exact output
    \g if @word{represent} does (which calls the C library's
    \g @code{ecvt_r()}); that is the case on not-too-old versions of
    \g glibc.
    b-exact-len e.p ;

\ For normal (as opposed to subnormal) numbers, it's probably good
\ enough to start at 15; it's probably also good enough to stop at 17
\ without checking that the result is exact.  In a test of 10,000,000
\ pseudo-random FP numbers, for the normal ones there were no
\ differences between the following and a version that starts at p=15
\ and, if necessary, calls p=17 without checking, except for
\ differences in exponential representation.  In a check of 30,799,692
\ numbers with short decimal mantissae (where differences are more
\ likely, because e. stops before e.15-17 starts), there were no
\ differences.  Also, the 15-17 variant is about 6 times faster; see
\ edot-check.fs.

: e. {: f: r -- :} \ gforth e-dot
    \G Print @i{r} with the least number of mantissa digits such that
    \G the result, when converted with @word{>float}, produces @i{r}
    \G (for finite @i{r}).  Caveat: @code{e.} only works as specified
    \G if @word{represent} (which calls the C library's
    \G @code{ecvt_r()}) produces the closest mantissa for the given
    \G buffer length; that is the case on not-too-old versions of
    \G glibc.
    20 1 do
        r i ['] e.p >string-execute {: c-addr u :} \ c-addr u dump
        c-addr u >float 0= if \ inf or nan
            leave then
        r f= if
            leave then
        c-addr free throw
    loop
    c-addr u type
    c-addr free throw ;

0 [if] \ testing
0e 6 e.p cr
1e 6 e.p cr
0.1e 6 e.p cr
0.00005e 6 e.p cr
0.000005e 6 e.p cr
2000e 6 e.p cr
20000e 6 e.p cr
inf 6 e.p cr
-inf 6 e.p cr
NaN 6 e.p cr

0e e.exact cr
1e e.exact cr
0.1e e.exact cr
0.00005e e.exact cr
0.000005e e.exact cr
2000e e.exact cr
20000e e.exact cr
inf e.exact cr
-inf e.exact cr
NaN e.exact cr

0e e. cr
1e e. cr
0.1e e. cr
0.00005e e. cr
0.000005e e. cr
2000e e. cr
20000e e. cr
inf e. cr
-inf e. cr
NaN e. cr

            
: s-as-f {: w^ n :} n f@ ;

1 s-as-f e. cr
2 s-as-f e. cr
3 s-as-f e. cr
4 s-as-f e. cr
5 s-as-f e. cr
6 s-as-f e. cr
7 s-as-f e. cr
8 s-as-f e. cr
9 s-as-f e. cr
[then]

