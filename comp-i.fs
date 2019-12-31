\ Compare nonrelocatable images and produce a relocatable image

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1996,1997,1998,2002,2003,2004,2007,2010,2012,2013,2015,2016,2017,2019 Free Software Foundation, Inc.

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

require sections.fs

s" address-unit-bits" environment? drop constant bits/au
9 constant maxdoer-tag

0 value image1
0 value size1
0 value image2
0 value size2
0 value reloc-bits
0 value reloc-size
Variable im-sects1 \ image sections
Variable im-sects2

0
field: sect-start
field: sect-size
field: sect-dp
drop

: write-cell { w^ w  file-id -- ior }
    \ write a cell to the file
    w cell file-id write-file ;

: bset ( bmask c-addr -- )
    tuck c@ or swap c! ; 

: set-bit { u addr -- }
    \ set bit u in bit-vector addr
    u bits/au /mod
    >r 1 bits/au 1- rot - lshift
    r> addr +  bset ;

: image-data { i-field expected-offset -- base offset }
    image1 i-field + @ image2 i-field + @ over - { base offset }
    offset 0=
    if
	." : images have the same base address; producing only a data-relocatable image" cr
    else
	\ the following sanity check produces false positices with exec-shield
	\ offset abs expected-offset <> abort" images produced by different engines"
	."  offset=" offset . cr
	image1 i-field + off  image2 i-field + off
    endif
    base offset ;

: >tag ( index -- tag )
    dup maxdoer-tag > IF
	$21 1 DO  dup tag-offsets I cells + @ < IF
		tag-offsets I 1- cells + @ - I 1- 9 lshift + negate
		UNLOOP  EXIT  THEN  LOOP
    THEN  -2 swap - ;

: sect-size@ ( sect -- size )
    dup sect-dp @ swap sect-start @ - ;

: sect-reloc {: x1 sects -- x2 :}
    \ check if x1 is in any of the sections decribed by sects; if so,
    \ x2 is the relocated x1, otherwise x2=x1; there may be addresses
    \ at the end of a section, so we include that in our section check
    \ (the "1+" below).
    sects stack# 0 u+do
	i sects $[] @ { sect }
	x1 sect sect-start @ sect sect-dp @ 1+ within if
	    x1 sect sect-start @ -
	    i bits/au cell 1- * lshift or unloop exit then
    loop
    x1 ;

: write-symbol { acell mask file-id u -- }
    \ Writes ACELL, which refers to some engine symbol (code address,
    \ xt, or label) and does the appropriate tagging.  MASK provides
    \ additional tagging information for this symbol, FILE-ID is the
    \ image-file and U is the cell index  in the image file.
    acell cell/ >tag mask xor file-id write-cell throw
    u reloc-bits set-bit ;

0 Value cbase  0 Value coffset
0 Value xbase  0 Value xoffset
0 Value lbase  0 Value loffset

: set-image-offsets ( -- )
    ."  code" 14 cells 26 cells image-data to coffset to cbase
    ."    xt" 15 cells 22 cells image-data to xoffset to xbase
    ." label" 16 cells 18 cells image-data to loffset to lbase ;

: alloc-reloc-bits ( size -- )
    reloc-bits ?dup-IF  free throw  THEN
    1- bits/au / 1+ to reloc-size
    reloc-size allocate throw to reloc-bits
    reloc-bits reloc-size erase ;

: compare-section { sect1 sect2 size file-id -- }
    \G compares sect1 and sect2 (of size cells) and sets reloc-bits.
    \G offset is the difference for relocated addresses
    \ this definition is certainly to long and too complex, but is
    \ hard to factor.
    size alloc-reloc-bits
    size 0 u+do
	sect1 i th @ sect2 i th @ { cell1 cell2 }
	case 
	    cell1 cell2 = ?of
		cell1 file-id write-cell throw endof
	    cell1 im-sects1 sect-reloc cell2 im-sects2 sect-reloc over = ?of
		file-id write-cell throw
		i reloc-bits set-bit endof
	    drop
	    cell1 coffset + cell2 = ?of
		cell1 cbase - $4000 file-id i write-symbol endof
	    cell1 xoffset + cell2 = ?of
		cell1 xbase -     0 file-id i write-symbol endof
	    cell1 loffset + cell2 = ?of
		cell1 lbase - $8000 file-id i write-symbol endof
	    cell1 file-id write-cell throw
	    cell1 cell2 <> if
		0 i th 9 u.r cell1 17 u.r cell2 17 u.r cr
	    endif
	0 endcase
    loop
    reloc-bits reloc-size file-id write-file throw ;

: compare-sections { file-id -- }
    im-sects1 stack# 0 DO
	I IF  s" Section." file-id write-file throw  THEN
	i im-sects1 $[] @
	i im-sects2 $[] @ dup sect-size@ aligned cell/
	file-id compare-section
    LOOP ;

: image-sections { image size sects -- }
    \ process the sections (in particular, compute offsets)
    ."            start      size        dp" cr
    image size bounds U+DO
	I sect-start @ #16 hex.r
	I sect-size  @ #10 hex.r
	I sect-size@   #10 hex.r cr
	I sects >stack
	I sect-size@ ?dup-0=-IF  LEAVE  THEN
	dup I + s" Section." tuck str= 0= ?LEAVE
    8 +  +LOOP ;

: check-sections ( -- )
    im-sects1 stack# 0 ?do
	assert( i im-sects1 $[] @ sect-start @
	        i im-sects2 $[] @ sect-start @ <> )
	assert( i im-sects1 $[] @ sect-size @
	        i im-sects2 $[] @ sect-size @ = )
    loop ;	

: prepare-sections ( -- )
    image1 size1 im-sects1 image-sections
    image2 size2 im-sects2 image-sections
    im-sects1 stack# im-sects2 stack# <> abort" image misfit: #sections"
    check-sections ;

: comp-image ( "image-file1" "image-file2" "new-image" -- )
    name slurp-file { file1 fsize1 }
    file1 fsize1 s" Gforth6" search 0= abort" not a Gforth image"
    drop 8 + file1 - { header-offset }
    file1 fsize1 header-offset /string to size1 to image1
    size1 aligned size1 <> abort" unaligned image size"
    name slurp-file header-offset /string to size2 to image2
    size1 size2 <> abort" image sizes differ"
    set-image-offsets
    prepare-sections
    name ( "new-image" ) w/o bin create-file throw { outfile }
    file1 header-offset outfile write-file throw
    outfile ['] compare-sections $10 base-execute
    outfile close-file throw ;
