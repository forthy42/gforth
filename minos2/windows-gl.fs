\ Linux bindings for GLES

\ Copyright (C) 2014 Free Software Foundation, Inc.

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

also user32 also gdi32 also win32

WS_VISIBLE WS_POPUP or WS_BORDER or WS_OVERLAPPEDWINDOW or Constant wStyle

RECT buffer: windowRect
WNDCLASSW buffer: windowClass
Variable hInstance
Variable lIcon
Variable sIcon

: gl-window-proc { wnd msg w l -- n }
    1 msg case
	WM_CREATE  of ." Created" cr endof
	WM_PAINT   of ." Painted" cr endof
	WM_DESTROY of ." Destroyed" cr  endof
	WM_CHAR    of ." Char: " w . cr endof
	WM_NCACTIVATE of  ." ncactivate " w . cr drop 0  endof
	WM_GETICON of ." GetIcon: " w . drop
	    w 1 = IF lIcon @ ELSE sIcon @ THEN dup . cr endof
	." MSG: " dup .
	drop wnd msg w l DefWindowProc dup . cr
	0 endcase ;

' gl-window-proc WNDPROC: Constant gl-window-proc-cb

: adjust ( w h -- )
    0 windowRect RECT-left l!
    0 windowRect RECT-top l!
    windowRect RECT-right l!
    windowRect RECT-bottom l!
    windowRect wStyle 0 AdjustWindowRect drop
    windowRect RECT-left sl@  windowRect RECT-right  sl@ +
    windowRect RECT-top  sl@  windowRect RECT-bottom sl@ + ;

: register-class ( -- )
    0 0 GetModuleHandle hInstance !
    "gforth.ico" 0 lIcon sIcon 1 ExtractIconEx
    CS_OWNDC 3 or                     windowClass WNDCLASSW-style l!
    hInstance @                       windowClass WNDCLASSW-hInstance !
    BLACK_BRUSH GetStockObject        windowClass WNDCLASSW-hbrBackground !
    gl-window-proc-cb                 windowClass WNDCLASSW-lpfnWndProc !
    "gforth" >utf16 2 + save-mem drop windowClass WNDCLASSW-lpszClassName !
    windowClass RegisterClass drop ;

: make-window ( w h -- hnd )  2>r
    0 "gforth" "GL-Window" wStyle 0 0 2r> adjust 0 0
    hInstance @ 0 CreateWindowEx ;
