\ Convert image to C include file

0 Value image
0 Value bitmap

Create magicbuf 8 allot

: search-magic ( fd -- )  >r
    BEGIN  magicbuf 8 r@ read-file throw  8 =  WHILE
	magicbuf s" Gforth1" tuck compare 0=  UNTIL
    ELSE  true abort" No magic found"  THEN
    rdrop ;

Create image-header  4 cells allot
Variable image-cells
Variable bitmap-chars

: read-header ( fd -- )
    image-header 4 cells rot read-file throw drop
    image-header 2 cells + @ dup cell / image-cells ! 1- 8 cells / 1+ bitmap-chars !
    image-cells @ cells allocate throw to image
    bitmap-chars @ allocate throw to bitmap ;

: read-dictionary ( fd -- )  >r
    image image-cells @ cells r> read-file throw drop ;

: read-bitmap ( fd -- )  >r
    bitmap bitmap-chars @ r> read-file throw drop ;

: .08x ( n -- ) 0 <# # # # # # # # # 'x hold '0 hold #> type ;
: .02x ( n -- ) 0 <# # # 'x hold '0 hold #> type ;

: .image ( -- )
    image-cells @ 0 ?DO
	I 4 + I' min I ?DO  space image I cells + @ .08x ." ," LOOP cr
	4 +LOOP ;

: .reloc ( -- )
    bitmap-chars @ 0 ?DO
	I 8 + I' min I ?DO  space bitmap I + c@ .02x ." ," LOOP cr
	8 +LOOP ;

: read-image ( addr u -- )
    r/o bin open-file throw >r
    r@ search-magic
    r@ file-position throw r@ read-header r@ reposition-file throw
    r@ read-dictionary r@ read-bitmap r> close-file throw ;

: .imagesize ( -- )
    image-header 3 cells + @ 1 cells / .08x ;

: .relocsize ( -- )
    bitmap-chars @ .08x ;

: fi2c ( addr u -- )  base @ >r hex
    read-image
    ." #include " '" emit ." forth.h" '" emit cr
    ." Cell image[" .imagesize ." ] = {" cr .image ." };" cr
    ." const char reloc_bits[" .relocsize ." ] = {" cr .reloc ." };" cr
    r> base ! ;

