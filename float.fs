\ High level floating point                            14jan94py

\ Copyright (C) 1995,1997 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

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
    here dup sfaligned swap ?DO  bl c,  LOOP ;
: dfalign ( -- ) \ float-ext d-f-align
    here dup dfaligned swap ?DO  bl c,  LOOP ;

1 sfloats constant sfloat+ ( sf-addr1 -- sf-addr2 ) \ float-ext s-float-plus
dofield: lastxt code-address! \ change the constant into a field

1 dfloats constant dfloat+ ( df-addr1 -- df-addr2 ) \ float-ext d-float-plus
dofield: lastxt code-address! \ change the constant into a field

: f, ( f -- )  here 1 floats allot f! ;

: fconstant  ( r "name" -- ) \ float
    Create f,
DOES> ( -- r )
    f@ ;

: fdepth ( -- +n ) \ floating f-depth
    \G @var{+n} is the current number of (floating-point) values on the
    \G floating-point stack.
    fp0 @ fp@ - [ 1 floats ] Literal / ;

: FLit ( -- r )  r> dup f@ float+ >r ;
: FLiteral ( compilation r -- ; run-time -- r ) \ float
    \G Compile appropriate code such that, at run-time, @var{r} is placed
    \G on the (floating-point) stack. Interpretation semantics are undefined.
    BEGIN  here cell+ dup faligned <>  WHILE  postpone noop  REPEAT
    postpone FLit  f, ;  immediate

&15 Value precision ( -- u ) \ floating-ext
\G @var{u} is the number of significant digits currently used by
\G @code{F.} @code{FE.} and @code{FS.} 
: set-precision ( u -- ) \ floating-ext
    \G Set the number of significant digits currently used by
    \G @code{F.} @code{FE.} and @code{FS.} to @var{u}.
    to precision ;

: scratch ( r -- addr len )
  pad precision - precision ;

: zeros ( n -- )   0 max 0 ?DO  '0 emit  LOOP ;

: -zeros ( addr u -- addr' u' )
  BEGIN  dup  WHILE  1- 2dup + c@ '0 <>  UNTIL  1+  THEN ;

: f$ ( f -- n )  scratch represent 0=
  IF  2drop  scratch 3 min type  rdrop  EXIT  THEN
  IF  '- emit  THEN ;

: f.  ( r -- ) \ floating-ext f-dot
\G Display (the floating-point number) @var{r} using fixed-point notation,
\G followed by a space.
  f$ dup >r 0<
  IF    '0 emit
  ELSE  scratch r@ min type  r@ precision - zeros  THEN
  '. emit r@ negate zeros
  scratch r> 0 max /string 0 max -zeros type space ;
\ I'm afraid this does not really implement ansi semantics wrt precision.
\ Shouldn't precision indicate the number of places shown after the point?

: fe. ( r -- ) \ floating-ext f-e-dot
\G Display @var{r} using engineering notation, followed by a space.
  f$ 1- s>d 3 fm/mod 3 * >r 1+ >r
  scratch r@ min type '. emit  scratch r> /string type
  'E emit r> . ;

: fs. ( r -- ) \ floating-ext f-s-dot
\G Display @var{r} using scientific notation, followed by a space.
  f$ 1-
  scratch over c@ emit '. emit 1 /string type
  'E emit . ;

require debugs.fs

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

:noname ( c-addr u -- )
    2dup sfnumber
    IF
	2drop POSTPONE FLiteral
    ELSE
	defers compiler-notfound
    ENDIF ;
IS compiler-notfound

:noname ( c-addr u -- r )
    2dup sfnumber
    IF
	2drop
    ELSE
	defers interpreter-notfound
    ENDIF ;
IS interpreter-notfound

: fvariable ( "name" -- ) \ float
    Create 0.0E0 f, ;
    \ does> ( -- f-addr )

1.0e0 fasin 2.0e0 f* fconstant pi ( -- r ) \ gforth
\G FCONSTANT: @var{r} is the value pi; the ratio of a circle's area
\G to its diameter.

: f2* ( r1 -- r2 ) \ gforth
    \G Multiply @var{r1} by 2.0e0
    2.0e0 f* ;

: f2/ ( r1 -- r2 ) \ gforth
    \G Multiply @var{r1} by 0.5e0
    0.5e0 f* ;

: 1/f ( r1 -- r2 ) \ gforth
    \G Divide 1.0e0 by @var{r1}.
    1.0e0 fswap f/ ;


\ We now have primitives for these, so we need not define them

\ : falog ( f -- 10^f )  [ 10.0e0 fln ] FLiteral f* fexp ;

\ : fsinh    fexpm1 fdup fdup 1.0e0 f+ f/ f+ f2/ ;
\ : fcosh    fexp fdup 1/f f+ f2/ ;
\ : ftanh    f2* fexpm1 fdup 2.0e0 f+ f/ ;

\ : fatanh   fdup f0< >r fabs 1.0e0 fover f- f/  f2* flnp1 f2/
\            r> IF  fnegate  THEN ;
\ : facosh   fdup fdup f* 1.0e0 f- fsqrt f+ fln ;
\ : fasinh   fdup fdup f* 1.0e0 f+ fsqrt f/ fatanh ;

\ !! factor out parts
: f~ ( f1 f2 f3 -- flag ) \ float-ext
    fdup f0=
    IF
	fdrop f= EXIT
    THEN
    fdup f0>
    IF
	frot frot f- fabs fswap
    ELSE
	fnegate frot frot fover fabs fover fabs f+ frot frot
	f- fabs frot frot f*
    THEN
    f< ;

: f.s ( -- ) \ gforth f-dot-s
    \G Display the number of items on the floating-point stack,
    \G followed by a list of the items; TOS is the right-most item.
    ." <" fdepth 0 .r ." > " fdepth 0 max maxdepth-.s @ min dup 0 
    ?DO  dup i - 1- floats fp@ + f@ f.  LOOP  drop ; 
