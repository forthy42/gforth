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

// exec: sed -e 's/^c-library\( .*\)/[IFUNDEF] opengl cs-vocabulary opengl [THEN]\nget-current >r also opengl definitions\n\nc-library\1\n/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g'

#define SWIG_FORTH_GFORTH_LIBRARY "GLX"

%include <GL/glx.h>
