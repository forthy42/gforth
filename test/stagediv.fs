\ tests and benchmarks for staged division

\ Authors: Anton Ertl
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

-1 1 rshift dup constant max-n
1+ constant min-n

: check ( f -- )
	0= abort" test failed" ;

: ntest {: n | buf[ staged/-size ] -- :}
    n buf[ /f-stage1m
	0 buf[ /f-stage2m 0 = check
	-1 buf[ /f-stage2m -1 = check
	min-n n /f {: q1 :}
    min-n buf[ /f-stage2m q1 = check
	q1 1+ n * 1- {: lo :}
	lo buf[ /f-stage2m q1 = check
    lo buf[ modf-stage2m n 1- = check
    lo buf[ /modf-stage2m n 1- q1 d= check
    lo 1+ buf[ /f-stage2m q1 1+ = check
    lo 1+ buf[ modf-stage2m 0 = check
	max-n n /f {: q2 :}
	max-n buf[ /f-stage2m q2 = check
	q2 n * {: hi :}
	hi buf[ /f-stage2m q2 = check
    hi 1- buf[ /f-stage2m q2 1- = check
    max-n buf[ /modf-stage2m n * + max-n = check ;

: ntests ( -- )
	10000000 1 do i ntest loop
    max-n max-n 10000000 - do i ntest loop ;

: /fstagebench ( -- )
    {: | buf[ staged/-size ] :}
    3 buf[ /f-stage1m
    -1 -1 -100000000 do i - buf[ /f-stage2m loop drop ;

: /fbench ( -- )
    3 {: x :} -1 -1 -100000000 do i - x /f loop drop ;

: modfstagebench ( -- )
    {: | buf[ staged/-size ] :}
    3 buf[ /f-stage1m
    -1 -1 -100000000 do i - buf[ modf-stage2m loop drop ;

: modfbench ( -- )
    3 {: x :} -1 -1 -100000000 do i - x modf loop drop ;

: /modfstagebench ( -- )
    {: | buf[ staged/-size ] :}
    3 buf[ /f-stage1m
    -1 -1 -100000000 do i - buf[ /modf-stage2m - loop drop ;

: /modfbench ( -- )
    3 {: x :} -1 -1 -100000000 do i - x /modf - loop drop ;

: /fstage1bench ( -- )
    {: | buf[ staged/-size ] :}
    10000001 2 do i buf[ /f-stage1m loop ;

: utest {: u | buf[ staged/-size ] -- :}
	u buf[ u/-stage1m
	0 buf[ u/-stage2m 0= check
	u 1- buf[ u/-stage2m 0= check
	u buf[ u/-stage2m 1 = check
	-1 u u/ dup {: q1 :} u * {: hi :}
	hi buf[ u/-stage2m q1 = check
    hi 1- buf[ u/-stage2m q1 1- = check
    hi buf[ umod-stage2m 0= check
    hi 1- buf[ umod-stage2m u 1- = check
    -1 buf[ u/mod-stage2m u * + -1 = check ;

: utests ( -- )
	10000000 2 do i utest loop
	min-n 5000000 + min-n 5000000 - do i utest loop
	-1 -10000000 do i utest loop ;

: u/stagebench ( -- )
    {: | buf[ staged/-size ] :}
    3 buf[ u/-stage1m
    -1 -1 -100000000 do i - buf[ u/-stage2m loop drop ;

: u/bench ( -- )
    3 {: x :} -1 -1 -100000000 do i - x u/ loop drop ;

: umodstagebench ( -- )
    {: | buf[ staged/-size ] :}
    3 buf[ u/-stage1m
    -1 -1 -100000000 do i - buf[ umod-stage2m loop drop ;

: umodbench ( -- )
    3 {: x :} -1 -1 -100000000 do i - x umod loop drop ;

: u/modstagebench ( -- )
    {: | buf[ staged/-size ] :}
    3 buf[ u/-stage1m
    -1 -1 -100000000 do i - buf[ u/mod-stage2m - loop drop ;

: u/modbench ( -- )
    3 {: x :} -1 -1 -100000000 do i - x u/mod - loop drop ;

: u/stage1bench ( -- )
    {: | buf[ staged/-size ] :}
    10000001 2 do i buf[ u/-stage1m loop ;
0 [if]
    \ test with:
    gforth-fast stagediv.fs -e "ntests utests bye"

    \ Benchmark with:
    for i in u/ umod u/mod /f modf /modf; do
        cyc=`perf stat -x " " -e cycles gforth-fast test/stagediv.fs -e "${i}bench bye" 2>&1 | awk '{printf("%5.1f",$1/100000000)}'`;
        cycstage=`perf stat -x " " -e cycles gforth-fast test/stagediv.fs -e "${i}stagebench bye" 2>&1 | awk '{printf("%5.1f",$1/100000000)}'`;
        echo $cyc $cycstage" "$i;
     done; \
     for i in u/stage1 /fstage1; do
         perf stat -x " " -e cycles gforth-fast test/stagediv.fs -e "${i}bench bye" 2>&1 | awk '{printf("%10.1f '$i'\n",$1/10000000)}';
     done
    \ Results (in cycles per iteration of the microbenchmark):
     Haswell             Skylake              Zen2
    norm stag           norm stag           norm stag
    48.1 21.5 u/        41.3 15.5 u/        35.2 21.4 u/   
    46.6 26.0 umod      39.8 19.8 umod      36.9 25.8 umod 
    53.4 32.1 u/mod     44.0 25.3 u/mod     43.0 33.9 u/mod
    56.4 23.4 /f        48.7 16.9 /f        36.2 22.5 /f   
    55.8 27.2 modf      47.8 20.4 modf      37.9 27.2 modf 
    64.4 33.3 /modf     52.9 24.6 /modf     45.8 35.3 /modf
        224.3 u/stage1      229.4 u/stage1     102.2 u/stage1
        592.5 /fstage1      466.9 /fstage1     546.0 /fstage1
[then]
