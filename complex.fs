\ complex numbers

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2005,2007,2015,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

: fl>      ( -- r ) 0 f@localn lp+ ;

: zdup     ( z -- z z ) fover fover ;
: zdrop    ( z -- ) fdrop fdrop ;
: zover    ( z1 z2 -- z1 z2 z1 ) 3 fpick 3 fpick ;
: z>r      ( z -- l:z )  f>l f>l ;
: zr>      ( r:z -- z )  fl> fl> ;
: zswap    ( z1 z2 -- z2 z1 ) frot f>l frot fl> ;
: zpick    ( z1 .. zn n -- z1 .. zn z1 ) 2* 1+ dup >r fpick r> fpick ;
\ : zpin     2* 1+ dup >r fpin r> fpin ;
: zdepth   ( -- u ) fdepth 2/ ;
: zrot     ( z1 z2 z3 -- z2 z3 z1 ) z>r zswap zr> zswap ;
: z-rot    ( z1 z2 z3 -- z3 z1 z2 ) zswap z>r zswap zr> ;
: z@       ( zaddr -- z ) dup >r f@ r> float+ f@ ;
: z!       ( z zaddr -- ) dup >r float+ f! r> f! ;
: z+!      ( z zaddr -- ) dup >r float+ f+! r> f+! ;

\ locals                                               10jan15py

to-table: z!-table z! z+!

z!-table locals-to-class: to-z:

