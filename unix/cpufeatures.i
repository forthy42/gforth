// this file is in the public domain
%module cpufeatureslib
%insert("include")
%{
#include "cpu-features.h"
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define __BEGIN_DECLS
#define __END_DECLS

%apply unsigned long long { uint64_t };

%include "cpu-features.h"
