\ OTHER.FS     Ansforth extentions for CROSS           9may93jaw

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


\ make ansforth compatible                              9may93jaw
\ the cross compiler should run
\ with any ansforth environment

: ?EXIT    s" IF EXIT THEN" evaluate ; immediate
: bounds   over + swap ;
: capitalize ( addr -- addr )
  dup count chars bounds
  ?DO  I c@ [char] a [char] { within
       IF  I c@ bl - I c!  THEN  1 chars +LOOP ;
: name bl word ( capitalize ) ;
: on true swap ! ;
: off false swap ! ;
: place ( adr len adr )
        2dup c! char+ swap move ;
: +place ( adr len adr )
        2dup c@ + over c!
        dup c@ char+ + swap move ;
: -rot  rot rot ;

include toolsext.fs

