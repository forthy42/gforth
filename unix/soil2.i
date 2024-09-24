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

// exec: sed -e 's/\(s" soil2" add-lib\)/\1\ne? os-type s" linux-android" string-prefix? \[IF]\n    s" EGL" add-lib\n    s" GLESv2" add-lib\n    s" m" add-lib\n\[THEN]/g'


%apply SWIGTYPE * { unsigned char const *const };

%include <SOIL2.h>
