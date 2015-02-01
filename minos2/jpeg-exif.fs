\ a simple jpeg parser to read important EXIF stuff

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
    BEGIN  read-tag dup $D9 = IF  drop 0  EXIT  THEN
	$E1 <>  WHILE  read-len jpeg+seek  REPEAT
    read-len ;

: >exif ( addr u -- )
    r/o open-file throw to jpeg-fd ?soi search-exif
    jpeg-fd file-position throw drop + to exif-end ;

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

: exw ( -- word )
    jpeg-fd key-file  jpeg-fd key-file
    exif-endian IF  swap  THEN  8 lshift or ;

: exl ( -- long )
    exw exw    exif-endian IF  swap  THEN  16 lshift or ;

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

: >thumb ( -- )
    exw 0 ?DO
	exw exw exl exl { cmd type len offset }
	cmd $201 = IF  offset to thumb-off  THEN
	cmd $202 = IF  offset to thumb-len  THEN
    LOOP ;

: >thumbnail ( fn-addr u1 -- jpeg-addr u2 )
    >exif ?exif exw 12 * jpeg+seek exl exif-seek >thumb
    thumb-len thumb-off exif-slurp
    jpeg-fd close-file throw 0 to jpeg-fd ;
