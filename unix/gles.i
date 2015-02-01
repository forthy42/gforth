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
%apply long { EGLNativePixmapType };
%apply SWIGTYPE * { GLintptr, GLsizeiptr, EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define __ANDROID__
#define GL_APICALL
#define GL_APIENTRY

%include <GLES2/gl2.h>
%include <GLES2/gl2ext.h>
