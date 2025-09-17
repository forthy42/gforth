\ test some gforth extension words

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2003,2004,2005,2006,2007,2009,2011,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

require ./tester.fs
decimal

\ test whether FILL corrupts FTOS (regression)

variable x
T{ 3e x 1 cells 'a' fill -> 3e }T

\ f>str-rdp (then f.rdp and f>buf-rdb should also be ok)

t{  12.3456789e 7 3 1 f>str-rdp s"  12.346" str= -> true }t
t{  12.3456789e 7 4 1 f>str-rdp s" 12.3457" str= -> true }t
t{ -12.3456789e 7 4 1 f>str-rdp s" -1.23E1" str= -> true }t
t{      0.0996e 7 3 1 f>str-rdp s"   0.100" str= -> true }t
t{      0.0996e 7 3 3 f>str-rdp s" 9.96E-2" str= -> true }t
t{    999.9994e 7 3 1 f>str-rdp s" 999.999" str= -> true }t
t{    999.9996e 7 3 1 f>str-rdp s" 1.000E3" str= -> true }t
t{       -1e-20 5 2 1 f>str-rdp s" *****"   str= -> true }t

\ 0x hex number conversion, or not

decimal
t{ 0x10 -> 16 }t
t{ 0X10 -> 16 }t
36 base !
t{ 0x10 -> x10 }t
decimal
t{ 'a' -> 97 }t
t{ 'A  -> 65 }t
t{ 1. '1 -> 1. 49 }t

\ REPRESENT has no trailing 0s even for inf and nan

t{  1e 0e f/ pad 16 represent drop 2drop pad 15 + c@ '0 = -> false }t
t{  0e 0e f/ pad 16 represent drop 2drop pad 15 + c@ '0 = -> false }t
t{ -1e 0e f/ pad 16 represent drop 2drop pad 15 + c@ '0 = -> false }t

\ safe/string

TESTING safe/string

[IFUNDEF] s1
    T{ :  s1 S" abcdefghijklmnopqrstuvwxyz" ; -> }T
[THEN]
t{ s1 29 safe/string -> s1 + 0 }t
T{ s1  5 safe/string -> s1 SWAP 5 + SWAP 5 - }T
T{ s1 10 safe/string -4 safe/string -> s1 10 safe/string }T
T{ s1  0 safe/string -> s1 }T
T{ s1 -5 safe/string -> s1 }T

\ TRY and friends

: 0<-throw ( n -- )
    0< throw ;

: try-test1 ( n1 -- n2 )
    try
        dup 0<-throw
        iferror
            2drop 25
        then
        1+
    endtry ;

t{ -5 try-test1 -> 26 }t
t{ 5  try-test1 ->  6 }t

: try-test2 ( n1 -- n2 )
    try
        0
    restore
        drop 1+ dup 0<-throw
    endtry ;

t{ -5 try-test2 -> 0 }t
t{  5 try-test2 -> 6 }t

: try-test3 ( n1 -- n2 )
    try
        dup 0<-throw
    endtry-iferror
        2drop 10
    else
        1+
    then ;

t{ -5 try-test3 -> 10 }t
t{  5 try-test3 ->  6 }t

\ fcopysign

t{  5e  1e fcopysign ->  5e }t
t{ -5e  1e fcopysign ->  5e }t
t{  5e -1e fcopysign -> -5e }t
t{ -5e -1e fcopysign -> -5e }t
\ tests involving -0e?

\ ?of nextcase contof

: mysgn ( n1 -- n2 )
    case
	dup 0< ?of drop -1 endof
	dup 0> ?of drop 1 endof
	dup
    endcase ;

t{  5 mysgn ->  1 }t
t{ -3 mysgn -> -1 }t
t{  0 mysgn ->  0 }t

: myscan ( addr1 n1 char -- addr2 n2 )
    >r case
	dup 0= ?of endof
        over c@ r@ = ?of endof
        1 /string
        next-case
    rdrop ;

t{ s" dhfa;jfsdk" 2dup ';' myscan 2swap 4 /string d= -> true }t
t{ s" abcdef" 2dup 'g' myscan 2swap 6 /string d= -> true }t


: gcd ( n1 n2 -- n )
    case
	2dup > ?of tuck - contof
	2dup < ?of over - contof
    endcase ;

t{ 48 42 gcd -> 6 }t
t{ 42 48 gcd -> 6 }t


: x1 ( u -- u u1 ... un )
    case
	dup
	1 of endof
        dup 1 and ?of 3 * 1+ contof
        2/
    next-case ;

t{ 7 x1 -> 7 22 11 34 17 52 26 13 40 20 10 5 16 8 4 2 1 }t

\ recognizer tests

T{ 0 rec-sequence: RS -> }T

T{ :noname 1 ;  :noname 2 ;  :noname 3  ; translate: translate-1 -> }T
T{ :noname 10 ; :noname 20 ; :noname 30 ; translate: translate-2 -> }T

\ really stupid: 1 character length or 2 characters
T{ : rec-1 NIP 1 = IF translate-1 ELSE 0 THEN ; -> }T
T{ : rec-2 NIP 2 = IF translate-2 ELSE 0 THEN ; -> }T

T{ translate-1 interpreting  -> 1 }T
T{ translate-1 compiling     -> 2 }T
T{ translate-1 postponing    -> 3 }T

\ set and get methods
T{ 0 ' RS set-recs -> }T
T{ ' RS get-recs -> 0 }T

T{ ' rec-1 1 ' RS set-recs -> }T
T{ ' RS get-recs -> ' rec-1 1 }T

T{ ' rec-1 ' rec-2 2 ' RS set-recs -> }T
T{ ' RS get-recs -> ' rec-1 ' rec-2 2 }T

\ testing RECOGNIZE
T{         0 ' RS set-recs -> }T
T{ S" 1"     RS   -> 0 }T
T{ ' rec-1 1 ' RS set-recs -> }T
T{ S" 1"     RS   -> translate-1 }T
T{ S" 10"    RS   -> 0 }T
T{ ' rec-2 ' rec-1 2 ' RS set-recs -> }T
T{ S" 10"    RS   -> translate-2 }T

\ extended synonym behaviour
t{ create coc1 -> }t
t{ synonym coc2 coc1 -> }t
t{ coc2 -> coc1 }t
t{ : coc3 coc2 ; -> }t
t{ coc3 -> coc1 }t
t{ ' coc2 -> ' coc1 }t \ so >body obviously works

t{ defer cod1 -> }t
t{ synonym cod2 cod1 -> }t
t{ ' true is cod2 -> }t
t{ cod2 -> true }t
t{ cod1 -> true }t
t{ action-of cod2 -> ' true }t
: cod2-ao action-of cod2 ;
t{ cod2-ao -> ' true }t

\ synonym behaviour for umethods; SOURCE is a umethod
t{ synonym source2 source -> }t
t{ ' source2 -> ' source }t
t{ action-of source2 -> action-of source }t
: source2-ao action-of source2 ;
t{ source2-ao -> action-of source }t

\ synonym and immediate
t{ create coc8 immediate -> }t
t{ synonym coc9 coc8 -> }t
t{ "coc8" find-name immediate? -> true }t
t{ "coc9" find-name immediate? -> true }t
t{ ] coc9 [ -> coc8 }t

\ synonym and using nts as xts
t{ "coc2" find-name execute -> coc1 }t
\ t{ "coc2" find-name >body -> coc1 }t
t{ : coca [ "coc2" find-name compile, ] ; -> }t
t{ coca -> coc1 }t


\ alias

t{ ' coc1 alias coc4 -> }t
t{ coc4 -> coc1 }t
t{ : coc5 coc4 ; -> }t
t{ coc5 -> coc1 }t
t{ ' coc4 -> ' coc1 }t

t{ ' coc1 alias coc6 immediate -> }t
t{ "coc1" find-name immediate? -> false }t
t{ "coc6" find-name immediate? -> true }t
t{ ] coc6 [ -> coc1 }t
t{ ' coc6 -> ' coc1 }t

t{ ' cod1 alias aod2 -> }t
t{ ' false is aod2 -> }t
t{ aod2 -> false }t
t{ cod1 -> false }t
t{ action-of aod2 -> ' false }t
: aod2-ao action-of aod2 ;
t{ aod2-ao -> ' false }t

\ alias behaviour for umethods; SOURCE is a umethod
t{ ' source alias source3 -> }t
t{ ' source3 -> ' source }t
t{ action-of source3 -> action-of source }t
: source3-ao action-of source3 ;
t{ source3-ao -> action-of source }t

\ alias and using nts as xts
t{ "coc4" find-name execute -> coc1 }t
\ t{ "coc4" find-name >body -> coc1 }t
t{ : cocb [ "coc4" find-name compile, ] ; -> }t
t{ cocb -> coc1 }t


\ interpret/compile:

t{ ' coc1 ' coc8 interpret/compile: cocc -> }t
t{ cocc -> coc1 }t
t{ ] cocc [ -> coc8 }t
t{ "cocc" find-name execute -> coc1 }t
t{ : cocd [ "cocc" find-name compile, ] ; -> }t
t{ cocd -> coc1 }t

t{ ' coc1 ' coc8 interpret/compile: coce immediate -> }t
t{ coce -> coc1 }t
t{ ] coce [ -> coc1 }t

\ +to and addr

1 value foo#
2. 2value foo##
3e fvalue foo%
: +foo# +to foo# ;
\ : &foo# addr foo# ;
: +foo## +to foo## ;
\ : &foo## addr foo## ;
: +foo% +to foo% ;
\ : &foo% addr foo% ;

t{ 2 +to foo# foo# -> 3 }t
\ t{ addr foo# @ -> 3 }t
t{ 5. +to foo## foo## -> 7. }t
\ t{ addr foo## 2@ -> 7. }t
t{ 7e +to foo% foo% -> 10e }t
\ t{ addr foo% f@ -> 10e }t

t{ 2 +foo# foo# -> 5 }t
\ t{ &foo# @ -> 5 }t
t{ 5. +foo## foo## -> 12. }t
\ t{ &foo## 2@ -> 12. }t
t{ 7e +foo% foo% -> 17e }t
\ t{ &foo% f@ -> 17e }t

\ replace-word

t{ : rw-test1 1 ; : rw-test2 2 ; -> }t
t{ rw-test1 -> 1 }t
t{ rw-test2 -> 2 }t
t{ ' rw-test1 ' rw-test2 replace-word -> }t
t{ rw-test2 -> 1 }t

\ execute-exit

t{ : execute1 execute-exit ; -> }t
t{ 1 >r ' r> execute1 -> 1 }t
t{ : execute2 {: xt :} xt execute-exit ; -> }t
t{ : execute-exit-test {: a :} >r ['] r> execute2 a ; -> }t
t{ 1 2 execute-exit-test -> 1 2 }t

\ postpone locals
0 warnings !@ >r

t{ : pl-test1 'a' {: c: l :} postpone l ; immediate -> }t
t{ : pl-test2 pl-test1 ; -> }t
t{ pl-test2 -> 'a' }t
t{ : pl-test3 8 9 {: d: l :} postpone l ; immediate -> }t
t{ : pl-test4 pl-test3 ; -> }t
t{ pl-test4 -> 8 9 }t
t{ : pl-test5 123e {: f: l :} postpone l ; immediate -> }t
t{ : pl-test6 pl-test5 ; -> }t
t{ pl-test6 -> 123e }t
t{ : pl-test7 123 {: l :} postpone l ; immediate -> }t
t{ : pl-test8 pl-test7 ; -> }t
t{ pl-test8 -> 123 }t
t{ : pl-test9 ['] + {: xt: l :} postpone l ; immediate -> }t
t{ : pl-testa pl-test9 ; -> }t
t{ 3 6 pl-testa -> 9 }t

r> warnings !

\ optimized pick and fpick
t{ : pick-test 4 pick 3 pick 2 pick 1 pick 0 pick ; -> }t
t{ 5 6 7 8 9 pick-test -> 5 6 7 8 9 5 7 9 7 7 }t
t{ : fpick-test 4 fpick 3 fpick 2 fpick 1 fpick 0 fpick ; -> }t
t{ 5e 6e 7e 8e 9e fpick-test -> 5e 6e 7e 8e 9e 5e 7e 9e 7e 7e }t

\ testing standard recongizers
\ `<word> and ``<word>

t{ `needs -> ' needs }t
t{ ``needs -> "needs" find-name }t

\ more standard recognizers
: eval-rec ( addr u -- )
    [: parse-name rec-forth ;] execute-parsing ;
: eval-rec-interpret ( addr u -- )
    [: parse-name rec-forth interpreting ;] execute-parsing ;

t{ s" needs" rec-forth -> ``needs translate-name }t
t{ s\" \"a string 123\"" eval-rec -rot s\" \"a" str= -> scan-translate-string true }t
t{ s\" \"a string 123\"" eval-rec-interpret s" a string 123" str= -> true }t
t{ "->#123." rec-forth -> 0 }t
t{ "+>123e" rec-forth -> 0 }t
t{ "`#123." rec-forth -> 0 }t
t{ "``#123." rec-forth -> 0 }t
t{ "`123e" rec-forth -> 0 }t
t{ "``123e" rec-forth -> 0 }t
t{ "${HOME}" rec-forth -rot "HOME" str= -> translate-env true }t 
t{ "${HOME}" eval-rec-interpret s" HOME" getenv str= -> true }t 
t{ "`xlerb" rec-forth -> 0 }t
t{ "``xlerb" rec-forth -> 0 }t
t{ "->xlerb" rec-forth -> 0 }t

0 warnings !@ >r
: eval-catch ( addr u -- throwcode | results 0 )
    [{: addr u :}h1 addr u evaluate ;] catch-nobt ;
r> warnings !

1 Value value1
2 Varue Varue2
Defer defer3
' dup is defer3

t{ "3 ->value1" eval-catch value1 -> 0 3 }t
t{ "2 +>value1" eval-catch value1 -> 0 5 }t
\ t{ "'>value1" eval-catch value1 -> -2054 5 }t
t{ "@>value1" eval-catch value1 -> -21 5 }t
t{ "3 ->Varue2" eval-catch Varue2 -> 0 3 }t
t{ "2 +>Varue2" eval-catch Varue2 -> 0 5 }t
t{ "'>Varue2" eval-catch Varue2 -> `Varue2 0 5 }t
t{ "@>Varue2" eval-catch Varue2 -> -21 5 }t
t{ "@>defer3" eval-catch -> ' dup 0 }t
t{ "' noop =>defer3" eval-catch @>defer3 -> 0 ' noop }t
t{ "2 +>defer3" eval-catch @>defer3 -> -21 ' noop }t
\ t{ "'>defer3" eval-catch @>defer3 -> -2054 ' noop }t

\ tests for non-existing words

T{ ' ' catch xlerb -> -13 }T
T{ s" xlerb" find-name -> 0 }T

T{ s" 3 to xlerb" eval-catch -> -13 }T

\ division words

t{ -10  7 /s -> -1 }t
t{  10 -7 /s -> -1 }t
t{ -10 -7 /s ->  1 }t

t{ -10  7 /f -> -2 }t
t{  10 -7 /f -> -2 }t
t{ -10 -7 /f ->  1 }t

t{ -10  7 /  -> -2 }t
t{  10 -7 /  -> -2 }t
t{ -10 -7 /  ->  1 }t

t{ -1 2 u/ -> max-n }t

t{ -10  7 mods -> -3 }t
t{  10 -7 mods ->  3 }t
t{ -10 -7 mods -> -3 }t

t{ -10  7 modf ->  4 }t
t{  10 -7 modf -> -4 }t
t{ -10 -7 modf -> -3 }t

t{ -10  7 mod  ->  4 }t
t{  10 -7 mod  -> -4 }t
t{ -10 -7 mod  -> -3 }t

t{ -1 4 umod -> 3 }t

t{ -10  7 /mods -> -3 -1 }t
t{  10 -7 /mods ->  3 -1 }t
t{ -10 -7 /mods -> -3  1 }t

t{ -10  7 /modf ->  4 -2 }t
t{  10 -7 /modf -> -4 -2 }t
t{ -10 -7 /modf -> -3  1 }t

t{ -10  7 /mod  ->  4 -2 }t
t{  10 -7 /mod  -> -4 -2 }t
t{ -10 -7 /mod  -> -3  1 }t

t{ -1 2 u/mod -> 1 max-n }t

t{ -5 2  7 */s -> -1 }t
t{  5 2 -7 */s -> -1 }t
t{ -5 2 -7 */s ->  1 }t

t{ -5 2  7 */f -> -2 }t
t{  5 2 -7 */f -> -2 }t
t{ -5 2 -7 */f ->  1 }t

t{ -5 2  7 */  -> -2 }t
t{  5 2 -7 */  -> -2 }t
t{ -5 2 -7 */  ->  1 }t

t{ -2 15 -1 u*/ -> 14 }t

t{ -2 5  7 */mods -> -3 -1 }t
t{  2 5 -7 */mods ->  3 -1 }t
t{ -2 5 -7 */mods -> -3  1 }t

t{ -2 5  7 */modf ->  4 -2 }t
t{  2 5 -7 */modf -> -4 -2 }t
t{ -2 5 -7 */modf -> -3  1 }t

t{ -2 5  7 */mod  ->  4 -2 }t
t{  2 5 -7 */mod  -> -4 -2 }t
t{ -2 5 -7 */mod  -> -3  1 }t

t{ -2 15 -1 u*/mod -> -16 14 }t

\ optimization of division words

\ division by power of 2
t{ :noname -21 4 /f    ; execute -> -6 }t
t{ :noname -21 4 modf  ; execute ->  3 }t
t{ :noname -21 4 /modf ; execute ->  3 -6 }t
t{ :noname  25 4 u/    ; execute ->  6 }t
t{ :noname  25 4 umod  ; execute ->  1 }t
t{ :noname  25 4 u/mod ; execute ->  1 6 }t

\ division by other optimizable divisor
t{ :noname -20 3 /f    ; execute -> -7 }t
t{ :noname -20 3 modf  ; execute ->  1 }t
t{ :noname -20 3 /modf ; execute ->  1 -7 }t
t{ :noname  39 5 u/    ; execute ->  7 }t
t{ :noname  39 5 umod  ; execute ->  4 }t
t{ :noname  39 5 u/mod ; execute ->  4 7 }t

\ division by non-optimizable divisor
t{ :noname -20 -3 /f    ; execute ->  6 }t
t{ :noname -20 -3 modf  ; execute ->  -2 }t
t{ :noname -20 -3 /modf ; execute ->  -2 6 }t

\ closures

\ : homeloc <{: w^ a w^ b w^ c :}h a b c ;> ;

\ t{ 1 2 3 homeloc >r @ swap @ rot @ r> free -> 1 2 3 0 }t

0 warnings !@ >r
: combiner [{: a b xt: do-it | c :}h1 a b do-it ;] ;

t{ 1 2 ' + combiner execute -> 3 }t
t{ #1234 #5678 ' xor combiner execute -> #4860 }t
t{ 0 0 ' + combiner #1234 #5678 third >body 2! execute -> #6912 }t

: A {: w^ k x1 x2 x3 xt: x4 xt: x5 | w^ B :} recursive
    k @ 0<= IF  x4 x5 +  ELSE
	B k x1 x2 x3 action-of x4 [{: B k x1 x2 x3 x4 :}L
	    -1 k +!
	    k @ B @ x1 x2 x3 x4 A ;] dup B !
	execute  THEN ;
: man-or-boy? ( n -- n' ) [: 1 ;] [: -1 ;] 2dup swap [: 0 ;] A ;
r> warnings !

t{ 0 man-or-boy? -> 1 }t
t{ 1 man-or-boy? -> 0 }t
t{ 2 man-or-boy? -> -2 }t
t{ 3 man-or-boy? -> 0 }t
t{ 4 man-or-boy? -> 1 }t
t{ 5 man-or-boy? -> 0 }t
t{ 6 man-or-boy? -> 1 }t
t{ 7 man-or-boy? -> -1 }t
t{ 8 man-or-boy? -> -10 }t

\ const-does>
0 warnings !@
t{ : cdtest1 2 2 CONST-DOES> swap fswap ; -> }t
warnings !
t{ 1 2 3e 4e cdtest1 cdtest2 -> }t
t{ : cdtest3 cdtest2 ; -> }t
t{ cdtest2 -> 2 1 4e 3e }t
t{ cdtest3 -> 2 1 4e 3e }t

\ litstack clear when starting colon definition?
t{ : foo 1 [ : bar ; clearstack bar -> }t

\ compile, state clear when starting colon definition?
0 warnings !@ >r
t{ ' drop compile, : csc1 80 ; csc1 -> 80 }t
t{ ' drop compile, :noname 80 ; execute -> 80 }t
t{ ' drop compile, [: 80 ;] execute -> 80 }t
r> warnings !


\ -loop

t{ : test--loop do i swap dup -loop drop ; -> }t
t{ 1 2 5 test--loop -> 5 4 3 }t
t{ 2 2 5 test--loop -> 5 3 }t
t{ -1 0 0 test--loop -> 0 1 }t
t{ max-n 0 0 test--loop -> 0 max-n negate 2 }t
t{ max-n 1+ 0 0 test--loop -> 0 max-n 1+ }t

\ mem+loop mem-loop

t{ create test-mem*a 3 , 5 , 1 , -3 , -> }t
t{ : test-mem+3 test-mem*a 4 cell array>mem mem+do i @ loop ; -> }
t{ test-mem+3 -> 3 5 1 -3 }t
t{ : test-mem+2 4 cell array>mem mem+do i @ loop ; -> }
t{ test-mem*a test-mem+2 -> 3 5 1 -3 }t
t{ : test-mem+1 cell array>mem mem+do i @ loop ; -> }
t{ test-mem*a 4 test-mem+1 -> 3 5 1 -3 }t
t{ : test-mem+0 array>mem mem+do i @ loop ; -> }
t{ test-mem*a 4 cell test-mem+0 -> 3 5 1 -3 }t
t{ : test-mem-3 test-mem*a 4 cell array>mem mem-do i @ loop ; -> }
t{ test-mem-3 -> -3 1 5 3 }t
t{ : test-mem-2 4 cell array>mem mem-do i @ loop ; -> }
t{ test-mem*a test-mem-2 -> -3 1 5 3 }t
t{ : test-mem-1 cell array>mem mem-do i @ loop ; -> }
t{ test-mem*a 4 test-mem-1 -> -3 1 5 3 }t
t{ : test-mem-0 array>mem mem-do i @ loop ; -> }
t{ test-mem*a 4 cell test-mem-0 -> -3 1 5 3 }t
t{ test-mem*a 1 cell test-mem-0 -> 3 }t
t{ test-mem*a 0 cell test-mem-0 -> }t
0 warnings !@ >r
t{ : test-mem-l 1 {: a :} array>mem mem-do 2 {: b :} i @ loop a ; -> }
r> warnings !
t{ test-mem*a 4 cell test-mem-l -> -3 1 5 3 1 }t

\ -[do u-[do

t{ : -[do-test -[do i -1 +loop ; -> }t
t{ -1 1 -[do-test -> 1 0 -1 }t
t{ 0 0 -[do-test -> 0 }t
t{ 0 -1 -[do-test -> }t

t{ : u-[do-test u-[do i -1 +loop ; -> }t
t{ max-n max-n 1+ u-[do-test -> max-n 1+ max-n }t
t{ 0 0 u-[do-test -> 0 }t
t{ max-n 1+ max-n u-[do-test -> }t

\ rpick

: t-rpick ( n1 n2 n3 n4 -- n4 n3 n2 n1 )
    >r >r >r >r
    3 rpick 2 rpick 1 rpick 0 rpick
    rdrop rdrop rdrop rdrop ;
t{ 1 2 3 4 t-rpick -> 4 3 2 1 }t

\ extra-section

t{ 1 extra-section t-extra-section -> }t
t{ ' unused t-extra-section 1 pagesize within -> true }t
t{ ' unused t-extra-section ' allot ' t-extra-section catch -> 0 }t
t{ 1 ' allot ' t-extra-section catch nip nip -> -8 }t

\ refill test with different newlines

: refill-tester ( n -- addr u )
    [: 0 ?DO  refill drop source type ." /"  LOOP  refill drop ;] $tmp ;

t{ 4 refill-tester
ab
cde
fghijklmn
s" ab/cde/fghi/jklmn/" str= -> true }t

\ refill with&without newline at end of last line
\ (do not add a newline to the end of this buffer!)
\ This test absolutely has to be the last one in this file, don't add
\ further tests after this, don't add a newline to this file!

5 0 [DO]
    [I] .
[LOOP]
10 5 [DO] [I] . [LOOP] cr