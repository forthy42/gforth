\ wrapper to load Swig-generated libraries

Voctable gdi32
get-current also gdi32 definitions

c-library gdi
    \c #include <w32api/wtypes.h>
    \c #include <w32api/wingdi.h>
    s" gdi32" add-lib
    s" opengl32" add-lib
    s" msimg32" add-lib
    s" winspool" add-lib
    include wingdi.fs
end-c-library

previous set-current
