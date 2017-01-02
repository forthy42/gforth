\ startup file

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2016 Free Software Foundation, Inc.

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
include options.fs
include environ.fs
include ~+/envos.fs
include errors.fs
include extend.fs              \ load core-extended
include compat/caseext.fs
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
\ include vt100key.fs \ ekey.fs now does the same
require debugs.fs
require assert.fs
require blocks.fs
\ require intcomp.fs
require savesys.fs
require table.fs
require ekey.fs
include history.fs
require quotes.fs
require quotedstring.fs
require parsedto.fs
require substitute.fs
require code.fs
require see.fs
require see-ext.fs
require backtrac.fs
require simp-see.fs
require debug.fs
require dis-gdb.fs
require struct0x.fs
require libcc.fs
require prelude.fs
require str-exec.fs
require gforthrc.fs
include ansi.fs

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

\ The names are the file names of the proposals (without extension) on
\ http://www.forth200x.org/, prefixed with X:
' noop alias X:2value
' noop alias X:buffer
' noop alias X:deferred
' noop alias X:defined
' noop alias X:ekeys
' noop alias X:escaped-strings
' noop alias X:extension-query
' noop alias X:fp-stack
' noop alias X:ftrunc
' noop alias X:fvalue
' noop alias X:locals
' noop alias X:n-to-r
' noop alias X:number-prefixes
' noop alias X:parse-name
' noop alias X:required
' noop alias X:s-escape-quote
' noop alias X:s-to-f
' noop alias X:structures
' noop alias X:synonym
' noop alias X:text-substitution
' noop alias X:throw-iors
' noop alias X:traverse-wordlist
' noop alias X:xchar
set-current

-2 warnings !

require siteinit.fs
