\ wrapper to load Swig-generated libraries

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2018,2019,2021,2023 Free Software Foundation, Inc.

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

e? os-type s" ios" str= [IF]
    include unix/ios-gles3.fs
[ELSE]
    include unix/gles3.fs
    include unix/egl.fs
[THEN]
