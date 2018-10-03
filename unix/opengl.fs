\ wrapper to load Swig-generated libraries

\ Copyright (C) 2015,2016,2017 Free Software Foundation, Inc.

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
    e? os-type s" darwin" str= [IF]
	\c #include <OpenGL/gl.h>
	\c #include <OpenGL/glext.h>
    [ELSE]
	e? os-type s" cygwin" str= [IF]
	    \c #include <GL/gl.h>
	    \c #include <GL/glext.h>
	[ELSE]
	    \c #include <GL/glx.h>
	    \c #include <GL/glext.h>
	[THEN]
    [THEN]
    
    e? os-type 3 /string s" win" str= [IF] \ darwin, cygwin...
	s" GL" add-lib
    
	include unix/gl.fs
    [ELSE]
	s" GL" add-lib
    
	include unix/gl.fs
	include unix/glx.fs
    [THEN]
    
end-c-library

previous set-current
