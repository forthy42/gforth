\ soil wrapper

\ dummy load for Android
e? os-type s" linux-android" string-prefix? [IF]
    s" libtypeset.so" open-path-lib drop
[THEN]

Vocabulary freetype-gl

get-current also freetype-gl definitions

c-library freetype-gllib
    s" os-type" environment? [IF]
	s" linux-android" string-prefix? [IF]
	    s" typeset" add-lib
	[ELSE]
	    s" freetype-gl" add-lib
	[THEN]
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
