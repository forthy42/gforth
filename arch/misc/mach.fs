\ Parameter for target systems                         06oct92py

\ Copyright (C) 1995,2003,2007 Free Software Foundation, Inc.

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

    2 Constant cell
    1 Constant cell<<
    4 Constant cell>bit
    8 Constant bits/char
    8 Constant float
    2 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

false Constant NIL  \ relocating

: prims-include  ." Include primitives" cr s" arch/misc/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/misc/asm.fs" included ;
: >boot
    hex
    S" data-stack-top SP' 2* ! return-stack-top RP' 2* ! ' boot >body IP' 2* !" 
    evaluate decimal ;

>ENVIRON

false SetValue file		\ controls the presence of the
				\ file access wordset
false SetValue OS		\ flag to indicate a operating system

false SetValue prims		\ true: primitives are c-code

false SetValue floating		\ floating point wordset is present

false SetValue glocals		\ gforth locals are present
				\ will be loaded
false SetValue dcomps		\ double number comparisons

false SetValue hash		\ hashing primitives are loaded/present

false SetValue xconds		\ used together with glocals,
				\ special conditionals supporting gforths'
				\ local variables
false SetValue header		\ save a header information

true SetValue ec
false SetValue crlf
true SetValue ITC
false SetValue new-input
false SetValue peephole
true SetValue abranch       \ enables absolute branches

\ true SetValue rom

\ true SetValue compiler
\ false SetValue primtrace
\ true SetValue no-userspace


\ cell 2 = [IF] 32 [ELSE] 256 [THEN] KB Constant kernel-size

16 KB		SetValue stack-size
15 KB 512 +	SetValue fstack-size
15 KB		SetValue rstack-size
14 KB 512 +	SetValue lstack-size

0 SetValue kernel-start
cell 2 = [IF] 32 [ELSE] 256 [THEN] KB SetValue kernel-size

