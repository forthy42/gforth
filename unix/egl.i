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

#ifdef host_os_linux_android
#define __ANDROID__
#define ANDROID
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType, EGLNativeWindowType, EGLNativePixmapType };
#endif
#ifdef host_os_linux_gnu
#define __unix__
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif
#ifdef host_os_darwin14.0.0
#define __unix__
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif

%include <EGL/egl.h>
