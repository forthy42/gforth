\ wrap TYPE and EMIT into strings using string.fs
\
\ Copyright (C) 2013,2014 Free Software Foundation, Inc.

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

\ overwrite

: $over ( addr u $addr off -- )
    \G overwrite string at offset off with addr u
    swap >r
    r@ @ 0= IF  s" " r@ $!  THEN
    2dup + r@ $@len > IF
	2dup + r@ $@len tuck max r@ $!len
	r@ $@ rot /string bl fill
    THEN
    r> $@ rot /string rot umin move ;

\ string array words

: $[] ( n addr -- addr' )
    \G index into the string array and return the address at index n
    >r
    r@ @ 0= IF  s" " r@ $!  THEN
    r@ $@ 2 pick cells /string
    dup cell < IF
	2drop r@ $@len
	over 1+ cells r@ $!len
	r@ $@ rot /string 0 fill
	r@ $@ 2 pick cells /string
    THEN  drop nip rdrop ;

: $[]! ( addr u n $[]addr -- )  $[] $! ;
\G store a string into an array at index n
: $[]+! ( addr u n $[]addr -- )  $[] $+! ;
\G add a string to the string at addr n
: $[]@ ( n $[]addr -- addr u )  $[] $@ ;
\G fetch a string from array index n -- return the zero string if empty
: $[]# ( addr -- len )          $@len cell/ ;
\G return the number of elements in an array
: $+[]! ( addr u $[]addr -- ) dup $[]# swap $[]! ;
\G add a string at the end of the array

User tmp$[] \ temporary string buffers
User tmp$#  \ temporary string buffer counter
$10 Value tmps# \ how many temporary strings

: tmp$ ( -- addr )
    tmp$# @ tmp$[] $[] ;

User $execstr-ptr
tmp$ $execstr-ptr !

: $type ( addr u -- )  $execstr-ptr @ $+! ;
: $emit ( char -- )    $execstr-ptr @ c$+! ;
: $cr   ( -- ) newline $type ;
24 80 2Constant $form

' $type ' $emit ' $cr ' $form output: $-out

: $exec ( xt addr -- )
    \G execute xt while the standard output (TYPE, EMIT, and everything
    \G that uses them) is redirected to the string variable addr.
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
: $. ( addr -- )
    \G print a string, shortcut
    $@ type ;

: $tmp ( xt -- addr u )
    \G generate a temporary string from the output of a word
    1 tmp$# @ + tmps# mod tmp$# !
    s" " tmp$ $!  tmp$ $exec  tmp$ $@ ;

:noname ( -- )  defers 'cold  tmp$[] off ;  is 'cold

\ slurp in lines and files into strings and string-arrays

: $slurp ( fid addr -- )
    \G slurp a file @var{fid} into a string @var{addr2}
    dup $init swap >r
    r@ file-size throw drop over $!len
    0. r@ reposition-file throw
    dup $@ r> read-file throw swap $!len ;
: $slurp-file ( addr1 u1 addr2 -- )
    \G slurp a named file @var{addr1 u1} into a string @var{addr2}
    >r r/o open-file throw dup r> $slurp close-file throw ;

: $slurp-line { fid addr -- flag }  addr $off  addr $init
    BEGIN
	addr $@len dup { sk } 2* $100 umax dup { sz } addr $!len
	addr $@ sk /string fid read-line throw
	swap dup sz = WHILE  2drop  REPEAT  sk + addr $!len ;
: $[]slurp { fid addr -- }
    \G slurp a file @var{fid} line by line into a string array @var{addr}
    0 { i }  BEGIN  fid i addr $[] $slurp-line  WHILE
	    i 1+ to i   REPEAT
    \ we need to take off the last line, though
    i addr $[] $off  i cells addr $!len ;
: $[]slurp-file ( addr u $addr -- )
    \G slurp a named file @var{addr u} line by line into a string array @var{$addr}
    >r r/o open-file throw dup r> $[]slurp close-file throw ;

: $[]map { addr xt -- }
    \G execute @var{xt} for all elements of the string array @var{addr}.
    \G xt is @var{( addr u -- )}, getting one string at a time
    addr $[]# 0 ?DO  I addr $[]@ xt execute  LOOP ;
: $[]. ( addr -- )
    \G print all array entries
    [: type cr ;] $[]map ;
: $[]off ( addr -- )
    \G release a string array
    dup $[]# 0 ?DO  I over $[] $off  LOOP  $off ;