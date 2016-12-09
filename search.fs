\ search order wordset                                 14may93py

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2005,2007,2011,2015 Free Software Foundation, Inc.

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

require struct.fs

$10 stack: vocstack

: >back ( x stack -- )
    \G push to bottom of stack
    >r r@ $@len cell+ r@ $!len
    r@ $@ cell- over cell+ swap move
    r> $@ drop ! ;
: back> ( stack -- x )
    \G pop from bottom of stack
    >r r@ $@ IF  @ r@ 0 cell $del  ELSE  drop 0  THEN
    rdrop ;

: set-current  ( wid -- )  \ search
  \G Set the compilation word list to the word list identified by @i{wid}.
  current ! ;

:noname ( -- addr )
    vocstack $@ + cell- ;
is context

: definitions  ( -- ) \ search
  \G Set the compilation word list to be the same as the word list
  \G that is currently at the top of the search order.
  context @ current ! ;

\ wordlist Vocabulary also previous                    14may93py

Variable slowvoc   0 slowvoc !

\ Forth-wordlist AConstant Forth-wordlist

: mappedwordlist ( map-struct -- wid )	\ gforth
\G Create a wordlist with a special map-structure.
  align here swap A, 0 A, voclink @ A, 0 A,
  dup wordlist-link voclink !
  dup initvoc ;

: wordlist  ( -- wid ) \ search
  \G Create a new, empty word list represented by @i{wid}.
  slowvoc @
  IF    \ this is now f83search because hashing may be loaded already
	\ jaw
	f83search 
  ELSE  Forth-wordlist wordlist-map @   THEN
  mappedwordlist ;

: Vocabulary ( "name" -- ) \ gforth
  \G Create a definition "name" and associate a new word list with it.
  \G The run-time effect of "name" is to replace the @i{wid} at the
  \G top of the search order with the @i{wid} associated with the new
  \G word list.
  Create wordlist drop  DOES> context ! ;

: >order ( wid -- ) \ gforth to-order
    \g Push @var{wid} on the search order.
    vocstack >stack ;

: also  ( -- ) \ search-ext
  \G Like @code{DUP} for the search order. Usually used before a
  \G vocabulary (e.g., @code{also Forth}); the combined effect is to push
  \G the wordlist represented by the vocabulary on the search order.
  context @ >order ;

: previous ( -- ) \ search-ext
  \G Drop the wordlist at the top of the search order.
  vocstack stack> drop ;

\ vocabulary find                                      14may93py

: (vocfind)  ( addr count wid -- nfa|false )
    \ !! generalize this to be independent of vp
    drop vocstack $@ bounds cell- swap cell- -DO
	( addr count ) \ note that the loop does not reach 0
	2dup I @ find-name-in ?dup-IF ( addr count nt )
	    nip nip unloop exit THEN
	cell [ 2 cells ] Literal I cell- 2@ <> select \ skip double entries
	\ note that we search first and then skip, because the first search
	\ has a very likely hit.  So doubles will be skipped, tripples not
    -loop
    2drop false ;

[ifundef] locals-wordlist
    0 value locals-wordlist
[then]

: (localsvocfind)  ( addr count wid -- nfa|false )
    \ !! use generalized (vocfind)
    drop locals-wordlist
    IF 2dup locals-wordlist find-name-in dup
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

Vocabulary Forth ( -- ) \ search-ext
  \G Replace the @i{wid} at the top of the search order with the
  \G @i{wid} associated with the word list @code{forth-wordlist}.


Vocabulary Root ( -- ) \ gforth
  \G Add the root wordlist to the search order stack.  This vocabulary
  \G makes up the minimum search order and contains only a
  \G search-order words.

: Only ( -- ) \ search-ext
  \G Set the search order to the implementation-defined minimum search
  \G order (for Gforth, this is the word list @code{Root}).
  0 1 vocstack set-stack Root also ;

[ifundef] 'image
defer 'image ( -- ) ' noop is 'image
[then]

:noname ( -- )
    \ save search order here
    defers 'image vocstack $save ; is 'image

: init-vp  ( -- )
    vocstack $boot ;

:noname
   init-vp DEFERS 'cold ;
IS 'cold

Only Forth also definitions

\ set initial search order                             14may93py

Forth-wordlist wordlist-id @ ' Forth >body wordlist-id !

lookup ! \ our dictionary search order becomes the law ( -- )

' Forth >body to Forth-wordlist \ "forth definitions get-current" and "forth-wordlist" should produce the same wid


\ get-order set-order                                  14may93py

: get-order  ( -- widn .. wid1 n ) \ search
  \G Copy the search order to the data stack. The current search order
  \G has @i{n} entries, of which @i{wid1} represents the wordlist
  \G that is searched first (the word list at the top of the search
  \G order) and @i{widn} represents the wordlist that is searched
  \G last.
    vocstack get-stack ;

: set-order  ( widn .. wid1 n -- ) \ search
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
    vocstack set-stack ;

: seal ( -- ) \ gforth
  \G Remove all word lists from the search order stack other than the word
  \G list that is currently on the top of the search order stack.
  context @ 1 set-order ;

[IFUNDEF] .name
: id. ( nt -- ) \ gforth  i-d-dot
    \G Print the name of the word represented by @var{nt}.
    \ this name comes from fig-Forth
    name>string type space ;

' id. alias .id ( nt -- ) \ F83  dot-i-d
\G F83 name for @code{id.}.

' id. alias .name ( nt -- ) \ gforth-obsolete  dot-name
\G Gforth <=0.5.0 name for @code{id.}.

[THEN]

: .voc ( wid -- ) \ gforth  dot-voc
\G print the name of the wordlist represented by @var{wid}.  Can
\G only print names defined with @code{vocabulary} or
    \G @code{wordlist constant}, otherwise prints @samp{address}.
    dup body> head?  IF  body> id.  EXIT  THEN
    #10 cells 2 cells DO
	dup wordlist-struct %size + I + head?
	true = if ( wid nt )
	    dup wordlist-struct %size + I + swap >r
	    dup name>int dup >code-address docon: = swap >body @ r> = and if
		id. unloop exit
	    endif
	endif
    cell +LOOP
    '<' emit 0 .r ." > " ;

: order ( -- )  \  search-ext
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

: map-vocs ( ... xt -- ... ) \ gforth
    \G xt: ( ... wid -- ... ) free to use the stack underneath
    \G run as long as f is true
    >r voclink
    BEGIN
	@ dup
    WHILE
	dup >r 0 wordlist-link - i' execute r>
    REPEAT
    drop rdrop ;

: vocs ( -- ) \ gforth
    \G List vocabularies and wordlists defined in the system.
    ['] .voc map-vocs ;

Root definitions

' words Alias words  ( -- ) \ tools
\G Display a list of all of the definitions in the word list at the top
\G of the search order.
' Forth Alias Forth \ alias- search-ext
' forth-wordlist alias forth-wordlist ( -- wid ) \ search
  \G @code{Constant} -- @i{wid} identifies the word list that includes all of the standard words
  \G provided by Gforth. When Gforth is invoked, this word list is the compilation word
  \G list and is at the top of the search order.
' set-order alias set-order ( wid1 ... widu u -- ) \ alias- search
' order alias order ( -- ) \ alias- search-ext

Forth definitions

