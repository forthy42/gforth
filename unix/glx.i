// this file is in the public domain
%module glx
%insert("include")
%{
#define GLX_GLXEXT_PROTOTYPES
#include <GL/glx.h>
%}
%apply int { XID, Bool, GLsizei, Pixmap, Font, Window };
%apply long long { int64_t };
%apply float { GLfloat };

#define SWIG_FORTH_GFORTH_LIBRARY "GLX"

%include <GL/glx.h>
