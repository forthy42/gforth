\ source location handling

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


\ related stuff can be found in kernel.fs

\ this stuff is used by (at least) assert.fs and debugging.fs

require struct.fs

struct
    1 cells: field sourcepos-name#
    1 cells: field sourcepos-line#
end-struct sourcepos
    
: sourcepos, ( -- )
    \ record the current source position HERE
    loadfilename# @ , sourceline# , ;

: get-sourcepos ( a-addr -- c-addr u n )
    \ c-addr u is the filename, n is the line number
    included-files 2@ drop over sourcepos-name# @ 2* cells + 2@
    rot sourcepos-line# @ ;

: print-sourcepos ( a-addr -- )
    get-sourcepos
    >r type ." :" r> 0 .r ;
