\ EXTEND.FS    CORE-EXT Word not fully tested!         12may93jaw

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


\ May be cross-compiled

decimal

\ .(                                                    12may93jaw

: .(   ( compilation "...<paren>" -- ) \ core-ext dot-paren
    [char] ) parse type ; immediate

\ VALUE 2>R 2R> 2R@                                     17may93jaw

\ !! 2value

: 2Literal ( compilation w1 w2 -- ; run-time  -- w1 w2 ) \ double two-literal
    swap postpone Literal  postpone Literal ; immediate restrict

' drop alias d>s ( d -- n ) \ double		d_to_s

: m*/ ( d1 n2 u3 -- dqout ) \ double m-star-slash
    >r s>d >r abs -rot
    s>d r> xor r> swap >r >r dabs rot tuck um* 2swap um*
    swap >r 0 d+ r> -rot r@ um/mod -rot r> um/mod nip swap
    r> IF dnegate THEN ;

\ CASE OF ENDOF ENDCASE                                 17may93jaw

\ just as described in dpANS5

0 CONSTANT case ( compilation  -- case-sys ; run-time  -- ) \ core-ext
    immediate

: of ( compilation  -- of-sys ; run-time x1 x2 -- |x1 ) \ core-ext
    \ !! the implementation does not match the stack effect
    1+ >r
    postpone over postpone = postpone if postpone drop
    r> ; immediate

: endof ( compilation case-sys1 of-sys -- case-sys2 ; run-time  -- ) \ core-ext end-of
    >r postpone else r> ; immediate

: endcase ( compilation case-sys -- ; run-time x -- ) \ core-ext end-case
    postpone drop
    0 ?do postpone then loop ; immediate

\ C"                                                    17may93jaw

: (c")     "lit ;

: CLiteral
    postpone (c") here over char+ allot  place align ; immediate restrict

: C" ( compilation "...<quote>" -- ; run-time  -- c-addr ) \ core-ext c-quote
    [char] " parse postpone CLiteral ; immediate restrict

\ [COMPILE]                                             17may93jaw

: [compile] ( compilation "name" -- ; run-time ? -- ? ) \ core-ext bracket-compile
    comp' drop compile, ; immediate

\ CONVERT                                               17may93jaw

: convert ( ud1 c-addr1 -- ud2 c-addr2 ) \ core-ext
    \G obsolescent; superseded by @code{>number}.
    char+ true >number drop ;

\ ERASE                                                 17may93jaw

: erase ( addr len -- ) \ core-ext
    \ !! dependence on "1 chars 1 ="
    ( 0 1 chars um/mod nip )  0 fill ;
: blank ( addr len -- ) \ string
    bl fill ;

\ SEARCH                                                02sep94py

: search ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 flag ) \ string
    \ not very efficient; but if we want efficiency, we'll do it as primitive
    2>r 2dup
    begin
	dup r@ >=
    while
	over 2r@ swap -text 0= if
	    2swap 2drop 2r> 2drop true exit
	endif
	1 /string
    repeat
    2drop 2r> 2drop false ;

\ SOURCE-ID SAVE-INPUT RESTORE-INPUT                    11jun93jaw

: source-id ( -- 0 | -1 | fileid ) \ core-ext,file source-i-d
  loadfile @ dup 0= IF  drop sourceline# 0 min  THEN ;

: save-input ( -- x1 .. xn n ) \ core-ext
    >in @
    loadfile @
    if
	loadfile @ file-position throw
    else
	blk @
	linestart @
    then
    sourceline#
    >tib @
    source-id
    6 ;

: restore-input ( x1 .. xn n -- flag ) \ core-ext
    6 <> -12 and throw
    source-id <> -12 and throw
    >tib !
    >r ( line# )
    loadfile @ 0<>
    if
	loadfile @ reposition-file throw
    else
	linestart !
	blk !
	sourceline# r@ <> blk @ 0= and loadfile @ 0= and
	if
	    drop rdrop true EXIT
	then
    then
    r> loadline !
    >in !
    false ;

\ This things we don't need, but for being complete... jaw

\ EXPECT SPAN                                           17may93jaw

variable span ( -- a-addr ) \ core-ext
\ obsolescent

: expect ( c-addr +len -- ) \ core-ext
    \ obsolescent; use accept
    0 rot over
    BEGIN ( maxlen span c-addr pos1 )
	key decode ( maxlen span c-addr pos2 flag )
	>r 2over = r> or
    UNTIL
    2 pick swap /string type
    nip span ! ;

\ marker                                               18dec94py

\ Marker creates a mark that is removed (including everything 
\ defined afterwards) when executing the mark.

: marker, ( -- mark )  here dup A,
  voclink @ A, voclink
  BEGIN  @ dup WHILE  dup 0 wordlist-link - @ A,  REPEAT  drop
  udp @ , ;

: marker! ( mark -- )
    dup @ swap cell+
    dup @ voclink ! cell+
    voclink
    BEGIN
	@ dup 
    WHILE
	over @ over 0 wordlist-link - !
	swap cell+ swap
    REPEAT
    drop  voclink
    BEGIN
	@ dup
    WHILE
	dup 0 wordlist-link - rehash
    REPEAT
    drop
    @ udp !  dp ! ;

: marker ( "mark" -- )
    marker, Create A,
DOES> ( -- )
    @ marker! ;

