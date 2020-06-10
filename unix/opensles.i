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

#define SWIG_FORTH_OPTIONS "no-use-structs"

// exec: sed -e 's/^c-library\( .*\)/vocabulary opensles``get-current also opensles definitions``c-library\1`s" ((struct SL:*(Cell*)(x.spx[arg0])" ptr-declare $+[]!/g' -e 's/^end-c-library/end-c-library`previous set-current/g' -e 's/s" opensles" add-lib/s" OpenSLES" add-lib/g' -e 's/_-/-/g' -e 's/\([^_]\)_$/\1/g' | tr '`' '\n'

#define const
#define SL_API
#define SLAPIENTRY

%include <SLES/OpenSLES.h>
%include <SLES/OpenSLES_AndroidConfiguration.h>
%include <SLES/OpenSLES_AndroidMetadata.h>
