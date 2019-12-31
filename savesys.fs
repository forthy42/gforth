\ image dump                                           15nov94py

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1995,1997,2003,2006,2007,2010,2011,2012,2016,2017,2018,2019 Free Software Foundation, Inc.

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

: del-included-files ( addr u -- )
    included-files $@ bounds ?DO
	I $@ 2over string-prefix? IF  I 0 2 pick $del  THEN
    cell +LOOP  2drop ;

: repl-included-files ( addr1 u1 addr2 u2 -- )
    included-files $@ bounds ?DO
	2over I $@ 2swap string-prefix? IF
	    I 0 4 pick $del  2dup I 0 $ins
	THEN
    cell +LOOP  2drop 2drop ;

: update-image-included-files ( -- )
    s" GFORTHDESTDIR" getenv del-included-files ;

: update-maintask ( -- )
    throw-entry main-task udp @ throw-entry next-task - /string move ;

: prepare-for-dump ( -- )
    update-image-included-files
    'image
    update-maintask ;

: preamble-start ( -- addr )
    \ dump the part from "#! /..." to FORTHSTART
    forthstart begin \ search for start of file ("#! " at a multiple of 8)
	8 -
	dup 4 s" #! /" str=
    until ( imagestart ) ;

[IFUNDEF] dump-sections
    Defer dump-sections ' drop is dump-sections
[THEN]

: dump-fi ( c-addr u -- )
    prepare-for-dump
    w/o bin create-file throw >r
    preamble-start here over - r@ write-file throw
    r@ dump-sections
    r> close-file throw ;

: savesystem ( "name" -- ) \ gforth
    name dump-fi ;
