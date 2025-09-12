\ Generate tests from BidiCharacterTest.txt

\ Authors: Bernd Paysan
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

[IFUNDEF] recognize-execute
    : recognize-execute ( xt recognizer -- )
	['] rec-forth rot wrap-xt ;
[THEN]

: rec-xemit ( addr u -- n/d table )
    ['] rec-num $10 base-execute dup ['] recognized-num = IF
	drop [: drop xemit ;]
    THEN ;

: hex2xchars ( -- )
    ['] cr is line-end-hook ['] read-loop ['] rec-xemit recognize-execute
    ['] noop is line-end-hook ;

stdin ' hex2xchars execute-parsing-file
