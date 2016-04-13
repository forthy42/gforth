\ include xlib stuff

Vocabulary X11
get-current also X11 definitions

c-library x
    \c #include <X11/X.h>
    \c #include <X11/Xlib.h>
    \c #include <X11/Xutil.h>

    s" X11" add-lib

    0 warnings !@
    include unix/xlib.fs

    \ several vararg functions have to be declared by hand
    c-function XVaCreateNestedList_2 XVaCreateNestedList n a a a a 0 -- a
    c-function XCreateIC_2 XCreateIC a a a a a 0 -- a
    c-function XCreateIC_3 XCreateIC a a a a a a a 0 -- a
    c-function XSetICValues_2 XSetICValues a a a a a 0 -- a
    warnings !
end-c-library

previous set-current
