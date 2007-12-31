\ kernel.fs this is a master include file for the kernel sources 2may97jaw

\ Copyright (C) 1995,1996,1997,1998,1999,2001,2003,2005,2006,2007 Free Software Foundation, Inc.

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

\ include ./basics.fs
\ include ./io.fs		\ basic io functions
has? interpreter [IF]
    include ./int.fs
    has? compiler [IF]
        include ./comp.fs
    [THEN]
[THEN]
has? new-input [IF]
    include ./accept.fs
    include ./input.fs
[ELSE]
    include ./saccept.fs
[THEN]
has? os [IF]
include ./license.fs
include ./xchars.fs
[THEN]
\ include ./nio.fs
