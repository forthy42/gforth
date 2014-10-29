%module soil
%insert("include")
%{
#include <SOIL.h>
%}

%apply SWIGTYPE * { unsigned char const *const };

%include <SOIL.h>
