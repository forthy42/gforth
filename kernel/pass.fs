\ pass.fs pass pointers from cross to target		20May99jaw

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke, Neal Crook
\ Copyright (C) 1999,2001,2003,2006,2007,2013,2016,2017,2018,2019 Free Software Foundation, Inc.

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

\ set udp
has? no-userspace 0= [IF]
UNLOCK user-region extent nip LOCK udp !
[THEN]

\ Set up last and forth-wordlist with the address of the last word's
\ link field
UNLOCK tlast @ LOCK
dup forth-wordlist has? ec 0= [IF] wordlist-id [THEN] ! Last !

unlock vt, tvtable-list @ lock vtable-list !

\ list of arrays to restore at boot
align here boot[][] ! boot[][],

\ list of strings to resture at boot
align here boot$[] !  boot$[],

included-files, included-files !

align here default-recognizer !
2 cells , ' rec-num A, ' rec-nt A,

align here image-header 4 cells + !
locs[],

align here wheres !
wheres,

>ram
