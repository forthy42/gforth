// this file is in the public domain
%module gles
%insert("include")
%{
#include <OpenGLES/ES2/gl.h>
#include <OpenGLES/ES2/glext.h>
%}
%apply void { GLvoid };
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType, GLintptr, GLsizeiptr };
%apply SWIGTYPE * { EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define GL_APICALL
#define GL_APIENTRY
#define __OSX_AVAILABLE_STARTING(x, y)

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary opengl``get-current also opengl definitions``c-library\1`/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'

%include <OpenGLES.framework/Headers/ES2/gl.h>
%include <OpenGLES.framework/Headers/ES2/glext.h>
