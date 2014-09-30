%module gl
%insert("include")
%{
#define GL_GLEXT_PROTOTYPES
#include <GL/gl.h>
#include <GL/glext.h>
%}
#define GL_GLEXT_PROTOTYPES

%include <GL/gl.h>
%include <GL/glext.h>
