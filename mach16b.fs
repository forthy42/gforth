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
    8 Constant /maxalign
 true Constant bigendian
( true=big, false=little )

\ feature list

true Constant NIL  \ relocating

true Constant has-files 
true Constant has-OS
true Constant has-prims
true Constant has-floats
true Constant has-locals
true Constant has-dcomps
true Constant has-hash
true Constant has-xconds
true Constant has-header
