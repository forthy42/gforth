\ implementation of Forth 200x structures

\ Copyright (C) 2007 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

: +field ( n1 n2 "name" -- n3 ) \ X:structures plus-field
    over if
        (field) over ,
    else
        create dozerofield
    then
    + ;

: begin-structure ( "name" -- struct-sys 0 ) \ X:structures
    0 value lastxt >body 0 ;

: end-structure ( struct-sys +n -- ) \ X:structures
    swap ! ;

: cfield: ( u1 "name" -- u2 ) \ X:structures
    1 +field ;

: field: ( u1 "name" -- u2 ) \ X:structures
    aligned cell +field ;

: 2field: ( u1 "name" -- u2 ) \ gforth
    aligned 2 cells +field ;

: ffield: ( u1 "name" -- u2 ) \ X:structures
    faligned 1 floats +field ;

: sffield: ( u1 "name" -- u2 ) \ X:structures
    sfaligned 1 sfloats +field ;

: dffield: ( u1 "name" -- u2 ) \ X:structures
    dfaligned 1 dfloats +field ;
