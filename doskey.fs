\ MS-DOS key interpreter                               17oct94py

\ Copyright (C) 1995,1997,2000,2003,2007 Free Software Foundation, Inc.

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

Create translate $100 allot
translate $100 erase

: trans:  char translate + c! ;

: dos-decode ( max span addr pos1 -- max span addr pos2 flag )
  key translate + c@ dup IF  decode  THEN ;

ctrl B trans: K
ctrl F trans: M
ctrl P trans: H
ctrl N trans: P
ctrl A trans: G
ctrl E trans: O
ctrl X trans: S

' dos-decode  ctrlkeys !
