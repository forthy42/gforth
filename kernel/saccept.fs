\ a very simple accept approach

\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003 Free Software Foundation, Inc.

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

require ./io.fs

: accept ( adr len -- len )
  over + over ( start end pnt )
  BEGIN
   key dup #del = IF drop #bs THEN
   dup bl u<
   IF	dup #cr = over #lf = or IF space drop nip swap - EXIT THEN
	#bs = IF 3 pick over <> 
    	IF 1 chars - #bs emit bl emit #bs emit ELSE bell THEN THEN
   ELSE	>r 2dup <> IF r> dup emit over c! char+ ELSE r> drop bell THEN
   THEN 
  AGAIN ;
  
