\ wrapper to load Swig-generated libraries

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
    
	include glwin.fs
	include wgl.fs
    [ELSE]
	s" GL" add-lib
    
	include gl.fs
	include glx.fs
    [THEN]
    
end-c-library

previous set-current