\ lib.fs	shared library support package 		11may97py

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2005,2007 Free Software Foundation, Inc.

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

Create icall-table
    ] icall0 ;s icall1 ;s icall2 ;s icall3 ;s icall4 ;s icall5 ;s icall6 ;s
      NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  NIL swap
      NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  icall20 ;s [
Create fcall-table
    ] fcall0 ;s fcall1 ;s fcall2 ;s fcall3 ;s fcall4 ;s fcall5 ;s fcall6 ;s
      NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  NIL swap
      NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  NIL swap  fcall20 ;s [

Variable libs 0 libs !
\G links between libraries

Variable legacy  legacy off

: @lib ( lib -- )
    \G obtains library handle
    cell+ dup 2 cells + count open-lib
    dup 0= abort" Library not found" swap ! ;

: @proc ( lib addr -- )
    \G obtains symbol address
    cell+ tuck 2 cells + count rot cell+ @
    lib-sym  dup 0= abort" Proc not found!" swap cell+ ! ;

-1 Constant <addr>
 0 Constant <int>
 1 Constant <float>
 2 Constant <void>
 4 Constant <int...>
 5 Constant <float...>
 6 Constant <void...>

: proc, ( pars type lib addr -- )
    \G allocates and initializes proc stub
    \G stub format:
    \G    linked list in library
    \G    address of proc
    \G    offset in lcall1-table to call proc
    \G    OS name of symbol as counted string
    legacy @ IF  (int) -rot  THEN
    here 2dup swap 2 cells + dup @ A, !
    2swap  1 and  IF  fcall-table  ELSE  icall-table  THEN  swap
    cells 2* + , 0 , bl sword string, @proc ;

: proc:  ( pars type lib "name" "string" -- )
    \G Creates a named proc stub
    Create proc,
DOES> ( x1 .. xn -- r )
    cell+ 2@ >r ;

: vaproc:  ( pars type lib "name" "string" -- )
    \G Creates a named proc stub with variable arguments
    Create proc,
DOES> ( x1 .. xn n -- r )
    cell+ 2@ rot 2* cells + >r ;

: (>void)  >r ;

: vproc:  ( pars type lib "name" "string" -- )
    \G Creates a named proc stub for void functions
    Create proc,
DOES> ( x1 .. xn -- )
    cell+ 2@ (>void) drop ;

: vvaproc:  ( pars type lib "name" "string" -- )
    \G Creates a named proc stub with variable arguments, void return
    Create proc,
DOES> ( x1 .. xn n -- )
    cell+ 2@ rot 2* cells + (>void) drop ;

: label: ( type lib "name" "string" -- )
    \G Creates a named label stub
    -1 -rot Create proc,
DOES> ( -- addr )
    [ 2 cells ] Literal + @ ;

: library ( "name" "file" -- )
    \G loads library "file" and creates a proc defining word "name"
    \G library format:
    \G    linked list of libraries
    \G    library handle
    \G    linked list of library's procs
    \G    OS name of library as counted string
    Create  here libs @ A, dup libs !
    0 , 0 A, bl sword string, @lib
DOES> ( pars/ type -- )
    over -1 = IF  label:
    ELSE
	over 4 and IF
	    over 2 and IF  vvaproc:  ELSE  vaproc:  THEN
	ELSE
	    over 2 and IF  vproc:  ELSE  proc:  THEN
	THEN
    THEN ;

: init-shared-libs ( -- )
    defers 'cold  libs
    0  libs  BEGIN  @ dup  WHILE  dup  REPEAT  drop
    BEGIN  dup  WHILE  >r
	r@ @lib
	r@ 2 cells +  BEGIN  @ dup  WHILE  r@ over @proc  REPEAT
	drop rdrop
    REPEAT  drop ;

' init-shared-libs IS 'cold
