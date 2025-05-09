\ a simple jpeg parser to read important EXIF stuff

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2019,2020,2021,2023,2024 Free Software Foundation, Inc.

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

require mini-oof2.fs

s" Invalid JPEG file" exception Constant !!no-jpeg!!
s" Exif exhausted"    exception Constant !!oo-exif!!
s" Not an Exif chunk" exception Constant !!no-exif!!
s" Invalid WEBP file" exception Constant !!no-webp!!

user-o exif-o

object uclass exif-o
    cell uvar exif-start
    cell uvar exif-end
    cell uvar exif-endian
    cell uvar jpeg-fd
    cell uvar thumb-off
    cell uvar thumb-len
    cell uvar exif-idf
    cell uvar exif-gps
    cell uvar intop-idf
    cell uvar img-orient
    cell uvar img-w
    cell uvar img-h
    cell uvar ©-notice
    cell uvar artist
    cell uvar exif-pad

    umethod exb
    umethod ex-seek? ( -- u )
    umethod ex-seek ( u -- )
    umethod exif-read ( u -- addr u )
    umethod exif>read ( addr u -- )
    umethod >exif-open ( addr u -- )
end-class exif-class

exif-class ' new static-a with-allocater dup constant file-exif-o exif-o !

\ file variant

: file-exb ( -- c ) jpeg-fd @ key-file ;
: file-ex-seek? ( -- u )
    jpeg-fd @ file-position throw drop ;
: file-ex-seek ( u -- )
    0 jpeg-fd @ reposition-file throw ;
: file-exif-read ( n -- addr u )
    exif-pad $free exif-pad $!len
    exif-pad $@ jpeg-fd @ read-file throw
    exif-pad $@ drop swap ;
