\ growable buffers/array

\ Copyright (C) 2000,2007 Free Software Foundation, Inc.

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

struct
    cell% field growable-address1
    cell% field growable-size
end-struct growable%

: init-growable ( growable -- )
    0 over growable-address1 !
    1 cells swap growable-size ! ;

: grow-to ( u growable -- )
    \ grow growable to at least u aus
    dup >r growable-size @ begin
	2dup u> while
	2* repeat
    nip r@ growable-address1 @ over resize throw
    \ !! assumptions: resize with current address 0 is allocate;
    \ resizing to the current size is cheap
    r@ growable-address1 !
    r> growable-size ! ;

: growable-addr ( offset growable -- address )
    \ address at offset within growable
    growable-address1 @ + ;

: fit-growable ( offset usize growable -- address )
    \ address is at offset within growable; growable becomes large
    \ enough to have an object of size usize there
    >r over + r@ grow-to
    r> growable-addr ;

false [if] \ test code

growable% %allot constant x
x init-growable
x growable-size ?
10 x grow-to
x growable-size ?
4 x growable-addr hex.
12 8 x fit-growable hex.
x growable-size ?
.s

[then]

