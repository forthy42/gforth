// this file is in the public domain
%module soil2
%insert("include")
%{
#include <SOIL2.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// exec: sed -e 's/\(s" soil2" add-lib\)/\1\ne? os-type s" linux-android" string-prefix? \[IF]\n\ts" EGL" add-lib\n\ts" GLESv2" add-lib\n\ts" m" add-lib\n\[THEN]/g'


%apply SWIGTYPE * { unsigned char const *const };

%include <SOIL2.h>
