\ data structures (like C structs)

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.


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

\ This is an ANS Forth program with an environmental dependency on
\ alignments that are powers of 2 (rewrite nalign for other systems)
\ and with an environmental dependence on case insensitivity (convert
\ everything to upper case for state sensitive systems).

\ The program uses the following words
\ !!

: nalign ( addr1 n -- addr2 )
\ addr2 is the aligned version of addr1 wrt the alignment size n
 1- tuck +  swap invert and ;

: dofield ( -- )
does> ( name execution: addr1 -- addr2 )
    @ + ;

: dozerofield ( -- )
    immediate
does> ( name execution: -- )
    drop ;

: create-field ( offset1 align1 size align "name" -- offset2 align2 )
    create
    >r rot r@ nalign  dup ,  ( align1 size offset  R: align )
    + swap r> nalign ;

: field ( offset1 align1 size align "name" -- offset2 align2 )
    \ name execution: addr1 -- addr2
    3 pick >r \ this uglyness is just for optimizing with dozerofield
    create-field
    r>
    dup if
	dofield
    else
	dozerofield
    then ;

: end-struct ( size align -- )
    tuck nalign swap \ pad size to full alignment
    2constant ;

0 1 chars end-struct struct

\ I don't really like the "type:" syntax. Any other ideas? - anton
\ Also, this seems to be somewhat general. It probably belongs to some
\ other place
: cells: ( n -- size align )
    cells 1 aligned ;

: doubles: ( n -- size align )
    2* cells 1 aligned ;

: chars: ( n -- size align )
    chars 1 chars ;

: floats: ( n -- size align )
    floats 1 faligned ;

: dfloats: ( n -- size align )
    dfloats 1 dfaligned ;

: sfloats: ( n -- size align )
    sfloats 1 sfaligned ;

: struct-align ( size align -- )
    here swap nalign here - allot
    drop ;

: struct-allot ( size align -- addr )
    over swap struct-align
    here swap allot ;

: struct-allocate ( size align -- addr ior )
    drop allocate ;

: struct-alloc ( size align -- addr )
    struct-allocate throw ;
