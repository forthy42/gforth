\ png library

\ we preload the library, because it is in our own path
s" /data/data/gnu.gforth/lib/libpng.so" open-lib drop

Vocabulary pnglib
get-current also pnglib definitions

c-library png
    [IFDEF] android
	s" ./libpng/.libs" add-libpath
	s" png" add-lib
	\c #include <zlib.h>
	\c #include "../../../../libpng/pngconf.h"
	\c #include "../../../../libpng/png.h"
    [ELSE]
	[IFDEF] linux
	    s" png12" add-lib
	    \c #include <zlib.h>
	    \c #include <libpng12/pngconf.h>
	    \c #include <libpng12/png.h>
	[THEN]
    [THEN]
    include pnglib.fs

end-c-library

previous set-current