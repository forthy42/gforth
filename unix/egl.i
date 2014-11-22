%module egl
%insert("include")
%{
#include <EGL/egl.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}
%apply int { EGLint };
%apply SWIGTYPE * { EGLNativeDisplayType, EGLNativeWindowType, EGLNativePixmapType };

#define __ANDROID__
#define ANDROID
#define EGLAPI
#define EGLAPIENTRY

%include <EGL/egl.h>
