\ VT100.STR     VT100 excape sequences                  20may93jaw

\ Copyright (C) 1995,1999,2000,2003 Free Software Foundation, Inc.

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

decimal

: pn    base @ swap decimal 0 u.r base ! ;
: ;pn   [char] ; emit pn ;
: ESC[  27 emit [char] [ emit ;

: at-xy ( u1 u2 -- ) \ facility at-x-y
  \G Position the cursor so that subsequent text output will take
  \G place at column @var{u1}, row @var{u2} of the display. (column 0,
  \G row 0 is the top left-hand corner of the display).
  1+ swap 1+ swap ESC[ pn ;pn [char] H emit ;

: page ( -- ) \ facility
  \G Clear the display and set the cursor to the top left-hand
  \G corner.
  ESC[ ." 2J" 0 0 at-xy ;

