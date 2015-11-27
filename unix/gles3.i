// this file is in the public domain
%module gles
%insert("include")
%{
#include <GLES3/gl3.h>
#include <GLES3/gl3ext.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}
#define const
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType }
%apply long long { GLuint64 };
%apply SWIGTYPE * { GLintptr, GLsizeiptr, EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define __ANDROID__
#define GL_APICALL
#define GL_APIENTRY

%include <GLES3/gl3.h>
%include <GLES3/gl3ext.h>
