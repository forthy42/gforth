\ File specifiers                                       11jun93jaw

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

: push-file  ( -- )  r>
    loadline @ >r
    loadfile @ >r
    blk @      >r
    tibstack @ >r
    >tib @     >r
    #tib @     >r
    >in @      >r  >r
    >tib @ tibstack @ = IF  #tib @ tibstack +!  THEN
    tibstack @ >tib ! ;

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
  r> >in      !
  r> #tib     !
  r> >tib     !
  r> tibstack !
  r> blk      !
  r> loadfile !
  r> loadline !  >r ;

: read-loop ( i*x -- j*x )
  BEGIN  refill  WHILE  interpret  REPEAT ;

: include-file ( i*x wfileid -- j*x ) \ file
    \G Interpret (process using the text interpreter) the contents of
    \G the file @var{wfileid}.
    push-file  loadfile !
    0 loadline ! blk off  ['] read-loop catch
    loadfile @ close-file swap 2dup or
    pop-file  drop throw throw ;

\ additional words only needed if there is file support

Warnings off

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

Warnings on
