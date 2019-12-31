\ LOOK.FS      xt -> lfa                               22may93jaw

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke
\ Copyright (C) 1995,1996,1997,2000,2003,2007,2011,2012,2013,2014,2015,2017,2019 Free Software Foundation, Inc.

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

\ Look checks first if the word is a primitive. If yes then the
\ vocabulary in the primitive area is beeing searched, meaning
\ creating for each word a xt and comparing it...

\ If a word is no primitive look searches backwards to find the nfa.
\ Problems: A compiled xt via compile, might be created with noname:
\           a noname: leaves now a empty name field

require stuff.fs
require environ.fs

decimal

\ look                                                  17may93jaw

: xt= ( ca xt -- flag )
    \G compare threaded-code cell with the primitive xt
    first-throw @ >r first-throw off
    @ swap threading-method 1 umin 0 +DO  ['] @ catch drop  LOOP  =
    r> first-throw ! ;

: threaded>xt ( ca -- xt|0 )
    \G For the code address ca of a primitive, find the xt (or 0).
    [IFDEF] decompile-prim
	decompile-prim
    [THEN]
    \ walk through the array of primitive CAs
    >r ['] image-header >link @ begin
	dup while
	    r@ over xt= if
		rdrop exit
	    endif
	    >link @
    repeat
    drop rdrop 0 ;

\ !!! nicht optimal!
[IFUNDEF] look
has? ec [IF]

has? rom 
[IF]
: search-name  ( xt startlfa -- nt|0 )
    \ look up name of primitive with code at xt
    swap
    >r false swap
    BEGIN
	>link @ dup
    WHILE
	    dup name>int
	    r@ = IF
		nip dup
	    THEN
    REPEAT
    drop rdrop ;

: prim>name ( xt -- nt|0 )
    forth-wordlist @ search-name ;

: look ( xt -- lfa flag )
    dup [ unlock rom-dictionary area lock ] 
    literal literal within
    IF
	>head-noprim dup ?? <>
    ELSE
	prim>name dup 0<>
    THEN ;
[ELSE]
: look ( cfa -- lfa flag )
    >head-noprim dup ['] ??? <> ;
[THEN]

[ELSE]

: PrimStart ['] true >head-noprim ;

: look ( xt -- nt flag )
    dup xt? IF  dup name>string nip 0>  ELSE  0  THEN ;

[THEN]
[THEN]

: >name ( xt -- nt|0 ) \ gforth to-name
    \G The primary name token @i{nt} of the word represented by
    \G @i{xt}.  Returns 0 if @i{xt} is not an xt (using a heuristic
    \G check that has a small chance of misidentifying a non-xt as
    \G xt), or if the primary nt is of an unnamed word.  As of Gforth
    \G 1.0, every xt has a primary nt, but other named words may have
    \G the same interpretation sematics xt.
    look and ;

synonym xt>name noop ( xt -- nt ) \ gforth xt-to-name
\G Produces the primary nt for an xt.  If @i{xt} is not an xt, @i{nt}
\G is not guaranteed to be an nt.

: threaded>name ( ca -- nt|0 )
    threaded>xt >name ;

' >name ALIAS >head \ gforth to-head
' >name Alias prim>name
\G another name of @code{>name}

\ print recognizer stack

[IFDEF] forth-recognizer
    : .recs ( -- )
	get-recognizers 0 ?DO
	    >name .name
	LOOP ;
[THEN]
