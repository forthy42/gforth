\ require.fs

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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

\ Now: Kernel Module, Reloadable

create included-files 0 , 0 , ( pointer to and count of included files )
\ here ," ./the terminal" dup c@ swap 1 + swap , A, here 2 cells -
\ ./ is confusing for the search path stuff! There should be never a .
\ in sourcefilename....
here ," #terminal#" dup c@ swap 1 + swap , A, here 2 cells -
create image-included-files  1 , A, ( pointer to and count of included files )
\ included-files points to ALLOCATEd space, while image-included-files
\ points to ALLOTed objects, so it survives a save-system

: loadfilename ( -- a-addr ) \ gforth
    \G @i{a-addr} @code{2@@} produces the current file name ( @i{c-addr u} )
    included-files 2@ loadfilename# @ min 2* cells + ;

: sourcefilename ( -- c-addr u ) \ gforth
    \G The name of the source file which is currently the input
    \G source.  The result is valid only while the file is being
    \G loaded.  If the current input source is no (stream) file, the
    \G result is undefined.
    loadfilename 2@ ;

: sourceline# ( -- u ) \ gforth		sourceline-number
    \G The line number of the line that is currently being interpreted
    \G from a (stream) file. The first line has the number 1. If the
    \G current input source is not a (stream) file, the result is
    \G undefined.
    loadline @ ;

: init-included-files ( -- ) \ gforth
    \G Clear the list of earlier included files.
    image-included-files 2@ 2* cells save-mem drop ( addr )
    image-included-files 2@ nip included-files 2! ;

: included? ( c-addr u -- f ) \ gforth
    \G True only if the file @var{c-addr u} is in the list of earlier
    \G included files. If the file has been loaded, it may have been
    \G specified as, say, @file{foo.fs} and found somewhere on the
    \G Forth search path. To return @code{true} from @code{included?},
    \G you must specify the exact path to the file, even if that is
    \G @file{./foo.fs}
    included-files 2@ 0
    ?do ( c-addr u addr )
	dup >r 2@ 2over compare 0=
	if
	    2drop rdrop unloop
	    true EXIT
	then
	r> cell+ cell+
    loop
    2drop drop false ;

: add-included-file ( c-addr u -- ) \ gforth
    \G add name c-addr u to included-files
    included-files 2@ 2* cells 2 cells extend-mem
    2/ cell / included-files 2!
    2! ;

: included1 ( i*x file-id c-addr u -- j*x ) \ gforth
    \G Include the file file-id with the name given by @var{c-addr u}.
    loadfilename# @ >r
    save-mem add-included-file ( file-id )
    included-files 2@ nip 1- loadfilename# !
    ['] include-file catch
    r> loadfilename# !
    throw ;
    
: included ( i*x c-addr u -- j*x ) \ file
    \G @code{include-file} the file whose name is given by the string
    \G @var{c-addr u}.
    open-fpath-file throw included1 ;

: required ( i*x addr u -- j*x ) \ gforth
    \G @code{include-file} the file with the name given by @var{addr
    \G u}, if it is not @code{included} (or @code{required})
    \G already. Currently this works by comparing the name of the file
    \G (with path) against the names of earlier included files.
    \ however, it may be better to fstat the file,
    \ and compare the device and inode. The advantages would be: no
    \ problems with several paths to the same file (e.g., due to
    \ links) and we would catch files included with include-file and
    \ write a require-file.
    open-fpath-file throw 2dup included?
    if
	2drop close-file throw
    else
	included1
    then ;

\ INCLUDE                                               9may93jaw

: include  ( ... "file" -- ... ) \ gforth
    \G @code{include-file} the file @var{file}.
    name included ;

: require  ( ... "file" -- ... ) \ gforth
    \G @code{include-file} @var{file} only if it is not included already.
    name required ;

0 [IF]
: \I
  here 
  0 word count
  string,
  needsrcs^ @ ! ;

: .modules
  cr
  needs^ @
  BEGIN		dup 
  WHILE		dup cell+ count type cr
		5 spaces
		dup cell+ count + aligned
		@ dup IF count type ELSE drop THEN cr
		@
  REPEAT
  drop ;

: loadfilename#>str ( n -- adr len )
\ this converts the filenumber into the string
  loadfilenamecount @ swap -
  needs^ @
  swap 0 ?DO dup 0= IF LEAVE THEN @ LOOP 
  dup IF cell+ count ELSE drop s" NOT FOUND" THEN ;
[THEN]

: loadfilename#>str ( n -- adr len )
    included-files 2@ drop swap 2* cells + 2@ ;

: .modules
    included-files 2@ 2* cells bounds ?DO
	cr I 2@ type  2 cells +LOOP ;  

\ contains tools/newrequire.fs
\ \I $Id: require.fs,v 1.9 1999-12-03 18:49:52 crook Exp $

