\ soil texture

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019,2020,2021 Free Software Foundation, Inc.

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

require gl-helper.fs
require unix/stb-image.fs
"unix/webp.fs" ' file>fpath catch nip nip 0= [IF]
    require unix/webp.fs
[THEN]
require jpeg-exif.fs

\ also soil

: >texture ( addr w h -- )
    third >r rgba-texture mipmap linear-mipmap r> free throw ;
: mem-webp? ( addr u -- flag )
    8 safe/string "WEBP" string-prefix? ;
: img>mem ( addr u -- memimg w h )
    { | w^ w w^ h w^ ch# }
    [IFDEF] webp [ also webp ]
	2dup mem-webp?
	IF
	    w h WebPDecodeRGBA
	ELSE
	    w h ch# 4 stbi_load_from_memory
	THEN [ previous ]
    [ELSE]
	w h ch# 4 stbi_load_from_memory
    [THEN]
    w @ h @ ;
: mem>texture ( addr u -- w h )
    over >r img>mem r> free throw 2dup 2>r >texture 2r> ;
: load-texture ( addr u -- w h )
    open-fpath-file throw 2drop slurp-fid mem>texture ;
: >subtex ( addr x y w h -- )
    4 pick >r rgba-subtex wrap-texture mipmap linear-mipmap r> free throw ;
: mem>subtex ( x y addr u -- w h )
    over >r img>mem r> free throw 2>r -rot 2r@ >subtex 2r> ;
: load-subtex ( x y addr u -- w h )
    open-fpath-file throw 2drop slurp-fid mem>subtex ;

tex: thumbnails

: load-thumb ( addr u -- w h )
    >thumbnail mem>texture ;
: load-subthumb ( x y addr u -- w h )
    >thumbnail mem>subtex ;

\ previous
