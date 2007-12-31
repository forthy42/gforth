\ run-time routine headers

\ Copyright (C) 1997,1998,2002,2003,2006,2007 Free Software Foundation, Inc.

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

-2 Doer: :docol
-3 Doer: :docon
-4 Doer: :dovar
-5 Doer: :douser
-6 Doer: :dodefer
-7 Doer: :dofield
-8 Doer: :dovalue
-9 Doer: :dodoes
-&10 Doer: :doesjump
-&2 first-primitive \ this does not work for (at least) (DODOES),
                    \ so these routines are commented out
