\ data structures (like C structs)

\ Copyright (C) 1995-2003 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

: naligned ( addr1 n -- addr2 ) \ gforth
\g @var{addr2} is the aligned version of @var{addr1} with respect to the
\g alignment @var{n}.
 1- tuck +  swap invert and ;

' naligned alias nalign \ old name, obsolete

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
    \g Create a field @var{name} with offset @var{offset1}, and the type
    \g given by @var{align size}. @var{offset2} is the offset of the
    \g next field, and @var{align2} is the alignment of all fields.@*
    \g @code{name} execution: @var{addr1} -- @var{addr2}.@*
    \g @var{addr2}=@var{addr1}+@var{offset1}
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
\g Define a structure/type descriptor @var{name} with alignment
\g @var{align} and size @var{size1} (@var{size} rounded up to be a
\g multiple of @var{align}).@*
\g @code{name} execution: -- @var{align size1}@*
    over nalign \ pad size to full alignment
    2constant ;

1 chars 0 end-struct struct ( -- align size ) \ gforth
\g An empty structure, used to start a structure definition.

\ type descriptors
1 aligned   1 cells   2constant cell% ( -- align size ) \ gforth
1 chars     1 chars   2constant char% ( -- align size ) \ gforth
1 faligned  1 floats  2constant float% ( -- align size ) \ gforth
1 dfaligned 1 dfloats 2constant dfloat% ( -- align size ) \ gforth
1 sfaligned 1 sfloats 2constant sfloat% ( -- align size ) \ gforth
cell% 2*              2constant double% ( -- align size ) \ gforth

\ memory allocation words
' drop alias %alignment ( align size -- align ) \ gforth
\g The alignment of the structure.
' nip alias %size ( align size -- size ) \ gforth
\g The size of the structure.

: %align ( align size -- ) \ gforth
    \G Align the data space pointer to the alignment @var{align}. 
    drop here swap nalign here - allot ;

: %allot ( align size -- addr ) \ gforth
    \g Allot @var{size} address units of data space with alignment
    \g @var{align}; the resulting block of data is found at
    \g @var{addr}.
    tuck %align
    here swap allot ;

: %allocate ( align size -- addr ior ) \ gforth
    \g Allocate @var{size} address units with alignment @var{align},
    \g similar to @code{allocate}.
    nip allocate ;

: %alloc ( size align -- addr ) \ gforth
    \g Allocate @var{size} address units with alignment @var{align},
    \g giving a data block at @var{addr}; @code{throw} an ior code
    \g if not successful.
    %allocate throw ;
