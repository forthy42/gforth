// this file is in the public domain
%module glwin
%insert("include")
%{
#define GL_GLEXT_PROTOTYPES
#include <stdint.h>
#include <w32api/GL/gl.h>
#include <w32api/GL/glext.h>
%}
#define GL_GLEXT_PROTOTYPES
#define __STDC__
#define SWIG_FORTH_OPTIONS "no-callbacks"
#define WINGDIAPI
#define APIENTRY

// exec: sed -e 's/\(c-function .*\(NV\|SUN\|IBM\|ATI\|AMD\|EXT\|SUN\|SGI\|MESA\|INTEL\|HP\|GREMEDY\|APPLE\|OES\|3DFX\|ARB\|INGR\)\)/\\ \1/g'

%apply long long { GLint64, GLint64EXT, GLuint64, GLuint64EXT };
%apply long { GLsizeiptr, GLintptr, GLsizeiptrARB, GLintptrARB };

%include <w32api/GL/gl.h>
%include <w32api/GL/glext.h>
