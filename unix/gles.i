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
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType, GLintptr, GLsizeiptr };
%apply int { GLfixed };
%apply SWIGTYPE * { EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define SWIG_FORTH_GFORTH_LIBRARY "GLESv2"

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
#define __ANDROID__
#define ANDROID
#endif
#define GL_APICALL
#define GL_APIENTRY

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary opengl\nget-current >r also opengl definitions\n\nc-library\1\n/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g'

%include <GLES2/gl2.h>
%include <GLES2/gl2ext.h>
