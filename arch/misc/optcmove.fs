\ optimized cmove to use cell wide @ and !
\ (C) Jens Wilke, PUBLIC DOMAIN

: (cmove)  ( c_from c_to u -- )
 bounds ?DO  dup c@ I c! 1+  LOOP  drop ;

: cmove ( c_from c_to u -- )
  \ check whether optimization makes sense
  dup 20 u< IF (cmove) EXIT THEN
  over [ 1 cells 1- ] Literal and >r
  rot dup [ 1 cells 1- ] Literal and
  dup r> <> 
  \ relative cell offset is not identical fallback to (cmove)
  IF drop -rot (cmove) EXIT THEN
  ?dup 
  IF    ( c_to u c_from u2 )
        [ 1 cells ] Literal swap -
        >r -rot r> tuck - >r >r 2dup r> (cmove) r>
  ELSE  -rot
  THEN
  >r aligned swap aligned swap r>
  2dup dup [ 1 cells 1- ] Literal and dup >r - + >r
  [ 1 cells 2 = [IF] ]
    1
  [ [THEN] ]
  [ 1 cells 4 = [IF] ]
    2
  [ [THEN] ]
  [ 1 cells 8 = [IF] ]
    3
  [ [THEN] ]
  tuck rshift -rot rshift swap bounds
  DO dup @ I cells ! cell+ LOOP
  r> r> (cmove) ;
