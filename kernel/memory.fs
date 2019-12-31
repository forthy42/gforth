\ memory-allocation wordset

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 1995,1998,1999,2001,2003,2006,2007,2011,2013,2014,2015,2016,2019 Free Software Foundation, Inc.

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

\ May be cross-compiled

here
' heap-allocate a,
' heap-free a,
' heap-resize a,
A, here Aconstant heap-words

uval-o current-memory-words
heap-words uto current-memory-words
0 0
umethod allocate ( u -- a_addr wior )	\ memory
    \G Allocate @i{u} address units of contiguous data space. The
    \G initial contents of the data space is undefined. If the
    \G allocation is successful, @i{a-addr} is the start address of
    \G the allocated region and @i{wior} is 0. If the allocation
    \G fails, @i{a-addr} is undefined and @i{wior} is a non-zero I/O
    \G result code.

umethod free	( a_addr -- wior )	\ memory
    \G Return the region of data space starting at @i{a-addr} to the
    \G system.  The region must originally have been obtained using
    \G @code{allocate} or @code{resize}. If the operational is
    \G successful, @i{wior} is 0.  If the operation fails, @i{wior} is
    \G a non-zero I/O result code.

umethod resize	( a_addr1 u -- a_addr2 wior )	\ memory
    \G Change the size of the allocated area at @i{a-addr1} to @i{u}
    \G address units, possibly moving the contents to a different
    \G area. @i{a-addr2} is the address of the resulting area.  If the
    \G operation is successful, @i{wior} is 0.  If the operation
    \G fails, @i{wior} is a non-zero I/O result code. If @i{a-addr1}
    \G is 0, Gforth's (but not the Standard) @code{resize}
    \G @code{allocate}s @i{u} address units.
2drop
