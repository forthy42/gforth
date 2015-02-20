// this file is in the public domain
%module soil
%insert("include")
%{
#include <SOIL2.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply SWIGTYPE * { unsigned char const *const };

%include <SOIL2.h>
