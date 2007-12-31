\ kernel to verilog converter

\ Copyright (C) 1998,2000,2003,2004,2007 Free Software Foundation, Inc.

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

: .## base @ >r hex 0 <# # # #> type r> base ! ;

create item 2 allot

: >v ( addr u -- ) r/o open-file throw >r
    ." @0"
    BEGIN  item 2 r@ read-file throw  WHILE
	cr item c@ .## item char+ c@ .##
    REPEAT
    cr r> close-file throw ;

script? [IF]
   : all2v argc @ 2 ?DO I arg >v LOOP ;
   all2v bye
[THEN]
