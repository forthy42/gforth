\ convert Forth strings to zero-terminated strings

\ Copyright (C) 2008 Free Software Foundation, Inc.

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

c-library cstr
c-function cstr       cstr       a n n -- a ( c-addr u fclear -- c-addr2 )
c-function tilde_cstr tilde_cstr a n n -- a ( c-addr u fclear -- c-addr2 )
end-c-library
