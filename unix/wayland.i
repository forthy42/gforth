// this file is in the public domain
%module wayland
%insert("include")
%{
#include <wayland/wayland-client.h>
#include <wayland/wayland-egl.h>
%}

%apply int { int32_t, wl_fixed_t }
%apply unsigned int { uint32_t }
%apply SWIGTYPE * { wl_dispatcher_func_t, wl_log_func_t }

%include <wayland/wayland-client-core.h>
%include <wayland/wayland-client-protocol.h>
%include <wayland/wayland-egl-core.h>

// exec: sed -e 's/c-funptr \(.*\)() {.*} \(.*\)/c-callback \1: \2/g'
