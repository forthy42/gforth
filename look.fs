\ LOOK.FS      xt -> lfa                               22may93jaw

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke
\ Copyright (C) 1995,1996,1997,2000,2003,2007,2011,2012,2013,2014,2015,2017,2019,2021,2023,2024 Free Software Foundation, Inc.

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
    \ compare threaded-code cell with the primitive xt
    >code-address swap threading-method IF
	['] >code-address catch-nobt drop
    THEN  = ;

: threaded>xt ( ca -- xt|0 )
    \ Given the static code address of a primitive (i.e., coming from
    \ @decompile-prim), xt is the xt of the primitive there.  Return 0
    \ if there is no primitive there.
    \
    \ walk through the array of primitive CAs
    >r ['] image-header >link @ begin
	dup while
	    r@ over xt= if
		rdrop exit
	    endif
	    >link @
    repeat
    drop rdrop 0 ;

: @threaded>xt ( a-addr -- xt|0 ) \ gforth-internal
    \G Given a threaded-code address a-addr, xt is the xt of the
    \G primitive there.  Return 0 if there is no primitive there.
    [IFDEF] @decompile-prim
        @decompile-prim
    [ELSE]
        @
    [THEN]
    threaded>xt ;

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
    \G @i{nt} is the primary name token of the word represented by
    \G @i{xt}.  Returns 0 if @i{xt} is not an xt (using a heuristic
    \G check that has a small chance of misidentifying a non-xt as
    \G xt), or (before Gforth 1.0) if the primary nt is of an unnamed
    \G word.  As of Gforth 1.0, every xt has a primary nt.  Several
    \G words can have the same xt, but only one of them has the
    \G primary nt of that xt.
    look and ;

: threaded>name ( ca -- nt|0 )
    \ for static cas only
    threaded>xt >name ;

: @threaded>name ( a-addr -- nt|0 )
    @threaded>xt >name ;

' >name ALIAS >head \ gforth to-head
' >name Alias prim>name

\ print recognizer stack

[IFDEF] forth-recognize
    : .recognizer-sequence ( recognizer -- )
	get-recognizer-sequence 0 ?DO
	    dup defers@ >does-code ['] recognize =
	    IF  dup >r  ELSE  0 >r  THEN
	    dup >voc >does-code [ ' forth >does-code ] Literal = IF
		>voc
	    THEN
\	    name>string 2dup s" rec-" string-prefix? IF
\		4 /string  9 attr! ." ~"  0 attr!
\	    THEN  type space
	    id.  r> ?dup-IF
		." ( " recurse ." ) "
	    THEN
	LOOP ;
    : .recognizers ( -- ) \ gforth-experimental dot-recognizers
        \G Print the current recognizer order, with the first-searched
	\G recognizer leftmost (unlike .order).  The inverted @code{~} is
	\G displayed instead of @code{rec-}, which is the common prefix
	\G of all recognizers.
	['] forth-recognize .recognizer-sequence ;
[THEN]
