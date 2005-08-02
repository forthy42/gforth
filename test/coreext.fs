\ test some core ext words

\ Copyright (C) 2005 Free Software Foundation, Inc.

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

{ 1 2 3 4 0 roll -> 1 2 3 4 }
{ 1 2 3 4 1 roll -> 1 2 4 3 }
{ 1 2 3 4 2 roll -> 1 3 4 2 }
{ 1 2 3 4 3 roll -> 2 3 4 1 }
