\ environmental queries

\ wordlist constant environment-wordlist

Create environment-wordlist  wordlist drop

: environment? ( c-addr u -- false / ... true )
    environment-wordlist search-wordlist if
	execute true
    else
	false
    endif ;

environment-wordlist set-current
get-order environment-wordlist swap 1+ set-order

\ assumes that chars, cells and doubles use an integral number of aus

\ this should be computed in C as CHAR_BITS/sizeof(char),
\ but I don't know any machine with gcc where an au does not have 8 bits.
8 constant ADDRESS-UNIT-BITS
1 ADDRESS-UNIT-BITS chars lshift 1- constant MAX-CHAR
MAX-CHAR constant /COUNTED-STRING
ADDRESS-UNIT-BITS cells 2* 2 + constant /HOLD
&84 constant /PAD
true constant CORE
\ CORE-EXT?
1 -3 mod 0< constant FLOORED

1 ADDRESS-UNIT-BITS cells 1- lshift 1- constant MAX-N
-1 constant MAX-U

-1 MAX-N 2constant MAX-D
-1. 2constant MAX-UD

0 1 2constant gforth \ minor mayor version

\ !! RETURN-STACK-CELLS
\ !! STACK-CELLS

forth definitions
previous

