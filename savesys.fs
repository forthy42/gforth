\ image dump                                           15nov94py

\ Copyright (C) 1995,1997,2003,2006,2007,2010,2011,2012,2016 Free Software Foundation, Inc.

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

: update-image-included-files ( -- )
    included-files $save
    s" GFORTHDESTDIR" getenv  included-files $@ bounds ?DO
	I @ in-dictionary? 0= IF
	    2dup I $@ string-prefix? IF
		I 0 2 pick $del  THEN
	    I $save
	THEN
    cell +LOOP  2drop maxalign ;

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
    
: dump-fi ( c-addr u -- )
    prepare-for-dump
    here forthstart - forthstart 2 cells + !
    w/o bin create-file throw >r
    preamble-start here over - r@ write-file throw
    r> close-file throw ;

: savesystem ( "name" -- ) \ gforth
    name dump-fi ;
