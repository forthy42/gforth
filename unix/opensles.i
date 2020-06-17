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

// exec: sed -e 's/^c-library\( .*\)/vocabulary opensles``get-current also opensles definitions``c-library\1`s" ((struct SL:*(Cell*)(x.spx[arg0])" ptr-declare $+[]!/g' -e 's|^end-c-library|include unix/opensles-vals.fs`end-c-library`previous set-current|g' -e 's/s" opensles" add-lib/s" OpenSLES" add-lib/g' -e 's/_-/-/g' -e 's/\([^_]\)_$/\1/g' | tr '`' '\n' | awk '/^begin-structure .*Itf$/{print "("; p++} 1; p && /end-structure$/{print ")"; p=0}'

#define const
#define SL_API
#define SLAPIENTRY

%include <SLES/OpenSLES_Platform.h>
%include <SLES/OpenSLES.h>
%include <SLES/OpenSLES_AndroidConfiguration.h>
%include <SLES/OpenSLES_AndroidMetadata.h>
%include <SLES/OpenSLES_Android.h>
