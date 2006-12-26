\ image dump                                           15nov94py

\ Copyright (C) 1995,1997,2003 Free Software Foundation, Inc.

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

: save-mem-dict { addr1 u -- addr2 u }
    here { addr2 }
    u allot
    addr1 addr2 u move
    addr2 u ;

: update-image-included-files ( -- )
    included-files 2@ { addr cnt }
    image-included-files 2@ { old-addr old-cnt }
    align here { new-addr }
    cnt 2* cells allot
    new-addr cnt image-included-files 2!
    old-addr new-addr old-cnt 2* cells move
    cnt old-cnt
    U+DO
        addr i 2* cells + 2@ save-mem-dict
	new-addr i 2* cells + 2!
    LOOP
    maxalign ;

: dump-fi ( addr u -- )
    w/o bin create-file throw >r
    update-image-included-files
    update-image-order
    here forthstart - forthstart 2 cells + !
    forthstart
    begin \ search for start of file ("#! " at a multiple of 8)
	8 -
	dup 3 s" #! " str=
    until ( imagestart )
    here over - r@ write-file throw
    r> close-file throw ;

: savesystem ( "name" -- ) \ gforth
    name dump-fi ;
