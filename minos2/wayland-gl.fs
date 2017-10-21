\ Wayland bindings for GLES

\ Copyright (C) 2017 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

require unix/opengles.fs
require unix/waylandlib.fs
require mini-oof2.fs
require struct-val.fs

also wayland

0 Value dpy        \ wayland display
0 Value compositor \ wayland compositor
0 Value wl-shell   \ wayland shell
0 Value wl-egl-dpy \ egl display
0 Value registry

\ listeners
: registry+ { data registry name interface version -- }
    interface cstring>sstring 2dup s" wl_compositor" str= IF
	2drop
	registry name wl_compositor_interface 0 wl_registry_bind to compositor
    ELSE
	s" wl_shell" str= IF
	    registry name wl_shell_interface 0 wl_registry_bind to wl-shell
	THEN
    THEN ;
: registry- { data registry name -- } ;

' registry- wl_registry_listener-global_remove:
' registry+ wl_registry_listener-global:
Create registry_listener , ,

: wl-connect ( -- )
    0 0 wl_display_connect to dpy
    dpy wl_display_get_registry to registry
    registry registry_listener 0 wl_registry_add_listener drop
    dpy wl_display_roundtrip drop ;
