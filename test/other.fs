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


\ recurse and locals

: fac { n -- n! }
    n 0>
    if
	n 1- recurse n *
    else
	1
    endif ;

5 fac 120 <> throw

\ TO and locals

: locals-test2 ( -- )
    true dup dup dup { addr1 u1 addr2 u2 -- n }
    false TO addr1
    addr1 false <> abort" TO does not work on locals" ;
locals-test2

\ multiple reveals (recursive)

0
: xxx recursive ;
throw \ if the TOS is not 0, throw an error

\ look for primitives

' + look 0= throw ( nt )
s" +" find-name <> throw

\ represent

1e pad 5 represent -1 <> swap 0 <> or swap 1 <> or throw

\ -trailing

s" a     " 2 /string -trailing throw drop

\ convert (has to skip first char)

0. s" 123  " drop convert drop 23. d<> throw

\ search

name abc 2dup name xyza search throw d<> throw
name b 2dup name abc search throw d<> throw

\ only

: test-only ( -- )
    get-order get-current
    0 set-current
    only
    get-current >r
    set-current set-order
    r> abort" ONLY sets current" ;
test-only

\ create-interpret/compile

: constant ( n "name" -- )
    create-interpret/compile
    ,
interpretation>
    @
<interpretation
compilation>
    @ postpone literal
<compilation ;

5 constant five
five 5 <> throw
: five' five ;
five' 5 <> throw

\ comments across several lines

( fjklfjlas;d
abort" ( does not work across lines"
)

s" ( testing ( without delimited by newline in non-files" evaluate

\ last test!
\ testing '(' without ')' at end-of-file
." expect ``warning: ')' missing''" cr
(
