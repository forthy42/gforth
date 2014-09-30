%module gles
%insert("include")
%{
#include <GLES2/gl2.h>
%}
%apply float { GLfloat, GLclampf };
%apply SWIGTYPE * { GLintptr, GLsizeiptr };

#define __ANDROID__
#define GL_APICALL
#define GL_APIENTRY

%include <GLES2/gl2.h>
