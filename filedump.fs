#! /usr/local/lib/gforth/0.2.0/kernel.fi
\ file hex dump

\ Copyright (C) 1997,2002,2003,2004,2007 Free Software Foundation, Inc.

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

Create buf $10 allot

: dumpline ( addr handle -- flag )
  buf $10 rot read-file throw
  dup /dump !  $10 <> swap 6 u.r ." : "  buf .line cr ;

: init  cr $10 base ! ;

: filedump  ( addr count -- )  init r/o bin open-file throw >r
  0  BEGIN  $10 bounds  r@ dumpline  UNTIL  drop
  r> close-file throw ;

script? [IF]
   : alldump argc @ 1 ?DO I arg 2dup type ." :" filedump LOOP ;
   alldump bye
[THEN]
