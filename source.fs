\ source location handling

\ Copyright (C) 1995,1997 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.


\ related stuff can be found in kernel.fs

\ this stuff is used by (at least) assert.fs and debugging.fs

require struct.fs

struct
    cell% field sourcepos-name#
    cell% field sourcepos-line#
end-struct sourcepos
    
: sourcepos, ( -- )
    \ record the current source position HERE
    loadfilename# @ , sourceline# , ;

: get-sourcepos ( a-addr -- c-addr u n )
    \ c-addr u is the filename, n is the line number
    dup sourcepos-name# @ loadfilename#>str
    rot sourcepos-line# @ ;

: print-sourcepos ( a-addr -- )
    get-sourcepos
    >r type ." :"
    base @ decimal r> 0 .r base ! ;