: file-exif>read ( addr u -- u' )
    jpeg-fd @ read-file throw ;
: file>exif-open ( addr u -- )
    r/o open-file throw jpeg-fd ! ;

' file-exb is exb
' file-ex-seek? is ex-seek?
' file-ex-seek is ex-seek
' file-exif-read is exif-read
' file-exif>read is exif>read
' file>exif-open is >exif-open

: file-exif  file-exif-o exif-o ! ;

\ memory variant

exif-class uclass exif-o
    2 cells uvar exif-mem
    cell uvar exif-pos
end-class exif-mem-class

exif-mem-class new exif-o !

: mem-exif/ ( -- addr u )  exif-mem 2@ exif-pos @ safe/string ;
: mem-exb ( -- c )
    mem-exif/ drop c@  1 exif-pos +! ;
: mem-ex-seek? ( -- u )
    exif-pos @ ;
: mem-ex-seek ( u -- )
    exif-pos ! ;
: mem-exif-read ( u -- addr u )
    exif-pad $free dup exif-pad $!len
    >r mem-exif/ r@ umin exif-pad $@ 2dup 2>r move 2r>
    r> exif-pos +! ;
: mem-exif>read ( addr u -- u' )
    2>r mem-exif/ r> umin dup exif-pos +! r> swap dup >r move r> ;
: mem>exif-open ( addr u -- )
    exif-mem 2!  0 exif-pos ! ;

' mem-exb is exb
' mem-ex-seek? is ex-seek?
' mem-ex-seek is ex-seek
' mem-exif-read is exif-read
' mem-exif>read is exif>read
' mem>exif-open is >exif-open

: mem-exif ( -- )
    exif-mem-class new exif-o ! ;
: exif> ( -- )
    exif-o @ .dispose file-exif ;

exif>

: +seek ( n -- )  ex-seek? + ex-seek ;

: ?tag ( -- )
    exb $FF <> IF  !!no-jpeg!! throw  THEN ;

: read-tag ( -- tag ) ?tag exb ;

: read-len ( -- len )  exb 8 lshift exb or 2 - ;

: ?soi ( -- )  read-tag $D8 <> IF  !!no-jpeg!! throw  THEN ;

: search-exif.jpeg ( -- len )
    BEGIN  read-tag dup $D9 $DB within IF  drop 0  EXIT  THEN
	$E1 <>  WHILE  read-len +seek  REPEAT
    read-len ;
: search-exif.webp ( -- len )
    BEGIN  4 exif-read "EXIF" str= 0= WHILE
	    4 exif-read 4 = WHILE   l@ lle +seek  REPEAT
	drop 0  EXIT  THEN
    4 exif-read 4 = IF  l@ lle  ELSE  drop 0  THEN ;

: >exif-st.jpeg ( -- flag )
    ?soi search-exif.jpeg  dup 0= ?EXIT  ex-seek? + exif-end !  true ;
: >exif-st.webp ( -- flag )
    4 exif-read "RIFF" str= 0= IF  !!no-webp!! throw  THEN
    4 exif-read 2drop
    4 exif-read "WEBP" str= 0= IF  !!no-webp!! throw  THEN
    search-exif.webp dup 0= ?EXIT  ex-seek? + exif-end !  true ;

\ exif tags

: exif-seek ( n -- )  exif-start @ + ex-seek ;

: exif-read-at ( offset n -- addr u )
    ex-seek? >r swap exif-seek exif-read  r> ex-seek ;

: exif-slurp ( offset u -- addr u )
    ex-seek? >r >r exif-seek
    r@ allocate throw dup r> exif>read
    r> ex-seek ;

: ex>< ( n1 n2 -- n3 n4 )  exif-endian @ IF  swap  THEN ;
: exw ( -- word )
    exb exb ex><  8 lshift or ;
: exl ( -- long )
    exw exw ex>< 16 lshift or ;

: ?exif-jpeg ( -- )
    6 exif-read "Exif\0\0" str= 0= IF  !!no-exif!! throw  THEN ;

: exif-start! ( -- )
    ex-seek? exif-start ! ;
: ?exif ( -- )
    exif-start!
    8 exif-read 2dup "II*\0\10\0\0\0" str= IF
	2drop exif-endian off  EXIT  THEN
    "MM\0*\0\0\0\10" str= IF
	exif-endian on  EXIT  THEN
    !!no-exif!! throw ;

\ read and print exif information

Create exif-sizes 0 c, 1 c, 1 c, 2 c, 4 c, 8 c, 1 c, 1 c, 2 c, 4 c, 7 c, 4 c, 8 c,
DOES> + c@ ;

: .exif-tag ( -- )
    exw exw exl exl { cmd type len offset }
    cmd h. type h. len h.
    type exif-sizes len * { size }
    size 4 > IF
	cr offset size exif-read-at $100 umin dump
    ELSE
	offset h. cr
    THEN ;

: .exif-tags ( -- )
    exw 0 ?DO  .exif-tag  LOOP ;

: .exifs ( -- )
    .exif-tags exl exif-seek .exif-tags ;

\ search for thumbnail image

debug: exif( \ )

: >thumb ( -- )
    exw 0 ?DO
	exw exw exl exl { cmd typ len offset }
	exif( offset len typ cmd [: cr h. h. h. h. ;] do-debug )
	offset
	case typ
	    #1    of  $18 rshift      endof
	    #3    of  $10 rshift      endof
	    #6    of  $18 rshift c>s  endof
	    #8    of  $10 rshift w>s  endof
	    #9    of             l>s  endof
	endcase
	case cmd \ len ~~ drop
	    $100  of  img-w      !  endof
	    $101  of  img-h      !  endof
	    $112  of  img-orient !  endof
	    $13b  of  len exif-slurp over >r artist $!
		r> free throw  endof
	    $201  of  thumb-off  !  endof
	    $202  of  thumb-len  !  endof
	    $8769 of  exif-idf   !  endof
	    $8825 of  exif-gps   !  endof
	    $8298 of  len exif-slurp over >r ©-notice $!
		r> free throw  endof
	    $A005 of  intop-idf  !  endof
	    $FFFF of  drop LEAVE    endof
	    nip
	endcase
    LOOP ;

: thumbnail@ ( -- addr u )
    thumb-off @ thumb-len @ dup IF  exif-slurp  THEN ;
: exif-init ( -- )
    img-orient off  thumb-off off  thumb-len off
    exif-idf off  intop-idf off  exif-gps off ;
: suffix ( addr u -- addr' u' )
    2dup '.' scan-back nip /string ;
: jpeg? ( addr u -- flag )
    suffix 2dup "JPG" capscompare 0= >r "JPEG" capscompare 0= r> or ;
: webp? ( addr u -- flag )
    suffix "WEBP" capscompare 0= ;
: >exif ( addr u -- flag )
    2dup 2>r
    >exif-open
    2r@ jpeg?
    IF	>exif-st.jpeg
	?exif-jpeg
    ELSE
	2r@ webp?
	IF    >exif-st.webp
	ELSE  false  THEN
    THEN  2rdrop ;
: >thumb-scan ( fn-addr u1 -- )
    exif-init >exif
    IF
	?exif >thumb  exl exif-seek >thumb
    THEN ;

: exif-close ( -- )
    jpeg-fd @ ?dup-IF   close-file jpeg-fd off throw
    ELSE  exif-o @ .dispose  file-exif  THEN ;
: >thumbnail ( fn-addr u1 -- jpeg-addr u2 )
    >thumb-scan thumbnail@ exif-close ;
