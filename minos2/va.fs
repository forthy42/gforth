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
#0. 2Value profile-mask

: >profile-mask ( -- )
    #0. profile-list profiles# sfloats bounds U+DO
	#1. I l@ $10 cells 1- and dlshift rot or >r or r>
    1 sfloats +LOOP  to profile-mask ;
: profile? ( profile -- flag )
    >r #1. r> dlshift profile-mask rot and >r and r> or 0<> ;

[: rot execute ;] VAMessageCallback: Constant VAMessageCB

: va-display ( dpy -- )
    { | w^ major w^ minor }
    vaGetDisplayGLX to va-dpy
    va-dpy VaMessageCB ['] 2drop vaSetInfoCallback drop
    va-dpy major minor vaInitialize ?va-ior
    va( ." VA-API version: " major l@ 0 .r '.' emit minor l@ 0 .r cr
    dup vaQueryVendorString type cr )
    va-dpy vaMaxNumEntrypoints sfloats allocate throw to profile-list
    va-dpy profile-list addr profiles# vaQueryConfigProfiles ?va-ior
    >profile-mask ;

' VAProfileNone , here
' VAProfileMPEG2Simple ,
' VAProfileMPEG2Main ,
' VAProfileMPEG4Simple ,
' VAProfileMPEG4AdvancedSimple ,
' VAProfileMPEG4Main ,
' VAProfileH264Baseline ,
' VAProfileH264Main ,
' VAProfileH264High ,
' VAProfileVC1Simple ,
' VAProfileVC1Main ,
' VAProfileVC1Advanced ,
' VAProfileH263Baseline ,
' VAProfileJPEGBaseline ,
' VAProfileH264ConstrainedBaseline ,
' VAProfileVP8Version0_3 ,
' VAProfileH264MultiviewHigh ,
' VAProfileH264StereoHigh ,
' VAProfileHEVCMain ,
' VAProfileHEVCMain10 ,
' VAProfileVP9Profile0 ,
' VAProfileVP9Profile1 ,
' VAProfileVP9Profile2 ,
' VAProfileVP9Profile3 ,
' VAProfileHEVCMain12 ,
' VAProfileHEVCMain422_10 ,
' VAProfileHEVCMain422_12 ,
' VAProfileHEVCMain444 ,
' VAProfileHEVCMain444_10 ,
' VAProfileHEVCMain444_12 ,
' VAProfileHEVCSccMain ,
' VAProfileHEVCSccMain10 ,
' VAProfileHEVCSccMain444 ,
' VAProfileAV1Profile0 ,
' VAProfileAV1Profile1 ,
Constant profile-names

: .profiles ( -- )
    profile-list profiles# sfloats bounds U+DO
	I sl@ cells profile-names + @ .name
    1 sfloats  +LOOP ;

previous set-current
