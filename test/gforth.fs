\ test some gforth extension words

\ Copyright (C) 2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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

require ./tester.fs
decimal

\ f>str-rdp (then f.rdp and f>buf-rdb should also be ok)

{  12.3456789e 7 3 1 f>str-rdp s"  12.346" str= -> true }
{  12.3456789e 7 4 1 f>str-rdp s" 12.3457" str= -> true }
{ -12.3456789e 7 4 1 f>str-rdp s" -1.23E1" str= -> true }
{      0.0996e 7 3 1 f>str-rdp s"   0.100" str= -> true }
{      0.0996e 7 3 3 f>str-rdp s" 9.96E-2" str= -> true }
{    999.9994e 7 3 1 f>str-rdp s" 999.999" str= -> true }
{    999.9996e 7 3 1 f>str-rdp s" 1.000E3" str= -> true }
{       -1e-20 5 2 1 f>str-rdp s" *****"   str= -> true }

\ 0x hex number conversion, or not

decimal
{ 0x10 -> 16 }
{ 0X10 -> 16 }
36 base !
{ 0x10 -> x10 }
decimal
{ 'a' -> 97 }
{ 'A  -> 65 }
{ 1. '1 -> 1. 49 }

\ REPRESENT has no trailing 0s even for inf and nan

{  1e 0e f/ pad 16 represent drop 2drop pad 15 + c@ '0 = -> false }
{  0e 0e f/ pad 16 represent drop 2drop pad 15 + c@ '0 = -> false }
{ -1e 0e f/ pad 16 represent drop 2drop pad 15 + c@ '0 = -> false }

\ TRY and friends

: 0<-throw ( n -- )
    0< throw ;

: try-test1 ( n1 -- n2 )
    try
        dup 0<-throw
        iferror
            2drop 25
        then
        1+
    endtry ;

{ -5 try-test1 -> 26 }
{ 5  try-test1 ->  6 }

: try-test2 ( n1 -- n2 )
    try
        0
    restore
        drop 1+ dup 0<-throw
    endtry ;

{ -5 try-test2 -> 0 }
{  5 try-test2 -> 6 }

: try-test3 ( n1 -- n2 )
    try
        dup 0<-throw
    endtry-iferror
        2drop 10
    else
        1+
    then ;

{ -5 try-test3 -> 10 }
{  5 try-test3 ->  6 }
