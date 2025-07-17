// this file is in the public domain
%module opensles
%insert("include")
%{
#include <SLES/OpenSLES_Android.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary opensles\nget-current >r also opensles definitions\n\nc-library\1\ns" ((struct SL:*(Cell*)(x.spx[arg0])" ptr-declare $+[]!/g' -e 's|^end-c-library|include unix/opensles-vals.fs\nend-c-library\nprevious r> set-current|g' -e 's/s" opensles" add-lib/s" OpenSLES" add-lib/g' -e 's/_-/-/g' -e 's/\([^_]\)_$/\1/g' | awk '/^begin-structure .*Itf$/{print "("; p++} 1; p && /end-structure$/{print ")"; p=0}'

#define const
#define SL_API
#define SLAPIENTRY

%include <SLES/OpenSLES_Platform.h>
%include <SLES/OpenSLES.h>
%include <SLES/OpenSLES_AndroidConfiguration.h>
%include <SLES/OpenSLES_AndroidMetadata.h>
%include <SLES/OpenSLES_Android.h>
