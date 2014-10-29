%module png
%insert("include")
%{
#include "libpng/png.h"
%}

%apply int { png_size_t, time_t };

#define __ANDROID__
#define FAR

%include "libpng/pngconf.h"
%include "libpng/png.h"
