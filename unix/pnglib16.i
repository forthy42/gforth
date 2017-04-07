%module png
%insert("include")
%{
#include "libpng16/png.h"
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply int { png_size_t, time_t };

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
# define __ANDROID__
#endif
#define FAR
#define CHAR_BIT 8
#define UCHAR_MAX 255
#define SHORT_MIN -32768
#define SHORT_MAX 32767
#define USHRT_MAX 65535
#define INT_MIN -2147483648
#define INT_MAX 2147483647
#define UINT_MAX 4294967295U
#define PNG_ERROR_TEXT_SUPPORTED
#define PNG_FUNCTION(type, name, args, attributes) extern type name args

%include "libpng16/pngconf.h"
%include "libpng16/png.h"

// exec: sed -e 's/c-function png_info_init_3/\\ c-function png_info_init_3/g'
