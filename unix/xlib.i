%module xlib
%insert("include")
%{
#include <X11/X.h>
#include <X11/Xlib.h>
#include <X11/Xutil.h>
%}

#define _XFUNCPROTOBEGIN
#define _XFUNCPROTOEND
#define _Xconst const
#define _X_DEPRECATED
#define _X_SENTINEL(x)
%apply short { wchar_t };

// exec: sed -e 's/\(c-function XKeycodeToKeysym\)/\\ \1/g'

%include <X11/X.h>
%include <X11/Xlib.h>
%include <X11/Xutil.h>
