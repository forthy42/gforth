\ File specifiers                                       11jun93jaw

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

4 Constant w/o ( -- fam ) \ file	w-o
2 Constant r/w ( -- fam ) \ file	r-w
0 Constant r/o ( -- fam ) \ file	r-o

: bin ( fam1 -- fam2 ) \ file
    1 or ;

\ BIN WRITE-LINE                                        11jun93jaw

: write-line ( c-addr u fileid -- ior ) \ file
    dup >r write-file
    ?dup IF
	r> drop EXIT
    THEN
    #lf r> emit-file ;

\ include-file                                         07apr93py

: push-file  ( -- )  r>
  sourceline# >r  loadfile @ >r
  blk @ >r  tibstack @ >r  >tib @ >r  #tib @ >r
  >tib @ tibstack @ = IF  r@ tibstack +!  THEN
  tibstack @ >tib ! >in @ >r  >r ;

: pop-file   ( throw-code -- throw-code )
  dup IF
         source >in @ sourceline# sourcefilename
	 error-stack dup @ dup 1+
	 max-errors 1- min error-stack !
	 6 * cells + cell+
	 5 cells bounds swap DO
	                    I !
	 -1 cells +LOOP
  THEN
  r>
  r> >in !  r> #tib !  r> >tib !  r> tibstack !  r> blk !
  r> loadfile ! r> loadline !  >r ;

: read-loop ( i*x -- j*x )
  BEGIN  refill  WHILE  interpret  REPEAT ;

: include-file ( i*x fid -- j*x ) \ file
  push-file  loadfile !
  0 loadline ! blk off  ['] read-loop catch
  loadfile @ close-file swap 2dup or
  pop-file  drop throw throw ;

create pathfilenamebuf 256 chars allot \ !! make this grow on demand

: absolut-path? ( addr u -- flag ) \ gforth
    \G a path is absolute, if it starts with a / or a ~ (~ expansion),
    \G or if it is in the form ./* or ../*, extended regexp: ^[/~]|./|../
    \G Pathes simply containing a / are not absolute!
    over c@ '/ = >r
    over c@ '~ = >r
    2dup 2 min S" ./" compare 0= >r
         3 min S" ../" compare 0=
    r> r> r> or or or ;

: open-path-file ( c-addr1 u1 -- file-id c-addr2 u2 ) \ gforth
    \G opens a file for reading, searching in the path for it (unless
    \G the filename contains a slash); c-addr2 u2 is the full filename
    \G (valid until the next call); if the file is not found (or in
    \G case of other errors for each try), -38 (non-existant file) is
    \G thrown. Opening for other access modes makes little sense, as
    \G the path will usually contain dirs that are only readable for
    \G the user
    \ !! use file-status to determine access mode?
    2dup absolut-path?
    IF \ the filename contains a slash
	2dup r/o open-file throw ( c-addr1 u1 file-id )
	-rot >r pathfilenamebuf r@ cmove ( file-id R: u1 )
	pathfilenamebuf r> EXIT
    THEN
    pathdirs 2@ 0
    ?DO ( c-addr1 u1 dirnamep )
	dup >r 2@ dup >r pathfilenamebuf swap cmove ( addr u )
	2dup pathfilenamebuf r@ chars + swap cmove ( addr u )
	pathfilenamebuf over r> + dup >r r/o open-file 0=
	IF ( addr u file-id )
	    nip nip r> rdrop 0 LEAVE
	THEN
	rdrop drop r> cell+ cell+
    LOOP
    0<> -&38 and throw ( file-id u2 )
    pathfilenamebuf swap ;

create included-files 0 , 0 , ( pointer to and count of included files )
here ," the terminal" dup c@ swap 1 + swap , A, here 2 cells -
create image-included-files  1 , A, ( pointer to and count of included files )
\ included-files points to ALLOCATEd space, while image-included-files
\ points to ALLOTed objects, so it survives a save-system

: loadfilename ( -- a-addr )
    \G a-addr 2@ produces the current file name ( c-addr u )
    included-files 2@ drop loadfilename# @ 2* cells + ;

: sourcefilename ( -- c-addr u ) \ gforth
    \G the name of the source file which is currently the input
    \G source.  The result is valid only while the file is being
    \G loaded.  If the current input source is no (stream) file, the
    \G result is undefined.
    loadfilename 2@ ;

: sourceline# ( -- u ) \ gforth		sourceline-number
    \G the line number of the line that is currently being interpreted
    \G from a (stream) file. The first line has the number 1. If the
    \G current input source is no (stream) file, the result is
    \G undefined.
    loadline @ ;

: init-included-files ( -- )
    image-included-files 2@ 2* cells save-mem drop ( addr )
    image-included-files 2@ nip included-files 2! ;

: included? ( c-addr u -- f ) \ gforth
    \G true, iff filename c-addr u is in included-files
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
    \G include the file file-id with the name given by c-addr u
    loadfilename# @ >r
    save-mem add-included-file ( file-id )
    included-files 2@ nip 1- loadfilename# !
    ['] include-file catch
    r> loadfilename# !
    throw ;
    
: included ( i*x addr u -- j*x ) \ file
    open-path-file included1 ;

: required ( i*x addr u -- j*x ) \ gforth
    \G include the file with the name given by addr u, if it is not
    \G included already. Currently this works by comparing the name of
    \G the file (with path) against the names of earlier included
    \G files; however, it would probably be better to fstat the file,
    \G and compare the device and inode. The advantages would be: no
    \G problems with several paths to the same file (e.g., due to
    \G links) and we would catch files included with include-file and
    \G write a require-file.
    open-path-file 2dup included?
    if
	2drop close-file throw
    else
	included1
    then ;

\ INCLUDE                                               9may93jaw

: include  ( "file" -- ) \ gforth
  name included ;

: require  ( "file" -- ) \ gforth
  name required ;

\ additional words only needed if there is file support

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
		." warning: ')' missing" cr
	    THEN
	    EXIT
	THEN
    REPEAT ; immediate

