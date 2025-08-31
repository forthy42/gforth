\ pass.fs pass pointers from cross to target		20May99jaw

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1999,2001,2003,2006,2007,2012,2019 Free Software Foundation, Inc.

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


\ Set up dictionary pointer
>ram here normal-dp !

\ set udp
has? no-userspace 0= [IF]
UNLOCK user-region extent nip LOCK udp !
[THEN]

\ Set up last and forth-wordlist with the address of the last word's
\ link field
UNLOCK tlast @ LOCK
has? f83headerstring 0= [IF] 1 cells - [THEN] dup forth-wordlist has? ec 0= [IF] wordlist-id [THEN] ! Last !
