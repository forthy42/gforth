%module png
%insert("include")
%{
#include "libpng/png.h"
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply int { png_size_t, time_t };

#define __ANDROID__
#define FAR

%include "libpng/pngconf.h"
%include "libpng/png.h"
