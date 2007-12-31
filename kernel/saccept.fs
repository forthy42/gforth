\ a very simple accept approach

\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2006,2007 Free Software Foundation, Inc.

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

require ./io.fs

\ : xon $11 emit ;
\ : xoff $13 emit ;

Variable eof
Variable echo  -1 echo !

: accept ( adr len -- len )
  ( xon ) over + over ( start end pnt )  eof off
  BEGIN
   key dup #del = IF drop #bs THEN
   dup bl u<
   IF
       dup #cr = over #lf = or IF
	   echo @ IF  space  THEN  drop nip swap - ( xoff ) EXIT THEN
       dup #eof = IF  eof on  THEN
       #bs = IF 2 pick over <>
	   IF 1 chars -
	       echo @ IF  #bs emit bl emit #bs emit  THEN
	   ELSE  echo @ IF  bell  THEN  THEN  THEN
   ELSE	>r 2dup <> IF r>
	   echo @ IF  dup emit  THEN
	   over c! char+ ELSE r> drop bell THEN
   THEN 
  AGAIN ;
  
