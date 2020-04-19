// this file is in the public domain
%module pulse
%insert("include")
%{
#include <pulse/pulseaudio.h>
%}

#define PA_GCC_CONST
#define PA_GCC_PURE
#define PA_GCC_DEPRECATED
#define PA_C_DECL_BEGIN
#define PA_C_DECL_END
#define PA_GCC_MALLOC
#define PA_GCC_ALLOC_SIZE(x)
#define PA_GCC_ALLOC_SIZE2(x, y)
#define PA_GCC_PRINTF_ATTR(x, y)

%apply long long { int64_t }

// exec: sed -e 's/^c-library/vocabulary pulse``get-current also pulse definitions``c-library/g' -e 's/^end-c-library/end-c-library`previous set-current/g' -e 's/add-lib/add-lib`s" ((struct pa_:x.spx[arg0]" ptr-declare $+[]!/g' -e 's/\(c-function .*_autoload\)/\\ \1/g' | tr '`' '\n'

%include <pulse/direction.h>
%include <pulse/mainloop-api.h>
%include <pulse/sample.h>
%include <pulse/format.h>
%include <pulse/def.h>
%include <pulse/context.h>
%include <pulse/stream.h>
%include <pulse/introspect.h>
%include <pulse/subscribe.h>
%include <pulse/scache.h>
%include <pulse/version.h>
%include <pulse/error.h>
%include <pulse/operation.h>
%include <pulse/channelmap.h>
%include <pulse/volume.h>
%include <pulse/xmalloc.h>
%include <pulse/utf8.h>
%include <pulse/thread-mainloop.h>
%include <pulse/mainloop.h>
%include <pulse/mainloop-signal.h>
%include <pulse/util.h>
%include <pulse/timeval.h>
%include <pulse/proplist.h>
%include <pulse/rtclock.h>
