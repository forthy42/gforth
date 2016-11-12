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
WNDCLASSEXW buffer: windowClass
Variable hInstance
Variable lIcon
Variable sIcon
Variable createstruc

: ime-default { wnd msg w l -- n }
    wnd msg w l ImmIsUIMessage dup 0= IF
	drop wnd msg w l DefWindowProc
    THEN ;

: gl-window-proc { wnd msg w l -- n }
\    ." msg: " msg . w . l hex. cr
    msg case
	WM_NCCREATE of  l createstruc !  1 ms wnd msg w l DefWindowProc  endof
	WM_CREATE  of  l createstruc !   wnd msg w l DefWindowProc  endof
\	WM_NCPAINT of ." NC Painted " cr wnd msg w l DefWindowProc endof
	WM_PAINT   of ." Painted " cr wnd msg w l DefWindowProc endof
	WM_DESTROY of ." Destroyed " cr  wnd msg w l DefWindowProc endof
	WM_CHAR    of ." Char: " cr  wnd msg w l DefWindowProc endof
	WM_NCACTIVATE of  w 0= negate  endof
	WM_ACTIVATE of    w 0= negate  endof
	WM_GETICON of  lIcon sIcon w 1 = select @  endof
	WM_IME_SETCONTEXT of  0 endof
	drop wnd msg w l DefWindowProc \ dup . cr
	0 endcase ;

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
    WNDCLASSEXW                         windowClass WNDCLASSEXW-cbSize l!
    CS_OWNDC 3 or                      windowClass WNDCLASSEXW-style l!
    hInstance @                        windowClass WNDCLASSEXW-hInstance !
    BLACK_BRUSH GetStockObject         windowClass WNDCLASSEXW-hbrBackground !
    gl-window-proc-cb                  windowClass WNDCLASSEXW-lpfnWndProc !
    lIcon @                            windowClass WNDCLASSEXW-hIcon !
    sIcon @                            windowClass WNDCLASSEXW-hIconSm !
    5                                  windowClass WNDCLASSEXW-hbrBackground !
    0 0 32512 ( IDC_ARROW ) LoadCursor windowClass WNDCLASSEXW-hCursor !
    "gforth" >utf16 2 + save-mem drop  windowClass WNDCLASSEXW-lpszClassName !
    windowClass RegisterClassEx drop ;

: make-window ( w h -- hnd )  2>r
    0 "gforth" "GL-Window" wStyle 2r> adjust 0 0
    hInstance @ 0 CreateWindowEx ;

register-class
640 400 make-window