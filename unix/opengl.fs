\ wrapper to load Swig-generated libraries

\ Copyright (C) 2015,2016 Free Software Foundation, Inc.

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

c-library opengl
    \c #define GL_GLEXT_PROTOTYPES
    e? os-type s" cygwin" str= [IF]
	\c #include <w32api/GL/gl.h>
	\c #include <w32api/GL/glext.h>
	\c #include <w32api/GL/wglext.h>
    [ELSE]
	\c #include <GL/glx.h>
	\c #include <GL/glext.h>
    [THEN]
    
    e? os-type s" cygwin" str= [IF]
	s" opengl32" add-lib
    
	include unix/glwin.fs
	include unix/wgl.fs
    [ELSE]
	s" GL" add-lib
    
	include unix/gl.fs
	include unix/glx.fs
    [THEN]
    
end-c-library

previous set-current