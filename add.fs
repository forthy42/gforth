\ ADD.FS       Kernal additional things                20may93jaw

\ linked list primitive
: linked        here over @ a, swap ! ;

: discard       0 ?DO drop LOOP ;

