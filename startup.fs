\ startup file

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

warnings off
include search.fs
include environ.fs
include envos.fs
include errors.fs
include extend.fs              \ load core-extended
include hash.fs

\ require interpretation.fs
\ include float.fs
\ include search.fs
include glocals.fs
include wordinfo.fs
include vt100.fs
\ include colorize.fs
include see.fs
include see-ext.fs
include bufio.fs
include debug.fs
require stuff.fs
include history.fs
s" os-class" environment? dup [IF] drop s" dos" compare 0= [THEN]
[IF]
include doskey.fs
[ELSE]
include vt100key.fs
[THEN]
require debugs.fs
require assert.fs
require blocks.fs
require intcomp.fs
require savesys.fs
require table.fs

\ define the environmental queries for all the loaded wordsets
\ since the blocks wordset is loaded in a single file, its queries
\ are defined there
\ queries for other things than presence of a wordset are answered
\ in environ.fs
get-current environment-wordlist set-current
true constant double
true constant double-ext
true constant exception
true constant exception-ext
true constant facility
\ !! facility-ext
true constant file
true constant file-ext
true constant floating
true constant floating-ext
true constant locals
true constant locals-ext
true constant memory-alloc
true constant memory-alloc-ext
true constant tools
\ !! tools-ext
true constant search-order
true constant search-order-ext
true constant string
true constant string-ext
set-current

warnings on

require siteinit.fs
