%module egl
%insert("include")
%{
#include <EGL/egl.h>
%}
%apply int { EGLint };
%apply SWIGTYPE * { EGLNativeDisplayType, EGLNativeWindowType, EGLNativePixmapType };

#define __ANDROID__
#define ANDROID
#define EGLAPI
#define EGLAPIENTRY

%include <EGL/egl.h>
