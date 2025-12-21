\ search order wordset                                 14may93py

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook, David Kühling, Jens Wilke
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2005,2007,2011,2015,2016,2017,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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
require rec-sequence.fs
[IFUNDEF] name>interpret
    ' name>int alias name>interpret
    ' name>comp alias name>compile
[THEN]

0 rec-sequence: search-order

: >back ( x stack -- ) \ gforth-experimental to-back
    \G Insert @i{x} at the bottom of @i{stack}.
    dup >r $@len cell+ r@ $!len
    r@ $@ cell- over cell+ swap move
    r> $@ drop ! ;
: back> ( stack -- x ) \ gforth-experimental back-from
    \G Remove item @i{x} from bottom of @i{stack}.
    dup >r $@ IF  @ r@ 0 cell $del  ELSE  drop 0  THEN
    rdrop ;

: set-current  ( wid -- )  \ search
  \G Set the compilation word list to the word list identified by @i{wid}.
  current ! ;

:noname ( -- addr )
    ['] search-order >body $@ + cell- ;
is context

: definitions  ( -- ) \ search
  \G Set the compilation word list to be the same as the word list
  \G that is currently at the top of the search order.
  context @ current ! ;

\ wordlist Vocabulary also previous                    14may93py

Variable slowvoc   0 slowvoc !

\ Forth-wordlist AConstant Forth-wordlist

: wl, ( -- )
    0 A, here voclink @ A, voclink ! 0 A, ;

: set-wordlist ( reveal-xt rec-xt -- )
    noname-hm  set-does>  set-to ;

: wordlist-class ( reveal-xt rec-xt -- wid )
    hmsave 2>r latestnt >r
    forth-wordlist noname-from here body> >r wl,
    set-wordlist  r@ initwl r>  hm,
    r> make-latest 2r> hmrestore ;

: mappedwordlist ( map-struct -- wid )	\ gforth-internal
\G Create a wordlist with a special map-structure.
    cfalign
    [ 0 >body ] [IF] A, here dodoes: A, [ELSE] dodoes: A, A, here [THEN]
    wl, dup initwl ;

: wordlist  ( -- wid ) \ search
  \G Create a new, empty word list represented by @i{wid}.
  slowvoc @
  IF    \ this is now f83search because hashing may be loaded already
	\ jaw
	f83search
  ELSE  Forth-wordlist wordlist-map @   THEN
  mappedwordlist ;

: Vocabulary ( "name" -- ) \ gforth
    \G Create a definition "name" and associate a new word list @i{wid1} with it.@*
    \G @i{name} execution: ( -- ) replace the @i{wid2} at the
    \G top of the search order with @i{wid1}.
  Create  [: 0 wordlist-start - context ! ;] set-does> wordlist drop ;

(field) >wordlist ( voc-xt -- wordlist ) 0 wordlist-start negate >body ,
(field) >voc ( wordlist -- voc-xt ) 0 >body negate wordlist-start ,

: >order ( wid -- ) \ gforth to-order
    \g Push @var{wid} on the search order.
    ['] search-order >body >stack ;

: also  ( -- ) \ search-ext
  \G Like @code{DUP} for the search order. Usually used before a
  \G vocabulary (e.g., @code{also Forth}); the combined effect is to push
  \G the wordlist represented by the vocabulary on the search order.
  context @ >order ;

: previous ( -- ) \ search-ext
  \G Drop the wordlist at the top of the search order.
  ['] search-order >body stack> drop ;

\ In the kernel the dictionary search works on only one wordlist.
\ The following stuff builds a thing that looks to the kernel like one
\ wordlist, but when searched it searches the whole search order
\  (including locals)

\ Only root                                            14may93py

voclink off \ remove forth-wordlist from voclink

Vocabulary Forth ( -- ) \ search-ext
  \G Replace the @i{wid} at the top of the search order with the
  \G @i{wid} associated with the word list @code{forth-wordlist}.


Vocabulary Root ( -- ) \ gforth
  \G Replace the top of the search order by the root wordlist.  This
  \G vocabulary makes up the minimum search order and contains only a
  \G few search-order words.

: Only ( -- ) \ search-ext
  \G Set the search order to the implementation-defined minimum search
  \G order (for Gforth, this is the word list @code{Root}).
  ['] Root >wordlist dup 2 ['] search-order >body set-stack ;

Only Forth also definitions

\ set initial search order                             14may93py

Forth-wordlist wordlist-id @ ' Forth >wordlist wordlist-id !

' search-order is rec-name \ our dictionary search order becomes the law ( -- )

' Forth >wordlist to Forth-wordlist \ "forth definitions get-current" and "forth-wordlist" should produce the same wid


\ get-order set-order                                  14may93py

: get-order  ( -- widn .. wid1 n ) \ search
  \G Copy the search order to the data stack. The current search order
  \G has @i{n} entries, of which @i{wid1} represents the wordlist
  \G that is searched first (the word list at the top of the search
  \G order) and @i{widn} represents the wordlist that is searched
  \G last.
    ['] search-order >body get-stack ;

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
    ['] search-order >body set-stack ;

: seal ( -- ) \ gforth
  \G Remove all word lists from the search order stack other than the word
  \G list that is currently on the top of the search order stack.
  context @ 1 set-order ;

: id. ( nt -- ) \ gforth  i-d-dot
    \G Print the name of the word represented by @var{nt}.
    \ this name comes from fig-Forth
    name>string type space ;

' id. alias .id ( nt -- ) \ gforth dot-i-d
\G F83 name for @code{id.}.

: .voc ( wid -- ) \ gforth  dot-voc
    \G print the name of the wordlist represented by @var{wid}.  Can
    \G only print names defined with @code{vocabulary} or
    \G @code{wordlist constant}, otherwise prints @samp{address}.
    dup >voc xt?  IF  >voc id.  EXIT  THEN
    #10 cells 2 cells DO
	dup wordlist-struct %size + I + xt?
	true = if ( wid nt )
	    dup wordlist-struct %size + I + swap >r
	    dup name>interpret dup >code-address docon: = swap >body @ r> = and if
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
    \G Perform xt ( ... wid -- ... ) for all wordlists (including
    \G tables and cs-wordlists) in the system.
    >r voclink
    BEGIN
	@ dup
    WHILE
	dup >r 0 wordlist-link - r'@ execute r>
    REPEAT
    drop rdrop ;

: vocs ( -- ) \ gforth
    \G List vocabularies and wordlists defined in the system.
    ['] .voc map-vocs ;

Root definitions

' words Alias words  ( -- )
' Forth Alias Forth ( -- )
' forth-wordlist alias forth-wordlist ( -- wid )
' set-order alias set-order ( wid1 ... widu u -- )
' order alias order ( -- )

Forth definitions

