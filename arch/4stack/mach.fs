\ Parameter for target systems                         06oct92py

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

    4 Constant cell
    2 Constant cell<<
    5 Constant cell>bit
    8 Constant bits/byte
    8 Constant float
    8 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

true  Constant NIL  \ relocating

: prims-include  ." Include primitives" cr s" arch/4stack/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/4stack/asm.fs" included ;

: >boot
    S" ' boot >body $800 ! here $804 !" evaluate ;

>ENVIRON

false Constant file		\ controls the presence of the
				\ file access wordset
false Constant OS		\ flag to indicate a operating system

false Constant prims		\ true: primitives are c-code

false Constant floating		\ floating point wordset is present

false Constant glocals		\ gforth locals are present
				\ will be loaded
false Constant dcomps		\ double number comparisons

false Constant hash		\ hashing primitives are loaded/present

false Constant xconds		\ used together with glocals,
				\ special conditionals supporting gforths'
				\ local variables
false Constant header		\ save a header information

false Constant ec
false Constant crlf
false Constant ITC
