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

: nalign ( addr1 n -- addr2 )
\ addr2 is the aligned version of addr1 wrt the alignment size n
 1- tuck +  swap invert and ;

: field ( offset1 align1 size align -- offset2 align2 )
    create
    >r rot r@ nalign  dup ,  ( align1 size offset )
    + swap r> nalign
does> ( addr1 -- addr2 )
    @ + ;

: end-struct ( size align -- )
 2constant ;

0 1 chars end-struct struct

\ I don't really like the "type:" syntax. Any other ideas? - anton
\ Also, this seems to be somewhat general. It probably belongs to some
\ other place
: cells: ( n -- size align )
    cells 1 cells ;

: doubles: ( n -- size align )
    2* cells 1 cells ;

: chars: ( n -- size align )
    chars 1 chars ;

: floats: ( n -- size align )
    floats 1 floats ;

: dfloats: ( n -- size align )
    dfloats 1 dfloats ;

: sfloats: ( n -- size align )
    sfloats 1 sfloats ;

: struct-align ( size align -- )
    here swap nalign here - allot
    drop ;

: struct-allot ( size align -- addr )
    over swap struct-align
    here swap allot ;

: struct-allocate ( size align -- addr ior )
    drop allocate ;
