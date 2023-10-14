// this file is in the public domain
%module pipewire
%insert("include")
%{
#include <pipewire/pipewire.h>
%}

#define SPA_PRINTF_FUNC(x, y)
#define SPA_SENTINEL
#define SPA_DEPRECATED
#define SPA_WARN_UNUSED_RESULT

%apply long long { int64_t }
%apply SWIGTYPE * { spa_invoke_func_t, va_list }

// exec: sed -e 's/" pipewire" add-lib/" pipewire-0.3" add-lib/g' -e 's/^c-library/cs-vocabulary pipewire``get-current also pipewire definitions``c-library/g' -e 's/^end-c-library/end-c-library`previous set-current/g' -e 's/add-lib/add-lib`s" ((struct pw_:x.spx[arg0]" ptr-declare $+[]!/g' | tr '`' '\n'

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
