\ LOOK.FS      xt -> lfa                               22may93jaw

\ Copyright (C) 1995,1996,1997,2000 Free Software Foundation, Inc.

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

: (look)  ( xt startlfa -- lfa flag )
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
    drop rdrop
    dup 0<> ;

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
: prim>name ( xt -- nt flag )
    forth-wordlist @ (look) ;

: look
    dup [ unlock rom-dictionary area lock ] 
    literal literal within
    IF
	>head-noprim dup ?? <>
    ELSE
	xt>threaded threaded>name
    THEN ;
[ELSE]
: look ( cfa -- lfa flag )
    >head-noprim dup ??? <> ;
[THEN]

[ELSE]

: PrimStart ['] true >head-noprim ;

: prim>name ( xt -- lfa flag )
    PrimStart (look) ;

: look ( cfa -- lfa flag )
    dup in-dictionary?
    IF
	>head-noprim dup ??? <>
    ELSE
	prim>name
    THEN ;

[THEN]
[THEN]

: threaded>name ( ca -- lfa flag )
    threaded>xt prim>name ;

: >head ( cfa -- nt|0 ) \ gforth to-head
    \G tries to find the name token nt of the word represented by cfa;
    \G returns 0 if it fails.  This word is not absolutely reliable,
    \G it may give false positives and produce wrong nts.
    look and ;

' >head ALIAS >name \ gforth to-name
\G old name of @code{>head}
