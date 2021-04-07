// this file is in the public domain
%module avcodec
%insert("include")
%{
#include <libavcodec/avcodec.h>
#include <libavutil/pixfmt.h>
#undef gforth_d2ll
#define gforth_d2ll(x1,x2) av_make_q(x1,x2)
%}

#define attribute_deprecated

%apply long long { AVRational }

// exec: sed -e 's/add-lib/add-lib`s" ((struct AV:x.spx[arg0]" ptr-declare $+[]!/g' | tr '`' '\n'

%include <libavcodec/avcodec.h>
%include <libavutil/pixfmt.h>
