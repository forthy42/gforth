\ terminal size stuff

\ Copyright (C) 1996,1997,2000,2003,2007 Free Software Foundation, Inc.

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

\ # rows and columns that the terminal has.
\ these words are also present in PFE.

[IFUNDEF] form : form &24 &80 ; [THEN]

[IFUNDEF] rows
: rows ( -- u )
    form drop ;

: cols ( -- u )
    form nip ;
[THEN]
