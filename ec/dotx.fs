

\ for 16 bit machines only

[IFUNDEF] 8>>
: 8>> 8 rshift ;
[THEN]

: .digit
  $0f and
   dup 9 u>
   IF   
        [ char A char 9 - 1- ] Literal +
   THEN 
  '0 + (emit) ;

: .w
	dup 8>> 2/ 2/ 2/ 2/ .digit
	dup 8>> .digit
	dup 2/ 2/ 2/ 2/ .digit
	.digit ;

: .x 	
	dup 8>> 8>> .w .w $20 (emit) ;

\ !! depth reibauen

: .sx
  \ SP@ SP0 @ swap - 2/ 
  depth
  dup '< emit .x '> emit dup
  0 ?DO dup pick .x 1- LOOP drop ;
