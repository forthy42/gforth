\ search order wordset                                 14may93py

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

$10 constant maxvp
Variable vp
  0 A, 0 A,  0 A, 0 A,   0 A, 0 A,   0 A, 0 A, 
  0 A, 0 A,  0 A, 0 A,   0 A, 0 A,   0 A, 0 A, 

: get-current  ( -- wid )  current @ ;
: set-current  ( wid -- )  current ! ;

\ : context ( -- addr )  vp dup @ cells + ;
: vp! dup vp ! cells vp + to context ;
: definitions  ( -- )  context @ current ! ;

\ wordlist Vocabulary also previous                    14may93py

Variable slowvoc   0 slowvoc !

\ Forth-wordlist AConstant Forth-wordlist

: mappedwordlist ( map-struct -- wid )	\ gforth
\G creates a wordlist with a special map-structure
  here 0 A, swap A, voclink @ A, 0 A,
  dup wordlist-link voclink !
  dup initvoc ;

: wordlist  ( -- wid )
  slowvoc @
  IF    \ this is now f83search because hashing may be loaded already
	\ jaw
	f83search 
  ELSE  Forth-wordlist wordlist-map @   THEN
  mappedwordlist ;

: Vocabulary ( -- ) Create wordlist drop  DOES> context ! ;

: also  ( -- )
  context @ vp @ 1+ dup maxvp > abort" Vocstack full"
  vp! context ! ;

: previous ( -- )  vp @ 1- dup 0= abort" Vocstack empty" vp! ;

\ vocabulary find                                      14may93py

: (vocfind)  ( addr count wid -- nfa|false )
    \ !! generalize this to be independent of vp
    drop vp dup @ 1- cells over +
    DO  2dup I 2@ over <>
        IF  (search-wordlist) dup
	    IF  nip nip  UNLOOP EXIT
	    THEN  drop
        ELSE  drop 2drop  THEN
    [ -1 cells ] Literal +LOOP
    2drop false ;

0 value locals-wordlist

: (localsvocfind)  ( addr count wid -- nfa|false )
    \ !! use generalized (vocfind)
    drop locals-wordlist
    IF 2dup locals-wordlist (search-wordlist) dup
	IF nip nip
	    EXIT
	THEN drop
    THEN
    0 (vocfind) ;

\ In the kernel the dictionary search works on only one wordlist.
\ The following stuff builds a thing that looks to the kernel like one
\ wordlist, but when searched it searches the whole search order
\  (including locals)

\ this is the wordlist-map of the dictionary
Create vocsearch ( -- wordlist-map )
' (localsvocfind) A, ' (reveal) A,  ' drop A, ' drop A,

\ create dummy wordlist for kernel
slowvoc on
vocsearch mappedwordlist \ the wordlist structure ( -- wid )

\ we don't want the dummy wordlist in our linked list
0 Voclink !
slowvoc off

\ Only root                                            14may93py

Vocabulary Forth
Vocabulary Root

: Only  1 vp! Root also ;

\ set initial search order                             14may93py

Forth-wordlist @ ' Forth >body !

0 vp! also Root also definitions
Only Forth also definitions
lookup ! \ our dictionary search order becomes the law ( -- )

' Forth >body to Forth-wordlist \ "forth definitions get-current" and "forth-wordlist" should produce the same wid


\ get-order set-order                                  14may93py

: get-order  ( -- wid1 .. widn n )
  vp @ 0 ?DO  vp cell+ I cells + @  LOOP  vp @ ;

: set-order  ( wid1 .. widn n / -1 -- )
  dup -1 = IF  drop Only exit  THEN  dup vp!
  ?dup IF  1- FOR  vp cell+ I cells + !  NEXT  THEN ;

: seal ( -- )  context @ 1 set-order ;

: .voc
    body> >head name>string type space ;

: order ( -- )  \  search-ext
    \g prints the search order and the @code{current} wordlist.  The
    \g standard requires that the wordlists are printed in the order
    \g in which they are searched. Therefore, the output is reversed
    \g with respect to the conventional way of displaying stacks. The
    \g @code{current} wordlist is displayed last.
    get-order 0
    ?DO
	.voc
    LOOP
    4 spaces get-current .voc ;

: vocs ( -- ) \ gforth
    \g prints vocabularies and wordlists defined in the system.
    voclink
    BEGIN
	@ dup
    WHILE
	dup 0 wordlist-link - .voc
    REPEAT
    drop ;

Root definitions

' words Alias words
' Forth Alias Forth
' forth-wordlist alias forth-wordlist
' set-order alias set-order
' order alias order

Forth definitions

