\ include xlib stuff

Vocabulary X11
get-current also X11 definitions

c-library xlib
    \c #include <X11/X.h>
    \c #include <X11/Xlib.h>
    \c #include <X11/Xutil.h>

    s" X11" add-lib

    include xlib.fs

end-c-library

previous set-current
