\ environmental queries

\ Copyright (C) 1995 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ wordlist constant environment-wordlist

Create environment-wordlist  wordlist drop

: environment? ( c-addr u -- false / ... true ) \ core environment-query
    environment-wordlist search-wordlist if
	execute true
    else
	false
    endif ;

environment-wordlist set-current
get-order environment-wordlist swap 1+ set-order

\ assumes that chars, cells and doubles use an integral number of aus

\ this should be computed in C as CHAR_BITS/sizeof(char),
\ but I don't know any machine with gcc where an au does not have 8 bits.
8 constant ADDRESS-UNIT-BITS ( -- n ) \ environment
1 ADDRESS-UNIT-BITS chars lshift 1- constant MAX-CHAR
MAX-CHAR constant /COUNTED-STRING
ADDRESS-UNIT-BITS cells 2* 2 + constant /HOLD
&84 constant /PAD
true constant CORE
true constant CORE-EXT
1 -3 mod 0< constant FLOORED

1 ADDRESS-UNIT-BITS cells 1- lshift 1- constant MAX-N
-1 constant MAX-U

-1 MAX-N 2constant MAX-D
-1. 2constant MAX-UD

version-string 2constant gforth \ version string (for versions>0.3.0)
\ the version strings of the various versions are guaranteed to be
\ sorted lexicographically

: return-stack-cells ( -- n )
    [ forthstart 6 cells + ] literal @ cell / ;

: stack-cells ( -- n )
    [ forthstart 4 cells + ] literal @ cell / ;

: floating-stack ( -- n )
    [ forthstart 5 cells + ] literal @
    [IFDEF] float  float  [ELSE]  [ 1 floats ] Literal [THEN] / ;

\ !! max-float
15 constant #locals \ 1000 64 /
    \ One local can take up to 64 bytes, the size of locals-buffer is 1000
maxvp constant wordlists

forth definitions
previous

