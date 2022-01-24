// this file is in the public domain
%module xrandr
%insert("include")
%{
#include <X11/extensions/Xrandr.h>
#define _EVDEVK(x) x
%}

#define SWIG_FORTH_GFORTH_LIBRARY "Xrandr"

#define _XFUNCPROTOBEGIN
#define _XFUNCPROTOEND
#define _Xconst const
#define _X_DEPRECATED
#define _X_SENTINEL(x)

#define _XRRModeInfo
#define _XRRScreenResources
#define _XRROutputInfo
#define _XRRCrtcInfo
#define _XRRCrtcGamma
#define _XRRCrtcTransformAttributes
#define _XRRPanning
#define _XRRProviderResources
#define _XRRProviderInfo
#define _XRRMonitorInfo

%apply short { wchar_t };
%apply unsigned long long { XSyncValue };
%apply unsigned int { XID, XSyncCounter, XSyncAlarm, XSyncFence };

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary Xrandr``get-current also Xrandr definitions``c-library\1`s" a a 0" vararg$ $!/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'

#define XK_MISCELLANY

%include <X11/extensions/Xrandr.h>
