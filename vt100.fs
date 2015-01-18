\ VT100.STR     VT100 excape sequences                  20may93jaw

\ Copyright (C) 1995,1999,2000,2003,2007,2012,2013,2014 Free Software Foundation, Inc.

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

decimal

[IFUNDEF] #esc  27 Constant #esc  [THEN]

: pn    base @ swap decimal 0 u.r base ! ;
: ;pn   [char] ; emit pn ;
: ESC[  #esc emit [char] [ emit ;

: vt100-at-xy ( u1 u2 -- ) \ facility at-x-y
  \G Position the cursor so that subsequent text output will take
  \G place at column @var{u1}, row @var{u2} of the display. (column 0,
  \G row 0 is the top left-hand corner of the display).
  1+ swap 1+ swap ESC[ pn ;pn [char] H emit ;

[IFUNDEF] at-deltaxy  Defer at-deltaxy [THEN]
: vt100-at-deltaxy ( x y -- )
    \G position the cursor relative to the current cursor position
    \G by adding @var{x} to the column and @var{y} to the row, negative
    \G numbers move up and left, positive down and right.
    over 0< over 0= and IF  drop abs backspaces  EXIT  THEN
    base @ >r decimal
    ?dup IF
	#esc emit '[ emit  dup abs 0 .r 0< IF  'A  ELSE  'B  THEN  emit
    THEN
    ?dup IF
	#esc emit '[ emit  dup abs 0 .r 0< IF  'D  ELSE  'C  THEN  emit
    THEN  r> base ! ;

: vt100-page ( -- ) \ facility
  \G Clear the display and set the cursor to the top left-hand
  \G corner.
  ESC[ ." 2J" 0 0 at-xy ;

' vt100-at-xy IS at-xy
' vt100-at-deltaxy IS at-deltaxy
' vt100-page IS page

[IFDEF] debug-out
    debug-out op-vector !
    
    ' vt100-at-xy IS at-xy
    ' vt100-at-deltaxy IS at-deltaxy
    ' vt100-page IS page
    
    default-out op-vector !
[THEN]