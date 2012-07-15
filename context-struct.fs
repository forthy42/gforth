\ Current object structure

\ Copyright (C) 2012 Free Software Foundation, Inc.

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

\ This file is part of the Forth meta object protocol effort

[IFUNDEF] >o
    user op \ object pointer
    : o#+ ( #o -- a ) r> dup cell+ >r @ op @ + ; compile-only
    : >o  ( a -- r:a' )  r> op @ >r >r op ! ; compile-only
    : o>  ( r:a -- )  r> r> op ! >r ; compile-only
[THEN]

Variable do-field,

: o+ o#+ [ 0 , ] + ;

: field-context: ( xt-comp xt-int -- )  Create , , DOES> do-field, ! ;

' lit+ ' +   field-context: default-field
' o#+  ' o+  field-context: current-field

default-field

: +field ( n1 n2 "name" -- n3 ) \ X:structures plus-field
    create-interpret/compile over , +
interpretation>
    @ do-field, @ perform
<interpretation
compilation>
    @ do-field, @ cell+ @ compile, ,
<compilation ;

: extend-structure ( n "name" -- struct-sys n ) \ Gforth
    \g extend an existing structure
    >r 0 value lastxt >body r> ;

: begin-structure ( "name" -- struct-sys 0 ) \ X:structures
    0 extend-structure ;

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
