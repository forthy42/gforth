\ LOOK.FS      xt -> lfa                               22may93jaw

\ Copyright (C) 1995,1996,1997,2000,2003,2007 Free Software Foundation, Inc.

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

\ rename to discover!!!

: xt>threaded ( xt -- x )
\G produces the threaded-code cell for the primitive xt
    threading-method 0= if
	@
    then ;

: search-name  ( xt startlfa -- nt|0 )
    \ look up name of primitive with code at xt
    swap
    >r false swap
    BEGIN
	@ dup
    WHILE
	    dup name>int
	    r@ = IF
		nip dup
	    THEN
    REPEAT
    drop rdrop ;

: threaded>xt ( ca -- xt|0 )
\G For the code address ca of a primitive, find the xt (or 0).
    [IFDEF] decompile-prim
	decompile-prim
    [THEN]
     \ walk through the array of primitive CAs
    >r ['] noop begin
	dup @ while
	    dup xt>threaded r@ = if
		rdrop exit
	    endif
	    cell+
    repeat
    drop rdrop 0 ;

\ !!! nicht optimal!
[IFUNDEF] look
has? ec [IF]

has? rom 
[IF]
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
    >head-noprim dup ??? <> ;
[THEN]

[ELSE]

: PrimStart ['] true >head-noprim ;

: prim>name ( xt -- nt|0 )
    PrimStart search-name ;

: look ( xt -- lfa flag )
    dup in-dictionary?
    IF
	>head-noprim dup ??? <>
    ELSE
	prim>name dup 0<>
    THEN ;

[THEN]
[THEN]

: threaded>name ( ca -- nt|0 )
    threaded>xt prim>name ;

: >name ( xt -- nt|0 ) \ gforth to-name
    \G tries to find the name token @var{nt} of the word represented
    \G by @var{xt}; returns 0 if it fails.  This word is not
    \G absolutely reliable, it may give false positives and produce
    \G wrong nts.
    look and ;

' >name ALIAS >head \ gforth to-head
\G another name of @code{>name}
