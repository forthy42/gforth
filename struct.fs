\ data structures (like C structs)

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook
\ Copyright (C) 1995,1997,2000,2003,2007,2019,2021,2023,2024 Free Software Foundation, Inc.

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

: naligned ( addr1 n -- addr2 ) \ gforth-internal
\g @var{Addr2} is the aligned version of @var{addr1} with respect to the
\g alignment @var{n}.  Another name for this word is @code{*aligned}.
    1- tuck +  swap invert and ;
fold1:
    1- dup >lits postpone + invert >lits postpone and ;

: field, ( align1 offset1 align size --  align2 offset2 )
    swap rot over naligned dup , ( align1 size align offset )
    rot + >r naligned r> ;

: create-field ( align1 offset1 align size --  align2 offset2 )
    create field, ;

: field ( align1 offset1 align size "name" --  align2 offset2 ) \ gforth
    \g Create a field @var{name} with offset @var{offset1}, and the type
    \g given by @var{align size}. @var{offset2} is the offset of the
    \g next field, and @var{align2} is the alignment of all fields.@*
    \g @code{name} execution: @var{addr1} -- @var{addr2}.@*
    \g @var{addr2}=@var{addr1}+@var{offset1}
    (Field) field, ;

: end-struct ( align size "name" -- ) \ gforth
\g Define a structure/type descriptor @var{name} with alignment
\g @var{align} and size @var{size1} (@var{size} rounded up to be a
\g multiple of @var{align}).@*
\g @code{name} execution: -- @var{align size1}@*
    over naligned \ pad size to full alignment
    2constant ;

1 chars 0 end-struct struct ( -- align size ) \ gforth
\g An empty structure, used to start a structure definition.

\ type descriptors
1 aligned   1 cells   2constant cell% ( -- align size ) \ gforth cell-percent
1 chars     1 chars   2constant char% ( -- align size ) \ gforth char-percent
1 faligned  1 floats  2constant float% ( -- align size ) \ gforth float-percent
1 dfaligned 1 dfloats 2constant dfloat% ( -- align size ) \ gforth d-float-percent
1 sfaligned 1 sfloats 2constant sfloat% ( -- align size ) \ gforth s-float-percent
cell% 2*              2constant double% ( -- align size ) \ gforth double-percent
\G describes a double cell (equivalent to @code{cell% 2*}).

\ memory allocation words
' drop alias %alignment ( align size -- align ) \ gforth percent-alignment
\g The alignment of the structure.
' nip alias %size ( align size -- size ) \ gforth percent-size
\g The size of the structure.

: %align ( align size -- ) \ gforth percent-align
    \G Align the data space pointer to the alignment @var{align}. 
    drop here swap naligned here - allot ;

: %allot ( align size -- addr ) \ gforth percent-allot
    \g Allot @var{size} address units of data space with alignment
    \g @var{align}; the resulting block of data is found at
    \g @var{addr}.
    tuck %align
    here swap allot ;

: %allocate ( align size -- addr ior ) \ gforth percent-allocate
    \g Allocate @var{size} address units with alignment @var{align},
    \g similar to @code{allocate}.
    nip allocate ;

: %alloc ( align size -- addr ) \ gforth percent-alloc
    \g Allocate @var{size} address units with alignment @var{align},
    \g giving a data block at @var{addr}; @code{throw} an ior code
    \g if not successful.
    %allocate throw ;
