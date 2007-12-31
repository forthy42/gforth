\ Parameter for target systems                         06oct92py

\ Copyright (C) 1995,2000,2003,2007 Free Software Foundation, Inc.

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
   20 Constant bits/char
    4 Constant float
    4 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

\ generic mach file for pc gforth				03sep97jaw

true Constant NIL  \ relocating

>ENVIRON

false Constant file		\ controls the presence of the
				\ file access wordset
false Constant OS		\ flag to indicate a operating system

true Constant prims		\ true: primitives are c-code

false Constant floating		\ floating point wordset is present

false Constant glocals		\ gforth locals are present
				\ will be loaded
false Constant dcomps		\ double number comparisons

false Constant hash		\ hashing primitives are loaded/present

false Constant xconds		\ used together with glocals,
				\ special conditionals supporting gforths'
				\ local variables
true Constant header		\ save a header information

true Constant ec
true Constant relocate
false Constant crlf

28 KB Constant kernel-size
40 Constant stack-size
140 Constant rstack-size
40 Constant fstack-size
40 Constant lstack-size
