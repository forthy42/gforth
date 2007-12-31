\ environmental queries

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2007 Free Software Foundation, Inc.

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

\ wordlist constant environment-wordlist

vocabulary environment ( -- ) \ gforth
\ for win32forth compatibility

' environment >body constant environment-wordlist ( -- wid ) \ gforth
  \G @i{wid} identifies the word list that is searched by environmental
  \G queries.


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

: e? name environment? 0= ABORT" environmental dependency not existing" ;

: $has? environment? 0= IF false THEN ;

: has? name $has? ;

environment-wordlist set-current
get-order environment-wordlist swap 1+ set-order

\ assumes that chars, cells and doubles use an integral number of aus

\ this should be computed in C as CHAR_BITS/sizeof(char),
\ but I don't know any machine with gcc where an au does not have 8 bits.
8 constant ADDRESS-UNIT-BITS ( -- n ) \ environment
\G Size of one address unit, in bits.

1 ADDRESS-UNIT-BITS chars lshift 1- constant MAX-CHAR ( -- u ) \ environment
\G Maximum value of any character in the character set

MAX-CHAR constant /COUNTED-STRING ( -- n ) \ environment
\G Maximum size of a counted string, in characters.

ADDRESS-UNIT-BITS cells 2* 2 + constant /HOLD ( -- n ) \ environment
\G Size of the pictured numeric string output buffer, in characters.

&84 constant /PAD ( -- n ) \ environment
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

version-string 2constant gforth ( -- c-addr u ) \ gforth-environment
\G Counted string representing a version string for this version of
\G Gforth (for versions>0.3.0).  The version strings of the various
\G versions are guaranteed to be ordered lexicographically.

: return-stack-cells ( -- n ) \ environment
    \G Maximum size of the return stack, in cells.
    [ forthstart 6 cells + ] literal @ cell / ;

: stack-cells ( -- n ) \ environment
    \G Maximum size of the data stack, in cells.
    [ forthstart 4 cells + ] literal @ cell / ;

: floating-stack ( -- n ) \ environment
    \G @var{n} is non-zero, showing that Gforth maintains a separate
    \G floating-point stack of depth @var{n}.
    [ forthstart 5 cells + ] literal @
    [IFDEF] float  float  [ELSE]  [ 1 floats ] Literal [THEN] / ;

15 constant #locals \ 1000 64 /
    \ One local can take up to 64 bytes, the size of locals-buffer is 1000
maxvp constant wordlists

forth definitions
previous

