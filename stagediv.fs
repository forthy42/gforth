\ 2-stage division

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

0
field: staged/-inverse \ for staged u/: the low cell of the inverse
field: staged/-shift   \ for staged /f
' staged/-shift alias staged/-inverse-hi \ high cell of the inverse for u/
field: staged/-divisor \ for computing the modulus
constant staged/-size ( -- u )
\g size of buffer for @code{u/-stage1m} or @code{/f-stage1m}.


\ unsigned division

\ this uses the method described in:
\ @InProceedings{ertl19kps,
\   author =       {M. Anton Ertl},
\   title =        {Integer Division by Multiplying with the
\                   Double-Width Reciprocal},
\   crossref =     {kps19},
\   pages =        {75--84},
\   url =          {http://www.complang.tuwien.ac.at/papers/ertl19kps.pdf},
\   url-slides =   {http://www.complang.tuwien.ac.at/papers/ertl19kps-slides.pdf},
\   abstract =     {Earlier work on integer division by multiplying with
\                   the reciprocal has focused on multiplying with a
\                   single-width reciprocal, combined with a correction
\                   and followed by a shift.  The present work explores
\                   using a double-width reciprocal to allow getting rid
\                   of the correction and shift.}
\ }
\ 
\ @Proceedings{kps19,
\   title =        {20. Kolloquium Programmiersprachen und Grundlagen
\                   der Programmierung (KPS)},
\   booktitle =    {20. Kolloquium Programmiersprachen und Grundlagen
\                   der Programmierung (KPS)},
\   year =         {2019},
\   key =          {kps19},
\   editor =       {Martin Pl\"umicke and Fayez Abu Alia},
\   url =          {https://www.hb.dhbw-stuttgart.de/kps2019/kps2019_Tagungsband.pdf}
\ }

0 [if]
    \ commented out because it is a primitive
: u/-stage2m {: udividend addr -- uquotient :}
    udividend addr staged/-inverse @ um* nip 0
    udividend addr staged/-inverse-hi @ um* d+ nip ;

: umod-stage2m {: udividend addr -- umodulus :}
    udividend dup addr u/-stage2m addr staged/-divisor @ * - ;

: u/mod-stage2m {: udividend addr -- umodulus uquotient :}
    udividend addr u/-stage2m udividend over addr staged/-divisor @ * - swap ;
[then]

: u/-stage1m {: udivisor addr -- :}
    udivisor 2 u< -24 and throw
    udivisor addr staged/-divisor !
    0 1 udivisor um/mod addr staged/-inverse-hi !
    udivisor 1- swap udivisor um/mod addr staged/-inverse ! drop ;

0 [if]
    -1 1 rshift dup constant max-n
    1+ constant min-n

    : check ( f -- )
	0= abort" test failed" ;
    
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
        -1 -1 -100000000 do i - 3 u/ loop drop ;
    
    : umodstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ u/-stage1m
        -1 -1 -100000000 do i - buf[ umod-stage2m loop drop ;

    : umodbench ( -- )
        -1 -1 -100000000 do i - 3 umod loop drop ;

    : u/modstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ u/-stage1m
        -1 -1 -100000000 do i - buf[ u/mod-stage2m - loop drop ;

    : u/modbench ( -- )
        -1 -1 -100000000 do i - 3 u/mod - loop drop ;

    \ utests
[then]