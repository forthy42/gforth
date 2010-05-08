\ test bitmask code

\ Copyright (C) 2010 Free Software Foundation, Inc.

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

require ../asm/bitmask.fs

{ : test1 maskinto $F0F0F0 ; -> }
{ $567 $abcde test1 -> $5a6c7e }

require ../asm/bitmask2.fs

{ $567 $F0F0F0 dispense -> $506070 }
{ $567 $abcde $F0F0F0 embed -> $5a6c7e }

{ : test1 maskinto $F0F0F0 ; -> }
{ $567 $abcde test1 -> $5a6c7e }

{ 127 8 narrow -> $FF }
{ -128 8 narrow -> $80 }
{ 128 8 ' narrow catch nip nip -> -2 }
{ -129 8 ' narrow catch nip nip -> -2 }

