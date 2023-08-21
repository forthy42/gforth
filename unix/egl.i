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

#define SWIG_FORTH_GFORTH_LIBRARY "EGL"
#define SWIG_FORTH_OPTIONS "no-callbacks"

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
#define __ANDROID__
#define ANDROID
#define EGLAPI
#define EGLAPIENTRY
#define EGLAPIENTRYP void *
%apply SWIGTYPE * { EGLNativeDisplayType, EGLNativeWindowType, EGLNativePixmapType };
#endif
#if defined(host_os_linux_gnu) || defined(host_os_linux_gnueabi) || defined(host_os_linux_gnueabihf) || defined(host_os_linux_gnux32)
#define __unix__
#define EGLAPI
#define EGLAPIENTRY
#define EGLAPIENTRYP *
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif
#ifdef host_os_darwin
#define __unix__
#define EGLAPI
#define EGLAPIENTRY
#define EGLAPIENTRYP *
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif
#ifdef host_os_cygwin
#define EGLAPI
#define EGLAPIENTRY
#define EGLAPIENTRYP *
%apply SWIGTYPE * { EGLNativeDisplayType };
%apply long { EGLNativeWindowType, EGLNativePixmapType };
#endif

// exec: sed -e 's/^c-library\( .*\)/get-current also opengl definitions``c-library\1`/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'

%include <EGL/egl.h>
