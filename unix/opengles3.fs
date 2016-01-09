\ wrapper to load Swig-generated libraries

Vocabulary opengl
get-current also opengl definitions

c-library opengles3
    e? os-type s" ios" str= [IF]
	\c #include <OpenGLES/ES3/gl.h>
	\c #include <OpenGLES/ES3/glext.h>

	include unix/ios-gles3.fs
    [ELSE]
	\c #include <GLES3/gl3.h>
	\c #include <GLES3/gl3ext.h>
	\c #include <EGL/egl.h>
	
	e? os-type s" cygwin" str= [IF]
	    s" GLESv2" add-lib
	[ELSE]
	    s" GLESv3" add-lib
	[THEN]
	s" EGL" add-lib
	
	include unix/gles3.fs
	include unix/egl.fs
    [THEN]
end-c-library

previous set-current
