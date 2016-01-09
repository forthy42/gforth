\ wrapper around cpufeatures for Android

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

c-library cpufeatureslib
    \c #include "../../../../unix/cpu-features.c"
    include unix/cpufeatures.fs
end-c-library

android_getCpuFeatures drop
android_getCpuFamily ANDROID_CPU_FAMILY_ARM = [IF]
    ANDROID_CPU_ARM_FEATURE_NEON and
[ELSE]
    android_getCpuFamily ANDROID_CPU_FAMILY_X86 = [IF]
	ANDROID_CPU_X86_FEATURE_SSSE3 and
    [ELSE]
	drop false
    [THEN]
[THEN]
0<> constant fast-lib