// this file is in the public domain
%module gl
%insert("include")
%{
#define GL_GLEXT_PROTOTYPES
#include <stdint.h>
#include <OpenGL/gl.h>
#include <OpenGL/glext.h>
%}
#define GL_GLEXT_PROTOTYPES
#define __STDC__
#define SWIG_FORTH_OPTIONS "no-callbacks"
#define WINGDIAPI
#define APIENTRY

%apply long long { GLint64, GLint64EXT, GLuint64, GLuint64EXT };
%apply long { GLsizeiptr, GLintptr, GLsizeiptrARB, GLintptrARB };
%apply char { GLchar }

// exec: sed -e 's/\(c-function .*\(NV\|SUN\|IBM\|ATI\|AMD\|SUN\|SGI\|MESA\|INTEL\|HP\|GREMEDY\|APPLE\|OES\|3DFX\|ARB\|INGR\)\)/\\ \1/g'

%include <OpenGL/gl.h>
%include <OpenGL/glext.h>
