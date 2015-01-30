\ wrapper to load Swig-generated libraries

Vocabulary opengl
get-current also opengl definitions

c-library opengles3
    \c #include <GLES3/gl3.h>
    \c #include <GLES3/gl3ext.h>
    \c #include <EGL/egl.h>

    s" GLESv3" add-lib
    s" EGL" add-lib
    
    include gles3.fs
    include egl.fs

end-c-library

previous set-current
