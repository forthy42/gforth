#! /usr/local/lib/gforth/0.2.0/kernel.fi
\ file hex dump

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

Create buffer $10 allot

: dumpline ( addr handle -- flag )
  buffer $10 rot read-file throw
  dup /dump !  $10 <> swap 6 u.r ." : "  buffer .line cr ;

: init  cr $10 base ! ;

: filedump  ( addr count -- )  init r/o bin open-file throw >r
  0  BEGIN  $10 bounds  r@ dumpline  UNTIL  drop
  r> close-file throw ;

script? [IF]
   : alldump argc @ 2 ?DO I arg 2dup type ." :" filedump LOOP ;
   alldump bye
[THEN]
