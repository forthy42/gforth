\ png library

e? os-type s" linux-android" string-prefix? [IF]
    s" libpng.so" also c-lib open-path-lib drop previous
[THEN]

Vocabulary pnglib
get-current also pnglib definitions

c-library png
    s" unix/pnglib16.fs" open-fpath-file 0= [IF]
	2drop close-file throw
	: png16 ;
    [THEN]
    \c #include <zlib.h>
    [IFDEF] png16
	s" png16" add-lib
	\c #define PNG_STDIO_SUPPORTED 1
	\c #define PNG_ERROR_TEXT_SUPPORTED 1
	\c #include <libpng16/pngconf.h>
	\c #include <libpng16/png.h>
	include unix/pnglib16.fs
    [ELSE]
	s" png12" add-lib
	\c #define PNG_SKIP_SETJMP_CHECK 1
	\c #include <libpng12/pngconf.h>
	\c #include <libpng12/png.h>
	include unix/pnglib.fs
    [THEN]
end-c-library

previous set-current
