// this file is in the public domain
%module gles
%insert("include")
%{
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}
#define const
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType, GLintptr, GLsizeiptr };
%apply SWIGTYPE * { EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
#define __ANDROID__
#define ANDROID
#endif
#define GL_APICALL
#define GL_APIENTRY

%include <GLES2/gl2.h>
%include <GLES2/gl2ext.h>
