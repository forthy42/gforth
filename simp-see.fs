\ simple-minded see (good for seeing what the compiler produces)

\ Copyright (C) 2001 Free Software Foundation, Inc.

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

require see.fs

: simple-see-range ( addr1 addr2 -- )
    swap u+do
	cr xpos off i hex. i cell+ i @ .word drop
	cell +loop
;

: simple-see ( "name" -- )
    \ !! at the moment NEXT-HEAD is a little too optimistic (see
    \ comment in HEAD?)
    ' >body dup next-head simple-see-range ;

