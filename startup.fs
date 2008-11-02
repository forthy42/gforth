\ startup file

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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

\ don't require except.fs, because except.fs is not in included-files
\ (see exboot.fs)
[IFUNDEF] try
require except.fs \ included on command line
[THEN]

[IFDEF] throw>error
    ' throw>error Alias rethrow
[THEN]

warnings off
include search.fs
include environ.fs
include envos.fs
include errors.fs
include extend.fs              \ load core-extended
include hash.fs

[ifundef] xemit
    require kernel/xchars.fs
[then]
\ require interpretation.fs
\ include float.fs
\ include search.fs
require compat/strcomp.fs
include glocals.fs
require float.fs
require stuff.fs
include wordinfo.fs
include vt100.fs
\ include colorize.fs
include bufio.fs
require utf-8.fs
include history.fs
s" os-class" environment? dup [IF] drop s" unix" str= [THEN]
[IF]
include vt100key.fs
[ELSE]
include doskey.fs
[THEN]
require debugs.fs
require assert.fs
require blocks.fs
require intcomp.fs
require savesys.fs
require table.fs
s" os-class" environment? dup [IF] drop s" unix" str= [THEN]
[IF]
require ekey.fs
[ELSE]
require dosekey.fs
[THEN]
require quotes.fs
require see.fs
require see-ext.fs
require backtrac.fs
require simp-see.fs
require debug.fs
require code.fs
require dis-gdb.fs
require libcc.fs
require struct0x.fs

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
true constant facility-ext
true constant file
true constant file-ext
true constant floating
true constant floating-ext
true constant locals
true constant locals-ext
true constant memory-alloc
true constant memory-alloc-ext
true constant tools
true constant tools-ext
true constant search-order
true constant search-order-ext
true constant string
true constant string-ext
\ extension queries
' noop alias X:deferred
' noop alias X:defined
' noop alias X:ekeys
' noop alias X:extension-query
' noop alias X:fp-stack
' noop alias X:number-prefixes
' noop alias X:parse-name
' noop alias X:required
' noop alias X:structures
set-current

warnings on

require siteinit.fs
