// this file is in the public domain
%module webp
%insert("include")
%{
#include <webp/decode.h>
#include <webp/encode.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary webp\nget-current >r also webp definitions\n\nc-library\1\n/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g'

%apply unsigned long { size_t }
%apply unsigned int { uint32_t }

%include <webp/types.h>
%include <webp/decode.h>
%include <webp/encode.h>
