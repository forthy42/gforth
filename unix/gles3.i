%module gles
%insert("include")
%{
#include <GLES3/gl3.h>
#include <GLES3/gl3ext.h>
%}
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType };
%apply SWIGTYPE * { GLintptr, GLsizeiptr, EGLBoolean };

#define __ANDROID__
#define GL_APICALL
#define GL_APIENTRY

%include <GLES3/gl3.h>
%include <GLES3/gl3ext.h>
