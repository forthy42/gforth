\ wrapper to load Swig-generated libraries

Vocabulary opengl
get-current also opengl definitions

c-library opengl
    \c #include <GL/gl.h>
    \c #define GL_GLEXT_PROTOTYPES
    \c #include <GL/glx.h>

    s" GL" add-lib
    
    \ This is the missing piece:
    \ you need to get a linkable copy of libui.so
    \ s" ui" add-lib
    \ \c void* android_createDisplaySurface(void);
    \ c-function android_createDisplaySurface android_createDisplaySurface -- a ( -- window )
   
    include gl.fs
    include glx.fs

end-c-library

previous set-current