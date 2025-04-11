\ wrap TYPE and EMIT into strings using string.fs
\
\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2013,2014,2015,2016,2017,2019,2020,2021,2022,2023 Free Software Foundation, Inc.

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

require glocals.fs

\ overwrite

: $over ( addr u $addr off -- ) \ gforth string-over
    \G Overwrite @var{u} bytes at offset @var{off} bytes in the string
    \G @var{$addr} with the string at @var{addr u}.
    swap dup
    >r @ 0= IF  s" " r@ $!  THEN
    2dup + r@ $@len > IF
	2dup + r@ $@len tuck max r@ $!len
	r@ $@ rot /string bl fill
    THEN
    r> $@ rot /string rot umin move ;
synonym $off $free \ legacy, don't use

\ string array words

: $[]! ( c-addr u n $[]addr -- ) \ gforth string-array-store
    \G Store string @i{c-addr y} into the string array @i{$[]addr} at
    \G index @i{n}.  The array is resized if needed.
    $[] $! ;

: $[]+! ( c-addr u n $[]addr -- ) \ gforth string-array-plus-store
    \G Append the string @i{c-addr u} to the string at index @i{n}.
    \G The array is resized if needed.  Don't confuse this with
    \G @code{$+[]!}.
    $[] $+! ;

: $[]# ( $[]addr -- len ) \ gforth string-array-num
\G return the number of elements in an array
    $@len cell/ ;
: $[]@ ( n $[]addr -- addr u ) \ gforth string-array-fetch
\G fetch a string from array index @i{n} --- return the zero string if
\G empty, and don't accidentally grow the array.
    2dup $[]# u< IF $[] $@ ELSE 2drop 0. THEN ;
: $+[]! ( c-addr u $[]addr -- ) \ gforth string-append-array
    \G Store the string @i{c-addr u} as the new last element of string
    \G array @i{$[]addr}.  The array is resized if needed.
    dup $[]# swap $[]! ;

User tmp$[] \ temporary string buffers
User tmp$#  \ temporary string buffer counter
$F Value tmps# \ how many temporary strings -1 (pow of 2!)

: tmp$ ( -- addr )
    tmp$# @ tmps# and tmp$[] $[] ;

User $execstr-ptr
tmp$ $execstr-ptr !

: $type ( addr u -- )  $execstr-ptr @ $+! ;
: $emit ( char -- )    $execstr-ptr @ c$+! ;
: $cr   ( -- ) newline $type ;
1 -1 1 rshift 2Constant $form

' $type ' $emit ' $cr ' $form output: $-out

: $exec ( xt $addr -- ) \ gforth string-exec
    \G execute xt while the standard output (TYPE, EMIT, and everything
    \G that uses them) is appended to the string in @var{$addr}.
    $execstr-ptr @ op-vector @
    { oldstr oldout }
    try
	$execstr-ptr ! $-out execute
	0 \ throw ball
    restore
	oldstr $execstr-ptr !
	oldout op-vector !
    endtry
    throw ;
: $. ( $addr -- ) \ gforth string-dot
    \G print a string, shortcut
    $@ type ;

: $tmp ( xt -- addr u ) \ gforth string-t-m-p
    \G Like @code{>string-execute}, but the result is deallocated when
    \G @code{$tmp} is invoked the next time, and you must not
    \G @code{free} it yourself.
    1 tmp$# +!@ drop 0 { w^ tmp$$ } tmp$$ $exec
    tmp$$ @ tmp$ tuck dup $free ! $@ ;

:noname ( -- )  defers 'image tmp$[] off $execstr-ptr off ; is 'image

\ slurp in lines and files into strings and string-arrays

: $+slurp ( fid $addr -- ) \ gforth string-plus-slurp
    \G Read the file @i{fid} until the end (without closing it) and
    \G append the read data to the string at @i{$addr}.
    swap dup >r file-size throw r@ file-position throw d- drop
    dup rot r> over >r >r $+!len swap r> over >r read-file throw
    r> - dup 0< IF  r> $+!len drop  ELSE  drop rdrop  THEN ;

: $slurp ( fid $addr -- ) \ gforth string-slurp
    \G Read the file @i{fid} until the end (without closing it) and put
    \G the read data into the string at @i{$addr}.
    dup $free $+slurp ;

: $+slurp-file ( c-addr u $addr -- ) \ gforth string-plus+slurp-file
    \G Append all the data in the file named @i{c-addr u} to the
    \G string at @i{$addr}.
    >r r/o open-file throw dup r> $+slurp close-file throw ;

: $slurp-file ( c-addr u $addr -- ) \ gforth string-slurp-file
    \G Put all the data in the file named @i{c-addr u} into the string
    \G at @i{$addr}.
    dup $free $+slurp-file ;

: $slurp-line { fid addr -- flag }  addr $free
    BEGIN
	addr $@len dup { sk } $100 umax dup >r addr $+!len
	r@ fid read-line throw
	swap dup r> = WHILE  2drop  REPEAT  sk + addr $!len ;
: $[]slurp ( fid $[]addr -- ) \ gforth string-array-slurp
    \G slurp a file @var{fid} line by line into a string array @var{$[]addr}
    { fid addr }
    0 { ii }  BEGIN  fid ii addr $[] $slurp-line  WHILE
	    ii 1+ to ii   REPEAT
    \ we need to take off the last line, though, if it is empty
    ii addr $[]@ nip IF  ii 1+ to ii  THEN
    addr $[]# ii U+DO  I addr $[] $free  LOOP
    ii cells addr $!len ;
: $[]slurp-file ( addr u $[]addr -- ) \ gforth string-array-slurp-file
    \G slurp a named file @var{addr u} line by line into a string array @var{$[]addr}
    >r r/o open-file throw dup r> $[]slurp close-file throw ;

: $[]map ( $[]addr xt -- ) \ gforth string-array-map
    \G execute @var{xt} for all elements of the string array @var{$[]addr}.
    \G xt is @var{( addr u -- )}, getting one string at a time
    { addr xt }
    addr $[]# 0 ?DO  I addr $[]@ xt execute  LOOP ;
: $[]. ( $[]addr -- ) \ gforth string-array-dot
    \G print all array entries
    [: type cr ;] $[]map ;
: $[]free ( $[]addr -- ) \ gforth string-array-free
    \G @var{$[]addr} contains the address of a cell-counted string that contains the
    \G addresses of a number of cell-counted strings; $[]free frees
    \G these strings, frees the array, and sets addr to 0
    dup $[]# 0 ?DO I over $[] $free LOOP $free ;
