\ Compare nonrelocatable images and produce a relocatable image

\ Copyright (C) 1996 Free Software Foundation, Inc.

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

s" address-unit-bits" environment? drop constant bits/au

: write-cell { w^ w  file-id -- ior }
    \ write a cell to the file
    w cell file-id write-file ;

: th ( addr1 n -- addr2 )
    cells + ;

: set-bit { u addr -- }
    \ set bit u in bit-vector addr
    u bits/au /mod
    >r 1 bits/au 1- rot - lshift
    r> addr +  cset ;

: compare-images { image1 image2 reloc-bits size file-id -- }
    \ compares image1 and image2 (of size cells) and sets reloc-bits.
    \ offset is the difference for relocated addresses
    image1 @ image2 @ over - { base offset }
    offset 0= abort" images have the same base"
    ." offset=" offset . cr
    size 0
    u+do
	image1 i th @ image2 i th @ { cell1 cell2 }
	cell1 offset + cell2 = if
	    cell1 base - file-id write-cell throw
	    i reloc-bits set-bit
	else
	    cell1 file-id write-cell throw
	    cell1 cell2 <> if
		0 i th 9 u.r cell1 17 u.r cell2 17 u.r cr
	    endif
	endif
    loop ;

: slurp-file ( c-addr1 u1 -- c-addr2 u2 )
    \ c-addr1 u1 is the filename, c-addr2 u2 is the file's contents
    r/o bin open-file throw >r
    here $7fffffff r@ read-file throw
    r> close-file throw
    here swap
    dup allot ;

: comp-image ( "image-file1" "image-file2" "new-image" -- )
    name { d: image-file1 }
    name { d: image-file2 }
    name { d: new-image }
    maxalign image-file1 slurp-file { image1 size1 }
    maxalign image-file2 slurp-file { image2 size2 }
    image1 size1 s" Gforth1" search 0= abort" not a Gforth image"
    drop 8 + image1 - { header-offset }
    size1 size2 <> abort" image sizes differ"
    size1 aligned size1 <> abort" unaligned image size"
    size1 image1 header-offset + 2 cells + @ header-offset + <> abort" header gives wrong size"
    new-image w/o bin create-file throw { outfile }
    size1 header-offset - 1- cell / bits/au / 1+ { reloc-size }
    maxalign here { reloc-bits }
    reloc-size allot
    reloc-bits reloc-size erase
    image1 header-offset outfile write-file throw
    base @ hex
    image1 header-offset +  image2 header-offset +  reloc-bits
    size1 header-offset - aligned cell /  outfile  compare-images
    base !
    reloc-bits reloc-size outfile write-file throw
    outfile close-file throw ;

    
