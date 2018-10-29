\ soil wrapper

Vocabulary freetype-gl

get-current also freetype-gl definitions

e? os-type s" linux-android" string-prefix? [IF]
    s" libtypeset.so" also c-lib open-path-lib drop previous
[THEN]

include unix/freetype_gl.fs

previous set-current
