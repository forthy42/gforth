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
%apply unsigned long long { uint64_t }
%apply unsigned int { size_t, pa_usec_t, uint32_t }
%apply unsigned char { uint8_t }

// exec: sed -e 's/^c-library/cs-vocabulary pulse\nget-current >r also pulse definitions\n\nc-library/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g' -e 's/add-lib/add-lib\ns" ((struct pa_:x.spx[arg0]" ptr-declare $+[]!/g' -e 's/\(c-function .*_autoload\)/\\ \1/g' -e 's/c-function pa_proplist_setf /\\ c-function pa_proplist_setf /g'
// prep: sed -e 's/\(swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*\)/if(offsetof(\2, \3) >= 0) \1/g'

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
