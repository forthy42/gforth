\ generic mach file for pc gforth				03sep97jaw

\ Copyright (C) 1995,1996,1997 Free Software Foundation, Inc.

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

true Constant NIL  \ relocating

>ENVIRON

true Constant file		\ controls the presence of the
				\ file access wordset
true Constant OS		\ flag to indicate a operating system

true Constant prims		\ true: primitives are c-code

true Constant floating		\ floating point wordset is present

true Constant glocals		\ gforth locals are present
				\ will be loaded
true Constant dcomps		\ double number comparisons

true Constant hash		\ hashing primitives are loaded/present

true Constant xconds		\ used together with glocals,
				\ special conditionals supporting gforths'
				\ local variables
true Constant header		\ save a header information

false Constant ec
false Constant crlf

cell 2 = [IF] 32 [ELSE] 256 [THEN] KB Constant kernel-size

16 KB		Constant stack-size
15 KB 512 +	Constant fstack-size
15 KB		Constant rstack-size
14 KB 512 +	Constant lstack-size
