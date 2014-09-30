%module glext
%insert("include")
%{
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
%}

#define __ANDROID__
#define GL_APICALL
#define GL_APIENTRY

%include <GLES2/gl2ext.h>
