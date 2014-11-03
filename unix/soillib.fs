\ soil wrapper

\ dummy load for Android
s" /data/data/gnu.gforth/lib/libsoil.so" open-lib drop

Vocabulary soil

get-current also soil definitions

c-library soillib
    s" soil" add-lib
    [IFDEF] android
	s" ./.libs" add-libpath
	\c #include "../../../../SOIL.h"
    [ELSE]
	\c #include "SOIL.h"
    [THEN]

    include soil.fs
end-c-library

previous set-current
