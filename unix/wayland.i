// this file is in the public domain
%module wayland
%insert("include")
%{
#ifdef __cplusplus
#include <wayland-client-protocol.hpp>
#include <wayland-client-protocol-unstable.hpp>
#include <wayland-egl.hpp>
#include <wayland-util.hpp>
using namespace wayland;
#define WAYLAND_DETAIL(x) wayland::detail::x
#else
#include <wayland-egl.h>
#include <wayland-util.h>
#define WAYLAND_DETAIL(x) x
#endif
#include <wayland-client-protocol.h>
#include <wayland-cursor.h>
#include <text-input-unstable-v3.h>
#include <xdg-shell.h>
%}

%apply int { int32_t, wl_fixed_t }
%apply unsigned int { uint32_t }
%apply SWIGTYPE * { wl_dispatcher_func_t, wl_log_func_t }
#define WL_HIDE_DEPRECATED
#define WAYLAND_DETAIL(x) x

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary wayland``get-current also wayland definitions``c++-library\1`s" wayland" open-fpath-file 0= [IF] rot close-file throw add-incdir [THEN]`/g' -e 's|^end-c-library|include unix/wayland-interfaces.fs`end-c-library`previous set-current|g' -e 's/c-funptr \(.*\)() {.*} \(.*\)/c-callback \1: \2/g' -e 's:" wayland" add-lib:" wayland-egl++ -lwayland-client++ -lwayland-cursor++ -lwayland-client-extra++ -lwayland-client-unstable++" add-lib:g' | tr '`' '\n'

%include <wayland-client.h>
%include <wayland-client-core.h>
%include <wayland-client-protocol.h>
%include <wayland-egl.h>
%include <wayland-egl-core.h>
%include <wayland-cursor.h>
%include <wayland-util.h>
%include <text-input-unstable-v3.h>
%include <xdg-shell.h>
