// this file is in the public domain
%module gles
%insert("include")
%{
#include <OpenGLES/ES3/gl.h>
#include <OpenGLES/ES3/glext.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}
%apply void { GLvoid };
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType, GLintptr, GLsizeiptr };
%apply SWIGTYPE * { EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define GL_APICALL
#define GL_APIENTRY
#define __OSX_AVAILABLE_STARTING(x, y)

%include <OpenGLES.framework/Headers/ES3/gl.h>
%include <OpenGLES.framework/Headers/ES3/glext.h>
