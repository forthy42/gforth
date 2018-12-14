// this file is in the public domain
%module wayland
%insert("include")
%{
#include <wayland-egl.h>
#include <wayland-cursor.h>
%}

%apply int { int32_t, wl_fixed_t }
%apply unsigned int { uint32_t }
%apply SWIGTYPE * { wl_dispatcher_func_t, wl_log_func_t }
#define WL_HIDE_DEPRECATED

#define SWIG_FORTH_OPTIONS "no-pre-postfix"

%include <wayland-client.h>
%include <wayland-client-core.h>
%include <wayland-client-protocol.h>
%include <wayland-egl.h>
%include <wayland-egl-core.h>
%include <wayland-cursor.h>

// exec: sed -e 's/c-funptr \(.*\)() {.*} \(.*\)/c-callback \1: \2/g'
