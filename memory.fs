\ memory access words                                25sep2014py

\ this file is in the public domain, it's just the reference
\ implementation. Assumes byte addressed access
\ and 32 bit minimum

pad off 1 pad ! pad c@ 1 = [IF]
    \ little endian
    : w@ ( addr -- w )  dup c@ swap 1+ c@ 8 lshift or ;
    : l@ ( addr -- l )  dup w@ swap 2 + w@ 16 lshift or ;
    1 cells 8 = [IF]
	: x@ ( addr -- x )  dup l@ swap 4 + l@ 32 lshift or ;
	: xd@ ( addr -- xd ) x@ 0 ;
    [ELSE]
	: xd@ ( addr -- x )  dup l@ swap 4 + l@ ;
    [THEN]
[ELSE]
    : w@ ( addr -- w )  dup c@ 8 lshift swap 1+ c@ or ;
    : l@ ( addr -- l )  dup w@ 16 lshift swap 2 + w@ or ;
    1 cells 8 = [IF]
	: x@ ( addr -- x )  dup l@ 32 lshift swap 4 + l@ or ;
	: xd@ ( addr -- xd ) x@ 0 ;
    [ELSE]
	: xd@ ( addr -- xd )  dup 4 + l@ swap l@ ;
    [THEN]
[THEN]

: w>< ( w -- w' )
    dup 8 rshift $FF and swap $FF and 8 lshift or ;
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

: c>s ( c -- n )  dup       $80 and negate or ;
: w>s ( w -- n )  dup     $8000 and negate or ;
: l>s ( l -- n )  dup $80000000 and negate or ;