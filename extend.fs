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

: value ( w -- ) \ core-ext
  (constant) , ;
\ !! 2value

: 2Literal ( compilation w1 w2 -- ; run-time  -- w1 w2 ) \ double two-literal
    swap postpone Literal  postpone Literal ; immediate restrict

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

\ UNUSED                                                17may93jaw

: unused ( -- u ) \ core-ext
    s0 @ 512 -        \ for stack
    here - ;

\ [COMPILE]                                             17may93jaw

: [compile] ( compilation "name" -- ; run-time ? -- ? ) \ core-ext bracket-compile
    ' compile, ; immediate

\ MARKER                                                17may93jaw

\ : marker here last @ create , , DOES> dup @ last ! cell+ @ dp ! ;
\ doesn't work now. vocabularies?

\ CONVERT                                               17may93jaw

: convert ( ud1 c-addr1 -- ud2 c-addr2 ) \ core-ext
    \ obsolescent; supersedet by @code{>number}.
    true >number drop ;

\ ERASE                                                 17may93jaw

: erase ( addr len -- ) \ core-ext
    \ !! dependence on "1 chars 1 ="
    ( 0 1 chars um/mod nip )  0 fill ;
: blank ( addr len -- ) \ string
    bl fill ;

\ SEARCH                                                02sep94py

: search   ( buf buflen text textlen -- restbuf restlen flag ) \ string
    2over  2 pick - 1+ 3 pick c@ >r
    BEGIN
	r@ scan dup
    WHILE
	>r >r  2dup r@ -text
	0=
	IF
	    >r drop 2drop r> r> r> rot + 1- rdrop true
	    EXIT
	THEN
	r> r>  1 /string
    REPEAT
    2drop 2drop  rdrop false ;

\ ROLL                                                  17may93jaw

: roll  ( x0 x1 .. xn n -- x1 .. xn x0 ) \ core-ext
  dup 1+ pick >r
  cells sp@ cell+ dup cell+ rot move drop r> ;

\ SOURCE-ID SAVE-INPUT RESTORE-INPUT                    11jun93jaw

: source-id ( -- 0 | -1 | fileid ) \ core-ext source-i-d
  loadfile @ dup 0= IF  drop sourceline# 0 min  THEN ;

: save-input ( -- x1 .. xn n ) \ core-ext
  >in @
  loadfile @ ?dup
  IF    dup file-position throw sourceline# >tib @ 6
        #tib @ >tib +!
  ELSE  sourceline# blk @ linestart @ >tib @ 5 THEN
;

: restore-input ( x1 .. xn n -- flag ) \ core-ext
  swap >tib !
  6 = IF   loadline ! rot dup loadfile !
           reposition-file IF drop true EXIT THEN
      ELSE linestart ! blk !
           dup sourceline# <> IF 2drop true EXIT THEN
           loadline !
      THEN
  >in ! false ;



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
    type-rest drop
    2drop nip span ! ;

