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
1 cells #4 < 0= [IF]
    : l>< ( l -- l' )
	dup #16 rshift $FFFF and w>< swap
	$FFFF and w>< #16 lshift or ;
    : ld>< ( ld -- ld' )  drop l>< 0 ;
    1 cells #8 < 0= [IF]
	: x>< ( x -- x' )
	    dup #32 rshift $FFFFFFFF and l>< swap
	    $FFFFFFFF and l>< #32 lshift or ;
	: xd>< ( xd -- xd' )  drop x>< 0 ;
    [ELSE]
	: xd>< ( x -- x' ) l>< swap l>< ;
    [THEN]
    : xq>< ( xd -- xd' )  2drop xd>< $0. ;
[ELSE]
    : ld>< ( ld -- ld' ) w>< swap w>< ;
    : xq>< ( xq -- xq' ) l>< 2swap l>< ;
[THEN]

\ helper words for combining/splitting; endian-dependent byte swap

1 pad ! pad c@ 1 = [IF] \ little endian
    : cc>w ( b1 b2 -- w )  #8 lshift or ;
    : w>cc ( w -- b1 b2 )  dup       $FF and swap  #8 rshift ;
    synonym wbe w><
    synonym wle noop
    1 cells #4 < 0= [IF]
	: ww>l ( w1 w2 -- l )  #16 lshift or ;
	: l>ww ( l -- w1 w2 )  dup     $FFFF and swap #16 rshift ;
	1 cells #8 < 0= [IF]
	    : ll>x ( l1 l2 -- x )  #32 lshift or ;
	    : x>ll ( x -- l1 l2 )  dup $FFFFFFFF and swap #32 rshift ;
	    synonym xbe x><
	    synonym xle noop
	[ELSE]
	    synonym ll>xd noop ( l1 l2 -- dx )
	    synonym xd>ll noop ( l1 l2 -- dx )
	[THEN]
	synonym lbe l><
	synonym lle noop
	synonym xdbe xd><
	synonym xdle noop
	synonym ldbe ld><
	synonym ldle noop
	synonym xqbe xq><
	synonym xqle noop
    [ELSE]
	synonym ww>dl noop ( l1 l2 -- dx )
	synonym dl>ww noop ( dx -- l1 l2 )
	synonym ll>qx noop ( dl1 dl2 -- qx )
	synonym qx>ll noop ( qx -- dl1 dl2 )
	synonym ldbe ld><
	synonym ldle noop
	synonym xqbe xq><
	synonym xqle noop
    [THEN]
[ELSE] \ big endian
    : cc>w ( b1 b2 -- w )  swap #8 lshift or ;
    : w>cc ( w -- b1 b2 )  dup       $FF and swap  #8 rshift swap ;
    synonym wle w><
    synonym wbe noop
    1 cells #4 < 0= [IF]
	: ww>l ( w1 w2 -- l )  swap #16 lshift or ;
	: l>ww ( l -- w1 w2 )  dup     $FFFF and swap #16 rshift swap ;
	1 cells #8 < 0= [IF]
	    : ll>x ( l1 l2 -- x )  swap #32 lshift or ;
	    : x>ll ( x -- l1 l2 )  dup $FFFFFFFF and swap #32 rshift swap ;
	    synonym xle x><
	    synonym xbe noop
	[ELSE]
	    synonym ll>xd swap ( l1 l2 -- dx )
	    synonym xd>ll swap ( l1 l2 -- dx )
	[THEN]
	synonym lle l><
	synonym lbe noop
	synonym xdle xd><
	synonym xdbe noop
	synonym ldle ld><
	synonym ldbe noop
	synonym xqle xq><
	synonym xqbe noop
    [ELSE]
	synonym ww>ld swap ( l1 l2 -- dx )
	synonym ld>ww swap ( l1 l2 -- dx )
	synonym ll>xq 2swap ( dl1 dl2 -- qx )
	synonym xq>ll 2swap ( dl1 dl2 -- qx )
	synonym ldle ld><
	synonym ldbe noop
	synonym xqle xq><
	synonym xqbe noop
    [THEN]
[THEN]

\ actual words for memory access

: w@ ( addr -- w )  dup c@ swap 1+ c@ cc>w ;
: w! ( w addr -- )  >r w>cc r@ c! r> 1+ c! ;
: w, ( w -- )  here w!  2 allot ;
1 cells #4 < 0= [IF]
    : l@ ( addr -- l )  dup w@ swap 2 + w@ ww>l ;
    : l! ( l addr -- )  >r l>ww r@ w! r> 2 + w! ;
    : l, ( l -- )  here l!  4 allot ;
[ELSE]
    : ld@ ( addr -- dl )  dup w@ swap 2 + w@ ww>ld ;
    : ld! ( dl addr -- )  >r ld>ww r@ w! r> 2 + w! ;
    : ld, ( dl -- )  here ld!  4 allot ;
[THEN]
1 cells #8 < 0= [IF]
    : x@ ( addr -- x )  dup l@ swap 4 + l@ ll>x ;
    : x! ( x addr -- )  >r x>ll r@ l! r> 4 + l! ;
    : x, ( x -- )  here x!  8 allot ;
[ELSE] 1 cells #4 < 0= [IF]
	: xd@ ( addr -- xd )  dup l@ swap 4 + l@ ll>xd ;
	: xd! ( xd addr -- )  >r xd>ll r@ l! r> 4 + l! ;
	: xd, ( xd -- )  here xd!  8 allot ;
    [ELSE]
	: xq@ ( addr -- x )  dup ld@ swap 4 + ld@ ll>xq ;
	: xq! ( x addr -- )  >r xq>ll r@ ld! r> 4 + ld! ;
	: xq, ( xq -- )  here xq!  8 allot ;
    [THEN]
[THEN]

\ alignments

: *aligned ( addr n -- addr' )  tuck 1- + swap negate and ;
: *align ( n -- )  here swap *aligned dp ! ;

: walign ( -- )  2 *align ;
: waligned ( addr -- addr' )  2 *aligned ;
: wfield: ( offset -- offset' ) waligned 2 +field ;
: lalign ( -- )  4 *align ;
: laligned ( addr -- addr' )  4 *aligned ;
: lfield: ( offset -- offset' ) laligned 4 +field ;
: xalign ( -- )  8 *align ;
: xaligned ( addr -- addr' )  8 *aligned ;
: xfield: ( offset -- offset' ) xaligned 8 +field ;

\ actual words for sign extension

: mask>s ( x u -- n )
    over and negate or ;
: c>s ( c -- n )  $80 mask>s ;
1 cells 4 < 0= [IF]
    : w>s ( w -- n )  $8000 mask>s ;
    1 cells 8 < 0= [IF]
	: l>s ( l -- n )  $80000000 mask>s ;
    [THEN]
[THEN]

\ compatibility with systems with smaller word size

: xd! ( xd addr -- ) nip x! ;
: xd@ ( addr -- xd ) x@ 0 ;
: xd, ( xd -- ) drop x, ;

: xq! ( xq addr -- ) nip nip xd! ;
: xq@ ( addr -- xq ) xd@ $0. ;
: xq, ( xq -- ) 2drop xd, ;

: ld! ( ld addr -- ) nip l! ;
: ld@ ( addr -- ld ) l@ 0 ;
: ld, ( ld -- ) drop l, ;

: xdle  drop xle 0 ;
: xdbe  drop xbe 0 ;

: ldle  drop lle 0 ;
: ldbe  drop lbe 0 ;

: xqle  2drop xdle #0. ;
: xqbe  2drop xdbe #0. ;

\ compatibility with systems with larger word size
\ beware of information loss!

: l@ ( addr -- l )  ld@ drop ;
: l! ( l addr -- )  0 swap ld! ;
: l, ( l -- ) 0 ld, ;

: xd@ ( addr -- x )  xq@ 2drop ;
: xd! ( x addr -- )  $0. 2swap xq! ;
: xd, ( x -- ) $0. xq, ;

: x@ ( addr -- x )  xd@ drop ;
: x! ( x addr -- )  0 swap xd! ;
: x, ( x -- ) 0 xd, ;

previous

\ tests

[defined] test-it [IF]
Create readpad $01 c, $23 c, $45 c, $67 c, $89 c, $ab c, $cd c, $ef c,
$10 buffer: writepad
require test/ttester.fs
T{ readpad w@ wbe -> $0123 }T
T{ readpad w@ wle -> $2301 }T
1 cells 4 < 0= [IF]
    T{ readpad l@ lbe -> $01234567 }T
    T{ readpad l@ lle -> $67452301 }T
    1 cells 8 < 0= [IF]
	T{ readpad x@ xbe -> $0123456789ABCDEF }T
	T{ readpad x@ xle -> $EFCDAB8967452301 }T
    [ELSE]
	T{ readpad xd@ xdbe -> $0123456789ABCDEF. }T
	T{ readpad xd@ xdle -> $EFCDAB8967452301. }T
    [THEN]    
    T{ readpad ld@ ldbe -> $01234567. }T
    T{ readpad ld@ ldle -> $67452301. }T
    1 cells 4 < 0= [IF]
	T{ readpad xq@ xqbe -> $0123456789ABCDEF. $0. }T
	T{ readpad xq@ xqle -> $EFCDAB8967452301. $0. }T
    [ELSE]
	T{ readpad xq@ xqbe -> $89ABCDEF. $01234567. }T
	T{ readpad xq@ xqle -> $67452301. $EFCDAB89. }T
    [THEN]
[ELSE]
    T{ readpad ld@ ldbe -> $01234567. }T
    T{ readpad ld@ ldle -> $67452301. }T
    T{ readpad xq@ xqbe -> $89ABCDEF. $01234567. }T
    T{ readpad xq@ xqle -> $67452301. $EFCDAB89. }T
[THEN]
[THEN]