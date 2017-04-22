\ useful windows functions (kernel32 and imm32)

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

cs-vocabulary win32
get-current also win32 definitions

c-library win32
    \c #include <w32api/wtypes.h>
    \c #include <w32api/winbase.h>
    \c #include <w32api/imm.h>
    c-function GetModuleHandle GetModuleHandleW ws -- a ( string -- handle )
    c-function GetLastError GetLastError -- n ( -- n )
    c-function ImmIsUIMessage ImmIsUIMessageW a n n n -- n ( wnd msg wparam lparam -- bool )
    c-function ExtractIcon ExtractIconW a ws n -- a ( inst file* n -- handle )
    c-function ExtractIconEx ExtractIconExW ws n a a n -- n ( file* i licons* sicons* n -- n )
    s" kernel32" add-lib
    s" imm32" add-lib
    s" shell32" add-lib
end-c-library

previous set-current