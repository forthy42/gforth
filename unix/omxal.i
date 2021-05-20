%module omxal
%insert("include")
%{
#include <OMXAL/OpenMAXAL_Platform.h>
#include <OMXAL/OpenMAXAL.h>
#include <OMXAL/OpenMAXAL_Android.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define __ANDROID__
#define ANDROID
#define XA_API
#define const

%apply int { XAint32, XAmillibel }
%apply unsigned int { XAuint32, XAmillisecond, XAmilliHertz, XAmillidegree, XAmicrosecond, XAboolean, XAresult }
%apply unsigned short { XAuint16 }
%apply short { XAint16, XApermille }
%apply char { XAchar, XAuint8, XAint8 }
%apply unsigned long long { XAuint64, XAAuint64, XAtime }
%apply long long { XAint64, XAAint64 }
%apply SWIGTYPE * { void * }

// exec: sed -e s/Itf_-/Itf-/g -e s/ID_-/ID-/g -e 's/omxal"/OpenMAXAL"/g' -e 's/^\(.*c-library\)/\\ \1/g' -e 's/\([^_]\)_$/\1/g' | awk '/^begin-structure .*Itf$/{print "("; p++} 1; p && /end-structure$/{print ")"; p=0}'

%include "OMXAL/OpenMAXAL_Platform.h"
%include "OMXAL/OpenMAXAL.h"
%include "OMXAL/OpenMAXAL_Android.h"
