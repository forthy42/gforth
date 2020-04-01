// this file is in the public domain
%module avcodec
%insert("include")
%{
#include <libavcodec/avcodec.h>
#undef gforth_d2ll
#define gforth_d2ll(x1,x2) av_make_q(x1,x2)
%}

#define attribute_deprecated

%apply long long { AVRational }

%include <libavcodec/avcodec.h>
