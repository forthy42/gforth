\ data structures (like C structs)

\ Copyright (C) 1995, 1997 Free Software Foundation, Inc.

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

: nalign ( addr1 n -- addr2 ) \ gforth
\g @code{addr2} is the aligned version of @code{addr1} wrt the
\g alignment @code{n}.
 1- tuck +  swap invert and ;

: dozerofield ( -- )
    \ a field that makes no change
    \ to enable accessing the offset with "['] <field> >body @" this
    \ is not implemented with "['] noop alias"
    last @
    if
	immediate
    then
does> ( name execution: -- )
    drop ;

: field, ( align1 offset1 align size --  align2 offset2 )
    swap rot over nalign dup , ( align1 size align offset )
    rot + >r nalign r> ;

: create-field ( align1 offset1 align size --  align2 offset2 )
    create field, ;

: field ( align1 offset1 align size "name" --  align2 offset2 ) \ gforth
    \G name execution: ( addr1 -- addr2 )
    2 pick 
    if \ field offset <> 0
	[IFDEF]  (Field)
	    (Field)
	[ELSE]
	    Header reveal dofield: cfa,
	[THEN]
    else
	create dozerofield
    then
    field, ;

: end-struct ( align size "name" -- ) \ gforth
\g @code{name} execution: @code{addr1 -- addr1+offset1}@*
\g create a field @code{name} with offset @code{offset1}, and the type
\g given by @code{size align}. @code{offset2} is the offset of the
\g next field, and @code{align2} is the alignment of all fields.
    over nalign \ pad size to full alignment
    2constant ;

1 chars 0 end-struct struct ( -- align size ) \ gforth
\g an empty structure, used to start a structure definition.

\ type descriptors
1 aligned   1 cells   2constant cell% ( -- align size ) \ gforth
1 chars     1 chars   2constant char% ( -- align size ) \ gforth
1 faligned  1 floats  2constant float% ( -- align size ) \ gforth
1 dfaligned 1 dfloats 2constant dfloat% ( -- align size ) \ gforth
1 sfaligned 1 sfloats 2constant sfloat% ( -- align size ) \ gforth
cell% 2*              2constant double% ( -- align size ) \ gforth

\ memory allocation words
' drop alias %alignment ( align size -- align ) \ gforth
\g the alignment of the structure
' nip alias %size ( align size -- size ) \ gforth
\g the size of the structure

: %align ( align size -- ) \ gforth
    \G align the data space pointer to the alignment @code{align}. 
    drop here swap nalign here - allot ;

: %allot ( align size -- addr ) \ gforth
    \g allot @code{size} address units of data space with alignment
    \g @code{align}; the resulting block of data is found at
    \g @code{addr}.
    tuck %align
    here swap allot ;

: %allocate ( align size -- addr ior ) \ gforth
    \g allocate @code{size} address units with alignment @code{align},
    \g similar to @code{allocate}.
    nip allocate ;

: %alloc ( size align -- addr ) \ gforth
    \g allocate @code{size} address units with alignment @code{align},
    \g giving a data block at @code{addr}; @code{throw}s an ior code
    \g if not successful.
    %allocate throw ;
