\ soil texture

require gl-helper.fs
s" unix/soil2.fs" open-fpath-file 0= [IF]
    \ prefer soil2 over soil
    2drop close-file throw
    require unix/soil2lib.fs
[ELSE]
    2drop drop
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