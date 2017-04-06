\ png library

e? os-type s" linux-android" string-prefix? [IF]
    s" libpng.so" also c-lib open-path-lib drop previous
[THEN]

Vocabulary pnglib
get-current also pnglib definitions

c-library png
    e? os-type s" linux-android" string-prefix? [IF]
	s" png" add-lib
	\c #include <zlib.h>
	\c #include "../../../../libpng/pngconf.h"
	\c #include "../../../../libpng/png.h"
    [ELSE]
	e? os-type s" linux" string-prefix? [IF]
	    s" unix/pnglib16.fs" open-fpath-file 0= [IF]
		2drop close-file throw
		: png16 ;
	    [THEN]
	    [IFDEF] png16
		s" png16" add-lib
		\c #define PNG_STDIO_SUPPORTED 1
		\c #define PNG_ERROR_TEXT_SUPPORTED 1
		\c #include <zlib.h>
		\c #include <libpng16/pngconf.h>
		\c #include <libpng16/png.h>
	    [ELSE]
		s" png12" add-lib
		\c #include <zlib.h>
		\c #include <libpng12/pngconf.h>
		\c #include <libpng12/png.h>
	    [THEN]
	[THEN]
    [THEN]
    [IFDEF] png16
	include unix/pnglib16.fs
    [ELSE]
	include unix/pnglib.fs
    [THEN]
end-c-library

previous set-current