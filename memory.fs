\ memory access words                                25sep2014py

\ this file is in the public domain, it's just the reference
\ implementation. Assumes byte addressed access
\ and 16 bit minimum

: w>< ( w -- w' )
    dup 8 rshift $FF and swap $FF and 8 lshift or ;
1 cells 4 >= [IF]
    : l>< ( l -- l' )
	dup 16 rshift $FFFF and w>< swap
	$FFFF and w>< 16 lshift or ;
    1 cells 8 = [IF]
	: x>< ( x -- x' )
	    dup 32 rshift $FFFFFFFF and l>< swap
	    $FFFFFFFF and l>< 32 lshift or ;
	: xd>< ( xd -- xd' )  drop x>< 0 ;
    [ELSE]
	: xd>< ( x -- x' ) l>< swap l>< ;
    [THEN]
[ELSE]
    : ld>< ( ld -- ld' ) w>< swap w>< ;
    : xq>< ( xq -- xq' ) l>< 2swap l>< ;
[THEN]

pad off 1 pad ! pad c@ 1 = [IF]
    \ little endian
    : w@ ( addr -- w )  dup c@ swap 1+ c@ 8 lshift or ;
    : w! ( w addr -- )  2dup c! 1+ >r 8 rshift r> c! ;
    1 cells 4 >= [IF]
	: l@ ( addr -- l )  dup w@ swap 2 + w@ 16 lshift or ;
	: l! ( l addr -- )  2dup w! 2 + >r 16 rshift r> w! ;
    [THEN]
    1 cells 8 = [IF]
	: x@ ( addr -- x )  dup l@ swap 4 + l@ 32 lshift or ;
	: x! ( x addr -- )  2dup l! 4 + >r 32 rshift r> l! ;
    [ELSE]
	1 cells 4 = [IF]
	    : xd@ ( addr -- xd )  dup >r l@ r> 4 + l@ ;
	    : xd! ( xd addr -- )  tuck 4 + l! l! ;
	[ELSE]
	    : ld@ ( addr -- ld )  dup w@ swap 2 + w@ ;
	    : xq@ ( addr -- xq )  dup >r ld@ r> 4 + ld@ ;
	    : ld! ( ld addr -- )  2dup w! 2 + w! ;
	    : xq! ( xq addr -- )  dup >r ld! r> 4 + ld! ;
	[THEN]
    [THEN]
    synonym wbe w><
    synonym lbe l><
    synonym xbe x><
    synonym wle noop
    synonym lle noop
    synonym xle noop
[ELSE]
    : w@ ( addr -- w )  dup c@ 8 lshift swap 1+ c@ or ;
    : w! ( w addr -- )  2dup 1+ c! >r 8 rshift r> c! ;
    1 cells 4 >= [IF]
	: l@ ( addr -- l )  dup w@ 16 lshift swap 2 + w@ or ;
	: l! ( l addr -- )  2dup 2 + w! >r 16 rshift r> w! ;
    [THEN]
    1 cells 8 = [IF]
	: x@ ( addr -- x )  dup l@ 32 lshift swap 4 + l@ or ;
	: x! ( x addr -- )  2dup 4 + l! >r 32 rshift r> l! ;
    [ELSE]
	1 cells 4 = [IF]
	    : xd@ ( addr -- x )  dup >r 4 + l@ r> l@ ;
	    : xd! ( x addr -- )  tuck l! 4 + l! ;
	[ELSE]
	    : ld@ ( addr -- ld )  dup 2 + w@ swap w@ ;
	    : xq@ ( addr -- xq )  dup >r 4 + ld@ r> ld@ ;
	    : ld! ( ld addr -- )  2dup 2 + w! w! ;
	    : xq! ( xq addr -- )  dup >r 4 + ld! r> ld! ;
	[THEN]
    [THEN]
    synonym wle w><
    synonym lle l><
    synonym xle x><
    synonym wbe noop
    synonym lbe noop
    synonym xbe noop
[THEN]

: c>s ( c -- n )  dup       $80 and negate or ;
1 cells 4 >= [IF]
    : w>s ( w -- n )  dup     $8000 and negate or ;
    1 cells 8 >= [IF]
	: l>s ( l -- n )  dup $80000000 and negate or ;
    [THEN]
[THEN]
