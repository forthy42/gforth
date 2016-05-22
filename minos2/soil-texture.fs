\ soil texture

\ Copyright (C) 2014 Free Software Foundation, Inc.

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
s" unix/soil2.fs" open-fpath-file 0= [IF]
    \ prefer soil2 over soil
    2drop close-file throw
    require unix/soil2lib.fs
[ELSE]
    require unix/soillib.fs
[THEN]
require jpeg-exif.fs

also soil

: >texture ( addr w h -- )
    2 pick >r rgba-texture wrap mipmap nearest r> free throw ;
: mem>texture ( addr u -- w h )
    over >r  0 0 0 { w^ w w^ h w^ ch# }
    w h ch# SOIL_LOAD_RGBA SOIL_load_image_from_memory
    r> free throw w @ h @  2dup 2>r >texture 2r> ;
: load-texture ( addr u -- w h )
    open-fpath-file throw 2drop slurp-fid mem>texture ;
: >subtex ( addr x y w h -- )
    4 pick >r rgba-subtex wrap mipmap nearest r> free throw ;
: mem>subtex ( x y addr u -- w h )
    over >r  0 0 0 { w^ w w^ h w^ ch# }
    w h ch# SOIL_LOAD_RGBA SOIL_load_image_from_memory
    r> free throw -rot w @ h @  2dup 2>r >subtex 2r> ;
: load-subtex ( x y addr u -- w h )
    open-fpath-file throw 2drop slurp-fid mem>subtex ;

tex: thumbnails

: load-thumb ( addr u -- w h )
    >thumbnail mem>texture ;
: load-subthumb ( x y addr u -- w h )
    >thumbnail mem>subtex ;

previous
