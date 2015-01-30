\ wrapper to load Swig-generated libraries

Vocabulary opengl
get-current also opengl definitions

c-library opengles
    \c #include <GLES2/gl2.h>
    \c #include <GLES2/gl2ext.h>
    \c #include <EGL/egl.h>

    s" GLESv2" add-lib
    s" EGL" add-lib
    
    include gles.fs
    include egl.fs

end-c-library

previous set-current
