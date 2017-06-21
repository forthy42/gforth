// this file is in the public domain
%module soil
%insert("include")
%{
#include <SOIL2.h>
%}

%apply SWIGTYPE * { unsigned char const *const };

%include <SOIL2.h>
