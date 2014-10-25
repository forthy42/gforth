%module gles
%insert("include")
%{
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
%}
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType };
%apply SWIGTYPE * { GLintptr, GLsizeiptr, EGLBoolean };

#define __ANDROID__
#define GL_APICALL
#define GL_APIENTRY

%include <GLES2/gl2.h>
%include <GLES2/gl2ext.h>
