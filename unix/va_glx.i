// this file is in the public domain
%module va_glx
%insert("include")
%{
#include <va/va_glx.h>
%}

%apply long long { int64_t }
%apply SWIGTYPE * { VADisplay };

// exec: sed -e 's/^c-library/get-current also [IFDEF] va va [THEN] definitions\n\nc-library/g' -e 's/^end-c-library/end-c-library\nprevious set-current/g' -e 's/va_glx"/va-glx"/g'

%include <va/va_glx.h>
