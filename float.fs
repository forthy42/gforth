\ High level floating point                            14jan94py

\ Copyright (C) 1995,1997,2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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

\ 1 cells 4 = [IF]
\ ' cells   Alias sfloats
\ ' cell+   Alias sfloat+
\ ' align   Alias sfalign
\ ' aligned Alias sfaligned
\ [ELSE]
\ : sfloats  2* 2* ;
\ : sfloat+  4 + ;
\ : sfaligned ( addr -- addr' )  3 + -4 and ;
\ : sfalign ( -- )  here dup sfaligned swap ?DO  bl c,  LOOP ;
\ [THEN]

\ 1 floats 8 = [IF]
\ ' floats   Alias dfloats
\ ' float+   Alias dfloat+
\ ' falign   Alias dfalign
\ ' faligned Alias dfaligned
\ [ELSE]
\ : dfloats  2* 2* 2* ;
\ : dfloat+  8 + ;
\ : dfaligned ( addr -- addr' )  7 + -8 and ;
\ : dfalign ( -- )  here dup dfaligned swap ?DO  bl c,  LOOP ;
\ [THEN]

: sfalign ( -- ) \ float-ext s-f-align
    \G If the data-space pointer is not single-float-aligned, reserve
    \G enough space to align it.
    here dup sfaligned swap ?DO  bl c,  LOOP ;
: dfalign ( -- ) \ float-ext d-f-align
    \G If the data-space pointer is not double-float-aligned, reserve
    \G enough space to align it.
    here dup dfaligned swap ?DO  bl c,  LOOP ;

(Field) sfloat+ ( sf-addr1 -- sf-addr2 ) \ float-ext s-float-plus
\G @code{1 sfloats +}.
    1 sfloats ,

(Field) dfloat+ ( df-addr1 -- df-addr2 ) \ float-ext d-float-plus
\G @code{1 dfloats +}.
    1 dfloats ,
    
: f, ( f -- ) \ gforth
    \G Reserve data space for one floating-point number and store
    \G @i{f} in the space.
    here 1 floats allot f! ;

: fconstant  ( r "name" -- ) \ float f-constant
    Create f,
DOES> ( -- r )
    f@ ;

: fdepth ( -- +n ) \ float f-depth
    \G @i{+n} is the current number of (floating-point) values on the
    \G floating-point stack.
    fp0 @ fp@ - [ 1 floats ] Literal / ;

: FLiteral ( compilation r -- ; run-time -- r ) \ float f-literal
    \G Compile appropriate code such that, at run-time, @i{r} is placed
    \G on the (floating-point) stack. Interpretation semantics are undefined.
    BEGIN  here cell+ cell+ dup faligned <>  WHILE  postpone noop  REPEAT
    postpone ahead here >r f, postpone then
    r> postpone literal postpone f@ ;  immediate

&15 Value precision ( -- u ) \ float-ext
\G @i{u} is the number of significant digits currently used by
\G @code{F.} @code{FE.} and @code{FS.} 
: set-precision ( u -- ) \ float-ext
    \G Set the number of significant digits currently used by
    \G @code{F.} @code{FE.} and @code{FS.} to @i{u}.
    to precision ;

: scratch ( r -- addr len )
  pad precision - precision ;

: zeros ( n -- )   0 max 0 ?DO  '0 emit  LOOP ;

: -zeros ( addr u -- addr' u' )
  BEGIN  dup  WHILE  1- 2dup + c@ '0 <>  UNTIL  1+  THEN ;

: f$ ( f -- n )  scratch represent 0=
  IF  2drop  scratch 3 min type  rdrop  EXIT  THEN
  IF  '- emit  THEN ;

: f.  ( r -- ) \ float-ext f-dot
\G Display (the floating-point number) @i{r} without exponent,
\G followed by a space.
  f$ dup >r 0<=
  IF    '0 emit
  ELSE  scratch r@ min type  r@ precision - zeros  THEN
  '. emit r@ negate zeros
  scratch r> 0 max /string 0 max -zeros type space ;
\ I'm afraid this does not really implement ansi semantics wrt precision.
\ Shouldn't precision indicate the number of places shown after the point?

\ Why do you think so? ANS Forth appears ambiguous on this point. -anton.

: fe. ( r -- ) \ float-ext f-e-dot
\G Display @i{r} using engineering notation (with exponent dividable
\G by 3), followed by a space.
  f$ 1- s>d 3 fm/mod 3 * >r 1+ >r
  scratch r@ tuck min tuck - >r type r> zeros
  '. emit scratch r> /string type
  'E emit r> . ;

: fs. ( r -- ) \ float-ext f-s-dot
\G Display @i{r} using scientific notation (with exponent), followed
\G by a space.
  f$ 1-
  scratch over c@ emit '. emit 1 /string type
  'E emit . ;

: sfnumber ( c-addr u -- r true | false )
    2dup [CHAR] e scan ( c-addr u c-addr2 u2 )
    dup 0=
    IF
	2drop 2dup [CHAR] E scan ( c-addr u c-addr3 u3 )
    THEN
    nip
    IF
	>float
    ELSE
	2drop false
    THEN ;

[ifundef] compiler-notfound1
defer compiler-notfound1
' no.extensions IS compiler-notfound1

:noname compiler-notfound1 execute ; is compiler-notfound

defer interpreter-notfound1
' no.extensions IS interpreter-notfound1

:noname interpreter-notfound1 execute ; is interpreter-notfound
[then]

:noname ( c-addr u -- ... xt )
    2dup sfnumber
    IF
	2drop [comp'] FLiteral
    ELSE
	defers compiler-notfound1
    ENDIF ;
IS compiler-notfound1

:noname ( c-addr u -- ... xt )
    2dup sfnumber
    IF
	2drop ['] noop
    ELSE
	defers interpreter-notfound1
    ENDIF ;
IS interpreter-notfound1

: fvariable ( "name" -- ) \ float f-variable
    Create 0.0E0 f, ;
    \ does> ( -- f-addr )

1.0e0 fasin 2.0e0 f* fconstant pi ( -- r ) \ gforth
\G @code{Fconstant} -- @i{r} is the value pi; the ratio of a circle's area
\G to its diameter.

: f2* ( r1 -- r2 ) \ gforth
    \G Multiply @i{r1} by 2.0e0
    2.0e0 f* ;

: f2/ ( r1 -- r2 ) \ gforth
    \G Multiply @i{r1} by 0.5e0
    0.5e0 f* ;

: 1/f ( r1 -- r2 ) \ gforth
    \G Divide 1.0e0 by @i{r1}.
    1.0e0 fswap f/ ;

get-current environment-wordlist set-current
1.7976931348623157e308 FConstant max-float
set-current

\ We now have primitives for these, so we need not define them

\ : falog ( f -- 10^f )  [ 10.0e0 fln ] FLiteral f* fexp ;

\ : fsinh    fexpm1 fdup fdup 1.0e0 f+ f/ f+ f2/ ;
\ : fcosh    fexp fdup 1/f f+ f2/ ;
\ : ftanh    f2* fexpm1 fdup 2.0e0 f+ f/ ;

\ : fatanh   fdup f0< >r fabs 1.0e0 fover f- f/  f2* flnp1 f2/
\            r> IF  fnegate  THEN ;
\ : facosh   fdup fdup f* 1.0e0 f- fsqrt f+ fln ;
\ : fasinh   fdup fdup f* 1.0e0 f+ fsqrt f/ fatanh ;

: f~abs ( r1 r2 r3 -- flag ) \ gforth
    \G Approximate equality with absolute error: |r1-r2|<r3.
    frot frot f- fabs fswap f< ;

: f~rel ( r1 r2 r3 -- flag ) \ gforth
    \G Approximate equality with relative error: |r1-r2|<r3*|r1+r2|.
	frot frot fover fabs fover fabs f+ frot frot
	f- fabs frot frot f* f< ;

: f~ ( r1 r2 r3 -- flag ) \ float-ext f-proximate
    \G ANS Forth medley for comparing r1 and r2 for equality: r3>0:
    \G @code{f~abs}; r3=0: bitwise comparison; r3<0: @code{fnegate f~rel}.
    fdup f0=
    IF \ bitwise comparison
	fp@ float+ 1 floats over float+ over str=
	fdrop fdrop fdrop
	EXIT
    THEN
    fdup f0>
    IF
	f~abs
    ELSE
	fnegate f~rel
    THEN ;

\ proposals from Krishna Myeni in <cjsp2d$47l$1@ngspool-d02.news.aol.com>
\ not sure if they are a good idea

: FTRUNC ( r1 -- r2 )
    \ round towards 0
    \ !! should be implemented properly
    F>D D>F ;

: FMOD ( r1 r2 -- r )
    \ remainder of r1/r2
    FOVER FOVER F/ ftrunc F* F- ;
