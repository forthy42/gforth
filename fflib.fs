\ lib.fs	shared library support package 		11may97py

\ Copyright (C) 1995,1996,1997,1998,2000 Free Software Foundation, Inc.

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

Variable libs 0 libs !
Variable thisproc
Variable thislib
\G links between libraries

: @lib ( lib -- )
    \G obtains library handle
    cell+ dup 2 cells + count open-lib
    dup 0= abort" Library not found" swap ! ;

: @proc ( lib addr -- )
    \G obtains symbol address
    cell+ tuck cell+ @ count rot cell+ @
    lib-sym  dup 0= abort" Proc not found!" swap ! ;

: proc, ( lib -- )
\G allocates and initializes proc stub
\G stub format:
\G    linked list in library
\G    address of proc
\G    ptr to OS name of symbol as counted string
\G    threaded code for invocation
    here dup thisproc !
    swap 2 cells + dup @ A, !
    0 , 0 A, ;

: proc:  ( lib "name" -- )
    \G Creates a named proc stub
    Create proc, 0
DOES> ( x1 .. xn -- r )
    dup cell+ @ swap 3 cells + >r ;

: library ( "name" "file" -- )
    \G loads library "file" and creates a proc defining word "name"
    \G library format:
    \G    linked list of libraries
    \G    library handle
    \G    linked list of library's procs
    \G    OS name of library as counted string
    Create  here libs @ A, dup libs !
    0 , 0 A, bl sword string, @lib
DOES> ( -- )  dup thislib ! proc: ;

: init-shared-libs ( -- )
    defers 'cold  libs
    0  libs  BEGIN  @ dup  WHILE  dup  REPEAT  drop
    BEGIN  dup  WHILE  >r
	r@ @lib
	r@ 2 cells +  BEGIN  @ dup  WHILE  r@ over @proc  REPEAT
	drop rdrop
    REPEAT  drop ;

' init-shared-libs IS 'cold

' av-int AConstant int
' av-float AConstant sf
' av-double AConstant df
' av-longlong AConstant llong
' av-ptr AConstant ptr

Variable revdec  revdec off
\ turn revdec on to compile bigFORTH libraries

: rettype ( endxt startxt "name" -- )
    create immediate 2,
  DOES>
    2@ compile, >r
    revdec @ IF
	0 >r  BEGIN  dup  WHILE  >r  REPEAT  drop
	BEGIN  r> dup  WHILE  compile,  REPEAT  drop
    ELSE
	BEGIN dup  WHILE  compile,  REPEAT  drop
    THEN
    r> compile,  postpone EXIT
    here thisproc @ 2 cells + ! bl sword s,
    thislib @ thisproc @ @proc ;

' av-call-void ' av-start-void rettype (void)
' av-call-int ' av-start-int rettype (int)
' av-call-float ' av-start-float rettype (sf)
' av-call-double ' av-start-double rettype (fp)
' av-call-longlong ' av-start-longlong rettype (llong)
' av-call-ptr ' av-start-ptr rettype (ptr)

\ compatibility layer for old library -- use is deprecated

Variable legacy

\ turn legacy on for old library

warnings @ warnings off

: (int) ( n -- )
    legacy @ IF
	>r ' execute r> 0 ?DO  int  LOOP
    THEN  (int) ;
: (void) ( n -- )
    legacy @ IF
	>r ' execute r> 0 ?DO  int  LOOP
    THEN  (void) ;
: (float) ( n -- )
    legacy @ IF
	>r ' execute r> 0 ?DO  df  LOOP
    THEN  (df) ;

warnings on

[ifdef] testing

library libc /lib/libc.so.6
                
libc sleep int (int) sleep
libc open  int int ptr (int) open
libc lseek int llong int (llong) lseek
libc read  int ptr int (int) read
libc close int (int) close

library libm /lib/libm.so.6

libm fmodf sf sf (sf) fmodf
libm fmod  df df (fp) fmod

[then]    
