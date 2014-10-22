%module gl
%insert("include")
%{
#define GL_GLEXT_PROTOTYPES
#include <GL/gl.h>
#include <GL/glext.h>
%}
#define GL_GLEXT_PROTOTYPES
#define __STDC__

%apply long long { GLint64, GLint64EXT, GLuint64, GLuint64EXT };
%apply SWIGTYPE * { GLsizeiptr, GLintptr, GLsizeiptrARB, GLintptrARB };

%include <GL/gl.h>
%include <GL/glext.h>
