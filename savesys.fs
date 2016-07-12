\ image dump                                           15nov94py

\ Copyright (C) 1995,1997,2003,2006,2007,2010,2011,2012 Free Software Foundation, Inc.

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

: delete-prefix ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 )
    \ if c-addr2 u2 is a prefix of c-addr1 u1, delete it
    2over 2over string-prefix? if
        nip /string
    else
        2drop
    endif ;

: update-image-included-files ( -- )
    included-files 2@ { addr cnt }
    image-included-files 2@ { old-addr old-cnt }
    align here { new-addr }
    cnt 2* cells allot
    new-addr cnt image-included-files 2!
    old-addr new-addr old-cnt 2* cells move
    cnt old-cnt
    U+DO
        addr i 2* cells + 2@
        s" GFORTHDESTDIR" getenv delete-prefix save-mem-dict
	new-addr i 2* cells + 2!
    LOOP
    maxalign ;

: update-maintask ( -- )
    throw-entry main-task udp @ throw-entry next-task - /string move ;

: prepare-for-dump ( -- )
    update-image-included-files
    update-image-order
    default-recognizer $save
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
