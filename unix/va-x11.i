// this file is in the public domain
%module va_x11
%insert("include")
%{
#include <va/va_x11.h>
%}

%apply long long { int64_t }
%apply SWIGTYPE * { VADisplay };

// exec: sed -e 's/^c-library/get-current also [IFDEF] va va [THEN] definitions``c-library/g' -e 's/^end-c-library/end-c-library`previous set-current/g' -e 's/va_x11"/va-x11"/g' | tr '`' '\n'

%include <va/va_x11.h>
