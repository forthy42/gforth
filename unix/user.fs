\ wrapper to load Swig-generated libraries

Voctable user32
get-current also user32 definitions

c-library user
    \c #include <w32api/wtypes.h>
    \c #include <w32api/winuser.h>
    s" user32" add-lib
    include winuser.fs
end-c-library

previous set-current
