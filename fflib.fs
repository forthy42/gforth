\ lib.fs	shared library support package 		16aug03py

\ Copyright (C) 1995,1996,1997,1998,2000,2003 Free Software Foundation, Inc.

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
Variable revdec  revdec off
\ turn revdec on to compile bigFORTH libraries

Vocabulary c-decl
Vocabulary cb-decl

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
    Create proc, 0 also c-decl
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

: rettype ( endxt startxt "name" -- )
    create immediate 2,
  DOES>
    2@ compile, >r
    revdec @ IF  0 >r
	BEGIN  dup  WHILE  >r  REPEAT drop
	BEGIN  r> dup  WHILE  compile,  REPEAT  drop
    ELSE
	BEGIN  dup  WHILE  compile,  REPEAT drop
    THEN
    r> compile,  postpone EXIT
    here thisproc @ 2 cells + ! bl sword s,
    thislib @ thisproc @ @proc previous ;

also c-decl definitions

' av-int AConstant int
' av-float AConstant sf
' av-double AConstant df
' av-longlong AConstant llong
' av-ptr AConstant ptr

' av-call-void ' av-start-void rettype (void)
' av-call-int ' av-start-int rettype (int)
' av-call-float ' av-start-float rettype (sf)
' av-call-double ' av-start-double rettype (fp)
' av-call-longlong ' av-start-longlong rettype (llong)
' av-call-ptr ' av-start-ptr rettype (ptr)

previous definitions

\ legacy interface for old library interface

also c-decl

: (int...) ( n -- )
    >r ' execute r> 0 ?DO  int  LOOP
    0 postpone Literal postpone ?DO postpone int postpone LOOP
    postpone (int) ;
: (void...) ( n -- )
    >r ' execute r> 0 ?DO  int  LOOP
    0 postpone Literal postpone ?DO postpone int postpone LOOP
    postpone (void) ;
: (float...) ( n -- )
    >r ' execute r> 0 ?DO  df  LOOP
    0 postpone Literal postpone ?DO postpone df postpone LOOP
    postpone (fp) ;
: (int) ( n -- )
    >r ' execute r> 0 ?DO  int  LOOP  postpone (int) ;
: (void) ( n -- )
    >r ' execute r> 0 ?DO  int  LOOP  postpone (void) ;
: (float) ( n -- )
    >r ' execute r> 0 ?DO  df   LOOP  postpone (fp) ;

previous

\ callback stuff

Variable callbacks
\G link between callbacks

: callback ( -- )
    Create  0 ] postpone >r also cb-decl
  DOES>
    Create here >r 0 , callbacks @ A, r@ callbacks !
    swap postpone Literal postpone call , postpone EXIT
    r> dup cell+ cell+ alloc-callback swap !
  DOES> @ ;

: callback; ( 0 xt1 .. xtn -- )
    BEGIN  over  WHILE  compile,  REPEAT
    postpone r> postpone execute compile, drop
    postpone EXIT postpone [ previous ; immediate

: va-ret ( xt xt -- )
    Create A, A, immediate
  DOES> 2@ compile, ;

: init-callbacks ( -- )
    defers 'cold  callbacks 1 cells -
    BEGIN  cell+ @ dup  WHILE  dup cell+ cell+ alloc-callback over !
    REPEAT  drop ;

' init-callbacks IS 'cold

also cb-decl definitions

\ arguments

' va-arg-int      Alias int
' va-arg-float    Alias sf
' va-arg-double   Alias df
' va-arg-longlong Alias llong
' va-arg-ptr      Alias ptr

' va-return-void     ' va-start-void     va-ret (void)
' va-return-int      ' va-start-int      va-ret (int)
' va-return-float    ' va-start-float    va-ret (sf)
' va-return-double   ' va-start-double   va-ret (fp)
' va-return-longlong ' va-start-longlong va-ret (llong)
' va-return-ptr      ' va-start-ptr      va-ret (ptr)

previous definitions

\ testing stuff

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

\ example for a windows callback
    
callback wincall (int) int int int int callback;

:noname ( a b c d -- e )  2drop 2drop 0 ; wincall do_timer

\ test a callback

callback 2:1 (int) int int callback;

: cb-test ( a b -- c )
    cr ." Testing callback"
    cr ." arguments: " .s
    cr ." result " + .s cr ;
' cb-test 2:1 c_plus

: test  c_plus av-start-int av-int av-int av-call-int ;

\ 3 4 test

[then]    
