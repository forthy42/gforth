%module soil
%insert("include")
%{
#include "openvg.h"
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply SWIGTYPE * { unsigned char const *const };

%include <openvg.h>
