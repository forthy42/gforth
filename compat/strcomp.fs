\ string comparisons

\ This file is in the public domain. NO WARRANTY.

\ Uses of COMPARE can be replaced with STR=, STRING-PREFIX?, and STR<
\ (and these can be implemented more efficiently and used more easily
\ than COMPARE).  See <news:2002Aug12.110229@a0.complang.tuwien.ac.at>
\ and following.

s" gforth" environment? [if]
    2drop defined str=
[else]
    \ : \G postpone \ ; immediate
    0
[then]
0= [if]

: str= ( c-addr1 u1 c-addr2 u2 -- f ) \ gforth str-equals
    \G Bytewise equality
    compare 0= ;

: string-prefix? ( c-addr1 u1 c-addr2 u2 -- f ) \ gforth string-prefix-question
    \G Is @var{c-addr2 u2} a prefix of @var{c-addr1 u1}?
    tuck 2>r umin 2r> str= ;

: string-suffix? ( c-addr1 u1 c-addr2 u2 -- f ) \ gforth string-suffix-question
    \G Is @var{c-addr2 u2} a suffix of @var{c-addr1 u1}?
    2>r tuck + swap r@ umin tuck - swap 2r> str= ;

: str< ( c-addr1 u1 c-addr2 u2 -- f ) \ gforth str-less-than
    \G Bytewise lexicographic comparison.
    compare 0< ;

[then]
