\ wrapper to load Swig-generated libraries

Vocabulary opengl
get-current also opengl definitions

e? os-type s" ios" str= [IF]
    include unix/ios-gles3.fs
[ELSE]
    include unix/gles3.fs
    include unix/egl.fs
[THEN]

previous set-current
