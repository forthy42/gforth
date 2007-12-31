\ provide TRY...ENDTRY-IFERROR...THEN on older Gforths

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


\ this provides the new-style TRY...ENDTRY-IFERROR...THEN syntax, but
\ not the other new TRY constructs.

[undefined] endtry-iferror [if]
    : endtry-iferror postpone recover ; immediate compile-only
[then]