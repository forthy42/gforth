\ complex numbers

\ Copyright (C) 2005 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\              *** Complex arithmetic ***              23sep91py

: complex' 2* floats ;
: complex+ float+ float+ ;

\ simple operations                                    02mar05py

: fl>      f@local0 lp+ ;

: zdup     fover fover ;
: zdrop    fdrop fdrop ;
: zover    3 fpick 3 fpick ;
: z>r      f>l f>l ;
: zr>      fl> fl> ;
: zswap    frot f>l frot fl> ;
: zpick    2* 1+ >r r@ fpick r> fpick ;
\ : zpin     2* 1+ >r r@ fpin r> fpin ;
: zdepth   fdepth 2/ ;
: zrot     z>r zswap zr> zswap ;
: z-rot    zswap z>r zswap zr> ;
: z@       dup >r f@ r> float+ f@ ;
: z!       dup >r float+ f! r> f! ;

\ simple operations                                    02mar05py
: z+       frot f+ f>l f+ fl> ;
: z-       fnegate frot f+ f>l f- fl> ;
: zr-      frot f- f>l fswap f- fl> ;
: x+       frot f+ fswap ;
: x-       fnegate x+ ;
: z*       fdup 4 fpick f* f>l fover 3 fpick f* f>l
           f>l fswap fl> f* f>l f* fl> f- fl> fl> f+ ;
: zscale   ftuck f* f>l f* fl> ;

\ simple operations                                    02mar05py

: znegate  fnegate fswap fnegate fswap ;
: zconj    fnegate ;
: z*i      fnegate fswap ;
: z/i      fswap fnegate ;
: zsqabs   fdup f* fswap fdup f* f+ ;
: 1/z      zconj zdup zsqabs 1/f zscale ;
: z/       1/z z* ;
: |z|      zsqabs fsqrt ;
: zabs     |z| 0e ;
: z2/      f2/ f>l f2/ fl> ;
: z2*      f2* f>l f2* fl> ;

: >polar  ( z -- r theta )  zdup  |z|  fswap frot fatan2 ;
: polar>  ( r theta -- z )  fsincos frot  zscale  fswap ;

\ zexp zln                                             02mar05py

: zexp     fsincos fswap frot fexp zscale ;
: pln      zdup fswap fatan2 frot frot |z| fln fswap ;
: zln      >polar fswap fln fswap ;

: z0=      f0= >r f0= r> and ;
: zsqrt    zdup z0= 0= IF
    fdup f0= IF  fdrop fsqrt 0e  EXIT  THEN
    zln z2/ zexp  THEN ;
: z**      zswap zln z* zexp ;
\ Test: Fibonacci-Zahlen
1e 5e fsqrt f+ f2/ fconstant g  1e g f- fconstant -h
: zfib  zdup z>r g 0e zswap z**
  zr> zswap z>r -h 0e zswap z** znegate zr> z+
  [ g -h f- 1/f ] FLiteral zscale ;

\ complexe Operationen                                 02mar05py

: zsinh    zexp zdup 1/z z- z2/ ;
: zcosh    zexp zdup 1/z z+ z2/ ;
: ztanh    z2* zexp zdup 1e 0e z- zswap 1e 0e z+ z/ ;

: zsin     z*i zsinh  z/i ;
: zcos     z*i zcosh ;
: ztan     z*i ztanh  z/i ;

: Real     fdrop ;
: Imag     fnip  ;

: Re       Real 0e ;
: Im       Imag 0e ;

\ complexe Operationen                                 02mar05py

: zasinh    zdup 1e f+   zover 1e f-   z* zsqrt z+ pln ;
: zacosh    zdup 1e x- z2/ zsqrt  zswap 1e x+ z2/ zsqrt z+
  pln z2* ;
: zatanh    zdup  1e x+ zln  zswap 1e x- znegate pln  z- z2/ ;
: zacoth    znegate zdup 1e x- pln  zswap 1e x+ pln   z- z2/ ;

pi f2/ FConstant pi/2

: zasin   ( f: z -- -iln[iz+sqrt[1-z^~2]] )   z*i zasinh z/i ;
: zacos   ( f: z -- pi/2-asin[z] )     pi/2 0e zswap zasin z- ;
: zatan   ( f: z -- [ln[1+iz]-ln[1-iz]]/2i ) z*i zatanh z/i ;
: zacot   ( f: z -- [ln[[z+i]/[z-i]]/2i )    z*i zacoth z/i ;

\ Ausgabe                                              24sep05py

Defer fc.       ' f. IS fc.
: z.       zdup z0= IF  zdrop ." 0 "  exit  THEN
           fdup f0= IF  fdrop fc. exit  THEN   fswap
           fdup f0= IF    fdrop
                    ELSE  fc.
                          fdup f0> IF  ." +"  THEN  THEN
           fc. ." i " ;
: z.s      zdepth 0 ?DO  i zpick zswap z>r z. zr>  LOOP ;
