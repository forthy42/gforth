%module glx
%insert("include")
%{
#include <GL/glx.h>
%}
%apply float { GLfloat };

%include <GL/glx.h>
