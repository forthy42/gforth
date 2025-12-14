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
    \G For most words (all words with the default implementation of
    \G @word{name>interpret}), @word{>name} is the inverse of
    \G @word{name>interpret}: for these words @code{@i{nt}
    \G name>interpret} produces @i{xt}.  For the other words
    \G @word{name} produces an nt for which @code{@i{nt}
    \G default-name>int} produces @i{xt}.  Returns 0 if @i{xt} is not
    \G an xt (using a heuristic check that has a small chance of
    \G misidentifying a non-xt as xt), or (before Gforth 1.0) if
    \G @i{xt} is of an unnamed word.  As of Gforth 1.0, unnamed words
    \G have nts, too, and @word{>name} produces an nt for xts of
    \G unnamed words.
    look and ;

: threaded>name ( ca -- nt|0 )
    \ for static cas only
    threaded>xt >name ;

: @threaded>name ( a-addr -- nt|0 )
    @threaded>xt >name ;

' >name ALIAS >head \ gforth to-head
' >name Alias prim>name

\ print recognizer stack

: .recognizer-sequence ( recognizer -- )
    get-recs 0 ?DO
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
: recs ( -- ) \ gforth-experimental
    \G Print the system recognizer order, with the first-searched
    \G recognizer leftmost.  For recognizer sequences, first the name
    \G is printed, then @samp{(}, then the content of the sequence,
    \G then @samp{)}.  For a deferred word, the name of the deferred
    \G word is shown, not that of the recognizer inside; if it
    \G contains a recognizer sequence, the name of the deferred word
    \G and the contents of the sequence are shown.
    ['] rec-forth .recognizer-sequence ;
