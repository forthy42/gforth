\ Linux bindings for GLES

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2019 Free Software Foundation, Inc.

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

\ todo: fix problem with callbacks and event queue in Cygwin

require unix/win32.fs
require unix/user.fs
require unix/gdi.fs
require utf16.fs
require unix/opengles.fs

debug: windows(
\ +db windows( \ )

also user32 also gdi32 also win32

WS_OVERLAPPEDWINDOW  WS_VISIBLE or  Constant wStyle

RECT        buffer: windowRect
WNDCLASSEXW buffer: windowClass
MSG         buffer: event

Variable hInstance
Variable lIcon
Variable sIcon
Variable createstruc

: ime-default { wnd msg w l -- n }
    wnd msg w l ImmIsUIMessage dup 0= IF
	drop wnd msg w l DefWindowProc
    THEN ;

: gl-window-proc { wnd msg w l -- n }
    windows( ." msg: " msg . w . l hex. ." :" )
    msg case
	WM_CREATE  of ." Created " cr wnd msg w l DefWindowProc  endof
	WM_PAINT   of ." Painted " cr wnd msg w l DefWindowProc endof
	WM_DESTROY of ." Destroyed " cr  wnd msg w l DefWindowProc endof
	WM_CHAR    of ." Char: " cr  wnd msg w l DefWindowProc endof
	WM_NCACTIVATE of  w 0= negate  endof
	WM_ACTIVATE of    w 0= negate  endof
	WM_GETICON of  lIcon sIcon w 1 = select @  endof
	WM_IME_SETCONTEXT of  0 endof
	drop wnd msg w l DefWindowProc
	0 endcase windows( dup . cr ) ;

' gl-window-proc WNDPROC: Constant gl-window-proc-cb

: adjust ( w h -- x y w h )
    #30 windowRect RECT-left l!
    #40 windowRect RECT-top l!
    windowRect RECT-bottom l!
    windowRect RECT-right l!
    windowRect wStyle 0 AdjustWindowRect drop
    windowRect RECT-left sl@
    windowRect RECT-top  sl@
    windowRect RECT-right  sl@
    windowRect RECT-bottom sl@ ;

: register-class ( -- )
    0 0 GetModuleHandle hInstance !
    "gforth.ico" 0 lIcon sIcon 1 ExtractIconEx drop
    \ ." Icons: " . lIcon ? sIcon ? cr
    WNDCLASSEXW                        windowClass WNDCLASSEXW-cbSize l!
    CS_OWNDC CS_VREDRAW or CS_HREDRAW or windowClass WNDCLASSEXW-style l!
    hInstance @                        windowClass WNDCLASSEXW-hInstance !
    BLACK_BRUSH GetStockObject         windowClass WNDCLASSEXW-hbrBackground !
    gl-window-proc-cb                  windowClass WNDCLASSEXW-lpfnWndProc !
    lIcon @                            windowClass WNDCLASSEXW-hIcon !
    sIcon @                            windowClass WNDCLASSEXW-hIconSm !
    5                                  windowClass WNDCLASSEXW-hbrBackground !
    0 0 32512 ( IDC_ARROW ) LoadCursor windowClass WNDCLASSEXW-hCursor !
    "gforth" >utf16 2 + save-mem drop  windowClass WNDCLASSEXW-lpszClassName !
    3                                  windowClass WNDCLASSEXW-lpszMenuName !
    windowClass RegisterClassEx drop ;

: make-window ( w h -- hnd )  2>r
    0 "gforth" "GL-Window" wStyle 0 0 2r> 0 0
    hInstance @ 0 CreateWindowEx ;

: get-events ( -- )
    BEGIN  event 0 0 0 PM_REMOVE PeekMessage  WHILE
	    event TranslateMessage drop
	    event DispatchMessage drop
    REPEAT ;

0 Value dpy
0 Value win

: get-display ( -- w h )
    register-class
    SM_CXMAXIMIZED GetSystemMetrics
    SM_CYMAXIMIZED GetSystemMetrics
    2dup make-window to win
    win GetDC to dpy ;

[IFDEF] dpy-w
    also opengl
    : getwh ( -- )
	0 0 dpy-w @ dpy-h @ glViewport ;
    
    : win-eglwin ( w h -- )  2drop ;
    previous
[THEN]

\ looper

get-current also forth definitions

require unix/socket.fs
require unix/pthread.fs

previous set-current

User xptimeout  cell uallot drop
#16 Value looper-to# \ 16ms, don't sleep too long
looper-to# #1000000 um* xptimeout 2!
2 Value xpollfd#
User xpollfds
xpollfds pollfd xpollfd# * dup cell- uallot drop erase

: >poll-events ( delay -- n )
    0 xptimeout 2!
    epiper @ fileno POLLIN  xpollfds fds!+ >r
    infile-id fileno POLLIN  r> fds!+ >r
    r> xpollfds - pollfd / ;

: xpoll ( -- flag )
    [IFDEF] ppoll
	xptimeout 0 ppoll 0>
    [ELSE]
	xptimeout 2@ #1000 * swap #1000000 / + poll 0>
    [THEN] ;

Defer ?looper-timeouts ' noop is ?looper-timeouts

: #looper ( delay -- ) #1000000 *
    ?looper-timeouts >poll-events >r
    xpollfds r> xpoll
    IF
	xpollfds          revents w@ POLLIN and IF  ?events  THEN
    THEN
    get-events ;

: >looper ( -- )  looper-to# #looper ;
