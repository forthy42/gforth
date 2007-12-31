\ Compare nonrelocatable images and produce a relocatable image

\ Copyright (C) 1996,1997,1998,2002,2003,2004,2007 Free Software Foundation, Inc.

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

s" address-unit-bits" environment? drop constant bits/au
7 constant dodoes-tag

: write-cell { w^ w  file-id -- ior }
    \ write a cell to the file
    w cell file-id write-file ;

: th ( addr1 n -- addr2 )
    cells + ;

: bset ( bmask c-addr -- )
    tuck c@ or swap c! ; 

: set-bit { u addr -- }
    \ set bit u in bit-vector addr
    u bits/au /mod
    >r 1 bits/au 1- rot - lshift
    r> addr +  bset ;

: image-data { image1 image2 i-field expected-offset -- base offset }
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
    dup dodoes-tag 2 + > IF
	$21 1 DO  dup tag-offsets I cells + @ < IF
		tag-offsets I 1- cells + @ - I 1- 9 lshift + negate
		UNLOOP  EXIT  THEN  LOOP
    THEN  -2 swap - ;

: compare-images { image1 image2 reloc-bits size file-id -- }
    \G compares image1 and image2 (of size cells) and sets reloc-bits.
    \G offset is the difference for relocated addresses
    \ this definition is certainly to long and too complex, but is
    \ hard to factor.
    image1 @ image2 @ over - { dbase doffset }
    doffset 0= abort" images have the same dictionary base address"
    ." data offset=" doffset . cr
    ." code" image1 image2 cell     26 cells image-data { cbase coffset }
    ."   xt" image1 image2 11 cells 22 cells image-data { xbase xoffset }
    size 0
    u+do
	image1 i th @ image2 i th @ { cell1 cell2 }
	cell1 doffset + cell2 =
	if
	    cell1 dbase - file-id write-cell throw
	    i reloc-bits set-bit
	else
	    coffset 0<> cell1 coffset + cell2 = and
	    if
		cell1 cbase - cell / { tag }
		tag dodoes-tag =
		if
		    \ make sure that the next cell will not be tagged
		    \ !! can probably be optimized away with hybrid threading
		    dbase negate image1 i 1+ th +!
		    dbase doffset + negate image2 i 1+ th +!
		endif
		tag >tag $4000 xor file-id write-cell throw
		i reloc-bits set-bit
	    else
		xoffset 0<> cell1 xoffset + cell2 = and
		if
		    cell1 xbase - cell / { tag }
		    tag dodoes-tag =
		    if
			\ make sure that the next cell will not be tagged
			\ !! can probably be optimized away with hybrid threading
			dbase negate image1 i 1+ th +!
			dbase doffset + negate image2 i 1+ th +!
		    endif
		    tag >tag file-id write-cell throw
		    i reloc-bits set-bit
		else
		    cell1 file-id write-cell throw
		    cell1 cell2 <>
		    if
			0 i th 9 u.r cell1 17 u.r cell2 17 u.r cr
		    endif
		endif
	    endif
	endif
    loop ;

: comp-image ( "image-file1" "image-file2" "new-image" -- )
    name slurp-file { image1 size1 }
    image1 size1 s" Gforth3" search 0= abort" not a Gforth image"
    drop 8 + image1 - { header-offset }
    size1 aligned size1 <> abort" unaligned image size"
    image1 header-offset + 2 cells + @ header-offset + size1 <> abort" header gives wrong size"
    name slurp-file { image2 size2 }
    size1 size2 <> abort" image sizes differ"
    name ( "new-image" ) w/o bin create-file throw { outfile }
    size1 header-offset - 1- cell / bits/au / 1+ { reloc-size }
    reloc-size allocate throw { reloc-bits }
    reloc-bits reloc-size erase
    image1 header-offset outfile write-file throw
    base @ hex
    image1 header-offset +  image2 header-offset +  reloc-bits
    size1 header-offset - aligned cell /  outfile  compare-images
    base !
    reloc-bits reloc-size outfile write-file throw
    outfile close-file throw ;

    
