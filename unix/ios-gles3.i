// this file is in the public domain
%module gles
%insert("include")
%{
#include <OpenGLES/ES3/gl.h>
#include <OpenGLES/ES3/glext.h>
%}
%apply void { GLvoid };
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType, GLintptr, GLsizeiptr };
%apply SWIGTYPE * { EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define GL_APICALL
#define GL_APIENTRY
#define __OSX_AVAILABLE_STARTING(x, y)

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary opengl\nget-current >r also opengl definitions\n\nc-library\1\n/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g'

%include <OpenGLES.framework/Headers/ES3/gl.h>
%include <OpenGLES.framework/Headers/ES3/glext.h>
