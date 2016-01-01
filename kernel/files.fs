\ File specifiers                                       11jun93jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2012,2013,2014,2015 Free Software Foundation, Inc.

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

4 Constant w/o ( -- fam ) \ file	w-o
2 Constant r/w ( -- fam ) \ file	r-w
0 Constant r/o ( -- fam ) \ file	r-o

: bin ( fam1 -- fam2 ) \ file
    1 or ;

\ file creation attributes

\ default is rw-rw-rw-

: +fmode ( fam1 rwxrwxrwx -- fam2 )
    \G add file access mode to fam - for create-file only
    $1B6 xor 4 lshift or ;

\ BIN WRITE-LINE                                        11jun93jaw

: write-line ( c-addr u wfileid -- ior ) \ file
    dup >r write-file
    ?dup IF
	r> drop EXIT
    THEN
    newline r> write-file ;

\ additional words only needed if there is file support

Redefinitions-start

: ( ( compilation 'ccc<close-paren>' -- ; run-time -- ) \ core,file	paren
    loadfile @ 0= IF  postpone (  EXIT  THEN
    BEGIN
	>in @
	[char] ) parse nip
	>in @ rot - = \ is there no delimter?
    WHILE
	refill 0=
	IF
	    warnings @
	    IF
		>stderr warn-color attr!
		." warning: ')' missing" cr
		default-color attr!
	    THEN
	    EXIT
	THEN
    REPEAT ; immediate

Redefinitions-end
