\ Compare nonrelocatable images and produce a relocatable image

\ Copyright (C) 1996,1997,1998,2002,2003,2004,2007,2010,2012,2013,2015 Free Software Foundation, Inc.

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
12 constant maxdoer-tag

0 value image1
0 value size1
0 value image2
0 value size2
0 value reloc-bits
0 value reloc-size
0 value im-sects1 \ image sections
0 value im-sects2
0 value #im-sects

synonym section-offset section-end
\ reused here: section-offset is the number you have to add to an
\ address that points into [section-start,section-dp] to get an offset
\ from forthstart in the current image
synonym section-end abort immediate
\ only use the new name

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
	0 image1 i-field + ! 0 image2 i-field + !
    endif
    base offset ;

: >tag ( index -- tag )
    dup maxdoer-tag > IF
	$21 1 DO  dup tag-offsets I cells + @ < IF
		tag-offsets I 1- cells + @ - I 1- 9 lshift + negate
		UNLOOP  EXIT  THEN  LOOP
    THEN  -2 swap - ;

: sect-reloc {: x1 sects -- x2 :}
    \ check if x1 is in any of the sections decribed by sects2; if so,
    \ x2 is the relocated x1, otherwise x2=x1; there may be addresses
    \ at the end of a section, so we include that in our section check
    \ (the "1+" below).
    sects #im-sects section-desc * bounds u+do
	x1 i section-start @ i section-dp @ 1+ within if
	    x1 i section-offset @ + unloop exit then
    section-desc +loop
    x1 ;

: compare-images { size file-id -- }
    \G compares image1 and image2 (of size cells) and sets reloc-bits.
    \G offset is the difference for relocated addresses
    \ this definition is certainly to long and too complex, but is
    \ hard to factor.
    cr ."  code" cell     26 cells image-data { cbase coffset }
    ."    xt" 13 cells 22 cells image-data { xbase xoffset }
    ." label" 14 cells 18 cells image-data { lbase loffset }
    size 0 u+do
	image1 i th @ image2 i th @ { cell1 cell2 }
	case 
	    cell1 cell2 = ?of
		cell1 file-id write-cell throw endof
	    cell1 im-sects1 sect-reloc cell2 im-sects2 sect-reloc over = ?of
		file-id write-cell throw
		i reloc-bits set-bit endof
	    drop
	    cell1 coffset + cell2 = ?of
		cell1 cbase - cell/ { tag }
		tag >tag $4000 xor file-id write-cell throw
		i reloc-bits set-bit endof
	    cell1 xoffset + cell2 = ?of
		cell1 xbase - cell/ { tag }
		tag >tag file-id write-cell throw
		i reloc-bits set-bit endof
	    cell1 loffset + cell2 = ?of
		cell1 lbase - cell/ { tag }
		tag >tag $8000 xor file-id write-cell throw
		i reloc-bits set-bit endof
	    cell1 file-id write-cell throw
	    cell1 cell2 <> if
		0 i th 9 u.r cell1 17 u.r cell2 17 u.r cr
	    endif
	0 endcase
    loop ;

: an.sections { sections u -- }
    cr sections #16 hex.r ."  start           offset               dp"
    u 0 u+do
        i section-desc * sections +
        dup section-start @ #21 hex.r
        dup section-end   @ #17 hex.r
        section-dp        @ #17 hex.r
    loop ;

: gen-section {: image -- im-sect :}
    \ generate a section for an old-style image (without sections)
    section-desc allocate throw {: sect :}
    image @ sect section-start !
    image @ negate sect section-offset !
    image 2 cells + @ image @ + sect section-dp !
    sect 1 an.sections
    sect ;

: old-image-format ( -- )
    image1 gen-section to im-sects1
    image2 gen-section to im-sects2
    1 to #im-sects ;
    
: process-sections { im-sects #im-sects image -- }
    \ im-sects #im-sects an.sections
    0 im-sects #im-sects section-desc * + im-sects u+do ( sect-offset )
	dup i section-start @ - i section-offset !
	i section-dp @ i section-start @ - +
    section-desc +loop
    assert( dup image 2 cells + @ = )
    im-sects #im-sects an.sections
    drop ;

: image-sections { image size -- im-sects #im-sects size' }
    \ process the sections (in particular, compute offsets)
    image size + cell- dup @ { #im-sects } ( addr )
    #im-sects section-desc * - { im-sects }
    im-sects image - { size' }
    assert( image 2 cells + @ size' = )
    assert( im-sects section-start @ image @ = )
    im-sects #im-sects image process-sections
    im-sects #im-sects size' ;

: check-sections ( -- )
    #im-sects section-desc * 0 +do
	assert( im-sects1 i + section-start @ im-sects2 i + section-start @ <> )
	assert( im-sects1 i + dup section-start @ swap section-offset @ +
	        im-sects2 i + dup section-start @ swap section-offset @ + = )
    section-desc +loop ;	

: new-image-format ( -- )
    image1 size1 image-sections to size1 to #im-sects to im-sects1
    image2 size2 image-sections to size2         swap to im-sects2
    #im-sects <> abort" image misfit: #sections"
    check-sections ;

: prepare-sections ( -- )
    image1 2 cells + @ size1 = if
	old-image-format
    else
	new-image-format
    then ;

: comp-image ( "image-file1" "image-file2" "new-image" -- )
    name slurp-file { file1 fsize1 }
    file1 fsize1 s" Gforth5" search 0= abort" not a Gforth image"
    drop 8 + file1 - { header-offset }
    file1 fsize1 header-offset /string to size1 to image1
    size1 aligned size1 <> abort" unaligned image size"
    name slurp-file header-offset /string to size2 to image2
    size1 size2 <> abort" image sizes differ"
    prepare-sections
    name ( "new-image" ) w/o bin create-file throw { outfile }
    size1 1- cell/ bits/au / 1+ to reloc-size
    reloc-size allocate throw to reloc-bits
    reloc-bits reloc-size erase
    file1 header-offset outfile write-file throw
    base @ hex
    size1 aligned cell/  outfile  compare-images
    base !
    reloc-bits reloc-size outfile write-file throw
    outfile close-file throw ;

    
