\ 

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 1996, 1998,1999,2003,2005,2006,2007,2010,2013,2019,2020,2021 Free Software Foundation, Inc.

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
: docon, ( -- )	\ gforth
    \G The code address of a @code{CONSTANT}.
    ['] bl hmcopy, ;
[THEN]

doer? :dovalue [IF]
: dovalue, ( -- )	\ gforth
    \G The code address of a @code{CONSTANT}.
    ['] def#tib hmcopy, ;
[THEN]

: docol, ( -- )	\ gforth
    \G The code address of a colon definition.
    ['] on hmcopy, ;

doer? :dovar [IF]
: dovar, ( -- )	\ gforth
    \G The code address of a @code{CREATE}d word.
    \ in rom-applications variable might be implemented with constant
    \ use really a created word!
    ['] udp hmcopy, ;
[THEN]

doer? :douser [IF]
: douser, ( -- )	\ gforth
    \G The code address of a @code{USER} variable.
    ['] sp0 hmcopy, ;
[THEN]

doer? :dodefer [IF]
: dodefer, ( -- )	\ gforth
    \G The code address of a @code{defer}ed word.
    ['] parser hmcopy, ;
[THEN]

doer? :dofield [IF]
: dofield, ( -- )	\ gforth
    \G The code address of a @code{field}.
    ['] >body hmcopy, ;
[THEN]

doer? :dodoes [IF]
: dodoes: ( -- addr )	\ gforth
    \G The code address of a @code{DOES>}-defined word.
    ['] (does-dummy) >code-address ;
[THEN]

doer? :doabicode [IF]
\ (ABI-CODE) (abi-code-dummy)
: doabicode, ( -- )	\ gforth
    \G The code address of a @code{ABI-CODE} definition.
    ['] (abi-code-dummy) hmcopy, ;
[THEN]

doer? :do;abicode [IF]
\ (;abi-code) (;abi-code-dummy)
: do;abicode, ( -- )
    ['] (;abi-code-dummy) hmcopy, ;
[THEN]

doer? :doextra [IF]
\ extra>-dummy (doextra-dummy)
: doextra, ( -- )
    ['] (doextra-dummy) hmcopy, ;
[THEN]
