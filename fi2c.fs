\ Convert image to C include file

\ Copyright (C) 1998 Free Software Foundation, Inc.

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

0 Value image
0 Value bitmap

Create magicbuf 8 allot

Variable bswap?
Variable tchars
Variable tcell
Variable au

: bswap ( n -- n' )  bswap? @ 0= ?EXIT  0
    over 24 rshift $FF       and or
    over  8 rshift $FF00     and or
    over  8 lshift $FF0000   and or
    over 24 lshift $FF000000 and or nip ;

: search-magic ( fd -- )  >r
    BEGIN  magicbuf 8 r@ read-file throw  8 =  WHILE
	magicbuf s" Gforth2" tuck compare 0=  UNTIL
    ELSE  true abort" No magic found"  THEN
    1 magicbuf 7 + c@ 5 rshift 3 and lshift tchars !
    1 magicbuf 7 + c@ 1 rshift 3 and lshift tcell !
    1 magicbuf 7 + c@ 3 rshift 3 and lshift au !
    magicbuf 7 + c@ 1 and 0=
    [ pad off 1 pad ! pad c@ 1 = ] Literal = bswap? !
    ." /* Image with " tcell @ . ." bytes cell, " tchars @ . ." bytes per char and " au @ . ." bytes per address unit */" cr
    rdrop ;

Create image-header  4 cells allot
Variable image-cells
Variable bitmap-chars

: read-header ( fd -- )
    image-header 4 cells rot read-file throw drop
    image-header 2 cells + @ bswap tchars @ * au @ /
    dup cell / image-cells ! 1- 8 cells / 1+ bitmap-chars !
    image-cells @ cells allocate throw to image
    bitmap-chars @ allocate throw to bitmap ;

: read-dictionary ( fd -- )  >r
    image image-cells @ cells r> read-file throw drop ;

: read-bitmap ( fd -- )  >r
    bitmap bitmap-chars @ r> read-file throw drop ;

: .08x ( n -- ) 0 <# tcell  @ 0 ?DO # # LOOP 'x hold '0 hold #> type ;
: .02x ( n -- ) 0 <# tchars @ 0 ?DO # # LOOP 'x hold '0 hold #> type ;

: .image ( -- )
    image-cells @ 0 ?DO
	I 4 + I' min I ?DO  space image I cells + @ bswap .08x ." ," LOOP cr
	4 +LOOP ;

: .reloc ( -- )
    bitmap-chars @ 0 ?DO
	I $10 + I' min I ?DO  space
	    0 I tchars @ bounds ?DO  8 lshift bitmap I + c@ +  LOOP
	    .02x ." ," tchars @ +LOOP cr
	$10 +LOOP ;

: read-image ( addr u -- )
    r/o bin open-file throw >r
    r@ search-magic
    r@ file-position throw r@ read-header r@ reposition-file throw
    r@ read-dictionary r@ read-bitmap r> close-file throw ;

: .imagesize ( -- )
    image-header 3 cells + @ bswap tcell @ / tchars @ * au @ / .08x ;

: .relocsize ( -- )
    bitmap-chars @ 1- tchars @ / 1+ .08x ;

: fi2c ( addr u -- )  base @ >r hex
    read-image
    ." #include " '" emit ." forth.h" '" emit cr
    ." Cell image[" .imagesize ." ] = {" cr .image ." };" cr
    ." const char reloc_bits[" .relocsize ." ] = {" cr .reloc ." };" cr
    r> base ! ;

