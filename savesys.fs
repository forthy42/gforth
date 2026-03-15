\ image dump                                           15nov94py

\ Copyright (C) 1995,1997,2003,2006,2007 Free Software Foundation, Inc.

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

: save-mem-dict { addr1 u -- addr2 u }
    here { addr2 }
    u allot
    addr1 addr2 u move
    addr2 u ;

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

Defer prepare-for-dump
:noname
    here forthstart - forthstart 2 cells + !
    forthstart 12 cells + 4 cells erase \ stack base addresses are not relevant
    sp0 pad 6 cells move  sp0 6 cells erase
    current-input @ pad 6 cells + !  current-input off
    outfile-id pad 7 cells + !  0 to outfile-id
    infile-id pad 8 cells + !  0 to infile-id
    0 0 pathstring 2!  0 argv ! \ no need for this
    0 to fpath  0 0 included-files 2!
    HashPointer @ pad 9 cells + !  HashPointer off
    HashTable pad 10 cells + ! 0 to HashTable
    0 to history
    block-buffers @ pad 11 cells + !  block-buffers off
; is prepare-for-dump

Defer restore-for-dump
:noname
    pad sp0 6 cells move
    pad 6 cells + @ current-input !
    pad 7 cells + @ to outfile-id
    pad 8 cells + @ to infile-id
    pad 9 cells + @ HashPointer !
    pad 10 cells + @ to HashTable
    pad 11 cells + @ block-buffers !
; is restore-for-dump

: dump-fi ( addr u -- )
    w/o bin create-file throw >r
    update-image-included-files
    update-image-order
    prepare-for-dump
    forthstart
    begin \ search for start of file ("#! " at a multiple of 8)
	8 -
	dup 3 s" #! " str=
    until ( imagestart )
    here over - r@ write-file throw
    r> close-file throw
    restore-for-dump ;

: savesystem ( "name" -- ) \ gforth
    name dump-fi ;
