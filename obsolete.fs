\ some obsolete code that is not needed anywhere else

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2017,2019,2022,2023 Free Software Foundation, Inc.

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

\ list obsolete words

[IFDEF] obsolete-mask
    : obsoletes ( -- ) \ gforth
	\G show all obsolete words
	cr 0 [: dup >f+c @ obsolete-mask and
	    IF  .word  ELSE  drop  THEN  true ;]
	context @ traverse-wordlist  drop ;
[ELSE]
    : obsolete ;
[THEN]
\ from kernel

: header, ( c-addr u -- ) \ gforth-obsolete
    \G create a header for a named word
    hm, name, hmtemplate namehm, named-hm ;

: longstring, ( c-addr u -- ) \ gforth-obsolete
    \G puts down string as longcstring
    dup , mem, ;

' latestxt alias lastxt \ gforth-obsolete
\G old name for @code{latestxt}.

: [(')]  ( compilation "name" -- ; run-time -- nt ) \ gforth-obsolete bracket-paren-tick
    (') postpone Literal ; immediate restrict

: definer! ( definer xt -- ) \ gforth-obsolete
    \G The word represented by @var{xt} changes its behaviour to the
    \G behaviour associated with @var{definer}.
    over 3 and case
        0 of code-address! endof
        1 of swap 3 invert and swap does-code! endof
        2 of swap 3 invert and swap
            do;abicode: any-code! ['] ;abi-code, set-optimizer endof
        -12 throw
    endcase ;

' opt: alias comp: ( compilation -- colon-sys2 ; run-time -- nest-sys ) \ gforth-obsolete
\G Use @code{opt:} instead.

' parse-name alias parse-word ( -- c-addr u ) \ gforth-obsolete
\G old name for @code{parse-name}; this word has a conflicting
\G behaviour in some other systems.

' parse-name alias name ( -- c-addr u ) \ gforth-obsolete
\G old name for @code{parse-name}
    
: .strings ( addr u -- ) \ gforth-obsolete
    \G list the strings from an array of string descriptors at addr
    \G with u entries, one per line.
    2* cells bounds ?DO
	cr I 2@ type 2 cells +LOOP ;

\ WORD SWORD

: (word) ( addr1 n1 char -- addr2 n2 )
  dup >r skip 2dup r> scan  nip - ; obsolete

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
    here place  bl here count + c!  here ; obsolete

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
    then ; obsolete

\ memory words with u or s prefix

' w@  alias uw@  ( c-addr -- u  ) obsolete
' l@  alias ul@  ( c-addr -- u  ) obsolete
[IFDEF] x@
' x@  alias ux@  ( c-addr -- u  ) obsolete
[THEN]
' xd@ alias uxd@ ( c-addr -- ud ) obsolete
inline: sw@  ( c-addr -- n ) ]]  w@  w>s [[ ;inline obsolete
inline: sl@  ( c-addr -- n ) ]]  l@  l>s [[ ;inline obsolete
[IFDEF] x@
inline: sx@  ( c-addr -- n ) ]]  x@  x>s [[ ;inline obsolete
[THEN]
inline: sxd@ ( c-addr -- d ) ]] xd@ xd>s [[ ;inline obsolete

\ various byte-order dependent memory words
\ replacement: compose sequences like "uw@ wbe w>s"

inline: be-w!  (  x c-addr -- )  ]] >r wbe  r>  w! [[ ;inline obsolete
inline: be-l!  (  x c-addr -- )  ]] >r lbe  r>  l! [[ ;inline obsolete
[IFDEF] x!
inline: be-x!  (  x c-addr -- )  ]] >r xbe  r>  x! [[ ;inline obsolete
[THEN]
inline: be-xd! ( xd c-addr -- )  ]] >r xdbe r> xd! [[ ;inline obsolete
inline: le-w!  (  x c-addr -- )  ]] >r wle  r>  w! [[ ;inline obsolete
inline: le-l!  (  x c-addr -- )  ]] >r lle  r>  l! [[ ;inline obsolete
[IFDEF] x!
inline: le-x!  (  x c-addr -- )  ]] >r xle  r>  x! [[ ;inline obsolete
[THEN]
inline: le-xd! ( xd c-addr -- )  ]] >r  xdle  r>   xd! [[ ;inline obsolete
inline:  be-uw@ ( c-addr -- u )  ]]  w@  wbe [[ ;inline obsolete
inline:  be-ul@ ( c-addr -- u )  ]]  l@  lbe [[ ;inline obsolete
[IFDEF] x@
inline:  be-ux@ ( c-addr -- u )  ]]  x@  xbe [[ ;inline obsolete
[THEN]
inline: be-uxd@ ( c-addr -- ud ) ]] xd@ xdbe [[ ;inline obsolete
inline:  le-uw@ ( c-addr -- u )  ]]  w@  wle [[ ;inline obsolete
inline:  le-ul@ ( c-addr -- u )  ]]  l@  lle [[ ;inline obsolete
[IFDEF] x@
inline:  le-ux@ ( c-addr -- u )  ]]  x@  xle [[ ;inline obsolete
[THEN]
inline: le-uxd@ ( c-addr -- ud ) ]] xd@ xdle [[ ;inline obsolete

