\ wordlib.fs Handle shared library with forth primitive extentions 9oct97jaw

\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2007 Free Software Foundation, Inc.

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

\ Author: Jens Wilke
\ Revision Log
\ 09oct97jaw V1.0   Initial Version

\ ToDo:
\
\ Bootup initialization
\ "require" for libs

0 Value wlib 	\ temporary library handle to make live easy

: wl-catalog ( n lib-addr -- addr | u )
    s" catalog" rot lib-sym dup 0=
    ABORT" No word catalog"
    icall1 ;

: wl-words ( lib-addr -- )
    0 
    BEGIN 2dup swap wl-catalog ?dup
    WHILE cell+ count type space
	  1+
    REPEAT 2drop ;

: wl-create ( adr adr2 len2 -- )
    nextname
    Create ,
    DOES> @ wcall ;

: wl-tovoc ( lib-addr -- )
    0 
    BEGIN 2dup swap wl-catalog ?dup
    WHILE dup @ swap cell+ count wl-create
	  1+
    REPEAT 2drop ;

: (WordLibrary)
    Create DOES> @ ;

: WordLibrary ( "wordname" "libfilename" )
    (WordLibrary)
    \ open library with forth path
    bl word count open-fpath-file throw rot close-file throw
    \ open library with correct path
    open-lib
    dup 0= ABORT" Library load error"
    dup ,
    wl-tovoc ;
