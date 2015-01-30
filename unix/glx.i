%module glx
%insert("include")
%{
#include <GL/glx.h>
%}
%apply int { XID, Bool, GLsizei, Pixmap, Font, Window };
%apply long long { int64_t };
%apply float { GLfloat };

%include <GL/glx.h>
