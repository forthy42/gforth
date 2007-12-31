\ string wordset test suite

\ Copyright (C) 2001,2003,2007 Free Software Foundation, Inc.

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

\ compare
{ here 0 c, 1 chars here 255 c, 1 chars compare -> -1 }

\ search
{ s" abcdef" s" abc" search -rot s" abcdef" str= -> true true }
{ s" abcdef" s" abd" search -rot s" abcdef" str= -> false true }
{ s" abcdef" s" bcd" search -rot s" bcdef"  str= -> true true }
{ s" abcdef" s" ef"  search -rot s" ef"     str= -> true true }
{ s" abcdef" s" fg"  search -rot s" abcdef" str= -> false true }
{ s" abcdef" s" "    search -rot s" abcdef" str= -> true true }
