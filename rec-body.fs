\ <body[+offset]> recognizer
\ <foo> puts the body of foo on the stack like ' foo >body does

\ Author: Bernd Paysan
\ Copyright (C) 2019 Free Software Foundation, Inc.

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

: rec-body ( addr u -- xt rectype-tick | rectype-null )
    \G words bracketed with @code{'<'} @code{'>'} return their body.
    \G Example: @code{<dup>} gives the body of dup
    over c@ '<' <> >r  2dup + 1- c@ '>' <> r> or
    if 2drop rectype-null exit then
    1 /string 1- '+' $split 2>r find-name
    dup 0= if  drop 2rdrop rectype-null exit then
    name>int >body
    2r> dup 0= if  2drop rectype-num  exit  then
    case  rec-num
    rectype-dnum of  drop + rectype-num   endof
    rectype-num  of       + rectype-num   endof
    swap  endcase ;

' rec-body forth-recognizer >back
