\ search order wordset                                 14may93py

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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

: get-current  ( -- wid ) \ search
  \G @var{wid} is the identifier of the current compilation word list.
  current @ ;

: set-current  ( wid -- )  \ search
  \G Set the compilation word list to the word list identified by @var{wid}.
  current ! ;

\ : context ( -- addr )  vp dup @ cells + ;
: vp! dup vp ! cells vp + to context ;
: definitions  ( -- ) \ search
  \G Make the compilation word list the same as the word list
  \G that is currently at the top of the search order stack.
  context @ current ! ;

\ wordlist Vocabulary also previous                    14may93py

Variable slowvoc   0 slowvoc !

\ Forth-wordlist AConstant Forth-wordlist

: mappedwordlist ( map-struct -- wid )	\ gforth
\G Create a wordlist with a special map-structure.
  here swap A, 0 A, voclink @ A, 0 A,
  dup wordlist-link voclink !
  dup initvoc ;

: wordlist  ( -- wid ) \ search
  \G Create a new, empty word list represented by @var{wid}.
  slowvoc @
  IF    \ this is now f83search because hashing may be loaded already
	\ jaw
	f83search 
  ELSE  Forth-wordlist wordlist-map @   THEN
  mappedwordlist ;

: Vocabulary ( "name" -- ) \ gforth
  \G Create a definition "name" and associate a new word list with it.
  \G The run-time effect of "name" is to push the new word list's wid
  \G onto the top of the search order stack.
  Create wordlist drop  DOES> context ! ;

: check-maxvp ( n -- )
    maxvp > -49 and throw ;

: push-order ( wid -- ) \ gforth
    \g Push @var{wid} on the search order.
    vp @ 1+ dup check-maxvp vp! context ! ;

: also  ( -- ) \ search ext
  \G Perform a @code{DUP} on the search order stack. Usually used prior
  \G to @code{Forth}, @code{definitions} etc.
  context @ push-order ;

: previous ( -- ) \ search ext
  \G Perform a @code{DROP} on the search order stack, thereby removing the wid at the
  \G top of the (search order) stack from the search order.
  vp @ 1- dup 0= -50 and throw vp! ;

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

Vocabulary Forth ( -- ) \ thisone- search-ext
  \G Push the @var{wid} associated with @code{forth-wordlist} onto the
  \G search order stack.

Vocabulary Root ( -- ) \ gforth
  \G Add the vocabulary @code{Root} to the search order stack.
  \G This vocabulary makes up the minimum search order and
  \G contains these words: @code{order} @code{set-order}
  \G @code{forth-wordlist} @code{Forth} @code{words}

: Only ( -- ) \ search-ext
  \G Set the search order to the implementation-defined minimum search
  \G order (for Gforth, this is the word list @code{Root}).
  1 vp! Root also ;

\ set initial search order                             14may93py

Forth-wordlist wordlist-id @ ' Forth >body wordlist-id !

0 vp! also Root also definitions
Only Forth also definitions
lookup ! \ our dictionary search order becomes the law ( -- )

' Forth >body to Forth-wordlist \ "forth definitions get-current" and "forth-wordlist" should produce the same wid


\ get-order set-order                                  14may93py

: get-order  ( -- widn .. wid1 n ) \ search
  \G Copy the search order stack to the data stack. The current search
  \G order has @var{n} entries, of which @var{wid1} represents the word
  \G list that is searched first (the word list at the top of the stack) and
  \G @var{widn} represents the word order that is searched last.
  vp @ 0 ?DO  vp cell+ I cells + @  LOOP  vp @ ;

: set-order  ( widn .. wid1 n -- ) \ thisone- search
    \G If @var{n}=0, empty the search order.  If @var{n}=-1, set the
    \G search order to the implementation-defined minimum search order
    \G (for Gforth, this is the word list @code{Root}). Otherwise,
    \G replace the existing search order with the @var{n} wid entries
    \G such that @var{wid1} represents the word list that will be
    \G searched first and @var{widn} represents the word list that will
    \G be searched last.
    dup -1 = IF
	drop only exit
    THEN
    dup check-maxvp
    dup vp!
    ?dup IF 1- FOR vp cell+ I cells + !  NEXT THEN ;

: seal ( -- ) \ gforth
  \G Remove all word lists from the search order stack other than the word
  \G list that is currently on the top of the search order stack.
  context @ 1 set-order ;

: .voc
    body> >head name>string type space ;

: order ( -- )  \  thisone- search-ext
  \G Print the search order and the compilation word list.  The
  \G word lists are printed in the order in which they are searched
  \G (which is reversed with respect to the conventional way of
  \G displaying stacks). The compilation word list is displayed last.
  \ The standard requires that the word lists are printed in the order
  \ in which they are searched. Therefore, the output is reversed
  \ with respect to the conventional way of displaying stacks.
    get-order 0
    ?DO
	.voc
    LOOP
    4 spaces get-current .voc ;

: vocs ( -- ) \ gforth
    \G List vocabularies and wordlists defined in the system.
    voclink
    BEGIN
	@ dup
    WHILE
	dup 0 wordlist-link - .voc
    REPEAT
    drop ;

Root definitions

' words Alias words  ( -- ) \ tools
\G Display a list of all of the definitions in the word list at the top
\G of the search order.
' Forth Alias Forth
' forth-wordlist alias forth-wordlist ( -- wid ) \ search
  \G CONSTANT: @var{wid} identifies the word list that includes all of the standard words
  \G provided by Gforth. When Gforth is invoked, this word list is the compilation word
  \G list and is at the top of the word list stack.
' set-order alias set-order
' order alias order

Forth definitions

