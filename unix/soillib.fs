\ soil wrapper

\ dummy load for Android
Vocabulary soil

get-current also soil definitions

c-library soillib
    e? os-type s" linux-android" string-prefix? [IF]
	s" libsoil.so" open-path-lib drop
    [THEN]
    
    s" soil" add-lib
    \c #include "SOIL.h"

    include soil.fs
end-c-library

previous set-current
