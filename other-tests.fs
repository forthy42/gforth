\ various tests, especially for bugs that have been fixed

\ Copyright (C) 1997 Free Software Foundation, Inc.

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

\ combination of marker and locals
marker foo1
marker foo2
foo2

: bar { xxx yyy } ;

foo1

\ locals in an if structure
: locals-test1
    lp@ swap
    if
	{ a } a
    else
    endif
    lp@ <> abort" locals in if error 1" ;

0 locals-test1
1 locals-test1

\ look for primitives

' + look 0= throw ( nt )
s" +" find-name <> throw

\ represent

1e pad 5 represent -1 <> swap 0 <> or swap 1 <> or throw

\ comments across several lines

( fjklfjlas;d
abort" ( does not work across lines"
)

s" ( testing ( without delimited by newline in non-files" evaluate

\ last test!
\ testing '(' without ')' at end-of-file
." expect ``warning: ')' missing''" cr
(
