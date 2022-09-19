\ some obsolete code that is not needed anywhere else

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2017,2019 Free Software Foundation, Inc.

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

\ WORD SWORD

: (word) ( addr1 n1 char -- addr2 n2 )
  dup >r skip 2dup r> scan  nip - ;

\ (word) should fold white spaces
\ this is what (parse-white) does

: place ( c-addr1 u c-addr2 ) \ gforth-obsolete place
    \G create a counted string of length @var{u} at @var{c-addr2}
    \G and copy the string @var{c-addr1 u} into that location.
    over >r  rot over 1+  r> move c! ;

: +place {: c-addr1 u1 c-addr2 -- :} \ gforth-obsolete plus-place
    \G append the string @var{c-addr1 u} to counted string at @var{c-addr2}
    \G and increase it's length by @var{u}.
    c-addr2 count {: c-addr u2 :}
    u2 u1 + $ff min {: u :}
    c-addr1 c-addr u u2 /string move
    u c-addr2 c! ;

: sword  ( char -- addr len ) \ gforth-obsolete s-word
\G Parses like @code{word}, but the output is like @code{parse} output.
\G @xref{core-idef}.
    \ this word was called PARSE-WORD until 0.3.0, but Open Firmware and
    \ dpANS6 A.6.2.2008 have a word with that name that behaves
    \ differently (like NAME).
    source 2dup >r >r >in @ over min /string
    rot dup bl = IF
        drop (parse-white)
    ELSE
        (word)
    THEN
[ has? new-input [IF] ]
    2dup input-lexeme!
[ [THEN] ]
    2dup + r> - 1+ r> min >in ! ;

: word   ( char "<chars>ccc<char>-- c-addr ) \ core
    \G Skip leading delimiters. Parse @i{ccc}, delimited by
    \G @i{char}, in the parse area. @i{c-addr} is the address of a
    \G transient region containing the parsed string in
    \G counted-string format. If the parse area was empty or
    \G contained no characters other than delimiters, the resulting
    \G string has zero length. A program may replace characters within
    \G the counted string. OBSOLESCENT: the counted string has a
    \G trailing space that is not included in its length.
    sword dup word-pno-size u>= IF  -18 throw  THEN
    here place  bl here count + c!  here ;

\ these transformations are used for legacy words like find

: sfind ( c-addr u -- 0 / xt +-1  ) \ gforth-obsolete
    find-name dup if
	dup name>compile >r swap name>interpret state @ select
	r> ['] execute = flag-sign
    then ;

: find ( c-addr -- xt +-1 | c-addr 0 ) \ core,search
    \G Search all word lists in the current search order for the
    \G definition named by the counted string at @i{c-addr}.  If the
    \G definition is not found, return 0. If the definition is found
    \G return 1 (if the definition has non-default compilation
    \G semantics) or -1 (if the definition has default compilation
    \G semantics).  The @i{xt} returned in interpret state represents
    \G the interpretation semantics.  The @i{xt} returned in compile
    \G state represented either the compilation semantics (for
    \G non-default compilation semantics) or the run-time semantics
    \G that the compilation semantics would @code{compile,} (for
    \G default compilation semantics).  The ANS Forth standard does
    \G not specify clearly what the returned @i{xt} represents (and
    \G also talks about immediacy instead of non-default compilation
    \G semantics), so this word is questionable in portable programs.
    \G If non-portability is ok, @code{find-name} and friends are
    \G better (@pxref{Name token}).
    dup count sfind dup
    if
	rot drop
    then ;

\ various byte-order dependent memory words
\ replacement: compose sequences like "uw@ wbe w>s"

: be-w!  ( x c-addr -- )  >r wbe  r> w! ;
: be-l!  ( x c-addr -- )  >r lbe  r> l! ;
: be-x!  ( x c-addr -- )  >r xbe  r> x! ;
: be-xd! ( xd c-addr -- ) >r xdbe r> xd! ;
: le-w!  ( x c-addr -- )  >r wle  r> w! ;
: le-l!  ( x c-addr -- )  >r lle  r> l! ;
: le-x!  ( x c-addr -- )  >r xle  r> x! ;
: le-xd! ( xd c-addr -- ) >r xdle r> xd! ;
: be-uw@ ( c-addr -- u )   uw@  wbe ;
: be-ul@ ( c-addr -- u )   ul@  lbe ;
: be-ux@ ( c-addr -- u )   ux@  xbe ;
: be-uxd@ ( c-addr -- ud ) uxd@ xdbe ;
: le-uw@ ( c-addr -- u )   uw@  wle ;
: le-ul@ ( c-addr -- u )   ul@  lle ;
: le-ux@ ( c-addr -- u )   ux@  xle ;
: le-uxd@ ( c-addr -- ud ) uxd@ xdle ;
