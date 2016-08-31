\ data structures (like C structs)

\ This file is in the public domain. NO WARRANTY.

\ This program uses the following words
\ from CORE :
\ : 1- + swap invert and ; DOES> @ immediate drop Create rot dup , >r
\ r> IF ELSE THEN over chars aligned cells 2* here - allot
\ from CORE-EXT :
\ tuck pick nip 
\ from BLOCK-EXT :
\ \ 
\ from DOUBLE :
\ 2Constant 
\ from EXCEPTION :
\ throw 
\ from FILE :
\ ( 
\ from FLOAT :
\ faligned floats 
\ from FLOAT-EXT :
\ dfaligned dfloats sfaligned sfloats 
\ from MEMORY :
\ allocate 

: naligned ( addr1 n -- addr2 )
    \ addr2 is the aligned version of addr1 wrt the alignment size n
    1- tuck +  swap invert and ;

: nalign naligned ; \ old name, obsolete

[undefined] +field [if]
: +field ( n1 n2 "name" -- n3 ) \ Forth-2012
    create over , +	
  does> ( name execution: addr1 -- addr2 )
    @ + ;
[then]

: 0field ( "name" -- )
    \ "name" does nothing and compiles nothing (as a field with 0 offset should)
    create immediate
  does> ( name execution: -- )
    drop ;
  
: opt-+field ( n1 n2 "name" -- n3 )
    \ like +FIELD, but optimize the n1=0 case
    over if
	+field
    else
	0field +
    then ;

: field ( align1 offset1 align size "name" --  align2 offset2 )
    \ name execution: addr1 -- addr2
    >r tuck naligned r> opt-+field ( align1 align offset2 )
    >r naligned r> ;

: end-struct ( align size "name" -- )
    over naligned \ pad size to full alignment
    2constant ;

\ an empty struct
1 chars 0 end-struct struct

\ type descriptors, all ( -- align size )
1 aligned   1 cells   2constant cell%
1 chars     1 chars   2constant char%
1 faligned  1 floats  2constant float%
1 dfaligned 1 dfloats 2constant dfloat%
1 sfaligned 1 sfloats 2constant sfloat%
cell% 2*              2constant double%

\ memory allocation words
: %alignment ( align size -- align )
    drop ;

: %size ( align size -- size )
    nip ;

: %align ( align size -- )
    drop here swap nalign here - allot ;

: %allot ( align size -- addr )
    tuck %align
    here swap allot ;

: %allocate ( align size -- addr ior )
    nip allocate ;

: %alloc ( align size -- addr )
    %allocate throw ;
