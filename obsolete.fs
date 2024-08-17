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

-1 warnings !@ \ supress obsolete warnings

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

' latestxt alias lastxt ( -- xt ) \ gforth-obsolete

\G old name for @code{latestxt}.

: [(')]  ( compilation "name" -- ; run-time -- nt ) \ gforth-obsolete bracket-paren-tick
    (') postpone Literal ; immediate restrict

: code-address! ( c_addr xt -- ) \ gforth-obsolete
    \G Change a code field with code address @i{c-addr} at @i{xt}.
    next-section latestnt >r dup xt>name make-latest
    over case
        docon:     of ['] constant, endof
        docol:     of ['] :,        endof
        dovar:     of ['] variable, endof
        douser:    of ['] user,     endof
        dodefer:   of ['] defer,    endof
        dofield:   of ['] field+,   endof
        doabicode: of ['] abi-code, endof
        drop ['] general-compile,
    endcase
    set-optimizer
    r> make-latest previous-section
    only-code-address! ;

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
    2 cells MEM+DO
        cr I 2@ type
    LOOP ;

\ memory words with u or s prefix

' w@  alias uw@  ( c-addr -- u  ) obsolete
' l@  alias ul@  ( c-addr -- u  ) obsolete
[IFDEF] x@
' x@  alias ux@  ( c-addr -- u  ) obsolete
[THEN]
' xd@ alias uxd@ ( c-addr -- ud ) obsolete
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

\ legacy rectype stuff

: rectype>int  ( rectype -- xt ) >body @ ;
: rectype>comp ( rectype -- xt ) cell >body + @ ;
: rectype>post ( rectype -- xt ) 2 cells >body + @ ;

: rectype ( int-xt comp-xt post-xt -- rectype ) \ gforth-obsolete
    \G create a new unnamed recognizer token
    noname translate: latestxt ; 

: rectype: ( int-xt comp-xt post-xt "name" -- ) \ gforth-obsolete
    \G create a new recognizer table
    rectype Constant ;

' notfound AConstant rectype-null \ gforth-obsolete
' translate-nt AConstant rectype-nt \ gforth-obsolete
' translate-num AConstant rectype-num \ gforth-obsolete
' translate-dnum AConstant rectype-dnum \ gforth-obsolete

warnings !
