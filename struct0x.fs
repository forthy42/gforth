\ implementation of Forth 200x structures

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2007,2012,2014,2015,2016,2018,2019,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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

: standard+field ( n1 n2 "name" -- n3 )
    (field) over , dup , + ;

: (sizeof) ( "name" -- size ) ' >body cell+ @ ;
: [sizeof] ( "name" -- size )
    (sizeof) postpone Literal ; immediate compile-only
' (sizeof) comp' [sizeof] drop
interpret/compile: sizeof ( "field" -- size )

Defer +field ( noffset1 nsize "name" -- noffset2 ) \ facility-ext plus-field
\G Defining word; defines @i{name} @code{( addr1 -- addr2 )}, where
\G @i{addr2} is @i{addr1+noffset1}.  @i{noffset2} is
\G @i{noffset1+nsize}.

\ A number of things have field-like structure, but not
\ exactly field-like behavior.  Objects, locals, etc.
\ Allow them to plug into +field.

defer standard:field ( -- ) \ gforth-internal
\g set +field to standard behavior
:noname  ['] standard+field IS +field ; is standard:field

standard:field

: extend-structure ( n "name" -- struct-sys n ) \ gforth
    \g Start a new structure @i{name} as extension of an existing
    \g structure with size @i{n}.
    standard:field >r 0 value latestxt >body r> ;

: begin-structure ( "name" -- struct-sys 0 ) \ facility-ext
    \ Start a structure definition and call it @i{name}
    0 extend-structure ;

: end-structure ( struct-sys +n -- ) \ facility-ext
    \g end a structure started with @code{begin-structure}
    swap ! ;

: cfield: ( u1 "name" -- u2 ) \ facility-ext c-field-colon
    \g Define a char-sized field
    1 +field ;

: wfield: ( u1 "name" -- u2 ) \ gforth w-field-colon
    \g Define a naturally aligned field for a 16-bit value.
    1 + -2 and 2 +field ;

: lfield: ( u1 "name" -- u2 ) \ gforth l-field-colon
    \g Define a naturally aligned field for a 32-bit value.
    3 + -4 and 4 +field ;

: xfield: ( u1 "name" -- u2 ) \ gforth x-field-colon
    \g Define a naturally aligned field for a 64-bit-value.
    7 + -8 and 8 +field ;

: field: ( u1 "name" -- u2 ) \ facility-ext field-colon
    \g Define an aligned cell-sized field
    aligned cell +field ;

: 2field: ( u1 "name" -- u2 ) \ gforth two-field-colon
    \g Define an aligned double-cell-sized field
    aligned 2 cells +field ;

: ffield: ( u1 "name" -- u2 ) \ floating-ext f-field-colon
    \g Define a faligned float-sized field
    faligned 1 floats +field ;

: sffield: ( u1 "name" -- u2 ) \ floating-ext s-f-field-colon
    \g Define a sfaligned sfloat-sized field
    sfaligned 1 sfloats +field ;

: dffield: ( u1 "name" -- u2 ) \ floating-ext d-f-field-colon
    \g Define a dfaligned dfloat-sized field
    dfaligned 1 dfloats +field ;
