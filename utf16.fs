\ simple tools to convert UTF-8 into UTF-16 and back

: .utf16 ( xchar -- )
    dup $10000 u>= IF
	$10000 - >r r@ 10 rshift $3FF and $D800 + recurse
	r> $3FF and $DC00 +
    THEN
    dup $FF and emit 8 rshift $FF and emit ;
: typex16 ( addr u -- )
    \ type UTF-8 string as UCS16 string
    bounds ?DO
	I xc@ .utf16
    I I' over - x-size +LOOP ;
: $utf16 ( addr1 u1 -- addr2 u2 )
    [: typex16 0 .utf16 ;] $tmp 2 - ;
: typex8 ( addr u -- )
    bounds ?DO
	I uw@ dup $D800 $DC00 within IF
	    $3FF and 10 lshift I 2 + uw@
	    $3FF and or xemit 4 \ no check for sanity
	ELSE  xemit 2  THEN
    +LOOP ;
: $utf8 ( addr1 u1 -- addr2 u2 )
    [: typex8 0 emit ;] $tmp 1- ;