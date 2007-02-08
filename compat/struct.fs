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

: dofield ( -- )
does> ( name execution: addr1 -- addr2 )
    @ + ;

: dozerofield ( -- )
    immediate
does> ( name execution: -- )
    drop ;

: create-field ( align1 offset1 align size "name" --  align2 offset2 )
    create swap rot over nalign dup , ( align1 size align offset )
    rot + >r nalign r> ;

: field ( align1 offset1 align size "name" --  align2 offset2 )
    \ name execution: addr1 -- addr2
    2 pick >r \ this uglyness is just for optimizing with dozerofield
    create-field
    r> if \ offset<>0
	dofield
    else
	dozerofield
    then ;

: end-struct ( align size "name" -- )
    over nalign \ pad size to full alignment
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
