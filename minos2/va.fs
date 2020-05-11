\ Video Accellerator driver

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

require unix/va.fs
require unix/va-x11.fs
require unix/va-glx.fs

get-current va also definitions

debug: va( \ )
\ +db va( \ )

$100 buffer: va-error$

: ?va-ior ( n -- )
    dup IF  [: vaErrorStr va-error$ place
	    va-error$ "error \ "
	    ! -2  throw ;] do-debug
    THEN drop ;

0 Value va-dpy
0 Value profile-list
0 Value profiles#

: va-display ( dpy -- )
    { | w^ major w^ minor }
    vaGetDisplay to va-dpy
    va-dpy major minor vaInitialize ?va-ior
    va( ." VA-API version: " major l@ 0 .r '.' emit minor l@ 0 .r cr
    dup vaQueryVendorString type cr )
    va-dpy vaMaxNumEntrypoints sfloats allocate throw to profile-list
    va-dpy profile-list addr profiles# vaQueryConfigProfiles ?va-ior
;

previous set-current
