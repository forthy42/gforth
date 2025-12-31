\ environmental queries

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2007,2012,2015,2016,2017,2019,2020,2021,2023,2024,2025 Free Software Foundation, Inc.

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

: ?: ( "name" -- ) \ gforth-experimental query-colon
    \G check if @var{"name"} exists.  If it does, scan the input until
    \G @code{;} is found.  Otherwise, define @var{"name"} with @code{:}
    \G and continue compiling the code following
    >in @ >r parse-name find-name 0= IF  r> >in ! :  EXIT  THEN  rdrop
    BEGIN  parse-name dup 0= IF  2drop refill 0=  ELSE   s" ;" str=  THEN
    UNTIL ;

?: cell/ 1 cells / ;
?: float/ 1 floats / ;

\ wordlist constant environment-wordlist

: (0s) ( n -- ) 0 +do '0' c, loop ;
: version-string>internal ( -- )
    version-string
    '.' $split 2swap 3 over - (0s) mem, '.' c,
    '.' $split 2swap 3 over - (0s) mem, '.' c,
    '_' $split 2swap 3 over - (0s) mem, dup
    IF '_' c, mem, ELSE 2drop THEN ;

vocabulary environment ( -- ) \ gforth
\g A vocabulary for @code{environment-wordlist} (present in Win32Forth
\g and VFX).

' environment >wordlist constant environment-wordlist ( -- wid ) \ gforth
  \G @i{wid} identifies the word list that is searched by environmental
  \G queries (present in SwiftForth and VFX).


: environment? ( c-addr u -- false / ... true ) \ core environment-query
    \G @i{c-addr, u} specify a counted string. If the string is not
    \G recognised, return a @code{false} flag. Otherwise return a
    \G @code{true} flag and some (string-specific) information about
    \G the queried string.
    environment-wordlist search-wordlist if
	execute true
    else
	false
    endif ;

: e? parse-name environment? 0= ABORT" environmental dependency not existing" ;

: $has? environment? 0= IF false THEN ;

: has? parse-name $has? ;

environment-wordlist set-current
get-order environment-wordlist swap 1+ set-order

\ assumes that chars, cells and doubles use an integral number of aus

\ this should be computed in C as CHAR_BITS/sizeof(char),
\ but I don't know any machine with gcc where an au does not have 8 bits.
8 constant ADDRESS-UNIT-BITS ( -- n ) \ environment
\G Size of one address unit, in bits.

1 ADDRESS-UNIT-BITS chars lshift 1- constant MAX-CHAR ( -- u ) \ environment
\G Maximum value of any character in the character set (there is also
\G @word{max-xchar}).

MAX-CHAR constant /COUNTED-STRING ( -- n ) \ environment slash-counted-string
\G Maximum size of a counted string, in characters.

ADDRESS-UNIT-BITS cells 2* 2 + constant /HOLD ( -- n ) \ environment slash-hold
\G Size of the pictured numeric string output buffer, in characters.

&84 constant /PAD ( -- n ) \ environment slash-pad
\G Size of the scratch area pointed to by @code{PAD}, in characters.

true constant CORE ( -- f ) \ environment
\G True if the complete core word set is present. Always true for Gforth.

true constant CORE-EXT ( -- f ) \ environment
\G True if the complete core extension word set is present. Always true for Gforth.

1 -3 mod 0< constant FLOORED ( -- f ) \ environment
\G True if @code{/} etc. perform floored division

1 ADDRESS-UNIT-BITS cells 1- lshift 1- constant MAX-N ( -- n ) \ environment
\G Largest usable signed integer.

-1 constant MAX-U ( -- u ) \ environment
\G Largest usable unsigned integer.

-1 MAX-N 2constant MAX-D ( -- d ) \ environment
\G Largest usable signed double.

-1. 2constant MAX-UD ( -- ud ) \ environment
\G Largest usable unsigned double.

here version-string>internal here over -
2constant gforth ( -- c-addr u ) \ gforth-environment
\G Counted string representing a version string for this version of
\G Gforth (for versions>0.3.0).  The version strings of the various
\G versions are guaranteed to be ordered lexicographically.

: return-stack-cells ( -- n ) \ environment
    \G Maximum size of the return stack, in cells.
    [ forthstart section-desc + #2 th ] literal @ cell/ ;

: stack-cells ( -- n ) \ environment
    \G Maximum size of the data stack, in cells.
    [ forthstart section-desc + #0 th ] literal @ cell/ ;

: floating-stack ( -- n ) \ environment
    \G @var{n} is non-zero, showing that Gforth maintains a separate
    \G floating-point stack of depth @var{n}.
    [ forthstart section-desc + #1 th ] literal @
    [IFDEF] float/  float/  [ELSE]  [ 1 floats ] Literal / [THEN] ;

100 constant #locals ( -- n ) \ environment number-locals
\g The maximum number of locals in a definition
\ empirically determined with:
\ : foo 0 do i . s" hdfkjsdfhkshdfkshfksjf" (local) loop ; immediate
\ : bar [ 100000 ] foo
\ which had a dictionary overflow at local 239

$400 constant wordlists ( -- n ) \ environment
\g the maximum number of wordlists usable in the search order
\ The limit is between 4000 and 5000
\ : foo 1000 0 do i . forth-wordlist >order loop ;
\ foo foo foo foo
\ foo


forth definitions
previous

