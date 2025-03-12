// this file is in the public domain
%module pipewire
%insert("include")
%{
#include <pipewire/pipewire.h>
#include <spa/pod/pod.h>
#include <spa/pod/builder.h>
#include <spa/pod/parser.h>
#include <spa/param/format.h>
#include <spa/param/audio/format.h>
#include <spa/param/audio/raw-utils.h>
#include <spa/param/audio/ape-utils.h>
#include <spa/param/audio/format-utils.h>
#include <spa/param/video/format.h>
#include <spa/param/video/format-utils.h>
#include <spa/utils/result.h>
#include <spa/param/props.h>
%}

#define SPA_PRINTF_FUNC(x, y)
#define SPA_DEFINE_AUTOPTR_CLEANUP(x, y, z)
#define SPA_DEFINE_AUTO_CLEANUP(x, y, z)
#define SPA_SENTINEL
#define SPA_DEPRECATED
#define SPA_WARN_UNUSED_RESULT

%apply int { int32_t }
%apply unsigned int { uint32_t }
%apply long long { int64_t, size_t }
%apply unsigned long long { uint64_t }
%apply SWIGTYPE * { spa_invoke_func_t, spa_source_io_func_t, spa_source_idle_func_t, spa_source_event_func_t, spa_source_timer_func_t, spa_source_signal_func_t, va_list }

// exec: sed -e 's/" pipewire" add-lib/" pipewire-0.3" add-lib/g' -e 's/^c-library/cs-vocabulary pipewire\n\nget-current also pipewire definitions\n\nc-library/g' -e 's/^end-c-library/end-c-library\nprevious set-current/g' -e 's/add-lib/add-lib\ns" ((struct pw_:x.spx[arg0]" ptr-declare $+[]!/g'

%include <pipewire/array.h>
%include <pipewire/client.h>
%include <pipewire/conf.h>
%include <pipewire/context.h>
%include <pipewire/device.h>
%include <pipewire/buffers.h>
%include <pipewire/core.h>
%include <pipewire/factory.h>
%include <pipewire/keys.h>
%include <pipewire/log.h>
%include <pipewire/loop.h>
%include <pipewire/link.h>
%include <pipewire/main-loop.h>
%include <pipewire/map.h>
%include <pipewire/mem.h>
%include <pipewire/module.h>
%include <pipewire/node.h>
%include <pipewire/properties.h>
%include <pipewire/proxy.h>
%include <pipewire/permission.h>
%include <pipewire/protocol.h>
%include <pipewire/port.h>
%include <pipewire/stream.h>
%include <pipewire/filter.h>
%include <pipewire/thread-loop.h>
%include <pipewire/data-loop.h>
%include <pipewire/type.h>
%include <pipewire/utils.h>
%include <pipewire/version.h>
%include <spa/pod/parser.h>
%include <spa/pod/builder.h>
%include <spa/param/audio/format.h>
%include <spa/param/format-utils.h>
%include <spa/param/audio/raw-utils.h>
%include <spa/param/audio/dsp-utils.h>
%include <spa/param/audio/iec958-utils.h>
%include <spa/param/audio/dsd-utils.h>
%include <spa/param/audio/mp3-utils.h>
%include <spa/param/audio/aac-utils.h>
%include <spa/param/audio/vorbis-utils.h>
%include <spa/param/audio/wma-utils.h>
%include <spa/param/audio/ra-utils.h>
%include <spa/param/audio/amr-utils.h>
%include <spa/param/audio/alac-utils.h>
%include <spa/param/audio/flac-utils.h>
%include <spa/param/audio/ape-utils.h>
%include <spa/param/audio/format-utils.h>
%include <spa/utils/result.h>
%include <spa/param/format-utils.h>
%include <spa/param/video/format.h>
%include <spa/param/video/raw-utils.h>
%include <spa/param/video/dsp-utils.h>
%include <spa/param/video/h264-utils.h>
%include <spa/param/video/mjpg-utils.h>
%include <spa/param/video/format-utils.h>
%include <spa/param/props.h>
