// this file is in the public domain
%module gl
%insert("include")
%{
#define GL_GLEXT_PROTOTYPES
#include <stdint.h>
#include <GL/gl.h>
#include <GL/glext.h>
%}
#define GL_GLEXT_PROTOTYPES
#define __STDC__
#define SWIG_FORTH_OPTIONS "no-callbacks"
#define const

%apply long long { GLint64, GLint64EXT, GLuint64, GLuint64EXT };
%apply long { GLsizeiptr, GLintptr, GLsizeiptrARB, GLintptrARB };

%include <GL/gl.h>
%include <GL/glext.h>
