\ wrapper to load Swig-generated libraries

Vocabulary opengl
get-current also opengl definitions

c-library opengl
    \c #include <GLES2/gl2.h>
    \c #include <GLES2/gl2ext.h>
    \c #include <EGL/egl.h>

    s" GLESv2" add-lib
    s" EGL" add-lib
    
    \ This is the missing piece:
    \ you need to get a linkable copy of libui.so
    \ s" ui" add-lib
    \ \c void* android_createDisplaySurface(void);
    \ c-function android_createDisplaySurface android_createDisplaySurface -- a ( -- window )
   
    include gles.fs
    include egl.fs

end-c-library

previous set-current