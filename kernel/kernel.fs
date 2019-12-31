\ kernel.fs this is a master include file for the kernel sources 2may97jaw

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,1999,2001,2003,2005,2006,2007,2011,2012,2013,2017,2019 Free Software Foundation, Inc.

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

require ./vars.fs
include ./input-class.fs
include ./int.fs
has? compiler [IF]
    include ./vtables.fs
    include ./comp.fs
[THEN]
include ./accept.fs
include ./input.fs
has? os [IF]
    include ./license.fs
    include kernel/authors.fs
    include ./xchars.fs
[THEN]
\ include ./nio.fs
