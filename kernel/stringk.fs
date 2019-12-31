\ dynamic string handling                              10aug99py

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2000,2005,2007,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

: delete   ( buffer size u -- ) \ gforth-string
    \G deletes the first @var{u} bytes from a buffer and fills the
    \G rest at the end with blanks.
    over umin >r  r@ - ( left over )
    2dup swap dup  r@ +  -rot swap move  + r> bl fill ;
: insert   ( string length buffer size -- ) \ gforth-string
    \G inserts a string at the front of a buffer. The remaining
    \G bytes are moved on.
    rot over umin >r  r@ - ( left over )
    over dup r@ +  rot move   r> move  ;

[IFUNDEF] >pow2
    : >pow2 ( n -- pow2 )
	1-
	dup 2/ or \ next power of 2
	dup 2 rshift or
	dup 4 rshift or
	dup 8 rshift or
	dup #16 rshift or
	[ cell 8 = [IF] ]
	    dup #32 rshift or
	[ [THEN] ] 1+ ;
[THEN]

: $padding ( n -- n' ) \ gforth-string
    [ 6 cells ] Literal +  >pow2  [ -4 cells ] Literal and ;
: $free ( $addr -- ) \ gforth-string string-free
    \G free the string pointed to by addr, and set addr to 0
    0 swap !@ ?dup-IF  free throw  THEN ;
' $free alias $off \ set the string to the neutral element

: $!buf ( $buf $addr -- ) \ gforth-string string-store-buf
    \G stores a buffer in a string variable and frees the previous buffer
    !@ ?dup-IF  free throw  THEN ;
: $make ( addr1 u -- $buf )
    \G create a string buffer as address on stack, which can be stored into
    \G a string variable, internal factor
    dup $padding allocate throw dup >r
    2dup ! cell+ swap move r> ;
: $@len ( $addr -- u ) \ gforth-string string-fetch-len
    \G returns the length of the stored string.
    @ dup IF  @  THEN ;
: $!len ( u $addr -- ) \ gforth-string string-store-len
    \G changes the length of the stored string.  Therefore we must
    \G change the memory area and adjust address and count cell as
    \G well.
    over $padding  over @ IF  \ fast path for unneeded size change
	over @ @ $padding over = IF  drop @ !  EXIT  THEN
    THEN
    over @ swap resize throw over ! @ ! ;
: $! ( addr1 u $addr -- ) \ gforth-string string-store
    \G stores a newly allocated string buffer at an address,
    \G frees the previous buffer if necessary.
    dup @ IF  \ fast path for strings with similar buffer size
	over $padding over @ @ $padding = IF
	    @ 2dup ! cell+ swap move  EXIT
	THEN  THEN
    2dup $!len @ cell+ swap move ;
: $@ ( $addr -- addr2 u ) \ gforth-string string-fetch
    \G returns the stored string.
    @ dup IF  dup cell+ swap @  ELSE  0  THEN ;
: $+!len ( u $addr -- addr )
    \G make room for u bytes at the end of the memory area referenced
    \G by $addr; addr is the address of the first of these bytes.
    >r r@ $@len tuck + r@ $!len r> @ cell+ + ;
: $+! ( addr1 u $addr -- ) \ gforth-string string-plus-store
    \G appends a string to another.
    over >r $+!len r> move ;
: c$+! ( char $addr -- ) \ gforth-string c-string-plus-store
    \G append a character to a string.
    dup $@len 1+ over $!len $@ + 1- c! ;

: $ins ( addr1 u $addr off -- ) \ gforth-string string-ins
    \G inserts a string at offset @var{off}.
    >r 2dup dup $@len under+ $!len  $@ r> safe/string insert ;
: $del ( addr off u -- ) \ gforth-string string-del
    \G deletes @var{u} bytes from a string with offset @var{off}.
    >r >r dup $@ r> safe/string r@ delete
    dup $@len r> - 0 max swap $!len ;

: $init ( $addr -- )
    \G store an empty string there, regardless of what was in before
    s" " $make swap ! ;

\ dynamic string handling                              12dec99py

: $split ( addr u char -- addr1 u1 addr2 u2 ) \ gforth-string string-split
    \G divides a string into two, with one char as separator (e.g. '?'
    \G for arguments in an HTML query)
    >r 2dup r> scan dup >r dup IF  1 /string  THEN
    2swap r> - 2swap ;

: $iter ( .. $addr char xt -- .. ) \ gforth-string string-iter
    \G takes a string apart piece for piece, also with a character as
    \G separator. For each part a passed token will be called. With
    \G this you can take apart arguments -- separated with '&' -- at
    \G ease.
    >r >r
    $@ BEGIN  dup  WHILE  r@ $split i' -rot >r >r execute r> r>
    REPEAT  2drop rdrop rdrop ;

\ basics for string arrays

: $room ( u $addr -- )
    \G generate room for at least u bytes, erase when expanding
    >r dup r@ $@len tuck u<= IF  rdrop 2drop EXIT  THEN
    - dup r> $+!len swap 0 fill ;

: $[] ( u $[]addr -- addr' )
    \G index into the string array and return the address at index @var{u}
    \G The array will be resized as needed
    >r cells dup cell+ r@ $room  r> $@ drop + ;

\ auto-save and restore strings in images

: $boot ( $addr -- )
    \G take string from dictionary to allocated memory.
    \G clean dictionary afterwards.
    dup >r $@ 2dup r> dup off $! 0 fill ;
: $save ( $addr -- )
    \G push string to dictionary for savesys
    dup >r $@ here r> ! dup , here swap dup aligned allot move ;
: $[]boot ( addr -- )
    \G take string array from dictionary to allocated memory
    dup $boot  $@ bounds ?DO
	I $boot
    cell +LOOP ;
: $[]save ( addr -- )
    \G push string array to dictionary for savesys
    dup $save $@ bounds ?DO
	I $save
    cell +LOOP ;

AVariable boot$[]  \ strings to be booted
AVariable boot[][] \ arrays to be booted

: $saved ( addr -- )
    \ mark an address as booted/saved
    boot$[] >stack ;
: $[]saved ( addr -- )
    \ mark an address as booted/saved
    boot[][] >stack ;
: $Variable ( -- )
    \G A string variable which is preserved across savesystem
    Create here $saved 0 , ;
: $[]Variable ( -- )
    \G A string variable which is preserved across savesystem
    Create here $[]saved 0 , ;
: boot-strings ( -- )
    boot[][] @ >r
    boot[][] $boot
    boot$[] $boot
    boot[][] $@ bounds ?DO
	I @ $[]boot
    cell +LOOP
    boot$[] $@ bounds ?DO
	I @ $boot
    cell +LOOP
    r> dp ! ;
: save-strings ( -- )
    boot[][] $save
    boot$[] $save
    boot[][] $@ bounds ?DO
	I @ $[]save
    cell +LOOP
    boot$[] $@ bounds ?DO
	I @ $save
    cell +LOOP ;

Defer 'image ( -- ) \G deferred word executed before saving an image
' save-strings IS 'image

