\ kernel.fs    GForth kernel                        17dec92py

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1998,2000,2003,2004,2005,2006,2007,2008,2010,2011,2012,2013,2014,2015,2016,2018,2019,2021,2022,2024 Free Software Foundation, Inc.

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

\ Idea and implementation: Bernd Paysan (py)

\ Needs:

require ./vars.fs
require ../compat/strcomp.fs

hex

\ labels for some code addresses

\- NIL NIL AConstant NIL \ gforth

\ Aliases

\ !! this is machine-dependent, but works on all but the strangest machines

: maxaligned ( addr1 -- addr2 ) \ gforth
    \G @i{addr2} is the first address after @i{addr1} that satisfies
    \G all alignment restrictions.
    [ /maxalign 1 - ] Literal + [ 0 /maxalign - ] Literal and ;
\ !! machine-dependent and won't work if "0 >body" <> "0 >body

' maxaligned Alias cfaligned ( addr1 -- addr2 ) \ gforth-internal
\G @i{addr2} is the first address after @i{addr1} that is aligned for
\G a code field (i.e., such that the corresponding body is maxaligned).

: chars ( n1 -- n2 ) \ core
\G @i{n2} is the number of address units of @i{n1} chars.
; immediate


\ : A!    ( addr1 addr2 -- ) \ gforth
\    dup relon ! ;
\ : A,    ( addr -- ) \ gforth
\    here cell allot A! ;
' ! alias A! ( addr1 addr2 -- ) \ gforth-internal

\ dictionary

image-header current-section !
image-header 4 cells + unlock cross-boot$[] >stack lock

: dictionary-end ( -- addr )
    section-start 2@ + ;

: usable-dictionary-end1 ( -- addr )
    dictionary-end
    [ word-pno-size pad-minsize + ] Literal
    current-section @ image-header @ = and - ;

defer usable-dictionary-end ( -- addr )
' usable-dictionary-end1 is usable-dictionary-end

: unused ( -- u ) \ core-ext
    \G Return the amount of free space remaining (in address units) in
    \G the region addressed by @code{here}.
    usable-dictionary-end here - ;

Defer in-dictionary? ( x -- f )
: in-dictionary1? ( x -- f )
    section-start 2@ tuck + within ;
' in-dictionary1? is in-dictionary?

\ here is used for pad calculation!

: dpp   ( -- addr ) \ gforth-internal
    uaddr dp ;

: here  ( -- addr ) \ core
    \G Return the address of the next free location in data space.
    dp @ ;

: ->here ( addr -- ) \ gforth to-here
    \G Change the value of @code{here} to @i{addr}.
    dp ! ;

\ on off                                               23feb93py

\ on is used by docol:
: on  ( a-addr -- ) \ gforth
    \G Set the (value of the) variable  at @i{a-addr} to @code{true}.
    true  swap ! ;
: off ( a-addr -- ) \ gforth
    \G Set the (value of the) variable at @i{a-addr} to @code{false}.
    false swap ! ;

\ dabs roll                                           17may93jaw

: dabs ( d -- ud ) \ double d-abs
    dup 0< IF dnegate THEN ;

: roll  ( x0 x1 .. xn n -- x1 .. xn x0 ) \ core-ext
    \  dup 1+ pick >r
    \  cells sp@ cell+ dup cell+ rot move drop r> ;
    dup 0<= if
	drop
    else
	swap >r 1- recurse r> swap 
    then ;

: stick ( x0 x1 ... xu x u -- x x1 ... xu ) \ gforth-internal
    \ replace x0 with x; e.g., 5 PICK 1+ 5 STICK increments the 6th
    \ stack element (not recommended).
    2 + cells sp@ + ! ;

\ bounds                                         13feb93py

[IFUNDEF] bounds
    : bounds ( addr u -- addr+u addr ) \ gforth
	\G Given a memory block represented by starting address @i{addr}
	\G and length @i{u} in aus, produce the end address @i{addr+u} and
	\G the start address in the right order for @code{u+do} or
	\G @code{?do}.
	over + swap ;
[THEN]

\ (word)                                               22feb93py

[IFUNDEF] scan
    : scan   ( addr1 n1 char -- addr2 n2 ) \ gforth
	\G skip all characters not equal to char
	>r
	BEGIN
	    dup
	WHILE
		over c@ r@ <>
	    WHILE
		    1 /string
	    REPEAT  THEN
	rdrop ;
[THEN]
[IFUNDEF] skip
    : skip   ( addr1 n1 char -- addr2 n2 ) \ gforth
	\G skip all characters equal to char
	>r
	BEGIN
	    dup
	WHILE
		over c@ r@  =
	    WHILE
		    1 /string
	    REPEAT  THEN
	rdrop ;
[THEN]

\ digit?                                               17dec92py

: digit?   ( char -- digit true/ false ) \ gforth-internal digit-question
    \g If @i{char} is a digit in the current @code{base}, return the
    \g value of that digit and true, otherwise return false.
    \ I declared this gforth-internal in case we want to change it to
    \ something like what's discussed in the thread containing
    \ <2022Jun19.085108@mips.complang.tuwien.ac.at>
  toupper '0' - dup 9 u> IF
    [ char A char 9 1 + - ] literal -
    dup 9 u<= IF
      drop false EXIT
    THEN
  THEN
  dup base @ u>= IF
    drop false EXIT
  THEN
  true ;

