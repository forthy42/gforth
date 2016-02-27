// this file is in the public domain
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

// exec: sed -e 's/ \(ws\|s\) \([nu]\) / a \2 /g' -e 's/XStoreNamedColor a u a u n/XStoreNamedColor a u s u n/g' -e 's/XInternAtom a a n/XInternAtom a s n/g' -e 's/XListFonts a a n a/XListFonts a s n a/g' -e 's/XListFontsWithInfo a a n a a/XListFontsWithInfo a s n a a/g' -e 's/XGeometry a n s a/XGeometry a n s s/g' -e 's/XStoreNamedColor a u a u n/XStoreNamedColor a u s u n/g' -e 's/XOpenDisplay a/XOpenDisplay s/g'

%include <X11/X.h>
%include <X11/Xlib.h>
%include <X11/Xutil.h>
