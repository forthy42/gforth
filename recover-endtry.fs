\ provide TRY...RECOVER...ENDTRY on newer Gforths

\ Copyright (C) 2007 Free Software Foundation, Inc.

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


\ this provides the old-style TRY...RECOVER...ENDTRY syntax, but after
\ loading this file the system will be incompatible with the new-style
\ TRY syntax.

[undefined] recover [if]

    : recover postpone endtry-iferror ; immediate compile-only
    : endtry  postpone then ; immediate compile-only

    \ now undefine the new words
    : iferror        -13 throw ; immediate
    : restore        -13 throw ; immediate
    : endtry-iferror -13 throw ; immediate
[then]