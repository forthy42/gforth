\ memory access words
\ this file is in the public domain, it's just the reference
\ implementation. Assumes byte addressed access, power of 2 bytes per cell

\ helper words for conditional compile

wordlist constant redef:

get-current redef: set-current

: : ( "name" -- ) \ conditional define
    >in @ >r postpone [defined]
    IF  r> drop BEGIN  ';' parse + source + =  WHILE  refill 0= UNTIL  THEN
    ELSE  r> >in ! :  THEN ;

: synonym ( "name" "oldname" -- ) \ conditional synonym
    >in @ >r postpone [defined]
    IF  r> drop parse-name 2drop
    ELSE  r> >in ! synonym  THEN ;

set-current

get-order redef: swap 1+ set-order

: noop ( -- ) ; \ does nothing

\ helper words for byte swap

: w>< ( w -- w' )
    dup #8 rshift $FF and swap $FF and 8 lshift or ;
1 cells #4 chars < 0= [IF]
    : l>< ( l -- l' )
	dup #16 rshift $FFFF and w>< swap
	$FFFF and w>< #16 lshift or ;
    1 cells #8 chars < 0= [IF]
	: x>< ( x -- x' )
	    dup #32 rshift $FFFFFFFF and l>< swap
	    $FFFFFFFF and l>< #32 lshift or ;
    [THEN]
[THEN]

\ helper words for combining/splitting; endian-dependent byte swap

1 pad ! pad c@ 1 = [IF] \ little endian
    : cc>w ( b1 b2 -- w )      $FF and #8 lshift swap $FF and or ;
    : w>cc ( w -- b1 b2 )  dup $FF and swap #8 rshift $FF and swap ;
    synonym wbe w><
    synonym wle noop
    1 cells #4 chars < 0= [IF]
	: ww>l ( w1 w2 -- l )  #16 lshift or ;
	: l>ww ( l -- w1 w2 )  dup     $FFFF and swap #16 rshift swap ;
	1 cells #8 chars < 0= [IF]
	    : ll>x ( l1 l2 -- x )  #32 lshift or ;
	    : x>ll ( x -- l1 l2 )  dup $FFFFFFFF and swap #32 rshift swap ;
	    synonym xbe x><
	    synonym xle noop
	[THEN]
	synonym lbe l><
	synonym lle noop
    [THEN]
[ELSE] \ big endian
    : cc>w ( b1 b2 -- w )      $FF and swap $FF and #8 lshift or ;
    : w>cc ( w -- b1 b2 )  dup $FF and swap #8 rshift $FF and ;
    synonym wle w><
    synonym wbe noop
    1 cells #4 chars < 0= [IF]
	: ww>l ( w1 w2 -- l )  swap #16 lshift or ;
	: l>ww ( l -- w1 w2 )  dup     $FFFF and swap #16 rshift ;
	1 cells #8 chars < 0= [IF]
	    : ll>x ( l1 l2 -- x )  swap #32 lshift or ;
	    : x>ll ( x -- l1 l2 )  dup $FFFFFFFF and swap #32 rshift ;
	    synonym xle x><
	    synonym xbe noop
	[THEN]
	synonym lle l><
	synonym lbe noop
    [THEN]
[THEN]

\ actual words for memory access

: w@ ( addr -- w )  dup c@ swap char+ c@ cc>w ;
: w! ( w addr -- )  >r w>cc r@ c! r> char+ c! ;
: w, ( w -- )  here w!  2 chars allot ;
1 cells #4 chars < 0= [IF]
    : l@ ( addr -- l )  dup w@ swap 2 chars + w@ ww>l ;
    : l! ( l addr -- )  >r l>ww r@ w! r> 2 chars + w! ;
    : l, ( l -- )  here l!  4 chars allot ;
[THEN]
1 cells #8 chars < 0= [IF]
    : x@ ( addr -- x )  dup l@ swap 4 chars + l@ ll>x ;
    : x! ( x addr -- )  >r x>ll r@ l! r> 4 chars + l! ;
    : x, ( x -- )  here x!  8 chars allot ;
[THEN]

\ alignments

: *aligned ( addr n -- addr' )  tuck 1- + swap negate and ;
: *align ( n -- )  here swap *aligned dp ! ;

: walign ( -- )  2 chars *align ;
: waligned ( addr -- addr' )  2 chars *aligned ;
: wfield: ( offset -- offset' ) waligned 2 chars +field ;
1 cells #4 chars < 0= [IF]
    : lalign ( -- )  4 *align ;
    : laligned ( addr -- addr' )  4 chars *aligned ;
    : lfield: ( offset -- offset' ) laligned 4 chars +field ;
[THEN]
1 cells #8 chars < 0= [IF]
    : xalign ( -- )  8 *align ;
    : xaligned ( addr -- addr' )  8 chars *aligned ;
    : xfield: ( offset -- offset' ) xaligned 8 chars +field ;
[THEN]

\ actual words for sign extension

: mask>s ( x u -- n )
    over and negate or ;
: c>s ( c -- n )  $80 mask>s ;
1 cells 4 chars < 0= [IF]
    : w>s ( w -- n )  $8000 mask>s ;
    1 cells 8 chars < 0= [IF]
	: l>s ( l -- n )  $80000000 mask>s ;
    [THEN]
[THEN]

previous
