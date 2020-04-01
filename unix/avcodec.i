// this file is in the public domain
%module avcodec
%insert("include")
%{
#include <libavcodec/avcodec.h>
%}

#define attribute_deprecated

%include <libavcodec/avcodec.h>