: accumulate ( +d0 addr digit - +d1 addr )
  swap >r swap  base @  um* drop rot  base @  um* d+ r> ;

: >number ( ud1 c-addr1 u1 -- ud2 c-addr2 u2 ) \ core to-number
    \G Attempt to convert the character string @var{c-addr1 u1} to an
    \G unsigned number in the current number base. The double
    \G @var{ud1} accumulates the result of the conversion to form
    \G @var{ud2}. Conversion continues, left-to-right, until the whole
    \G string is converted or a character that is not convertable in
    \G the current number base is encountered (including + or -). For
    \G each convertable character, @var{ud1} is first multiplied by
    \G the value in @code{BASE} and then incremented by the value
    \G represented by the character. @var{c-addr2} is the location of
    \G the first unconverted character (past the end of the string if
    \G the whole string was converted). @var{u2} is the number of
    \G unconverted characters in the string. Overflow is not detected.
    0
    ?DO
	count digit?
    WHILE
	accumulate
    LOOP
        0
    ELSE
	1- delta-I
	UNLOOP
    THEN ;

: >number_ ( ud1 c-addr1 u1 -- ud2 c-addr2 u2 ) \ gforth-internal to-number-underscore
    \G like @code{>number}, but accepts @code{_} (and ignores it if
    \G @code{base<#41}).
    begin
        >number
        dup 0> while
            over c@ '_' = while
                1 /string
        repeat then ;

\ s>d um/mod						21mar93py

: s>d ( n -- d ) \ core		s-to-d
    dup 0< ;

: ud/mod ( ud1 u2 -- urem udquot ) \ gforth
    \G divide unsigned double @i{ud1} by @i{u2}, resulting in a unsigned double
    \G quotient @i{udquot} and a single remainder @i{urem}.
    over 0= if nip u/mod 0 exit then
    dup >r u/mod r> swap >r um/mod r> ;

\ catch throw                                          23feb93py

has? os 0= [IF]
    : catch  ( ... xt -- ... 0 )
	handler @ >r sp@ >r
	rp@ handler ! execute 0 r> drop r> handler ! ;
    : throw  ( error -- error )
	dup 0= IF  drop EXIT  THEN
	handler @ rp! r> swap >r sp! r> r> handler ! ;
[ELSE]
defer catch ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
\G @code{Executes} @i{xt}.  If execution returns normally,
\G @code{catch} pushes 0 on the stack.  If execution returns through
\G @code{throw}, all the stacks are reset to the depth on entry to
\G @code{catch}, and the TOS (the @i{xt} position) is replaced with
\G the throw code.

:noname ( ... xt -- ... 0 )
    execute 0 ;
is catch

defer throw ( y1 .. ym nerror -- y1 .. ym / z1 .. zn nerror ) \ exception
\G If @i{nerror} is 0, drop it and continue.  Otherwise, transfer
\G control to the next dynamically enclosing exception handler, reset
\G the stacks accordingly, and push @i{nerror}.

:noname ( y1 .. ym error -- y1 .. ym / z1 .. zn error )
    ?dup-if
	[ here throw-entry ! ]
	cr dup >r DoError cr
	[ has? file [IF] ] script? IF  r> (bye)  ELSE  quit  THEN
	[ [ELSE] ] quit [ [THEN] ]
    then ;
is throw
[THEN]

\ (abort")

: c(abort") ( c-addr -- )
    abort-string ! -2 throw ;

defer ?warning ( f xt -- )

:noname ( f xt -- )
    \ just a very basic version
    swap warnings @ and if
	>stderr cr ." warning: " execute exit then
    drop ;
is ?warning

: abort ( ?? -- ?? ) \ core,exception-ext
    \G @code{-1 throw}.
    -1 throw ;

\ ?stack                                               23feb93py

: ?stack ( ?? -- ?? ) \ gforth-internal
    sp@ sp0 @ u> IF    -4 throw  THEN
[ has? floating [IF] ]
    fp@ fp0 @ u> IF  -&45 throw  THEN
[ [THEN] ]
;
\ ?stack should be code -- it touches an empty stack!

\ DEPTH                                                 9may93jaw

: depth ( -- +n ) \ core depth
    \G @var{+n} is the number of values that were on the data stack before
    \G @var{+n} itself was placed on the stack.
    sp@ sp0 @ swap - cell/ ;

: clearstack ( ... -- ) \ gforth clear-stack
\G remove and discard all/any items from the data stack.
    sp0 @ sp! ;

: clearstacks ( ... -- ) \ gforth clear-stacks
\G empty data and FP stack
    clearstack
    clear-litstack
[ has? floating [IF] ]
    fp0 @ fp!
[ [THEN] ]
;

\ HEX DECIMAL                                           2may93jaw

: decimal ( -- ) \ core
    \G Set @code{base} to #10 (decimal).  In many cases
    \G @code{base-execute} is a better alternative.
    #10 base ! ;

: hex ( -- ) \ core-ext
    \G Set @code{base} to $10 (hexadecimal).  In many cases
    \G @code{base-execute} is a better alternative.
    $10 base ! ;
