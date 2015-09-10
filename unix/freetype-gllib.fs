\ soil wrapper

Vocabulary freetype-gl

get-current also freetype-gl definitions

e? os-type s" linux-android" string-prefix? [IF]
    s" libtypeset.so" open-path-lib drop
[THEN]

c-library freetype-gllib
    e? os-type s" linux-android" string-prefix? [IF]
	s" typeset" add-lib
    [ELSE]
	s" freetype-gl" add-lib
    [THEN]
    \c #include "freetype-gl.h"
    \c #include "vec234.h"
    \c #include "vector.h"
    \c #include "texture-atlas.h"
    \c #include "texture-font.h"

    include freetype-gl.fs
end-c-library

previous set-current
