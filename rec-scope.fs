\ scope recognizer

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

: find-name?in ( addr u wid/0 -- nt/0 )
    ?dup-IF  find-name-in  ELSE  find-name  THEN ;

: nosplit? ( addr1 u1 addr2 u2 --  addr1 u1 addr2 u2 flag )
    \G is true if it didn't split
    dup 0= IF  over >r 2over + r> =  ELSE  false  THEN ;

: scope-split ( addr u wid -- nt/0 )
    BEGIN  >r
	':' $split nosplit? IF  2drop r> find-name?in  EXIT  THEN
	2swap r> find-name?in
	dup WHILE
	    dup >does-code [ ' forth >does-code ]L = WHILE
		>body  REPEAT  THEN
    drop 2drop 0 ;

: rec-scope ( addr u -- xt | rectype-null )
    0 scope-split dup IF  rectype-nt  ELSE  drop  rectype-null  THEN ;

get-recognizers 1+ ' rec-scope -rot set-recognizers

: in ( "voc" "defining-word" -- )
    \G execute @var{defining-word} with @var{voc} as one-shot current
    \G directory. Example: @code{in gui : init-gl ... ;} will define
    \G @code{init-gl} in the @code{gui} vocabulary.
    get-current >r also ' execute definitions previous ' execute
    r> set-current ;

: ?search-prefix ( addr len wid/0 -- addr' len' )
    ?dup-IF
	wordlist-id @ 0 search-voc prefix-string
    ELSE   simple-search-prefix  THEN ;

: scope-search-prefix ( addr1 len1 -- addr2 len2 )
    0  BEGIN >r
	2dup ':' $split nosplit? IF
	    2drop 2drop r> ?search-prefix  EXIT
	THEN
	2swap r> find-name?in
	dup  WHILE
	    dup >does-code [ ' forth >does-code ]L =  WHILE
		>body >r 2nip r>  REPEAT  THEN
    nip nip ?search-prefix ;

' scope-search-prefix is search-prefix
