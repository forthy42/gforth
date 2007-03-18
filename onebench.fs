\ run timings on some small Forth benchmarks

\ Copyright (C) 2007 Free Software Foundation, Inc.

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

\ many platforms don't have GNU time, so we do it ourselves

.( sieve bubble matrix  fib) cr

warnings off

: include-main-time ( addr u -- )
    cputime d+ 2>r
    included s" main" evaluate
    cputime d+ 2r> d-
    <# # # # # # # '. hold #s #> 9 over - spaces 3 - type ;

s" siev.fs"   include-main-time
s" bubble.fs" include-main-time space
s" matrix.fs" include-main-time
s" fib.fs"    include-main-time
cr bye