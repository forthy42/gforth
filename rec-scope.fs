\ scope recognizer

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2017,2018,2019,2020,2022,2023,2024 Free Software Foundation, Inc.

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

: nosplit? ( addr1 u1 addr2 u2 --  addr1 u1 addr2 u2 flag ) \ gforth-experimental
    \G Used on the result of @code{$split}, flag is true if and only if the
    \G separator does not occur in the input string of @code{$split}.
    dup 0= IF  over >r 2over + r> =  ELSE  false  THEN ;

: scope-split ( addr u wid -- nt rectype-nt | 0 )
    BEGIN  >r
	':' $split nosplit? IF  2drop r> execute  EXIT  THEN
	2swap r> execute  WHILE
	    dup >does-code [ ' forth >does-code ]L = WHILE
		>wordlist  REPEAT  drop  THEN
    2drop 0 ;

: rec-scope ( addr u -- nt rectype-nt | 0 ) \ gforth-experimental
    \G Recognizes strings of the form (simplified)
    \G @code{@i{wordlist}:@i{word}}, where wordlist is found in the
    \G search order.  The result is the same as for @code{rec-nt} for
    \G @i{word} (the ordinary word recognizer) if the search order
    \G consists only of @i{wordlist}.  The general form can have
    \G several wordlists preceding @i{word}, separated by @code{:};
    \G the first (leftmost) wordlist is found in the search order, the
    \G second in the first, etc.  @i{word} is the looked up in the
    \G last (rightmost) wordlist.
    ['] search-order scope-split ;

action-of forth-recognize get-stack 1+ ' rec-scope -rot
action-of forth-recognize set-stack

: current-execute ( xt -- ) \ gforth-experimental
    \G execute current-changing word and revert current afterwards
    get-current >r catch r> set-current throw ;

: in ( "voc" "defining-word" -- ) \ gforth-experimental
    \G execute @var{defining-word} with @var{voc} as one-shot current
    \G directory. Example: @code{in gui : init-gl ... ;} will define
    \G @code{init-gl} in the @code{gui} vocabulary.
    [: ' also execute definitions previous ' execute ;] current-execute ;

: in-wordlist ( wordlist "defining-word" -- ) \ gforth-experimental
    \G execute @var{defining-word} with @var{wordlist} as one-shot current
    \G directory. Example: @code{gui-wordlist in-wordlist : init-gl ... ;}
    \G will define @code{init-gl} in the @code{gui-wordlist} wordlist.
    [: set-current ' execute ;] current-execute ;

: ?search-prefix ( addr len wid/0 -- addr' len' )
    ?dup-IF
	wordlist-id @ 0 search-voc prefix-string
    ELSE   simple-search-prefix  THEN ;

: scope-search-prefix ( addr1 len1 -- addr2 len2 )
    0  BEGIN >r
	2dup ':' $split nosplit? IF
	    2drop 2drop r> ?search-prefix  EXIT
	THEN
	2swap r> ?dup-0=-IF  ['] search-order  THEN  execute  WHILE
	    dup >does-code [ ' forth >does-code ]L =  WHILE
		>wordlist >r 2nip r>  REPEAT  drop  THEN
    2drop simple-search-prefix ;

' scope-search-prefix is search-prefix
