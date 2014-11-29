\ soil wrapper

\ dummy load for Android
[IFDEF] android
    s" /data/data/gnu.gforth/lib/libtypeset.so" open-lib drop
[THEN]

Vocabulary freetype-gl

get-current also freetype-gl definitions

c-library freetype-gllib
    [IFDEF] android
	s" freetype-gl" add-lib
    [ELSE]
	s" typeset" add-lib
    [THEN]
    \c #include "freetype-gl.h"
    \c #include "vec234.h"
    \c #include "vector.h"
    \c #include "texture-atlas.h"
    \c #include "texture-font.h"

    include freetype-gl.fs
end-c-library

previous set-current
