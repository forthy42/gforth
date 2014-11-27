\ soil wrapper

\ dummy load for Android
s" /data/data/gnu.gforth/lib/libfreetype-gl.so" open-lib drop

Vocabulary freetype-gl

get-current also freetype-gl definitions

c-library freetype-gllib
    s" freetype-gl" add-lib
    \c #include "freetype-gl.h"
    \c #include "vec234.h"
    \c #include "vector.h"
    \c #include "texture-atlas.h"
    \c #include "texture-font.h"

    include freetype-gl.fs
end-c-library

previous set-current
