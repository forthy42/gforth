// this file is in the public domain
%module soil
%insert("include")
%{
#include <SOIL/SOIL.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define SWIG_FORTH_GFORTH_LIBRARY "SOIL"

%apply SWIGTYPE * { unsigned char const *const };

%include <SOIL/SOIL.h>
