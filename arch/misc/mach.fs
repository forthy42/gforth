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

    2 Constant cell
    1 Constant cell<<
    4 Constant cell>bit
    8 Constant bits/byte
    8 Constant float
    2 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

false Constant NIL  \ relocating

false Constant has-files 
false Constant has-OS
false Constant has-prims
false Constant has-floats
false Constant has-locals
false Constant has-dcomps
false Constant has-hash
false Constant has-xconds
false Constant has-header
true  Constant ITC

: prims-include  ." Include primitives" cr s" arch/misc/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/misc/asm.fs" included ;
: >boot
    hex
    S" $6FF0 SP ! $7FF0 RP ! $7000 2* UP ! ' boot >body IP !" evaluate    
    decimal ;