: compile-pushlocal-z ( a-addr -- ) ( run-time: z -- )
    locals-size @ alignlp-f float+ float+ dup locals-size !
    swap !
    ]] f>l f>l [[ ;
: compile-z@local ( n -- )
    dup ]] literal f@localn [[ float+ ]] literal f@localn [[ ;

also locals-types definitions
: z: ( compilation "name" -- a-addr xt; run-time z -- ) \ gforth z-colon
    \G Define value-flavoured complex local @i{name} @code{( -- z1 )}
    [: @ lp-offset compile-z@local ;]
    ['] to-z: create-local
    ['] compile-pushlocal-z ;
: za: ( compilation "name" -- a-addr xt; run-time z -- ) \ gforth z-a-colon
    \G Define varue-flavoured complex local @i{name} @code{( -- z1 )}
    addressable: z: ;
: z^ ( "name" -- a-addr xt )
    w^ drop  ['] compile-pushlocal-z ;
previous definitions

also locals-types

z: some-zlocal 2drop

previous

\ Variables and values

: ZVariable ( -- )  Create 0e f, 0e f, ;

' >body z!-table to-class: z-to
: ZValue ( complex -- )
    Create 1 complex' small-allot z!
    ['] z@ set-does>
    [: lit, postpone z@ ;] set-optimizer
    ['] z-to set-to ;

: ZVarue ( complex -- ) \ gforth-obsolete
    ZValue addressable ;

\ simple operations                                    02mar05py
: z+       ( z1 z2 -- z1+z2 ) frot f+ f>l f+ fl> ;
: z-       ( z1 z2 -- z1-z2 ) fnegate frot f+ f>l f- fl> ;
: zr-      ( z1 z2 -- z2-z1 ) frot f- f>l fswap f- fl> ;
: x+       ( z r -- z+r ) frot f+ fswap ;
: x-       ( z r -- z-r ) fnegate x+ ;
: z*       ( z1 z2 -- z1*z2 )
    { f: r1 f: i1 f: r2 f: i2 -- }
    r1 r2 f* i1 i2 f* f-
    r1 i2 f* r2 i1 f* f+ ;
\ code using locals not only is easier to read, but also slightly faster
\           fdup 4 fpick f* f>l fover 3 fpick f* f>l
\           f>l fswap fl> f* f>l f* fl> f- fl> fl> f+ ;
: zscale   ( z r -- z*r ) ftuck f* f>l f* fl> ;

\ simple operations                                    02mar05py

: znegate  ( z -- -z ) fnegate fswap fnegate fswap ;
: zconj    ( rr ri -- rr -ri ) fnegate ;
: z*i      ( z -- z*i ) fnegate fswap ;
: z/i      ( z -- z/i ) fswap fnegate ;
: zsqabs   ( z -- |z|² ) fdup f* fswap fdup f* f+ ;
: 1/z      ( z -- 1/z ) zconj zdup zsqabs 1/f zscale ;
: z/       ( z1 z2 -- z1/z2 ) 1/z z* ;
: pyth ( a b c -- r ) f/ fdup f* 1e f+ fsqrt  f* ;
: |z| ( z -- r )
    \ compute sqrt(a^2+b^2) without overflow
    fabs fswap fabs
    fover fover f> IF
	fover ( f: a b a -- ) pyth  exit
    THEN
    fdup f0= IF  fnip
    ELSE  ftuck ( f: b a b -- ) pyth
    ENDIF ;
\ : |z|      ( z -- r ) zsqabs fsqrt ;
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
: zsqrt    ( z -- sqrt[z] )
    zdup z0= 0= IF
        fdup f0= IF
            fdrop fdup f0< if
                fnegate fsqrt 0e fswap exit then
            fsqrt 0e  EXIT  THEN
	zln z2/ zexp  THEN ;
: z**      ( z1 z2 -- z1**z2 )
  zdup z0=         if              zdrop zdrop 1e  0e exit then
  fover f0>        if zover z0= if zdrop ( 0+0i )     exit then then
  zdup f0= f0< and if zover z0= if zdrop zdrop Inf 0e exit then then
  zswap zln z* zexp ;
\ Test: Fibonacci-Zahlen
1e 5e fsqrt f+ f2/ fconstant phi
: zfib     ( z1 -- fib[z1] )
    zdup z>r phi 0e zswap z**
    zr> zswap z>r [ 1e phi f- ] FLiteral
    0e zswap z** znegate zr> z+
    [ 5e fsqrt 1/f ] FLiteral zscale ;

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
: zacosh    ( z -- acosh[z] )
    zdup 1e x- z2/ zsqrt  zswap 1e x+ z2/ zsqrt z+ pln z2* ;
: zatanh    ( z -- atanh[z] ) zdup  1e x+ zln  zswap 1e x- znegate pln  z- z2/ ;
: zacoth    ( z -- acoth[z] ) znegate zdup 1e x- pln  zswap 1e x+ pln   z- z2/ ;

pi f2/ FConstant pi/2

: zasin   ( z -- -iln[iz+sqrt[1-z^~2]] )   z*i zasinh z/i ;
: zacos   ( z -- pi/2-asin[z] )     pi/2 0e zswap zasin z- ;
: zatan   ( z -- [ln[1+iz]-ln[1-iz]]/2i ) z*i zatanh z/i ;
: zacot   ( z -- [ln[[z+i]/[z-i]]/2i )    z*i zacoth z/i ;

\ Ausgabe                                              24sep05py

Defer fc.       :noname f. 1 backspaces ; IS fc.
: z. ( z -- )
           zdup z0= IF  zdrop ." 0 "  exit  THEN
           fdup f0= IF  fdrop fc. space exit  THEN   fswap
           fdup f0= IF    fdrop
                    ELSE  fc. ." +"  THEN
           fc. ." i " ;
: z.s ( z1 .. zn -- z1 .. zn )
	   zdepth 0 ?DO  i zpick zswap z>r z. zr>  LOOP ;

\ recognizer

: zliteral ( z -- ) fswap ]] fliteral fliteral [[ ; immediate
' noop ' zliteral dup >postponer
translate: translate-complex
\ alternative:
\ : translate-complex ( z -- ) fswap translate-float translate-float ;

:noname ( locals-nt -- )
    dup name>interpret >does-code [ comp' some-zlocal drop >does-code ]L =
    IF    name-compsem ['] zliteral compile,
    ELSE  defers locals-post,
    THEN ; is locals-post,

: rec-complex ( addr u -- z translate-complex | 0 ) \ gforth
    \G Complex numbers are always in the format a+bi, where a and b are
    \G floating point numbers including their signs
    2dup + 1- c@ 'i' = IF
	1- '+' $split 2swap prefix-number >r prefix-number r>
	2dup <> IF  fdrop  THEN  and
	['] translate-complex and
    ELSE  2drop 0  THEN ;

' rec-complex ' rec-float action-of forth-recognize +after
