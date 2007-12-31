\ OTHER.FS     Ansforth extentions for CROSS           9may93jaw

\ Copyright (C) 1995,1998,2000,2003,2007 Free Software Foundation, Inc.

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


\ make ansforth compatible                              9may93jaw
\ the cross compiler should run
\ with any ansforth environment

: ?EXIT    POSTPONE if POSTPONE exit POSTPONE then ; immediate
: bounds   over + swap ;
: name bl word ;
: on true swap ! ;
: off false swap ! ;
: place ( adr len adr )
        2dup c! char+ swap move ;
: +place ( adr len adr )
        2dup c@ + over c!
        dup c@ char+ + swap move ;
: -rot  rot rot ;

include toolsext.fs

