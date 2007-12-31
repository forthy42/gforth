\ vt100 key interpreter                                17oct94py

\ Copyright (C) 1995,1996,1998,2000,2003,2007 Free Software Foundation, Inc.

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

Create translate $100 allot
translate $100 erase
Create transcode $100 allot
transcode $100 erase

: trans: ( char 'index' -- ) char translate + c! ;
: tcode  ( char index -- ) transcode + c! ;

: vt100-decode ( max span addr pos1 -- max span addr pos2 flag )
    key '[ = IF  0  base @ >r  &10 base !
	BEGIN  key dup digit?  WHILE  nip swap &10 * +  REPEAT
	r> base !
	dup '~ =  IF  drop transcode  ELSE  nip translate  THEN
	+ c@ dup  IF  decode  THEN
    ELSE  0  THEN ;

ctrl B trans: D
ctrl F trans: C
ctrl P trans: A
ctrl N trans: B
ctrl A trans: H
ctrl E trans: F

ctrl A 1 tcode
ctrl X 3 tcode
ctrl E 4 tcode

' vt100-decode  ctrlkeys $1B cells + !
