\ complex numbers

\ Copyright (C) 2005,2007 Free Software Foundation, Inc.

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

\              *** Complex arithmetic ***              23sep91py

: complex' ( n -- offset ) 2* floats ;
: complex+ ( zaddr -- zaddr' ) float+ float+ ;

\ simple operations                                    02mar05py

: fl>      ( -- r ) f@local0 lp+ ;

: zdup     ( z -- z z ) fover fover ;
: zdrop    ( z -- ) fdrop fdrop ;
: zover    ( z1 z2 -- z1 z2 z1 ) 3 fpick 3 fpick ;
: z>r      ( z -- r:z) f>l f>l ;
: zr>      ( r:z -- z ) fl> fl> ;
: zswap    ( z1 z2 -- z2 z1 ) frot f>l frot fl> ;
: zpick    ( z1 .. zn n -- z1 .. zn z1 ) 2* 1+ >r r@ fpick r> fpick ;
\ : zpin     2* 1+ >r r@ fpin r> fpin ;
: zdepth   ( -- u ) fdepth 2/ ;
: zrot     ( z1 z2 z3 -- z2 z3 z1 ) z>r zswap zr> zswap ;
: z-rot    ( z1 z2 z3 -- z3 z1 z2 ) zswap z>r zswap zr> ;
: z@       ( zaddr -- z ) dup >r f@ r> float+ f@ ;
: z!       ( z zaddr -- ) dup >r float+ f! r> f! ;

\ simple operations                                    02mar05py
: z+       ( z1 z2 -- z1+z2 ) frot f+ f>l f+ fl> ;
: z-       ( z1 z2 -- z1-z2 ) fnegate frot f+ f>l f- fl> ;
: zr-      ( z1 z2 -- z2-z1 ) frot f- f>l fswap f- fl> ;
: x+       ( z r -- z+r ) frot f+ fswap ;
: x-       ( z r -- z-r ) fnegate x+ ;
: z*       ( z1 z2 -- z1*z2 )
           fdup 4 fpick f* f>l fover 3 fpick f* f>l
           f>l fswap fl> f* f>l f* fl> f- fl> fl> f+ ;
: zscale   ( z r -- z*r ) ftuck f* f>l f* fl> ;

\ simple operations                                    02mar05py

: znegate  ( z -- -z ) fnegate fswap fnegate fswap ;
: zconj    ( rr ri -- rr -ri ) fnegate ;
: z*i      ( z -- z*i ) fnegate fswap ;
: z/i      ( z -- z/i ) fswap fnegate ;
: zsqabs   ( z -- |z|² ) fdup f* fswap fdup f* f+ ;
: 1/z      ( z -- 1/z ) zconj zdup zsqabs 1/f zscale ;
: z/       ( z1 z2 -- z1/z2 ) 1/z z* ;
: |z|      ( z -- r ) zsqabs fsqrt ;
: zabs     ( z -- |z| ) |z| 0e ;
: z2/      ( z -- z/2 ) f2/ f>l f2/ fl> ;
: z2*      ( z -- z*2 ) f2* f>l f2* fl> ;

: >polar  ( z -- r theta )  zdup  |z|  fswap frot fatan2 ;
: polar>  ( r theta -- z )  fsincos frot  zscale  fswap ;

\ zexp zln                                             02mar05py

: zexp     ( z -- exp[z] ) fsincos fswap frot fexp zscale ;
: pln      ( z -- pln[z] ) zdup fswap fatan2 frot frot |z| fln fswap ;
: zln      ( z -- ln[z] ) >polar fswap fln fswap ;

: z0=      ( z -- flag ) f0= >r f0= r> and ;
: zsqrt    ( z -- sqrt[z] ) zdup z0= 0= IF
    fdup f0= IF  fdrop fsqrt 0e  EXIT  THEN
    zln z2/ zexp  THEN ;
: z**      ( z1 z2 -- z1**z2 ) zswap zln z* zexp ;
\ Test: Fibonacci-Zahlen
1e 5e fsqrt f+ f2/ fconstant g  1e g f- fconstant -h
: zfib     ( z1 -- fib[z1] ) zdup z>r g 0e zswap z**
  zr> zswap z>r -h 0e zswap z** znegate zr> z+
  [ g -h f- 1/f ] FLiteral zscale ;

\ complexe Operationen                                 02mar05py

: zsinh    ( z -- sinh[z] ) zexp zdup 1/z z- z2/ ;
: zcosh    ( z -- cosh[z] ) zexp zdup 1/z z+ z2/ ;
: ztanh    ( z -- tanh[z] ) z2* zexp zdup 1e 0e z- zswap 1e 0e z+ z/ ;

: zsin     ( z -- sin[z] ) z*i zsinh  z/i ;
: zcos     ( z -- cos[z] ) z*i zcosh ;
: ztan     ( z -- tan[z] ) z*i ztanh  z/i ;

: Real     ( z -- r ) fdrop ;
: Imag     ( z -- i ) fnip  ;

: Re       ( z -- zr ) Real 0e ;
: Im       ( z -- zi ) Imag 0e ;

\ complexe Operationen                                 02mar05py

: zasinh    ( z -- asinh[z] ) zdup 1e f+   zover 1e f-   z* zsqrt z+ pln ;
: zacosh    ( z -- acosh[z] ) zdup 1e x- z2/ zsqrt  zswap 1e x+ z2/ zsqrt z+
  pln z2* ;
: zatanh    ( z -- atanh[z] ) zdup  1e x+ zln  zswap 1e x- znegate pln  z- z2/ ;
: zacoth    ( z -- acoth[z] ) znegate zdup 1e x- pln  zswap 1e x+ pln   z- z2/ ;

pi f2/ FConstant pi/2

: zasin   ( z -- -iln[iz+sqrt[1-z^~2]] )   z*i zasinh z/i ;
: zacos   ( z -- pi/2-asin[z] )     pi/2 0e zswap zasin z- ;
: zatan   ( z -- [ln[1+iz]-ln[1-iz]]/2i ) z*i zatanh z/i ;
: zacot   ( z -- [ln[[z+i]/[z-i]]/2i )    z*i zacoth z/i ;

\ Ausgabe                                              24sep05py

Defer fc.       ' f. IS fc.
: z. ( z -- )
           zdup z0= IF  zdrop ." 0 "  exit  THEN
           fdup f0= IF  fdrop fc. exit  THEN   fswap
           fdup f0= IF    fdrop
                    ELSE  fc.
                          fdup f0> IF  ." +"  THEN  THEN
           fc. ." i " ;
: z.s ( z1 .. zn -- z1 .. zn )
	   zdepth 0 ?DO  i zpick zswap z>r z. zr>  LOOP ;
