\ 

\ Copyright (C) 1996, 1998,1999,2003,2005,2006,2007 Free Software Foundation, Inc.

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

doer? :docon [IF]
: docon: ( -- addr )	\ gforth
    \G The code address of a @code{CONSTANT}.
    ['] bl >code-address ;
[THEN]

doer? :dovalue [IF]
: dovalue: ( -- addr )	\ gforth
    \G The code address of a @code{CONSTANT}.
    ['] def#tib >code-address ;
[THEN]

: docol: ( -- addr )	\ gforth
    \G The code address of a colon definition.
    ['] on >code-address ;
\ !! mark on

doer? :dovar [IF]
: dovar: ( -- addr )	\ gforth
    \G The code address of a @code{CREATE}d word.
    \ in rom-applications variable might be implemented with constant
    \ use really a created word!
    ['] ??? >code-address ;
[THEN]

doer? :douser [IF]
: douser: ( -- addr )	\ gforth
    \G The code address of a @code{USER} variable.
    ['] sp0 >code-address ;
[THEN]

doer? :dodefer [IF]
: dodefer: ( -- addr )	\ gforth
    \G The code address of a @code{defer}ed word.
    ['] parser1 >code-address ;
[THEN]

doer? :dofield [IF]

: dofield: ( -- addr )	\ gforth
    \G The code address of a @code{field}.
    ['] >body >code-address ;
[THEN]

true [IF] \ !! don't know what to put here
: dodoes: ( -- addr )	\ gforth
    \G The code address of a @code{field}???
    ['] DOES> >code-address ;
[THEN]
