\ File specifiers                                       11jun93jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007 Free Software Foundation, Inc.

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

\ BIN WRITE-LINE                                        11jun93jaw

: write-line ( c-addr u fileid -- ior ) \ file
    dup >r write-file
    ?dup IF
	r> drop EXIT
    THEN
    newline r> write-file ;

\ include-file                                         07apr93py

has? new-input 0= [IF]
: loadfilename>r ( addr1 u1 -- R: addr2 u2 )
    r> loadfilename 2@ 2>r >r
    loadfilename 2! ;

: r>loadfilename ( R: addr u -- )
    r> 2r> loadfilename 2! >r ;

: push-file  ( -- )  r>
    #fill-bytes @ >r
    loadline @    >r
    loadfile @    >r
    blk @         >r
    tibstack @    >r
    >tib @        >r
    #tib @        >r
    >in @         >r  >r
    >tib @ tibstack @ = IF  #tib @ tibstack +!  THEN
    tibstack @ >tib ! ;

: pop-file   ( throw-code -- throw-code )
  dup IF
      input-error-data >error
  THEN
  r>
  r> >in         !
  r> #tib        !
  r> >tib        !
  r> tibstack    !
  r> blk         !
  r> loadfile    !
  r> loadline    !
  r> #fill-bytes !  >r ;

: read-loop ( i*x -- j*x )
  BEGIN  refill  WHILE  interpret  REPEAT ;

: include-file1 ( i*x wfileid -- j*x ior1 ior2 )
    \G Interpret (process using the text interpreter) the contents of
    \G the file @var{wfileid}.
    push-file  loadfile !
    0 loadline ! blk off  ['] read-loop catch
    loadfile @ close-file swap 2dup or
    pop-file  drop ;

: include-file2 ( i*x wfileid -- j*x )
    \ like include-file, but does not update loadfile#
    include-file1 throw throw ;

: include-file ( i*x wfileid -- j*x ) \ file
    s" *a file*" loadfilename>r
    include-file1
    r>loadfilename
    throw throw ;
[THEN]
    
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
		." warning: ')' missing" cr
	    THEN
	    EXIT
	THEN
    REPEAT ; immediate

Redefinitions-end
