// this file is in the public domain
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

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
#define __ANDROID__
#define ANDROID
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType, EGLNativeWindowType, EGLNativePixmapType };
#endif
#if defined(host_os_linux_gnu) || defined(host_os_linux_gnueabi) || defined(host_os_linux_gnueabihf)
#define __unix__
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif
#ifdef host_os_darwin
#define __unix__
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif
#ifdef host_os_cygwin
#define EGLAPI
#define EGLAPIENTRY
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif

%include <EGL/egl.h>
