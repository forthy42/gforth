\ VT100.STR     VT100 excape sequences                  20may93jaw

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

decimal

: pn    base @ swap decimal 0 u.r base ! ;
: ;pn   [char] ; emit pn ;
: ESC[  27 emit [char] [ emit ;
: at-xy 1+ swap 1+ swap ESC[ pn ;pn [char] H emit ;
: page  ESC[ ." 2J" 0 0 at-xy ;

