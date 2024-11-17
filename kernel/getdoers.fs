\ 

\ Authors: Anton Ertl, Bernd Paysan, David Kühling, Neal Crook
\ Copyright (C) 1996, 1998,1999,2003,2005,2006,2007,2010,2013,2015,2016,2019,2020,2023 Free Software Foundation, Inc.

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
    ' bl >code-address AConstant docon: ( -- addr )	\ gforth
    \G The code address of a @code{CONSTANT}.
[THEN]

doer? :dovalue [IF]
    ' def#tib >code-address AConstant dovalue: ( -- addr )	\ gforth
    \G The code address of a @code{CONSTANT}.
[THEN]

' on >code-address AConstant docol: ( -- addr )	\ gforth
\G The code address of a colon definition.
\ !! mark on

doer? :dovar [IF]
    ' udp >code-address AConstant dovar: ( -- addr )	\ gforth
    \G The code address of a @code{CREATE}d word.
    \ in rom-applications variable might be implemented with constant
    \ use really a created word!
[THEN]

doer? :douser [IF]
    ' sp0 >code-address AConstant douser: ( -- addr )	\ gforth
    \G The code address of a @code{USER} variable.
[THEN]

doer? :dodefer [IF]
    ' parse-name >code-address AConstant dodefer: ( -- addr )	\ gforth
    \G The code address of a @code{defer}ed word.
[THEN]

doer? :dofield [IF]
    ' >body >code-address AConstant dofield: ( -- addr )	\ gforth
    \G The code address of a @code{field}.
[THEN]

doer? :dodoes [IF]
    does>-dummy (does-dummy)
    ' (does-dummy) >code-address AConstant dodoes: ( -- addr )	\ gforth
    \G The code address of a @code{DOES>}-defined word.
[THEN]

doer? :doabicode [IF]
    (ABI-CODE) (abi-code-dummy)
    ' (abi-code-dummy) >code-address AConstant doabicode: ( -- addr )	\ gforth
    \G The code address of a @code{ABI-CODE} definition.
[THEN]

doer? :do;abicode [IF]
    (;abi-code) (;abi-code-dummy)
    ' (;abi-code-dummy) >code-address AConstant do;abicode: ( -- addr )
[THEN]

doer? :doextraxt [IF]
    extraxt>-dummy (doextraxt-dummy)
    ' (doextraxt-dummy) >code-address AConstant doextraxt: ( -- addr )
[THEN]
