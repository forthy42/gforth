\ $Id: struct.fs,v 1.1 1994-02-11 16:30:47 anton Exp $

\ Usage example:
\
\ struct
\     1 cells: field search-method
\     1 cells: field reveal-method
\ end-struct wordlist-map
\
\ The structure can then be extended in the following way
\ wordlist-map
\     1 cells: field enum-method
\ end-struct ext-wordlist-map \ with the fields search-method,...,enum-method

\ 2, 2constant and nalign should be somewhere else
: 2, ( w1 w2 -- )
 here 2 cells allot 2! ;

: 2constant ( w1 w2 -- )
    create 2,
    does>  ( -- w1 w2 )
	2@ ;

: nalign ( addr1 n -- addr2 )
\ addr2 is the aligned version of addr1 wrt the alignment size n
 1- tuck +  swap invert and ;

: create-field ( offset1 align1 size align -- offset2 align2 )
\ note: this version uses local variables
     create
	>r rot r@ nalign  dup ,  ( align1 size offset )
	+ swap r> nalign ;

: end-struct ( size align -- )
 2constant ;

0 1 chars end-struct struct

: field  ( offset1 align1 size align -- offset2 align2 )
    create-field
    does> ( addr1 -- addr2 )
	@ + ;

\ I don't really like the "type:" syntax. Any other ideas? - anton
\ Also, this seems to be somewhat general. It probably belongs to some
\ other place
: cells: ( n -- size align )
    cells cell ;

: doubles: ( n -- size align )
    2* cells cell ;

: chars: ( n -- size align )
    chars 1 chars ;

: floats: ( n -- size align )
    floats 1 floats ;

\ dfoats and sfloats is not yet defined
\ : dfloats: ( n -- size align )
\     dfloats 1 dfloats ;
\ 
\ : sfloats: ( n -- size align )
\     sfloats 1 sfloats ;