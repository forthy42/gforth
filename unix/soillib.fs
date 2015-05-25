\ soil wrapper

\ dummy load for Android
e? os-type s" linux-android" string-prefix? [IF]
    s" libsoil.so" open-path-lib drop
[THEN]

Vocabulary soil

get-current also soil definitions

c-library soillib
    s" soil" add-lib
    \c #include "SOIL.h"

    include soil.fs
end-c-library

previous set-current
