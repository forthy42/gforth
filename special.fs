\ words with non-default and non-immediate compilation semantics

\ Copyright (C) 1996 Free Software Foundation, Inc.

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

\ this file comes last, because these words override cross' words.

create s"-buffer /line chars allot
:noname    [char] " parse
    /line min >r s"-buffer r@ cmove
    s"-buffer r> ;
:noname    [char] " parse postpone SLiteral ;
interpret/compile: S" ( compilation 'ccc"' -- ; run-time -- c-addr u )	\ core,file	s-quote

:noname    ' >body ! ;
:noname    ' >body postpone ALiteral postpone ! ;
interpret/compile: IS ( addr "name" -- ) \ gforth

:noname    ' >body @ ;
:noname    ' >body postpone ALiteral postpone @ ;
interpret/compile: What's ( "name" -- addr ) \ gforth

:noname    [char] " parse type ;
:noname    postpone (.") ,"  align ;
interpret/compile: ." ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote

\ DOES>                                                17mar93py

:noname
    dodoes, here !does ]
    defstart :-hook ;
:noname
    ;-hook postpone (does>) ?struc dodoes,
    defstart :-hook ;
interpret/compile: DOES>  ( compilation colon-sys1 -- colon-sys2 ; run-time nest-sys -- ) \ core	does
    
' IS Alias TO ( addr "name" -- ) \ core-ext
immediate


