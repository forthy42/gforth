\ Parameter for target systems                         06oct92py

\ Copyright (C) 1995,1999,2001,2003,2007 Free Software Foundation, Inc.

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

\ Autor:        Gerd Franzzkowiak (adaptet from 6502)
\ Log:          24.06.2009 gf: file generated
\

    4 Constant cell
    2 Constant cell<<
    5 Constant cell>bit
    8 Constant bits/char
    8 Constant float
    8 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

: prims-include  ." Include primitives" cr s" arch/m68k/prims.fs"   included ;
: asm-include    ." Include assembler" cr s" arch/m68k/asm.fs" included ;
\ : >boot          ." Prepare booting" cr
\     s" ' boot >body into-forth 1+ !" evaluate ;
: >boot ;

\ ?-gf-? false Constant NIL

>ENVIRON

true  DefaultValue ec
true  DefaultValue crlf
true  SetValue rom
\ true Constant has-rom
false SetValue peephole
false SetValue new-input     \ disables object oriented input

false DefaultValue file          \ controls the presence of the
                                 \ file access wordset
true  DefaultValue OS            \ flag to indicate a operating system
true  DefaultValue prims         \ true: primitives are c-code
false DefaultValue floating      \ floating point wordset is present
false DefaultValue glocals       \ gforth locals are present
                                 \ will be loaded
true  DefaultValue dcomps        \ double number comparisons
true  DefaultValue hash          \ hashing primitives are loaded/present
true  DefaultValue xconds        \ used together with glocals,
                                 \ special conditionals supporting gforths'
                                 \ local variables
true  DefaultValue header        \ save a header information
true  DefaultValue backtrace     \ enables backtrace code

true DefaultValue f83headerstring

cell 2 = [IF] &32 [ELSE] &256 [THEN] KB DefaultValue kernel-size
\ &32 KB          DefaultValue kernel-size

&8  KB          DefaultValue stack-size
\ &15 KB &512 +   DefaultValue fstack-size
&8  KB          DefaultValue rstack-size
\ 14 KB 512 +	Constant lstack-size

