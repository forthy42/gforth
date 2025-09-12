\ <body[+offset]> recognizer
\ <foo> puts the body of foo on the stack like ' foo >body does

\ Author: Bernd Paysan
\ Copyright (C) 2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

require rec-tick.fs

: rec-body ( addr u -- xt translate-cell | 0 ) \ gforth-experimental
    \G words bracketed with @code{'<'} @code{'>'} return their body.
    \G Example: @code{<dup>} gives the body of dup
    over c@ '<' <> >r  2dup + 1- c@ '>' <> r> or
    if 2drop 0 exit then
    1 /string 1- '+' $split 2>r forth-recognize-nt? dup 0= if
        2rdrop exit then
    name>interpret >body
    2r> dup 0= if  2drop translate-cell  exit  then
    case  rec-number
	translate-dcell of  drop + translate-cell   endof
	translate-cell  of       + translate-cell   endof
	swap  endcase ;

' rec-body action-of rec-forth >back
