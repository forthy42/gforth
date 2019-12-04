\ a simple jpeg parser to read important EXIF stuff

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016 Free Software Foundation, Inc.

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

\ Exif is FF E1 len<16>
\ SOI is FF D8

0 Value jpeg-fd
0 Value exif-end
0 Value exif-endian
0 Value exif-start

s" Invalid JPEG file" exception Constant !!no-jpeg!!
s" Exif exhausted"    exception Constant !!oo-exif!!
s" Not an Exif chunk" exception Constant !!no-exif!!

Defer exb
Defer ex-seek? ( -- u )
Defer ex-seek ( u -- )
Defer exif-read ( u -- addr u )
Defer exif>read ( addr u -- )
Defer >exif-open ( addr u -- )

\ file variant

: file-exb ( -- c ) jpeg-fd key-file ;
: file-ex-seek? ( -- u )
    jpeg-fd file-position throw drop ;
: file-ex-seek ( u -- )
    0 jpeg-fd reposition-file throw ;
: file-exif-read ( n -- addr u )
    pad swap jpeg-fd read-file throw
    pad swap ;
: file-exif>read ( addr u -- u' )
    jpeg-fd read-file throw ;
: file>exif-open ( addr u -- )
    r/o open-file throw to jpeg-fd ;

: file-exif ( -- )
    ['] file-exb is exb
    ['] file-ex-seek? is ex-seek?
    ['] file-ex-seek is ex-seek
    ['] file-exif-read is exif-read
    ['] file-exif>read is exif>read
    ['] file>exif-open is >exif-open ;
file-exif

\ memory variant

2Variable exif-mem
0 Value exif-pos
: mem-exif/ ( -- addr u )  exif-mem 2@ exif-pos safe/string ;
: mem-exb ( -- c )
    mem-exif/ drop c@  1 +to exif-pos ;
: mem-ex-seek ( u -- )
    to exif-pos ;
: mem-exif-read ( u -- addr u )
    >r mem-exif/ r@ umin pad swap 2dup 2>r move 2r>
    r> +to exif-pos ;
: mem-exif>read ( addr u -- u' )
    2>r mem-exif/ r> umin dup +to exif-pos r> swap dup >r move r> ;
: mem>exif-open ( addr u -- )
    exif-mem 2!  0 to exif-pos ;

: mem-exif ( -- )
    ['] mem-exb is exb
    ['] exif-pos is ex-seek?
    ['] mem-ex-seek is ex-seek
    ['] mem-exif-read is exif-read
    ['] mem-exif>read is exif>read
    ['] mem>exif-open is >exif-open ;

: jpeg+seek ( n -- )  ex-seek? + ex-seek ;

: ?tag ( -- )
    exb $FF <> IF  !!no-jpeg!! throw  THEN ;

: read-tag ( -- tag ) ?tag exb ;

: read-len ( -- len )  exb 8 lshift exb or 2 - ;

: ?soi ( -- )  read-tag $D8 <> IF  !!no-jpeg!! throw  THEN ;

: search-exif ( -- len )
    BEGIN  read-tag dup $D9 $DB within IF  drop 0  EXIT  THEN
	$E1 <>  WHILE  read-len jpeg+seek  REPEAT
    read-len ;

: >exif-st ( -- flag )
    ?soi search-exif  dup 0= ?EXIT  ex-seek? + to exif-end  true ;
: >exif ( addr u -- flag )
    >exif-open  >exif-st ;

\ exif tags

: exif-seek ( n -- )  exif-start + ex-seek ;

: exif-read-at ( n offset -- addr u )
    ex-seek? >r exif-seek exif-read  r> ex-seek ;

: exif-slurp ( u offset -- addr u )
    ex-seek? >r exif-seek >r
    r@ allocate throw dup r> exif>read
    r> ex-seek ;

: ex>< ( n1 n2 -- n3 n4 )  exif-endian IF  swap  THEN ;
: exw ( -- word )
    exb exb ex><  8 lshift or ;
: exl ( -- long )
    exw exw ex>< 16 lshift or ;

: >exif-start ( -- )
    ex-seek? to exif-start ;

: ?exif ( -- )
    6 exif-read "Exif\0\0" str= 0= IF  !!no-exif!! throw  THEN
    >exif-start
    8 exif-read 2dup "II*\0\10\0\0\0" str= IF
	2drop false to exif-endian  EXIT  THEN
    "MM\0*\0\0\0\10" str= IF
	true to exif-endian  EXIT  THEN
    !!no-exif!! throw ;

\ read and print exif information

Create exif-sizes 0 c, 1 c, 1 c, 2 c, 4 c, 8 c, 1 c, 1 c, 2 c, 4 c, 7 c, 4 c, 8 c,
DOES> + c@ ;

: .exif-tag ( -- )
    exw exw exl exl { cmd type len offset }
    cmd hex. type hex. len hex.
    type exif-sizes len * { size }
    size 4 > IF
	cr size offset exif-read-at $100 umin dump
    ELSE
	offset hex. cr
    THEN ;

: .exif-tags ( -- )
    exw 0 ?DO  .exif-tag  LOOP ;

: .exifs ( -- )
    .exif-tags exl exif-seek .exif-tags ;

\ search for thumbnail image

0 Value thumb-off
0 Value thumb-len
0 Value img-orient

: >thumb ( -- )
    exw 0 ?DO
	exw exw exl { cmd type len }
	exl case cmd
	    $112 of  to img-orient  endof
	    $201 of  to thumb-off   endof
	    $202 of  to thumb-len   endof
	    nip
	endcase
    LOOP ;

: >thumb-scan ( fn-addr u1 -- )
    >exif-open
    0 to img-orient  0 to thumb-off  0 to thumb-len
    >exif-st IF  ?exif exw 12 * jpeg+seek exl exif-seek >thumb  THEN ;

: exif-close ( -- )
    jpeg-fd ?dup-IF   close-file 0 to jpeg-fd throw  THEN ;
: thumbnail@ ( -- addr u )
    thumb-len thumb-off exif-slurp ;
: >thumbnail ( fn-addr u1 -- jpeg-addr u2 )
    >thumb-scan thumbnail@ exif-close ;
