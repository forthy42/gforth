\ MS-DOS key interpreter                               17oct94py

\ Copyright (C) 1995 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

Create translate $100 allot
translate $100 erase

: trans:  char translate + c! ;

: vt100-decode ( max span addr pos1 -- max span addr pos2 flag )
  key '[ = IF    key translate + c@ dup IF  decode  THEN
           ELSE  0  THEN ;

ctrl B trans: D
ctrl F trans: C
ctrl P trans: A
ctrl N trans: B

' vt100-decode  ctrlkeys $1B cells + !
