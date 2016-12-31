\ implementation of Forth 200x structures

\ Copyright (C) 2007,2012,2014,2015,2016 Free Software Foundation, Inc.

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

: standard+field ( n1 n2 "name" -- n3 ) \ X:structures plus-field
    over if
        (field) over , dup ,
    else
        create dozerofield over , dup ,
    then
    + ;

: (sizeof) ( "name" -- size ) ' >body cell+ @ ;
: [sizeof] ( "name" -- size )
    (sizeof) postpone Literal ; immediate compile-only
' (sizeof) comp' [sizeof] drop
interpret/compile: sizeof ( "field" -- size )

Defer +field
\ A number of things have field-like structure, but not
\ exactly field-like behavior.  Objects, locals, etc.
\ Allow them to plug into +field.

: standard:field ( -- )
    \g set +field to standard behavior
    ['] standard+field IS +field ;

standard:field

: extend-structure ( n "name" -- struct-sys n ) \ Gforth
    \g extend an existing structure
    standard:field >r 0 value lastxt >body r> ;

: begin-structure ( "name" -- struct-sys 0 ) \ X:structures
    0 extend-structure ;

: end-structure ( struct-sys +n -- ) \ X:structures
    swap ! ;

: cfield: ( u1 "name" -- u2 ) \ X:structures
    1 +field ;

: wfield: ( u1 "name" -- u2 ) \ X:structures
    1 + -2 and 2 +field ;

: lfield: ( u1 "name" -- u2 ) \ X:structures
    3 + -4 and 4 +field ;

: xfield: ( offset -- offset' )
    7 + -8 and 8 +field ;

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
