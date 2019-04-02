\ a simple jpeg parser to read important EXIF stuff

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

: ?tag ( -- )
    jpeg-fd key-file $FF <> IF  !!no-jpeg!! throw  THEN ;

: read-tag ( -- tag ) ?tag jpeg-fd key-file ;

: read-len ( -- len )  jpeg-fd key-file 8 lshift jpeg-fd key-file or 2 - ;

: ?soi ( -- )  read-tag $D8 <> IF  !!no-jpeg!! throw  THEN ;

: jpeg+seek ( n -- )  s>d
    jpeg-fd file-position throw d+
    jpeg-fd reposition-file throw ;

: search-exif ( -- len )
    BEGIN  read-tag dup $D9 $DB within IF  drop 0  EXIT  THEN
	$E1 <>  WHILE  read-len jpeg+seek  REPEAT
    read-len ;

: >exif ( addr u -- flag )
    r/o open-file throw to jpeg-fd ?soi search-exif
    dup 0= ?EXIT
    jpeg-fd file-position throw drop + to exif-end
    true ;

\ exif tags

: exif-seek ( n -- )  exif-start + 0 jpeg-fd reposition-file throw ;

: exif-read ( n -- addr u )
    pad swap jpeg-fd read-file throw
    pad swap ;

: exif-read-at ( n offset -- addr u )
    jpeg-fd file-position throw 2>r exif-seek exif-read
    2r> jpeg-fd reposition-file throw ;

: exif-slurp ( u offset -- addr u )
    jpeg-fd file-position throw 2>r exif-seek >r
    r@ allocate throw dup r> jpeg-fd read-file throw
    2r> jpeg-fd reposition-file throw ;

: ex>< ( n1 n2 -- n3 n4 )  exif-endian IF  swap  THEN ;
: exb ( -- byte )
    jpeg-fd key-file ;
: exw ( -- word )
    exb exb ex><  8 lshift or ;
: exl ( -- long )
    exw exw ex>< 16 lshift or ;

: >exif-start ( -- )
    jpeg-fd file-position throw drop to exif-start ;

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
    0 to img-orient  0 to thumb-off  0 to thumb-len
    >exif IF  ?exif exw 12 * jpeg+seek exl exif-seek >thumb  THEN ;

: exif-close ( -- )
    jpeg-fd ?dup-IF   close-file 0 to jpeg-fd throw  THEN ;
: thumbnail@ ( -- addr u )
    thumb-len thumb-off exif-slurp ;
: >thumbnail ( fn-addr u1 -- jpeg-addr u2 )
    >thumb-scan thumbnail@ exif-close ;
