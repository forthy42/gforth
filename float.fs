\ High level floating point                            14jan94py

1 cells 4 = [IF]
' cells   Alias sfloats
' cell+   Alias sfloat+
' align   Alias sfalign
' aligned Alias sfaligned
[ELSE]
: sfloats  2* 2* ;
: sfloat+  4 + ;
: sfaligned ( addr -- addr' )  3 + -4 and ;
: sfalign ( -- )  here dup sfaligned swap ?DO  bl c,  LOOP ;
[THEN]

1 floats 8 = [IF]
' floats   Alias dfloats
' float+   Alias dfloat+
' falign   Alias dfalign
' faligned Alias dfaligned
[ELSE]
: dfloats  2* 2* 2* ;
: dfloat+  8 + ;
: dfaligned ( addr -- addr' )  7 + -8 and ;
: dfalign ( -- )  here dup dfaligned swap ?DO  bl c,  LOOP ;
[THEN]

: f, ( f -- )  here 1 floats allot f! ;

: fconstant  ( r -- )
    Create f,
    DOES> f@ ;

: fdepth  ( -- n )  f0 @ fp@ - [ 1 floats ] Literal / ;

: FLit ( -- r )  r> dup f@ float+ >r ;
: FLiteral ( r -- )
  BEGIN  here cell+ dup faligned <>  WHILE  postpone noop  REPEAT
  postpone FLit  f, ;  immediate

&15 Value precision
: set-precision  to precision ;

: scratch ( r -- addr len )
  pad precision - precision ;

: zeros ( n -- )   0 max 0 ?DO  '0 emit  LOOP ;

: -zeros ( addr u -- addr' u' )
  BEGIN  dup  WHILE  1- 2dup + c@ '0 <>  UNTIL  1+  THEN ;

: f$ ( f -- n )  scratch represent 0=
  IF  2drop  scratch 3 min type  rdrop  EXIT  THEN
  IF  '- emit  THEN ;

: f.  ( r -- )  f$ dup >r 0<
  IF    '0 emit
  ELSE  scratch r@ min type  r@ precision - zeros  THEN
  '. emit r@ negate zeros
  scratch r> 0 max /string 0 max -zeros type space ;
\ I'm afraid this does not really implement ansi semantics wrt precision.
\ Shouldn't precision indicate the number of places shown after the point?

: fe. ( r -- )  f$ 1- s>d 3 fm/mod 3 * >r 1+ >r
  scratch r@ min type '. emit  scratch r> /string type
  'E emit r> . ;

: fs. ( r -- )  f$ 1-
  scratch over c@ emit '. emit 1 /string type
  'E emit . ;

: sfnumber ( c-addr u -- r / )
    2dup >float
    IF
	2drop state @
	IF
	    postpone FLiteral
	THEN
    ELSE
	defers notfound
    THEN ;

' sfnumber IS notfound

: fvariable ( -- )
    Create 0e0 f, ;
    \ does> ( -- f-addr )

1e0 fasin 2e0 f* fconstant pi

: f2*  2e0 f* ;
: f2/  .5e0 f* ;
: 1/f  1e0 fswap f/ ;


\ We now have primitives for these, so we need not define them

\ : falog ( f -- 10^f )  [ 10e0 fln ] FLiteral f* fexp ;

\ : fsinh    fexpm1 fdup fdup 1e0 f+ f/ f+ f2/ ;
\ : fcosh    fexp fdup 1/f f+ f2/ ;
\ : ftanh    f2* fexpm1 fdup 2e0 f+ f/ ;

\ : fatanh   fdup f0< >r fabs 1e0 fover f- f/  f2* flnp1 f2/
\            r> IF  fnegate  THEN ;
\ : facosh   fdup fdup f* 1e0 f- fsqrt f+ fln ;
\ : fasinh   fdup fdup f* 1e0 f+ fsqrt f/ fatanh ;

: f~ ( f1 f2 f3 -- flag )  fdup f0= IF  fdrop f= EXIT  THEN
  fdup f0>   IF  frot frot f- fabs fswap
           ELSE  fnegate frot frot fover fabs fover fabs f+ frot frot
                 f- fabs frot frot f*  THEN  f< ;

: f.s  ." <" fdepth 0 .r ." > " fdepth 0 max maxdepth-.s @ min dup 0 
  ?DO  dup i - 1- floats fp@ + f@ f.  LOOP  drop ; 
