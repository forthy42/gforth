\ High level floating point                            14jan94py

: faligned ( addr -- f-addr )
  [ 1 floats 1- ] Literal + [ -1 floats ] Literal and ;

: falign ( -- )  here dup aligned swap
  ?DO  bl c,  LOOP ;

: f, ( f -- )  here 1 floats allot f! ;

: fconstant  ( r -- )
  Create falign f,
  DOES>  faligned f@ ;

: fvariable
  Create falign 0 f,
  DOES>  faligned ;

: fdepth  ( -- n )  f0 @ fp@ - [ 1 floats ] Literal / ;

: FLit ( -- r )  r> faligned dup f@ float+ >r ;
: FLiteral ( r -- )  postpone FLit  falign f, ;  immediate

&16 Value precision
: set-precision  to precision ;

: scratch ( r -- addr len )
  pad precision - precision ;

: zeros ( n -- )   0 max 0 ?DO  '0 emit  LOOP ;

: -zeros ( addr u -- addr' u' )
  BEGIN  dup  WHILE  1- 2dup + c@ '0 <>  UNTIL  1+  THEN ;

: f.  ( r -- )  scratch represent 0=
  IF  2drop  scratch 3 min type  EXIT  THEN
  IF  '- emit  THEN  dup >r 0<
  IF  '0 emit
  ELSE  scratch r@ min type  r@ precision - zeros  THEN
  '. emit r@ negate zeros
  scratch r> 0 max /string 0 max -zeros type space ;

: fe. ( r -- )  scratch represent 0=
  IF  2drop  scratch 3 min type  EXIT  THEN
  IF  '- emit  THEN  1- s>d 3 fm/mod 3 * >r 1+ >r
  scratch r@ min type '. emit  scratch r> /string type
  'E emit r> . ;

: fs. ( r -- )  scratch represent 0=
  IF  2drop  scratch 3 min type  EXIT  THEN
  IF  '- emit  THEN  1- >r
  scratch 1 min type '. emit  scratch 1 /string type
  'E emit r> . ;

: fnumber ( string -- r / )  dup count >float 0=
  IF  defers notfound  ELSE  drop  THEN ;

' fnumber IS notfound

1e0 fasin 2e0 f* fconstant pi
