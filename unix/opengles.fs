\ wrapper to load Swig-generated libraries

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

Vocabulary opengl
get-current also opengl definitions

c-library opengles
    e? os-type s" ios" str= [IF]
	\c #include <OpenGLES/ES2/gl.h>
	\c #include <OpenGLES/ES2/glext.h>

	include unix/ios-gles.fs
    [ELSE]
        \c #include <GLES2/gl2.h>
        \c #include <GLES2/gl2ext.h>
        \c #include <EGL/egl.h>

	s" GLESv2" add-lib
	s" EGL" add-lib
    
	include unix/gles.fs
	include unix/egl.fs
    [THEN]
end-c-library

previous set-current
