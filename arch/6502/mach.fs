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
false Constant bigendian
( true=big, false=little )

\ feature list

0 Constant NIL  \ relocating

false Constant has-files 
false Constant has-OS
false Constant has-prims
false Constant has-floats
false Constant has-locals
false Constant has-dcomps
true Constant has-hash
false Constant has-xconds
false Constant has-header
true Constant has-rom
true Constant has-interpreter
true Constant has-crlf
: prims-include  ." Include primitives" cr s" arch/6502/prims.fs" included ;
: asm-include    ." Include assembler" cr s" arch/6502/asm.fs" included ;

>ENVIRON

true Value ec

