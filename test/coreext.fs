\ test some core ext words

\ Copyright (C) 2005,2007,2010,2011 Free Software Foundation, Inc.

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

testing core ext
{ 1 2 3 4 0 roll -> 1 2 3 4 }
{ 1 2 3 4 1 roll -> 1 2 4 3 }
{ 1 2 3 4 2 roll -> 1 3 4 2 }
{ 1 2 3 4 3 roll -> 2 3 4 1 }

{ 1 0 U> -> <TRUE> }
{ 2 1 U> -> <TRUE> }
{ MID-UINT 0 U> -> <TRUE> }
{ MAX-UINT 0 U> -> <TRUE> }
{ MAX-UINT MID-UINT U> -> <TRUE> }
{ 0 0 U> -> <FALSE> }
{ 1 1 U> -> <FALSE> }
{ 0 1 U> -> <FALSE> }
{ 1 2 U> -> <FALSE> }
{ 0 MID-UINT U> -> <FALSE> }
{ 0 MAX-UINT U> -> <FALSE> }
{ MID-UINT MAX-UINT U> -> <FALSE> }

