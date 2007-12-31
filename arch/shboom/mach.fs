\ Parameter for target systems                         06oct92py

\ Copyright (C) 1995,2001,2003,2007 Free Software Foundation, Inc.

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

    4 Constant cell
    2 Constant cell<<
    5 Constant cell>bit
    8 Constant bits/char
    8 Constant float
    4 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

\ false Constant NIL  \ relocating

: prims-include  ." Include primitives" cr s" arch/shboom/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/shboom/asm.fs" included ;
: >boot ;

>ENVIRON
false SetValue relocate
false SetValue file
false SetValue OS
false SetValue prims
false SetValue floating
false SetValue glocals
false SetValue dcomps
false SetValue hash
false SetValue xconds
false SetValue header
true SetValue ec
true SetValue crlf
false SetValue ITC
false SetValue new-input
false SetValue peephole

cell 2 = [IF] 32 [ELSE] 256 [THEN] KB Constant kernel-size

16 KB		Constant stack-size
15 KB 512 +	Constant fstack-size
15 KB		Constant rstack-size
14 KB 512 +	Constant lstack-size
