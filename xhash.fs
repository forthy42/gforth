\ a more secure hash using xorshift RNG

Variable seed
cell 4 = [IF]
: xorshift ( n -- n' )
    dup 1 lshift xor
    dup 3 rshift xor
    dup 10 lshift xor ;
[THEN]
cell 8 = [IF]
: xorshift ( n -- n' )
    dup 11 lshift xor
    dup 23 rshift xor
    dup 56 lshift xor ;
[THEN]
: rnd  seed @ xorshift dup seed ! ;

: xhash ( addr u -- hash )
    0 -rot bounds ?DO
	I c@ xor xorshift
    LOOP ;
