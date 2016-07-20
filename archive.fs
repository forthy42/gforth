\ very simple archive format                          29jul2012py

\ Copyright (C) 2012,2013 Free Software Foundation, Inc.

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

\ the intention for this format is to ship Gforth's files to
\ limited systems in a very simple format (simpler than tar)
\ so that unpacking is totally trivial.
\ File system capabilities expected are similar to vfat
\ no date stamp is given
\ the file is supposed to be compressed with zlib afterwards

\ Format of each file:
\ 32 bit len type filename\0 - counted+zero-terminated, aligned to 4 bytes
\ 32 bit size, little endian, aligend to 4 bytes
\ file contents, plus alignment to 4 bytes
\ type is:
\ 'f' for file,
\ 'd' for directory,
\ 's' for symlink,
\ 'h' for hardlink
\ rules for directories are: Specify each before first use

require unix/filestat.fs
require unix/libc.fs

4 buffer: fsize
file-stat buffer: statbuf

: .len ( n -- )  fsize le-l! fsize 4 type ;
: .z ( -- )  0 .len ;
: .entry ( addr u char -- addr u )
    >r dup 2 + .len r> emit 2dup type 0 emit ;

: -scan ( addr u char -- addr' u' )
  >r  BEGIN  dup  WHILE  1- 2dup + c@ r@ =  UNTIL  THEN
  rdrop ;

wordlist constant dirs

: :dir ( addr u -- )
    get-current >r dirs set-current nextname create r> set-current ;

"." :dir \ no need to create .

: ?dir ( addr u -- )
    '/' -scan dup 0= IF  2drop  EXIT  THEN
    2dup dirs search-wordlist 0= IF
	2dup recurse
	'd' .entry .z :dir
    ELSE
	drop 2drop
    THEN ;

: ?symlink ( addr u -- flag )
    statbuf lstat ?ior  statbuf st_mode w@ S_IFMT and S_IFLNK = ;

: dump-a-file ( addr u -- )
    2dup ?dir  2dup + 1- c@ '/' = ?EXIT
    2dup ?symlink IF  's' .entry  pad $200 readlink dup ?ior
	pad swap dup .len type  EXIT  THEN
    'f' .entry slurp-file dup .len 2dup type drop free throw ;

: dump-files ( -- )
    BEGIN  argc @ 1 >  WHILE
	    next-arg dump-a-file
    REPEAT ;

script? [IF]
    argc @ 1 = [IF]
	." call archive.fs <file list> >output" cr bye
    [ELSE]
	dump-files bye
    [THEN]
[THEN]
