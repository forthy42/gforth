\ Input                                                13feb93py

\ Copyright (C) 1995-1997 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

: (ins) ( max span addr pos1 key -- max span addr pos2 )
    >r 2dup + r@ swap c! r> emit 1+ rot 1+ -rot ;
: (bs) ( max span addr pos1 -- max span addr pos2 flag )
    dup IF
	#bs emit bl emit #bs emit 1- rot 1- -rot
    THEN false ;
: (ret)  true bl emit ;

Create ctrlkeys
  ] false false false false  false false false false
    (bs)  false (ret) false  false (ret) false false
    false false false false  false false false false
    false false false false  false false false false [

defer insert-char
' (ins) IS insert-char
defer everychar
' noop IS everychar

: decode ( max span addr pos1 key -- max span addr pos2 flag )
  everychar
  dup #del = IF  drop #bs  THEN  \ del is rubout
  dup bl <   IF  cells ctrlkeys + perform  EXIT  THEN
  >r 2over = IF  rdrop bell 0 EXIT  THEN
  r> insert-char 0 ;

: accept   ( addr len -- len ) \ core
  dup 0< IF    abs over dup 1 chars - c@ tuck type 
\ this allows to edit given strings
  ELSE  0  THEN rot over
  BEGIN  key decode  UNTIL
  2drop nip ;

