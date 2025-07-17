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

// prep: sed -e 's/^\(.*\(_dred_\|opus_packet_has_lbrr\).*\)$/#ifdef OPUS_GET_DRED_DURATION_REQUEST\n\1\n#endif/g'
// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary opus\nget-current >r also opus definitions\n\nc-library\1\ns" n" vararg$ $!/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g'

%include <opus/opus_types.h>
%include <opus/opus_defines.h>
%include <opus/opus.h>
