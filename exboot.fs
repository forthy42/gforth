\ ( n -- ) include exceptions.fs and restart Gforth, ignoring the
\ first n arguments. Gforth is restarted to create an exception frame
\ for the exception handler.

\ Copyright (C) 2000 Free Software Foundation, Inc.

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

require exceptions.fs

included-files 2@ image-included-files 2!
pathstring 2@ rot argv @ over cells + argc @ rot - boot
