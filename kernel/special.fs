\ words with non-default and non-immediate compilation semantics

\ Copyright (C) 1996,1998 Free Software Foundation, Inc.

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
has? compiler 0= 
[IF] : s" [ELSE] :noname [THEN]
	[char] " parse
    	/line min >r s"-buffer r@ cmove
    	s"-buffer r> ;
has? compiler [IF]
:noname [char] " parse postpone SLiteral ;
interpret/compile: S" ( compilation 'ccc"' -- ; run-time -- c-addr u )	\ core,file	s-quote
  \G Compilation: Parse a string @var{ccc} delimited by a @code{"}
  \G (double quote). At run-time, return the length, @var{u}, and the
  \G start address, @var{c-addr} of the string. Interpretation: parse
  \G the string as before, and return @var{c-addr}, @var{u}. The
  \G string is stored in a temporary buffer which may be overwritten
  \G by subsequent uses of @code{S"}.
[THEN]

has? compiler [IF]
: [IS] ( compilation "name" -- ; run-time xt -- ) \ possibly-gforth bracket-is
    ' >body postpone ALiteral postpone ! ; immediate restrict

:noname    ' >body ! ;
' [IS]
interpret/compile: IS ( addr "name" -- ) \ gforth

:noname    ' >body @ ;
:noname    ' >body postpone ALiteral postpone @ ;
interpret/compile: What's ( "name" -- addr ) \ gforth

:noname    [char] " parse type ;
:noname    postpone (.") ,"  align ;
interpret/compile: ." ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote
  \G Compilation: Parse a string @var{ccc} delimited by a " (double
  \G quote). At run-time, display the string. Interpretation semantics
  \G for this word are undefined in ANS Forth. Gforth's interpretation
  \G semantics are to display the string. This is the simplest way to
  \G display a string from within a definition; see examples below.

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

[THEN]

has? compiler [IF]
: interpret/compile? ( xt -- flag )
    >does-code ['] S" >does-code = ;
[ELSE]
: interpret/compile?
    false ;
[THEN]

