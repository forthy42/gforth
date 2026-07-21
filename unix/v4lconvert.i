// this file is in the public domain
%module v4lconvert
%insert("include")
%{
#include <libv4lconvert.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define __inline__
#define __user
#define __attribute__(x)

// exec: sed -e 's/^c-library\( .*\)/[IFUNDEF] v4l2 cs-vocabulary v4l2 [THEN]\n\nget-current also v4l2 definitions\n\nc-library\1\n/g' -e 's/^end-c-library/end-c-library\nprevious set-current/g'

%include <libv4lconvert.h>

