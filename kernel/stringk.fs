\ dynamic string handling                              10aug99py

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2000,2005,2007,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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

: delete   ( c-addr u u1 -- ) \ gforth
    \G In the memory block @i{c-addr u}, delete the first @i{u1} chars
    \G by copying the contents of the block starting at
    \G @i{c-addr}+@i{u1} there; fill the @i{u1} characters at the end
    \G of the block with blanks.
    over umin dup  >r - ( left over )
    2dup swap dup  r@ +  -rot swap move  + r> bl fill ;

: insert   ( c-addr1 u1 c-addr2 u2 -- ) \ gforth
    \G Move the contents of the buffer @i{c-addr2 u2} towards higher
    \G addresses by @i{u1} chars, and copy the string @i{c-addr1 u1}
    \G into the first @i{u1} chars of the buffer.
    rot over umin dup  >r - ( left over )
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

: $padding ( n -- n' ) \ gforth-internal
    \ Round up string length to next power of 2 for
    \ @code{allocate}/@code{resize}.
    [ 6 cells 1- ] Literal + [ -4 cells ] Literal and >pow2 ;
: $free ( $addr -- ) \ gforth string-free
    \G free the string pointed to by addr, and set addr to 0
    0 swap atomic!@ ?dup-IF  free throw  THEN ;

: $!buf ( $buf $addr -- ) \ gforth-internal string-store-buf
    \G stores a buffer in a string variable and frees the previous buffer
    atomic!@ ?dup-IF  free throw  THEN ;
: $make ( addr1 u -- $buf )
    \G create a string buffer as address on stack, which can be stored into
    \G a string variable, internal factor
    dup $padding allocate throw dup >r
    2dup ! cell+ swap move r> ;
: $@len ( $addr -- u ) \ gforth string-fetch-len
    \G returns the length of the stored string.
    @ dup IF  @  THEN ;
: $!len ( u $addr -- ) \ gforth string-store-len
    \G changes the length of the stored string.  Therefore we must
    \G change the memory area and adjust address and count cell as
    \G well.
    over $padding  over @ IF  \ fast path for unneeded size change
	over @ @ $padding over = IF  drop @ !  EXIT  THEN
    THEN
    over @ swap resize throw over ! @ ! ;
: $! ( addr1 u $addr -- ) \ gforth string-store
    \G stores a newly allocated string buffer at an address,
    \G frees the previous buffer if necessary.
    dup @ IF  \ fast path for strings with similar buffer size
	over $padding over @ @ $padding = IF
	    @ 2dup ! cell+ swap move  EXIT
	THEN  THEN
    2dup $!len @ cell+ swap move ;
: $@ ( $addr -- addr2 u ) \ gforth string-fetch
    \G returns the stored string.
    @ dup IF  dup cell+ swap @  ELSE  0  THEN ;
: $+!len ( u $addr -- addr ) \ gforth string-plus-store-len
    \G make room for u bytes at the end of the memory area referenced
    \G by $addr; addr is the address of the first of these bytes.
    dup >r $@len tuck + r@ $!len r> @ cell+ + ;
: $+! ( addr1 u $addr -- ) \ gforth string-plus-store
    \G appends a string to another.
    over >r $+!len r> move ;
: c$+! ( char $addr -- ) \ gforth c-string-plus-store
    \G append a character to a string.
    dup $@len 1+ over $!len $@ + 1- c! ;

: $ins ( addr1 u $addr off -- ) \ gforth string-ins
    \G Inserts string @var{addr1 u} at offset @var{off} bytes in the
    \G string @var{$addr}.
    >r 2dup dup $@len under+ $!len  $@ r> safe/string insert ;
: $del ( $addr off u -- ) \ gforth string-del
    \G Deletes @var{u} bytes at offset @var{off} bytes in the string @var{$addr}.
    >r >r dup $@ r> safe/string r@ delete
    dup $@len r> - 0 max swap $!len ;

: $init ( $addr -- ) \ gforth string-init
    \G store an empty string there, regardless of what was in before
    s" " $make swap ! ;

\ dynamic string handling                              12dec99py

: $split ( c-addr u char -- c-addr u1 c-addr2 u2 ) \ gforth string-split
    \G Divides a string @i{c-addr u} into two, with @i{char} as
    \G separator.  @i{U1} is the length of the string up to, but
    \G excluding the first occurrence of the separator, @i{c-addr2 u2}
    \G is the part of the input string behind the separator.  If the
    \G separator does not occur in the string, @i{u1}=@i{u}, @i{u2}=0
    \G and @i{c-addr2}=@i{c-addr}+@i{u}.
    >r 2dup r> scan dup >r dup IF 1 /string THEN
    2swap r> - 2swap ;

: $iter ( .. $addr char xt -- .. ) \ gforth string-iter
    \G Splits the string in @i{$addr} using @i{char} as separator.
    \G For each part, its descriptor @i{c-addr u} is pushed and @i{xt}
    \G @code{( @i{... c-addr u} -- @i{...} )} is executed.
    >r >r
    $@ BEGIN  dup  WHILE  r@ $split r'@ -rot >r >r execute r> r>
    REPEAT  2drop rdrop rdrop ;

\ basics for string arrays

: $room ( u $addr -- )
    \G generate room for at least @i{u} bytes, erase when expanding
    >r dup r@ $@len tuck u<= IF  rdrop 2drop EXIT  THEN
    - dup r> $+!len swap 0 fill ;

: $[] ( u $[]addr -- addr' ) \ gforth string-array
    \G @i{Addr'} is the address of the @i{u}th element of the string
    \G array @i{$[]addr}.  The array is resized if needed.
    >r cells dup cell+ r@ $room r> $@ drop + ;

\ bitstring access, used for compile-prims

: $bit ( u $addr -- c-addr mask )
    over 8 + 3 rshift over $room
    swap >r $@ drop r@ 3 rshift +
    $80 r> 7 and rshift ;

: $+bit ( u $addr -- )
    \G set bit @i{u} in the string
    $bit over c@ or swap c! ;
: $-bit ( u $addr -- )
    \G clear bit @i{u} in the string
    $bit invert over c@ and swap c! ;
: $bit@ ( u $addr -- flag )
    \G check bit @i{u} in the string
    $bit swap c@ and 0<> ;

\ auto-save and restore strings in images

: $boot ( $addr -- ) \ gforth-internal string-boot
    \G Take string from dictionary to allocated memory.
    \G Clean dictionary afterwards.
    dup >r $@ 2dup r> dup off $! 0 fill ;
: $save ( $addr -- ) \ gforth-internal string-save
    \G push string to dictionary for savesys
    dup >r $@ here r> ! dup , here swap dup aligned allot move ;
: $[]boot ( addr -- ) \ gforth-internal string-array-boot
    \G take string array from dictionary to allocated memory
    dup $boot  $@ bounds ?DO
	I $boot
    cell +LOOP ;
: $[]save ( addr -- ) \ gforth-internal string-array-save
    \G push string array to dictionary for savesys
    dup $save $@ bounds ?DO
	I $save
    cell +LOOP ;

AVariable boot$[]  \ strings to be booted
AVariable boot[][] \ arrays to be booted

: $saved ( addr -- ) \ gforth-internal string-saved
    \G mark an address as booted/saved
    boot$[] >stack ;
: $[]saved ( addr -- ) \ gforth-internal string-array-saved
    \G mark an address as booted/saved
    boot[][] >stack ;
: $Variable ( "name" -- ) \ gforth string-variable
    \G Defines a string variable whose content is preserved across savesystem
    Create here $saved 0 , ;
: $[]Variable ( "name" -- ) \ gforth string-array-variable
    \G Defines a string array variable whose content is preserved across savesystem
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
    r> ->here ;
: save-strings ( -- )
    boot[][] $save
    boot$[] $save
    boot[][] $@ bounds ?DO
	I @ $[]save
    cell +LOOP
    boot$[] $@ bounds ?DO
	I @ $save
    cell +LOOP ;

Defer 'image ( -- )
\G deferred word executed before saving an image
:noname clear-paths clear-args save-strings clear-leave-stack ; IS 'image
