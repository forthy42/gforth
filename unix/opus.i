// this file is in the public domain
%module opus
%insert("include")
%{
#include <opus/opus.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define PA_GCC_CONST
#define PA_GCC_PURE
#define PA_GCC_DEPRECATED
#define PA_C_DECL_BEGIN
#define PA_C_DECL_END
#define PA_GCC_MALLOC
#define PA_GCC_ALLOC_SIZE(x)
#define PA_GCC_ALLOC_SIZE2(x, y)
#define PA_GCC_PRINTF_ATTR(x, y)

// exec: sed -e 's/^c-library\( .*\)/vocabulary opus``get-current also opus definitions``c-library\1`s" n" vararg$ $!/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'

%include <opus/opus_types.h>
%include <opus/opus_defines.h>
%include <opus/opus.h>
